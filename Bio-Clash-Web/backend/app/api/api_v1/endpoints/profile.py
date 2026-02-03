"""
Profile API Endpoints
Onboarding and profile management.
"""
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session

from app.db.session import get_db
from app.core.deps import get_current_active_user
from app.models.user import User, Profile
from app.schemas.user import ProfileCreate, ProfileUpdate, ProfileResponse

router = APIRouter(prefix="/profile", tags=["Profile"])


def calculate_bmr(weight_kg: float, height_cm: float, age: int, gender: str) -> float:
    """
    Calculate Basal Metabolic Rate using Mifflin-St Jeor equation.
    """
    if gender == "male":
        return (10 * weight_kg) + (6.25 * height_cm) - (5 * age) + 5
    else:
        return (10 * weight_kg) + (6.25 * height_cm) - (5 * age) - 161


def calculate_tdee(bmr: float, activity_level: str = "moderate") -> float:
    """
    Calculate Total Daily Energy Expenditure.
    Activity multipliers: sedentary=1.2, light=1.375, moderate=1.55, active=1.725, very_active=1.9
    """
    multipliers = {
        "sedentary": 1.2,
        "light": 1.375,
        "moderate": 1.55,
        "active": 1.725,
        "very_active": 1.9
    }
    return bmr * multipliers.get(activity_level, 1.55)


@router.post("/onboard", response_model=ProfileResponse)
async def complete_onboarding(
    profile_data: ProfileCreate,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Complete user onboarding with physical stats and goals.
    Calculates BMR and TDEE.
    """
    profile = db.query(Profile).filter(Profile.user_id == current_user.id).first()
    
    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Profile not found"
        )
    
    # Update profile with onboarding data
    profile.age = profile_data.age
    profile.weight_kg = profile_data.weight_kg
    profile.height_cm = profile_data.height_cm
    profile.gender = profile_data.gender
    profile.goal = profile_data.goal
    profile.experience_level = profile_data.experience_level
    profile.injuries = profile_data.injuries
    profile.units_weight = profile_data.units_weight
    profile.units_height = profile_data.units_height
    
    # Calculate BMR and TDEE
    profile.bmr = calculate_bmr(
        profile_data.weight_kg,
        profile_data.height_cm,
        profile_data.age,
        profile_data.gender.value
    )
    profile.tdee = calculate_tdee(profile.bmr)
    
    db.commit()
    db.refresh(profile)
    
    return profile


@router.get("/", response_model=ProfileResponse)
async def get_profile(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """Get current user's profile."""
    profile = db.query(Profile).filter(Profile.user_id == current_user.id).first()
    
    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Profile not found"
        )
    
    return profile


@router.patch("/", response_model=ProfileResponse)
async def update_profile(
    profile_data: ProfileUpdate,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """Update user profile (partial update)."""
    profile = db.query(Profile).filter(Profile.user_id == current_user.id).first()
    
    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Profile not found"
        )
    
    # Update only provided fields
    update_data = profile_data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(profile, field, value)
    
    # Recalculate BMR/TDEE if weight/height/age changed
    if any(k in update_data for k in ["weight_kg", "height_cm", "age"]):
        if profile.weight_kg and profile.height_cm and profile.age and profile.gender:
            profile.bmr = calculate_bmr(
                profile.weight_kg,
                profile.height_cm,
                profile.age,
                profile.gender.value
            )
            profile.tdee = calculate_tdee(profile.bmr)
    
    db.commit()
    db.refresh(profile)
    
    return profile
