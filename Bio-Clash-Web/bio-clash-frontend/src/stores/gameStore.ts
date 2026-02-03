/**
 * Game Store
 * Zustand store for game state (village, resources, buildings)
 */
import { create } from 'zustand';

interface Building {
    id: string;
    building_type: string;
    level: number;
    position_x: number;
    position_y: number;
    health: number;
    max_health: number;
    damage_per_second: number;
    is_upgrading: boolean;
    is_damaged: boolean;
}

interface Village {
    id: string;
    town_hall_level: number;
    gold: number;
    gold_capacity: number;
    elixir: number;
    elixir_capacity: number;
    dark_elixir: number;
    dark_elixir_capacity: number;
    gems: number;
    gold_per_hour: number;
    elixir_per_hour: number;
    dark_elixir_per_hour: number;
    shield_active: boolean;
    shield_end_time: string | null;
    buildings: Building[];
}

interface RecoveryStatus {
    recovery_percent: number;
    status: string;
    shield_active: boolean;
    recommendations: string[];
}

interface GameState {
    village: Village | null;
    recovery: RecoveryStatus | null;
    selectedBuilding: Building | null;
    isLoading: boolean;

    // Actions
    setVillage: (village: Village) => void;
    setRecovery: (recovery: RecoveryStatus) => void;
    selectBuilding: (building: Building | null) => void;
    updateResources: (gold?: number, elixir?: number, darkElixir?: number) => void;
    setLoading: (loading: boolean) => void;
}

export const useGameStore = create<GameState>((set) => ({
    village: null,
    recovery: null,
    selectedBuilding: null,
    isLoading: false,

    setVillage: (village) => set({ village }),

    setRecovery: (recovery) => set({ recovery }),

    selectBuilding: (building) => set({ selectedBuilding: building }),

    updateResources: (gold, elixir, darkElixir) => {
        set((state) => ({
            village: state.village
                ? {
                    ...state.village,
                    gold: gold ?? state.village.gold,
                    elixir: elixir ?? state.village.elixir,
                    dark_elixir: darkElixir ?? state.village.dark_elixir,
                }
                : null,
        }));
    },

    setLoading: (loading) => set({ isLoading: loading }),
}));
