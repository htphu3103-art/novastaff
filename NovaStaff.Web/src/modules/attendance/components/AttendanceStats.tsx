import React from 'react';
import { Card, Col, Row, Statistic, Skeleton, Space } from 'antd';
import { motion, AnimatePresence } from 'framer-motion';
import { 
    CheckCircleOutlined, 
    UserDeleteOutlined, 
    ClockCircleOutlined, 
    RocketOutlined, 
    CalendarOutlined,
    TeamOutlined,
    LineChartOutlined
} from '@ant-design/icons';

interface AdminAttendanceStats {
    presentToday: number;
    totalEmployees: number;
    absent: number;
    lateArrivals: number;
    overtimeHours: number;
    pendingLeaves: number;
}

interface EmployeeAttendanceStats {
    workingDays: number;
    totalHours: number;
    lateCount: number;
}

interface AttendanceStatsProps {
    isAdmin: boolean;
    adminStats?: AdminAttendanceStats;
    employeeStats?: EmployeeAttendanceStats;
    loading?: boolean;
}

const defaultAdminStats: AdminAttendanceStats = {
    presentToday: 0,
    totalEmployees: 0,
    absent: 0,
    lateArrivals: 0,
    overtimeHours: 0,
    pendingLeaves: 0,
};

const defaultEmployeeStats: EmployeeAttendanceStats = {
    workingDays: 0,
    totalHours: 0,
    lateCount: 0,
};

const StatValue = ({ loading, children }: { loading: boolean, children: React.ReactNode }) => (
    <div style={{ height: 32, display: 'flex', alignItems: 'center' }}>
        <AnimatePresence mode="wait">
            {loading ? (
                <motion.div
                    key="loading"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    style={{ width: '60%' }}
                >
                    <Skeleton.Button active size="small" block style={{ height: 24, borderRadius: 4 }} />
                </motion.div>
            ) : (
                <motion.div
                    key="value"
                    initial={{ opacity: 0, y: 10, filter: 'blur(4px)' }}
                    animate={{ opacity: 1, y: 0, filter: 'blur(0px)' }}
                    transition={{ duration: 0.4, ease: [0.16, 1, 0.3, 1] }}
                >
                    {children}
                </motion.div>
            )}
        </AnimatePresence>
    </div>
);

export const AttendanceStats = ({ isAdmin, adminStats, employeeStats, loading = false }: AttendanceStatsProps) => {
    const admin = adminStats ?? defaultAdminStats;
    const employee = employeeStats ?? defaultEmployeeStats;

    return (
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            {isAdmin ? (
                <>
                    <Col xs={12} sm={8} lg={4}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Present Today</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <CheckCircleOutlined style={{ color: '#52c41a', fontSize: '16px' }} />
                                    <span style={{ color: '#52c41a', fontSize: '18px', fontWeight: 'bold' }}>{admin.presentToday}</span>
                                    <span style={{ fontSize: '12px', color: 'rgba(0,0,0,0.45)' }}>{`/ ${admin.totalEmployees}`}</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Absent</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <UserDeleteOutlined style={{ color: '#ff4d4f', fontSize: '16px' }} />
                                    <span style={{ color: '#ff4d4f', fontSize: '18px', fontWeight: 'bold' }}>{admin.absent}</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Late Arrivals</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <ClockCircleOutlined style={{ color: '#faad14', fontSize: '16px' }} />
                                    <span style={{ color: '#faad14', fontSize: '18px', fontWeight: 'bold' }}>{admin.lateArrivals}</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Overtime Hours</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <RocketOutlined style={{ color: '#1890ff', fontSize: '16px' }} />
                                    <span style={{ color: '#1890ff', fontSize: '18px', fontWeight: 'bold' }}>{admin.overtimeHours}</span>
                                    <span style={{ fontSize: '12px', color: 'rgba(0,0,0,0.45)' }}>h</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Pending Leaves</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <CalendarOutlined style={{ color: '#722ed1', fontSize: '16px' }} />
                                    <span style={{ color: '#722ed1', fontSize: '18px', fontWeight: 'bold' }}>{admin.pendingLeaves}</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Total Employees</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <TeamOutlined style={{ fontSize: '16px' }} />
                                    <span style={{ fontSize: '18px', fontWeight: 'bold' }}>{admin.totalEmployees}</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                </>
            ) : (
                <>
                    <Col xs={24} sm={8} lg={8}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Working Days</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <LineChartOutlined style={{ color: '#1890ff', fontSize: '16px' }} />
                                    <span style={{ fontSize: '18px', fontWeight: 'bold' }}>{employee.workingDays}</span>
                                    <span style={{ fontSize: '12px', color: 'rgba(0,0,0,0.45)' }}>days</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                    <Col xs={24} sm={8} lg={8}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Total Hours</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <ClockCircleOutlined style={{ color: '#52c41a', fontSize: '16px' }} />
                                    <span style={{ fontSize: '18px', fontWeight: 'bold' }}>{employee.totalHours.toFixed(2)}</span>
                                    <span style={{ fontSize: '12px', color: 'rgba(0,0,0,0.45)' }}>hrs</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                    <Col xs={24} sm={8} lg={8}>
                        <Card size="small" hoverable styles={{ body: { height: 80, display: 'flex', flexDirection: 'column', justifyContent: 'center' } }}>
                            <span style={{ fontSize: '14px', whiteSpace: 'nowrap', fontWeight: 500, color: 'rgba(0,0,0,0.45)', marginBottom: 4 }}>Late Count</span>
                            <StatValue loading={loading}>
                                <Space>
                                    <ClockCircleOutlined style={{ color: '#ff4d4f', fontSize: '16px' }} />
                                    <span style={{ color: '#ff4d4f', fontSize: '18px', fontWeight: 'bold' }}>{employee.lateCount}</span>
                                </Space>
                            </StatValue>
                        </Card>
                    </Col>
                </>
            )}
        </Row>
    );
};
