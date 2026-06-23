import React from 'react';
import { Card, Badge, Flex, Space, Avatar, Typography, Button } from 'antd';
import { NotificationOutlined } from '@ant-design/icons';
import { pendingRequests } from '../data/pendingRequests';
import { requestConfig } from '../constants/requestConfig';

const { Text } = Typography;

const PendingRequestCard: React.FC = () => {
    return (
        <Card
            title="Yêu cầu cần phê duyệt"
            extra={<Badge count={pendingRequests.length} offset={[10, 0]} color="#f5222d" />}
            style={{ borderRadius: 12, height: '100%' }}
        >
            <Flex vertical gap="middle">
                {pendingRequests.map((item) => {
                    const config = requestConfig[item.type] || {
                        icon: React.createElement(NotificationOutlined),
                        color: '#1890ff'
                    };

                    return (
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
                                    style={{ backgroundColor: `${config.color}15` }} // Adds transparency to background
                                    icon={React.cloneElement(config.icon as React.ReactElement, { style: { color: config.color } })}
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
                    );
                })}
            </Flex>

            <Button block style={{ marginTop: 16, borderRadius: 6 }}>
                Xem tất cả yêu cầu
            </Button>
        </Card>
    );
};

export default PendingRequestCard;
