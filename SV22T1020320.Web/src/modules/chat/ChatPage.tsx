import React, { useState, useRef, useEffect } from 'react';
import { Layout, Avatar, Badge, Input, Button, Space, Typography, message, Alert, Modal, Form, Input as AntInput, Select, List, Skeleton, Switch } from 'antd';
import {
    SearchOutlined,
    PaperClipOutlined,
    SmileOutlined,
    SendOutlined,
    TeamOutlined,
    SettingOutlined,
    PlusOutlined,
    UserAddOutlined,
} from '@ant-design/icons';
import { chatApi } from './services/chatApi';
import { signalRService } from './services/signalRService';
import { ChannelDto, MessageDto, MemberDto } from './types/chat';
import { useAuth } from '../../contexts/AuthContext';

import { useLocation } from 'react-router-dom';

const { Sider, Content } = Layout;
const { Text } = Typography;

// ─── Types ────────────────────────────────────────────────────────────────────

interface Channel {
    id: number;
    name: string;
    lastMsg: string;
    unread: number;
    color: string;
    bg: string;
    type: 'Group' | 'Direct';
}

interface DirectMessage {
    id: number;
    name: string;
    initials: string;
    color: string;
    bg: string;
    online: boolean;
}

interface Message {
    id: number;
    senderId: number;
    sender: string;
    initials: string;
    color: string;
    bg: string;
    text: string;
    time: string;
    reactions?: { emoji: string; count: number }[];
}

// ─── Helper functions ───────────────────────────────────────────────────────────

const getColorForUser = (initials: string): { color: string; bg: string } => {
    const colors = [
        { color: '#185FA5', bg: '#E6F1FB' },
        { color: '#0F6E56', bg: '#E1F5EE' },
        { color: '#854F0B', bg: '#FAEEDA' },
        { color: '#993556', bg: '#FBEAF0' },
    ];
    const index = initials.charCodeAt(0) % colors.length;
    return colors[index];
};

const formatTime = (isoString: string): string => {
    // Backend thường trả về DateTime không có chữ 'Z' ở cuối (Unspecified Kind).
    // Ta tự động thêm 'Z' để trình duyệt hiểu đó là giờ UTC và tự cộng 7 tiếng (múi giờ Việt Nam).
    const timeStr = isoString.endsWith('Z') ? isoString : `${isoString}Z`;
    const date = new Date(timeStr);
    return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
};

const mapChannelDtoToChannel = (dto: ChannelDto): Channel => ({
    id: dto.chatChannelID,
    name: dto.name,
    lastMsg: dto.lastMessage?.content || '',
    unread: dto.unreadCount,
    color: '#185FA5',
    bg: '#E6F1FB',
    type: dto.type,
});

const mapMessageDtoToMessage = (dto: MessageDto, currentUserId?: number): Message => {
    const { color, bg } = getColorForUser(dto.senderInitials);
    return {
        id: dto.chatMessageID,
        senderId: dto.senderUserID,
        sender: dto.senderName,
        initials: dto.senderInitials,
        color,
        bg,
        text: dto.content,
        time: formatTime(dto.sentAt),
        reactions: dto.reactions.map(r => ({ emoji: r.emoji, count: r.count })),
    };
};

// ─── Sub-components ────────────────────────────────────────────────────────────

const OnlineDot: React.FC<{ online: boolean }> = ({ online }) => (
    <span
        style={{
            position: 'absolute', bottom: 0, right: 0,
            width: 9, height: 9, borderRadius: '50%',
            background: online ? '#1D9E75' : '#aaa',
            border: '1.5px solid #fff',
        }}
    />
);

const ChatSkeleton: React.FC = () => {
    return (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
            {/* Left bubble skeleton */}
            <div style={{ display: 'flex', gap: 10, alignItems: 'flex-start' }} className="pulse-animation">
                <div style={{ width: 34, height: 34, borderRadius: '50%', background: '#ebebeb', flexShrink: 0 }} />
                <div style={{ display: 'flex', flexDirection: 'column', gap: 6, width: '40%' }}>
                    <div style={{ width: '80px', height: 10, background: '#f0f0f0', borderRadius: 4 }} />
                    <div style={{ height: 36, background: '#f8fafc', borderRadius: '4px 16px 16px 16px', border: '1px solid #e2e8f0' }} />
                </div>
            </div>

            {/* Right bubble skeleton */}
            <div style={{ display: 'flex', gap: 10, alignItems: 'flex-start', flexDirection: 'row-reverse' }} className="pulse-animation">
                <div style={{ width: 34, height: 34, borderRadius: '50%', background: '#ebebeb', flexShrink: 0 }} />
                <div style={{ display: 'flex', flexDirection: 'column', gap: 6, width: '45%', alignItems: 'flex-end' }}>
                    <div style={{ width: '60px', height: 10, background: '#f0f0f0', borderRadius: 4 }} />
                    <div style={{ height: 42, background: 'linear-gradient(135deg, #e0f2fe 0%, #bae6fd 100%)', borderRadius: '16px 4px 16px 16px', width: '100%' }} />
                </div>
            </div>

            {/* Left bubble skeleton 2 */}
            <div style={{ display: 'flex', gap: 10, alignItems: 'flex-start' }} className="pulse-animation">
                <div style={{ width: 34, height: 34, borderRadius: '50%', background: '#ebebeb', flexShrink: 0 }} />
                <div style={{ display: 'flex', flexDirection: 'column', gap: 6, width: '30%' }}>
                    <div style={{ width: '70px', height: 10, background: '#f0f0f0', borderRadius: 4 }} />
                    <div style={{ height: 34, background: '#f8fafc', borderRadius: '4px 16px 16px 16px', border: '1px solid #e2e8f0' }} />
                </div>
            </div>
        </div>
    );
};

const MessageBubble: React.FC<{ msg: Message; currentUserId?: number }> = ({ msg, currentUserId }) => {
    const isMe = msg.senderId == currentUserId;

    // Tự động phát hiện URL và chuyển đổi thành thẻ <a> click được
    const renderMessageText = (text: string) => {
        if (!text) return null;
        
        // Phát hiện URL bắt đầu bằng http://, https:// hoặc www.
        const urlRegex = /(https?:\/\/[^\s]+|www\.[^\s]+)/gi;
        const parts = text.split(urlRegex);
        
        if (parts.length === 1) {
            return text;
        }
        
        return parts.map((part, index) => {
            if (part.match(urlRegex)) {
                let href = part;
                if (!part.match(/^https?:\/\//i)) {
                    href = `http://${part}`;
                }
                return (
                    <a
                        key={index}
                        href={href}
                        target="_blank"
                        rel="noopener noreferrer"
                        style={{
                            color: isMe ? '#fff' : '#185FA5',
                            textDecoration: 'underline',
                            wordBreak: 'break-all',
                            fontWeight: 500,
                        }}
                    >
                        {part}
                    </a>
                );
            }
            return part;
        });
    };

    return (
        <div style={{ display: 'flex', flexDirection: isMe ? 'row-reverse' : 'row', gap: 10, marginBottom: 16 }}>
            {/* Avatar */}
            <div style={{ position: 'relative', flexShrink: 0 }}>
                <Avatar
                    style={{ background: msg.bg, color: msg.color, fontWeight: 500, fontSize: 12 }}
                    size={34}
                >
                    {msg.initials}
                </Avatar>
            </div>

            {/* Body */}
            <div style={{ maxWidth: '65%', display: 'flex', flexDirection: 'column', alignItems: isMe ? 'flex-end' : 'flex-start' }}>
                {/* Meta */}
                <div style={{ display: 'flex', flexDirection: isMe ? 'row-reverse' : 'row', alignItems: 'baseline', gap: 6, marginBottom: 3 }}>
                    <Text strong style={{ fontSize: 12 }}>{msg.sender}</Text>
                    <Text type="secondary" style={{ fontSize: 11 }}>{msg.time}</Text>
                </div>

                {/* Bubble */}
                <div
                    className={isMe ? 'message-bubble-me' : 'message-bubble-other'}
                    style={{
                        padding: '10px 14px',
                        borderRadius: isMe ? '16px 4px 16px 16px' : '4px 16px 16px 16px',
                        fontSize: 13.5,
                        lineHeight: 1.5,
                        color: isMe ? '#fff' : '#1e293b',
                        border: isMe ? 'none' : '1px solid #e2e8f0',
                        wordBreak: 'break-word',
                    }}
                >
                    {renderMessageText(msg.text)}
                </div>

                {/* Reactions */}
                {msg.reactions && (
                    <div style={{ display: 'flex', gap: 4, marginTop: 4 }}>
                        {msg.reactions.map((r, i) => (
                            <span
                                key={i}
                                style={{
                                    background: '#f0f0f0', border: '0.5px solid #e0e0e0',
                                    borderRadius: 12, padding: '2px 7px', fontSize: 11, cursor: 'pointer',
                                }}
                            >
                                {r.emoji} {r.count}
                            </span>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

// ─── Memory Cache for Zalo-like Instant Switching & Route Rehydration ──────────
let cachedActiveChannelId: number | null = null;
let cachedActiveChannelType: 'Group' | 'Direct' | null = null;
let cachedChannels: Channel[] | null = null;
let cachedDirects: DirectMessage[] | null = null;
let cachedMembers: { [channelId: number]: MemberDto[] } = {};
let cachedMessages: { [channelId: number]: Message[] } = {};

// ─── Main Component ────────────────────────────────────────────────────────────

const ChatPage: React.FC = () => {
    const { user } = useAuth();
    const location = useLocation();
    const [activeChannelId, setActiveChannelId] = useState<number | null>(() => cachedActiveChannelId);
    const [activeChannelType, setActiveChannelType] = useState<'Group' | 'Direct' | null>(() => cachedActiveChannelType);
    const [channels, setChannels] = useState<Channel[]>(() => cachedChannels || []);
    const [directs, setDirects] = useState<DirectMessage[]>(() => cachedDirects || []);
    const [members, setMembers] = useState<MemberDto[]>(() => 
        activeChannelId && cachedMembers[activeChannelId] ? cachedMembers[activeChannelId] : []
    );
    const [messages, setMessages] = useState<Message[]>(() => 
        activeChannelId && cachedMessages[activeChannelId] ? cachedMessages[activeChannelId] : []
    );
    const [inputValue, setInputValue] = useState('');
    const [searchValue, setSearchValue] = useState('');
    const [loading, setLoading] = useState(() => !cachedChannels);
    const [messagesLoading, setMessagesLoading] = useState(false);
    const [isSyncing, setIsSyncing] = useState(false);
    const [connectionStatus, setConnectionStatus] = useState<'connected' | 'reconnecting' | 'disconnected'>('disconnected');
    const [isCreateChannelModalOpen, setIsCreateChannelModalOpen] = useState(false);
    const [isAddMemberModalOpen, setIsAddMemberModalOpen] = useState(false);
    const [isMembersListModalOpen, setIsMembersListModalOpen] = useState(false);
    const [isCreateDirectModalOpen, setIsCreateDirectModalOpen] = useState(false);
    const [usersLookup, setUsersLookup] = useState<{ userID: number, fullName: string, department: string | null }[]>([]);
    const [selectedUserIds, setSelectedUserIds] = useState<number[]>([]);
    const [selectedDirectUserId, setSelectedDirectUserId] = useState<number | null>(null);
    const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
    const [chatSettings, setChatSettings] = useState(() => {
        const saved = localStorage.getItem('chat_settings');
        return saved ? JSON.parse(saved) : {
            duration: 4.5,
            privacy: false,
            sound: true,
            tabFlash: true
        };
    });
    const messagesEndRef = useRef<HTMLDivElement>(null);
    const messagesContainerRef = useRef<HTMLDivElement>(null);
    const activeChannelIdRef = useRef<number | null>(activeChannelId);
    const sentMessageIdsRef = useRef<Set<number>>(new Set());

    // Sync state updates to cache reactively
    useEffect(() => {
        cachedChannels = channels;
    }, [channels]);

    useEffect(() => {
        cachedDirects = directs;
    }, [directs]);

    useEffect(() => {
        if (activeChannelId) {
            cachedMessages[activeChannelId] = messages;
        }
    }, [messages, activeChannelId]);

    useEffect(() => {
        if (activeChannelId) {
            cachedMembers[activeChannelId] = members;
        }
    }, [members, activeChannelId]);

    useEffect(() => {
        activeChannelIdRef.current = activeChannelId;
        signalRService.activeChannelId = activeChannelId;
        cachedActiveChannelId = activeChannelId;
        cachedActiveChannelType = activeChannelType;
        return () => {
            signalRService.activeChannelId = null;
        };
    }, [activeChannelId, activeChannelType]);

    // Lắng nghe thay đổi tham số truy vấn trên URL (do click thông báo Toast hoặc chuyển kênh)
    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const qChannelId = params.get('channelId');
        if (qChannelId) {
            const parsedId = parseInt(qChannelId, 10);
            if (parsedId && parsedId !== activeChannelId) {
                // Tự động suy luận Type bằng cách tìm trong danh sách kênh / DM hiện tại
                const isGroup = channels.some(c => c.id === parsedId);
                const isDirect = directs.some(d => d.id === parsedId);
                
                if (isGroup) {
                    setActiveChannelId(parsedId);
                    setActiveChannelType('Group');
                } else if (isDirect) {
                    setActiveChannelId(parsedId);
                    setActiveChannelType('Direct');
                } else {
                    setActiveChannelId(parsedId);
                    setActiveChannelType('Group'); // Mặc định nếu không tìm thấy
                }
            }
        }
    }, [location.search, activeChannelId, channels, directs]);

    const activeChannel = React.useMemo(() => {
        if (!activeChannelId || !activeChannelType) return null;
        
        if (activeChannelType === 'Group') {
            const channel = channels.find(c => c.id === activeChannelId);
            if (channel) return channel;
        } else {
            const dm = directs.find(d => d.id === activeChannelId);
            if (dm) {
                return {
                    id: dm.id,
                    name: dm.name,
                    lastMsg: '',
                    unread: 0,
                    color: dm.color,
                    bg: dm.bg,
                    type: 'Direct' as const
                };
            }
        }
        return null;
    }, [channels, directs, activeChannelId, activeChannelType]);
    const currentUserId = user?.userId;

    // Connect to SignalR on mount
    useEffect(() => {
        let unsubscribeReceiveMessage: (() => void) | null = null;
        let unsubscribeReconnecting: (() => void) | null = null;
        let unsubscribeReconnected: (() => void) | null = null;
        
        const setupSignalR = async () => {
            try {
                // MUST register event handlers BEFORE starting connection
                unsubscribeReceiveMessage = signalRService.onReceiveMessage((msg: MessageDto) => {
                    const currentActiveId = activeChannelIdRef.current;
                    
                    // Only process message if it's for the current channel
                    if (msg.chatChannelID === currentActiveId) {
                        // Kiểm tra xem tin nhắn này có phải do chính tab này vừa gửi (đã update optimistic) không
                        const isDuplicate = sentMessageIdsRef.current.has(msg.chatMessageID);
                        
                        if (!isDuplicate) {
                            const newMsg = mapMessageDtoToMessage(msg);
                            setMessages(prev => {
                                // Tránh add trùng lặp nếu message này đã tồn tại
                                if (prev.some(m => m.id === newMsg.id)) return prev;
                                return [...prev, newMsg];
                            });
                        }
                        
                        // Dọn dẹp ID khỏi Set để tránh rác bộ nhớ (vì SignalR đã echo về rồi)
                        sentMessageIdsRef.current.delete(msg.chatMessageID);

                        // Đồng bộ đánh dấu đã đọc lên backend ngay lập tức vì người dùng đang mở sẵn kênh này
                        chatApi.markRead(currentActiveId).catch(err => {
                            console.error('Error marking live message as read:', err);
                        });
                    }
                    
                    // Update channels sidebar
                    setChannels(prev => prev.map(c => {
                        if (c.id === msg.chatChannelID) {
                            return {
                                ...c,
                                lastMsg: msg.content,
                                unread: msg.chatChannelID === currentActiveId ? 0 : c.unread + 1
                            };
                        }
                        return c;
                    }));
                });
                
                // Listen for reconnection events
                unsubscribeReconnecting = signalRService.onReconnecting(() => {
                    setConnectionStatus('reconnecting');
                });
                
                unsubscribeReconnected = signalRService.onReconnected(() => {
                    setConnectionStatus('connected');
                    // Reload messages after reconnection to catch any missed messages
                    if (activeChannelIdRef.current) {
                        chatApi.getMessages(activeChannelIdRef.current).then(result => {
                            const mappedMessages = result.messages.map(mapMessageDtoToMessage);
                            setMessages(mappedMessages);
                        }).catch(err => {
                            console.error('Error reloading messages after reconnection:', err);
                        });
                    }
                });

                await signalRService.connect();
                setConnectionStatus('connected');
                
            } catch (error) {
                console.error('SignalR connection error:', error);
                setConnectionStatus('disconnected');
            }
        };
        
        setupSignalR();
        
        return () => {
            unsubscribeReceiveMessage?.();
            unsubscribeReconnecting?.();
            unsubscribeReconnected?.();
        };
    }, [currentUserId]); // Removed activeChannelId to prevent reconnecting on channel switch

    // Load channels and directs on mount
    useEffect(() => {
        const loadData = async () => {
            const hasCache = !!cachedChannels;
            try {
                if (!hasCache) {
                    setLoading(true);
                } else {
                    setIsSyncing(true);
                }
                
                // Chỉ gọi 1 API getChannels() rồi filter client-side
                const allChannels = await chatApi.getChannels();
                
                const groupChannels = allChannels
                    .filter(c => c.type === 'Group')
                    .map(mapChannelDtoToChannel)
                    .map(c => c.id === activeChannelIdRef.current ? { ...c, unread: 0 } : c);
                setChannels(groupChannels);
                
                const directChannels = allChannels.filter(c => c.type === 'Direct');
                const mappedDirects = directChannels.map(d => {
                    const { color, bg } = getColorForUser(d.name.substring(0, 2).toUpperCase());
                    return {
                        id: d.chatChannelID,
                        name: d.name,
                        initials: d.name.substring(0, 2).toUpperCase(),
                        color,
                        bg,
                        online: false, // TODO: Get from members API
                    };
                });
                setDirects(mappedDirects);
            } catch (error) {
                message.error('Không thể tải dữ liệu chat');
                console.error('Error loading chat data:', error);
            } finally {
                setLoading(false);
                setIsSyncing(false);
            }
        };
        loadData();
        // Load active users list for selects
        chatApi.getUsersLookup().then(setUsersLookup).catch(console.error);
    }, []);

    // Join channel when activeChannelId or connectionStatus changes
    useEffect(() => {
        if (!activeChannelId || connectionStatus !== 'connected') return;
        
        const joinChannel = async () => {
            try {
                await signalRService.joinChannel(activeChannelId);
            } catch (error) {
                console.error('Error joining channel:', error);
            }
        };
        joinChannel();
    }, [activeChannelId, connectionStatus]);

    // Load messages when channel changes
    useEffect(() => {
        if (!activeChannelId) return;
        
        // Cập nhật UX lập tức: Xóa số thông báo (unread) khi người dùng click vào kênh
        setChannels(prev => prev.map(c => 
            c.id === activeChannelId ? { ...c, unread: 0 } : c
        ));
        
        const loadMessages = async () => {
            const hasCache = !!cachedMessages[activeChannelId];
            try {
                if (!hasCache) {
                    setMessagesLoading(true);
                } else {
                    setIsSyncing(true);
                }

                // Dùng Promise.all để gọi song song getMessages và getMembers
                const [result, membersData] = await Promise.all([
                    chatApi.getMessages(activeChannelId),
                    chatApi.getMembers(activeChannelId),
                ]);
                const mappedMessages = result.messages.map(mapMessageDtoToMessage);
                setMessages(mappedMessages);
                setMembers(membersData);
                
                // Force cuộn xuống tin nhắn mới nhất (dưới cùng) ngay khi load xong kênh
                setTimeout(() => {
                    messagesEndRef.current?.scrollIntoView({ behavior: 'auto' });
                }, 50);
                
                // Mark as read - fire-and-forget, không await để không block UI
                chatApi.markRead(activeChannelId).catch(err => {
                    console.error('Error marking as read:', err);
                });
            } catch (error) {
                message.error('Không thể tải tin nhắn');
                console.error('Error loading messages:', error);
            } finally {
                setIsSyncing(false);
                setMessagesLoading(false);
            }
        };
        loadMessages();
    }, [activeChannelId]);

    useEffect(() => {
        const container = messagesContainerRef.current;
        if (!container) return;
        const isNearBottom = container.scrollHeight - container.scrollTop - container.clientHeight < 150;
        if (isNearBottom || messages.length <= 20) {
            messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
        }
    }, [messages]);

    const handleSend = async () => {
        const text = inputValue.trim();
        if (!text || !activeChannelId) return;
        
        // Optimistic UI
        const tempId = Date.now();
        const optimisticMsg: Message = {
            id: tempId,
            senderId: currentUserId || 0,
            sender: user?.displayName || user?.username || 'Me',
            initials: (user?.displayName || user?.username || 'Me').substring(0, 2).toUpperCase(),
            color: '#fff',
            bg: '#185FA5',
            text: text,
            time: formatTime(new Date().toISOString()),
        };
        setMessages(prev => [...prev, optimisticMsg]);
        setChannels(prev => prev.map(c => c.id === activeChannelId ? { ...c, lastMsg: text } : c));
        setInputValue('');
        
        try {
            const messageDto = await chatApi.sendMessage(activeChannelId, { content: text });
            const newMsg = mapMessageDtoToMessage(messageDto);
            sentMessageIdsRef.current.add(messageDto.chatMessageID); // Đánh dấu ID đã được xử lý ở tab này
            
            setMessages(prev => {
                // Nếu SignalR đã add message này nhanh hơn tốc độ API trả về,
                // ta chỉ cần xóa bỏ cái message ảo (tempId) đi.
                if (prev.some(m => m.id === newMsg.id)) {
                    return prev.filter(m => m.id !== tempId);
                }
                // Còn nếu API trả về trước SignalR, ta đổi ID ảo thành ID thật
                return prev.map(m => m.id === tempId ? newMsg : m);
            });
        } catch (error) {
            message.error('Không thể gửi tin nhắn');
            console.error('Error sending message:', error);
            setMessages(prev => prev.filter(m => m.id !== tempId));
            setInputValue(text);
        }
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter') { handleSend(); return; }
    };

    const filteredChannels = (channels || []).filter(c =>
        c.name.toLowerCase().includes(searchValue.toLowerCase())
    );
    const filteredDMs = (directs || []).filter(d =>
        d.name.toLowerCase().includes(searchValue.toLowerCase())
    );

    return (
        <Layout
            style={{
                background: '#fff',
                borderRadius: 0,
                border: 'none',
                flex: 1,
                height: '100%',
                minHeight: 0,
            }}
        >
            {/* ── Sidebar ───────────────────────────────────────────── */}
            <Sider width={260} theme="light" style={{ background: '#f8fafc', borderRight: '1px solid #e2e8f0' }} className="sidebar-scroll-area">
                {/* Workspace header */}
                <div style={{ padding: '14px 16px', borderBottom: '0.5px solid #ebebeb' }}>
                    <Text strong style={{ fontSize: 13, color: '#666', letterSpacing: 0.5, textTransform: 'uppercase' }}>
                        EMS Workspace
                    </Text>
                </div>

                {/* Search */}
                <div style={{ padding: '10px 12px', borderBottom: '0.5px solid #ebebeb' }}>
                    <Input
                        prefix={<SearchOutlined style={{ color: '#bbb', fontSize: 12 }} />}
                        placeholder="Tìm kiếm..."
                        size="small"
                        value={searchValue}
                        onChange={e => setSearchValue(e.target.value)}
                        style={{ borderRadius: 6, fontSize: 12 }}
                    />
                </div>

                {/* Channels */}
                <div style={{ padding: '10px 14px 4px', fontSize: 11, color: '#aaa', letterSpacing: 0.6, textTransform: 'uppercase', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    Kênh
                    <Button
                        type="text"
                        size="small"
                        icon={<PlusOutlined />}
                        onClick={() => setIsCreateChannelModalOpen(true)}
                        style={{ fontSize: 11, color: '#666' }}
                    >
                        Tạo mới
                    </Button>
                </div>
                {loading ? (
                    <div style={{ padding: '16px' }}>
                        <Skeleton active title={false} paragraph={{ rows: 3, width: ['100%', '80%', '90%'] }} />
                    </div>
                ) : filteredChannels.length === 0 ? (
                    <div style={{ padding: '20px', textAlign: 'center', color: '#999', fontSize: 12 }}>
                        Chưa có kênh nào
                    </div>
                ) : (
                    filteredChannels.map(ch => {
                        const isActive = activeChannelId === ch.id && activeChannelType === 'Group';
                        return (
                            <div
                                key={ch.id}
                                onClick={() => {
                                    setActiveChannelId(ch.id);
                                    setActiveChannelType('Group');
                                }}
                                style={{
                                    display: 'flex', alignItems: 'center', gap: 10,
                                    padding: '8px 12px', cursor: 'pointer', borderRadius: 8,
                                    margin: '2px 8px',
                                    background: isActive ? '#fff' : 'transparent',
                                    boxShadow: isActive ? '0 1px 3px rgba(0,0,0,0.05), 0 1px 2px rgba(0,0,0,0.03)' : 'none',
                                    transition: 'all 0.2s ease',
                                }}
                            >
                                <div style={{
                                    width: 30, height: 30, borderRadius: 8,
                                    background: ch.bg, color: ch.color,
                                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                                    fontWeight: 500, fontSize: 13, flexShrink: 0,
                                }}>
                                    #
                                </div>
                                <div style={{ flex: 1, minWidth: 0 }}>
                                    <Text strong style={{ fontSize: 13, display: 'block' }}>{ch.name}</Text>
                                    <Text type="secondary" ellipsis style={{ fontSize: 11, width: 130 }}>{ch.lastMsg}</Text>
                                </div>
                                {ch.unread > 0 && <Badge count={ch.unread} size="small" />}
                            </div>
                        );
                    })
                )}

                {/* DMs */}
                <div style={{ padding: '12px 14px 4px', fontSize: 11, color: '#aaa', letterSpacing: 0.6, textTransform: 'uppercase', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    Tin nhắn trực tiếp
                    <Button
                        type="text"
                        size="small"
                        icon={<PlusOutlined />}
                        onClick={() => setIsCreateDirectModalOpen(true)}
                        style={{ fontSize: 11, color: '#666' }}
                    >
                        Mới
                    </Button>
                </div>
                {filteredDMs.map(dm => {
                    const isActive = activeChannelId === dm.id && activeChannelType === 'Direct';
                    return (
                        <div
                            key={dm.id}
                            onClick={() => {
                                setActiveChannelId(dm.id);
                                setActiveChannelType('Direct');
                            }}
                            style={{
                                display: 'flex', alignItems: 'center', gap: 8,
                                padding: '8px 12px', cursor: 'pointer', borderRadius: 8,
                                margin: '2px 8px',
                                background: isActive ? '#fff' : 'transparent',
                                boxShadow: isActive ? '0 1px 3px rgba(0,0,0,0.05), 0 1px 2px rgba(0,0,0,0.03)' : 'none',
                                transition: 'all 0.2s ease',
                            }}
                        >
                            <div style={{ position: 'relative', flexShrink: 0 }}>
                                <Avatar size={28} style={{ background: dm.bg, color: dm.color, fontSize: 11, fontWeight: 500 }}>
                                    {dm.initials}
                                </Avatar>
                                <OnlineDot online={dm.online} />
                            </div>
                            <Text style={{ fontSize: 13 }}>{dm.name}</Text>
                        </div>
                    );
                })}
            </Sider>

            {/* ── Main chat area ────────────────────────────────────── */}
            <Content style={{ display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
                {!activeChannel ? (
                    <div style={{
                        flex: 1,
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: '#999',
                    }}>
                        <TeamOutlined style={{ fontSize: 48, marginBottom: 16 }} />
                        <Text style={{ fontSize: 14 }}>Chọn một kênh để bắt đầu chat</Text>
                    </div>
                ) : (
                    <>
                        {/* Header */}
                        <div className="glass-morphism" style={{
                            padding: '12px 20px', borderBottom: '1px solid #e2e8f0',
                            display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                            zIndex: 10, position: 'relative',
                        }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                                <div style={{
                                    width: 32, height: 32, borderRadius: 10,
                                    background: activeChannel?.bg, color: activeChannel?.color,
                                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                                    fontWeight: 600, fontSize: 14,
                                    boxShadow: '0 2px 4px rgba(0,0,0,0.05)'
                                }}>
                                    {activeChannel?.type === 'Direct' ? '@' : '#'}
                                </div>
                                <div>
                                    <Text strong style={{ fontSize: 15, display: 'block' }}>{activeChannel?.name}</Text>
                                    <Text type="secondary" style={{ fontSize: 12 }}>
                                        {activeChannel?.type === 'Direct' 
                                            ? 'Trò chuyện 1-1 trực tuyến' 
                                            : `${(members || []).length} thành viên · kênh chung`
                                        }
                                    </Text>
                                </div>
                            </div>
                            <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
                                {isSyncing && (
                                    <Badge status="processing" text="Đang đồng bộ..." style={{ marginRight: 10, fontSize: 11.5 }} />
                                )}
                                {connectionStatus === 'reconnecting' && (
                                    <Badge status="warning" text="Đang kết nối lại..." style={{ marginRight: 10, fontSize: 12 }} />
                                )}
                                {connectionStatus === 'disconnected' && (
                                    <Badge status="error" text="Mất kết nối" style={{ marginRight: 10, fontSize: 12 }} />
                                )}
                                <Button icon={<SearchOutlined />} size="small" type="text" />
                                {activeChannel?.type === 'Group' && (
                                    <Button icon={<UserAddOutlined />} size="small" type="text" onClick={() => setIsAddMemberModalOpen(true)} />
                                )}
                                 <Button icon={<TeamOutlined />} size="small" type="text" onClick={() => setIsMembersListModalOpen(true)} />
                                <Button 
                                    icon={<SettingOutlined />} 
                                    size="small" 
                                    type="text" 
                                    onClick={() => setIsSettingsModalOpen(true)} 
                                />
                            </div>
                        </div>

                {/* Messages */}
                <div 
                    key={activeChannelId || 'no-channel'} 
                    ref={messagesContainerRef} 
                    className="chat-scroll-area fade-in-up" 
                    style={{ flex: 1, padding: '20px 24px', background: '#fff' }}
                >
                    {messagesLoading ? (
                        <ChatSkeleton />
                    ) : (
                        <>
                            <div style={{
                                textAlign: 'center', fontSize: 11, color: '#bbb',
                                marginBottom: 16, position: 'relative',
                            }}>
                                <span style={{
                                    background: '#fff', padding: '0 10px',
                                    position: 'relative', zIndex: 1,
                                }}>
                                    Hôm nay
                                </span>
                                <div style={{
                                    position: 'absolute', top: '50%', left: 0, right: 0,
                                    height: '0.5px', background: '#ebebeb', zIndex: 0,
                                }} />
                            </div>

                            {(messages || []).map(msg => (
                                <MessageBubble key={msg.id} msg={msg} currentUserId={currentUserId} />
                            ))}
                            <div ref={messagesEndRef} />
                        </>
                    )}
                </div>

                {/* Input */}
                <div style={{ padding: '16px 24px', borderTop: '1px solid #e2e8f0', background: '#fff' }}>
                    <div className="input-focus-ring" style={{
                        display: 'flex', alignItems: 'center', gap: 8,
                        border: '1px solid #cbd5e1', borderRadius: 12,
                        padding: '8px 12px', background: '#f8fafc',
                    }}>
                        <Button icon={<PaperClipOutlined />} type="text" size="small" style={{ color: '#64748b' }} />
                        <Input
                            variant="borderless"
                            placeholder={`Nhắn tin tới #${activeChannel?.name}...`}
                            style={{ flex: 1, fontSize: 14, background: 'transparent', padding: 0 }}
                            value={inputValue}
                            onChange={e => setInputValue(e.target.value)}
                            onKeyDown={handleKeyDown}
                        />
                        <Button icon={<SmileOutlined />} type="text" size="small" style={{ color: '#64748b' }} />
                        <Button
                            type="primary"
                            icon={<SendOutlined />}
                            size="middle"
                            onClick={handleSend}
                            style={{ borderRadius: 8, boxShadow: '0 2px 4px rgba(24, 95, 165, 0.2)' }}
                        />
                    </div>
                </div>
                    </>
                )}
            </Content>

            {/* Create Channel Modal */}
            <Modal
                title="Tạo kênh mới"
                open={isCreateChannelModalOpen}
                onCancel={() => setIsCreateChannelModalOpen(false)}
                footer={[
                    <Button key="cancel" onClick={() => setIsCreateChannelModalOpen(false)}>
                        Hủy
                    </Button>,
                    <Button key="submit" type="primary" htmlType="submit" form="createChannelForm">
                        Tạo kênh
                    </Button>,
                ]}
            >
                <Form
                    id="createChannelForm"
                    layout="vertical"
                    onFinish={async (values) => {
                        try {
                            await chatApi.createChannel({
                                name: values.name,
                                description: values.description,
                                memberUserIDs: values.memberUserIDs || [],
                            });
                            message.success('Đã tạo kênh thành công');
                            setIsCreateChannelModalOpen(false);
                            // Reload channels
                            const allChannels = await chatApi.getChannels();
                            const groupChannels = allChannels
                                .filter(c => c.type === 'Group')
                                .map(mapChannelDtoToChannel);
                            setChannels(groupChannels);
                            if (groupChannels.length > 0 && !activeChannelId) {
                                setActiveChannelId(groupChannels[0].id);
                                setActiveChannelType('Group');
                            }
                        } catch (error) {
                            message.error('Không thể tạo kênh');
                            console.error('Error creating channel:', error);
                        }
                    }}
                >
                    <Form.Item
                        label="Tên kênh"
                        name="name"
                        rules={[{ required: true, message: 'Vui lòng nhập tên kênh' }]}
                    >
                        <AntInput placeholder="Nhập tên kênh..." />
                    </Form.Item>
                    <Form.Item
                        label="Mô tả"
                        name="description"
                    >
                        <AntInput.TextArea placeholder="Nhập mô tả (tùy chọn)..." rows={2} />
                    </Form.Item>
                    <Form.Item
                        label="Thêm thành viên khi tạo"
                        name="memberUserIDs"
                    >
                        <Select
                            mode="multiple"
                            placeholder="Chọn thành viên để thêm..."
                            style={{ width: '100%' }}
                            options={usersLookup.map(u => ({
                                label: `${u.fullName} ${u.department ? `(${u.department})` : ''}`,
                                value: u.userID || (u as any).userId
                            }))}
                            filterOption={(input, option) =>
                                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                            }
                        />
                    </Form.Item>
                </Form>
            </Modal>

            {/* Add Member Modal */}
            <Modal
                title="Thêm thành viên vào kênh"
                open={isAddMemberModalOpen}
                onCancel={() => setIsAddMemberModalOpen(false)}
                onOk={async () => {
                    if (!activeChannelId || selectedUserIds.length === 0) return;
                    try {
                        await chatApi.addMembers(activeChannelId, selectedUserIds);
                        message.success('Thêm thành viên thành công!');
                        setIsAddMemberModalOpen(false);
                        setSelectedUserIds([]);
                        // Reload members
                        const membersData = await chatApi.getMembers(activeChannelId);
                        setMembers(membersData);
                    } catch (error: any) {
                        message.error(error.response?.data || 'Không thể thêm thành viên. Chỉ Admin của kênh mới được thêm!');
                        console.error('Error adding members:', error);
                    }
                }}
                okText="Thêm"
                cancelText="Hủy"
            >
                <div style={{ padding: '10px 0' }}>
                    <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
                        Chọn những nhân viên bạn muốn thêm vào kênh <strong>#{activeChannel?.name}</strong>.
                    </Text>
                    <Select
                        mode="multiple"
                        placeholder="Chọn thành viên..."
                        style={{ width: '100%' }}
                        value={selectedUserIds}
                        onChange={setSelectedUserIds}
                        options={(usersLookup || [])
                            .filter(u => {
                                const uId = u.userID || (u as any).userId;
                                return !(members || []).some(m => m.userID === uId);
                            })
                            .map(u => ({
                                label: `${u.fullName} ${u.department ? `(${u.department})` : ''}`,
                                value: u.userID || (u as any).userId
                            }))}
                        filterOption={(input, option) =>
                            (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                        }
                    />
                </div>
            </Modal>

            {/* Create Direct Message Modal */}
            <Modal
                title="Bắt đầu cuộc hội thoại mới"
                open={isCreateDirectModalOpen}
                onCancel={() => setIsCreateDirectModalOpen(false)}
                onOk={async () => {
                    if (!selectedDirectUserId) return;
                    try {
                        const newChannel = await chatApi.createDirect(selectedDirectUserId);
                        message.success('Đã tạo cuộc hội thoại trực tiếp!');
                        setIsCreateDirectModalOpen(false);
                        setSelectedDirectUserId(null);
                        
                        // Reload and map channels
                        const allChannels = await chatApi.getChannels();
                        
                        const groupChannels = allChannels
                            .filter(c => c.type === 'Group')
                            .map(mapChannelDtoToChannel);
                        setChannels(groupChannels);
                        
                        const directChannels = allChannels.filter(c => c.type === 'Direct');
                        const mappedDirects = directChannels.map(d => {
                            const { color, bg } = getColorForUser(d.name.substring(0, 2).toUpperCase());
                            return {
                                id: d.chatChannelID,
                                name: d.name,
                                initials: d.name.substring(0, 2).toUpperCase(),
                                color,
                                bg,
                                online: false,
                            };
                        });
                        setDirects(mappedDirects);
                        
                        // Select new channel
                        setActiveChannelId(newChannel.chatChannelID);
                        setActiveChannelType('Direct');
                    } catch (error) {
                        message.error('Không thể bắt đầu chat trực tiếp');
                        console.error('Error starting direct chat:', error);
                    }
                }}
                okText="Chat"
                cancelText="Hủy"
            >
                <div style={{ padding: '10px 0' }}>
                    <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
                        Chọn một nhân viên từ danh sách để bắt đầu nhắn tin 1-1.
                    </Text>
                    <Select
                        showSearch
                        placeholder="Tìm kiếm nhân viên..."
                        style={{ width: '100%' }}
                        value={selectedDirectUserId}
                        onChange={setSelectedDirectUserId}
                        options={(usersLookup || [])
                            .filter(u => {
                                const uId = u.userID || (u as any).userId;
                                if (uId === currentUserId) return false;
                                return !(directs || []).some(d => {
                                    if (d.name && d.name.startsWith('DM-')) {
                                        const parts = d.name.split('-');
                                        if (parts.length >= 3) {
                                            const id1 = parseInt(parts[1], 10);
                                            const id2 = parseInt(parts[2], 10);
                                            return id1 === uId || id2 === uId;
                                        }
                                    }
                                    return d.name === u.fullName;
                                });
                            })
                            .map(u => ({
                                label: `${u.fullName} ${u.department ? `(${u.department})` : ''}`,
                                value: u.userID || (u as any).userId
                            }))}
                        filterOption={(input, option) =>
                            (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                        }
                    />
                </div>
            </Modal>

            {/* Channel Members List Modal */}
            <Modal
                title={`Thành viên kênh #${activeChannel?.name}`}
                open={isMembersListModalOpen}
                onCancel={() => setIsMembersListModalOpen(false)}
                footer={[
                    <Button key="close" type="primary" onClick={() => setIsMembersListModalOpen(false)}>
                        Đóng
                    </Button>
                ]}
            >
                <div style={{ maxHeight: '400px', overflowY: 'auto', padding: '10px 0' }}>
                    <List
                        itemLayout="horizontal"
                        dataSource={members || []}
                        renderItem={(member) => {
                            const { color, bg } = getColorForUser(member.initials || '??');
                            return (
                                <List.Item>
                                    <List.Item.Meta
                                        avatar={
                                            <div style={{ position: 'relative' }}>
                                                <Avatar style={{ background: bg, color: color, fontWeight: 500 }}>
                                                    {member.initials}
                                                </Avatar>
                                                <OnlineDot online={member.isOnline} />
                                            </div>
                                        }
                                        title={<Text strong>{member.fullName}</Text>}
                                        description={member.isOnline ? "Đang trực tuyến" : "Ngoại tuyến"}
                                    />
                                </List.Item>
                            );
                        }}
                    />
                </div>
            </Modal>

            {/* Config Notification Modal */}
            <Modal
                title={
                    <Space>
                        <SettingOutlined style={{ color: '#4f46e5' }} />
                        <span style={{ fontWeight: 600 }}>Cấu hình thông báo cá nhân</span>
                    </Space>
                }
                open={isSettingsModalOpen}
                onCancel={() => setIsSettingsModalOpen(false)}
                onOk={() => {
                    localStorage.setItem('chat_settings', JSON.stringify(chatSettings));
                    message.success('Đã cập nhật cấu hình thông báo toàn hệ thống!');
                    setIsSettingsModalOpen(false);
                }}
                okText="Lưu cấu hình"
                cancelText="Hủy"
                destroyOnClose
            >
                <div style={{ padding: '16px 0' }}>
                    {/* 1. Thời gian tắt bong bóng */}
                    <div style={{ marginBottom: 20 }}>
                        <Text strong style={{ display: 'block', marginBottom: 6, fontSize: 13 }}>
                            Thời gian tự tắt của bong bóng thông báo (Toast)
                        </Text>
                        <Select
                            style={{ width: '100%' }}
                            value={chatSettings.duration}
                            onChange={(val) => setChatSettings({ ...chatSettings, duration: val })}
                            options={[
                                { label: 'Tắt thông báo hoàn hoàn', value: 0 },
                                { label: '2 giây (Ẩn nhanh)', value: 2 },
                                { label: '4.5 giây (Mặc định)', value: 4.5 },
                                { label: '7 giây (Ẩn chậm)', value: 7 },
                                { label: '10 giây (Ẩn rất chậm)', value: 10 },
                                { label: 'Giữ hiển thị vô hạn (Không tự động tắt)', value: -1 },
                            ]}
                        />
                    </div>

                    <div style={{ borderBottom: '1px solid #f1f5f9', margin: '16px 0' }} />

                    {/* 2. Chế độ riêng tư */}
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                        <div>
                            <Text strong style={{ display: 'block', fontSize: 13 }}>
                                Chế độ riêng tư (Ẩn nội dung)
                            </Text>
                            <Text type="secondary" style={{ fontSize: 11.5 }}>
                                Chỉ thông báo "Bạn có tin nhắn mới" thay vì hiện văn bản chi tiết.
                            </Text>
                        </div>
                        <Switch 
                            checked={chatSettings.privacy} 
                            onChange={(checked) => setChatSettings({ ...chatSettings, privacy: checked })} 
                        />
                    </div>

                    {/* 3. Âm thanh thông báo */}
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                        <div>
                            <Text strong style={{ display: 'block', fontSize: 13 }}>
                                Phát âm thanh thông báo
                            </Text>
                            <Text type="secondary" style={{ fontSize: 11.5 }}>
                                Phát tiếng chuông kép thanh lịch mỗi khi nhận được tin nhắn mới.
                            </Text>
                        </div>
                        <Switch 
                            checked={chatSettings.sound} 
                            onChange={(checked) => setChatSettings({ ...chatSettings, sound: checked })} 
                        />
                    </div>

                    {/* 4. Chớp nháy tab */}
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div>
                            <Text strong style={{ display: 'block', fontSize: 13 }}>
                                Chớp nháy tiêu đề Tab trình duyệt
                            </Text>
                            <Text type="secondary" style={{ fontSize: 11.5 }}>
                                Nhấp nháy tab khi nhận tin nhắn lúc đang mở tab ứng dụng khác.
                            </Text>
                        </div>
                        <Switch 
                            checked={chatSettings.tabFlash} 
                            onChange={(checked) => setChatSettings({ ...chatSettings, tabFlash: checked })} 
                        />
                    </div>
                </div>
            </Modal>
        </Layout>
    );
};

export default ChatPage;