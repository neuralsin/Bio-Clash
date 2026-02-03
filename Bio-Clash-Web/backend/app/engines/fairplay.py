"""
FairPlay Engines
1. Fatigue Oracle: Predicts recovery score, triggers shields
2. League Clustering: Groups users for fair matchmaking
"""
from typing import List, Tuple
from datetime import date, timedelta
import numpy as np
from sqlalchemy.orm import Session
from sqlalchemy import func

from app.core.config import settings
from app.core.enums import LeagueTier
from app.models.user import User
from app.models.fitness import DailyBiometrics, WorkoutLog


class FatigueOracle:
    """
    System 1: The Fatigue Oracle
    
    Uses linear combination of sleep, HRV, and training load
    to predict recovery percentage.
    
    Formula:
    RecoveryScore = (0.5 × Sleep_normalized) + (0.3 × HRV_normalized) - (0.2 × Load_normalized)
    
    If score < 30%: Shield activated (forced rest)
    If score > 80%: Builder boost (faster upgrades)
    """
    
    SLEEP_OPTIMAL_HOURS = 8.0
    HRV_BASELINE = 50.0  # Average HRV in ms
    LOAD_MAX_WEEKLY = 50000.0  # Max weekly volume considered "overtraining"
    
    def __init__(self, db: Session, user_id: str):
        self.db = db
        self.user_id = user_id
    
    def calculate_recovery_score(self) -> Tuple[float, str, List[str]]:
        """
        Calculate the user's recovery percentage.
        
        Returns:
            Tuple of (recovery_percent, status, recommendations)
        """
        # Get last 3 days of biometrics
        three_days_ago = date.today() - timedelta(days=3)
        biometrics = self.db.query(DailyBiometrics).filter(
            DailyBiometrics.user_id == self.user_id,
            DailyBiometrics.date >= three_days_ago
        ).order_by(DailyBiometrics.date.desc()).all()
        
        # Get last 7 days of training load
        seven_days_ago = date.today() - timedelta(days=7)
        weekly_volume = self.db.query(func.sum(WorkoutLog.total_volume_kg)).filter(
            WorkoutLog.user_id == self.user_id,
            WorkoutLog.date >= seven_days_ago
        ).scalar() or 0.0
        
        # Calculate normalized factors
        avg_sleep = 7.0  # Default
        avg_hrv = 50.0   # Default
        
        if biometrics:
            sleep_values = [b.sleep_hours for b in biometrics if b.sleep_hours]
            hrv_values = [b.hrv for b in biometrics if b.hrv]
            
            if sleep_values:
                avg_sleep = sum(sleep_values) / len(sleep_values)
            if hrv_values:
                avg_hrv = sum(hrv_values) / len(hrv_values)
        
        # Normalize to 0-1 scale
        sleep_normalized = min(1.0, avg_sleep / self.SLEEP_OPTIMAL_HOURS)
        hrv_normalized = min(1.0, avg_hrv / (self.HRV_BASELINE * 1.5))  # 75ms is excellent
        load_normalized = min(1.0, weekly_volume / self.LOAD_MAX_WEEKLY)
        
        # Apply formula
        recovery_score = (
            (0.5 * sleep_normalized) +
            (0.3 * hrv_normalized) -
            (0.2 * load_normalized)
        ) * 100
        
        # Clamp to 0-100
        recovery_score = max(0, min(100, recovery_score))
        
        # Determine status
        if recovery_score < settings.FATIGUE_SHIELD_THRESHOLD:
            status = "critical"
        elif recovery_score < 50:
            status = "fatigued"
        elif recovery_score < settings.FATIGUE_BOOST_THRESHOLD:
            status = "recovering"
        else:
            status = "optimal"
        
        # Generate recommendations
        recommendations = self._generate_recommendations(
            avg_sleep, avg_hrv, weekly_volume, recovery_score
        )
        
        return recovery_score, status, recommendations
    
    def _generate_recommendations(
        self, avg_sleep: float, avg_hrv: float, 
        weekly_volume: float, score: float
    ) -> List[str]:
        """Generate personalized recovery recommendations."""
        recs = []
        
        if avg_sleep < 6:
            recs.append("⚠️ Sleep deficit detected. Aim for 7-9 hours tonight.")
        elif avg_sleep < 7:
            recs.append("💤 Try to get an extra hour of sleep for optimal recovery.")
        
        if avg_hrv < 40:
            recs.append("❤️ Low HRV indicates stress. Consider a light day or active recovery.")
        
        if weekly_volume > 40000:
            recs.append("🏋️ High training load this week. Consider a deload.")
        
        if score < 30:
            recs.append("🛡️ SHIELD ACTIVATED: Your body needs rest. Take a rest day.")
        elif score > 80:
            recs.append("🚀 You're well recovered! Great time for an intense session.")
        
        if not recs:
            recs.append("✅ You're in good shape. Keep up the balanced approach!")
        
        return recs
    
    def should_activate_shield(self) -> bool:
        """Check if shield should be activated due to low recovery."""
        score, _, _ = self.calculate_recovery_score()
        return score < settings.FATIGUE_SHIELD_THRESHOLD


class LeagueClustering:
    """
    System 2: Smart Combat Power Scaling
    
    Uses K-Means clustering to group users into fair leagues
    based on biological output, not just level.
    
    Features:
    - Average weekly volume
    - Consistency score
    - Experience level
    
    Leagues: Bronze, Silver, Gold, Crystal, Titan
    """
    
    def __init__(self, db: Session):
        self.db = db
    
    def calculate_user_features(self, user_id: str) -> np.ndarray:
        """Extract features for a single user."""
        user = self.db.query(User).filter(User.id == user_id).first()
        if not user:
            return np.array([0, 0, 0])
        
        # Get weekly volume
        seven_days_ago = date.today() - timedelta(days=7)
        weekly_volume = self.db.query(func.sum(WorkoutLog.total_volume_kg)).filter(
            WorkoutLog.user_id == user_id,
            WorkoutLog.date >= seven_days_ago
        ).scalar() or 0.0
        
        # Normalize features
        volume_normalized = min(1.0, weekly_volume / 30000)  # 30k is high volume
        consistency_normalized = user.consistency_score / 100
        
        # Experience from profile
        from app.models.user import Profile
        profile = self.db.query(Profile).filter(Profile.user_id == user_id).first()
        exp_map = {"beginner": 0.2, "intermediate": 0.5, "advanced": 1.0}
        exp_normalized = exp_map.get(profile.experience_level.value if profile else "beginner", 0.2)
        
        return np.array([volume_normalized, consistency_normalized, exp_normalized])
    
    def assign_league(self, user_id: str) -> LeagueTier:
        """
        Assign a user to a league based on their features.
        
        For MVP: Simple threshold-based assignment.
        Production: Would run K-Means on all users periodically.
        """
        features = self.calculate_user_features(user_id)
        score = np.sum(features) / 3  # Simple average
        
        if score < 0.2:
            return LeagueTier.BRONZE
        elif score < 0.4:
            return LeagueTier.SILVER
        elif score < 0.6:
            return LeagueTier.GOLD
        elif score < 0.8:
            return LeagueTier.CRYSTAL
        else:
            return LeagueTier.TITAN
    
    def find_opponents(self, user_id: str, count: int = 5) -> List[str]:
        """
        Find suitable opponents in the same league.
        
        Returns list of user IDs.
        """
        user = self.db.query(User).filter(User.id == user_id).first()
        if not user:
            return []
        
        # Find users in same league, excluding self
        opponents = self.db.query(User).filter(
            User.league_tier == user.league_tier,
            User.id != user_id
        ).limit(count).all()
        
        return [o.id for o in opponents]
