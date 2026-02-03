/**
 * Dashboard Page
 * Main game view with village, resources, and status
 */
import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import {
    Swords, Trophy, Dumbbell, Heart,
    LogOut, RefreshCw, Crown, Users
} from 'lucide-react';
import { useNavigate, Link } from 'react-router-dom';

import { ResourceBar } from '../components/ResourceBar';
import { RecoveryStatus } from '../components/RecoveryStatus';
import { BuildingCard } from '../components/BuildingCard';
import { useAuthStore } from '../stores/authStore';
import { useGameStore } from '../stores/gameStore';
import { gameApi } from '../lib/api';

export function DashboardPage() {
    const navigate = useNavigate();
    const { user, logout, isAuthenticated } = useAuthStore();
    const { village, recovery, setVillage, setRecovery, setLoading, isLoading } = useGameStore();
    const [syncAnim, setSyncAnim] = useState(false);

    useEffect(() => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }
        loadData();

        // Sync resources every 30 seconds
        const interval = setInterval(() => syncResources(), 30000);
        return () => clearInterval(interval);
    }, [isAuthenticated]);

    const loadData = async () => {
        setLoading(true);
        try {
            const [villageRes, recoveryRes] = await Promise.all([
                gameApi.getVillage(),
                gameApi.getRecoveryScore(),
            ]);
            setVillage(villageRes.data);
            setRecovery(recoveryRes.data);
        } catch (error) {
            console.error('Failed to load data:', error);
        } finally {
            setLoading(false);
        }
    };

    const syncResources = async () => {
        setSyncAnim(true);
        try {
            const response = await gameApi.syncResources();
            if (village) {
                setVillage({
                    ...village,
                    gold: response.data.gold,
                    elixir: response.data.elixir,
                    dark_elixir: response.data.dark_elixir,
                });
            }
        } catch (error) {
            console.error('Failed to sync resources:', error);
        } finally {
            setTimeout(() => setSyncAnim(false), 1000);
        }
    };

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    if (isLoading || !village) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="text-center">
                    <RefreshCw className="w-12 h-12 animate-spin text-[var(--gold)] mx-auto mb-4" />
                    <p className="text-[var(--text-secondary)]">Loading your village...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen">
            {/* Top Bar */}
            <header className="sticky top-0 z-50 glass">
                <div className="max-w-7xl mx-auto px-4 py-3">
                    <div className="flex items-center justify-between">
                        {/* Logo & Username */}
                        <div className="flex items-center gap-4">
                            <div className="flex items-center gap-2">
                                <Crown className="w-6 h-6 text-[var(--gold)]" />
                                <span className="font-bold text-lg">{user?.username}</span>
                            </div>
                            <div className="px-3 py-1 rounded-full bg-[var(--bg-card)] text-sm capitalize">
                                {user?.league_tier || 'Bronze'} League
                            </div>
                        </div>

                        {/* Resources */}
                        <div className="hidden md:block">
                            <ResourceBar
                                gold={village.gold}
                                goldCapacity={village.gold_capacity}
                                elixir={village.elixir}
                                elixirCapacity={village.elixir_capacity}
                                darkElixir={village.dark_elixir}
                                darkElixirCapacity={village.dark_elixir_capacity}
                                gems={village.gems}
                            />
                        </div>

                        {/* Actions */}
                        <div className="flex items-center gap-2">
                            <button
                                className={`p-2 rounded-lg hover:bg-[var(--bg-card)] transition ${syncAnim ? 'animate-spin' : ''}`}
                                onClick={syncResources}
                                title="Sync Resources"
                            >
                                <RefreshCw className="w-5 h-5" />
                            </button>
                            <button
                                className="p-2 rounded-lg hover:bg-[var(--bg-card)] transition"
                                onClick={handleLogout}
                                title="Logout"
                            >
                                <LogOut className="w-5 h-5" />
                            </button>
                        </div>
                    </div>

                    {/* Mobile Resources */}
                    <div className="md:hidden mt-3">
                        <ResourceBar
                            gold={village.gold}
                            goldCapacity={village.gold_capacity}
                            elixir={village.elixir}
                            elixirCapacity={village.elixir_capacity}
                            darkElixir={village.dark_elixir}
                            darkElixirCapacity={village.dark_elixir_capacity}
                            gems={village.gems}
                        />
                    </div>
                </div>
            </header>

            {/* Main Content */}
            <main className="max-w-7xl mx-auto px-4 py-6">
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Left Column - Quick Actions */}
                    <div className="space-y-6">
                        {/* Recovery Status */}
                        {recovery && (
                            <RecoveryStatus
                                recoveryPercent={recovery.recovery_percent}
                                status={recovery.status}
                                shieldActive={recovery.shield_active}
                                recommendations={recovery.recommendations}
                            />
                        )}

                        {/* Quick Actions */}
                        <div className="card">
                            <h3 className="text-lg font-semibold mb-4">Quick Actions</h3>
                            <div className="space-y-3">
                                <Link to="/workout">
                                    <motion.button
                                        className="w-full p-4 rounded-lg bg-gradient-to-r from-green-600 to-emerald-500 text-white font-semibold flex items-center justify-center gap-2"
                                        whileHover={{ scale: 1.02 }}
                                        whileTap={{ scale: 0.98 }}
                                    >
                                        <Dumbbell className="w-5 h-5" />
                                        Log Workout
                                    </motion.button>
                                </Link>

                                <motion.button
                                    className="w-full p-4 rounded-lg bg-gradient-to-r from-red-600 to-orange-500 text-white font-semibold flex items-center justify-center gap-2"
                                    whileHover={{ scale: 1.02 }}
                                    whileTap={{ scale: 0.98 }}
                                    disabled={recovery?.shield_active}
                                >
                                    <Swords className="w-5 h-5" />
                                    {recovery?.shield_active ? 'Shield Active' : 'Find Raid'}
                                </motion.button>

                                <motion.button
                                    className="w-full p-4 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] font-semibold flex items-center justify-center gap-2"
                                    whileHover={{ scale: 1.02 }}
                                    whileTap={{ scale: 0.98 }}
                                >
                                    <Heart className="w-5 h-5 text-pink-400" />
                                    Log Biometrics
                                </motion.button>

                                <Link to="/clan">
                                    <motion.button
                                        className="w-full p-4 rounded-lg bg-gradient-to-r from-purple-600 to-indigo-500 text-white font-semibold flex items-center justify-center gap-2"
                                        whileHover={{ scale: 1.02 }}
                                        whileTap={{ scale: 0.98 }}
                                    >
                                        <Users className="w-5 h-5" />
                                        My Legion
                                    </motion.button>
                                </Link>
                            </div>
                        </div>

                        {/* League Info */}
                        <div className="card">
                            <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                                <Trophy className="w-5 h-5 text-[var(--gold)]" />
                                League Standing
                            </h3>
                            <div className="text-center py-4">
                                <div className="text-4xl font-bold text-[var(--gold)] mb-2">
                                    #{1}
                                </div>
                                <p className="text-sm text-[var(--text-secondary)]">
                                    in {user?.league_tier?.charAt(0).toUpperCase()}{user?.league_tier?.slice(1)} League
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Center & Right - Buildings */}
                    <div className="lg:col-span-2">
                        <div className="card mb-6">
                            <div className="flex items-center justify-between mb-4">
                                <h3 className="text-lg font-semibold">Your Village</h3>
                                <span className="text-sm text-[var(--text-secondary)]">
                                    Town Hall Lv. {village.town_hall_level}
                                </span>
                            </div>

                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                {village.buildings.map((building) => (
                                    <BuildingCard
                                        key={building.id}
                                        building={building}
                                        onClick={() => {
                                            // Navigate to building details
                                        }}
                                    />
                                ))}
                            </div>
                        </div>

                        {/* Resource Generation Stats */}
                        <div className="card">
                            <h3 className="text-lg font-semibold mb-4">Resource Generation</h3>
                            <div className="grid grid-cols-3 gap-4">
                                <div className="text-center">
                                    <div className="text-2xl font-bold text-[var(--gold)]">
                                        +{village.gold_per_hour.toFixed(0)}/hr
                                    </div>
                                    <div className="text-sm text-[var(--text-secondary)]">Gold</div>
                                </div>
                                <div className="text-center">
                                    <div className="text-2xl font-bold text-[var(--elixir)]">
                                        +{village.elixir_per_hour.toFixed(0)}/hr
                                    </div>
                                    <div className="text-sm text-[var(--text-secondary)]">Elixir</div>
                                </div>
                                <div className="text-center">
                                    <div className="text-2xl font-bold text-purple-400">
                                        +{(village.dark_elixir_per_hour || 0).toFixed(0)}/hr
                                    </div>
                                    <div className="text-sm text-[var(--text-secondary)]">Dark Elixir</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}
