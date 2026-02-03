"""
Clan Domain Schemas (Pydantic DTOs)
Request/Response models for Clan system.
"""
from pydantic import BaseModel, Field
from typing import Optional, List
from datetime import datetime

from app.core.enums import ClanRole, WarState, LeagueTier


# ============================================================
# CLAN SCHEMAS
# ============================================================

class ClanCreate(BaseModel):
    """Create a new clan."""
    name: str = Field(..., min_length=3, max_length=50)
    tag: str = Field(..., min_length=3, max_length=10)
    description: Optional[str] = ""
    badge_icon: str = "shield"
    badge_color: str = "#FFD700"
    is_public: bool = True
    min_trophies_required: int = 0


class ClanResponse(BaseModel):
    """Clan public info."""
    id: str
    name: str
    tag: str
    description: str
    badge_icon: str
    badge_color: str
    level: int
    total_trophies: int
    war_wins: int
    war_streak: int
    member_count: int = 0
    max_members: int
    is_public: bool
    total_attack_power: float
    total_defense_power: float
    
    class Config:
        from_attributes = True


class ClanDetailResponse(ClanResponse):
    """Clan with full details including members."""
    members: List["ClanMemberResponse"]
    
    class Config:
        from_attributes = True


class ClanMemberResponse(BaseModel):
    """Member info within a clan."""
    id: str
    user_id: str
    username: str
    role: ClanRole
    donations: int
    war_stars_earned: int
    attacks_won: int
    league_tier: LeagueTier
    joined_at: datetime
    
    class Config:
        from_attributes = True


class JoinClanRequest(BaseModel):
    """Request to join a clan."""
    clan_id: str


class PromoteMemberRequest(BaseModel):
    """Promote/demote a member."""
    member_id: str
    new_role: ClanRole


# ============================================================
# CLAN WAR SCHEMAS
# ============================================================

class WarSearchResponse(BaseModel):
    """Response when searching for war."""
    message: str
    war_id: Optional[str] = None
    state: WarState


class ClanWarResponse(BaseModel):
    """Clan war status."""
    id: str
    clan_name: str
    opponent_clan_name: str
    state: WarState
    clan_stars: int
    opponent_stars: int
    clan_destruction: float
    opponent_destruction: float
    battle_start: Optional[datetime]
    battle_end: Optional[datetime]
    time_remaining_seconds: Optional[int]  # Until next phase
    
    class Config:
        from_attributes = True


class WarMemberStatus(BaseModel):
    """Status of a member in war."""
    user_id: str
    username: str
    attack_power: float
    attacks_used: int
    max_attacks: int = 2
    stars_earned: int


class WarDetailResponse(ClanWarResponse):
    """War with member details."""
    clan_members: List[WarMemberStatus]
    opponent_members: List[WarMemberStatus]
    attacks: List["WarAttackResponse"]


class WarAttackRequest(BaseModel):
    """Request to attack in war."""
    war_id: str
    defender_id: str


class WarAttackResponse(BaseModel):
    """Result of a war attack."""
    attacker_username: str
    defender_username: str
    stars: int
    destruction_percent: float
    attack_power_used: float
    defense_power_faced: float
    attack_time: datetime
    
    class Config:
        from_attributes = True


# ============================================================
# CLAN CHAT SCHEMAS
# ============================================================

class ClanMessageCreate(BaseModel):
    """Send a message to clan chat."""
    message: str = Field(..., min_length=1, max_length=500)


class ClanMessageResponse(BaseModel):
    """Chat message response."""
    id: str
    user_id: str
    username: str
    message: str
    message_type: str
    created_at: datetime
    
    class Config:
        from_attributes = True


# Forward reference resolution
ClanDetailResponse.model_rebuild()
WarDetailResponse.model_rebuild()
