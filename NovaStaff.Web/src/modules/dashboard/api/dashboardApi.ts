import { axiosClient } from '@/utils/axiosClient';

export interface AttendanceSummary {
    presentToday: number;
    absentToday: number;
    absentWithLeave: number;
    absentWithoutLeave: number;
    attendanceRate: number;
}

export interface NewHiresSummary {
    thisMonth: number;
    lastMonth: number;
    growthRatePercent: number;
}

export interface KpiSummaryDto {
    totalEmployees: number;
    attendance: AttendanceSummary;
    pendingRequests: number;
    newHires: NewHiresSummary;
}

export interface EmployeeTrendDto {
    month: string;
    year: number;
    newEmployees: number;
    leftEmployees: number;
    taskCompletionRate: number;
}

export const dashboardApi = {
    getKpiSummary: (signal?: AbortSignal) => {
        return axiosClient.get<KpiSummaryDto>('/dashboard/kpi-summary', { signal });
    },
    getEmployeeTrends: (limit: number = 6, signal?: AbortSignal) => {
        return axiosClient.get<EmployeeTrendDto[]>('/dashboard/employee-trends', {
            params: { limit },
            signal
        });
    }
};
