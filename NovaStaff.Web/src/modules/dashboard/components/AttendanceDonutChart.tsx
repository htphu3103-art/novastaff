import React from 'react';
import { Card } from 'antd';
import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip, Legend } from 'recharts';
import { attendanceData } from '../data/attendanceData';

const AttendanceDonutChart: React.FC = () => {
    return (
        <Card
            title="Tỷ lệ hiện diện hôm nay"
            style={{ borderRadius: 12, height: '100%' }}
        >
            <div style={{ position: 'relative', width: '100%', height: 350 }}>
                <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                        <Pie
                            data={attendanceData}
                            dataKey="value"
                            nameKey="name"
                            cx="50%"
                            cy="50%"
                            innerRadius={70}
                            outerRadius={95}
                            paddingAngle={5}
                            labelLine={false}
                        >
                            {attendanceData.map((entry, index) => (
                                <Cell key={`cell-${index}`} fill={entry.color} />
                            ))}
                        </Pie>
                        <Tooltip formatter={(value) => `${value}%`} />
                        <Legend iconType="circle" wrapperStyle={{ fontSize: 12, paddingTop: 10 }} />
                    </PieChart>
                </ResponsiveContainer>
                
                {/* Text ở tâm biểu đồ Donut */}
                <div style={{
                    position: 'absolute',
                    top: '44%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    textAlign: 'center',
                    pointerEvents: 'none'
                }}>
                    <div style={{ fontSize: 28, fontWeight: 700, color: '#52c41a', lineHeight: 1 }}>94%</div>
                    <div style={{ fontSize: 12, color: '#8c8c8c', marginTop: 4 }}>Có mặt</div>
                </div>
            </div>
        </Card>
    );
};

export default AttendanceDonutChart;
