import React from 'react';
import { Row, Col, Card, Avatar, Badge, Button, Typography, Space, Flex } from 'antd';
import { NotificationOutlined, RiseOutlined, RightOutlined } from '@ant-design/icons';
import KPISection from './components/KPISection';

const { Title, Text } = Typography;

// Dữ liệu giả lập tạm thời cho danh sách phê duyệt (sẽ tách file sau)
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

            {/* KPI Cards (6 card) */}
            <div style={{ marginBottom: 24 }}>
                <KPISection />
            </div>

            {/* Biểu đồ và Danh sách phê duyệt (tạm thời inline trước khi modular hóa) */}
            <Row gutter={[16, 16]}>
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

                {/* Danh sách yêu cầu phê duyệt */}
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
