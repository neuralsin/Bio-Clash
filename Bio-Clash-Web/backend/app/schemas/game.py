"""
Game Domain Schemas (Pydantic DTOs)
Request/Response models for Village, Building, and Game APIs.
"""
from pydantic import BaseModel, Field
from typing import Optional, List
from datetime import datetime

from app.core.enums import BuildingType, LeagueTier


# ============================================================
# BUILDING SCHEMAS
# ============================================================

class BuildingBase(BaseModel):
    """Base building fields."""
    building_type: BuildingType
    level: int
    position_x: int
    position_y: int


class BuildingResponse(BuildingBase):
    """Building response with combat stats."""
    id: str
    health: int
    max_health: int
    damage_per_second: float
    range_tiles: float
    is_upgrading: bool
    is_damaged: bool

    class Config:
        from_attributes = True


class BuildingUpgradeRequest(BaseModel):
    """Request to upgrade a building."""
    building_id: str


class BuildingUpgradeRequirement(BaseModel):
    """What's needed to upgrade a building."""
    building_type: BuildingType
    current_level: int
    target_level: int
    gold_cost: int = 0
    elixir_cost: int = 0
    dark_elixir_cost: int = 0
    required_muscle: str  # The muscle group driving this building
    required_volume_kg: float  # Volume needed
    current_volume_kg: float  # User's current volume
    requirement_met: bool
    upgrade_duration_seconds: int


# ============================================================
# VILLAGE SCHEMAS
# ============================================================

class VillageBase(BaseModel):
    """Base village fields."""
    town_hall_level: int
    gold: int
    elixir: int
    dark_elixir: int
    gems: int


class VillageResponse(VillageBase):
    """Full village response for game view."""
    id: str
    user_id: str
    gold_capacity: int
    elixir_capacity: int
    dark_elixir_capacity: int
    gold_per_hour: float
    elixir_per_hour: float
    dark_elixir_per_hour: float
    shield_active: bool
    shield_end_time: Optional[datetime]
    buildings: List[BuildingResponse]
    last_resource_sync: datetime

    class Config:
        from_attributes = True


class ResourceSyncResponse(BaseModel):
    """Response after syncing resources."""
    gold: int
    elixir: int
    dark_elixir: int
    gold_gained: int
    elixir_gained: int
    dark_elixir_gained: int
    seconds_since_last_sync: int


# ============================================================
# UPGRADE QUEUE SCHEMAS
# ============================================================

class UpgradeQueueResponse(BaseModel):
    """Active upgrade status."""
    id: str
    building_id: str
    building_type: BuildingType
    target_level: int
    start_time: datetime
    finish_time: datetime
    is_paused: bool
    seconds_remaining: int

    class Config:
        from_attributes = True


# ============================================================
# RAID SCHEMAS
# ============================================================

class RaidSearchResponse(BaseModel):
    """Opponent found for raid."""
    opponent_id: str
    opponent_username: str
    opponent_league: LeagueTier
    opponent_town_hall: int
    estimated_loot_gold: int
    estimated_loot_elixir: int
    defense_power: float


class RaidBattleRequest(BaseModel):
    """Start a raid attack."""
    opponent_id: str


class RaidBattleResult(BaseModel):
    """Result of a raid battle."""
    victory: bool
    stars: int  # 0-3 stars
    damage_percent: float
    gold_stolen: int
    elixir_stolen: int
    dark_elixir_stolen: int
    trophies_gained: int
    attack_power_used: float
    defense_power_faced: float


# ============================================================
# FAIRPLAY SCHEMAS
# ============================================================

class RecoveryScoreResponse(BaseModel):
    """Recovery score from Fatigue Oracle."""
    recovery_percent: float
    status: str  # "optimal", "fatigued", "critical"
    shield_active: bool
    shield_reason: Optional[str]
    recommendations: List[str]


class LeagueInfoResponse(BaseModel):
    """User's league information."""
    current_league: LeagueTier
    league_rank: int
    users_in_league: int
    promotion_threshold: float  # Activity score needed to promote
    demotion_threshold: float   # Activity score below which you demote
