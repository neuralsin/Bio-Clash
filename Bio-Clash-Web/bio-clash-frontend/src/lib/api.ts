/**
 * API Client Configuration
 * Axios instance with base URL and interceptors
 */
import axios from 'axios';

const API_BASE_URL = '/api/v1';

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor - Add auth token
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor - Handle errors
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            // Token expired or invalid
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

// API Functions

// Auth
export const authApi = {
    register: (data: { email: string; username: string; password: string }) =>
        api.post('/auth/register', data),
    login: (email: string, password: string) => {
        const formData = new FormData();
        formData.append('username', email); // OAuth2 uses 'username' field for email
        formData.append('password', password);
        return api.post('/auth/login', formData, {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        });
    },
};

// Profile
export const profileApi = {
    get: () => api.get('/profile/'),
    onboard: (data: any) => api.post('/profile/onboard', data),
    update: (data: any) => api.patch('/profile/', data),
};

// Fitness
export const fitnessApi = {
    getExercises: (muscleGroup?: string) =>
        api.get('/fitness/exercises', { params: { muscle_group: muscleGroup } }),
    getExercisesGrouped: () => api.get('/fitness/exercises/grouped'),
    logWorkout: (data: any) => api.post('/fitness/workout', data),
    getWorkouts: (limit = 10, offset = 0) =>
        api.get('/fitness/workouts', { params: { limit, offset } }),
    logBiometrics: (data: any) => api.post('/fitness/biometrics', data),
    getStats: () => api.get('/fitness/stats'),
};

// Game
export const gameApi = {
    getVillage: () => api.get('/game/village'),
    syncResources: () => api.post('/game/village/sync'),
    getUpgradeRequirements: (buildingId: string) =>
        api.get(`/game/building/${buildingId}/upgrade-requirements`),
    startUpgrade: (buildingId: string) =>
        api.post(`/game/building/${buildingId}/upgrade`),
    getActiveUpgrades: () => api.get('/game/upgrades'),
    searchRaid: () => api.get('/game/raid/search'),
    attack: (opponentId: string) => api.post('/game/raid/attack', { opponent_id: opponentId }),
    getRecoveryScore: () => api.get('/game/fairplay/recovery'),
    getLeagueInfo: () => api.get('/game/fairplay/league'),
};

// Clan
export const clanApi = {
    create: (data: { name: string; tag: string; description?: string; is_public?: boolean }) =>
        api.post('/clan/create', data),
    search: (name?: string, minMembers = 0) =>
        api.get('/clan/search', { params: { name, min_members: minMembers } }),
    getMyClan: () => api.get('/clan/my'),
    join: (clanId: string) => api.post('/clan/join', { clan_id: clanId }),
    leave: () => api.post('/clan/leave'),
    promoteMember: (memberId: string, newRole: string) =>
        api.post('/clan/promote', { member_id: memberId, new_role: newRole }),

    // War
    startWarSearch: () => api.post('/clan/war/search'),
    getCurrentWar: () => api.get('/clan/war/current'),
    warAttack: (warId: string, defenderId: string) =>
        api.post('/clan/war/attack', { war_id: warId, defender_id: defenderId }),

    // Chat
    getChatHistory: (limit = 50) => api.get('/clan/chat/history', { params: { limit } }),
};

// WebSocket helpers for real-time features
export const wsUrl = (path: string) => {
    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    return `${protocol}//${window.location.host}/api/v1${path}`;
};

export const createClanChatWs = (clanId: string) => new WebSocket(wsUrl(`/clan/chat/${clanId}`));
export const createRaidWs = (raidId: string, role: string = 'attacker') =>
    new WebSocket(wsUrl(`/clan/raid/${raidId}?role=${role}`));

export default api;

