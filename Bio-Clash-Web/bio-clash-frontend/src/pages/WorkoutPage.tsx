/**
 * Workout Page
 * Full page for logging workouts
 */
import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { WorkoutLogger } from '../components/WorkoutLogger';
import { useGameStore } from '../stores/gameStore';
import { gameApi } from '../lib/api';

export function WorkoutPage() {
    const navigate = useNavigate();
    const { setVillage } = useGameStore();

    const handleComplete = async () => {
        // Refresh village data after workout
        try {
            const response = await gameApi.getVillage();
            setVillage(response.data);
        } catch (error) {
            console.error('Failed to refresh village:', error);
        }
    };

    return (
        <div className="min-h-screen">
            {/* Header */}
            <header className="sticky top-0 z-50 glass">
                <div className="max-w-4xl mx-auto px-4 py-4">
                    <button
                        className="flex items-center gap-2 text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition"
                        onClick={() => navigate('/dashboard')}
                    >
                        <ArrowLeft className="w-5 h-5" />
                        Back to Village
                    </button>
                </div>
            </header>

            {/* Content */}
            <main className="max-w-4xl mx-auto px-4 py-6">
                <WorkoutLogger onComplete={handleComplete} />
            </main>
        </div>
    );
}
