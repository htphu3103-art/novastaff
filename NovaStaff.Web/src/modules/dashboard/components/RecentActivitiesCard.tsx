import React from 'react';
import { Card, Timeline } from 'antd';
import { recentActivities } from '../data/recentActivities';

const RecentActivitiesCard: React.FC = () => {
    return (
        <Card
            title="Hoạt động gần đây"
            style={{ borderRadius: 12, height: '100%' }}
        >
            <div style={{ padding: '8px 0' }}>
                <Timeline items={recentActivities} />
            </div>
        </Card>
    );
};

export default RecentActivitiesCard;
