import React, { useState } from 'react';
import { Form, Input, Button, Typography, message } from 'antd';
import {
    Lock,
    ArrowRight,
    Users,
    BarChart3,
    ClipboardCheck,
    Shield,
    XCircle
} from 'lucide-react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import { authApi } from './api/authApi';
import { ActivateAccountRequest } from './types';

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

const ActivateAccountPage: React.FC = () => {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const [loading, setLoading] = useState(false);
    const [isSuccess, setIsSuccess] = useState(false);

    const onFinish = async (values: any) => {
        if (!token) return;
        
        setLoading(true);
        try {
            const requestData: ActivateAccountRequest = {
                token: token,
                newPassword: values.newPassword,
                confirmPassword: values.confirmPassword
            };
            
            await authApi.activateAccount(requestData);

            message.success('Tài khoản đã được kích hoạt thành công. Vui lòng đăng nhập.');
            setIsSuccess(true);
            setTimeout(() => {
                navigate('/login');
            }, 2000); 
        } catch (error: any) {
            console.error('Activate error:', error);
            const errorMsg = error.response?.data?.message || 'Link kích hoạt không hợp lệ hoặc đã hết hạn';
            message.error(errorMsg);
            setLoading(false);
        }
    };

    if (!token) {
        return (
            <div style={{
                display: 'flex', minHeight: '100vh',
                fontFamily: "'Inter', system-ui, sans-serif",
                background: '#f8fafc', alignItems: 'center', justifyContent: 'center'
            }}>
                <motion.div
                    initial={{ opacity: 0, scale: 0.9 }}
                    animate={{ opacity: 1, scale: 1 }}
                    style={{
                        background: '#fff', padding: '48px 40px',
                        borderRadius: '24px', textAlign: 'center',
                        boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.08)',
                        maxWidth: 400, width: '100%'
                    }}
                >
                    <XCircle size={64} color="#ef4444" style={{ margin: '0 auto 24px' }} />
                    <Title level={3} style={{ margin: '0 0 16px', color: '#0f172a' }}>Link không hợp lệ</Title>
                    <Text style={{ color: '#64748b', fontSize: 15, display: 'block', marginBottom: 32 }}>
                        Link kích hoạt này không tồn tại hoặc đã hết hạn. Vui lòng kiểm tra lại email hoặc liên hệ Admin.
                    </Text>
                    <Button 
                        type="primary" 
                        size="large" 
                        block 
                        onClick={() => navigate('/login')}
                        style={{
                            height: 48, borderRadius: 10, fontSize: 15, fontWeight: 600,
                            background: '#4f46e5', border: 'none'
                        }}
                    >
                        Go back to Login
                    </Button>
                </motion.div>
            </div>
        );
    }

    return (
        <div
            style={{
                display: 'flex',
                minHeight: '100vh',
                fontFamily: "'Inter', system-ui, sans-serif",
                background: '#ffffff',
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
                        Welcome to<br />
                        <span style={{
                            background: 'linear-gradient(to right, #c7d2fe, #818cf8)',
                            WebkitBackgroundClip: 'text',
                            WebkitTextFillColor: 'transparent'
                        }}>NovaStaff.</span>
                    </Title>
                    <Text style={{
                        color: 'rgba(255, 255, 255, 0.6)', fontSize: 16,
                        display: 'block', marginTop: 16, fontWeight: 400,
                        lineHeight: 1.5, maxWidth: 380, letterSpacing: '-0.2px'
                    }}>
                        Set up your account to start managing your workspace efficiently.
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
                    <motion.div variants={itemVariants} style={{ marginBottom: 32 }}>
                        <Title level={2} style={{
                            margin: 0, fontWeight: 700,
                            color: '#0f172a', letterSpacing: '-0.8px',
                            fontSize: 26,
                        }}>
                            Activate Account
                        </Title>
                        <Text style={{ color: '#64748b', fontSize: 14, marginTop: 4, display: 'block', fontWeight: 400 }}>
                            Please set a new password to secure your account
                        </Text>
                    </motion.div>

                    {/* Form */}
                    <motion.div variants={itemVariants}>
                        <Form
                            name="activate"
                            onFinish={onFinish}
                            layout="vertical"
                            requiredMark={false}
                            size="large"
                        >
                            <Form.Item
                                label={<Text style={{ fontWeight: 600, color: '#475569', fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.5px' }}>New Password</Text>}
                                name="newPassword"
                                rules={[
                                    { required: true, message: 'Vui lòng nhập mật khẩu mới' },
                                    { min: 8, message: 'Mật khẩu phải có ít nhất 8 ký tự' }
                                ]}
                                style={{ marginBottom: 16 }}
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

                            <Form.Item
                                label={<Text style={{ fontWeight: 600, color: '#475569', fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.5px' }}>Confirm Password</Text>}
                                name="confirmPassword"
                                dependencies={['newPassword']}
                                rules={[
                                    { required: true, message: 'Vui lòng xác nhận mật khẩu' },
                                    ({ getFieldValue }) => ({
                                        validator(_, value) {
                                            if (!value || getFieldValue('newPassword') === value) {
                                                return Promise.resolve();
                                            }
                                            return Promise.reject(new Error('Mật khẩu xác nhận không khớp!'));
                                        },
                                    }),
                                ]}
                                style={{ marginBottom: 24 }}
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
                                    Kích hoạt tài khoản
                                </Button>
                            </Form.Item>
                        </Form>
                    </motion.div>

                    {/* Version badge */}
                    <motion.div
                        variants={itemVariants}
                        style={{ textAlign: 'center', marginTop: 32 }}
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
                            Account Activated
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

export default ActivateAccountPage;
