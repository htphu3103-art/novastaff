import React, { useState, useEffect } from "react"
import { Layout, Menu, Avatar, Dropdown, Space, Button, Badge, Breadcrumb, Typography, ConfigProvider, App } from "antd"
import {
    LayoutDashboard,
    Building2,
    CheckSquare,
    Calendar,
    CreditCard,
    MessageSquare,
    Bell,
    User,
    LogOut,
    ChevronLeft,
    AlignJustify,
    Settings,
    HelpCircle,
    Shield
} from "lucide-react"
import { useNavigate, useLocation, useOutlet } from "react-router-dom"
import { motion, AnimatePresence } from "framer-motion"
import { useAuth } from "../../contexts/AuthContext"
import { UserRole } from "../../modules/auth/types"
import { signalRService } from "../../modules/chat/services/signalRService"

const { Header, Sider, Content } = Layout;
const { Title, Text } = Typography;

// ─── Design Tokens ────────────────────────────────────────────────────────────
// Spring easing used by Linear, Vercel, Radix UI — feels natural & snappy
const SPRING = "cubic-bezier(0.16, 1, 0.3, 1)";
const EASE_OUT = "cubic-bezier(0.25, 0.46, 0.45, 0.94)";
const SIDEBAR_DURATION = 320; // ms — sidebar width
const TEXT_DURATION = 150;    // ms — label fade (fast, text disappears first)
const CONTENT_DURATION = 200; // ms — page route transition
const MOBILE_BP = 992; // match AntD `lg` (px)

const SIDEBAR_STYLES = `
    /* ── Base: all animated elements use GPU-friendly properties ─────────── */
    .nova-sidebar {
        will-change: width, min-width, max-width, flex, background-color;
        transition: 
            width ${SIDEBAR_DURATION}ms ${SPRING},
            min-width ${SIDEBAR_DURATION}ms ${SPRING},
            max-width ${SIDEBAR_DURATION}ms ${SPRING},
            background-color ${SIDEBAR_DURATION}ms ${EASE_OUT} !important;
    }

    /* ── Menu item base ──────────────────────────────────────────────────── */
    .nova-sidebar .ant-menu-item,
    .nova-sidebar .ant-menu-submenu-title {
        transition:
            background ${150}ms ${EASE_OUT},
            color       ${150}ms ${EASE_OUT},
            transform   ${SIDEBAR_DURATION}ms ${SPRING} !important;
        will-change: transform, background;
    }

    /* ── Label text: max-width trick for smooth clip ─────────────────────── */
    .nova-sidebar .ant-menu-title-content {
        display: inline-block;
        max-width: 160px;
        opacity: 1;
        overflow: hidden;
        white-space: nowrap;
        transform: translateX(0px);
        transition:
            max-width   ${SIDEBAR_DURATION}ms ${SPRING},
            opacity     ${TEXT_DURATION}ms ${EASE_OUT},
            transform   ${SIDEBAR_DURATION}ms ${SPRING};
        will-change: max-width, opacity, transform;
    }

    /* ── Group title ─────────────────────────────────────────────────────── */
    .nova-sidebar .ant-menu-item-group-title {
        overflow: hidden;
        transition:
            max-height  ${SIDEBAR_DURATION}ms ${SPRING},
            opacity     ${TEXT_DURATION}ms ${EASE_OUT},
            padding     ${SIDEBAR_DURATION}ms ${SPRING},
            transform   ${SIDEBAR_DURATION}ms ${SPRING};
        max-height: 48px;
        opacity: 1;
        transform: translateY(0);
    }

    /* ── Icon ─────────────────────────────────────────────────────────────── */
    .nova-sidebar .ant-menu-item .ant-menu-item-icon,
    .nova-sidebar .ant-menu-item-group-list .ant-menu-item .ant-menu-item-icon {
        display: flex !important;
        align-items: center !important;
        justify-content: center !important;
        flex-shrink: 0;
        transition: 
            transform ${SIDEBAR_DURATION}ms ${SPRING},
            margin    ${SIDEBAR_DURATION}ms ${SPRING};
        will-change: transform, margin;
    }

    /* ── EXPANDED state ──────────────────────────────────────────────────── */
    .nova-sidebar:not(.ant-layout-sider-collapsed) .ant-menu-item,
    .nova-sidebar:not(.ant-layout-sider-collapsed) .ant-menu-submenu-title,
    .nova-sidebar:not(.ant-layout-sider-collapsed) .ant-menu-item-group-list .ant-menu-item {
        padding-inline: 20px !important;
        display: flex;
        align-items: center;
    }

    /* ── COLLAPSED: text fades + clips ───────────────────────────────────── */
    .nova-sidebar.ant-layout-sider-collapsed .ant-menu-title-content {
        max-width: 0 !important;
        opacity: 0 !important;
        transform: translateX(-12px) !important;
    }

    /* Group title collapses to nothing */
    .nova-sidebar.ant-layout-sider-collapsed .ant-menu-item-group-title {
        max-height: 0 !important;
        opacity: 0 !important;
        padding: 0 !important;
        transform: translateY(-8px) !important;
    }

    /* Icon centers when collapsed */
    .nova-sidebar.ant-layout-sider-collapsed .ant-menu-item,
    .nova-sidebar.ant-layout-sider-collapsed .ant-menu-submenu-title,
    .nova-sidebar.ant-layout-sider-collapsed .ant-menu-item-group-list .ant-menu-item {
        padding: 0 !important;
        margin-inline: 12px !important;
        width: calc(100% - 24px) !important;
        display: flex !important;
        justify-content: center !important;
        align-items: center !important;
        height: 40px !important;
        border-radius: 12px !important;
    }

    .nova-sidebar.ant-layout-sider-collapsed .ant-menu-item .ant-menu-item-icon,
    .nova-sidebar.ant-layout-sider-collapsed .ant-menu-item-group-list .ant-menu-item .ant-menu-item-icon {
        margin: 0 !important;
        transform: scale(1.15);
    }

    /* ── Hover micro-interaction ─────────────────────────────────────────── */
    .nova-sidebar .ant-menu-item:not(.ant-menu-item-selected):hover .ant-menu-item-icon,
    .nova-sidebar .ant-menu-item-group-list .ant-menu-item:not(.ant-menu-item-selected):hover .ant-menu-item-icon {
        transform: translateY(-1px) scale(1.1);
    }

    /* ── Profile hover ───────────────────────────────────────────────────── */
    .nova-header-profile:hover {
        background: rgba(0, 0, 0, 0.04);
        transform: translateY(-1px);
    }

    /* ── Collapse toggle button ──────────────────────────────────────────── */
    .nova-collapse-btn {
        transition: 
            background ${150}ms ${EASE_OUT},
            transform ${150}ms ${EASE_OUT} !important;
    }
    .nova-collapse-btn:hover {
        background: #e2e8f0 !important;
        transform: scale(1.05);
    }
    .nova-collapse-btn:active {
        transform: scale(0.95);
    }
    .nova-collapse-icon {
        transition: transform ${SIDEBAR_DURATION}ms ${SPRING};
        display: flex;
        align-items: center;
        justify-content: center;
    }
    .nova-collapse-icon.is-collapsed {
        transform: rotate(180deg);
    }

    /* Sidebar inner content transition */
    .nova-sidebar .ant-layout-sider-children {
        display: flex;
        flex-direction: column;
        transition: padding ${SIDEBAR_DURATION}ms ${SPRING};
    }

    /* ── Mobile overlay backdrop ─────────────────────────────────────────── */
    .nova-mobile-backdrop {
        position: fixed;
        inset: 0;
        background: rgba(2, 6, 23, 0.45);
        backdrop-filter: blur(2px);
        -webkit-backdrop-filter: blur(2px);
        z-index: 99; /* below sider (100) */
    }

`;

export default function MainLayout() {
    const navigate = useNavigate()
    const location = useLocation()
    const [collapsed, setCollapsed] = useState(false)
    const [isMobile, setIsMobile] = useState(false)
    const [mobileOpen, setMobileOpen] = useState(false)
    const currentOutlet = useOutlet()

    const { user, isAuthenticated, logout } = useAuth();
    const { notification } = App.useApp();
    const [appNotificationCount, setAppNotificationCount] = useState(0);

    // Smooth Intro Transition State
    const [showIntro, setShowIntro] = useState(() => sessionStorage.getItem('justLoggedIn') === 'true');

    // Global SignalR Connection & Toast Notification
    useEffect(() => {
        if (!isAuthenticated || !user) return;

        let unsubscribeReceiveMessage: (() => void) | null = null;

        const initGlobalSignalR = async () => {
            try {
                // Register global message listener for notification
                unsubscribeReceiveMessage = signalRService.onReceiveMessage((msg) => {
                    const isChatPage = window.location.pathname === '/chat';
                    const isActiveChannel = isChatPage && signalRService.activeChannelId === msg.chatChannelID;
                    const isFromMe = msg.senderUserID === user?.userId;

                    // Chỉ xử lý nếu tin nhắn không thuộc kênh đang chat và không phải do chính mình gửi
                    if (!isActiveChannel && !isFromMe) {
                        setAppNotificationCount((c) => c + 1);
                        // Tải cài đặt nâng cao từ localStorage
                        const saved = localStorage.getItem('chat_settings');
                        const settings = saved ? JSON.parse(saved) : {
                            duration: 4.5,
                            privacy: false,
                            sound: true,
                            tabFlash: true
                        };

                        // 1. Phát âm thanh thông báo qua Web Audio API (Synthesized chime)
                        if (settings.sound) {
                            try {
                                const audioCtx = new (window.AudioContext || (window as any).webkitAudioContext)();
                                const playTone = (freq: number, start: number, duration: number) => {
                                    const osc = audioCtx.createOscillator();
                                    const gainNode = audioCtx.createGain();
                                    osc.connect(gainNode);
                                    gainNode.connect(audioCtx.destination);
                                    osc.type = 'sine';
                                    osc.frequency.setValueAtTime(freq, start);
                                    gainNode.gain.setValueAtTime(0.06, start); // Âm lượng vừa phải thanh lịch
                                    gainNode.gain.exponentialRampToValueAtTime(0.001, start + duration);
                                    osc.start(start);
                                    osc.stop(start + duration);
                                };
                                // Tiếng chuông kép nhẹ nhàng (double chime)
                                playTone(830.61, audioCtx.currentTime, 0.35); // Ab5
                                playTone(1046.50, audioCtx.currentTime + 0.08, 0.3); // C6
                            } catch (e) {
                                console.warn('Synthesized audio error:', e);
                            }
                        }

                        // 2. Chớp nháy tiêu đề Tab trình duyệt nếu tab hiện tại không active
                        if (settings.tabFlash && !document.hasFocus()) {
                            const originalTitle = document.title;
                            let isAlert = false;
                            let count = 0;
                            const interval = setInterval(() => {
                                if (document.hasFocus() || count > 10) {
                                    clearInterval(interval);
                                    document.title = originalTitle;
                                    return;
                                }
                                document.title = isAlert ? `💬 Tin nhắn mới từ ${msg.senderName}...` : originalTitle;
                                isAlert = !isAlert;
                                count++;
                            }, 800);
                        }

                        // 3. Nếu tắt bong bóng thông báo hoàn toàn (duration = 0) thì dừng hiển thị Toast
                        if (settings.duration === 0) return;

                        // 4. Kiểm tra chế độ riêng tư
                        const displayContent = settings.privacy 
                            ? "Đã nhận một tin nhắn mới (Nội dung được ẩn vì bảo mật)" 
                            : msg.content;

                        // 5. Hiển thị Toast thông báo
                        const actualDuration = settings.duration === -1 ? 0 : settings.duration;

                        notification.open({
                            message: (
                                <span style={{ fontWeight: 600, color: '#1e293b', fontSize: 13.5 }}>
                                    Tin nhắn mới từ {msg.senderName}
                                </span>
                            ),
                            description: (
                                <div style={{ color: '#475569', fontSize: 12.5, marginTop: 2, textOverflow: 'ellipsis', overflow: 'hidden', whiteSpace: 'nowrap', maxWidth: 280 }}>
                                    {displayContent}
                                </div>
                            ),
                            placement: 'topRight',
                            duration: actualDuration,
                            icon: (
                                <Avatar 
                                    style={{ background: '#e6f1fb', color: '#185fa5', fontWeight: 600, fontSize: 11 }}
                                    size={32}
                                >
                                    {msg.senderInitials}
                                </Avatar>
                            ),
                            onClick: () => {
                                navigate(`/chat?channelId=${msg.chatChannelID}`);
                            },
                            style: {
                                borderRadius: '12px',
                                cursor: 'pointer',
                                border: '1px solid #f1f5f9',
                                boxShadow: '0 10px 15px -3px rgba(148, 163, 184, 0.12), 0 4px 6px -4px rgba(148, 163, 184, 0.12)',
                                background: '#ffffff',
                                padding: '12px 16px',
                            }
                        });
                    }
                });

                await signalRService.connect();
            } catch (error) {
                console.error('[SignalR Global] Failed to connect:', error);
            }
        };

        initGlobalSignalR();

        return () => {
            unsubscribeReceiveMessage?.();
            signalRService.disconnect();
        };
    }, [isAuthenticated, user?.userId, navigate]);

    useEffect(() => {
        if (!isAuthenticated || !user) navigate('/login');
    }, [user, isAuthenticated, navigate]);

    useEffect(() => {
        const mq = window.matchMedia(`(max-width: ${MOBILE_BP - 0.02}px)`);
        const apply = () => {
            const nextIsMobile = mq.matches;
            setIsMobile(nextIsMobile);
            if (!nextIsMobile) setMobileOpen(false);
        };
        apply();
        if (typeof mq.addEventListener === 'function') mq.addEventListener('change', apply);
        else mq.addListener(apply);
        return () => {
            if (typeof mq.removeEventListener === 'function') mq.removeEventListener('change', apply);
            else mq.removeListener(apply);
        };
    }, []);

    useEffect(() => {
        if (!isMobile) return;
        document.body.style.overflow = mobileOpen ? 'hidden' : '';
        return () => {
            document.body.style.overflow = '';
        };
    }, [isMobile, mobileOpen]);

    useEffect(() => {
        if (showIntro) {
            document.body.style.overflow = 'hidden'; // Lock scroll during intro
            sessionStorage.removeItem('justLoggedIn');
            const timer = setTimeout(() => {
                setShowIntro(false);
                setTimeout(() => {
                    document.body.style.backgroundColor = '';
                    document.body.style.overflow = ''; // Unlock scroll
                }, 800); // Revert body styles after exit animation
            }, 800); // Wait briefly before sliding away
            return () => clearTimeout(timer);
        } else {
            document.body.style.backgroundColor = '';
            document.body.style.overflow = '';
        }
    }, [showIntro]);

    if (!user) return null;

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const closeMobileNav = () => setMobileOpen(false);

    const chatMenuLabel = (
        <span style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 10 }}>
            <span>Trò chuyện</span>
            {!collapsed && appNotificationCount > 0 && (
                <Badge
                    count={appNotificationCount}
                    size="small"
                    overflowCount={99}
                    color="#818cf8"
                    style={{ boxShadow: 'none' }}
                />
            )}
        </span>
    );

    const menuItems = [
        {
            key: "/",
            icon: <LayoutDashboard size={18} strokeWidth={1.75} />,
            label: "Dashboard"
        },
        { type: 'divider', style: { background: 'rgba(255,255,255,0.06)', margin: '8px 0' } },

        // QUẢN LÝ group (Admin / Manager only)
        ...((user.role === UserRole.Admin || user.role === UserRole.Manager) ? [{
            key: 'admin-group',
            label: <span style={{ color: 'rgba(255,255,255,0.35)', fontSize: 10, fontWeight: 700, letterSpacing: '0.08em' }}>QUẢN LÝ</span>,
            type: 'group',
            children: [
                { key: "/departments", icon: <Building2 size={18} strokeWidth={1.75} />, label: "Phòng ban" },
            ]
        }] : []) as any,

        // CÔNG VIỆC group
        {
            key: 'work-group',
            label: <span style={{ color: 'rgba(255,255,255,0.35)', fontSize: 10, fontWeight: 700, letterSpacing: '0.08em' }}>CÔNG VIỆC</span>,
            type: 'group',
            children: [
                { key: "/tasks", icon: <CheckSquare size={18} strokeWidth={1.75} />, label: "Công việc" },
                { key: "/attendance", icon: <Calendar size={18} strokeWidth={1.75} />, label: "Chấm công" },
                { key: "/payroll", icon: <CreditCard size={18} strokeWidth={1.75} />, label: "Bảng lương" },
                { key: "/chat", icon: <MessageSquare size={18} strokeWidth={1.75} />, label: chatMenuLabel },
            ]
        }
    ];

    const breadcrumbItems = location.pathname
        .split('/')
        .filter(Boolean)
        .map(seg => ({ title: seg.charAt(0).toUpperCase() + seg.slice(1) }));

    return (
        <ConfigProvider
            theme={{
                token: {
                    colorPrimary: '#4f46e5',
                    borderRadius: 12,
                    fontFamily: "'Inter', sans-serif"
                },
                components: {
                    Layout: {
                        siderBg: '#1e1b4b',
                        headerHeight: 64,
                        headerBg: '#ffffff'
                    },
                    Menu: {
                        darkItemBg: '#1e1b4b',
                        darkItemColor: 'rgba(255,255,255,0.55)',
                        darkItemHoverColor: '#ffffff',
                        darkItemSelectedColor: '#ffffff',
                        darkItemSelectedBg: '#4f46e5',
                        itemBorderRadius: 10,
                        itemMarginInline: 12,
                        itemMarginBlock: 3,
                        groupTitleColor: 'rgba(255,255,255,0.35)',
                    }
                }
            }}
        >
            {/* Inject animation styles once */}
            <style>{SIDEBAR_STYLES}</style>

            <AnimatePresence>
                {showIntro && (
                    <motion.div
                        initial={{ top: 0 }}
                        exit={{ top: '-100%' }}
                        transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
                        style={{
                            position: 'fixed',
                            top: 0,
                            left: 0,
                            right: 0,
                            height: '100vh',
                            background: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 100%)',
                            zIndex: 99999, // Ensure it covers everything
                            display: 'flex',
                            flexDirection: 'column',
                            alignItems: 'center',
                            justifyContent: 'center',
                            overflow: 'hidden'
                        }}
                    >
                        <motion.div
                            exit={{ opacity: 0, scale: 0.8 }}
                            transition={{ duration: 0.4 }}
                            style={{
                                width: 80, height: 80,
                                background: 'rgba(255,255,255,0.1)',
                                border: '1px solid rgba(255,255,255,0.2)',
                                borderRadius: '20px',
                                display: 'flex', alignItems: 'center', justifyContent: 'center',
                                marginBottom: 24,
                                boxShadow: '0 0 40px rgba(99, 102, 241, 0.4)'
                            }}
                        >
                            <Shield size={40} color="#fff" strokeWidth={1.5} />
                        </motion.div>
                        <motion.div
                            exit={{ opacity: 0, y: -20 }}
                            transition={{ duration: 0.4 }}
                        >
                            <h3 style={{ color: '#fff', margin: 0, fontSize: 24, fontWeight: 600, letterSpacing: '-0.5px', fontFamily: "'Inter', sans-serif" }}>
                                Welcome to NovaStaff
                            </h3>
                        </motion.div>
                    </motion.div>
                )}
            </AnimatePresence>

            <App>
                <Layout style={{ minHeight: "100vh", background: "#f8fafc" }}>

                    {/* Mobile overlay backdrop */}
                    {isMobile && mobileOpen && (
                        <div
                            className="nova-mobile-backdrop"
                            onClick={closeMobileNav}
                            aria-hidden="true"
                        />
                    )}

                    {/* ── Sidebar ──────────────────────────────────────────── */}
                    <Sider
                        trigger={null}
                        collapsible
                        collapsed={isMobile ? !mobileOpen : collapsed}
                        width={isMobile ? 260 : 200}
                        collapsedWidth={isMobile ? 0 : 68}
                        className="nova-sidebar"
                        style={{
                            height: '100vh',
                            position: 'fixed',
                            left: 0, top: 0, bottom: 0,
                            zIndex: 100,
                            boxShadow: '4px 0 32px rgba(0,0,0,0.08)',
                            borderRight: '1px solid rgba(255,255,255,0.04)',
                            overflow: 'hidden',
                        }}
                    >
                        {/* Logo area */}
                        <div style={{
                            height: 72,
                            padding: '0 18px',
                            display: 'flex',
                            alignItems: 'center',
                            gap: 12,
                            overflow: 'hidden',
                            flexShrink: 0,
                            borderBottom: '1px solid rgba(255,255,255,0.05)',
                            marginBottom: 8,
                        }}>
                            {/* Icon — always visible, never moves */}
                            <div style={{
                                width: 32, height: 32, flexShrink: 0,
                                background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                                borderRadius: 10,
                                display: 'flex', alignItems: 'center', justifyContent: 'center',
                                color: 'white',
                                boxShadow: '0 4px 14px rgba(79,70,229,0.35)',
                            }}>
                                <Building2 size={18} />
                            </div>

                            {/* Text — fades & clips when collapsed */}
                            <div style={{
                                overflow: 'hidden',
                                maxWidth: collapsed ? 0 : 120,
                                opacity: collapsed ? 0 : 1,
                                transform: collapsed ? 'translateX(-8px)' : 'translateX(0)',
                                transition: `
                                    max-width ${TEXT_DURATION}ms ${EASE_OUT},
                                    opacity   ${TEXT_DURATION}ms ${EASE_OUT},
                                    transform ${TEXT_DURATION}ms ${EASE_OUT}
                                `,
                                whiteSpace: 'nowrap',
                                willChange: 'max-width, opacity, transform',
                            }}>
                                <div style={{ color: '#fff', fontSize: 15, fontWeight: 700, letterSpacing: '-0.02em', lineHeight: 1.2 }}>NOVA</div>
                                <div style={{ color: '#818cf8', fontSize: 9, fontWeight: 800, letterSpacing: '0.12em' }}>STAFF SYSTEM</div>
                            </div>
                        </div>

                        {/* Menu */}
                        <Menu
                            theme="dark"
                            mode="inline"
                            selectedKeys={[location.pathname]}
                            items={menuItems}
                            onClick={({ key }) => {
                                if (key === '/chat') {
                                    setAppNotificationCount(0);
                                }
                                navigate(key);
                                if (isMobile) closeMobileNav();
                            }}
                            style={{ border: 'none', background: 'transparent', paddingTop: 4 }}
                        />

                        {/* Help card — fades out when collapsed */}
                        <div style={{
                            position: 'absolute', bottom: 20, left: 0, width: '100%',
                            padding: '0 12px',
                            opacity: collapsed ? 0 : 1,
                            transform: collapsed ? 'translateY(12px) scale(0.94)' : 'translateY(0) scale(1)',
                            transformOrigin: 'bottom center',
                            transition: `
                                opacity   ${TEXT_DURATION}ms ${EASE_OUT},
                                transform ${TEXT_DURATION}ms ${EASE_OUT}
                            `,
                            pointerEvents: collapsed ? 'none' : 'auto',
                            willChange: 'opacity, transform',
                        }}>
                            <div style={{
                                background: 'rgba(99,102,241,0.08)',
                                borderRadius: 12,
                                padding: '14px 16px',
                                border: '1px solid rgba(99,102,241,0.18)'
                            }}>
                                <div style={{ color: '#fff', fontSize: 12, fontWeight: 600, marginBottom: 3 }}>Need Help?</div>
                                <div style={{ color: 'rgba(255,255,255,0.45)', fontSize: 11, marginBottom: 10 }}>
                                    Check our docs for more info.
                                </div>
                                <Button
                                    type="primary" ghost size="small" block
                                    style={{ fontSize: 11, height: 26, borderColor: 'rgba(255,255,255,0.18)', color: '#fff' }}
                                >
                                    Documentation
                                </Button>
                            </div>
                        </div>
                    </Sider>

                    {/* ── Main content area ─────────────────────────────────── */}
                    <Layout style={{
                        marginLeft: isMobile ? 0 : (collapsed ? 68 : 200),
                        transition: `margin-left ${SIDEBAR_DURATION}ms ${SPRING}`,
                        willChange: 'margin-left',
                        display: 'flex',
                        flexDirection: 'column',
                        height: '100vh',
                    }}>
                        {/* Header */}
                        <Header style={{
                            background: "rgba(255,255,255,0.92)",
                            backdropFilter: 'blur(12px)',
                            WebkitBackdropFilter: 'blur(12px)',
                            padding: isMobile ? '0 12px' : '0 24px',
                            display: "flex",
                            justifyContent: "space-between",
                            alignItems: "center",
                            flexShrink: 0,
                            zIndex: 90,
                            borderBottom: '1px solid rgba(226,232,240,0.8)',
                        }}>
                            <Space size={16}>
                                {/* Mobile: hamburger / Desktop: collapse chevron */}
                                <button
                                    onClick={() => {
                                        if (isMobile) setMobileOpen(v => !v);
                                        else setCollapsed(v => !v);
                                    }}
                                    className="nova-collapse-btn"
                                    style={{
                                        display: 'flex', alignItems: 'center', justifyContent: 'center',
                                        width: 32, height: 32,
                                        background: '#f1f5f9',
                                        border: 'none', borderRadius: 8,
                                        cursor: 'pointer', color: '#64748b',
                                    }}
                                >
                                    {isMobile ? (
                                        <AlignJustify size={16} strokeWidth={2} />
                                    ) : (
                                        <span className={`nova-collapse-icon${collapsed ? ' is-collapsed' : ''}`}>
                                            <ChevronLeft size={16} strokeWidth={2} />
                                        </span>
                                    )}
                                </button>

                                {!isMobile && (
                                    <Breadcrumb
                                        items={[{ title: <LayoutDashboard size={14} color="#94a3b8" /> }, ...breadcrumbItems]}
                                        separator={<span style={{ color: '#cbd5e1' }}>/</span>}
                                    />
                                )}
                            </Space>

                            <Space size={12}>
                                <Button
                                    type="text"
                                    icon={<Calendar size={15} strokeWidth={1.75} />}
                                    style={{
                                        color: '#64748b',
                                        fontSize: 13,
                                        fontWeight: 500,
                                        display: 'flex',
                                        alignItems: 'center',
                                        background: '#f1f5f9',
                                        borderRadius: 8,
                                        height: 32,
                                        padding: '0 12px'
                                    }}
                                >
                                    May 13, 2026
                                </Button>

                                <Badge count={appNotificationCount} size="small" offset={[-2, 2]} color="#6366f1">
                                    <Button
                                        type="text" shape="circle"
                                        icon={<Bell size={19} strokeWidth={1.75} />}
                                        style={{ color: '#64748b' }}
                                        onClick={() => {
                                            setAppNotificationCount(0);
                                            navigate('/chat');
                                        }}
                                    />
                                </Badge>

                                <div style={{ width: 1, height: 22, background: '#e2e8f0' }} />

                                <Dropdown
                                    menu={{
                                        items: [
                                            { key: 'profile', icon: <User size={15} />, label: 'Thông tin cá nhân' },
                                            { key: 'settings', icon: <Settings size={15} />, label: 'Cài đặt' },
                                            { key: 'help', icon: <HelpCircle size={15} />, label: 'Trợ giúp' },
                                            { type: 'divider' },
                                            { key: 'logout', icon: <LogOut size={15} />, label: 'Đăng xuất', danger: true, onClick: handleLogout }
                                        ]
                                    }}
                                    placement="bottomRight"
                                    arrow
                                >
                                    <div
                                        className="nova-header-profile"
                                        style={{
                                            cursor: 'pointer',
                                            padding: '4px 8px',
                                            borderRadius: 10,
                                            transition: `background 150ms ${EASE_OUT}`,
                                            display: 'flex', alignItems: 'center', gap: 10,
                                        }}
                                    >
                                        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', lineHeight: 1.3 }}>
                                            <Text strong style={{ fontSize: 13, color: '#0f172a' }}>
                                                {user.displayName || 'Người dùng'}
                                            </Text>
                                            <Text style={{ fontSize: 10, color: '#6366f1', textTransform: 'uppercase', fontWeight: 700, letterSpacing: '0.06em' }}>
                                                {user.role}
                                            </Text>
                                        </div>
                                        <Avatar
                                            size={34}
                                            style={{
                                                background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                                                boxShadow: '0 4px 12px rgba(79,70,229,0.25)',
                                                border: '2px solid #fff',
                                                flexShrink: 0,
                                            }}
                                            icon={!user.displayName && <User size={16} />}
                                        >
                                            {user.displayName?.charAt(0)}
                                        </Avatar>
                                    </div>
                                </Dropdown>
                            </Space>
                        </Header>

                        {/* Page content */}
                        <Content style={{ 
                            padding: '0px', 
                            background: '#f8fafc', 
                            flex: 1, 
                            overflow: location.pathname === '/chat' ? 'hidden' : 'auto',
                            scrollbarGutter: 'stable',
                            display: 'flex',
                            flexDirection: 'column',
                            position: 'relative'
                        }}>
                            {/* Senior Top Progress Indicator */}
                            <motion.div
                                key={`progress-${location.pathname}`}
                                initial={{ width: "0%", opacity: 1 }}
                                animate={{ width: "100%", opacity: 0 }}
                                transition={{ duration: 0.6, ease: "circOut" }}
                                style={{
                                    position: 'absolute',
                                    top: 0,
                                    left: 0,
                                    height: '2px',
                                    background: 'linear-gradient(90deg, #4f46e5, #818cf8)',
                                    zIndex: 1000,
                                    boxShadow: '0 0 8px rgba(79, 70, 229, 0.4)'
                                }}
                            />

                            <AnimatePresence mode="wait">
                                <motion.div
                                    key={location.pathname}
                                    initial={{ opacity: 0, y: 12, scale: 0.98, filter: 'blur(8px)' }}
                                    animate={{ opacity: 1, y: 0, scale: 1, filter: 'blur(0px)' }}
                                    exit={{ opacity: 0, y: -12, scale: 1.02, filter: 'blur(8px)' }}
                                    transition={{ 
                                        duration: 0.3, 
                                        ease: [0.16, 1, 0.3, 1] 
                                    }}
                                    style={{ 
                                        padding: location.pathname === '/chat' ? '0px' : '16px 24px 24px', 
                                        flex: 1,
                                        width: '100%',
                                        height: '100%',
                                        minHeight: 0,
                                        display: 'flex',
                                        flexDirection: 'column'
                                    }}
                                >
                                    {currentOutlet}
                                </motion.div>
                            </AnimatePresence>
                        </Content>
                    </Layout>
                </Layout>

            </App>
        </ConfigProvider>
    )
}