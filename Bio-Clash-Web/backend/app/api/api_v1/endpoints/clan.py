"""
Clan API Endpoints
Clan management, wars, and chat.
"""
from typing import List
from datetime import datetime
from fastapi import APIRouter, Depends, HTTPException, status, WebSocket, WebSocketDisconnect
from sqlalchemy.orm import Session

from app.db.session import get_db
from app.core.deps import get_current_active_user
from app.core.websocket import manager
from app.core.enums import ClanRole, WarState
from app.models.user import User
from app.models.clan import Clan, ClanMember, ClanWar, WarAttack, ClanMessage
from app.engines.clan import ClanManager, ClanWarEngine
from app.schemas.clan import (
    ClanCreate, ClanResponse, ClanDetailResponse, ClanMemberResponse,
    JoinClanRequest, PromoteMemberRequest,
    WarSearchResponse, ClanWarResponse, WarDetailResponse, 
    WarAttackRequest, WarAttackResponse,
    ClanMessageCreate, ClanMessageResponse
)

router = APIRouter(prefix="/clan", tags=["Clan"])


# ============================================================
# CLAN MANAGEMENT
# ============================================================

@router.post("/create", response_model=ClanResponse)
async def create_clan(
    clan_data: ClanCreate,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Create a new clan. The creator becomes the Leader.
    """
    try:
        manager = ClanManager(db)
        clan = manager.create_clan(
            user_id=current_user.id,
            name=clan_data.name,
            tag=clan_data.tag,
            description=clan_data.description,
            badge_icon=clan_data.badge_icon,
            badge_color=clan_data.badge_color,
            is_public=clan_data.is_public,
            min_trophies_required=clan_data.min_trophies_required
        )
        
        clan.member_count = 1
        return clan
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@router.get("/search", response_model=List[ClanResponse])
async def search_clans(
    name: str = "",
    min_members: int = 0,
    db: Session = Depends(get_db)
):
    """
    Search for public clans.
    """
    query = db.query(Clan).filter(Clan.is_public == True)
    
    if name:
        query = query.filter(Clan.name.ilike(f"%{name}%"))
    
    clans = query.limit(20).all()
    
    # Add member counts
    result = []
    for clan in clans:
        member_count = db.query(ClanMember).filter(ClanMember.clan_id == clan.id).count()
        if member_count >= min_members:
            clan.member_count = member_count
            result.append(clan)
    
    return result


@router.get("/my", response_model=ClanDetailResponse)
async def get_my_clan(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get current user's clan with all members.
    """
    membership = db.query(ClanMember).filter(ClanMember.user_id == current_user.id).first()
    
    if not membership:
        raise HTTPException(status_code=404, detail="You are not in a clan")
    
    clan = db.query(Clan).filter(Clan.id == membership.clan_id).first()
    
    # Get all members with user info
    members = db.query(ClanMember).filter(ClanMember.clan_id == clan.id).all()
    
    member_responses = []
    for m in members:
        user = db.query(User).filter(User.id == m.user_id).first()
        if user:
            member_responses.append(ClanMemberResponse(
                id=m.id,
                user_id=m.user_id,
                username=user.username,
                role=m.role,
                donations=m.donations,
                war_stars_earned=m.war_stars_earned,
                attacks_won=m.attacks_won,
                league_tier=user.league_tier,
                joined_at=m.joined_at
            ))
    
    clan.member_count = len(members)
    clan.members = member_responses
    
    return clan


@router.post("/join")
async def join_clan(
    request: JoinClanRequest,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Join a public clan.
    """
    try:
        manager = ClanManager(db)
        manager.join_clan(current_user.id, request.clan_id)
        return {"message": "Successfully joined the clan"}
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@router.post("/leave")
async def leave_clan(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Leave current clan.
    """
    try:
        manager = ClanManager(db)
        manager.leave_clan(current_user.id)
        return {"message": "Left the clan"}
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@router.post("/promote")
async def promote_member(
    request: PromoteMemberRequest,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Promote or demote a clan member (Leaders/Co-Leaders only).
    """
    try:
        manager = ClanManager(db)
        manager.promote_member(current_user.id, request.member_id, request.new_role)
        return {"message": f"Member role updated to {request.new_role.value}"}
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


# ============================================================
# CLAN WARS
# ============================================================

@router.post("/war/search", response_model=WarSearchResponse)
async def start_war_search(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Start searching for a clan war opponent.
    Only Leader/Co-Leader can start wars.
    """
    membership = db.query(ClanMember).filter(ClanMember.user_id == current_user.id).first()
    
    if not membership:
        raise HTTPException(status_code=400, detail="You are not in a clan")
    
    if membership.role not in [ClanRole.LEADER, ClanRole.CO_LEADER]:
        raise HTTPException(status_code=403, detail="Only Leaders can start wars")
    
    try:
        engine = ClanWarEngine(db)
        war = engine.start_war_search(membership.clan_id)
        
        if war.opponent_clan_id:
            return WarSearchResponse(
                message="War found! Preparation phase starting.",
                war_id=war.id,
                state=war.state
            )
        else:
            return WarSearchResponse(
                message="Searching for opponent...",
                war_id=war.id,
                state=WarState.MATCHMAKING
            )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@router.get("/war/current", response_model=ClanWarResponse)
async def get_current_war(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get current active war for user's clan.
    """
    membership = db.query(ClanMember).filter(ClanMember.user_id == current_user.id).first()
    
    if not membership:
        raise HTTPException(status_code=404, detail="You are not in a clan")
    
    war = db.query(ClanWar).filter(
        (ClanWar.clan_id == membership.clan_id) | (ClanWar.opponent_clan_id == membership.clan_id),
        ClanWar.state.in_([WarState.MATCHMAKING, WarState.PREPARATION, WarState.BATTLE])
    ).first()
    
    if not war:
        raise HTTPException(status_code=404, detail="No active war")
    
    # Get clan names
    clan = db.query(Clan).filter(Clan.id == war.clan_id).first()
    opponent = db.query(Clan).filter(Clan.id == war.opponent_clan_id).first() if war.opponent_clan_id else None
    
    # Calculate time remaining
    now = datetime.utcnow()
    time_remaining = None
    if war.state == WarState.PREPARATION and war.battle_start:
        time_remaining = int((war.battle_start - now).total_seconds())
    elif war.state == WarState.BATTLE and war.battle_end:
        time_remaining = int((war.battle_end - now).total_seconds())
    
    return ClanWarResponse(
        id=war.id,
        clan_name=clan.name if clan else "Unknown",
        opponent_clan_name=opponent.name if opponent else "Searching...",
        state=war.state,
        clan_stars=war.clan_stars,
        opponent_stars=war.opponent_stars,
        clan_destruction=war.clan_destruction,
        opponent_destruction=war.opponent_destruction,
        battle_start=war.battle_start,
        battle_end=war.battle_end,
        time_remaining_seconds=max(0, time_remaining) if time_remaining else None
    )


@router.post("/war/attack", response_model=WarAttackResponse)
async def execute_war_attack(
    request: WarAttackRequest,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Execute an attack in the current clan war.
    """
    try:
        engine = ClanWarEngine(db)
        attack = engine.execute_war_attack(
            war_id=request.war_id,
            attacker_id=current_user.id,
            defender_id=request.defender_id
        )
        
        # Get usernames
        attacker = db.query(User).filter(User.id == attack.attacker_id).first()
        defender = db.query(User).filter(User.id == attack.defender_id).first()
        
        return WarAttackResponse(
            attacker_username=attacker.username if attacker else "Unknown",
            defender_username=defender.username if defender else "Unknown",
            stars=attack.stars,
            destruction_percent=attack.destruction_percent,
            attack_power_used=attack.attack_power_used,
            defense_power_faced=attack.defense_power_faced,
            attack_time=attack.attack_time
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


# ============================================================
# CLAN CHAT (WebSocket)
# ============================================================

@router.websocket("/chat/{clan_id}")
async def clan_chat_websocket(
    websocket: WebSocket,
    clan_id: str,
    db: Session = Depends(get_db)
):
    """
    WebSocket endpoint for real-time clan chat.
    """
    # In production, would validate token from query param
    # For now, accept connection
    
    await manager.join_clan_room(websocket, clan_id, "anonymous")
    
    try:
        while True:
            data = await websocket.receive_json()
            
            # Handle different message types
            if data.get("type") == "chat":
                # Save to database
                message = ClanMessage(
                    clan_id=clan_id,
                    user_id=data.get("user_id", "anonymous"),
                    message=data.get("message", ""),
                    message_type="chat"
                )
                db.add(message)
                db.commit()
                
                # Broadcast to clan
                await manager.broadcast_to_clan(clan_id, {
                    "type": "chat",
                    "user_id": data.get("user_id"),
                    "username": data.get("username"),
                    "message": data.get("message"),
                    "timestamp": datetime.utcnow().isoformat()
                })
    
    except WebSocketDisconnect:
        manager.leave_clan_room(websocket, clan_id)


@router.get("/chat/history", response_model=List[ClanMessageResponse])
async def get_chat_history(
    limit: int = 50,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_active_user)
):
    """
    Get recent clan chat messages.
    """
    membership = db.query(ClanMember).filter(ClanMember.user_id == current_user.id).first()
    
    if not membership:
        raise HTTPException(status_code=404, detail="You are not in a clan")
    
    messages = db.query(ClanMessage).filter(
        ClanMessage.clan_id == membership.clan_id
    ).order_by(ClanMessage.created_at.desc()).limit(limit).all()
    
    result = []
    for msg in reversed(messages):  # Oldest first
        user = db.query(User).filter(User.id == msg.user_id).first()
        result.append(ClanMessageResponse(
            id=msg.id,
            user_id=msg.user_id,
            username=user.username if user else "Unknown",
            message=msg.message,
            message_type=msg.message_type,
            created_at=msg.created_at
        ))
    
    return result


# ============================================================
# REAL-TIME RAID (WebSocket)
# ============================================================

@router.websocket("/raid/{raid_id}")
async def raid_websocket(
    websocket: WebSocket,
    raid_id: str,
    role: str = "attacker"  # attacker or defender
):
    """
    WebSocket endpoint for real-time raid battles.
    
    Events:
    - troop_deployed: Attacker deploys a troop
    - building_hit: Building takes damage
    - spell_cast: Spell effect
    - raid_complete: Final result
    """
    await websocket.accept()
    
    # For MVP, just echo events
    try:
        while True:
            data = await websocket.receive_json()
            
            # Process raid events
            event_type = data.get("type")
            
            if event_type == "troop_deployed":
                # Calculate damage based on biological power
                await manager.send_raid_event(raid_id, {
                    "type": "troop_deployed",
                    "troop": data.get("troop"),
                    "x": data.get("x"),
                    "y": data.get("y")
                })
            
            elif event_type == "attack_building":
                await manager.send_raid_event(raid_id, {
                    "type": "building_hit",
                    "building_id": data.get("building_id"),
                    "damage": data.get("damage", 10)
                })
            
            elif event_type == "end_raid":
                await manager.end_raid_session(raid_id, {
                    "stars": data.get("stars", 0),
                    "destruction": data.get("destruction", 0),
                    "loot": data.get("loot", {})
                })
                break
    
    except WebSocketDisconnect:
        pass
