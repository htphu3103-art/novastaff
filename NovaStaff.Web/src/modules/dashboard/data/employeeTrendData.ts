export interface EmployeeTrend {
    month: string;
    newEmployees: number;
    leftEmployees: number;
}

export const employeeTrendData: EmployeeTrend[] = [
    { month: 'Jan', newEmployees: 10, leftEmployees: 2 },
    { month: 'Feb', newEmployees: 15, leftEmployees: 3 },
    { month: 'Mar', newEmployees: 8, leftEmployees: 1 },
    { month: 'Apr', newEmployees: 20, leftEmployees: 4 },
    { month: 'May', newEmployees: 12, leftEmployees: 5 },
    { month: 'Jun', newEmployees: 25, leftEmployees: 3 },
];
