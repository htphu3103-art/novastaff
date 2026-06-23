import React, { useEffect, useState } from 'react';
import { Row, Col, Skeleton } from 'antd';
import {
    TeamOutlined,
    CheckCircleOutlined,
    CloseCircleOutlined,
    NotificationOutlined,
    UserAddOutlined,
    RiseOutlined
} from '@ant-design/icons';
import StatCard from './StatCard';
import { dashboardApi, KpiSummaryDto } from '../api/dashboardApi';

const KPISection: React.FC = () => {
    const [data, setData] = useState<KpiSummaryDto | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const controller = new AbortController();

        const fetchAll = async () => {
            setLoading(true);
            try {
                const response = await dashboardApi.getKpiSummary(controller.signal);
                setData(response.data);
            } catch (err: any) {
                if (err?.name !== 'CanceledError' && err?.code !== 'ERR_CANCELED') {
                    console.error('[KPISection] Lỗi fetch dashboard KPI:', err);
                }
            } finally {
                setLoading(false);
            }
        };

        fetchAll();
        return () => controller.abort();
    }, []);

    if (loading) {
        return (
            <Row gutter={[16, 16]}>
                {Array.from({ length: 6 }).map((_, i) => (
                    <Col key={i} xs={12} sm={8} lg={4}>
                        <Skeleton.Node active style={{ width: '100%', height: 110, borderRadius: 12 }} />
                    </Col>
                ))}
            </Row>
        );
    }

    const absentDesc = [
        data?.attendance?.absentWithLeave ? `${data.attendance.absentWithLeave} có phép` : null,
        data?.attendance?.absentWithoutLeave ? `${data.attendance.absentWithoutLeave} không phép` : null,
    ].filter(Boolean).join(', ') || 'Không có dữ liệu';

    return (
        <Row gutter={[16, 16]}>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Tổng nhân sự"
                    value={data?.totalEmployees ?? 0}
                    prefix={<TeamOutlined />}
                    description="Nhân viên đang hoạt động"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Có mặt hôm nay"
                    value={data?.attendance?.presentToday ?? 0}
                    color="#52c41a"
                    prefix={<CheckCircleOutlined />}
                    description={`${data?.attendance?.attendanceRate ?? 0}% tổng nhân sự`}
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Vắng mặt"
                    value={data?.attendance?.absentToday ?? 0}
                    color="#ff4d4f"
                    prefix={<CloseCircleOutlined />}
                    description={absentDesc}
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Đơn chờ duyệt"
                    value={data?.pendingRequests ?? 0}
                    color="#fa8c16"
                    prefix={<NotificationOutlined />}
                    description="Yêu cầu mới cần xử lý"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Nhân viên mới"
                    value={data?.newHires?.thisMonth ?? 0}
                    color="#1677ff"
                    prefix={<UserAddOutlined />}
                    trend={
                        data && data.newHires.lastMonth > 0
                            ? {
                                value: `${Math.abs(Math.round(data.newHires.growthRatePercent))}%`,
                                isUp: data.newHires.thisMonth >= data.newHires.lastMonth,
                            }
                            : undefined
                    }
                    description="so với tháng trước"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Tỷ lệ đi làm"
                    value={data?.attendance?.attendanceRate ?? 0}
                    suffix="%"
                    color="#13c2c2"
                    prefix={<RiseOutlined />}
                    description="Đạt mục tiêu vận hành"
                />
            </Col>
        </Row>
    );
};

export default KPISection;
