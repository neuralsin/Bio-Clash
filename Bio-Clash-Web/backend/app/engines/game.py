"""
Game Engine
Handles village resources, building upgrades, and raid mechanics.
"""
from datetime import datetime, timedelta
from typing import Optional, Tuple, List
from sqlalchemy.orm import Session
from sqlalchemy import func

from app.core.config import settings
from app.core.enums import BuildingType, MuscleGroup
from app.models.game import Village, Building, UpgradeQueue, BUILDING_REQUIREMENTS
from app.models.fitness import WorkoutSet, WorkoutLog, Exercise
from app.models.user import User


class ResourceManager:
    """
    Manages passive resource generation and synchronization.
    
    Gold = Steps/Activity (from Gold Mines)
    Elixir = Sleep/Recovery (from Elixir Collectors)
    Dark Elixir = Intensity/PRs (from Dark Elixir Drills)
    """
    
    def __init__(self, db: Session, village: Village):
        self.db = db
        self.village = village
    
    def sync_resources(self) -> Tuple[int, int, int]:
        """
        Calculate and add resources since last sync.
        Returns (gold_gained, elixir_gained, dark_elixir_gained).
        """
        now = datetime.utcnow()
        seconds_elapsed = (now - self.village.last_resource_sync).total_seconds()
        hours_elapsed = seconds_elapsed / 3600
        
        # Calculate gains
        gold_gained = int(self.village.gold_per_hour * hours_elapsed)
        elixir_gained = int(self.village.elixir_per_hour * hours_elapsed)
        dark_gained = int(self.village.dark_elixir_per_hour * hours_elapsed)
        
        # Apply to village (respect capacity)
        self.village.gold = min(
            self.village.gold + gold_gained,
            self.village.gold_capacity
        )
        self.village.elixir = min(
            self.village.elixir + elixir_gained,
            self.village.elixir_capacity
        )
        self.village.dark_elixir = min(
            self.village.dark_elixir + dark_gained,
            self.village.dark_elixir_capacity
        )
        
        self.village.last_resource_sync = now
        self.db.commit()
        
        return gold_gained, elixir_gained, dark_gained


class UpgradeManager:
    """
    Manages building upgrades with THE CODEX requirements.
    
    Each building requires:
    1. Resource cost (Gold/Elixir/Dark Elixir)
    2. Fitness requirement (Volume in specific muscle group)
    """
    
    # Maps BuildingType to MuscleGroup
    BUILDING_TO_MUSCLE = {
        BuildingType.ARCHER_TOWER: MuscleGroup.CHEST,
        BuildingType.CANNON: MuscleGroup.BACK,
        BuildingType.MORTAR: MuscleGroup.TRICEPS,
        BuildingType.WIZARD_TOWER: MuscleGroup.SHOULDERS,
        BuildingType.INFERNO_TOWER: MuscleGroup.LEGS,
        BuildingType.HIDDEN_TESLA: MuscleGroup.BICEPS,
        BuildingType.X_BOW: MuscleGroup.CARDIO,
        BuildingType.EAGLE_ARTILLERY: MuscleGroup.COMPOUND,
        BuildingType.WALLS: MuscleGroup.CORE,
        BuildingType.AIR_DEFENSE: MuscleGroup.TRAPS,
    }
    
    def __init__(self, db: Session, user_id: str, village: Village):
        self.db = db
        self.user_id = user_id
        self.village = village
    
    def get_user_muscle_volume(self, muscle: MuscleGroup) -> float:
        """Get user's total volume for a specific muscle group."""
        # Get all exercises for this muscle
        exercise_ids = [e.id for e in self.db.query(Exercise).filter(
            Exercise.primary_muscle == muscle
        ).all()]
        
        if not exercise_ids:
            return 0.0
        
        # Sum volume from all sets
        volume = self.db.query(func.sum(WorkoutSet.volume)).filter(
            WorkoutSet.exercise_id.in_(exercise_ids),
            WorkoutSet.workout_log_id.in_(
                self.db.query(WorkoutLog.id).filter(WorkoutLog.user_id == self.user_id)
            )
        ).scalar() or 0.0
        
        return volume
    
    def check_upgrade_requirements(
        self, building: Building
    ) -> Tuple[bool, Optional[str], dict]:
        """
        Check if a building can be upgraded.
        
        Returns:
            (can_upgrade, reason_if_not, requirements_dict)
        """
        building_type = building.building_type
        target_level = building.level + 1
        
        # Get requirements from THE CODEX
        if building_type not in BUILDING_REQUIREMENTS:
            return False, "Building type not found in CODEX", {}
        
        reqs = BUILDING_REQUIREMENTS[building_type]
        level_reqs = reqs.get("levels", {}).get(target_level)
        
        if not level_reqs:
            return False, "Max level reached", {}
        
        requirements = {
            "target_level": target_level,
            "gold_cost": level_reqs.get("gold_cost", 0),
            "elixir_cost": level_reqs.get("elixir_cost", 0),
            "dark_elixir_cost": level_reqs.get("dark_elixir_cost", 0),
        }
        
        # Check Town Hall cap
        if target_level > self.village.town_hall_level + 2:
            return False, f"Upgrade Town Hall first (Current: {self.village.town_hall_level})", requirements
        
        # Check resource costs
        if self.village.gold < requirements["gold_cost"]:
            return False, f"Need {requirements['gold_cost']} Gold", requirements
        if self.village.elixir < requirements["elixir_cost"]:
            return False, f"Need {requirements['elixir_cost']} Elixir", requirements
        if self.village.dark_elixir < requirements["dark_elixir_cost"]:
            return False, f"Need {requirements['dark_elixir_cost']} Dark Elixir", requirements
        
        # Check fitness requirement (THE CODEX)
        if building_type in self.BUILDING_TO_MUSCLE:
            required_muscle = self.BUILDING_TO_MUSCLE[building_type]
            required_volume = level_reqs.get("volume_kg", 0)
            current_volume = self.get_user_muscle_volume(required_muscle)
            
            requirements["required_muscle"] = required_muscle.value
            requirements["required_volume_kg"] = required_volume
            requirements["current_volume_kg"] = current_volume
            
            if current_volume < required_volume:
                return False, f"Need {required_volume}kg {required_muscle.value} volume (You have: {current_volume:.0f}kg)", requirements
        
        # Special: Town Hall requires consistency
        if building_type == BuildingType.TOWN_HALL:
            user = self.db.query(User).filter(User.id == self.user_id).first()
            required_consistency = level_reqs.get("consistency_score", 0)
            
            requirements["required_consistency"] = required_consistency
            requirements["current_consistency"] = user.consistency_score if user else 0
            
            if user and user.consistency_score < required_consistency:
                return False, f"Need {required_consistency}% consistency (You have: {user.consistency_score:.0f}%)", requirements
        
        return True, None, requirements
    
    def start_upgrade(self, building: Building) -> Optional[UpgradeQueue]:
        """
        Start an upgrade for a building.
        Deducts resources and creates queue entry.
        """
        can_upgrade, reason, reqs = self.check_upgrade_requirements(building)
        
        if not can_upgrade:
            return None
        
        # Deduct resources
        self.village.gold -= reqs.get("gold_cost", 0)
        self.village.elixir -= reqs.get("elixir_cost", 0)
        self.village.dark_elixir -= reqs.get("dark_elixir_cost", 0)
        
        # Calculate duration (scales with level)
        base_duration = 60 * 60  # 1 hour base
        duration = base_duration * reqs["target_level"]
        
        # Create upgrade queue entry
        upgrade = UpgradeQueue(
            village_id=self.village.id,
            building_id=building.id,
            target_level=reqs["target_level"],
            gold_cost=reqs.get("gold_cost", 0),
            elixir_cost=reqs.get("elixir_cost", 0),
            dark_elixir_cost=reqs.get("dark_elixir_cost", 0),
            duration_seconds=duration,
            finish_time=datetime.utcnow() + timedelta(seconds=duration),
            required_muscle_volume=reqs.get("required_volume_kg", 0)
        )
        
        building.is_upgrading = True
        
        self.db.add(upgrade)
        self.db.commit()
        
        return upgrade


class RaidEngine:
    """
    Handles PvP raid mechanics.
    
    Attack Power = Upper body volume (Chest + Shoulders + Arms)
    Defense Power = Core + Legs + Back
    """
    
    def __init__(self, db: Session, attacker_id: str):
        self.db = db
        self.attacker_id = attacker_id
    
    def calculate_attack_power(self) -> float:
        """Calculate attacker's offensive power."""
        attack_muscles = [MuscleGroup.CHEST, MuscleGroup.SHOULDERS, MuscleGroup.TRICEPS, MuscleGroup.BICEPS]
        
        total = 0.0
        for muscle in attack_muscles:
            exercise_ids = [e.id for e in self.db.query(Exercise).filter(
                Exercise.primary_muscle == muscle
            ).all()]
            
            if exercise_ids:
                volume = self.db.query(func.sum(WorkoutSet.volume)).filter(
                    WorkoutSet.exercise_id.in_(exercise_ids),
                    WorkoutSet.workout_log_id.in_(
                        self.db.query(WorkoutLog.id).filter(WorkoutLog.user_id == self.attacker_id)
                    )
                ).scalar() or 0.0
                total += volume
        
        return total
    
    def calculate_defense_power(self, defender_id: str) -> float:
        """Calculate defender's defensive power."""
        defense_muscles = [MuscleGroup.CORE, MuscleGroup.LEGS, MuscleGroup.BACK]
        
        total = 0.0
        for muscle in defense_muscles:
            exercise_ids = [e.id for e in self.db.query(Exercise).filter(
                Exercise.primary_muscle == muscle
            ).all()]
            
            if exercise_ids:
                volume = self.db.query(func.sum(WorkoutSet.volume)).filter(
                    WorkoutSet.exercise_id.in_(exercise_ids),
                    WorkoutSet.workout_log_id.in_(
                        self.db.query(WorkoutLog.id).filter(WorkoutLog.user_id == defender_id)
                    )
                ).scalar() or 0.0
                total += volume
        
        return total
    
    def simulate_battle(self, defender_id: str) -> dict:
        """
        Simulate a raid battle.
        
        Returns battle result with loot.
        """
        attack_power = self.calculate_attack_power()
        defense_power = self.calculate_defense_power(defender_id)
        
        # Simple combat formula
        power_ratio = attack_power / max(defense_power, 1)
        
        # Determine stars and damage
        if power_ratio >= 2.0:
            stars = 3
            damage_percent = 100.0
        elif power_ratio >= 1.5:
            stars = 2
            damage_percent = 75.0
        elif power_ratio >= 1.0:
            stars = 1
            damage_percent = 50.0
        else:
            stars = 0
            damage_percent = power_ratio * 30
        
        victory = stars > 0
        
        # Calculate loot
        defender_village = self.db.query(Village).filter(Village.user_id == defender_id).first()
        
        if defender_village and victory:
            loot_percent = 0.1 + (stars * 0.05)  # 15-25% loot
            gold_stolen = int(defender_village.gold * loot_percent)
            elixir_stolen = int(defender_village.elixir * loot_percent)
            dark_stolen = int(defender_village.dark_elixir * loot_percent * 0.5)
        else:
            gold_stolen = 0
            elixir_stolen = 0
            dark_stolen = 0
        
        return {
            "victory": victory,
            "stars": stars,
            "damage_percent": damage_percent,
            "gold_stolen": gold_stolen,
            "elixir_stolen": elixir_stolen,
            "dark_elixir_stolen": dark_stolen,
            "attack_power_used": attack_power,
            "defense_power_faced": defense_power,
            "trophies_gained": stars * 10 if victory else -5
        }
