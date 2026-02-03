/**
 * Clan Page
 * View and manage clan, start wars, chat
 */
import { useEffect, useState, useRef } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
    Users, Crown, Shield, Sword, Swords,
    Search, Plus, Send, Trophy, Star,
    ArrowLeft, Loader2
} from 'lucide-react';

import { clanApi, createClanChatWs } from '../lib/api';
import { useClanStore } from '../stores/clanStore';
import { useAuthStore } from '../stores/authStore';

export function ClanPage() {
    const navigate = useNavigate();
    const { user } = useAuthStore();
    const { clan, war, messages, setClan, setWar, setMessages, addMessage, setLoading, isLoading } = useClanStore();

    const [activeTab, setActiveTab] = useState<'members' | 'war' | 'chat'>('members');
    const [searchQuery, setSearchQuery] = useState('');
    const [searchResults, setSearchResults] = useState<any[]>([]);
    const [showCreate, setShowCreate] = useState(false);
    const [newClanName, setNewClanName] = useState('');
    const [newClanTag, setNewClanTag] = useState('');
    const [chatMessage, setChatMessage] = useState('');
    const [ws, setWs] = useState<WebSocket | null>(null);

    const chatEndRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        loadClan();
    }, []);

    useEffect(() => {
        // Auto-scroll chat
        chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    useEffect(() => {
        // Connect to clan chat WebSocket when clan is loaded
        if (clan && activeTab === 'chat') {
            const socket = createClanChatWs(clan.id);

            socket.onmessage = (event) => {
                const data = JSON.parse(event.data);
                if (data.type === 'chat') {
                    addMessage({
                        id: Date.now().toString(),
                        user_id: data.user_id,
                        username: data.username,
                        message: data.message,
                        message_type: 'chat',
                        created_at: data.timestamp
                    });
                }
            };

            setWs(socket);

            return () => {
                socket.close();
            };
        }
    }, [clan, activeTab]);

    const loadClan = async () => {
        setLoading(true);
        try {
            const response = await clanApi.getMyClan();
            setClan(response.data);

            // Load chat history
            const chatResponse = await clanApi.getChatHistory();
            setMessages(chatResponse.data);

            // Load current war
            try {
                const warResponse = await clanApi.getCurrentWar();
                setWar(warResponse.data);
            } catch {
                setWar(null);
            }
        } catch {
            setClan(null);
        } finally {
            setLoading(false);
        }
    };

    const searchClans = async () => {
        if (!searchQuery.trim()) return;
        try {
            const response = await clanApi.search(searchQuery);
            setSearchResults(response.data);
        } catch (error) {
            console.error('Search failed:', error);
        }
    };

    const createClan = async () => {
        if (!newClanName.trim() || !newClanTag.trim()) return;
        try {
            await clanApi.create({
                name: newClanName,
                tag: newClanTag,
                is_public: true
            });
            setShowCreate(false);
            loadClan();
        } catch (error) {
            console.error('Create failed:', error);
        }
    };

    const joinClan = async (clanId: string) => {
        try {
            await clanApi.join(clanId);
            loadClan();
        } catch (error) {
            console.error('Join failed:', error);
        }
    };

    const leaveClan = async () => {
        if (!confirm('Are you sure you want to leave the clan?')) return;
        try {
            await clanApi.leave();
            setClan(null);
        } catch (error) {
            console.error('Leave failed:', error);
        }
    };

    const startWar = async () => {
        try {
            await clanApi.startWarSearch();
            const warResponse = await clanApi.getCurrentWar();
            setWar(warResponse.data);
        } catch (error) {
            console.error('War search failed:', error);
        }
    };

    const sendMessage = () => {
        if (!chatMessage.trim() || !ws) return;

        ws.send(JSON.stringify({
            type: 'chat',
            user_id: user?.id,
            username: user?.username,
            message: chatMessage
        }));

        setChatMessage('');
    };

    const getRoleIcon = (role: string) => {
        switch (role) {
            case 'leader': return <Crown className="w-4 h-4 text-yellow-400" />;
            case 'co_leader': return <Shield className="w-4 h-4 text-purple-400" />;
            case 'elder': return <Star className="w-4 h-4 text-blue-400" />;
            default: return <Sword className="w-4 h-4 text-gray-400" />;
        }
    };

    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <Loader2 className="w-12 h-12 animate-spin text-[var(--gold)]" />
            </div>
        );
    }

    // No clan - show search/create
    if (!clan) {
        return (
            <div className="min-h-screen p-4">
                <header className="mb-6">
                    <button
                        className="flex items-center gap-2 text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition mb-4"
                        onClick={() => navigate('/dashboard')}
                    >
                        <ArrowLeft className="w-5 h-5" />
                        Back
                    </button>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Users className="w-6 h-6 text-[var(--gold)]" />
                        Join a Legion
                    </h1>
                </header>

                {/* Search */}
                <div className="card mb-6">
                    <div className="flex gap-2">
                        <input
                            type="text"
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            placeholder="Search clans..."
                            className="flex-1 p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)]"
                            onKeyDown={(e) => e.key === 'Enter' && searchClans()}
                        />
                        <button className="btn-primary" onClick={searchClans}>
                            <Search className="w-5 h-5" />
                        </button>
                    </div>
                </div>

                {/* Search Results */}
                <div className="space-y-3 mb-6">
                    {searchResults.map((c) => (
                        <motion.div
                            key={c.id}
                            className="card card-hover flex items-center justify-between"
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                        >
                            <div>
                                <h3 className="font-semibold">{c.name}</h3>
                                <p className="text-sm text-[var(--text-secondary)]">
                                    {c.tag} • {c.member_count}/{c.max_members} members
                                </p>
                            </div>
                            <button
                                className="btn-gold text-sm py-2"
                                onClick={() => joinClan(c.id)}
                            >
                                Join
                            </button>
                        </motion.div>
                    ))}
                </div>

                {/* Create Clan */}
                <div className="card">
                    <button
                        className="w-full flex items-center justify-center gap-2 py-4 text-[var(--gold)]"
                        onClick={() => setShowCreate(!showCreate)}
                    >
                        <Plus className="w-5 h-5" />
                        Create Your Own Legion
                    </button>

                    <AnimatePresence>
                        {showCreate && (
                            <motion.div
                                initial={{ height: 0, opacity: 0 }}
                                animate={{ height: 'auto', opacity: 1 }}
                                exit={{ height: 0, opacity: 0 }}
                                className="overflow-hidden"
                            >
                                <div className="pt-4 space-y-4">
                                    <input
                                        type="text"
                                        value={newClanName}
                                        onChange={(e) => setNewClanName(e.target.value)}
                                        placeholder="Legion Name"
                                        className="w-full p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)]"
                                    />
                                    <input
                                        type="text"
                                        value={newClanTag}
                                        onChange={(e) => setNewClanTag(e.target.value.toUpperCase())}
                                        placeholder="Tag (e.g., ABC123)"
                                        maxLength={10}
                                        className="w-full p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)]"
                                    />
                                    <button className="w-full btn-gold" onClick={createClan}>
                                        Create Legion
                                    </button>
                                </div>
                            </motion.div>
                        )}
                    </AnimatePresence>
                </div>
            </div>
        );
    }

    // Has clan - show clan view
    return (
        <div className="min-h-screen flex flex-col">
            {/* Header */}
            <header className="sticky top-0 z-50 glass p-4">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                        <button
                            className="text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
                            onClick={() => navigate('/dashboard')}
                        >
                            <ArrowLeft className="w-5 h-5" />
                        </button>
                        <div>
                            <h1 className="font-bold text-lg">{clan.name}</h1>
                            <p className="text-sm text-[var(--text-secondary)]">{clan.tag}</p>
                        </div>
                    </div>
                    <div className="flex items-center gap-2">
                        <div className="text-right">
                            <div className="text-sm font-semibold text-[var(--gold)]">
                                Lv. {clan.level}
                            </div>
                            <div className="text-xs text-[var(--text-secondary)]">
                                {clan.war_wins} War Wins
                            </div>
                        </div>
                    </div>
                </div>

                {/* Tabs */}
                <div className="flex gap-2 mt-4">
                    {(['members', 'war', 'chat'] as const).map((tab) => (
                        <button
                            key={tab}
                            className={`flex-1 py-2 rounded-lg text-sm font-medium transition ${activeTab === tab
                                ? 'bg-[var(--gold)] text-[#0D1117]'
                                : 'bg-[var(--bg-secondary)] text-[var(--text-secondary)]'
                                }`}
                            onClick={() => setActiveTab(tab)}
                        >
                            {tab.charAt(0).toUpperCase() + tab.slice(1)}
                        </button>
                    ))}
                </div>
            </header>

            {/* Content */}
            <main className="flex-1 p-4 overflow-auto">
                {/* Members Tab */}
                {activeTab === 'members' && (
                    <div className="space-y-3">
                        {clan.members?.map((member) => (
                            <div
                                key={member.id}
                                className="card flex items-center justify-between"
                            >
                                <div className="flex items-center gap-3">
                                    {getRoleIcon(member.role)}
                                    <div>
                                        <p className="font-medium">{member.username}</p>
                                        <p className="text-xs text-[var(--text-secondary)] capitalize">
                                            {member.role.replace('_', ' ')} • {member.league_tier}
                                        </p>
                                    </div>
                                </div>
                                <div className="text-right">
                                    <p className="text-sm font-semibold text-[var(--gold)]">
                                        ⭐ {member.war_stars_earned}
                                    </p>
                                    <p className="text-xs text-[var(--text-secondary)]">
                                        {member.attacks_won} wins
                                    </p>
                                </div>
                            </div>
                        ))}

                        <button
                            className="w-full py-3 text-red-400 text-sm"
                            onClick={leaveClan}
                        >
                            Leave Legion
                        </button>
                    </div>
                )}

                {/* War Tab */}
                {activeTab === 'war' && (
                    <div>
                        {war ? (
                            <div className="card">
                                <div className="text-center mb-6">
                                    <h3 className="text-lg font-bold">{war.clan_name}</h3>
                                    <p className="text-sm text-[var(--text-secondary)]">VS</p>
                                    <h3 className="text-lg font-bold text-[var(--elixir)]">
                                        {war.opponent_clan_name}
                                    </h3>
                                </div>

                                <div className="grid grid-cols-2 gap-4 mb-6">
                                    <div className="text-center p-4 rounded-lg bg-[var(--bg-secondary)]">
                                        <div className="text-3xl font-bold text-[var(--gold)]">
                                            {war.clan_stars}
                                        </div>
                                        <div className="text-sm text-[var(--text-secondary)]">Stars</div>
                                    </div>
                                    <div className="text-center p-4 rounded-lg bg-[var(--bg-secondary)]">
                                        <div className="text-3xl font-bold text-[var(--elixir)]">
                                            {war.opponent_stars}
                                        </div>
                                        <div className="text-sm text-[var(--text-secondary)]">Enemy Stars</div>
                                    </div>
                                </div>

                                <div className="text-center">
                                    <p className="text-sm text-[var(--text-secondary)] capitalize mb-2">
                                        {war.state.replace('_', ' ')}
                                    </p>
                                    {war.time_remaining_seconds && (
                                        <p className="text-lg font-medium">
                                            {Math.floor(war.time_remaining_seconds / 3600)}h{' '}
                                            {Math.floor((war.time_remaining_seconds % 3600) / 60)}m remaining
                                        </p>
                                    )}
                                </div>
                            </div>
                        ) : (
                            <div className="text-center py-12">
                                <Swords className="w-16 h-16 text-[var(--text-secondary)] mx-auto mb-4" />
                                <p className="text-[var(--text-secondary)] mb-6">No active war</p>
                                <button className="btn-gold" onClick={startWar}>
                                    <Trophy className="w-5 h-5 mr-2 inline" />
                                    Start War Search
                                </button>
                            </div>
                        )}
                    </div>
                )}

                {/* Chat Tab */}
                {activeTab === 'chat' && (
                    <div className="flex flex-col h-[calc(100vh-220px)]">
                        <div className="flex-1 overflow-auto space-y-3">
                            {messages.map((msg) => (
                                <div
                                    key={msg.id}
                                    className={`p-3 rounded-lg ${msg.user_id === user?.id
                                        ? 'bg-[var(--gold)]/20 ml-8'
                                        : 'bg-[var(--bg-secondary)] mr-8'
                                        }`}
                                >
                                    <p className="text-xs font-medium text-[var(--text-secondary)]">
                                        {msg.username}
                                    </p>
                                    <p className="text-sm">{msg.message}</p>
                                </div>
                            ))}
                            <div ref={chatEndRef} />
                        </div>

                        <div className="flex gap-2 pt-4">
                            <input
                                type="text"
                                value={chatMessage}
                                onChange={(e) => setChatMessage(e.target.value)}
                                placeholder="Type a message..."
                                className="flex-1 p-3 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border)]"
                                onKeyDown={(e) => e.key === 'Enter' && sendMessage()}
                            />
                            <button className="btn-primary" onClick={sendMessage}>
                                <Send className="w-5 h-5" />
                            </button>
                        </div>
                    </div>
                )}
            </main>
        </div>
    );
}
