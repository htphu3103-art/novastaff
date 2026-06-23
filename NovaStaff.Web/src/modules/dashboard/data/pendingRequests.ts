import { RequestType } from '../constants/requestConfig';

export interface PendingRequest {
    id: number;
    title: string;
    time: string;
    type: RequestType;
}

export const pendingRequests: PendingRequest[] = [
    { id: 1, title: 'Đơn nghỉ phép - Nguyễn Văn A', time: '10 phút trước', type: 'Leave' },
    { id: 2, title: 'Yêu cầu tuyển dụng - Team IT', time: '1 giờ trước', type: 'Hiring' },
    { id: 3, title: 'Sửa bảng công - Trần Thị B', time: '3 giờ trước', type: 'Attendance' },
    { id: 4, title: 'Đề xuất tăng lương - Lê Văn C', time: '5 giờ trước', type: 'Salary' },
];
