import React from 'react';
import AdminView from './AdminView';
import EmployeeView from './components/EmployeeView';
import { useAuth } from '../../contexts/AuthContext';
import { UserRole } from '../auth/types';

const DashboardPage: React.FC = () => {
    const { user } = useAuth();
    const isAdmin = user?.role === UserRole.Admin || user?.role === UserRole.Manager;

    return (
        <div style={{ padding: '4px' }}>
            {isAdmin ? <AdminView /> : <EmployeeView />}
        </div>
    );
};

export default DashboardPage;