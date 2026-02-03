/**
 * Recovery Status Component
 * Displays the Fatigue Oracle's assessment
 */
import { motion } from 'framer-motion';
import { Shield, Heart, AlertTriangle, Zap } from 'lucide-react';

interface RecoveryStatusProps {
    recoveryPercent: number;
    status: string;
    shieldActive: boolean;
    recommendations: string[];
}

export function RecoveryStatus({
    recoveryPercent,
    status,
    shieldActive,
    recommendations,
}: RecoveryStatusProps) {
    const getStatusColor = () => {
        if (status === 'critical') return 'text-red-500';
        if (status === 'fatigued') return 'text-orange-500';
        if (status === 'recovering') return 'text-yellow-500';
        return 'text-green-500';
    };

    const getStatusIcon = () => {
        if (status === 'critical') return <AlertTriangle className="w-6 h-6" />;
        if (status === 'fatigued') return <AlertTriangle className="w-6 h-6" />;
        return <Heart className="w-6 h-6" />;
    };

    return (
        <motion.div
            className="card"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
        >
            <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold flex items-center gap-2">
                    <Zap className="w-5 h-5 text-yellow-400" />
                    Recovery Status
                </h3>
                {shieldActive && (
                    <div className="flex items-center gap-2 px-3 py-1 rounded-full bg-blue-500/20 text-blue-400">
                        <Shield className="w-4 h-4" />
                        <span className="text-sm font-medium">Shield Active</span>
                    </div>
                )}
            </div>

            {/* Recovery Meter */}
            <div className="mb-4">
                <div className="flex justify-between items-center mb-2">
                    <span className={`text-2xl font-bold ${getStatusColor()}`}>
                        {recoveryPercent.toFixed(0)}%
                    </span>
                    <div className={`flex items-center gap-1 ${getStatusColor()}`}>
                        {getStatusIcon()}
                        <span className="capitalize font-medium">{status}</span>
                    </div>
                </div>
                <div className="h-3 bg-[var(--bg-primary)] rounded-full overflow-hidden">
                    <motion.div
                        className={`h-full rounded-full ${status === 'optimal'
                                ? 'bg-gradient-to-r from-green-500 to-emerald-400'
                                : status === 'recovering'
                                    ? 'bg-gradient-to-r from-yellow-500 to-orange-400'
                                    : status === 'fatigued'
                                        ? 'bg-gradient-to-r from-orange-500 to-red-400'
                                        : 'bg-gradient-to-r from-red-600 to-red-400'
                            }`}
                        initial={{ width: 0 }}
                        animate={{ width: `${recoveryPercent}%` }}
                        transition={{ duration: 0.5, ease: 'easeOut' }}
                    />
                </div>
            </div>

            {/* Recommendations */}
            {recommendations.length > 0 && (
                <div className="space-y-2">
                    <h4 className="text-sm font-medium text-[var(--text-secondary)]">
                        Recommendations
                    </h4>
                    {recommendations.map((rec, idx) => (
                        <div
                            key={idx}
                            className="text-sm p-2 rounded bg-[var(--bg-secondary)] border border-[var(--border)]"
                        >
                            {rec}
                        </div>
                    ))}
                </div>
            )}
        </motion.div>
    );
}
