"""
Fitness Domain Models
- Exercise: The library of all exercises
- WorkoutLog: A single workout session
- WorkoutSet: Individual sets within a workout
- DailyBiometrics: Sleep, HRV, Steps tracking
"""
import uuid
from datetime import datetime, date
from sqlalchemy import Column, String, Integer, Float, DateTime, Date, ForeignKey, Enum as SQLEnum, Text
from sqlalchemy.orm import relationship

from app.db.session import Base
from app.core.enums import MuscleGroup, ExerciseCategory


class Exercise(Base):
    """
    Master exercise library.
    Each exercise is mapped to a MuscleGroup which determines building upgrades.
    """
    __tablename__ = "exercises"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    name = Column(String(100), unique=True, nullable=False, index=True)
    
    # Core Mapping (THE CODEX)
    primary_muscle = Column(SQLEnum(MuscleGroup), nullable=False, index=True)
    secondary_muscles = Column(String(255), nullable=True)  # Comma-separated
    
    category = Column(SQLEnum(ExerciseCategory), nullable=False)
    
    # Metadata
    description = Column(Text, nullable=True)
    equipment_needed = Column(String(255), nullable=True)
    difficulty = Column(Integer, default=1)  # 1-5 scale
    video_url = Column(String(500), nullable=True)
    
    # Workout sets relationship
    workout_sets = relationship("WorkoutSet", back_populates="exercise")


class WorkoutLog(Base):
    """
    A complete workout session.
    Contains multiple WorkoutSets.
    """
    __tablename__ = "workout_logs"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), nullable=False, index=True)
    
    # Session Info
    date = Column(Date, default=date.today, index=True)
    start_time = Column(DateTime, nullable=True)
    end_time = Column(DateTime, nullable=True)
    duration_minutes = Column(Integer, nullable=True)
    
    # Aggregated Stats (Calculated post-workout)
    total_volume_kg = Column(Float, default=0.0)  # Sum of (weight * reps)
    total_sets = Column(Integer, default=0)
    avg_rpe = Column(Float, nullable=True)  # Average RPE for session
    
    # Notes
    notes = Column(Text, nullable=True)
    
    # Timestamps
    created_at = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    user = relationship("User", back_populates="workout_logs")
    sets = relationship("WorkoutSet", back_populates="workout_log", cascade="all, delete-orphan")


class WorkoutSet(Base):
    """
    Individual set within a workout.
    This is THE granular data that powers the game.
    """
    __tablename__ = "workout_sets"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    workout_log_id = Column(String(36), ForeignKey("workout_logs.id"), nullable=False, index=True)
    exercise_id = Column(String(36), ForeignKey("exercises.id"), nullable=False, index=True)
    
    # Set Data
    set_number = Column(Integer, nullable=False)
    reps = Column(Integer, nullable=True)
    weight_kg = Column(Float, nullable=True)
    
    # Cardio-specific
    distance_km = Column(Float, nullable=True)
    duration_seconds = Column(Integer, nullable=True)
    
    # Advanced Tracking
    rpe = Column(Integer, nullable=True)  # Rate of Perceived Exertion (1-10)
    rir = Column(Integer, nullable=True)  # Reps in Reserve
    tempo = Column(String(20), nullable=True)  # e.g., "3-1-2-0"
    
    # Calculated
    volume = Column(Float, default=0.0)  # weight_kg * reps
    
    # Relationships
    workout_log = relationship("WorkoutLog", back_populates="sets")
    exercise = relationship("Exercise", back_populates="workout_sets")


class DailyBiometrics(Base):
    """
    Daily health metrics for the Fatigue Oracle.
    These drive Recovery Score and Shield mechanics.
    """
    __tablename__ = "daily_biometrics"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), nullable=False, index=True)
    date = Column(Date, default=date.today, index=True)
    
    # Sleep (Primary Elixir driver)
    sleep_hours = Column(Float, nullable=True)
    sleep_quality = Column(Integer, nullable=True)  # 1-10 subjective
    deep_sleep_percent = Column(Float, nullable=True)
    
    # Heart (Recovery indicators)
    resting_hr = Column(Integer, nullable=True)
    hrv = Column(Float, nullable=True)  # Heart Rate Variability (ms)
    
    # Activity (Gold driver)
    steps = Column(Integer, nullable=True)
    active_calories = Column(Integer, nullable=True)
    
    # Mood & Stress
    mood = Column(Integer, nullable=True)  # 1-10
    stress_level = Column(Integer, nullable=True)  # 1-10
    
    # Hydration
    water_liters = Column(Float, nullable=True)
    
    # Timestamps
    created_at = Column(DateTime, default=datetime.utcnow)
    
    # Relationship
    user = relationship("User", back_populates="daily_biometrics")
