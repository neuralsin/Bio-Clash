/**
 * Clan Store (Zustand)
 * Manages clan state, war, and chat.
 */
import { create } from 'zustand';

interface ClanMember {
    id: string;
    user_id: string;
    username: string;
    role: string;
    donations: number;
    war_stars_earned: number;
    attacks_won: number;
    league_tier: string;
    joined_at: string;
}

interface Clan {
    id: string;
    name: string;
    tag: string;
    description: string;
    badge_icon: string;
    badge_color: string;
    level: number;
    total_trophies: number;
    war_wins: number;
    war_streak: number;
    member_count: number;
    max_members: number;
    is_public: boolean;
    total_attack_power: number;
    total_defense_power: number;
    members?: ClanMember[];
}

interface ClanWar {
    id: string;
    clan_name: string;
    opponent_clan_name: string;
    state: string;
    clan_stars: number;
    opponent_stars: number;
    clan_destruction: number;
    opponent_destruction: number;
    battle_start: string | null;
    battle_end: string | null;
    time_remaining_seconds: number | null;
}

interface ChatMessage {
    id: string;
    user_id: string;
    username: string;
    message: string;
    message_type: string;
    created_at: string;
}

interface ClanState {
    clan: Clan | null;
    war: ClanWar | null;
    messages: ChatMessage[];
    isLoading: boolean;

    setClan: (clan: Clan | null) => void;
    setWar: (war: ClanWar | null) => void;
    addMessage: (message: ChatMessage) => void;
    setMessages: (messages: ChatMessage[]) => void;
    setLoading: (loading: boolean) => void;
    clearClan: () => void;
}

export const useClanStore = create<ClanState>((set) => ({
    clan: null,
    war: null,
    messages: [],
    isLoading: false,

    setClan: (clan) => set({ clan }),
    setWar: (war) => set({ war }),
    addMessage: (message) => set((state) => ({
        messages: [...state.messages, message]
    })),
    setMessages: (messages) => set({ messages }),
    setLoading: (isLoading) => set({ isLoading }),
    clearClan: () => set({ clan: null, war: null, messages: [] }),
}));
