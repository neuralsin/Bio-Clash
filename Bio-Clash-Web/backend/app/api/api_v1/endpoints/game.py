"""
Game API Endpoints
Village management, Building upgrades, Raids, and FairPlay status.
"""
from datetime import datetime
from typing import List
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session

from app.db.session import get_db
from app.core.deps import get_current_active_user
from app.models.user import User
from app.models.game import Village, Building, UpgradeQueue
from app.engines.fairplay import FatigueOracle, LeagueClustering
from app.engines.game import ResourceManager, UpgradeManager, RaidEngine
from app.schemas.game import (
    VillageResponse, BuildingResponse, BuildingUpgradeRequest, BuildingUpgradeRequirement,
    ResourceSyncResponse, UpgradeQueueResponse,
    RaidSearchResponse, RaidBattleRequest, RaidBattleResult,
    RecoveryScoreResponse, LeagueInfoResponse
)

router = APIRouter(prefix="/game", tags=["Game"])


# ============================================================
# VILLAGE ENDPOINTS
# ============================================================

@router.get("/village", response_model=VillageResponse)
async def get_village(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get current user's village with all buildings.
    Also syncs resources before returning.
    """
    village = db.query(Village).filter(Village.user_id == current_user.id).first()
    
    if not village:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Village not found. Please complete registration."
        )
    
    # Sync resources
    ResourceManager(db, village).sync_resources()
    
    # Get buildings
    buildings = db.query(Building).filter(Building.village_id == village.id).all()
    village.buildings = buildings
    
    return village


@router.post("/village/sync", response_model=ResourceSyncResponse)
async def sync_resources(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Manually sync village resources.
    Called periodically by frontend.
    """
    village = db.query(Village).filter(Village.user_id == current_user.id).first()
    
    if not village:
        raise HTTPException(status_code=404, detail="Village not found")
    
    seconds_elapsed = (datetime.utcnow() - village.last_resource_sync).total_seconds()
    gold_g, elixir_g, dark_g = ResourceManager(db, village).sync_resources()
    
    return ResourceSyncResponse(
        gold=village.gold,
        elixir=village.elixir,
        dark_elixir=village.dark_elixir,
        gold_gained=gold_g,
        elixir_gained=elixir_g,
        dark_elixir_gained=dark_g,
        seconds_since_last_sync=int(seconds_elapsed)
    )


# ============================================================
# BUILDING ENDPOINTS
# ============================================================

@router.get("/building/{building_id}/upgrade-requirements", response_model=BuildingUpgradeRequirement)
async def get_upgrade_requirements(
    building_id: str,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get requirements to upgrade a specific building.
    Shows resource costs AND fitness requirements (THE CODEX).
    """
    building = db.query(Building).filter(Building.id == building_id).first()
    
    if not building:
        raise HTTPException(status_code=404, detail="Building not found")
    
    village = db.query(Village).filter(Village.id == building.village_id).first()
    
    if not village or village.user_id != current_user.id:
        raise HTTPException(status_code=403, detail="Not your building")
    
    manager = UpgradeManager(db, current_user.id, village)
    can_upgrade, reason, reqs = manager.check_upgrade_requirements(building)
    
    return BuildingUpgradeRequirement(
        building_type=building.building_type,
        current_level=building.level,
        target_level=reqs.get("target_level", building.level + 1),
        gold_cost=reqs.get("gold_cost", 0),
        elixir_cost=reqs.get("elixir_cost", 0),
        dark_elixir_cost=reqs.get("dark_elixir_cost", 0),
        required_muscle=reqs.get("required_muscle", "none"),
        required_volume_kg=reqs.get("required_volume_kg", 0),
        current_volume_kg=reqs.get("current_volume_kg", 0),
        requirement_met=can_upgrade,
        upgrade_duration_seconds=3600 * reqs.get("target_level", 1)  # 1hr per level
    )


@router.post("/building/{building_id}/upgrade")
async def start_building_upgrade(
    building_id: str,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Start upgrading a building.
    Requires resources AND fitness requirements to be met.
    """
    building = db.query(Building).filter(Building.id == building_id).first()
    
    if not building:
        raise HTTPException(status_code=404, detail="Building not found")
    
    village = db.query(Village).filter(Village.id == building.village_id).first()
    
    if not village or village.user_id != current_user.id:
        raise HTTPException(status_code=403, detail="Not your building")
    
    if building.is_upgrading:
        raise HTTPException(status_code=400, detail="Building is already upgrading")
    
    # Check FairPlay: Shield blocks upgrades
    oracle = FatigueOracle(db, current_user.id)
    if oracle.should_activate_shield():
        raise HTTPException(
            status_code=400,
            detail="You are too fatigued to start upgrades. Rest and recover first."
        )
    
    manager = UpgradeManager(db, current_user.id, village)
    upgrade = manager.start_upgrade(building)
    
    if not upgrade:
        _, reason, _ = manager.check_upgrade_requirements(building)
        raise HTTPException(status_code=400, detail=reason or "Cannot upgrade")
    
    return {"message": f"Upgrade started! Will complete at {upgrade.finish_time}"}


@router.get("/upgrades", response_model=List[UpgradeQueueResponse])
async def get_active_upgrades(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """Get all active upgrades in the queue."""
    village = db.query(Village).filter(Village.user_id == current_user.id).first()
    
    if not village:
        raise HTTPException(status_code=404, detail="Village not found")
    
    upgrades = db.query(UpgradeQueue).filter(UpgradeQueue.village_id == village.id).all()
    
    now = datetime.utcnow()
    result = []
    for u in upgrades:
        seconds_remaining = max(0, int((u.finish_time - now).total_seconds()))
        building = db.query(Building).filter(Building.id == u.building_id).first()
        
        result.append(UpgradeQueueResponse(
            id=u.id,
            building_id=u.building_id,
            building_type=building.building_type if building else None,
            target_level=u.target_level,
            start_time=u.start_time,
            finish_time=u.finish_time,
            is_paused=u.is_paused,
            seconds_remaining=seconds_remaining
        ))
    
    return result


# ============================================================
# RAID ENDPOINTS
# ============================================================

@router.get("/raid/search", response_model=RaidSearchResponse)
async def search_for_opponent(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Find an opponent to raid.
    Uses FairPlay matchmaking to find someone in the same league.
    """
    # Check if user can raid (not shielded)
    village = db.query(Village).filter(Village.user_id == current_user.id).first()
    if village and village.shield_active:
        raise HTTPException(status_code=400, detail="You have a shield active. Cannot raid.")
    
    # Find opponent using clustering
    clustering = LeagueClustering(db)
    opponent_ids = clustering.find_opponents(current_user.id, count=1)
    
    if not opponent_ids:
        raise HTTPException(status_code=404, detail="No suitable opponents found")
    
    opponent_id = opponent_ids[0]
    opponent = db.query(User).filter(User.id == opponent_id).first()
    opponent_village = db.query(Village).filter(Village.user_id == opponent_id).first()
    
    if not opponent or not opponent_village:
        raise HTTPException(status_code=404, detail="Opponent data not found")
    
    # Estimate loot
    raid_engine = RaidEngine(db, current_user.id)
    defense_power = raid_engine.calculate_defense_power(opponent_id)
    
    return RaidSearchResponse(
        opponent_id=opponent.id,
        opponent_username=opponent.username,
        opponent_league=opponent.league_tier,
        opponent_town_hall=opponent_village.town_hall_level,
        estimated_loot_gold=int(opponent_village.gold * 0.2),
        estimated_loot_elixir=int(opponent_village.elixir * 0.2),
        defense_power=defense_power
    )


@router.post("/raid/attack", response_model=RaidBattleResult)
async def attack_opponent(
    raid_data: RaidBattleRequest,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Execute a raid attack on an opponent.
    Battle is simulated based on biological stats.
    """
    # Validate opponent exists
    opponent = db.query(User).filter(User.id == raid_data.opponent_id).first()
    if not opponent:
        raise HTTPException(status_code=404, detail="Opponent not found")
    
    # Run battle simulation
    raid_engine = RaidEngine(db, current_user.id)
    result = raid_engine.simulate_battle(raid_data.opponent_id)
    
    # Apply loot transfer
    if result["victory"]:
        attacker_village = db.query(Village).filter(Village.user_id == current_user.id).first()
        defender_village = db.query(Village).filter(Village.user_id == raid_data.opponent_id).first()
        
        if attacker_village and defender_village:
            # Take from defender
            defender_village.gold -= result["gold_stolen"]
            defender_village.elixir -= result["elixir_stolen"]
            defender_village.dark_elixir -= result["dark_elixir_stolen"]
            
            # Give to attacker
            attacker_village.gold = min(
                attacker_village.gold + result["gold_stolen"],
                attacker_village.gold_capacity
            )
            attacker_village.elixir = min(
                attacker_village.elixir + result["elixir_stolen"],
                attacker_village.elixir_capacity
            )
            attacker_village.dark_elixir = min(
                attacker_village.dark_elixir + result["dark_elixir_stolen"],
                attacker_village.dark_elixir_capacity
            )
            
            db.commit()
    
    return RaidBattleResult(**result)


# ============================================================
# FAIRPLAY ENDPOINTS
# ============================================================

@router.get("/fairplay/recovery", response_model=RecoveryScoreResponse)
async def get_recovery_score(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get the Fatigue Oracle's recovery assessment.
    This is the core of the anti-burnout system.
    """
    oracle = FatigueOracle(db, current_user.id)
    score, status, recommendations = oracle.calculate_recovery_score()
    
    shield_active = score < 30
    shield_reason = "Low recovery score triggered automatic protection" if shield_active else None
    
    # Update user's recovery score
    current_user.recovery_score = score
    
    # Activate shield on village if needed
    if shield_active:
        village = db.query(Village).filter(Village.user_id == current_user.id).first()
        if village:
            from datetime import timedelta
            village.shield_active = True
            village.shield_end_time = datetime.utcnow() + timedelta(hours=8)
    
    db.commit()
    
    return RecoveryScoreResponse(
        recovery_percent=score,
        status=status,
        shield_active=shield_active,
        shield_reason=shield_reason,
        recommendations=recommendations
    )


@router.get("/fairplay/league", response_model=LeagueInfoResponse)
async def get_league_info(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get user's league information.
    Leagues are determined by K-Means clustering of biological output.
    """
    clustering = LeagueClustering(db)
    
    # Recalculate league
    new_league = clustering.assign_league(current_user.id)
    
    if current_user.league_tier != new_league:
        current_user.league_tier = new_league
        db.commit()
    
    # Count users in same league
    users_in_league = db.query(User).filter(User.league_tier == current_user.league_tier).count()
    
    return LeagueInfoResponse(
        current_league=current_user.league_tier,
        league_rank=1,  # Would need ranking calculation
        users_in_league=users_in_league,
        promotion_threshold=0.8,
        demotion_threshold=0.2
    )
