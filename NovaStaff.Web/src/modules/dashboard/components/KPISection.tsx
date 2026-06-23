import React from 'react';
import { Row, Col, Card } from 'antd';
import {
    TeamOutlined,
    CheckCircleOutlined,
    CloseCircleOutlined,
    NotificationOutlined,
    UserAddOutlined,
    RiseOutlined
} from '@ant-design/icons';
import StatCard from './StatCard';

const KPISection: React.FC = () => {
    return (
        <Row gutter={[16, 16]}>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Tổng nhân sự"
                    value={150}
                    prefix={<TeamOutlined />}
                    trend={{ value: '12%', isUp: true }}
                    description="so với tháng trước"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Có mặt hôm nay"
                    value={142}
                    color="#52c41a"
                    prefix={<CheckCircleOutlined />}
                    description="94% tổng nhân sự"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Vắng mặt"
                    value={8}
                    color="#ff4d4f"
                    prefix={<CloseCircleOutlined />}
                    description="5 có phép, 3 không phép"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Đơn chờ duyệt"
                    value={4}
                    color="#fa8c16"
                    prefix={<NotificationOutlined />}
                    description="Yêu cầu mới cần xử lý"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Nhân viên mới"
                    value={12}
                    color="#1677ff"
                    prefix={<UserAddOutlined />}
                    trend={{ value: '8%', isUp: true }}
                    description="so với tháng trước"
                />
            </Col>
            <Col xs={12} sm={8} lg={4}>
                <StatCard
                    title="Tỷ lệ đi làm"
                    value={94}
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
