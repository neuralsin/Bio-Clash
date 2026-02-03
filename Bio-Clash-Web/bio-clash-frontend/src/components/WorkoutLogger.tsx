/**
 * Workout Logger Component
 * Main interface for logging workouts with sets and reps
 */
import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import {
    Dumbbell, Plus, Minus, Check, X, ChevronDown,
    Timer, Flame, Trophy
} from 'lucide-react';
import { fitnessApi } from '../lib/api';

interface Exercise {
    id: string;
    name: string;
    primary_muscle: string;
    category: string;
    equipment_needed: string;
}

interface WorkoutSet {
    exercise_id: string;
    exercise_name: string;
    set_number: number;
    reps: number;
    weight_kg: number;
    rpe?: number;
}

interface WorkoutLoggerProps {
    onComplete?: (summary: any) => void;
}

export function WorkoutLogger({ onComplete }: WorkoutLoggerProps) {
    const [exercises, setExercises] = useState<Record<string, Exercise[]>>({});
    const [sets, setSets] = useState<WorkoutSet[]>([]);
    const [selectedMuscle, setSelectedMuscle] = useState<string>('');
    const [selectedExercise, setSelectedExercise] = useState<Exercise | null>(null);
    const [currentReps, setCurrentReps] = useState(10);
    const [currentWeight, setCurrentWeight] = useState(20);
    const [isLoading, setIsLoading] = useState(false);
    const [showSuccess, setShowSuccess] = useState(false);
    const [workoutSummary, setWorkoutSummary] = useState<any>(null);

    // Load exercises on mount
    useEffect(() => {
        loadExercises();
    }, []);

    const loadExercises = async () => {
        try {
            const response = await fitnessApi.getExercisesGrouped();
            const grouped: Record<string, Exercise[]> = {};
            response.data.forEach((group: any) => {
                grouped[group.muscle_group] = group.exercises;
            });
            setExercises(grouped);
        } catch (error) {
            console.error('Failed to load exercises:', error);
        }
    };

    const addSet = () => {
        if (!selectedExercise) return;

        const newSet: WorkoutSet = {
            exercise_id: selectedExercise.id,
            exercise_name: selectedExercise.name,
            set_number: sets.filter(s => s.exercise_id === selectedExercise.id).length + 1,
            reps: currentReps,
            weight_kg: currentWeight,
        };

        setSets([...sets, newSet]);
    };

    const removeSet = (index: number) => {
        setSets(sets.filter((_, i) => i !== index));
    };

    const finishWorkout = async () => {
        if (sets.length === 0) return;

        setIsLoading(true);
        try {
            const workoutData = {
                sets: sets.map((s, idx) => ({
                    exercise_id: s.exercise_id,
                    set_number: idx + 1,
                    reps: s.reps,
                    weight_kg: s.weight_kg,
                })),
            };

            const response = await fitnessApi.logWorkout(workoutData);
            setWorkoutSummary(response.data);
            setShowSuccess(true);
            setSets([]);

            if (onComplete) {
                onComplete(response.data);
            }
        } catch (error) {
            console.error('Failed to log workout:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const muscles = Object.keys(exercises);
    const muscleExercises = selectedMuscle ? exercises[selectedMuscle] || [] : [];
    const totalVolume = sets.reduce((acc, s) => acc + s.reps * s.weight_kg, 0);

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <h2 className="text-2xl font-bold flex items-center gap-2">
                    <Dumbbell className="w-6 h-6 text-[var(--gold)]" />
                    Log Workout
                </h2>
                {sets.length > 0 && (
                    <div className="flex items-center gap-4">
                        <span className="text-sm text-[var(--text-secondary)]">
                            {sets.length} sets • {totalVolume.toFixed(0)}kg volume
                        </span>
                        <button
                            className="btn-gold"
                            onClick={finishWorkout}
                            disabled={isLoading}
                        >
                            <Check className="w-4 h-4 mr-2 inline" />
                            Finish Workout
                        </button>
                    </div>
                )}
            </div>

            {/* Exercise Selector */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Muscle Group */}
                <div className="card">
                    <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                        Muscle Group
                    </label>
                    <div className="relative">
                        <select
                            className="w-full p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] text-[var(--text-primary)] appearance-none cursor-pointer"
                            value={selectedMuscle}
                            onChange={(e) => {
                                setSelectedMuscle(e.target.value);
                                setSelectedExercise(null);
                            }}
                        >
                            <option value="">Select muscle group</option>
                            {muscles.map((muscle) => (
                                <option key={muscle} value={muscle}>
                                    {muscle.charAt(0).toUpperCase() + muscle.slice(1)}
                                </option>
                            ))}
                        </select>
                        <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-[var(--text-secondary)] pointer-events-none" />
                    </div>
                </div>

                {/* Exercise */}
                <div className="card">
                    <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                        Exercise
                    </label>
                    <div className="relative">
                        <select
                            className="w-full p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] text-[var(--text-primary)] appearance-none cursor-pointer"
                            value={selectedExercise?.id || ''}
                            onChange={(e) => {
                                const ex = muscleExercises.find((ex) => ex.id === e.target.value);
                                setSelectedExercise(ex || null);
                            }}
                            disabled={!selectedMuscle}
                        >
                            <option value="">Select exercise</option>
                            {muscleExercises.map((ex) => (
                                <option key={ex.id} value={ex.id}>
                                    {ex.name}
                                </option>
                            ))}
                        </select>
                        <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-[var(--text-secondary)] pointer-events-none" />
                    </div>
                </div>
            </div>

            {/* Set Input */}
            {selectedExercise && (
                <motion.div
                    className="card"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                >
                    <h3 className="text-lg font-semibold mb-4">{selectedExercise.name}</h3>

                    <div className="grid grid-cols-2 gap-6">
                        {/* Weight */}
                        <div>
                            <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                                Weight (kg)
                            </label>
                            <div className="flex items-center gap-3">
                                <button
                                    className="w-10 h-10 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] flex items-center justify-center hover:bg-[var(--border)] transition"
                                    onClick={() => setCurrentWeight(Math.max(0, currentWeight - 2.5))}
                                >
                                    <Minus className="w-4 h-4" />
                                </button>
                                <input
                                    type="number"
                                    className="flex-1 p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] text-center text-xl font-bold"
                                    value={currentWeight}
                                    onChange={(e) => setCurrentWeight(Number(e.target.value))}
                                />
                                <button
                                    className="w-10 h-10 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] flex items-center justify-center hover:bg-[var(--border)] transition"
                                    onClick={() => setCurrentWeight(currentWeight + 2.5)}
                                >
                                    <Plus className="w-4 h-4" />
                                </button>
                            </div>
                        </div>

                        {/* Reps */}
                        <div>
                            <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                                Reps
                            </label>
                            <div className="flex items-center gap-3">
                                <button
                                    className="w-10 h-10 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] flex items-center justify-center hover:bg-[var(--border)] transition"
                                    onClick={() => setCurrentReps(Math.max(1, currentReps - 1))}
                                >
                                    <Minus className="w-4 h-4" />
                                </button>
                                <input
                                    type="number"
                                    className="flex-1 p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] text-center text-xl font-bold"
                                    value={currentReps}
                                    onChange={(e) => setCurrentReps(Number(e.target.value))}
                                />
                                <button
                                    className="w-10 h-10 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] flex items-center justify-center hover:bg-[var(--border)] transition"
                                    onClick={() => setCurrentReps(currentReps + 1)}
                                >
                                    <Plus className="w-4 h-4" />
                                </button>
                            </div>
                        </div>
                    </div>

                    <button
                        className="w-full mt-4 btn-primary"
                        onClick={addSet}
                    >
                        <Plus className="w-4 h-4 mr-2 inline" />
                        Add Set ({currentWeight}kg × {currentReps} = {(currentWeight * currentReps).toFixed(0)}kg)
                    </button>
                </motion.div>
            )}

            {/* Current Sets */}
            <AnimatePresence>
                {sets.length > 0 && (
                    <motion.div
                        className="card"
                        initial={{ opacity: 0, height: 0 }}
                        animate={{ opacity: 1, height: 'auto' }}
                        exit={{ opacity: 0, height: 0 }}
                    >
                        <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                            <Timer className="w-5 h-5 text-blue-400" />
                            Current Session
                        </h3>
                        <div className="space-y-2">
                            {sets.map((set, idx) => (
                                <motion.div
                                    key={idx}
                                    className="flex items-center justify-between p-3 rounded-lg bg-[var(--bg-secondary)]"
                                    initial={{ opacity: 0, x: -20 }}
                                    animate={{ opacity: 1, x: 0 }}
                                    exit={{ opacity: 0, x: 20 }}
                                >
                                    <div className="flex items-center gap-3">
                                        <span className="w-6 h-6 rounded-full bg-[var(--gold)] text-[#0D1117] text-sm font-bold flex items-center justify-center">
                                            {idx + 1}
                                        </span>
                                        <span className="font-medium">{set.exercise_name}</span>
                                    </div>
                                    <div className="flex items-center gap-4">
                                        <span className="text-[var(--text-secondary)]">
                                            {set.weight_kg}kg × {set.reps}
                                        </span>
                                        <span className="text-[var(--gold)] font-semibold">
                                            {(set.weight_kg * set.reps).toFixed(0)}kg
                                        </span>
                                        <button
                                            className="text-red-400 hover:text-red-300 transition"
                                            onClick={() => removeSet(idx)}
                                        >
                                            <X className="w-4 h-4" />
                                        </button>
                                    </div>
                                </motion.div>
                            ))}
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>

            {/* Success Modal */}
            <AnimatePresence>
                {showSuccess && workoutSummary && (
                    <motion.div
                        className="fixed inset-0 bg-black/70 flex items-center justify-center z-50"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        onClick={() => setShowSuccess(false)}
                    >
                        <motion.div
                            className="card max-w-md w-full mx-4"
                            initial={{ scale: 0.8, opacity: 0 }}
                            animate={{ scale: 1, opacity: 1 }}
                            exit={{ scale: 0.8, opacity: 0 }}
                            onClick={(e) => e.stopPropagation()}
                        >
                            <div className="text-center">
                                <div className="w-16 h-16 rounded-full bg-green-500/20 flex items-center justify-center mx-auto mb-4">
                                    <Trophy className="w-8 h-8 text-green-400" />
                                </div>
                                <h3 className="text-2xl font-bold mb-2">Workout Complete!</h3>
                                <p className="text-[var(--text-secondary)] mb-6">
                                    You've earned resources for your base!
                                </p>

                                <div className="grid grid-cols-2 gap-4 mb-6">
                                    <div className="p-4 rounded-lg bg-[var(--bg-secondary)]">
                                        <div className="text-2xl font-bold text-[var(--gold)]">
                                            +{workoutSummary.gold_earned}
                                        </div>
                                        <div className="text-sm text-[var(--text-secondary)]">Gold</div>
                                    </div>
                                    <div className="p-4 rounded-lg bg-[var(--bg-secondary)]">
                                        <div className="text-2xl font-bold text-[var(--elixir)]">
                                            +{workoutSummary.elixir_earned}
                                        </div>
                                        <div className="text-sm text-[var(--text-secondary)]">Elixir</div>
                                    </div>
                                </div>

                                <div className="text-left p-4 rounded-lg bg-[var(--bg-secondary)] mb-6">
                                    <div className="text-sm text-[var(--text-secondary)] mb-2">Total Volume</div>
                                    <div className="text-xl font-bold flex items-center gap-2">
                                        <Flame className="w-5 h-5 text-orange-400" />
                                        {workoutSummary.total_volume_kg.toFixed(0)} kg
                                    </div>
                                </div>

                                <button
                                    className="w-full btn-primary"
                                    onClick={() => setShowSuccess(false)}
                                >
                                    Awesome!
                                </button>
                            </div>
                        </motion.div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}
