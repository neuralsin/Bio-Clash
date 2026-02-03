/**
 * Login Page
 */
import { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, Link } from 'react-router-dom';
import { Sword, Mail, Lock, Loader2 } from 'lucide-react';
import { authApi } from '../lib/api';
import { useAuthStore } from '../stores/authStore';

export function LoginPage() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();
    const login = useAuthStore((state) => state.login);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setIsLoading(true);

        try {
            const response = await authApi.login(email, password);
            login(response.data.access_token, {
                id: '',
                email,
                username: email.split('@')[0],
                consistency_score: 0,
                recovery_score: 0,
                league_tier: 'bronze'
            });
            navigate('/dashboard');
        } catch (err: any) {
            setError(err.response?.data?.detail || 'Login failed');
        } finally {
            setIsLoading(false);
        }
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
                        Your Body Builds Your Base
                    </p>
                </div>

                {/* Form */}
                <div className="card">
                    <h2 className="text-xl font-semibold mb-6 text-center">Welcome Back, Chief!</h2>

                    {error && (
                        <div className="mb-4 p-3 rounded-lg bg-red-500/10 border border-red-500/50 text-red-400 text-sm">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-4">
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
                        </div>

                        <button
                            type="submit"
                            className="w-full btn-gold flex items-center justify-center"
                            disabled={isLoading}
                        >
                            {isLoading ? (
                                <Loader2 className="w-5 h-5 animate-spin" />
                            ) : (
                                'Enter Battle'
                            )}
                        </button>
                    </form>

                    <div className="mt-6 text-center text-sm text-[var(--text-secondary)]">
                        New to Bio-Clash?{' '}
                        <Link to="/register" className="text-[var(--gold)] hover:underline">
                            Create Account
                        </Link>
                    </div>
                </div>
            </motion.div>
        </div>
    );
}
