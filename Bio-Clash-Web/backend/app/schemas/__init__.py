# Schemas package - Export all schemas
from app.schemas.user import (
    UserCreate, UserLogin, Token, TokenData,
    UserBase, UserResponse, UserWithProfile,
    ProfileCreate, ProfileUpdate, ProfileResponse
)
from app.schemas.fitness import (
    ExerciseBase, ExerciseCreate, ExerciseResponse, ExerciseListResponse,
    WorkoutSetCreate, WorkoutSetResponse,
    WorkoutLogCreate, WorkoutLogResponse, WorkoutSummary,
    BiometricsCreate, BiometricsResponse,
    MuscleVolumeStats, UserFitnessStats
)
from app.schemas.game import (
    BuildingBase, BuildingResponse, BuildingUpgradeRequest, BuildingUpgradeRequirement,
    VillageBase, VillageResponse, ResourceSyncResponse,
    UpgradeQueueResponse,
    RaidSearchResponse, RaidBattleRequest, RaidBattleResult,
    RecoveryScoreResponse, LeagueInfoResponse
)
