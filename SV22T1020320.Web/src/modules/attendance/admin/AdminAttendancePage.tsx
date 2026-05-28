import React, { useState, useEffect, useCallback } from 'react';
import { Card, Col, Row, Button, Typography, Space, Input, Select, DatePicker, App } from 'antd';
import { ExportOutlined, SearchOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons';
import { AttendanceStats } from '../components/AttendanceStats';
import { AttendanceTable } from '../components/AttendanceTable';
import { AttendanceFormModal } from '../components/AttendanceFormModal';
import { AttendanceDto, AttendanceStatus, AttendanceFilter, LeaveRequestDto, LeaveRequestStatus, LeaveType } from '../types';
import { attendanceApi } from '../api/attendanceApi';
import { employeeApi } from '../../employee/api/employeeApi';
import { LeaveRequestTable } from '../components/LeaveRequestTable';
import { leaveRequestApi } from '../api/leaveRequestApi';
import dayjs from 'dayjs';
import { Tabs } from 'antd';

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

const tabContentVariants: Variants = {
    hidden: { opacity: 0, y: 10 },
    visible: { 
        opacity: 1, 
        y: 0,
        transition: { duration: 0.3, ease: "easeOut" }
    }
};

const { Title, Text } = Typography;

export default function AdminAttendancePage() {
    const { message, modal } = App.useApp();
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isStatsLoading, setIsStatsLoading] = useState(true);
    const [dataSource, setDataSource] = useState<AttendanceDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [adminStats, setAdminStats] = useState({
        presentToday: 0,
        totalEmployees: 0,
        lateArrivals: 0,
        absent: 0,
        overtimeHours: 0,
        pendingLeaves: 0
    });

    const [leaveRequests, setLeaveRequests] = useState<LeaveRequestDto[]>([]);
    const [isLeavesLoading, setIsLeavesLoading] = useState(true);

    // Filter states
    const [searchText, setSearchText] = useState('');
    const [statusFilter, setStatusFilter] = useState<AttendanceStatus | undefined>(undefined);
    const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null] | null>(null);
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(20);

    const fetchLeaveRequests = useCallback(async () => {
        setIsLeavesLoading(true);
        try {
            const res = await leaveRequestApi.getPending();
            setLeaveRequests(res.data);
        } catch (error) {
            console.error('Fetch Leave Requests Error:', error);
        } finally {
            setIsLeavesLoading(false);
        }
    }, []);

    const fetchStats = useCallback(async () => {
        setIsStatsLoading(true);
        try {
            const today = dayjs().startOf('day');
            const endOfDay = dayjs().endOf('day');

            const [empRes, attRes, leaveRes] = await Promise.all([
                employeeApi.getPaged({}, 1, 1),
                attendanceApi.getPaged({ from: today.toISOString(), to: endOfDay.toISOString() }, 1, 1000),
                leaveRequestApi.getPending()
            ]);

            const totalEmployees = empRes.data.totalCount;
            const todayRecords = attRes.data.items;

            const presentTodayCount = todayRecords.filter(r =>
                r.status === AttendanceStatus.Present ||
                r.status === AttendanceStatus.Late ||
                r.status === AttendanceStatus.HalfDay
            ).length;

            const lateArrivalsCount = todayRecords.filter(r => r.status === AttendanceStatus.Late).length;

            const overtimeHoursCount = todayRecords.reduce((sum, r) => {
                const hours = r.workHours || 0;
                return sum + (hours > 8 ? hours - 8 : 0);
            }, 0);

            const pendingLeavesCount = leaveRes.data.length;

            setAdminStats({
                presentToday: presentTodayCount,
                totalEmployees: totalEmployees,
                lateArrivals: lateArrivalsCount,
                absent: Math.max(0, totalEmployees - presentTodayCount),
                overtimeHours: Math.round(overtimeHoursCount * 10) / 10,
                pendingLeaves: pendingLeavesCount
            });
        } catch (error) {
            console.error('Fetch Stats Error:', error);
        } finally {
            setIsStatsLoading(false);
        }
    }, []);

    const fetchAttendance = useCallback(async () => {
        setIsLoading(true);
        try {
            const filter: AttendanceFilter = {
                status: statusFilter,
                from: dateRange?.[0]?.toISOString(),
                to: dateRange?.[1]?.toISOString(),
            };

            const [response] = await Promise.all([
                attendanceApi.getPaged(filter, pageIndex, pageSize),
                new Promise(resolve => setTimeout(resolve, 300))
            ]);
            setDataSource(response.data.items);
            setTotalCount(response.data.totalCount);
        } catch (error) {
            message.error('Không thể tải dữ liệu chấm công');
        } finally {
            setIsLoading(false);
        }
    }, [statusFilter, dateRange, pageIndex, pageSize, message]);

    useEffect(() => {
        fetchAttendance();
        fetchStats();
        fetchLeaveRequests();
    }, [fetchAttendance, fetchStats, fetchLeaveRequests]);

    const handleDelete = async (recordId: number) => {
        try {
            await attendanceApi.delete(recordId);
            message.success('Đã xóa bản ghi thành công');
            fetchAttendance();
            fetchStats();
        } catch (error) {
            message.error('Xóa bản ghi thất bại');
        }
    };

    const handleFilter = () => {
        setPageIndex(1);
        fetchAttendance();
    };

    const handleReset = () => {
        setSearchText('');
        setStatusFilter(undefined);
        setDateRange(null);
        setPageIndex(1);
    };

    const handleApproveLeave = async (requestId: number) => {
        try {
            await leaveRequestApi.approve(requestId);
            message.success('Đã phê duyệt đơn nghỉ phép');
            fetchLeaveRequests();
            fetchStats();
        } catch (error) {
            message.error('Phê duyệt thất bại');
        }
    };

    const handleRejectLeave = (requestId: number) => {
        modal.confirm({
            title: 'Từ chối đơn nghỉ phép',
            content: (
                <Input.TextArea
                    placeholder="Nhập lý do từ chối..."
                    id="reject-reason"
                    rows={4}
                    style={{ marginTop: 16 }}
                />
            ),
            onOk: async () => {
                const reason = (document.getElementById('reject-reason') as HTMLTextAreaElement)?.value;
                if (!reason) {
                    message.warning('Vui lòng nhập lý do từ chối');
                    return Promise.reject();
                }
                try {
                    await leaveRequestApi.reject(requestId, { reason });
                    message.success('Đã từ chối đơn nghỉ phép');
                    fetchLeaveRequests();
                    fetchStats();
                } catch (error) {
                    message.error('Từ chối thất bại');
                }
            }
        });
    };

    useEffect(() => {
        setAdminStats(prev => ({
            ...prev,
            pendingLeaves: leaveRequests.filter(r => r.status === LeaveRequestStatus.Pending).length
        }));
    }, [leaveRequests]);

    return (
        <motion.div 
            className="admin-attendance-page"
            variants={containerVariants}
            initial="hidden"
            animate="visible"
        >
            <motion.div 
                style={{ marginBottom: 24, display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '16px' }}
                variants={itemVariants}
            >
                <div>
                    <Title level={3} style={{ margin: 0 }}>Quản lý Chấm công Hệ thống</Title>
                    <Text type="secondary">Theo dõi và điều chỉnh dữ liệu chấm công của toàn bộ nhân viên</Text>
                </div>
                <Space wrap>
                    <Button icon={<ReloadOutlined />} onClick={handleReset}>Làm mới bộ lọc</Button>
                    <Button icon={<PlusOutlined />} type="primary" onClick={() => setIsCreateModalOpen(true)}>Nhập thủ công</Button>
                    <Button icon={<ExportOutlined />}>Xuất báo cáo (Excel)</Button>
                </Space>
            </motion.div>

            <motion.div variants={itemVariants}>
                <AttendanceStats isAdmin={true} adminStats={adminStats} loading={isStatsLoading || isLoading} />
            </motion.div>

            <motion.div variants={itemVariants}>
                <Tabs
                    defaultActiveKey="1"
                    type="card"
                    className="attendance-tabs"
                    destroyOnHidden={true}
                    items={[
                        {
                            key: '1',
                            label: 'Nhật ký Chấm công',
                            children: (
                                <motion.div
                                    variants={tabContentVariants}
                                    initial="hidden"
                                    animate="visible"
                                >
                                    <Card style={{ marginBottom: 24, borderRadius: 12 }}>
                                        <Row gutter={[16, 16]}>
                                            <Col xs={24} sm={12} lg={6}>
                                                <Input
                                                    placeholder="Tìm kiếm theo ID hoặc Tên..."
                                                    prefix={<SearchOutlined />}
                                                    value={searchText}
                                                    onChange={(e) => setSearchText(e.target.value)}
                                                    onPressEnter={handleFilter}
                                                />
                                            </Col>
                                            <Col xs={24} sm={12} lg={7}>
                                                <DatePicker.RangePicker
                                                    style={{ width: '100%' }}
                                                    value={dateRange}
                                                    onChange={(dates) => setDateRange(dates as any)}
                                                />
                                            </Col>
                                            <Col xs={24} sm={12} lg={6}>
                                                <Select
                                                    placeholder="Trạng thái"
                                                    style={{ width: '100%' }}
                                                    allowClear
                                                    value={statusFilter}
                                                    onChange={setStatusFilter}
                                                >
                                                    <Select.Option value={AttendanceStatus.Present}>Có mặt (Present)</Select.Option>
                                                    <Select.Option value={AttendanceStatus.Late}>Đi muộn (Late)</Select.Option>
                                                    <Select.Option value={AttendanceStatus.Absent}>Vắng mặt (Absent)</Select.Option>
                                                    <Select.Option value={AttendanceStatus.HalfDay}>Nửa ngày (Half Day)</Select.Option>
                                                    <Select.Option value={AttendanceStatus.Leave}>Nghỉ phép (On Leave)</Select.Option>
                                                </Select>
                                            </Col>
                                            <Col xs={24} lg={5}>
                                                <Button type="primary" block onClick={handleFilter}>Lọc dữ liệu</Button>
                                            </Col>
                                        </Row>
                                    </Card>

                                    <Card style={{ borderRadius: 12, minHeight: 460 }}>
                                        <AnimatePresence mode="wait">
                                            {isLoading ? (
                                                <motion.div
                                                    key="loading"
                                                    initial={{ opacity: 0 }}
                                                    animate={{ opacity: 1 }}
                                                    exit={{ opacity: 0 }}
                                                    transition={{ duration: 0.2 }}
                                                >
                                                    <AttendanceTable
                                                        isAdmin={true}
                                                        dataSource={[]}
                                                        loading={true}
                                                    />
                                                </motion.div>
                                            ) : (
                                                <motion.div
                                                    key="data"
                                                    initial={{ opacity: 0 }}
                                                    animate={{ opacity: 1 }}
                                                    transition={{ duration: 0.3 }}
                                                >
                                                    <AttendanceTable
                                                        isAdmin={true}
                                                        dataSource={dataSource}
                                                        loading={false}
                                                        onDelete={handleDelete}
                                                        onRefresh={() => { fetchAttendance(); fetchStats(); }}
                                                    />
                                                </motion.div>
                                            )}
                                        </AnimatePresence>
                                    </Card>
                                </motion.div>
                            )
                        },
                        {
                            key: '2',
                            label: 'Duyệt đơn Nghỉ phép',
                            children: (
                                <motion.div
                                    variants={tabContentVariants}
                                    initial="hidden"
                                    animate="visible"
                                >
                                    <Card style={{ borderRadius: 12, minHeight: 460 }}>
                                        <LeaveRequestTable
                                            dataSource={leaveRequests}
                                            onApprove={handleApproveLeave}
                                            onReject={handleRejectLeave}
                                            onView={(record: LeaveRequestDto) => {
                                                modal.info({
                                                    title: 'Chi tiết đơn nghỉ phép',
                                                    content: (
                                                        <div style={{ marginTop: 16 }}>
                                                            <p><b>Nhân viên:</b> {record.employeeName} ({record.employeeCode})</p>
                                                            <p><b>Loại nghỉ:</b> {record.leaveType}</p>
                                                            <p><b>Thời gian:</b> {dayjs(record.fromDate).format('DD/MM/YYYY')} - {dayjs(record.toDate).format('DD/MM/YYYY')}</p>
                                                            <p><b>Lý do:</b> {record.reason}</p>
                                                            <p><b>Trạng thái:</b> {record.status}</p>
                                                            {record.approvedBy && <p><b>Người duyệt (ID):</b> {record.approvedBy}</p>}
                                                            {record.approvedDate && <p><b>Ngày duyệt:</b> {dayjs(record.approvedDate).format('DD/MM/YYYY HH:mm')}</p>}
                                                        </div>
                                                    ),
                                                    width: 500
                                                });
                                            }}
                                        />
                                    </Card>
                                </motion.div>
                            )
                        }
                    ]}
                />
            </motion.div>

            <AttendanceFormModal
                open={isCreateModalOpen}
                onClose={() => setIsCreateModalOpen(false)}
                onSuccess={() => { fetchAttendance(); fetchStats(); }}
            />
        </motion.div>
    );
}
