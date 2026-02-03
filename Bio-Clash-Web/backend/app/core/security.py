"""
Security Module
Password hashing and JWT token management.
HACKATHON DEMO: Using simple base64 encoding instead of bcrypt for demo purposes.
"""
from datetime import datetime, timedelta
from typing import Optional
import base64
from jose import JWTError, jwt

from app.core.config import settings


def verify_password(plain_password: str, hashed_password: str) -> bool:
    """Verify a password against its hash. DEMO: Simple base64 comparison."""
    try:
        encoded = base64.b64encode(plain_password.encode()).decode()
        return encoded == hashed_password
    except:
        return False


def get_password_hash(password: str) -> str:
    """Hash a password. DEMO: Simple base64 encoding."""
    return base64.b64encode(password.encode()).decode()


def create_access_token(data: dict, expires_delta: Optional[timedelta] = None) -> str:
    """Create a JWT access token."""
    to_encode = data.copy()
    if expires_delta:
        expire = datetime.utcnow() + expires_delta
    else:
        expire = datetime.utcnow() + timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES)
    to_encode.update({"exp": expire})
    encoded_jwt = jwt.encode(to_encode, settings.SECRET_KEY, algorithm=settings.ALGORITHM)
    return encoded_jwt


def decode_access_token(token: str) -> Optional[str]:
    """Decode a JWT token and return user_id."""
    try:
        payload = jwt.decode(token, settings.SECRET_KEY, algorithms=[settings.ALGORITHM])
        user_id: str = payload.get("sub")
        return user_id
    except JWTError:
        return None
