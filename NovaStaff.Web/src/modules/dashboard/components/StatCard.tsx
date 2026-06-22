import React from 'react';
import { Card, Statistic, Typography, Space } from 'antd';

const { Text } = Typography;

interface StatCardProps {
    title: string;
    value: string | number;
    prefix?: React.ReactNode;
    suffix?: string;
    color?: string;
    trend?: {
        value: string;
        isUp: boolean;
    };
    description?: string;
}

const StatCard: React.FC<StatCardProps> = ({
    title,
    value,
    prefix,
    suffix,
    color = '#1890ff',
    trend,
    description
}) => {
    return (
        <Card
            variant="borderless" // Cập nhật từ bordered={false}
            hoverable
            style={{ borderRadius: 12, height: '100%' }}
        >
            <Statistic
                title={<Text type="secondary" strong>{title.toUpperCase()}</Text>}
                value={value}
                prefix={prefix}
                suffix={suffix}
                // Cập nhật: AntD 6.x dùng styles.content thay cho valueStyle
                styles={{
                    content: { color: color, fontWeight: 700, fontSize: 24 }
                }}
            />

            {(trend || description) && (
                <div style={{ marginTop: 12 }}>
                    <Space size={4}>
                        {trend && (
                            <Text type={trend.isUp ? 'success' : 'danger'} style={{ fontSize: 13, fontWeight: 600 }}>
                                {trend.isUp ? '↑' : '↓'} {trend.value}
                            </Text>
                        )}
                        {description && (
                            <Text type="secondary" style={{ fontSize: 12 }}>
                                {description}
                            </Text>
                        )}
                    </Space>
                </div>
            )}
        </Card>
    );
};

export default StatCard;