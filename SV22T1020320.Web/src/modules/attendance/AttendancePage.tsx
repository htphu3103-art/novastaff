import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { UserRole } from '../auth/types';
import AdminAttendancePage from './admin/AdminAttendancePage';
import EmployeeAttendancePage from './employee/EmployeeAttendancePage';

const AttendancePage: React.FC = () => {
    const { user } = useAuth();
    
    if (!user) return null;

    const isAdmin = user.role === UserRole.Admin || user.role === UserRole.Manager;

    if (isAdmin) {
        return <AdminAttendancePage />;
    }

    return <EmployeeAttendancePage />;
};

export default AttendancePage;