import React from 'react';
import { Row, Col, Card, Avatar, Badge, Button, Typography, Space, Flex } from 'antd';
import {
    TeamOutlined,
    CheckCircleOutlined,
    RiseOutlined,
    NotificationOutlined,
    CloseCircleOutlined,
    RightOutlined
} from '@ant-design/icons';
import StatCard from './StatCard';

const { Title, Text } = Typography;

// Dữ liệu giả lập cho danh sách phê duyệt
const pendingRequests = [
    { id: 1, title: 'Đơn nghỉ phép - Nguyễn Văn A', time: '10 phút trước', type: 'Leave' },
    { id: 2, title: 'Yêu cầu tuyển dụng - Team IT', time: '1 giờ trước', type: 'Hiring' },
    { id: 3, title: 'Sửa bảng công - Trần Thị B', time: '3 giờ trước', type: 'Attendance' },
    { id: 4, title: 'Đề xuất tăng lương - Lê Văn C', time: '5 giờ trước', type: 'Salary' },
];

const AdminView: React.FC = () => {
    return (
        <div className="admin-dashboard-view">
            {/* Header Section */}
            <div style={{ marginBottom: 24 }}>
                <Title level={4} style={{ marginTop: 0 }}>Hệ thống Quản trị Chiến lược</Title>
                <Text type="secondary">Dữ liệu được cập nhật thời gian thực từ các phòng ban.</Text>
            </div>

            {/* Thống kê nhanh sử dụng StatCard */}
            <Row gutter={[16, 16]}>
                <Col xs={24} sm={12} lg={6}>
                    <StatCard
                        title="Tổng nhân sự"
                        value={150}
                        prefix={<TeamOutlined />}
                        trend={{ value: '12%', isUp: true }}
                        description="so với tháng trước"
                    />
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <StatCard
                        title="Đang có mặt"
                        value={142}
                        color="#3f8600"
                        prefix={<CheckCircleOutlined />}
                        description="94% tổng nhân sự"
                    />
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <StatCard
                        title="Vắng mặt"
                        value={8}
                        color="#cf1322"
                        prefix={<CloseCircleOutlined />}
                        description="5 có phép, 3 không phép"
                    />
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <StatCard
                        title="Quỹ lương"
                        value={1.2}
                        suffix="tỷ"
                        prefix={<RiseOutlined />}
                        color="#722ed1"
                        description="Tổng chi trả dự kiến"
                    />
                </Col>
            </Row>

            {/* Biểu đồ và Danh sách phê duyệt */}
            <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
                {/* Khu vực biểu đồ */}
                <Col xs={24} xl={16}>
                    <Card
                        title="Biến động nhân sự & Hiệu suất"
                        extra={<Button type="link" icon={<RightOutlined />}>Báo cáo chi tiết</Button>}
                        style={{ borderRadius: 12, height: '100%' }}
                    >
                        <Flex
                            vertical
                            align="center"
                            justify="center"
                            style={{
                                height: 350,
                                background: '#fafafa',
                                borderRadius: 8,
                                border: '1px dashed #d9d9d9'
                            }}
                        >
                            <RiseOutlined style={{ fontSize: 48, color: '#d9d9d9', marginBottom: 16 }} />
                            <Text type="secondary">Khu vực hiển thị biểu đồ Recharts / Ant Design Charts</Text>
                            <Text type="secondary" style={{ fontSize: 12 }}>(Biểu đồ đường thể hiện nhân sự mới và nghỉ việc)</Text>
                        </Flex>
                    </Card>
                </Col>

                {/* Danh sách yêu cầu phê duyệt - Đã sửa lỗi Deprecated List */}
                <Col xs={24} xl={8}>
                    <Card
                        title="Yêu cầu cần phê duyệt"
                        extra={<Badge count={pendingRequests.length} offset={[10, 0]} color="#f5222d" />}
                        style={{ borderRadius: 12, height: '100%' }}
                    >
                        <Flex vertical gap="middle">
                            {pendingRequests.map((item) => (
                                <Flex
                                    key={item.id}
                                    justify="space-between"
                                    align="center"
                                    style={{
                                        paddingBottom: 12,
                                        borderBottom: '1px solid #f0f0f0'
                                    }}
                                >
                                    <Space size="middle">
                                        <Avatar
                                            style={{ backgroundColor: '#f0f5ff' }}
                                            icon={<NotificationOutlined style={{ color: '#1890ff' }} />}
                                        />
                                        <div>
                                            <Text strong style={{ fontSize: 13, display: 'block' }}>
                                                {item.title}
                                            </Text>
                                            <Text type="secondary" style={{ fontSize: 12 }}>
                                                {item.time}
                                            </Text>
                                        </div>
                                    </Space>
                                    <Button type="link" size="small">Duyệt</Button>
                                </Flex>
                            ))}
                        </Flex>

                        <Button block style={{ marginTop: 16, borderRadius: 6 }}>
                            Xem tất cả yêu cầu
                        </Button>
                    </Card>
                </Col>
            </Row>
        </div>
    );
};

export default AdminView;