import React, { useState, useEffect, useCallback } from 'react';
import { Typography, Button, Space, Card, Row, Col, Input, message, Modal } from 'antd';
import { DollarOutlined, SearchOutlined, PlusOutlined } from '@ant-design/icons';
import { PayrollStats } from './components/PayrollStats';
import { PeriodListTable } from './components/PeriodListTable';
import { CreatePeriodModal } from './components/CreatePeriodModal';
import { PeriodDetailView } from './components/PeriodDetailView';
import { PayslipModal } from './components/PayslipModal';
import { IPayrollItem, PayrollPeriodSummaryDto } from './types';
import { useAuth } from '../../contexts/AuthContext';
import { UserRole } from '../../modules/auth/types';
import { payrollApi } from './api/payrollApi';

const { Title, Text } = Typography;
import { motion, Variants, AnimatePresence } from 'framer-motion';

const containerVariants: Variants = {
    hidden: { opacity: 0 },
    visible: {
        opacity: 1,
        transition: {
            staggerChildren: 0.08
        }
    }
};

const itemVariants: Variants = {
    hidden: { opacity: 0, y: 10 },
    visible: {
        opacity: 1,
        y: 0,
        transition: {
            duration: 0.5,
            ease: "easeOut"
        }
    }
};

const PayrollPage: React.FC = () => {
    const { user } = useAuth();
    const isAdmin = user?.role === UserRole.Admin || user?.role === UserRole.Manager;

    const [isPayslipModalOpen, setIsPayslipModalOpen] = useState(false);
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

    // Period selection
    const [selectedPeriodId, setSelectedPeriodId] = useState<number | null>(null);

    // For Employee temp view
    const [selectedEmployee, setSelectedEmployee] = useState<any | null>(null);

    // Period List states
    const [periods, setPeriods] = useState<PayrollPeriodSummaryDto[]>([]);
    const [loading, setLoading] = useState(false);

    const loadPeriods = useCallback(async () => {
        if (!isAdmin) return;
        setLoading(true);
        try {
            // Thêm độ trễ 300ms để hiệu ứng load mượt mà hơn, tránh nháy giao diện
            const [res] = await Promise.all([
                payrollApi.getPeriods(1, 50),
                new Promise(resolve => setTimeout(resolve, 300))
            ]);
            setPeriods(res.data.items || []);
        } catch (error: any) {
            console.error('Failed to load periods', error);
            message.error('Không thể tải danh sách kỳ lương');
        } finally {
            setLoading(false);
        }
    }, [isAdmin]);

    useEffect(() => {
        if (!selectedPeriodId) {
            loadPeriods();
        }
    }, [loadPeriods, selectedPeriodId]);

    const handleViewDetail = (periodId: number) => {
        setSelectedPeriodId(periodId);
        // Cuộn xuống phần chi tiết mượt mà
        setTimeout(() => {
            document.getElementById('period-detail-section')?.scrollIntoView({ behavior: 'smooth' });
        }, 100);
    };

    return (
        <motion.div 
            style={{ padding: '24px' }}
            variants={containerVariants}
            initial="hidden"
            animate="visible"
        >
            {/* Header Section */}
            <motion.div variants={itemVariants}>
                <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
                    <Col>
                        <Title level={3} style={{ margin: 0 }}>
                            {isAdmin ? "Quản Lý Bảng Lương" : "Phiếu Lương Cá Nhân"}
                        </Title>
                        {isAdmin ? (
                            <Text type="secondary">Danh sách các kỳ lương trong hệ thống</Text>
                        ) : (
                            <Text type="secondary">Kỳ lương tháng 04/2026</Text>
                        )}
                    </Col>
                    {isAdmin && (
                        <Col>
                            <Space>
                                <Button
                                    type="primary"
                                    icon={<PlusOutlined />}
                                    onClick={() => setIsCreateModalOpen(true)}
                                >
                                    Tạo kỳ lương mới
                                </Button>
                            </Space>
                        </Col>
                    )}
                </Row>
            </motion.div>

            {/* Thống kê quỹ lương toàn công ty */}
            {isAdmin && (
                <motion.div variants={itemVariants}>
                    <PayrollStats
                        loading={loading}
                        totalNetSalary={periods.reduce((sum, p) => sum + p.totalNetSalary, 0)}
                        totalEmployees={periods.reduce((sum, p) => sum + p.totalEmployees, 0)}
                        paidCount={periods.filter(p => p.status === 4).reduce((sum, p) => sum + p.totalEmployees, 0)}
                    />
                </motion.div>
            )}

            {/* Khu vực chính */}
            <motion.div variants={itemVariants}>
                <Card style={{ borderRadius: 12, minHeight: 460 }}>
                    <AnimatePresence mode="wait">
                        {loading ? (
                            <motion.div 
                                key="loading"
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                exit={{ opacity: 0 }}
                                transition={{ duration: 0.2 }}
                            >
                                <PeriodListTable
                                    dataSource={[]}
                                    loading={true}
                                    onViewDetail={handleViewDetail}
                                />
                            </motion.div>
                        ) : (
                            <motion.div
                                key="data"
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                transition={{ duration: 0.3 }}
                            >
                                {isAdmin ? (
                                    <>
                                        <div style={{ marginBottom: 16, display: 'flex', gap: '10px' }}>
                                            <Input
                                                placeholder="Tìm kỳ lương..."
                                                prefix={<SearchOutlined />}
                                                style={{ width: 250 }}
                                            />
                                        </div>
                                        <PeriodListTable
                                            dataSource={periods}
                                            loading={false}
                                            onViewDetail={handleViewDetail}
                                        />
                                    </>
                                ) : (
                                    <div style={{ textAlign: 'center', padding: '40px' }}>
                                        <DollarOutlined style={{ fontSize: 64, color: '#52c41a', marginBottom: 16 }} />
                                        <Title level={4}>Phiếu lương tháng 04 đã sẵn sàng</Title>
                                        <Text style={{ marginBottom: 24, display: 'block' }}>
                                            Tổng thực nhận: <b style={{ fontSize: 20 }}>15,300,000đ</b>
                                        </Text>
                                        <Button type="primary" onClick={() => setIsPayslipModalOpen(true)}>Xem chi tiết phiếu lương</Button>
                                    </div>
                                )}
                            </motion.div>
                        )}
                    </AnimatePresence>
                </Card>
            </motion.div>

            <Modal
                open={!!selectedPeriodId}
                onCancel={() => setSelectedPeriodId(null)}
                width="95%"
                footer={null}
                closable={false}
                destroyOnHidden
                styles={{ body: { padding: 0 } }}
                style={{ top: 20 }}
            >
                {selectedPeriodId && (
                    <PeriodDetailView
                        periodId={selectedPeriodId}
                        onBack={() => setSelectedPeriodId(null)}
                    />
                )}
            </Modal>

            <PayslipModal
                open={isPayslipModalOpen}
                data={selectedEmployee}
                onCancel={() => setIsPayslipModalOpen(false)}
            />

            <CreatePeriodModal
                open={isCreateModalOpen}
                onCancel={() => setIsCreateModalOpen(false)}
                onSuccess={() => {
                    setIsCreateModalOpen(false);
                    loadPeriods();
                }}
            />
        </motion.div>
    );
};

export default PayrollPage;
