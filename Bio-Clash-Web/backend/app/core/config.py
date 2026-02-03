import os
from dotenv import load_dotenv

load_dotenv()

class Settings:
    """Application configuration settings."""
    
    # App Info
    APP_NAME: str = "Bio-Clash"
    APP_VERSION: str = "0.1.0"
    DEBUG: bool = os.getenv("DEBUG", "True").lower() == "true"
    
    # Database
    DATABASE_URL: str = os.getenv("DATABASE_URL", "sqlite:///./bioclash.db")
    
    # Security
    SECRET_KEY: str = os.getenv("SECRET_KEY", "bio-clash-super-secret-key-change-in-production")
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 60 * 24 * 7  # 7 days
    
    # FairPlay Engine Thresholds
    FATIGUE_SHIELD_THRESHOLD: int = 30  # Recovery % below this triggers forced shield
    FATIGUE_BOOST_THRESHOLD: int = 80   # Recovery % above this gives builder boost
    
    # Game Constants
    BUILDER_COUNT_DEFAULT: int = 2
    SHIELD_DURATION_HOURS: int = 8
    RESOURCE_SYNC_INTERVAL_SECONDS: int = 60
    
    # Clustering (Leagues)
    NUM_LEAGUES: int = 5  # Bronze, Silver, Gold, Crystal, Titan

settings = Settings()
