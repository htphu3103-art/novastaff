export interface AttendanceData {
    name: string;
    value: number;
    color: string;
}

export const attendanceData: AttendanceData[] = [
    { name: 'Có mặt', value: 94, color: '#52c41a' },
    { name: 'Vắng mặt', value: 6, color: '#ff4d4f' },
];
