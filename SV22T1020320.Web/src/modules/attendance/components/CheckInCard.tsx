import React, { useEffect, useState } from 'react';
import { Button, Card, Col, Row, Space, Spin, Tag, Typography } from 'antd';
import { LoginOutlined, LogoutOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { AttendanceDto } from '../types';

const { Title, Text } = Typography;

interface CheckInCardProps {
    todayRecord?: AttendanceDto | null;
    loading?: boolean;
    actionLoading?: boolean;
    actionType?: 'check-in' | 'check-out' | null;
    onCheckIn?: () => void;
    onCheckOut?: () => void;
}

export const CheckInCard: React.FC<CheckInCardProps> = ({
    todayRecord,
    loading = false,
    actionLoading = false,
    actionType = null,
    onCheckIn,
    onCheckOut
}) => {
    const [now, setNow] = useState(dayjs());

    useEffect(() => {
        const timer = setInterval(() => setNow(dayjs()), 1000);
        return () => clearInterval(timer);
    }, []);

    const canCheckIn = !todayRecord?.isCheckedIn;
    const canCheckOut = !!todayRecord?.isCheckedIn && !todayRecord?.isCheckedOut;

    const checkInText = todayRecord?.checkIn ? dayjs(todayRecord.checkIn).format('HH:mm:ss') : '--:--:--';
    const checkOutText = todayRecord?.checkOut ? dayjs(todayRecord.checkOut).format('HH:mm:ss') : '--:--:--';
    const statusText = todayRecord?.statusDisplay || (todayRecord?.isCheckedIn ? 'Working' : 'Not checked in');
    const statusColor = todayRecord?.isCheckedOut ? 'success' : todayRecord?.isCheckedIn ? 'processing' : 'default';

    return (
        <Card style={{ marginBottom: 16, background: 'linear-gradient(135deg, #ffffff 0%, #f0f5ff 100%)', border: '1px solid #adc6ff' }}>
            <Spin spinning={loading}>
                <Row align="middle" justify="space-between" gutter={[16, 16]}>
                    <Col>
                        <Space orientation="vertical" size={0}>
                            <Text type="secondary">Today is: {now.format('DD/MM/YYYY')}</Text>
                            <Title level={2} style={{ margin: 0, color: '#1d39c4' }}>{now.format('HH:mm:ss')}</Title>
                            <Space wrap>
                                <Tag color={statusColor}>Status: {statusText}</Tag>
                                <Tag color="blue">In: {checkInText}</Tag>
                                <Tag color="purple">Out: {checkOutText}</Tag>
                            </Space>
                        </Space>
                    </Col>
                    <Col>
                        <Space size="middle" style={{ width: '100%', justifyContent: 'flex-start' }} wrap>
                            <Button
                                type="primary"
                                size="large"
                                icon={<LoginOutlined />}
                                onClick={onCheckIn}
                                disabled={!canCheckIn || actionLoading}
                                loading={actionLoading && actionType === 'check-in'}
                                style={{ minWidth: 120 }}
                            >
                                Check-in
                            </Button>
                            <Button
                                size="large"
                                icon={<LogoutOutlined />}
                                onClick={onCheckOut}
                                disabled={!canCheckOut || actionLoading}
                                loading={actionLoading && actionType === 'check-out'}
                                style={{ minWidth: 120 }}
                            >
                                Check-out
                            </Button>
                        </Space>
                    </Col>
                </Row>
            </Spin>
        </Card>
    );
};
