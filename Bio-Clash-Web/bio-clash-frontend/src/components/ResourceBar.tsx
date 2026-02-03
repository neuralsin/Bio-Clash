/**
 * Resource Bar Component
 * Displays Gold, Elixir, Dark Elixir, and Gems
 */
import { motion } from 'framer-motion';
import { Coins, Droplet, Gem, Sparkles } from 'lucide-react';

interface ResourceBarProps {
    gold: number;
    goldCapacity: number;
    elixir: number;
    elixirCapacity: number;
    darkElixir: number;
    darkElixirCapacity: number;
    gems: number;
}

export function ResourceBar({
    gold,
    goldCapacity,
    elixir,
    elixirCapacity,
    darkElixir,
    darkElixirCapacity,
    gems,
}: ResourceBarProps) {
    const formatNumber = (num: number) => {
        if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`;
        if (num >= 1000) return `${(num / 1000).toFixed(1)}K`;
        return num.toString();
    };

    return (
        <div className="flex items-center gap-4 flex-wrap">
            {/* Gold */}
            <motion.div
                className="resource-bar"
                whileHover={{ scale: 1.05 }}
                transition={{ type: 'spring', stiffness: 300 }}
            >
                <div className="w-8 h-8 rounded-full gold-gradient flex items-center justify-center">
                    <Coins className="w-5 h-5 text-[#0D1117]" />
                </div>
                <div className="flex flex-col">
                    <span className="text-sm font-bold text-[var(--gold)]">
                        {formatNumber(gold)}
                    </span>
                    <div className="w-20 h-1 bg-[var(--bg-primary)] rounded-full overflow-hidden">
                        <div
                            className="h-full gold-gradient transition-all duration-300"
                            style={{ width: `${(gold / goldCapacity) * 100}%` }}
                        />
                    </div>
                </div>
            </motion.div>

            {/* Elixir */}
            <motion.div
                className="resource-bar"
                whileHover={{ scale: 1.05 }}
                transition={{ type: 'spring', stiffness: 300 }}
            >
                <div className="w-8 h-8 rounded-full elixir-gradient flex items-center justify-center">
                    <Droplet className="w-5 h-5 text-white" />
                </div>
                <div className="flex flex-col">
                    <span className="text-sm font-bold text-[var(--elixir)]">
                        {formatNumber(elixir)}
                    </span>
                    <div className="w-20 h-1 bg-[var(--bg-primary)] rounded-full overflow-hidden">
                        <div
                            className="h-full elixir-gradient transition-all duration-300"
                            style={{ width: `${(elixir / elixirCapacity) * 100}%` }}
                        />
                    </div>
                </div>
            </motion.div>

            {/* Dark Elixir */}
            <motion.div
                className="resource-bar"
                whileHover={{ scale: 1.05 }}
                transition={{ type: 'spring', stiffness: 300 }}
            >
                <div className="w-8 h-8 rounded-full dark-elixir-gradient flex items-center justify-center">
                    <Sparkles className="w-5 h-5 text-white" />
                </div>
                <div className="flex flex-col">
                    <span className="text-sm font-bold text-purple-400">
                        {formatNumber(darkElixir)}
                    </span>
                    <div className="w-20 h-1 bg-[var(--bg-primary)] rounded-full overflow-hidden">
                        <div
                            className="h-full dark-elixir-gradient transition-all duration-300"
                            style={{ width: `${(darkElixir / darkElixirCapacity) * 100}%` }}
                        />
                    </div>
                </div>
            </motion.div>

            {/* Gems */}
            <motion.div
                className="resource-bar"
                whileHover={{ scale: 1.05 }}
                transition={{ type: 'spring', stiffness: 300 }}
            >
                <div className="w-8 h-8 rounded-full bg-gradient-to-br from-emerald-400 to-emerald-600 flex items-center justify-center">
                    <Gem className="w-5 h-5 text-white" />
                </div>
                <span className="text-sm font-bold text-emerald-400">{gems}</span>
            </motion.div>
        </div>
    );
}
