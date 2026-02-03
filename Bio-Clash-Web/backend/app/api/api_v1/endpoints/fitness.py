"""
Fitness API Endpoints
Exercise library, Workout logging, Biometrics, and Stats.
"""
from datetime import date, datetime
from typing import List, Optional
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.orm import Session
from sqlalchemy import func

from app.db.session import get_db
from app.core.deps import get_current_active_user
from app.core.enums import MuscleGroup
from app.models.user import User
from app.models.fitness import Exercise, WorkoutLog, WorkoutSet, DailyBiometrics
from app.schemas.fitness import (
    ExerciseResponse, ExerciseListResponse,
    WorkoutLogCreate, WorkoutLogResponse, WorkoutSummary, WorkoutSetResponse,
    BiometricsCreate, BiometricsResponse,
    MuscleVolumeStats, UserFitnessStats
)

router = APIRouter(prefix="/fitness", tags=["Fitness"])


# ============================================================
# EXERCISE ENDPOINTS
# ============================================================

@router.get("/exercises", response_model=List[ExerciseResponse])
async def get_all_exercises(
    muscle_group: Optional[MuscleGroup] = None,
    db: Session = Depends(get_db)
):
    """
    Get all exercises, optionally filtered by muscle group.
    """
    query = db.query(Exercise)
    
    if muscle_group:
        query = query.filter(Exercise.primary_muscle == muscle_group)
    
    exercises = query.order_by(Exercise.name).all()
    return exercises


@router.get("/exercises/grouped", response_model=List[ExerciseListResponse])
async def get_exercises_grouped(db: Session = Depends(get_db)):
    """
    Get all exercises grouped by muscle group (for UI selector).
    """
    result = []
    for muscle in MuscleGroup:
        exercises = db.query(Exercise).filter(
            Exercise.primary_muscle == muscle
        ).order_by(Exercise.name).all()
        
        if exercises:
            result.append(ExerciseListResponse(
                muscle_group=muscle,
                exercises=exercises
            ))
    
    return result


# ============================================================
# WORKOUT ENDPOINTS
# ============================================================

@router.post("/workout", response_model=WorkoutSummary)
async def log_workout(
    workout_data: WorkoutLogCreate,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Log a complete workout session.
    
    THE CORE MECHANIC: Each set adds volume to the corresponding muscle,
    unlocking building upgrades in the game.
    """
    # Create workout log
    workout_log = WorkoutLog(
        user_id=current_user.id,
        date=workout_data.date or date.today(),
        start_time=workout_data.start_time,
        end_time=workout_data.end_time,
        notes=workout_data.notes
    )
    
    if workout_data.start_time and workout_data.end_time:
        duration = (workout_data.end_time - workout_data.start_time).total_seconds() / 60
        workout_log.duration_minutes = int(duration)
    
    db.add(workout_log)
    db.flush()
    
    # Process sets and calculate volumes
    total_volume = 0.0
    total_rpe = 0.0
    rpe_count = 0
    muscles_worked = {}
    
    for set_data in workout_data.sets:
        # Get exercise for muscle mapping
        exercise = db.query(Exercise).filter(Exercise.id == set_data.exercise_id).first()
        if not exercise:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Exercise {set_data.exercise_id} not found"
            )
        
        # Calculate set volume
        set_volume = (set_data.weight_kg or 0) * (set_data.reps or 0)
        
        # Create set record
        workout_set = WorkoutSet(
            workout_log_id=workout_log.id,
            exercise_id=set_data.exercise_id,
            set_number=set_data.set_number,
            reps=set_data.reps,
            weight_kg=set_data.weight_kg,
            distance_km=set_data.distance_km,
            duration_seconds=set_data.duration_seconds,
            rpe=set_data.rpe,
            rir=set_data.rir,
            tempo=set_data.tempo,
            volume=set_volume
        )
        db.add(workout_set)
        
        total_volume += set_volume
        
        if set_data.rpe:
            total_rpe += set_data.rpe
            rpe_count += 1
        
        # Track volume per muscle
        muscle_key = exercise.primary_muscle.value
        muscles_worked[muscle_key] = muscles_worked.get(muscle_key, 0) + set_volume
    
    # Update workout log aggregates
    workout_log.total_volume_kg = total_volume
    workout_log.total_sets = len(workout_data.sets)
    workout_log.avg_rpe = (total_rpe / rpe_count) if rpe_count > 0 else None
    
    # Calculate resources earned (THE HARVEST)
    # Gold from activity, scaled by volume
    gold_earned = int(total_volume / 10)  # 10kg = 1 gold
    
    # Elixir from workout completion (not sleep, that's separate)
    elixir_earned = int(len(workout_data.sets) * 5)  # 5 elixir per set
    
    # Update village resources
    from app.models.game import Village
    village = db.query(Village).filter(Village.user_id == current_user.id).first()
    if village:
        village.gold = min(village.gold + gold_earned, village.gold_capacity)
        village.elixir = min(village.elixir + elixir_earned, village.elixir_capacity)
    
    # Update user consistency (simple: +1 point per workout, scaled)
    current_user.consistency_score = min(100, current_user.consistency_score + 2)
    
    db.commit()
    db.refresh(workout_log)
    
    # Determine which buildings can now be upgraded
    buildings_unlocked = []  # Would query BUILDING_REQUIREMENTS
    
    return WorkoutSummary(
        workout_id=workout_log.id,
        total_volume_kg=total_volume,
        muscles_worked=muscles_worked,
        gold_earned=gold_earned,
        elixir_earned=elixir_earned,
        buildings_unlocked=buildings_unlocked
    )


@router.get("/workouts", response_model=List[WorkoutLogResponse])
async def get_workout_history(
    limit: int = Query(default=10, le=50),
    offset: int = Query(default=0, ge=0),
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """Get user's workout history."""
    workouts = db.query(WorkoutLog).filter(
        WorkoutLog.user_id == current_user.id
    ).order_by(WorkoutLog.date.desc()).offset(offset).limit(limit).all()
    
    # Enrich sets with exercise names
    result = []
    for workout in workouts:
        sets_with_names = []
        for s in workout.sets:
            exercise = db.query(Exercise).filter(Exercise.id == s.exercise_id).first()
            sets_with_names.append(WorkoutSetResponse(
                id=s.id,
                exercise_id=s.exercise_id,
                exercise_name=exercise.name if exercise else None,
                set_number=s.set_number,
                reps=s.reps,
                weight_kg=s.weight_kg,
                distance_km=s.distance_km,
                duration_seconds=s.duration_seconds,
                rpe=s.rpe,
                volume=s.volume
            ))
        
        result.append(WorkoutLogResponse(
            id=workout.id,
            user_id=workout.user_id,
            date=workout.date,
            duration_minutes=workout.duration_minutes,
            total_volume_kg=workout.total_volume_kg,
            total_sets=workout.total_sets,
            avg_rpe=workout.avg_rpe,
            notes=workout.notes,
            sets=sets_with_names,
            created_at=workout.created_at
        ))
    
    return result


# ============================================================
# BIOMETRICS ENDPOINTS
# ============================================================

@router.post("/biometrics", response_model=BiometricsResponse)
async def log_biometrics(
    data: BiometricsCreate,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Log daily biometrics (sleep, HRV, steps).
    This feeds the Fatigue Oracle and Elixir generation.
    """
    log_date = data.date or date.today()
    
    # Check if entry exists for today
    existing = db.query(DailyBiometrics).filter(
        DailyBiometrics.user_id == current_user.id,
        DailyBiometrics.date == log_date
    ).first()
    
    if existing:
        # Update existing entry
        for field, value in data.model_dump(exclude_unset=True, exclude={'date'}).items():
            setattr(existing, field, value)
        db.commit()
        db.refresh(existing)
        
        # Update elixir generation based on sleep
        _update_elixir_rate(db, current_user.id, existing.sleep_hours)
        
        return existing
    
    # Create new entry
    biometrics = DailyBiometrics(
        user_id=current_user.id,
        **data.model_dump(exclude_unset=True)
    )
    db.add(biometrics)
    db.commit()
    db.refresh(biometrics)
    
    # Update elixir generation based on sleep
    _update_elixir_rate(db, current_user.id, biometrics.sleep_hours)
    
    return biometrics


def _update_elixir_rate(db: Session, user_id: str, sleep_hours: Optional[float]):
    """Update village elixir generation based on sleep."""
    if sleep_hours is None:
        return
    
    from app.models.game import Village
    village = db.query(Village).filter(Village.user_id == user_id).first()
    if village:
        # Base rate 100/hr, +20% per hour of sleep over 6
        bonus_hours = max(0, sleep_hours - 6)
        village.elixir_per_hour = 100 * (1 + bonus_hours * 0.2)
        db.commit()


@router.get("/stats", response_model=UserFitnessStats)
async def get_fitness_stats(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get aggregated fitness statistics.
    This is used for game upgrade requirements.
    """
    # Total workout count
    total_workouts = db.query(func.count(WorkoutLog.id)).filter(
        WorkoutLog.user_id == current_user.id
    ).scalar() or 0
    
    # Total volume
    total_volume = db.query(func.sum(WorkoutLog.total_volume_kg)).filter(
        WorkoutLog.user_id == current_user.id
    ).scalar() or 0.0
    
    # Volume per muscle group (THE CODEX lookups)
    muscle_stats = []
    for muscle in MuscleGroup:
        # Get exercises for this muscle
        exercise_ids = [e.id for e in db.query(Exercise).filter(
            Exercise.primary_muscle == muscle
        ).all()]
        
        if exercise_ids:
            # Sum volume from sets of these exercises
            volume = db.query(func.sum(WorkoutSet.volume)).filter(
                WorkoutSet.exercise_id.in_(exercise_ids),
                WorkoutSet.workout_log_id.in_(
                    db.query(WorkoutLog.id).filter(WorkoutLog.user_id == current_user.id)
                )
            ).scalar() or 0.0
            
            set_count = db.query(func.count(WorkoutSet.id)).filter(
                WorkoutSet.exercise_id.in_(exercise_ids),
                WorkoutSet.workout_log_id.in_(
                    db.query(WorkoutLog.id).filter(WorkoutLog.user_id == current_user.id)
                )
            ).scalar() or 0
            
            if volume > 0 or set_count > 0:
                muscle_stats.append(MuscleVolumeStats(
                    muscle_group=muscle,
                    total_volume_kg=volume,
                    total_sets=set_count,
                    last_workout_date=None  # Could query for this
                ))
    
    return UserFitnessStats(
        total_workouts=total_workouts,
        total_volume_kg=total_volume,
        avg_weekly_volume=total_volume / max(1, total_workouts / 3),  # Rough estimate
        muscle_volumes=muscle_stats,
        consistency_score=current_user.consistency_score,
        current_streak=0  # Would need streak tracking
    )
