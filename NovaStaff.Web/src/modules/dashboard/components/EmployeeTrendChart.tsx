import React from 'react';
import { Card, Button } from 'antd';
import { RightOutlined } from '@ant-design/icons';
import {
    ResponsiveContainer,
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend
} from 'recharts';
import { employeeTrendData } from '../data/employeeTrendData';

const EmployeeTrendChart: React.FC = () => {
    return (
        <Card
            title="Biến động nhân sự & Hiệu suất"
            extra={<Button type="link" icon={<RightOutlined />}>Báo cáo chi tiết</Button>}
            style={{ borderRadius: 12, height: '100%' }}
        >
            <ResponsiveContainer width="100%" height={350}>
                <LineChart data={employeeTrendData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                    <XAxis dataKey="month" tickLine={false} style={{ fontSize: 12 }} />
                    <YAxis tickLine={false} style={{ fontSize: 12 }} />
                    <Tooltip contentStyle={{ borderRadius: 8, border: '1px solid #f0f0f0' }} />
                    <Legend iconType="circle" wrapperStyle={{ fontSize: 12, paddingTop: 10 }} />
                    <Line
                        type="monotone"
                        dataKey="newEmployees"
                        name="Nhân viên mới"
                        stroke="#1677ff"
                        strokeWidth={3}
                        dot={false}
                        activeDot={{ r: 6 }}
                    />
                    <Line
                        type="monotone"
                        dataKey="leftEmployees"
                        name="Nhân viên nghỉ việc"
                        stroke="#ff4d4f"
                        strokeWidth={3}
                        dot={false}
                        activeDot={{ r: 6 }}
                    />
                </LineChart>
            </ResponsiveContainer>
        </Card>
    );
};

export default EmployeeTrendChart;
