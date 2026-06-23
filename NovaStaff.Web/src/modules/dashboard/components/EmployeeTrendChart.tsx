import React, { useEffect, useState } from 'react';
import { Card, Button, Skeleton } from 'antd';
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
import { dashboardApi, EmployeeTrendDto } from '../api/dashboardApi';

const EmployeeTrendChart: React.FC = () => {
    const [data, setData] = useState<EmployeeTrendDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const controller = new AbortController();
        const fetchData = async () => {
            setLoading(true);
            try {
                const response = await dashboardApi.getEmployeeTrends(6, controller.signal);
                setData(response.data);
            } catch (err: any) {
                if (err?.name !== 'CanceledError' && err?.code !== 'ERR_CANCELED') {
                    console.error('[EmployeeTrendChart] Lỗi fetch trends:', err);
                }
            } finally {
                setLoading(false);
            }
        };

        fetchData();
        return () => controller.abort();
    }, []);

    if (loading) {
        return (
            <Card
                title="Biến động nhân sự & Hiệu suất"
                extra={<Button type="link" icon={<RightOutlined />}>Báo cáo chi tiết</Button>}
                style={{ borderRadius: 12, height: '100%' }}
            >
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 350 }}>
                    <Skeleton active paragraph={{ rows: 8 }} />
                </div>
            </Card>
        );
    }

    return (
        <Card
            title="Biến động nhân sự & Hiệu suất"
            extra={<Button type="link" icon={<RightOutlined />}>Báo cáo chi tiết</Button>}
            style={{ borderRadius: 12, height: '100%' }}
        >
            <div style={{ position: 'relative', width: '100%', height: 350 }}>
                <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={data} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                        <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                        <XAxis dataKey="month" tickLine={false} style={{ fontSize: 12 }} />
                        <YAxis yAxisId="left" tickLine={false} style={{ fontSize: 12 }} />
                        <YAxis 
                            yAxisId="right" 
                            orientation="right" 
                            tickLine={false} 
                            style={{ fontSize: 12 }} 
                            tickFormatter={(value) => `${value}%`}
                        />
                        <Tooltip 
                            contentStyle={{ borderRadius: 8, border: '1px solid #f0f0f0' }} 
                            formatter={(value: any, name: any) => {
                                if (name === 'Hiệu suất công việc') return [`${value}%`, name];
                                return [value, name];
                            }}
                        />
                        <Legend iconType="circle" wrapperStyle={{ fontSize: 12, paddingTop: 10 }} />
                        <Line
                            yAxisId="left"
                            type="monotone"
                            dataKey="newEmployees"
                            name="Nhân viên mới"
                            stroke="#1677ff"
                            strokeWidth={3}
                            dot={false}
                            activeDot={{ r: 6 }}
                        />
                        <Line
                            yAxisId="left"
                            type="monotone"
                            dataKey="leftEmployees"
                            name="Nhân viên nghỉ việc"
                            stroke="#ff4d4f"
                            strokeWidth={3}
                            dot={false}
                            activeDot={{ r: 6 }}
                        />
                        <Line
                            yAxisId="right"
                            type="monotone"
                            dataKey="taskCompletionRate"
                            name="Hiệu suất công việc"
                            stroke="#52c41a"
                            strokeWidth={3}
                            dot={false}
                            activeDot={{ r: 6 }}
                        />
                    </LineChart>
                </ResponsiveContainer>
            </div>
        </Card>
    );
};

export default EmployeeTrendChart;
