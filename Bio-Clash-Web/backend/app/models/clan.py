"""
Clan/Legion Domain Models
Handles clans, membership, and clan wars.
"""
import uuid
from datetime import datetime
from sqlalchemy import Column, String, Integer, Float, Boolean, DateTime, Text, ForeignKey, Enum as SQLEnum
from sqlalchemy.orm import relationship

from app.db.session import Base
from app.core.enums import ClanRole, WarState


class Clan(Base):
    """
    Clan/Legion - A group of players competing together.
    """
    __tablename__ = "clans"
    
    id = Column(String, primary_key=True, default=lambda: str(uuid.uuid4()))
    
    # Basic Info
    name = Column(String(50), unique=True, nullable=False)
    tag = Column(String(10), unique=True, nullable=False)  # e.g., #ABC123
    description = Column(Text, default="")
    badge_icon = Column(String(50), default="shield")  # Icon name
    badge_color = Column(String(7), default="#FFD700")  # Hex color
    
    # Stats
    level = Column(Integer, default=1)
    total_xp = Column(Integer, default=0)
    total_trophies = Column(Integer, default=0)
    war_wins = Column(Integer, default=0)
    war_losses = Column(Integer, default=0)
    war_streak = Column(Integer, default=0)
    
    # Settings
    is_public = Column(Boolean, default=True)  # Can anyone join?
    min_trophies_required = Column(Integer, default=0)
    max_members = Column(Integer, default=50)
    
    # Aggregate biological power (sum of all members)
    total_attack_power = Column(Float, default=0)
    total_defense_power = Column(Float, default=0)
    
    created_at = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    members = relationship("ClanMember", back_populates="clan", cascade="all, delete-orphan")
    wars = relationship("ClanWar", foreign_keys="ClanWar.clan_id", back_populates="clan")


class ClanMember(Base):
    """
    Clan membership - Links users to clans with roles.
    """
    __tablename__ = "clan_members"
    
    id = Column(String, primary_key=True, default=lambda: str(uuid.uuid4()))
    
    clan_id = Column(String, ForeignKey("clans.id", ondelete="CASCADE"), nullable=False)
    user_id = Column(String, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    
    role = Column(SQLEnum(ClanRole), default=ClanRole.MEMBER)
    
    # Contribution stats for this member
    donations = Column(Integer, default=0)  # Resources donated
    war_stars_earned = Column(Integer, default=0)
    attacks_won = Column(Integer, default=0)
    
    joined_at = Column(DateTime, default=datetime.utcnow)
    last_active = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    clan = relationship("Clan", back_populates="members")
    user = relationship("User", backref="clan_membership")


class ClanWar(Base):
    """
    Clan War - Team vs Team competition.
    Total biological power determines the winner.
    """
    __tablename__ = "clan_wars"
    
    id = Column(String, primary_key=True, default=lambda: str(uuid.uuid4()))
    
    # Participating clans
    clan_id = Column(String, ForeignKey("clans.id"), nullable=False)
    opponent_clan_id = Column(String, ForeignKey("clans.id"), nullable=False)
    
    # War state
    state = Column(SQLEnum(WarState), default=WarState.PREPARATION)
    
    # Timing
    preparation_start = Column(DateTime, default=datetime.utcnow)
    battle_start = Column(DateTime, nullable=True)
    battle_end = Column(DateTime, nullable=True)
    
    # Scores (total stars earned by each side)
    clan_stars = Column(Integer, default=0)
    opponent_stars = Column(Integer, default=0)
    
    # Total destruction percentage
    clan_destruction = Column(Float, default=0)
    opponent_destruction = Column(Float, default=0)
    
    # Winner (set when war ends)
    winner_clan_id = Column(String, ForeignKey("clans.id"), nullable=True)
    
    # Relationships
    clan = relationship("Clan", foreign_keys=[clan_id], back_populates="wars")
    opponent_clan = relationship("Clan", foreign_keys=[opponent_clan_id])
    attacks = relationship("WarAttack", back_populates="war", cascade="all, delete-orphan")


class WarAttack(Base):
    """
    Individual attack in a clan war.
    """
    __tablename__ = "war_attacks"
    
    id = Column(String, primary_key=True, default=lambda: str(uuid.uuid4()))
    
    war_id = Column(String, ForeignKey("clan_wars.id", ondelete="CASCADE"), nullable=False)
    attacker_id = Column(String, ForeignKey("users.id"), nullable=False)
    defender_id = Column(String, ForeignKey("users.id"), nullable=False)
    
    # Result
    stars = Column(Integer, default=0)  # 0-3
    destruction_percent = Column(Float, default=0)
    attack_power_used = Column(Float, default=0)
    defense_power_faced = Column(Float, default=0)
    
    # Loot (shared with clan)
    gold_earned = Column(Integer, default=0)
    elixir_earned = Column(Integer, default=0)
    
    attack_time = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    war = relationship("ClanWar", back_populates="attacks")
    attacker = relationship("User", foreign_keys=[attacker_id])
    defender = relationship("User", foreign_keys=[defender_id])


class ClanMessage(Base):
    """
    Clan chat message for real-time communication.
    """
    __tablename__ = "clan_messages"
    
    id = Column(String, primary_key=True, default=lambda: str(uuid.uuid4()))
    
    clan_id = Column(String, ForeignKey("clans.id", ondelete="CASCADE"), nullable=False)
    user_id = Column(String, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    
    message = Column(Text, nullable=False)
    message_type = Column(String(20), default="chat")  # chat, system, war_update
    
    created_at = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    user = relationship("User")
