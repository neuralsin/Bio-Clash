/**
 * Register Page
 */
import { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, Link } from 'react-router-dom';
import { Sword, Mail, Lock, User, Loader2, Check } from 'lucide-react';
import { authApi } from '../lib/api';
import { useAuthStore } from '../stores/authStore';

export function RegisterPage() {
    const [email, setEmail] = useState('');
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();
    const login = useAuthStore((state) => state.login);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (password !== confirmPassword) {
            setError('Passwords do not match');
            return;
        }

        if (password.length < 6) {
            setError('Password must be at least 6 characters');
            return;
        }

        setIsLoading(true);

        try {
            // Register
            await authApi.register({ email, username, password });

            // Auto-login
            const loginResponse = await authApi.login(email, password);
            login(loginResponse.data.access_token, {
                id: '',
                email,
                username,
                consistency_score: 0,
                recovery_score: 0,
                league_tier: 'bronze'
            });

            navigate('/onboarding');
        } catch (err: any) {
            setError(err.response?.data?.detail || 'Registration failed');
        } finally {
            setIsLoading(false);
        }
    };

    const passwordStrength = () => {
        if (password.length === 0) return 0;
        if (password.length < 6) return 1;
        if (password.length < 10) return 2;
        return 3;
    };

    return (
        <div className="min-h-screen flex items-center justify-center p-4">
            <motion.div
                className="w-full max-w-md"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
            >
                {/* Logo */}
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-20 h-20 rounded-full gold-gradient mb-4 animate-pulse-gold">
                        <Sword className="w-10 h-10 text-[#0D1117]" />
                    </div>
                    <h1 className="text-3xl font-bold">Bio-Clash</h1>
                    <p className="text-[var(--text-secondary)] mt-2">
                        Build Your Empire with Your Body
                    </p>
                </div>

                {/* Form */}
                <div className="card">
                    <h2 className="text-xl font-semibold mb-6 text-center">Create Your Village</h2>

                    {error && (
                        <div className="mb-4 p-3 rounded-lg bg-red-500/10 border border-red-500/50 text-red-400 text-sm">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                                Chief Name
                            </label>
                            <div className="relative">
                                <User className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-[var(--text-secondary)]" />
                                <input
                                    type="text"
                                    value={username}
                                    onChange={(e) => setUsername(e.target.value)}
                                    className="w-full pl-10 pr-4 py-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] focus:border-[var(--gold)] focus:outline-none transition"
                                    placeholder="ChiefDestroyer"
                                    required
                                    minLength={3}
                                />
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                                Email
                            </label>
                            <div className="relative">
                                <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-[var(--text-secondary)]" />
                                <input
                                    type="email"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    className="w-full pl-10 pr-4 py-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] focus:border-[var(--gold)] focus:outline-none transition"
                                    placeholder="chief@bioclash.com"
                                    required
                                />
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                                Password
                            </label>
                            <div className="relative">
                                <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-[var(--text-secondary)]" />
                                <input
                                    type="password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="w-full pl-10 pr-4 py-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] focus:border-[var(--gold)] focus:outline-none transition"
                                    placeholder="••••••••"
                                    required
                                />
                            </div>
                            {/* Password strength indicator */}
                            <div className="mt-2 flex gap-1">
                                {[1, 2, 3].map((level) => (
                                    <div
                                        key={level}
                                        className={`h-1 flex-1 rounded-full transition ${passwordStrength() >= level
                                                ? level === 1
                                                    ? 'bg-red-500'
                                                    : level === 2
                                                        ? 'bg-yellow-500'
                                                        : 'bg-green-500'
                                                : 'bg-[var(--border)]'
                                            }`}
                                    />
                                ))}
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-2 text-[var(--text-secondary)]">
                                Confirm Password
                            </label>
                            <div className="relative">
                                <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-[var(--text-secondary)]" />
                                <input
                                    type="password"
                                    value={confirmPassword}
                                    onChange={(e) => setConfirmPassword(e.target.value)}
                                    className="w-full pl-10 pr-4 py-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)] focus:border-[var(--gold)] focus:outline-none transition"
                                    placeholder="••••••••"
                                    required
                                />
                                {confirmPassword && password === confirmPassword && (
                                    <Check className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-500" />
                                )}
                            </div>
                        </div>

                        <button
                            type="submit"
                            className="w-full btn-gold flex items-center justify-center"
                            disabled={isLoading}
                        >
                            {isLoading ? (
                                <Loader2 className="w-5 h-5 animate-spin" />
                            ) : (
                                'Start Building'
                            )}
                        </button>
                    </form>

                    <div className="mt-6 text-center text-sm text-[var(--text-secondary)]">
                        Already have a village?{' '}
                        <Link to="/login" className="text-[var(--gold)] hover:underline">
                            Login
                        </Link>
                    </div>
                </div>
            </motion.div>
        </div>
    );
}
