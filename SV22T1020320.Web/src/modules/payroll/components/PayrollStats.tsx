import React from 'react';
import { Card, Col, Row, Statistic, Progress, Typography, Space, Tag, Skeleton } from 'antd';
import { motion, Variants } from 'framer-motion';
import {
    RiseOutlined,
    FallOutlined,
    DollarCircleOutlined,
    UsergroupAddOutlined,
    WalletOutlined
} from '@ant-design/icons';

const { Text, Title } = Typography;

interface PayrollStatsProps {
    totalNetSalary?: number;
    totalEmployees?: number;
    paidCount?: number;
    loading?: boolean;
}

const statsContentVariants: Variants = {
    hidden: { opacity: 0, y: 5 },
    visible: { 
        opacity: 1, 
        y: 0,
        transition: { duration: 0.3, ease: "easeOut" }
    }
};

export const PayrollStats: React.FC<PayrollStatsProps> = ({ 
    totalNetSalary = 0, 
    totalEmployees = 0, 
    paidCount = 0,
    loading = false
}) => {
    const averageSalary = totalEmployees > 0 ? totalNetSalary / totalEmployees : 0;
    const paidPercent = totalEmployees > 0 ? Math.round((paidCount / totalEmployees) * 100) : 0;

    return (
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            {/* 1. Tổng quỹ lương */}
            <Col xs={24} md={8}>
                <Card variant="borderless" hoverable style={{ height: '100%', overflow: 'hidden' }}>
                    {loading ? (
                        <Skeleton active paragraph={{ rows: 1 }} />
                    ) : (
                        <motion.div
                            variants={statsContentVariants}
                            initial="hidden"
                            animate="visible"
                        >
                            <Statistic
                                title="Tổng quỹ lương tháng này"
                                value={totalNetSalary}
                                precision={0}
                                styles={{
                                    content: { color: '#1d39c4' }
                                }}
                                prefix={<DollarCircleOutlined />}
                                suffix="đ"
                            />
                            <Space style={{ marginTop: 8 }}>
                                <Text type="secondary">Cập nhật theo dữ liệu thực tế</Text>
                            </Space>
                        </motion.div>
                    )}
                </Card>
            </Col>

            {/* 2. Trạng thái chi trả */}
            <Col xs={24} md={8}>
                <Card variant="borderless" hoverable style={{ height: '100%', overflow: 'hidden' }}>
                    {loading ? (
                        <Skeleton active paragraph={{ rows: 1 }} />
                    ) : (
                        <motion.div
                            variants={statsContentVariants}
                            initial="hidden"
                            animate="visible"
                        >
                            <div style={{ marginBottom: 8 }}>
                                <Text type="secondary">Tiến độ chi trả lương</Text>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Title level={3} style={{ margin: 0 }}>{paidPercent}%</Title>
                                <WalletOutlined style={{ fontSize: 24, color: '#52c41a' }} />
                            </div>
                            <Progress
                                percent={paidPercent}
                                status={paidPercent === 100 ? "success" : "active"}
                                strokeColor={{ '0%': '#108ee9', '100%': '#87d068' }}
                                showInfo={false}
                            />
                            <Text style={{ fontSize: '12px' }} type="secondary">
                                Đã thanh toán cho {paidCount}/{totalEmployees} nhân sự
                            </Text>
                        </motion.div>
                    )}
                </Card>
            </Col>

            {/* 3. Chi phí trung bình/Nhân sự */}
            <Col xs={24} md={8}>
                <Card variant="borderless" hoverable style={{ height: '100%', overflow: 'hidden' }}>
                    {loading ? (
                        <Skeleton active paragraph={{ rows: 1 }} />
                    ) : (
                        <motion.div
                            variants={statsContentVariants}
                            initial="hidden"
                            animate="visible"
                        >
                            <Statistic
                                title="Lương trung bình / Người"
                                value={averageSalary}
                                precision={0}
                                prefix={<UsergroupAddOutlined />}
                                suffix="đ"
                            />
                            <Space style={{ marginTop: 8 }}>
                                <Text type="secondary">Chi phí bình quân</Text>
                            </Space>
                        </motion.div>
                    )}
                </Card>
            </Col>
        </Row>
    );
};