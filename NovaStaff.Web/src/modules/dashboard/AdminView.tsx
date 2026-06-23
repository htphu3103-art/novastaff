import React from 'react';
import { Row, Col, Typography } from 'antd';
import KPISection from './components/KPISection';
import EmployeeTrendChart from './components/EmployeeTrendChart';
import AttendanceDonutChart from './components/AttendanceDonutChart';
import PendingRequestCard from './components/PendingRequestCard';
import RecentActivitiesCard from './components/RecentActivitiesCard';

const { Title, Text } = Typography;

const AdminView: React.FC = () => {
    return (
        <div className="admin-dashboard-view">
            {/* Header Section */}
            <div style={{ marginBottom: 24 }}>
                <Title level={4} style={{ marginTop: 0 }}>Hệ thống Quản trị Chiến lược</Title>
                <Text type="secondary">Dữ liệu được cập nhật thời gian thực từ các phòng ban.</Text>
            </div>

            {/* KPI Cards (6 card) */}
            <div style={{ marginBottom: 24 }}>
                <KPISection />
            </div>

            {/* Hàng 2: Biểu đồ */}
            <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
                {/* Khu vực biểu đồ biến động nhân sự */}
                <Col xs={24} xl={16}>
                    <EmployeeTrendChart />
                </Col>

                {/* Biểu đồ tròn tỷ lệ hiện diện */}
                <Col xs={24} xl={8}>
                    <AttendanceDonutChart />
                </Col>
            </Row>

            {/* Hàng 3: Vận hành (Yêu cầu cần duyệt & Hoạt động gần đây) */}
            <Row gutter={[16, 16]}>
                {/* Danh sách yêu cầu phê duyệt */}
                <Col xs={24} xl={12}>
                    <PendingRequestCard />
                </Col>

                {/* Hoạt động gần đây */}
                <Col xs={24} xl={12}>
                    <RecentActivitiesCard />
                </Col>
            </Row>
        </div>
    );
};

export default AdminView;
