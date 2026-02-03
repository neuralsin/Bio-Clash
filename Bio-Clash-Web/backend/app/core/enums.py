from enum import Enum

class MuscleGroup(str, Enum):
    """Muscle groups that map to specific buildings in the game."""
    CHEST = "chest"           # -> Archer Tower
    BACK = "back"             # -> Cannon
    LEGS = "legs"             # -> Inferno Tower
    SHOULDERS = "shoulders"   # -> Wizard Tower
    TRICEPS = "triceps"       # -> Mortar
    BICEPS = "biceps"         # -> Hidden Tesla
    CORE = "core"             # -> Walls
    CARDIO = "cardio"         # -> X-Bow
    COMPOUND = "compound"     # -> Eagle Artillery
    TRAPS = "traps"           # -> Air Defense

class ExerciseCategory(str, Enum):
    """Type of exercise."""
    STRENGTH = "strength"
    CARDIO = "cardio"
    FLEXIBILITY = "flexibility"
    COMPOUND = "compound"

class FitnessGoal(str, Enum):
    """User's primary fitness goal."""
    FAT_LOSS = "fat_loss"
    MUSCLE_GAIN = "muscle_gain"
    RECOMP = "recomp"
    ENDURANCE = "endurance"
    GENERAL_HEALTH = "general_health"

class ExperienceLevel(str, Enum):
    """User's training experience."""
    BEGINNER = "beginner"
    INTERMEDIATE = "intermediate"
    ADVANCED = "advanced"

class BuildingType(str, Enum):
    """Game building types mapped to muscle groups."""
    TOWN_HALL = "town_hall"           # Overall Consistency
    WALLS = "walls"                   # Core
    ARCHER_TOWER = "archer_tower"     # Chest
    CANNON = "cannon"                 # Back
    MORTAR = "mortar"                 # Triceps
    WIZARD_TOWER = "wizard_tower"     # Shoulders
    INFERNO_TOWER = "inferno_tower"   # Legs
    HIDDEN_TESLA = "hidden_tesla"     # Biceps
    X_BOW = "x_bow"                   # Cardio/Endurance
    EAGLE_ARTILLERY = "eagle_artillery"  # Compound Lifts
    AIR_DEFENSE = "air_defense"       # Traps
    GOLD_MINE = "gold_mine"           # Steps/Activity
    ELIXIR_COLLECTOR = "elixir_collector"  # Sleep/Recovery
    DARK_ELIXIR_DRILL = "dark_elixir_drill"  # Intensity/PRs
    ARMY_CAMP = "army_camp"           # Nutrition
    CLAN_CASTLE = "clan_castle"       # Social
    LABORATORY = "laboratory"         # Form/Skill
    BUILDERS_HUT = "builders_hut"     # Rest Days

class LeagueTier(str, Enum):
    """Fair matchmaking leagues based on biological output."""
    BRONZE = "bronze"
    SILVER = "silver"
    GOLD = "gold"
    CRYSTAL = "crystal"
    TITAN = "titan"

class Gender(str, Enum):
    """User gender for calculations."""
    MALE = "male"
    FEMALE = "female"
    OTHER = "other"


class ClanRole(str, Enum):
    """Roles within a clan."""
    LEADER = "leader"
    CO_LEADER = "co_leader"
    ELDER = "elder"
    MEMBER = "member"


class WarState(str, Enum):
    """State of a clan war."""
    MATCHMAKING = "matchmaking"  # Looking for opponent
    PREPARATION = "preparation"  # 1 day prep
    BATTLE = "battle"            # 2 day battle
    WAR_ENDED = "war_ended"      # War complete
