import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { App, Calendar, Card, Col, Row, Select, Space, Tag, Typography, Tabs, Button } from 'antd';
import { ClockCircleOutlined, HistoryOutlined, FileTextOutlined, PlusOutlined } from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';
import axios from 'axios';
import { AttendanceStats } from '../components/AttendanceStats';
import { AttendanceTable } from '../components/AttendanceTable';
import { CheckInCard } from '../components/CheckInCard';
import { AttendanceDto, AttendanceStatus, LeaveRequestDto, LeaveRequestStatus, LeaveType } from '../types';
import { attendanceApi } from '../api/attendanceApi';
import { useAuth } from '../../../contexts/AuthContext';
import { LeaveRequestModal } from '../components/LeaveRequestModal';
import { LeaveRequestTable } from '../components/LeaveRequestTable';
import { leaveRequestApi } from '../api/leaveRequestApi';
import { motion, Variants } from 'framer-motion';

const { Title, Text } = Typography;

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
            ease: "easeOut" // Fixed: Changed from cubic-bezier array to named easing for better compatibility
        }
    }
};

const tabContentVariants: Variants = {
    hidden: { opacity: 0, y: 10 },
    visible: { 
        opacity: 1, 
        y: 0,
        transition: { duration: 0.3, ease: "easeOut" }
    }
};

const extractErrorMessage = (error: unknown, fallback: string): string => {
    if (axios.isAxiosError(error)) {
        if (typeof error.response?.data === 'string' && error.response.data.trim()) {
            return error.response.data;
        }

        const payload = error.response?.data as { message?: string; title?: string; detail?: string } | undefined;
        if (payload?.message) return payload.message;
        if (payload?.title) return payload.title;
        if (payload?.detail) return payload.detail;
    }

    return fallback;
};

const getStatusDotColor = (status: AttendanceStatus): string => {
    switch (status) {
        case AttendanceStatus.Present: return '#52c41a';
        case AttendanceStatus.Late: return '#faad14';
        case AttendanceStatus.Absent: return '#ff4d4f';
        case AttendanceStatus.HalfDay: return '#1677ff';
        case AttendanceStatus.Leave: return '#722ed1';
        default: return '#d9d9d9';
    }
};

export default function EmployeeAttendancePage() {
    const { message, modal } = App.useApp();
    const { user } = useAuth();
    const employeeId = user?.userId;

    const [calendarValue, setCalendarValue] = useState(dayjs());
    const [monthlyRecords, setMonthlyRecords] = useState<AttendanceDto[]>([]);
    const [todayRecord, setTodayRecord] = useState<AttendanceDto | null>(null);
    const [totalHoursFromApi, setTotalHoursFromApi] = useState<number | null>(null);

    const [isHistoryLoading, setIsHistoryLoading] = useState(true);
    const [isTodayLoading, setIsTodayLoading] = useState(true);
    const [isActionLoading, setIsActionLoading] = useState(false);
    const [actionType, setActionType] = useState<'check-in' | 'check-out' | null>(null);
    const [isLeaveModalOpen, setIsLeaveModalOpen] = useState(false);

    const [personalLeaves, setPersonalLeaves] = useState<LeaveRequestDto[]>([]);
    const [isLeavesLoading, setIsLeavesLoading] = useState(true);

    const loadTodayRecord = useCallback(async (showError = true) => {
        setIsTodayLoading(true);
        try {
            const [res] = await Promise.all([
                attendanceApi.getTodaySelf(),
                new Promise(resolve => setTimeout(resolve, 300))
            ]);
            setTodayRecord(res.data);
        } catch (error) {
            if (axios.isAxiosError(error) && (error.response?.status === 404 || error.response?.status === 204)) {
                setTodayRecord(null);
            } else if (showError) {
                message.error(extractErrorMessage(error, 'Cannot load today attendance.'));
            }
        } finally {
            setIsTodayLoading(false);
        }
    }, [message]);

    const loadMonthlyRecords = useCallback(async (value: Dayjs) => {
        if (!employeeId) return;
        setIsHistoryLoading(true);
        try {
            const year = value.year();
            const month = value.month() + 1;
            const [res] = await Promise.all([
                attendanceApi.getPaged({ from: value.startOf('month').toISOString(), to: value.endOf('month').toISOString() }, 1, 100),
                new Promise(resolve => setTimeout(resolve, 300))
            ]);
            setMonthlyRecords(res.data.items);
            
            const totalHoursRes = await attendanceApi.getTotalHoursSelf(year, month);
            setTotalHoursFromApi(totalHoursRes.data?.totalHours ?? null);
        } catch (err) {
            console.error("Error loading monthly records:", err);
        } finally {
            setIsHistoryLoading(false);
        }
    }, [employeeId]);

    const loadPersonalLeaves = useCallback(async () => {
        if (!employeeId) return;
        setIsLeavesLoading(true);
        try {
            const res = await leaveRequestApi.getMyRequests();
            setPersonalLeaves(res.data);
        } catch (error) {
            console.error("Error loading leave requests:", error);
        } finally {
            setIsLeavesLoading(false);
        }
    }, [employeeId]);

    const refreshData = useCallback(async () => {
        await Promise.all([
            loadTodayRecord(false),
            loadMonthlyRecords(calendarValue),
            loadPersonalLeaves(),
        ]);
    }, [loadTodayRecord, loadMonthlyRecords, loadPersonalLeaves, calendarValue]);

    useEffect(() => {
        if (employeeId) refreshData();
    }, [employeeId, refreshData]);

    const handleAttendance = useCallback(async (type: 'check-in' | 'check-out') => {
        setIsActionLoading(true);
        setActionType(type);
        try {
            if (type === 'check-in') await attendanceApi.checkInSelf();
            else await attendanceApi.checkOutSelf();
            message.success(`${type === 'check-in' ? 'Check-in' : 'Check-out'} successful.`);
            await refreshData();
        } catch (error) {
            message.error(extractErrorMessage(error, `${type === 'check-in' ? 'Check-in' : 'Check-out'} failed.`));
        } finally {
            setIsActionLoading(false);
            setActionType(null);
        }
    }, [message, refreshData]);

    const employeeStats = useMemo(() => {
        const workingStatuses = new Set([AttendanceStatus.Present, AttendanceStatus.Late, AttendanceStatus.HalfDay]);
        const workingDays = monthlyRecords.filter(r => workingStatuses.has(r.status)).length;
        const lateCount = monthlyRecords.filter(r => r.status === AttendanceStatus.Late).length;
        const totalHours = typeof totalHoursFromApi === 'number' ? totalHoursFromApi : monthlyRecords.reduce((sum, r) => sum + (r.workHours ?? 0), 0);
        return { workingDays, lateCount, totalHours };
    }, [monthlyRecords, totalHoursFromApi]);

    const recordsByDate = useMemo(() => {
        const map = new Map<string, AttendanceDto>();
        monthlyRecords.forEach(r => map.set(dayjs(r.workDate).format('YYYY-MM-DD'), r));
        return map;
    }, [monthlyRecords]);

    return (
        <motion.div 
            className="employee-attendance-page"
            variants={containerVariants}
            initial="hidden"
            animate="visible"
        >
            <motion.div 
                style={{ marginBottom: 24, display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '16px' }}
                variants={itemVariants}
            >
                <div>
                    <Title level={3} style={{ margin: 0 }}>Bảng công Cá nhân</Title>
                    <Text type="secondary">Theo dõi thời gian làm việc và lịch sử chấm công của bạn</Text>
                </div>
                <Space>
                    <Tag color="blue" icon={<ClockCircleOutlined />} style={{ padding: '4px 12px', borderRadius: 6 }}>
                        Ca làm việc: Hành chính (08:00 - 17:00)
                    </Tag>
                </Space>
            </motion.div>

            <motion.div variants={itemVariants}>
                <AttendanceStats isAdmin={false} employeeStats={employeeStats} loading={isHistoryLoading || isTodayLoading} />
            </motion.div>

            <Row gutter={[24, 24]}>
                <Col xs={24} lg={16}>
                    <motion.div variants={itemVariants}>
                        <Tabs
                            type="card"
                            destroyInactiveTabPane={true}
                            items={[
                                {
                                    key: '1',
                                    label: <Space><ClockCircleOutlined /> Chấm công hôm nay</Space>,
                                    children: (
                                        <motion.div
                                            variants={tabContentVariants}
                                            initial="hidden"
                                            animate="visible"
                                            style={{ display: 'flex', flexDirection: 'column', gap: 24 }}
                                        >
                                            <CheckInCard
                                                loading={isTodayLoading || isActionLoading}
                                                todayRecord={todayRecord}
                                                onCheckIn={() => handleAttendance('check-in')}
                                                onCheckOut={() => handleAttendance('check-out')}
                                                actionType={actionType}
                                            />
                                            <Card 
                                                title={<Space><HistoryOutlined /> Lịch sử gần đây</Space>}
                                                style={{ borderRadius: 12, minHeight: 460 }}
                                            >
                                                <AttendanceTable 
                                                    isAdmin={false} 
                                                    dataSource={monthlyRecords.slice(0, 5)} 
                                                    loading={isHistoryLoading} 
                                                />
                                            </Card>
                                        </motion.div>
                                    )
                                },
                                {
                                    key: '2',
                                    label: <Space><FileTextOutlined /> Đơn nghỉ phép</Space>,
                                    children: (
                                        <motion.div
                                            variants={tabContentVariants}
                                            initial="hidden"
                                            animate="visible"
                                        >
                                            <Card 
                                                title={
                                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                                        <span>Danh sách đơn đã gửi</span>
                                                        <Button type="primary" size="small" icon={<PlusOutlined />} onClick={() => setIsLeaveModalOpen(true)}>Tạo đơn mới</Button>
                                                    </div>
                                                }
                                                style={{ borderRadius: 12, minHeight: 460 }}
                                            >
                                                <LeaveRequestTable 
                                                    dataSource={personalLeaves}
                                                    onApprove={() => {}} 
                                                    onReject={() => {}} 
                                                    onView={(record) => {
                                                        modal.info({
                                                            title: 'Chi tiết đơn nghỉ phép',
                                                            content: (
                                                                <div style={{ marginTop: 16 }}>
                                                                    <p><b>Loại nghỉ:</b> {record.leaveType}</p>
                                                                    <p><b>Thời gian:</b> {dayjs(record.fromDate).format('DD/MM/YYYY')} - {dayjs(record.toDate).format('DD/MM/YYYY')}</p>
                                                                    <p><b>Lý do:</b> {record.reason}</p>
                                                                    <p><b>Trạng thái:</b> {record.status}</p>
                                                                </div>
                                                            )
                                                        });
                                                    }}
                                                    showEmployeeInfo={false}
                                                    loading={isLeavesLoading}
                                                />
                                            </Card>
                                        </motion.div>
                                    )
                                }
                            ]}
                        />
                    </motion.div>
                </Col>

                <Col xs={24} lg={8}>
                    <motion.div variants={itemVariants}>
                        <Card title="Lịch biểu tháng" style={{ borderRadius: 12 }}>
                            <Calendar
                                fullscreen={false}
                                value={calendarValue}
                                onChange={setCalendarValue}
                                fullCellRender={(current, info) => {
                                    if (info.type !== 'date') return info.originNode;
                                    const dayRecord = recordsByDate.get(current.format('YYYY-MM-DD'));
                                    if (!dayRecord) return info.originNode;
                                    return (
                                        <div style={{ padding: 2, height: '100%' }}>
                                            <div style={{ 
                                                background: getStatusDotColor(dayRecord.status), 
                                                color: '#fff', borderRadius: 4, textAlign: 'center', height: '100%', minHeight: 24, display: 'flex', alignItems: 'center', justifyContent: 'center' 
                                            }}>
                                                {current.date()}
                                            </div>
                                        </div>
                                    );
                                }}
                            />
                            <div style={{ marginTop: 16 }}>
                                <Space direction="vertical" size={4}>
                                    <Text strong>Chú thích:</Text>
                                    <Space><div style={{ width: 8, height: 8, borderRadius: '50%', background: '#52c41a' }} /> Đúng giờ</Space>
                                    <Space><div style={{ width: 8, height: 8, borderRadius: '50%', background: '#faad14' }} /> Muộn / Sớm</Space>
                                    <Space><div style={{ width: 8, height: 8, borderRadius: '50%', background: '#ff4d4f' }} /> Vắng mặt</Space>
                                </Space>
                            </div>
                        </Card>
                    </motion.div>
                </Col>
            </Row>

            <LeaveRequestModal
                open={isLeaveModalOpen}
                onCancel={() => setIsLeaveModalOpen(false)}
                onSuccess={refreshData}
            />
        </motion.div>
    );
}
