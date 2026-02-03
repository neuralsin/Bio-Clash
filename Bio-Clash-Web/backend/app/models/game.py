"""
Game Domain Models
- Village: The user's base (1:1 with User)
- Building: Individual structures in the village
- UpgradeQueue: Active upgrades with timers
"""
import uuid
from datetime import datetime
from sqlalchemy import Column, String, Integer, Float, DateTime, Boolean, ForeignKey, Enum as SQLEnum
from sqlalchemy.orm import relationship

from app.db.session import Base
from app.core.enums import BuildingType


class Village(Base):
    """
    The user's game base. 1:1 with User.
    Contains resources and high-level stats.
    """
    __tablename__ = "villages"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), unique=True, nullable=False)
    
    # Town Hall (Limited by Consistency Score)
    town_hall_level = Column(Integer, default=1)
    
    # Resources
    gold = Column(Integer, default=500)
    gold_capacity = Column(Integer, default=5000)
    elixir = Column(Integer, default=500)
    elixir_capacity = Column(Integer, default=5000)
    dark_elixir = Column(Integer, default=0)
    dark_elixir_capacity = Column(Integer, default=500)
    gems = Column(Integer, default=50)  # Earned via streaks/PRs
    
    # Resource Generation (per hour, updated by fitness data)
    gold_per_hour = Column(Float, default=100.0)
    elixir_per_hour = Column(Float, default=100.0)
    dark_elixir_per_hour = Column(Float, default=0.0)
    
    # Shield (Fatigue Oracle protection)
    shield_active = Column(Boolean, default=False)
    shield_end_time = Column(DateTime, nullable=True)
    
    # Timestamps for resource sync
    last_resource_sync = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    user = relationship("User", back_populates="village")
    buildings = relationship("Building", back_populates="village", cascade="all, delete-orphan")
    upgrade_queue = relationship("UpgradeQueue", back_populates="village", cascade="all, delete-orphan")


class Building(Base):
    """
    Individual building in a village.
    Level is gated by corresponding MuscleGroup volume.
    """
    __tablename__ = "buildings"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    village_id = Column(String(36), ForeignKey("villages.id"), nullable=False, index=True)
    
    # Building Identity
    building_type = Column(SQLEnum(BuildingType), nullable=False, index=True)
    level = Column(Integer, default=1)
    
    # Position on grid (for rendering)
    position_x = Column(Integer, default=0)
    position_y = Column(Integer, default=0)
    
    # Combat Stats (scaled by level)
    health = Column(Integer, default=100)
    max_health = Column(Integer, default=100)
    damage_per_second = Column(Float, default=10.0)
    range_tiles = Column(Float, default=5.0)
    
    # Status
    is_upgrading = Column(Boolean, default=False)
    is_damaged = Column(Boolean, default=False)
    
    # Timestamps
    created_at = Column(DateTime, default=datetime.utcnow)
    
    # Relationships
    village = relationship("Village", back_populates="buildings")


class UpgradeQueue(Base):
    """
    Active upgrade for a building.
    Can be paused if Recovery Score drops too low.
    """
    __tablename__ = "upgrade_queue"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    village_id = Column(String(36), ForeignKey("villages.id"), nullable=False, index=True)
    building_id = Column(String(36), ForeignKey("buildings.id"), nullable=False)
    
    # Upgrade Info
    target_level = Column(Integer, nullable=False)
    
    # Costs (Snapshot at time of upgrade start)
    gold_cost = Column(Integer, default=0)
    elixir_cost = Column(Integer, default=0)
    dark_elixir_cost = Column(Integer, default=0)
    
    # Timing
    start_time = Column(DateTime, default=datetime.utcnow)
    duration_seconds = Column(Integer, nullable=False)  # Base duration
    finish_time = Column(DateTime, nullable=False)
    
    # FairPlay: Paused if fatigued
    is_paused = Column(Boolean, default=False)
    pause_start_time = Column(DateTime, nullable=True)
    accumulated_pause_seconds = Column(Integer, default=0)
    
    # Fitness Requirement Met
    fitness_requirement_met = Column(Boolean, default=True)
    required_muscle_volume = Column(Float, default=0.0)  # Volume needed to unlock
    
    # Relationships
    village = relationship("Village", back_populates="upgrade_queue")


# ============================================================
# THE CODEX: Building Requirements Mapping
# This is a utility mapping, not a DB model.
# It defines what muscle volume unlocks each building level.
# ============================================================

BUILDING_REQUIREMENTS = {
    BuildingType.TOWN_HALL: {
        "driver": "consistency_score",  # Special: not a muscle, but streak
        "levels": {
            1: {"consistency_score": 0},
            2: {"consistency_score": 20},
            3: {"consistency_score": 40},
            4: {"consistency_score": 60},
            5: {"consistency_score": 80},
        }
    },
    BuildingType.ARCHER_TOWER: {
        "driver": "chest",
        "levels": {
            1: {"volume_kg": 0, "gold_cost": 0},
            2: {"volume_kg": 500, "gold_cost": 1000},
            3: {"volume_kg": 2000, "gold_cost": 5000},
            4: {"volume_kg": 5000, "gold_cost": 15000},
            5: {"volume_kg": 10000, "gold_cost": 50000},
        }
    },
    BuildingType.CANNON: {
        "driver": "back",
        "levels": {
            1: {"volume_kg": 0, "gold_cost": 0},
            2: {"volume_kg": 500, "gold_cost": 1000},
            3: {"volume_kg": 2000, "gold_cost": 5000},
            4: {"volume_kg": 5000, "gold_cost": 15000},
            5: {"volume_kg": 10000, "gold_cost": 50000},
        }
    },
    BuildingType.MORTAR: {
        "driver": "triceps",
        "levels": {
            1: {"volume_kg": 0, "gold_cost": 0},
            2: {"volume_kg": 300, "gold_cost": 800},
            3: {"volume_kg": 1000, "gold_cost": 4000},
            4: {"volume_kg": 3000, "gold_cost": 12000},
            5: {"volume_kg": 7000, "gold_cost": 40000},
        }
    },
    BuildingType.WIZARD_TOWER: {
        "driver": "shoulders",
        "levels": {
            1: {"volume_kg": 0, "gold_cost": 0},
            2: {"volume_kg": 400, "gold_cost": 1200},
            3: {"volume_kg": 1500, "gold_cost": 6000},
            4: {"volume_kg": 4000, "gold_cost": 18000},
            5: {"volume_kg": 8000, "gold_cost": 55000},
        }
    },
    BuildingType.INFERNO_TOWER: {
        "driver": "legs",
        "levels": {
            1: {"volume_kg": 0, "dark_elixir_cost": 0},
            2: {"volume_kg": 2000, "dark_elixir_cost": 100},
            3: {"volume_kg": 6000, "dark_elixir_cost": 300},
            4: {"volume_kg": 15000, "dark_elixir_cost": 600},
            5: {"volume_kg": 30000, "dark_elixir_cost": 1000},
        }
    },
    BuildingType.HIDDEN_TESLA: {
        "driver": "biceps",
        "levels": {
            1: {"volume_kg": 0, "gold_cost": 0},
            2: {"volume_kg": 200, "gold_cost": 500},
            3: {"volume_kg": 800, "gold_cost": 2500},
            4: {"volume_kg": 2000, "gold_cost": 8000},
            5: {"volume_kg": 5000, "gold_cost": 25000},
        }
    },
    BuildingType.X_BOW: {
        "driver": "cardio",
        "levels": {
            1: {"cardio_minutes": 0, "elixir_cost": 0},
            2: {"cardio_minutes": 60, "elixir_cost": 2000},
            3: {"cardio_minutes": 200, "elixir_cost": 8000},
            4: {"cardio_minutes": 500, "elixir_cost": 25000},
            5: {"cardio_minutes": 1000, "elixir_cost": 75000},
        }
    },
    BuildingType.EAGLE_ARTILLERY: {
        "driver": "compound",
        "levels": {
            1: {"volume_kg": 0, "dark_elixir_cost": 0},
            2: {"volume_kg": 5000, "dark_elixir_cost": 200},
            3: {"volume_kg": 15000, "dark_elixir_cost": 500},
            4: {"volume_kg": 35000, "dark_elixir_cost": 1000},
            5: {"volume_kg": 75000, "dark_elixir_cost": 2000},
        }
    },
    BuildingType.WALLS: {
        "driver": "core",
        "levels": {
            1: {"volume_kg": 0, "gold_cost": 0},
            2: {"volume_kg": 500, "gold_cost": 500},
            3: {"volume_kg": 1500, "gold_cost": 2000},
            4: {"volume_kg": 4000, "gold_cost": 8000},
            5: {"volume_kg": 10000, "gold_cost": 25000},
        }
    },
    BuildingType.AIR_DEFENSE: {
        "driver": "traps",
        "levels": {
            1: {"volume_kg": 0, "gold_cost": 0},
            2: {"volume_kg": 300, "gold_cost": 800},
            3: {"volume_kg": 1000, "gold_cost": 4000},
            4: {"volume_kg": 3000, "gold_cost": 12000},
            5: {"volume_kg": 7000, "gold_cost": 40000},
        }
    },
}
