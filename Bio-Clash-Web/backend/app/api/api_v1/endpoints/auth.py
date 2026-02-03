"""
Auth API Endpoints
Registration, Login, Token management.
"""
from datetime import timedelta
from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordRequestForm
from sqlalchemy.orm import Session

from app.db.session import get_db
from app.core.security import verify_password, get_password_hash, create_access_token
from app.core.config import settings
from app.models.user import User, Profile
from app.models.game import Village, Building
from app.core.enums import BuildingType
from app.schemas.user import UserCreate, UserResponse, Token

router = APIRouter(prefix="/auth", tags=["Authentication"])


@router.post("/register", response_model=UserResponse, status_code=status.HTTP_201_CREATED)
async def register(user_data: UserCreate, db: Session = Depends(get_db)):
    """
    Register a new user.
    Creates User, empty Profile, and initial Village with starter buildings.
    """
    # Check if email exists
    if db.query(User).filter(User.email == user_data.email).first():
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Email already registered"
        )
    
    # Check if username exists
    if db.query(User).filter(User.username == user_data.username).first():
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Username already taken"
        )
    
    # Create user
    hashed_password = get_password_hash(user_data.password)
    new_user = User(
        email=user_data.email,
        username=user_data.username,
        hashed_password=hashed_password
    )
    db.add(new_user)
    db.flush()  # Get the user.id
    
    # Create empty profile
    new_profile = Profile(user_id=new_user.id)
    db.add(new_profile)
    
    # Create initial village with starter buildings
    new_village = Village(user_id=new_user.id)
    db.add(new_village)
    db.flush()
    
    # Add starter buildings (THE CODEX: Level 1 of each)
    starter_buildings = [
        Building(village_id=new_village.id, building_type=BuildingType.TOWN_HALL, level=1, position_x=5, position_y=5),
        Building(village_id=new_village.id, building_type=BuildingType.CANNON, level=1, position_x=3, position_y=3),
        Building(village_id=new_village.id, building_type=BuildingType.ARCHER_TOWER, level=1, position_x=7, position_y=3),
        Building(village_id=new_village.id, building_type=BuildingType.WALLS, level=1, position_x=4, position_y=4),
        Building(village_id=new_village.id, building_type=BuildingType.GOLD_MINE, level=1, position_x=2, position_y=6),
        Building(village_id=new_village.id, building_type=BuildingType.ELIXIR_COLLECTOR, level=1, position_x=8, position_y=6),
    ]
    db.add_all(starter_buildings)
    
    db.commit()
    db.refresh(new_user)
    
    return new_user


@router.post("/login", response_model=Token)
async def login(
    form_data: OAuth2PasswordRequestForm = Depends(),
    db: Session = Depends(get_db)
):
    """
    Login and receive JWT token.
    Uses OAuth2 password flow (username field contains email).
    """
    user = db.query(User).filter(User.email == form_data.username).first()
    
    if not user or not verify_password(form_data.password, user.hashed_password):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect email or password",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    # Update last login
    from datetime import datetime
    user.last_login = datetime.utcnow()
    db.commit()
    
    # Create token
    access_token_expires = timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES)
    access_token = create_access_token(
        data={"sub": user.id},
        expires_delta=access_token_expires
    )
    
    return {"access_token": access_token, "token_type": "bearer"}


@router.get("/me", response_model=UserResponse)
async def get_me(
    current_user: User = Depends(get_db)  # Will fix with proper dep
):
    """Get current user info."""
    # This will be properly implemented with deps
    pass
