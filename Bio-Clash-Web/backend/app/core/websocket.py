"""
WebSocket Manager
Handles real-time connections for raids, chat, and war updates.
"""
from typing import Dict, List, Set
from fastapi import WebSocket
from datetime import datetime
import json


class ConnectionManager:
    """
    Manages WebSocket connections for real-time features.
    
    Connection Types:
    - Clan Chat: One room per clan
    - War Battle: One room per active war
    - Raid: Direct 1:1 for raid attacks
    """
    
    def __init__(self):
        # Map of room_id -> set of WebSocket connections
        self.clan_rooms: Dict[str, List[WebSocket]] = {}
        self.war_rooms: Dict[str, List[WebSocket]] = {}
        self.raid_sessions: Dict[str, Dict[str, WebSocket]] = {}  # raid_id -> {attacker, defender}
        
        # Map user_id -> WebSocket for direct messaging
        self.user_connections: Dict[str, WebSocket] = {}
    
    async def connect_user(self, websocket: WebSocket, user_id: str):
        """Connect a user for general notifications."""
        await websocket.accept()
        self.user_connections[user_id] = websocket
    
    def disconnect_user(self, user_id: str):
        """Disconnect a user."""
        if user_id in self.user_connections:
            del self.user_connections[user_id]
    
    # ============================================================
    # CLAN CHAT
    # ============================================================
    
    async def join_clan_room(self, websocket: WebSocket, clan_id: str, user_id: str):
        """Join clan chat room."""
        await websocket.accept()
        
        if clan_id not in self.clan_rooms:
            self.clan_rooms[clan_id] = []
        
        self.clan_rooms[clan_id].append(websocket)
        
        # Notify others
        await self.broadcast_to_clan(clan_id, {
            "type": "user_joined",
            "user_id": user_id,
            "timestamp": datetime.utcnow().isoformat()
        }, exclude=websocket)
    
    def leave_clan_room(self, websocket: WebSocket, clan_id: str):
        """Leave clan chat room."""
        if clan_id in self.clan_rooms:
            if websocket in self.clan_rooms[clan_id]:
                self.clan_rooms[clan_id].remove(websocket)
            if not self.clan_rooms[clan_id]:
                del self.clan_rooms[clan_id]
    
    async def broadcast_to_clan(
        self, clan_id: str, message: dict, exclude: WebSocket = None
    ):
        """Broadcast message to all clan members."""
        if clan_id not in self.clan_rooms:
            return
        
        for connection in self.clan_rooms[clan_id]:
            if connection != exclude:
                try:
                    await connection.send_json(message)
                except:
                    pass  # Connection might be closed
    
    # ============================================================
    # WAR ROOM
    # ============================================================
    
    async def join_war_room(self, websocket: WebSocket, war_id: str, user_id: str):
        """Join war battle room for live updates."""
        await websocket.accept()
        
        if war_id not in self.war_rooms:
            self.war_rooms[war_id] = []
        
        self.war_rooms[war_id].append(websocket)
    
    def leave_war_room(self, websocket: WebSocket, war_id: str):
        """Leave war room."""
        if war_id in self.war_rooms:
            if websocket in self.war_rooms[war_id]:
                self.war_rooms[war_id].remove(websocket)
            if not self.war_rooms[war_id]:
                del self.war_rooms[war_id]
    
    async def broadcast_war_update(self, war_id: str, update: dict):
        """Broadcast war update to all participants."""
        if war_id not in self.war_rooms:
            return
        
        for connection in self.war_rooms[war_id]:
            try:
                await connection.send_json(update)
            except:
                pass
    
    # ============================================================
    # REAL-TIME RAIDS
    # ============================================================
    
    async def start_raid_session(
        self, raid_id: str, 
        attacker_ws: WebSocket, 
        defender_ws: WebSocket = None
    ):
        """
        Start a real-time raid session.
        Attacker sees their attack; defender sees their base being attacked.
        """
        await attacker_ws.accept()
        
        self.raid_sessions[raid_id] = {
            "attacker": attacker_ws,
            "defender": defender_ws,
            "started_at": datetime.utcnow().isoformat()
        }
        
        # Notify both parties
        await attacker_ws.send_json({
            "type": "raid_started",
            "raid_id": raid_id,
            "role": "attacker"
        })
        
        if defender_ws:
            await defender_ws.send_json({
                "type": "raid_started", 
                "raid_id": raid_id,
                "role": "defender"
            })
    
    async def send_raid_event(self, raid_id: str, event: dict, to: str = "both"):
        """
        Send a raid event to attacker, defender, or both.
        
        Events:
        - troop_deployed: {x, y, troop_type}
        - building_attacked: {building_id, damage}
        - building_destroyed: {building_id}
        - spell_used: {x, y, spell_type}
        - raid_completed: {stars, destruction, loot}
        """
        if raid_id not in self.raid_sessions:
            return
        
        session = self.raid_sessions[raid_id]
        event["timestamp"] = datetime.utcnow().isoformat()
        
        if to in ["attacker", "both"] and session.get("attacker"):
            try:
                await session["attacker"].send_json(event)
            except:
                pass
        
        if to in ["defender", "both"] and session.get("defender"):
            try:
                await session["defender"].send_json(event)
            except:
                pass
    
    async def end_raid_session(self, raid_id: str, result: dict):
        """End a raid session and send final result."""
        if raid_id not in self.raid_sessions:
            return
        
        session = self.raid_sessions[raid_id]
        
        final_message = {
            "type": "raid_ended",
            "raid_id": raid_id,
            **result
        }
        
        if session.get("attacker"):
            try:
                await session["attacker"].send_json(final_message)
                await session["attacker"].close()
            except:
                pass
        
        if session.get("defender"):
            try:
                await session["defender"].send_json(final_message)
                await session["defender"].close()
            except:
                pass
        
        del self.raid_sessions[raid_id]
    
    # ============================================================
    # UTILITY
    # ============================================================
    
    async def send_to_user(self, user_id: str, message: dict):
        """Send a message directly to a user."""
        if user_id in self.user_connections:
            try:
                await self.user_connections[user_id].send_json(message)
            except:
                pass
    
    def get_online_users(self) -> Set[str]:
        """Get set of online user IDs."""
        return set(self.user_connections.keys())


# Global connection manager instance
manager = ConnectionManager()
