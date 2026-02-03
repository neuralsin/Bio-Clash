"""
Bio-Clash API
Main FastAPI Application Entry Point

The Gamified Fitness Platform where Your Body Builds Your Base.
"""
import json
from pathlib import Path
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.core.config import settings
from app.db.session import Base, engine, SessionLocal
from app.api.api_v1.api import api_router


@asynccontextmanager
async def lifespan(app: FastAPI):
    """
    Startup and shutdown events.
    Creates database tables and seeds exercise data.
    """
    # Startup
    print("🚀 Starting Bio-Clash API...")
    
    # Create all tables
    Base.metadata.create_all(bind=engine)
    print("✅ Database tables created")
    
    # Seed exercise data if empty
    seed_exercises()
    
    yield
    
    # Shutdown
    print("👋 Shutting down Bio-Clash API...")


def seed_exercises():
    """Seed the exercise library from JSON if empty."""
    from app.models.fitness import Exercise
    from app.core.enums import MuscleGroup, ExerciseCategory
    
    db = SessionLocal()
    try:
        # Check if exercises exist
        count = db.query(Exercise).count()
        if count > 0:
            print(f"📚 Exercise library already seeded ({count} exercises)")
            return
        
        # Load from JSON
        exercises_file = Path(__file__).parent / "data" / "exercises.json"
        if not exercises_file.exists():
            print("⚠️ exercises.json not found, skipping seed")
            return
        
        with open(exercises_file, "r") as f:
            exercises_data = json.load(f)
        
        for ex_data in exercises_data:
            exercise = Exercise(
                name=ex_data["name"],
                primary_muscle=MuscleGroup(ex_data["primary_muscle"]),
                secondary_muscles=ex_data.get("secondary_muscles"),
                category=ExerciseCategory(ex_data["category"]),
                equipment_needed=ex_data.get("equipment_needed"),
                difficulty=ex_data.get("difficulty", 1),
                description=ex_data.get("description")
            )
            db.add(exercise)
        
        db.commit()
        print(f"✅ Seeded {len(exercises_data)} exercises")
    
    except Exception as e:
        print(f"❌ Error seeding exercises: {e}")
        db.rollback()
    finally:
        db.close()


# Create FastAPI app
app = FastAPI(
    title=settings.APP_NAME,
    version=settings.APP_VERSION,
    description="""
    ## Bio-Clash API
    
    The Gamified Fitness Platform where **Your Body Builds Your Base**.
    
    ### Features:
    - 🏋️ **Fitness Tracking**: Log workouts, sets, reps with muscle-group mapping
    - 🏰 **Game Logic**: Building upgrades tied to biological metrics
    - ⚖️ **FairPlay Engine**: Fatigue Oracle + K-Means matchmaking
    - ⚔️ **Raids**: PvP battles based on fitness stats
    
    ### THE CODEX (Body-to-Building Mapping):
    | Building | Muscle Group |
    |----------|--------------|
    | Archer Tower | Chest |
    | Cannon | Back |
    | Mortar | Triceps |
    | Wizard Tower | Shoulders |
    | Inferno Tower | Legs |
    | Walls | Core |
    """,
    lifespan=lifespan
)

# CORS Middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Configure for production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Include API router
app.include_router(api_router, prefix="/api/v1")


@app.get("/", tags=["Health"])
async def root():
    """Health check endpoint."""
    return {
        "app": settings.APP_NAME,
        "version": settings.APP_VERSION,
        "status": "healthy",
        "message": "Your Logic Builds Your Body. Your Body Builds Your Base."
    }


@app.get("/health", tags=["Health"])
async def health_check():
    """Detailed health check."""
    return {
        "status": "healthy",
        "database": "connected",
        "fairplay_engine": "active"
    }
