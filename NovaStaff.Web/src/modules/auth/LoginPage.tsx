import React, { useState } from 'react';
import { Form, Input, Button, Typography, message, Checkbox, Divider } from 'antd';
import {
    User,
    Lock,
    ArrowRight,
    Users,
    BarChart3,
    ClipboardCheck,
    Shield,
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { authApi } from './api/authApi';
import { LoginRequest } from './types';
import { useAuth } from '../../contexts/AuthContext';

const { Title, Text } = Typography;

// Icon components sử dụng Lucide với màu sắc riêng biệt
const IconPeople = () => <Users size={20} strokeWidth={2} color="#60a5fa" />;
const IconChart = () => <BarChart3 size={20} strokeWidth={2} color="#34d399" />;
const IconClipboard = () => <ClipboardCheck size={20} strokeWidth={2} color="#fb923c" />;

const containerVariants: any = {
    hidden: { opacity: 0 },
    visible: {
        opacity: 1,
        transition: { staggerChildren: 0.1, delayChildren: 0.2 },
    },
};

const itemVariants: any = {
    hidden: { opacity: 0, y: 16 },
    visible: { opacity: 1, y: 0, transition: { duration: 0.5, ease: 'easeOut' } },
};

const LoginPage: React.FC = () => {
    const navigate = useNavigate();
    const { login } = useAuth();
    const [loading, setLoading] = useState(false);
    const [isSuccess, setIsSuccess] = useState(false);

    const onFinish = async (values: LoginRequest) => {
        setLoading(true);
        try {
            const response = await authApi.login(values);
            const { accessToken } = response.data;
            localStorage.setItem('token', accessToken);

            const userResponse = await authApi.getCurrentUser();
            login(accessToken, userResponse.data);

            message.success('Welcome back to NovaStaff!');
            setIsSuccess(true);
            sessionStorage.setItem('justLoggedIn', 'true');
            setTimeout(() => {
                document.body.style.backgroundColor = '#1e1b4b'; // Prevent flash
                document.body.style.overflow = 'hidden'; // Prevent scrollbar shift
                navigate('/');
            }, 1200); // Allow animation to finish before navigating
        } catch (error: any) {
            console.error('Login error:', error);
            const errorMsg = error.response?.data?.message || 'Invalid username or password';
            message.error(errorMsg);
            setLoading(false);
        }
    };

    return (
        <div
            style={{
                display: 'flex',
                minHeight: '100vh',
                fontFamily: "'Inter', system-ui, sans-serif",
                background: '#ffffff', // Đảm bảo nền chính đồng nhất để không bị 'lộ' góc cắt
            }}
        >
            {/* ───────── LEFT PANEL (Branding) ───────── */}
            <motion.div
                initial={{ opacity: 0, x: -40 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
                style={{
                    flex: '0 0 38%',
                    background: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 50%, #4338ca 100%)',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'flex-start',
                    padding: '80px 100px 80px 60px',
                    position: 'relative',
                    overflow: 'hidden',
                    borderTopRightRadius: '160px',
                    borderBottomRightRadius: '160px',
                    boxShadow: '60px 0 100px -30px rgba(0,0,0,0.2)',
                    zIndex: 2,
                    borderRight: '1px solid rgba(255,255,255,0.08)',
                }}
                className="login-left-panel"
            >
                {/* Texture Hạt (Senior Touch) */}
                <div style={{
                    position: 'absolute', inset: 0,
                    opacity: 0.35,
                    mixBlendMode: 'overlay',
                    pointerEvents: 'none',
                    backgroundImage: `url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)'/%3E%3C/svg%3E")`,
                }} />

                {/* Organic Glows */}
                <div style={{
                    position: 'absolute', top: '-20%', right: '-10%',
                    width: '80%', height: '80%',
                    background: 'radial-gradient(circle, rgba(99, 102, 241, 0.25) 0%, transparent 70%)',
                    filter: 'blur(100px)', pointerEvents: 'none',
                }} />
                <div style={{
                    position: 'absolute', bottom: '-10%', left: '-10%',
                    width: '60%', height: '60%',
                    background: 'radial-gradient(circle, rgba(139, 92, 246, 0.15) 0%, transparent 70%)',
                    filter: 'blur(80px)', pointerEvents: 'none',
                }} />

                {/* Content */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.3, duration: 0.6 }}
                    style={{ position: 'relative', zIndex: 1 }}
                >
                    <motion.div
                        style={{
                            width: 44, height: 44,
                            background: 'linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0.05) 100%)',
                            border: '1px solid rgba(255, 255, 255, 0.25)',
                            borderRadius: '12px',
                            display: 'flex', alignItems: 'center', justifyContent: 'center',
                            marginBottom: 32,
                            boxShadow: '0 10px 30px -5px rgba(0, 0, 0, 0.3)',
                        }}
                    >
                        <Shield size={24} strokeWidth={1.5} color="#fff" />
                    </motion.div>

                    <Title level={1} style={{
                        color: '#fff', margin: 0, fontWeight: 700,
                        fontSize: 40, letterSpacing: '-2px', lineHeight: 1.1,
                    }}>
                        Empower your<br />
                        <span style={{
                            background: 'linear-gradient(to right, #c7d2fe, #818cf8)',
                            WebkitBackgroundClip: 'text',
                            WebkitTextFillColor: 'transparent'
                        }}>Talent.</span>
                    </Title>
                    <Text style={{
                        color: 'rgba(255, 255, 255, 0.6)', fontSize: 16,
                        display: 'block', marginTop: 16, fontWeight: 400,
                        lineHeight: 1.5, maxWidth: 380, letterSpacing: '-0.2px'
                    }}>
                        Next-gen HR management for modern high-growth teams.
                    </Text>

                    <div style={{ marginTop: 48, position: 'relative', paddingLeft: 8 }}>
                        {/* Dòng kẻ dọc kết nối */}
                        <div style={{
                            position: 'absolute', left: 24, top: 20, bottom: 20,
                            width: '1px', background: 'linear-gradient(to bottom, rgba(255,255,255,0.2) 0%, rgba(255,255,255,0.05) 100%)',
                        }} />

                        {[
                            { icon: <IconPeople />, title: 'Workforce OS', desc: 'Centralized talent management', accent: 'rgba(96, 165, 250, 0.4)' },
                            { icon: <IconChart />, title: 'FinOps Payroll', desc: 'Automated tax & benefit tracking', accent: 'rgba(52, 211, 153, 0.4)' },
                            { icon: <IconClipboard />, title: 'Flow Automator', desc: 'Custom leave & approval engine', accent: 'rgba(251, 146, 60, 0.4)' },
                        ].map((item, i) => (
                            <motion.div
                                key={i}
                                initial={{ opacity: 0, x: -20 }}
                                animate={{ opacity: 1, x: 0 }}
                                transition={{ delay: 0.5 + i * 0.1, duration: 0.5 }}
                                style={{
                                    display: 'flex', alignItems: 'flex-start', gap: 24,
                                    marginBottom: 32,
                                    position: 'relative',
                                }}
                            >
                                <div style={{
                                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                                    width: 32, height: 32, borderRadius: '50%', flexShrink: 0,
                                    background: '#1e1b4b',
                                    border: '1px solid rgba(255,255,255,0.2)',
                                    zIndex: 2,
                                    boxShadow: `0 0 10px ${item.accent}`,
                                }}>
                                    {React.cloneElement(item.icon as React.ReactElement<any>, { size: 16 })}
                                </div>
                                <div style={{ paddingTop: 4 }}>
                                    <Text style={{ color: '#fff', fontWeight: 600, fontSize: 15, display: 'block', letterSpacing: '-0.3px', lineHeight: 1 }}>
                                        {item.title}
                                    </Text>
                                    <Text style={{ color: 'rgba(255, 255, 255, 0.45)', fontSize: 13, fontWeight: 400, marginTop: 6, display: 'block' }}>
                                        {item.desc}
                                    </Text>
                                </div>
                            </motion.div>
                        ))}
                    </div>
                </motion.div>
            </motion.div>

            {/* ───────── RIGHT PANEL (Form) ───────── */}
            <div
                style={{
                    flex: 1,
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    padding: '20px',
                    background: '#ffffff',
                    position: 'relative',
                }}
            >
                {/* Subtle background element */}
                <div style={{
                    position: 'absolute', top: '10%', right: '5%',
                    width: '300px', height: '300px',
                    background: 'radial-gradient(circle, rgba(79, 70, 229, 0.03) 0%, transparent 70%)',
                    filter: 'blur(40px)', pointerEvents: 'none',
                }} />

                <motion.div
                    variants={containerVariants}
                    initial="hidden"
                    animate="visible"
                    style={{
                        width: '100%',
                        maxWidth: 420,
                        background: '#fff',
                        padding: '36px 40px',
                        borderRadius: '24px',
                        border: '1.5px solid #e2e8f0',
                        boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.08), 0 0 0 1px rgba(0, 0, 0, 0.02)',
                        position: 'relative',
                        zIndex: 1,
                    }}
                >
                    {/* Header */}
                    <motion.div variants={itemVariants} style={{ marginBottom: 24 }}>
                        <Title level={2} style={{
                            margin: 0, fontWeight: 700,
                            color: '#0f172a', letterSpacing: '-0.8px',
                            fontSize: 26,
                        }}>
                            Welcome back
                        </Title>
                        <Text style={{ color: '#64748b', fontSize: 14, marginTop: 4, display: 'block', fontWeight: 400 }}>
                            Sign in to manage your NovaStaff workspace
                        </Text>
                    </motion.div>

                    {/* Social Login Buttons */}
                    <motion.div variants={itemVariants} style={{ display: 'flex', gap: 10, marginBottom: 20 }}>
                        <Button
                            size="large"
                            icon={<svg width="18" height="18" viewBox="0 0 24 24">
                                <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
                                <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
                                <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05" />
                                <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
                            </svg>}
                            style={{
                                flex: 1, height: 42, borderRadius: 12,
                                border: '1px solid #e2e8f0',
                                background: '#fff', color: '#334155',
                                fontWeight: 600, fontSize: 13,
                                display: 'flex', alignItems: 'center', justifyContent: 'center',
                                transition: 'all 0.2s ease',
                            }}
                        >
                            Google
                        </Button>
                        <Button
                            size="large"
                            icon={<svg width="18" height="18" viewBox="0 0 23 23">
                                <path fill="#f35325" d="M1 1h10v10H1z" />
                                <path fill="#81bc06" d="M12 1h10v10H12z" />
                                <path fill="#05a6f0" d="M1 12h10v10H1z" />
                                <path fill="#ffba08" d="M12 12h10v10H12z" />
                            </svg>}
                            style={{
                                flex: 1, height: 42, borderRadius: 12,
                                border: '1px solid #e2e8f0',
                                background: '#fff', color: '#334155',
                                fontWeight: 600, fontSize: 13,
                                display: 'flex', alignItems: 'center', justifyContent: 'center',
                                transition: 'all 0.2s ease',
                            }}
                        >
                            Microsoft
                        </Button>
                    </motion.div>

                    {/* Divider */}
                    <motion.div variants={itemVariants}>
                        <Divider style={{ color: '#64748b', fontSize: 12, borderColor: '#cbd5e1', margin: '0 0 16px' }}>
                            or continue with username
                        </Divider>
                    </motion.div>

                    {/* Login Form */}
                    <motion.div variants={itemVariants}>
                        <Form
                            name="login"
                            onFinish={onFinish}
                            layout="vertical"
                            requiredMark={false}
                            size="large"
                        >
                            <Form.Item
                                label={<Text style={{ fontWeight: 600, color: '#475569', fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.5px' }}>Email</Text>}
                                name="username"
                                rules={[{ required: true, message: 'Please enter your username' }]}
                                style={{ marginBottom: 14 }}
                            >
                                <Input
                                    prefix={<User size={16} color="#64748b" style={{ marginRight: 6 }} />}
                                    placeholder="yourname@company.com"
                                    style={{
                                        borderRadius: 10, padding: '10px 14px',
                                        border: '1.5px solid #e2e8f0',
                                        fontSize: 14,
                                        background: '#f8fafc',
                                        transition: 'all 0.2s',
                                    }}
                                />
                            </Form.Item>

                            <Form.Item
                                label={<Text style={{ fontWeight: 600, color: '#475569', fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.5px' }}>Password</Text>}
                                name="password"
                                rules={[{ required: true, message: 'Please enter your password' }]}
                                style={{ marginBottom: 10 }}
                            >
                                <Input.Password
                                    prefix={<Lock size={16} color="#64748b" style={{ marginRight: 6 }} />}
                                    placeholder="••••••••"
                                    style={{
                                        borderRadius: 10, padding: '10px 14px',
                                        border: '1.5px solid #e2e8f0',
                                        fontSize: 14,
                                        background: '#f8fafc',
                                        transition: 'all 0.2s',
                                    }}
                                />
                            </Form.Item>

                            <div style={{
                                display: 'flex', justifyContent: 'space-between',
                                alignItems: 'center', marginBottom: 18,
                            }}>
                                <Checkbox>
                                    <Text style={{ fontSize: 14, color: '#111827', fontWeight: 500 }}>Remember me</Text>
                                </Checkbox>
                                <Button
                                    type="link"
                                    style={{ padding: 0, height: 'auto', fontSize: 14, fontWeight: 500, color: '#4f46e5' }}
                                >
                                    Forgot password?
                                </Button>
                            </div>

                            <Form.Item style={{ marginBottom: 0 }}>
                                <Button
                                    type="primary"
                                    htmlType="submit"
                                    block
                                    loading={loading}
                                    icon={<ArrowRight size={18} />}
                                    style={{
                                        height: 48,
                                        borderRadius: 10,
                                        fontSize: 15,
                                        fontWeight: 600,
                                        background: 'linear-gradient(135deg, #4f46e5 0%, #6366f1 100%)',
                                        border: 'none',
                                        boxShadow: '0 10px 20px -5px rgba(79, 70, 229, 0.4)',
                                        letterSpacing: '0.5px',
                                        marginTop: 4,
                                    }}
                                >
                                    Sign In
                                </Button>
                            </Form.Item>
                        </Form>
                    </motion.div>

                    {/* Footer */}
                    <motion.div
                        variants={itemVariants}
                        style={{ textAlign: 'center', marginTop: 22 }}
                    >
                        <Text style={{ color: '#64748b', fontSize: 14 }}>
                            Don't have an account?{' '}
                            <Button
                                type="link"
                                style={{
                                    padding: 0, height: 'auto',
                                    fontWeight: 600, fontSize: 14,
                                    color: '#4f46e5',
                                }}
                            >
                                Contact Admin
                            </Button>
                        </Text>
                    </motion.div>

                    {/* Version badge */}
                    <motion.div
                        variants={itemVariants}
                        style={{ textAlign: 'center', marginTop: 16 }}
                    >
                        <Text style={{ color: '#cbd5e1', fontSize: 12 }}>
                            NovaStaff HR © 2025 · v2.0.0
                        </Text>
                    </motion.div>
                </motion.div>
            </div>

            {/* Success Overlay Transition (Senior Touch) */}
            {isSuccess && (
                <motion.div
                    initial={{ top: '100%' }}
                    animate={{ top: 0 }}
                    transition={{ duration: 0.7, ease: [0.16, 1, 0.3, 1] }}
                    style={{
                        position: 'fixed',
                        left: 0,
                        right: 0,
                        bottom: 0,
                        height: '100vh',
                        background: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 100%)',
                        zIndex: 9999,
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        justifyContent: 'center',
                        overflow: 'hidden'
                    }}
                >
                    <motion.div
                        initial={{ opacity: 0, scale: 0.8 }}
                        animate={{ opacity: 1, scale: 1 }}
                        transition={{ delay: 0.3, duration: 0.5, ease: 'easeOut' }}
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
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.4, duration: 0.5 }}
                    >
                        <h3 style={{ color: '#fff', margin: 0, fontSize: 24, fontWeight: 600, letterSpacing: '-0.5px', fontFamily: "'Inter', sans-serif" }}>
                            Welcome to NovaStaff
                        </h3>
                    </motion.div>
                </motion.div>
            )}

            {/* Responsive style for small screens */}
            <style>{`
                @media (max-width: 768px) {
                    .login-left-panel {
                        display: none !important;
                    }
                }
            `}</style>
        </div>
    );
};

export default LoginPage;