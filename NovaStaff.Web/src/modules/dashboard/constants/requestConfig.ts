import React from 'react';
import { CalendarOutlined, ClockCircleOutlined, DollarOutlined, UserAddOutlined } from '@ant-design/icons';

export const requestConfig = {
    Leave: {
        icon: React.createElement(CalendarOutlined),
        color: '#fa8c16'
    },
    Attendance: {
        icon: React.createElement(ClockCircleOutlined),
        color: '#1677ff'
    },
    Salary: {
        icon: React.createElement(DollarOutlined),
        color: '#722ed1'
    },
    Hiring: {
        icon: React.createElement(UserAddOutlined),
        color: '#52c41a'
    }
};

export type RequestType = keyof typeof requestConfig;
