"""
User Domain Schemas (Pydantic DTOs)
Request/Response models for User and Profile APIs.
"""
from pydantic import BaseModel, EmailStr, Field
from typing import Optional, List
from datetime import datetime

from app.core.enums import FitnessGoal, ExperienceLevel, Gender, LeagueTier


# ============================================================
# AUTH SCHEMAS
# ============================================================

class UserCreate(BaseModel):
    """Registration request."""
    email: EmailStr
    username: str = Field(..., min_length=3, max_length=50)
    password: str = Field(..., min_length=6)


class UserLogin(BaseModel):
    """Login request."""
    email: EmailStr
    password: str


class Token(BaseModel):
    """JWT Token response."""
    access_token: str
    token_type: str = "bearer"


class TokenData(BaseModel):
    """Decoded token payload."""
    user_id: Optional[str] = None


# ============================================================
# USER SCHEMAS
# ============================================================

class UserBase(BaseModel):
    """Base user fields."""
    email: EmailStr
    username: str


class UserResponse(UserBase):
    """User response (public fields)."""
    id: str
    consistency_score: float
    recovery_score: float
    league_tier: LeagueTier
    created_at: datetime

    class Config:
        from_attributes = True


class UserWithProfile(UserResponse):
    """User with profile included."""
    profile: Optional["ProfileResponse"] = None


# ============================================================
# PROFILE SCHEMAS
# ============================================================

class ProfileCreate(BaseModel):
    """Onboarding profile creation."""
    age: int = Field(..., ge=13, le=100)
    weight_kg: float = Field(..., ge=30, le=300)
    height_cm: float = Field(..., ge=100, le=250)
    gender: Gender
    goal: FitnessGoal
    experience_level: ExperienceLevel
    injuries: List[str] = []
    units_weight: str = "kg"
    units_height: str = "cm"


class ProfileUpdate(BaseModel):
    """Profile update (partial)."""
    age: Optional[int] = None
    weight_kg: Optional[float] = None
    height_cm: Optional[float] = None
    body_fat_percent: Optional[float] = None
    goal: Optional[FitnessGoal] = None
    experience_level: Optional[ExperienceLevel] = None
    injuries: Optional[List[str]] = None


class ProfileResponse(BaseModel):
    """Profile response."""
    id: str
    age: Optional[int]
    weight_kg: Optional[float]
    height_cm: Optional[float]
    gender: Optional[Gender]
    body_fat_percent: Optional[float]
    goal: FitnessGoal
    experience_level: ExperienceLevel
    bmr: Optional[float]
    tdee: Optional[float]
    injuries: List[str]
    dark_mode: int

    class Config:
        from_attributes = True


# Forward reference resolution
UserWithProfile.model_rebuild()
