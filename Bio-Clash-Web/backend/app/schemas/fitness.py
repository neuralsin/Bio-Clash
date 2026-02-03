"""
Fitness Domain Schemas (Pydantic DTOs)
Request/Response models for Exercise, Workout, and Biometrics APIs.
"""
from pydantic import BaseModel, Field
from typing import Optional, List
from datetime import datetime, date

from app.core.enums import MuscleGroup, ExerciseCategory


# ============================================================
# EXERCISE SCHEMAS
# ============================================================

class ExerciseBase(BaseModel):
    """Base exercise fields."""
    name: str
    primary_muscle: MuscleGroup
    secondary_muscles: Optional[str] = None
    category: ExerciseCategory
    equipment_needed: Optional[str] = None
    difficulty: int = Field(default=1, ge=1, le=5)


class ExerciseCreate(ExerciseBase):
    """Create new exercise."""
    description: Optional[str] = None
    video_url: Optional[str] = None


class ExerciseResponse(ExerciseBase):
    """Exercise response."""
    id: str
    description: Optional[str]
    video_url: Optional[str]

    class Config:
        from_attributes = True


class ExerciseListResponse(BaseModel):
    """Grouped exercise list for UI."""
    muscle_group: MuscleGroup
    exercises: List[ExerciseResponse]


# ============================================================
# WORKOUT SET SCHEMAS
# ============================================================

class WorkoutSetCreate(BaseModel):
    """Single set input during workout logging."""
    exercise_id: str
    set_number: int = Field(..., ge=1)
    reps: Optional[int] = Field(None, ge=0)
    weight_kg: Optional[float] = Field(None, ge=0)
    distance_km: Optional[float] = Field(None, ge=0)
    duration_seconds: Optional[int] = Field(None, ge=0)
    rpe: Optional[int] = Field(None, ge=1, le=10)
    rir: Optional[int] = Field(None, ge=0, le=10)
    tempo: Optional[str] = None


class WorkoutSetResponse(BaseModel):
    """Set response with calculated volume."""
    id: str
    exercise_id: str
    exercise_name: Optional[str] = None
    set_number: int
    reps: Optional[int]
    weight_kg: Optional[float]
    distance_km: Optional[float]
    duration_seconds: Optional[int]
    rpe: Optional[int]
    volume: float

    class Config:
        from_attributes = True


# ============================================================
# WORKOUT LOG SCHEMAS
# ============================================================

class WorkoutLogCreate(BaseModel):
    """Full workout session submission."""
    date: Optional[date] = None
    start_time: Optional[datetime] = None
    end_time: Optional[datetime] = None
    notes: Optional[str] = None
    sets: List[WorkoutSetCreate]


class WorkoutLogResponse(BaseModel):
    """Workout log response with aggregated stats."""
    id: str
    user_id: str
    date: date
    duration_minutes: Optional[int]
    total_volume_kg: float
    total_sets: int
    avg_rpe: Optional[float]
    notes: Optional[str]
    sets: List[WorkoutSetResponse]
    created_at: datetime

    class Config:
        from_attributes = True


class WorkoutSummary(BaseModel):
    """Post-workout summary showing rewards earned."""
    workout_id: str
    total_volume_kg: float
    muscles_worked: dict[str, float]  # MuscleGroup -> Volume
    gold_earned: int
    elixir_earned: int
    buildings_unlocked: List[str]  # Building types that can now be upgraded


# ============================================================
# BIOMETRICS SCHEMAS
# ============================================================

class BiometricsCreate(BaseModel):
    """Daily biometrics input."""
    date: Optional[date] = None
    sleep_hours: Optional[float] = Field(None, ge=0, le=24)
    sleep_quality: Optional[int] = Field(None, ge=1, le=10)
    deep_sleep_percent: Optional[float] = Field(None, ge=0, le=100)
    resting_hr: Optional[int] = Field(None, ge=30, le=200)
    hrv: Optional[float] = Field(None, ge=0)
    steps: Optional[int] = Field(None, ge=0)
    active_calories: Optional[int] = Field(None, ge=0)
    mood: Optional[int] = Field(None, ge=1, le=10)
    stress_level: Optional[int] = Field(None, ge=1, le=10)
    water_liters: Optional[float] = Field(None, ge=0)


class BiometricsResponse(BiometricsCreate):
    """Biometrics response."""
    id: str
    user_id: str
    created_at: datetime

    class Config:
        from_attributes = True


# ============================================================
# VOLUME AGGREGATION SCHEMAS
# ============================================================

class MuscleVolumeStats(BaseModel):
    """Volume stats per muscle group (for upgrades)."""
    muscle_group: MuscleGroup
    total_volume_kg: float
    total_sets: int
    last_workout_date: Optional[date]


class UserFitnessStats(BaseModel):
    """Aggregated fitness stats for a user."""
    total_workouts: int
    total_volume_kg: float
    avg_weekly_volume: float
    muscle_volumes: List[MuscleVolumeStats]
    consistency_score: float
    current_streak: int
