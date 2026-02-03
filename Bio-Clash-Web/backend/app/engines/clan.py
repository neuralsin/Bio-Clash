"""
Clan Engine
Handles clan management, war matchmaking, and war battles.
"""
from datetime import datetime, timedelta
from typing import List, Optional, Tuple
from sqlalchemy.orm import Session
from sqlalchemy import func

from app.core.enums import ClanRole, WarState
from app.models.clan import Clan, ClanMember, ClanWar, WarAttack
from app.models.user import User
from app.engines.game import RaidEngine


class ClanManager:
    """
    Manages clan creation, membership, and stats.
    """
    
    def __init__(self, db: Session):
        self.db = db
    
    def create_clan(self, user_id: str, name: str, tag: str, **kwargs) -> Clan:
        """
        Create a new clan with the user as leader.
        """
        # Check if user is already in a clan
        existing = self.db.query(ClanMember).filter(ClanMember.user_id == user_id).first()
        if existing:
            raise ValueError("You must leave your current clan first")
        
        # Create clan
        clan = Clan(
            name=name,
            tag=tag.upper(),
            description=kwargs.get("description", ""),
            badge_icon=kwargs.get("badge_icon", "shield"),
            badge_color=kwargs.get("badge_color", "#FFD700"),
            is_public=kwargs.get("is_public", True),
            min_trophies_required=kwargs.get("min_trophies_required", 0)
        )
        self.db.add(clan)
        self.db.flush()
        
        # Add creator as leader
        leader = ClanMember(
            clan_id=clan.id,
            user_id=user_id,
            role=ClanRole.LEADER
        )
        self.db.add(leader)
        self.db.commit()
        
        return clan
    
    def join_clan(self, user_id: str, clan_id: str) -> ClanMember:
        """
        Join a clan.
        """
        # Check if already in a clan
        existing = self.db.query(ClanMember).filter(ClanMember.user_id == user_id).first()
        if existing:
            raise ValueError("You must leave your current clan first")
        
        # Get clan
        clan = self.db.query(Clan).filter(Clan.id == clan_id).first()
        if not clan:
            raise ValueError("Clan not found")
        
        if not clan.is_public:
            raise ValueError("This clan is invite-only")
        
        # Check member count
        member_count = self.db.query(ClanMember).filter(ClanMember.clan_id == clan_id).count()
        if member_count >= clan.max_members:
            raise ValueError("Clan is full")
        
        # Add member
        member = ClanMember(
            clan_id=clan_id,
            user_id=user_id,
            role=ClanRole.MEMBER
        )
        self.db.add(member)
        self.db.commit()
        
        # Update clan power
        self._recalculate_clan_power(clan_id)
        
        return member
    
    def leave_clan(self, user_id: str) -> bool:
        """
        Leave current clan.
        """
        member = self.db.query(ClanMember).filter(ClanMember.user_id == user_id).first()
        if not member:
            return False
        
        clan_id = member.clan_id
        
        # Leaders can't leave, must transfer leadership first
        if member.role == ClanRole.LEADER:
            other_members = self.db.query(ClanMember).filter(
                ClanMember.clan_id == clan_id,
                ClanMember.user_id != user_id
            ).count()
            
            if other_members > 0:
                raise ValueError("Transfer leadership before leaving")
            else:
                # Last member, delete clan
                self.db.query(Clan).filter(Clan.id == clan_id).delete()
        
        self.db.delete(member)
        self.db.commit()
        
        # Recalculate power if clan still exists
        clan = self.db.query(Clan).filter(Clan.id == clan_id).first()
        if clan:
            self._recalculate_clan_power(clan_id)
        
        return True
    
    def promote_member(self, leader_id: str, member_id: str, new_role: ClanRole) -> bool:
        """
        Promote/demote a clan member.
        """
        # Get leader's membership
        leader_member = self.db.query(ClanMember).filter(ClanMember.user_id == leader_id).first()
        if not leader_member or leader_member.role not in [ClanRole.LEADER, ClanRole.CO_LEADER]:
            raise ValueError("Insufficient permissions")
        
        # Get target member
        target = self.db.query(ClanMember).filter(ClanMember.id == member_id).first()
        if not target or target.clan_id != leader_member.clan_id:
            raise ValueError("Member not found in your clan")
        
        # Can't change leader role through this
        if target.role == ClanRole.LEADER or new_role == ClanRole.LEADER:
            raise ValueError("Use transfer leadership for leader role")
        
        target.role = new_role
        self.db.commit()
        return True
    
    def _recalculate_clan_power(self, clan_id: str):
        """
        Recalculate total clan attack and defense power.
        Aggregates biological stats from all members.
        """
        clan = self.db.query(Clan).filter(Clan.id == clan_id).first()
        if not clan:
            return
        
        total_attack = 0.0
        total_defense = 0.0
        
        members = self.db.query(ClanMember).filter(ClanMember.clan_id == clan_id).all()
        
        for member in members:
            raid_engine = RaidEngine(self.db, member.user_id)
            total_attack += raid_engine.calculate_attack_power()
            total_defense += raid_engine.calculate_defense_power(member.user_id)
        
        clan.total_attack_power = total_attack
        clan.total_defense_power = total_defense
        self.db.commit()


class ClanWarEngine:
    """
    Manages clan wars: matchmaking, battles, and resolution.
    """
    
    PREP_DURATION_HOURS = 24
    BATTLE_DURATION_HOURS = 48
    MAX_ATTACKS_PER_MEMBER = 2
    
    def __init__(self, db: Session):
        self.db = db
    
    def start_war_search(self, clan_id: str) -> ClanWar:
        """
        Start searching for a war opponent.
        Uses K-Means-like matching on total clan power.
        """
        clan = self.db.query(Clan).filter(Clan.id == clan_id).first()
        if not clan:
            raise ValueError("Clan not found")
        
        # Check if already in a war
        active_war = self.db.query(ClanWar).filter(
            ClanWar.clan_id == clan_id,
            ClanWar.state.in_([WarState.MATCHMAKING, WarState.PREPARATION, WarState.BATTLE])
        ).first()
        
        if active_war:
            raise ValueError("Already in an active war")
        
        # Find a suitable opponent
        opponent = self._find_opponent(clan)
        
        if opponent:
            # Create war
            war = ClanWar(
                clan_id=clan_id,
                opponent_clan_id=opponent.id,
                state=WarState.PREPARATION,
                battle_start=datetime.utcnow() + timedelta(hours=self.PREP_DURATION_HOURS)
            )
        else:
            # No opponent found, put in matchmaking queue
            war = ClanWar(
                clan_id=clan_id,
                opponent_clan_id=None,
                state=WarState.MATCHMAKING
            )
        
        self.db.add(war)
        self.db.commit()
        return war
    
    def _find_opponent(self, clan: Clan) -> Optional[Clan]:
        """
        Find a suitable opponent based on power level.
        """
        # Look for other clans in matchmaking
        matchmaking_wars = self.db.query(ClanWar).filter(
            ClanWar.state == WarState.MATCHMAKING,
            ClanWar.clan_id != clan.id
        ).all()
        
        if matchmaking_wars:
            # Match with the first one in queue (simple FIFO)
            opponent_id = matchmaking_wars[0].clan_id
            opponent = self.db.query(Clan).filter(Clan.id == opponent_id).first()
            
            # Remove opponent's matchmaking entry
            self.db.delete(matchmaking_wars[0])
            
            return opponent
        
        # No one in queue, look for similar power clans
        power_range = clan.total_attack_power * 0.3  # 30% tolerance
        
        opponent = self.db.query(Clan).filter(
            Clan.id != clan.id,
            Clan.total_attack_power >= clan.total_attack_power - power_range,
            Clan.total_attack_power <= clan.total_attack_power + power_range
        ).first()
        
        return opponent
    
    def execute_war_attack(
        self, war_id: str, attacker_id: str, defender_id: str
    ) -> WarAttack:
        """
        Execute an attack in a clan war.
        """
        war = self.db.query(ClanWar).filter(ClanWar.id == war_id).first()
        if not war:
            raise ValueError("War not found")
        
        if war.state != WarState.BATTLE:
            raise ValueError("War is not in battle phase")
        
        # Check if attacker is in the war
        attacker_member = self.db.query(ClanMember).filter(
            ClanMember.user_id == attacker_id
        ).first()
        
        if not attacker_member:
            raise ValueError("You are not in a clan")
        
        if attacker_member.clan_id not in [war.clan_id, war.opponent_clan_id]:
            raise ValueError("You are not part of this war")
        
        # Check attack limit
        attacks_used = self.db.query(WarAttack).filter(
            WarAttack.war_id == war_id,
            WarAttack.attacker_id == attacker_id
        ).count()
        
        if attacks_used >= self.MAX_ATTACKS_PER_MEMBER:
            raise ValueError("No attacks remaining")
        
        # Execute attack using RaidEngine
        raid_engine = RaidEngine(self.db, attacker_id)
        result = raid_engine.simulate_battle(defender_id)
        
        # Record attack
        attack = WarAttack(
            war_id=war_id,
            attacker_id=attacker_id,
            defender_id=defender_id,
            stars=result["stars"],
            destruction_percent=result["damage_percent"],
            attack_power_used=result["attack_power_used"],
            defense_power_faced=result["defense_power_faced"],
            gold_earned=result["gold_stolen"] // 2,  # Shared with clan
            elixir_earned=result["elixir_stolen"] // 2
        )
        self.db.add(attack)
        
        # Update war scores
        if attacker_member.clan_id == war.clan_id:
            war.clan_stars += result["stars"]
            war.clan_destruction = max(war.clan_destruction, result["damage_percent"])
        else:
            war.opponent_stars += result["stars"]
            war.opponent_destruction = max(war.opponent_destruction, result["damage_percent"])
        
        # Update member stats
        attacker_member.war_stars_earned += result["stars"]
        if result["victory"]:
            attacker_member.attacks_won += 1
        
        self.db.commit()
        
        return attack
    
    def check_and_end_wars(self):
        """
        Background task: Check for wars that should end and resolve them.
        """
        now = datetime.utcnow()
        
        # Find wars that should start battle phase
        prep_wars = self.db.query(ClanWar).filter(
            ClanWar.state == WarState.PREPARATION,
            ClanWar.battle_start <= now
        ).all()
        
        for war in prep_wars:
            war.state = WarState.BATTLE
            war.battle_end = now + timedelta(hours=self.BATTLE_DURATION_HOURS)
        
        # Find wars that should end
        ending_wars = self.db.query(ClanWar).filter(
            ClanWar.state == WarState.BATTLE,
            ClanWar.battle_end <= now
        ).all()
        
        for war in ending_wars:
            self._resolve_war(war)
        
        self.db.commit()
    
    def _resolve_war(self, war: ClanWar):
        """
        Determine winner and apply rewards.
        """
        war.state = WarState.WAR_ENDED
        
        # Determine winner
        if war.clan_stars > war.opponent_stars:
            war.winner_clan_id = war.clan_id
            winner_clan = self.db.query(Clan).filter(Clan.id == war.clan_id).first()
            loser_clan = self.db.query(Clan).filter(Clan.id == war.opponent_clan_id).first()
        elif war.opponent_stars > war.clan_stars:
            war.winner_clan_id = war.opponent_clan_id
            winner_clan = self.db.query(Clan).filter(Clan.id == war.opponent_clan_id).first()
            loser_clan = self.db.query(Clan).filter(Clan.id == war.clan_id).first()
        elif war.clan_destruction > war.opponent_destruction:
            war.winner_clan_id = war.clan_id
            winner_clan = self.db.query(Clan).filter(Clan.id == war.clan_id).first()
            loser_clan = self.db.query(Clan).filter(Clan.id == war.opponent_clan_id).first()
        else:
            # Tie or opponent wins on destruction
            war.winner_clan_id = war.opponent_clan_id
            winner_clan = self.db.query(Clan).filter(Clan.id == war.opponent_clan_id).first()
            loser_clan = self.db.query(Clan).filter(Clan.id == war.clan_id).first()
        
        if winner_clan:
            winner_clan.war_wins += 1
            winner_clan.war_streak += 1
            winner_clan.total_xp += 100
        
        if loser_clan:
            loser_clan.war_losses += 1
            loser_clan.war_streak = 0
