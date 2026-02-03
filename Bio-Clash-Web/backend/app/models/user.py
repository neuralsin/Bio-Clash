"""
User Domain Models
- User: Core authentication entity
- Profile: Extended user information for fitness calculations
"""
import uuid
from datetime import datetime
from sqlalchemy import Column, String, Integer, Float, DateTime, ForeignKey, Enum as SQLEnum
from sqlalchemy.dialects.sqlite import JSON
from sqlalchemy.orm import relationship

from app.db.session import Base
from app.core.enums import FitnessGoal, ExperienceLevel, Gender, LeagueTier


class User(Base):
    """
    Core user entity for authentication.
    The 'consistency_score' is central to Town Hall progression.
    """
    __tablename__ = "users"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    email = Column(String(255), unique=True, index=True, nullable=False)
    hashed_password = Column(String(255), nullable=False)
    username = Column(String(50), unique=True, index=True, nullable=False)
    
    # FairPlay Metrics (Updated by engines)
    consistency_score = Column(Float, default=0.0)  # 0-100, determines Town Hall cap
    recovery_score = Column(Float, default=100.0)   # 0-100, from Fatigue Oracle
    league_tier = Column(SQLEnum(LeagueTier), default=LeagueTier.BRONZE)
    
    # Timestamps
    created_at = Column(DateTime, default=datetime.utcnow)
    last_login = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    profile = relationship("Profile", back_populates="user", uselist=False)
    village = relationship("Village", back_populates="user", uselist=False)
    workout_logs = relationship("WorkoutLog", back_populates="user")
    daily_biometrics = relationship("DailyBiometrics", back_populates="user")


class Profile(Base):
    """
    Extended user profile for fitness calculations (BMR, TDEE).
    Captures onboarding data.
    """
    __tablename__ = "profiles"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), unique=True, nullable=False)
    
    # Physical Stats
    age = Column(Integer, nullable=True)
    weight_kg = Column(Float, nullable=True)
    height_cm = Column(Float, nullable=True)
    gender = Column(SQLEnum(Gender), nullable=True)
    body_fat_percent = Column(Float, nullable=True)
    
    # Goals & Experience
    goal = Column(SQLEnum(FitnessGoal), default=FitnessGoal.GENERAL_HEALTH)
    experience_level = Column(SQLEnum(ExperienceLevel), default=ExperienceLevel.BEGINNER)
    
    # Injury Flags (JSON for flexibility)
    injuries = Column(JSON, default=list)  # e.g., ["knee", "lower_back"]
    
    # Calculated Metrics
    bmr = Column(Float, nullable=True)  # Basal Metabolic Rate
    tdee = Column(Float, nullable=True) # Total Daily Energy Expenditure
    
    # Preferences
    units_weight = Column(String(10), default="kg")  # kg or lbs
    units_height = Column(String(10), default="cm")  # cm or ft
    dark_mode = Column(Integer, default=1)  # 1 = enabled
    
    # Relationship
    user = relationship("User", back_populates="profile")
