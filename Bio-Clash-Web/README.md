# 🏰 Bio-Clash: Where Your Body Builds Your Base

**The Gamified Fitness Platform**  
*Your Logic Builds Your Body. Your Body Builds Your Base.*

---

## 🎯 What is Bio-Clash?

Bio-Clash is a revolutionary fitness platform that transforms your workout data into a **Clash of Clans-style** strategy game. Every rep builds your empire. Every muscle group powers a specific building.

### THE CODEX: Body-to-Building Mapping

| Building | Muscle Group | Strategic Function |
|----------|-------------|-------------------|
| 🏹 Archer Tower | **Chest** | Long-range defense |
| 🔫 Cannon | **Back** | Primary damage dealer |
| 💣 Mortar | **Triceps** | Splash damage |
| 🔮 Wizard Tower | **Shoulders** | Magic defense |
| 🔥 Inferno Tower | **Legs** | High-tier defense |
| ⚡ Hidden Tesla | **Biceps** | Surprise defense |
| 🎯 X-Bow | **Cardio** | Long-range attacker |
| 🦅 Eagle Artillery | **Compounds** | Ultimate weapon |
| 🧱 Walls | **Core** | Base protection |
| 🏛️ Town Hall | **Consistency** | Determines max levels |

---

## 🚀 Quick Start

### Prerequisites
- Python 3.10+
- Node.js 18+
- npm or yarn

### Backend Setup

```bash
cd Bio-Clash-Web/backend

# Create virtual environment
python -m venv venv
venv\Scripts\activate  # Windows
# source venv/bin/activate  # Linux/Mac

# Install dependencies
pip install -r requirements.txt

# Run the API
uvicorn app.main:app --reload --port 8000
```

### Frontend Setup

```bash
cd Bio-Clash-Web/bio-clash-frontend

# Install dependencies
npm install

# Run dev server
npm run dev
```

The app will be available at:
- Frontend: http://localhost:3000
- Backend API: http://localhost:8000
- API Docs: http://localhost:8000/docs

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                        FRONTEND (React)                          │
│  ┌─────────┐ ┌──────────┐ ┌───────────┐ ┌─────────────────────┐  │
│  │ Login   │ │ Register │ │ Dashboard │ │   Workout Logger    │  │
│  └────┬────┘ └────┬─────┘ └─────┬─────┘ └──────────┬──────────┘  │
│       └───────────┴─────────────┴──────────────────┘             │
│                              │                                    │
│                     ┌────────┴────────┐                          │
│                     │   Zustand Store │                          │
│                     │  (Auth + Game)  │                          │
│                     └────────┬────────┘                          │
└──────────────────────────────┼───────────────────────────────────┘
                               │ HTTP/REST
┌──────────────────────────────┼───────────────────────────────────┐
│                        BACKEND (FastAPI)                         │
│                              │                                    │
│  ┌───────────────────────────┴───────────────────────────────┐   │
│  │                      API Layer v1                          │   │
│  │  ┌──────┐ ┌─────────┐ ┌─────────┐ ┌────────────────────┐  │   │
│  │  │ Auth │ │ Profile │ │ Fitness │ │        Game        │  │   │
│  │  └──────┘ └─────────┘ └─────────┘ └────────────────────┘  │   │
│  └───────────────────────────┬───────────────────────────────┘   │
│                              │                                    │
│  ┌───────────────────────────┴───────────────────────────────┐   │
│  │                    Engine Layer                            │   │
│  │  ┌─────────────────────┐  ┌─────────────────────────────┐ │   │
│  │  │   Fatigue Oracle    │  │     League Clustering       │ │   │
│  │  │ (Recovery Scoring)  │  │       (K-Means)             │ │   │
│  │  └─────────────────────┘  └─────────────────────────────┘ │   │
│  │  ┌─────────────────────┐  ┌─────────────────────────────┐ │   │
│  │  │  Resource Manager   │  │      Upgrade Manager        │ │   │
│  │  │ (Gold/Elixir Sync)  │  │   (THE CODEX Rules)         │ │   │
│  │  └─────────────────────┘  └─────────────────────────────┘ │   │
│  └───────────────────────────────────────────────────────────┘   │
│                              │                                    │
│  ┌───────────────────────────┴───────────────────────────────┐   │
│  │                   Data Layer (SQLite)                      │   │
│  │  ┌──────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐           │   │
│  │  │ User │ │ Profile │ │ Workouts │ │ Village  │           │   │
│  │  └──────┘ └─────────┘ └──────────┘ └──────────┘           │   │
│  └───────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
```

---

## 🧠 Core Systems

### 1. FairPlay Engine

**Fatigue Oracle** - Predicts recovery score based on:
- Sleep quality (last 3 days)
- Heart Rate Variability (HRV)
- Training load (weekly volume)

```python
RecoveryScore = (0.5 × Sleep) + (0.3 × HRV) - (0.2 × Load)
```

If `RecoveryScore < 30%`:
- 🛡️ Shield Auto-Activates
- ⛔ Cannot start raids
- ⛔ Cannot start upgrades

**League Clustering** - Groups users for fair matchmaking:
- Uses K-Means clustering on biological output
- Leagues: Bronze → Silver → Gold → Crystal → Titan

### 2. Resource Economy

| Resource | Source | Use |
|----------|--------|-----|
| 🪙 Gold | Workout volume, steps | Building upgrades |
| 💧 Elixir | Sleep quality, recovery | Building upgrades |
| ⚫ Dark Elixir | PRs, intensity | Hero summoning |
| 💎 Gems | Achievements | Skip timers |

### 3. Building Upgrade System

Each building requires:
1. **Resource cost** (Gold/Elixir)
2. **Fitness requirement** (X kg volume in specific muscle)

Example:
```
Archer Tower Lv. 2 → Lv. 3
├── Cost: 5,000 Gold
├── Requires: 500kg Chest volume
└── Time: 3 hours
```

---

## 📱 Demo Flow

### For Judges / Demo Day

1. **Register** - Create account, starts with basic village
2. **Log Workout** - Add exercises by muscle group, sets/reps
3. **See Rewards** - Gold + Elixir earned based on volume
4. **Check Village** - Buildings show which muscles power them
5. **Check Recovery** - Fatigue Oracle shows recovery status
6. **Attempt Upgrade** - See fitness requirements for buildings
7. **Search Raid** - Find opponent in same league

---

## 🔌 API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Create account
- `POST /api/v1/auth/login` - Get JWT token

### Profile
- `POST /api/v1/profile/onboard` - Complete onboarding
- `GET /api/v1/profile/` - Get profile
- `PATCH /api/v1/profile/` - Update profile

### Fitness
- `GET /api/v1/fitness/exercises` - List all exercises
- `GET /api/v1/fitness/exercises/grouped` - Exercises by muscle
- `POST /api/v1/fitness/workout` - Log a workout
- `GET /api/v1/fitness/workouts` - Workout history
- `POST /api/v1/fitness/biometrics` - Log sleep/HRV
- `GET /api/v1/fitness/stats` - Aggregated stats

### Game
- `GET /api/v1/game/village` - Get village + buildings
- `POST /api/v1/game/village/sync` - Sync resources
- `GET /api/v1/game/building/{id}/upgrade-requirements` - Check requirements
- `POST /api/v1/game/building/{id}/upgrade` - Start upgrade
- `GET /api/v1/game/raid/search` - Find opponent
- `POST /api/v1/game/raid/attack` - Execute raid
- `GET /api/v1/game/fairplay/recovery` - Get recovery score
- `GET /api/v1/game/fairplay/league` - Get league info

---

## 🛠️ Tech Stack

### Backend
- **FastAPI** - Modern Python web framework
- **SQLAlchemy** - ORM
- **SQLite** - Database (MVP)
- **Scikit-learn** - K-Means clustering
- **NumPy/Pandas** - Data processing
- **JWT** - Authentication

### Frontend
- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool
- **TailwindCSS 4** - Styling
- **Zustand** - State management
- **Framer Motion** - Animations
- **Lucide React** - Icons
- **React Router 6** - Routing

---

## 📁 Project Structure

```
Bio-Clash-Web/
├── backend/
│   ├── app/
│   │   ├── api/
│   │   │   └── api_v1/
│   │   │       ├── api.py
│   │   │       └── endpoints/
│   │   │           ├── auth.py
│   │   │           ├── profile.py
│   │   │           ├── fitness.py
│   │   │           └── game.py
│   │   ├── core/
│   │   │   ├── config.py
│   │   │   ├── enums.py
│   │   │   ├── security.py
│   │   │   └── deps.py
│   │   ├── db/
│   │   │   └── session.py
│   │   ├── models/
│   │   │   ├── user.py
│   │   │   ├── fitness.py
│   │   │   └── game.py
│   │   ├── schemas/
│   │   │   ├── user.py
│   │   │   ├── fitness.py
│   │   │   └── game.py
│   │   ├── engines/
│   │   │   ├── fairplay.py
│   │   │   └── game.py
│   │   ├── data/
│   │   │   └── exercises.json
│   │   └── main.py
│   └── requirements.txt
│
└── bio-clash-frontend/
    ├── src/
    │   ├── components/
    │   │   ├── ResourceBar.tsx
    │   │   ├── RecoveryStatus.tsx
    │   │   ├── BuildingCard.tsx
    │   │   └── WorkoutLogger.tsx
    │   ├── pages/
    │   │   ├── LoginPage.tsx
    │   │   ├── RegisterPage.tsx
    │   │   ├── DashboardPage.tsx
    │   │   └── WorkoutPage.tsx
    │   ├── stores/
    │   │   ├── authStore.ts
    │   │   └── gameStore.ts
    │   ├── lib/
    │   │   └── api.ts
    │   ├── App.tsx
    │   ├── main.tsx
    │   └── index.css
    ├── package.json
    └── vite.config.ts
```

---

## 🏆 Judge Talking Points

1. **Innovation**: First fitness app that maps specific muscle groups to game mechanics
2. **Fair Play**: Recovery-aware system prevents burnout and grinding abuse
3. **ML Integration**: K-Means clustering for fair matchmaking, regression for fatigue prediction
4. **Full Stack**: Complete FastAPI + React implementation
5. **Game Design**: Authentic Clash of Clans-inspired economy and progression
6. **Scalability**: Built on proven technologies (FastAPI, SQLAlchemy, Zustand)

---

## 🚀 Future Roadmap

- [ ] Real health API integration (Apple Health, Google Fit)
- [ ] Anti-cheat with accelerometer pattern analysis
- [ ] Clan/Legion system for team competitions
- [ ] Real-time multiplayer raids
- [ ] Mobile app (React Native)
- [ ] Wearable integration (Whoop, Garmin)

---

## 📄 License

MIT License - Built for hackathon purposes.

---

**Built with 💪 and ☕ in 24 hours**

*Your Body. Your Base. Your Victory.*
