import React from 'react';
import { Drawer, Descriptions, Tag, Space, Typography } from 'antd';
import { AttendanceDto, AttendanceStatus } from '../types';
import dayjs from 'dayjs';

const { Text } = Typography;

interface AttendanceDetailsDrawerProps {
    open: boolean;
    onClose: () => void;
    record?: AttendanceDto | null;
}

export const AttendanceDetailsDrawer: React.FC<AttendanceDetailsDrawerProps> = ({ open, onClose, record }) => {
    if (!record) return null;

    const getStatusTag = (status: AttendanceStatus, text: string) => {
        let color = 'default';
        switch (status) {
            case AttendanceStatus.Present: color = 'green'; break;
            case AttendanceStatus.Late: color = 'warning'; break;
            case AttendanceStatus.Absent: color = 'error'; break;
            case AttendanceStatus.HalfDay: color = 'blue'; break;
            case AttendanceStatus.Leave: color = 'purple'; break;
        }
        return <Tag color={color}>{text}</Tag>;
    };

    return (
        <Drawer
            title="Attendance Details"
            placement="right"
            onClose={onClose}
            open={open}
            width={400}
        >
            <Descriptions column={1} bordered size="small">
                <Descriptions.Item label="Employee">
                    <Space orientation="vertical" size={0}>
                        <strong>{record.employeeName}</strong>
                        <Text type="secondary">{record.employeeCode}</Text>
                    </Space>
                </Descriptions.Item>
                
                <Descriptions.Item label="Work Date">
                    {record.workDate ? dayjs(record.workDate).format('dddd, DD/MM/YYYY') : '-'}
                </Descriptions.Item>

                <Descriptions.Item label="Status">
                    {getStatusTag(record.status, record.statusDisplay)}
                </Descriptions.Item>

                <Descriptions.Item label="Check-in">
                    {record.checkIn ? <Text type="success">{dayjs(record.checkIn).format('HH:mm:ss')}</Text> : '--:--:--'}
                </Descriptions.Item>

                <Descriptions.Item label="Check-out">
                    {record.checkOut ? <Text type="danger">{dayjs(record.checkOut).format('HH:mm:ss')}</Text> : '--:--:--'}
                </Descriptions.Item>

                <Descriptions.Item label="Total Hours">
                    {record.workHours ? `${record.workHours} hours` : '-'}
                </Descriptions.Item>

                <Descriptions.Item label="Note/Reason">
                    {record.note || <i>No notes</i>}
                </Descriptions.Item>
            </Descriptions>
            
            <div style={{ marginTop: 24 }}>
                <Text type="secondary" style={{ fontSize: 12 }}>
                    Created: {record.createdDate ? dayjs(record.createdDate).format('DD/MM/YYYY HH:mm') : '-'}
                </Text>
            </div>
        </Drawer>
    );
};
