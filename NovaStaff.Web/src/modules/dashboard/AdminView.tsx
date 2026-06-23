import React from 'react';
import { Row, Col } from 'antd';
import KPISection from './components/KPISection';
import EmployeeTrendChart from './components/EmployeeTrendChart';
import PendingRequestCard from './components/PendingRequestCard';
import RecentActivitiesCard from './components/RecentActivitiesCard';


const AdminView: React.FC = () => {
    return (
        <div className="admin-dashboard-view">
            {/* KPI Cards (6 card) */}
            <div style={{ marginBottom: 24 }}>
                <KPISection />
            </div>

            {/* Hàng 2: Biểu đồ */}
            <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
                {/* Khu vực biểu đồ biến động nhân sự */}
                <Col xs={24} xl={24}>
                    <EmployeeTrendChart />
                </Col>
            </Row>

            {/* Hàng 3: Vận hành (Yêu cầu cần duyệt & Hoạt động gần đây) */}
            <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
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
