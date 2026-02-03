/**
 * Building Card Component
 * Displays a single building with its stats and upgrade status
 */
import { motion } from 'framer-motion';
import {
    Castle, Building2, Target, Sword, Flame, Zap,
    Shield, Crosshair, Rocket, Wind, Pickaxe, Droplets
} from 'lucide-react';

interface BuildingCardProps {
    building: {
        id: string;
        building_type: string;
        level: number;
        health: number;
        max_health: number;
        is_upgrading: boolean;
        is_damaged: boolean;
    };
    onClick?: () => void;
}

const BUILDING_INFO: Record<string, { icon: any; color: string; muscle: string }> = {
    town_hall: { icon: Castle, color: 'text-yellow-400', muscle: 'Consistency' },
    archer_tower: { icon: Target, color: 'text-pink-400', muscle: 'Chest' },
    cannon: { icon: Sword, color: 'text-blue-400', muscle: 'Back' },
    mortar: { icon: Rocket, color: 'text-orange-400', muscle: 'Triceps' },
    wizard_tower: { icon: Flame, color: 'text-purple-400', muscle: 'Shoulders' },
    inferno_tower: { icon: Flame, color: 'text-red-500', muscle: 'Legs' },
    hidden_tesla: { icon: Zap, color: 'text-yellow-300', muscle: 'Biceps' },
    x_bow: { icon: Crosshair, color: 'text-cyan-400', muscle: 'Cardio' },
    eagle_artillery: { icon: Rocket, color: 'text-indigo-400', muscle: 'Compounds' },
    walls: { icon: Shield, color: 'text-gray-400', muscle: 'Core' },
    air_defense: { icon: Wind, color: 'text-teal-400', muscle: 'Traps' },
    gold_mine: { icon: Pickaxe, color: 'text-yellow-500', muscle: 'Activity' },
    elixir_collector: { icon: Droplets, color: 'text-pink-500', muscle: 'Recovery' },
};

export function BuildingCard({ building, onClick }: BuildingCardProps) {
    const info = BUILDING_INFO[building.building_type] || {
        icon: Building2,
        color: 'text-gray-400',
        muscle: 'Unknown',
    };

    const Icon = info.icon;
    const healthPercent = (building.health / building.max_health) * 100;

    return (
        <motion.div
            className="card card-hover cursor-pointer relative overflow-hidden"
            onClick={onClick}
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
        >
            {/* Upgrading indicator */}
            {building.is_upgrading && (
                <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-blue-500 via-purple-500 to-blue-500 animate-pulse" />
            )}

            {/* Damaged indicator */}
            {building.is_damaged && (
                <div className="absolute top-2 right-2 px-2 py-1 rounded bg-red-500/20 text-red-400 text-xs font-medium">
                    Damaged
                </div>
            )}

            <div className="flex items-center gap-4">
                {/* Icon */}
                <div className={`w-12 h-12 rounded-lg bg-[var(--bg-secondary)] flex items-center justify-center ${info.color}`}>
                    <Icon className="w-6 h-6" />
                </div>

                {/* Info */}
                <div className="flex-1">
                    <div className="flex items-center justify-between">
                        <h4 className="font-semibold capitalize">
                            {building.building_type.replace(/_/g, ' ')}
                        </h4>
                        <span className="text-sm font-bold text-[var(--gold)]">
                            Lv. {building.level}
                        </span>
                    </div>
                    <p className="text-xs text-[var(--text-secondary)]">
                        Powered by: <span className={info.color}>{info.muscle}</span>
                    </p>

                    {/* Health bar */}
                    <div className="mt-2 h-1.5 bg-[var(--bg-primary)] rounded-full overflow-hidden">
                        <div
                            className={`h-full rounded-full transition-all duration-300 ${healthPercent > 60
                                    ? 'bg-green-500'
                                    : healthPercent > 30
                                        ? 'bg-yellow-500'
                                        : 'bg-red-500'
                                }`}
                            style={{ width: `${healthPercent}%` }}
                        />
                    </div>
                </div>
            </div>
        </motion.div>
    );
}
