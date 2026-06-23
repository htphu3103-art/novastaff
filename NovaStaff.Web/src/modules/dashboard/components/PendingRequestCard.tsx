import React, { useEffect, useState } from 'react';
import { Card, Badge, Flex, Space, Avatar, Typography, Button, Skeleton, Empty } from 'antd';
import { NotificationOutlined } from '@ant-design/icons';
import { leaveRequestApi } from '../../attendance/api/leaveRequestApi';
import { LeaveRequestDto } from '../../attendance/types';
import { requestConfig } from '../constants/requestConfig';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import 'dayjs/locale/vi';

dayjs.extend(relativeTime);
dayjs.locale('vi');

const { Text } = Typography;

const PendingRequestCard: React.FC = () => {
    const [requests, setRequests] = useState<LeaveRequestDto[]>([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        leaveRequestApi.getPending()
            .then(res => setRequests(res.data))
            .catch(err => console.error('[PendingRequestCard] Lỗi fetch:', err))
            .finally(() => setLoading(false));
    }, []);

    const renderContent = () => {
        if (loading) {
            return Array.from({ length: 3 }).map((_, i) => (
                <Skeleton key={i} active avatar paragraph={{ rows: 1 }} />
            ));
        }

        if (requests.length === 0) {
            return <Empty description="Không có yêu cầu nào cần duyệt" image={Empty.PRESENTED_IMAGE_SIMPLE} />;
        }

        return requests.map((item) => {
            // Tất cả đơn từ API đều là Leave
            const config = requestConfig['Leave'];
            const title = `Đơn nghỉ phép - ${item.employeeName ?? `NV #${item.employeeId}`}`;
            const timeAgo = dayjs(item.createdDate).fromNow();

            return (
                <Flex
                    key={item.requestId}
                    justify="space-between"
                    align="center"
                    style={{
                        paddingBottom: 12,
                        borderBottom: '1px solid #f0f0f0'
                    }}
                >
                    <Space size="middle">
                        <Avatar
                            style={{ backgroundColor: `${config.color}15`, color: config.color }}
                            icon={config.icon}
                        />
                        <div>
                            <Text strong style={{ fontSize: 13, display: 'block' }}>
                                {title}
                            </Text>
                            <Text type="secondary" style={{ fontSize: 12 }}>
                                {timeAgo}
                            </Text>
                        </div>
                    </Space>
                    <Button
                        type="link"
                        size="small"
                        onClick={() => navigate('/attendance')}
                    >
                        Duyệt
                    </Button>
                </Flex>
            );
        });
    };

    return (
        <Card
            title="Yêu cầu cần phê duyệt"
            extra={
                !loading && (
                    <Badge count={requests.length} offset={[10, 0]} color="#f5222d" />
                )
            }
            style={{ borderRadius: 12, height: '100%' }}
        >
            <Flex vertical gap="middle">
                {renderContent()}
            </Flex>

            <Button
                block
                style={{ marginTop: 16, borderRadius: 6 }}
                onClick={() => navigate('/attendance')}
            >
                Xem tất cả yêu cầu
            </Button>
        </Card>
    );
};

export default PendingRequestCard;
