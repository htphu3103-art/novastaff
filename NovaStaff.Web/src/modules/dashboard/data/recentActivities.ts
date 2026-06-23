export interface RecentActivity {
    color?: string;
    children: string;
}

export const recentActivities: RecentActivity[] = [
    { color: 'green', children: 'Nguyễn Văn A check-in lúc 08:00' },
    { color: 'blue', children: 'Trần Văn B tạo đơn nghỉ phép lúc 08:50' },
    { color: 'orange', children: 'Team IT tạo yêu cầu tuyển dụng lúc 08:30' },
    { color: 'purple', children: 'Nguyễn Văn C được thêm vào phòng ban lúc 08:00' },
];
