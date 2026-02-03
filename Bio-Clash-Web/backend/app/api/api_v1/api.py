"""
API v1 Router
Aggregates all endpoint routers.
"""
from fastapi import APIRouter

from app.api.api_v1.endpoints import auth, profile, fitness, game, clan

api_router = APIRouter()

# Include all endpoint routers
api_router.include_router(auth.router)
api_router.include_router(profile.router)
api_router.include_router(fitness.router)
api_router.include_router(game.router)
api_router.include_router(clan.router)
