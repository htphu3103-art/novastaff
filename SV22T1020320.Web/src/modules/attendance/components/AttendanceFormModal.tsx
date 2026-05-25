import React, { useEffect, useState } from 'react';
import { Modal, Form, Input, DatePicker, Select, TimePicker, Row, Col, App, InputNumber } from 'antd';
import { AttendanceDto, AttendanceStatus, CreateAttendanceRequest, UpdateAttendanceRequest } from '../types';
import { attendanceApi } from '../api/attendanceApi';
import dayjs from 'dayjs';

interface AttendanceFormModalProps {
    open: boolean;
    onClose: () => void;
    onSuccess?: () => void;
    record?: AttendanceDto | null;
}

export const AttendanceFormModal: React.FC<AttendanceFormModalProps> = ({ open, onClose, onSuccess, record }) => {
    const { message } = App.useApp();
    const [form] = Form.useForm();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const isEdit = !!record;

    useEffect(() => {
        if (open && record) {
            form.setFieldsValue({
                employeeId: record.employeeId,
                workDate: record.workDate ? dayjs(record.workDate) : null,
                checkIn: record.checkIn ? dayjs(record.checkIn) : null,
                checkOut: record.checkOut ? dayjs(record.checkOut) : null,
                status: record.status,
                note: record.note,
            });
        } else if (open && !record) {
            form.resetFields();
            form.setFieldsValue({
                workDate: dayjs(),
                status: AttendanceStatus.Present
            });
        }
    }, [open, record, form]);

    const handleOk = async () => {
        try {
            const values = await form.validateFields();
            setIsSubmitting(true);

            // Chuyển đổi format dữ liệu cho API
            const workDateStr = values.workDate.format('YYYY-MM-DD');
            
            // Check-in / Check-out cần kết hợp với Work Date
            const combineDateAndTime = (date: any, time: any) => {
                if (!time) return null;
                return date.hour(time.hour()).minute(time.minute()).second(time.second()).toISOString();
            };

            const checkInIso = combineDateAndTime(values.workDate, values.checkIn);
            const checkOutIso = combineDateAndTime(values.workDate, values.checkOut);

            if (isEdit && record) {
                const updateData: UpdateAttendanceRequest = {
                    checkIn: checkInIso,
                    checkOut: checkOutIso,
                    status: values.status,
                    note: values.note
                };
                await attendanceApi.update(record.recordId, updateData);
                message.success('Cập nhật bản ghi thành công');
            } else {
                const createData: CreateAttendanceRequest = {
                    employeeId: values.employeeId,
                    workDate: workDateStr,
                    checkIn: checkInIso,
                    checkOut: checkOutIso,
                    status: values.status,
                    note: values.note
                };
                await attendanceApi.createManual(createData);
                message.success('Tạo bản ghi chấm công thành công');
            }

            onSuccess?.();
            onClose();
        } catch (error: any) {
            console.error('Attendance Form Error:', error);
            message.error(error.response?.data?.message || 'Có lỗi xảy ra khi lưu dữ liệu');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <Modal
            title={isEdit ? "Chỉnh sửa bản ghi chấm công" : "Tạo bản ghi chấm công thủ công"}
            open={open}
            onOk={handleOk}
            onCancel={onClose}
            width={600}
            confirmLoading={isSubmitting}
            destroyOnHidden
        >
            <Form form={form} layout="vertical" style={{ marginTop: 20 }}>
                <Row gutter={16}>
                    <Col span={12}>
                        <Form.Item label="Mã/ID Nhân viên" name="employeeId" rules={[{ required: true, message: 'Vui lòng nhập ID nhân viên' }]}>
                            <InputNumber style={{ width: '100%' }} placeholder="Nhập ID nhân viên" disabled={isEdit} />
                        </Form.Item>
                    </Col>
                    <Col span={12}>
                        <Form.Item label="Ngày làm việc" name="workDate" rules={[{ required: true, message: 'Vui lòng chọn ngày' }]}>
                            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" disabled={isEdit} />
                        </Form.Item>
                    </Col>
                </Row>
                
                <Row gutter={16}>
                    <Col span={12}>
                        <Form.Item label="Giờ vào (Check-in)" name="checkIn">
                            <TimePicker style={{ width: '100%' }} format="HH:mm:ss" />
                        </Form.Item>
                    </Col>
                    <Col span={12}>
                        <Form.Item label="Giờ ra (Check-out)" name="checkOut">
                            <TimePicker style={{ width: '100%' }} format="HH:mm:ss" />
                        </Form.Item>
                    </Col>
                </Row>

                <Row gutter={16}>
                    <Col span={24}>
                        <Form.Item label="Trạng thái" name="status" rules={[{ required: true, message: 'Vui lòng chọn trạng thái' }]}>
                            <Select placeholder="Chọn trạng thái">
                                <Select.Option value={AttendanceStatus.Present}>Có mặt (Present)</Select.Option>
                                <Select.Option value={AttendanceStatus.Late}>Đi muộn (Late)</Select.Option>
                                <Select.Option value={AttendanceStatus.Absent}>Vắng mặt (Absent)</Select.Option>
                                <Select.Option value={AttendanceStatus.HalfDay}>Nửa ngày (Half Day)</Select.Option>
                                <Select.Option value={AttendanceStatus.Leave}>Nghỉ phép (On Leave)</Select.Option>
                            </Select>
                        </Form.Item>
                    </Col>
                </Row>

                <Form.Item label="Ghi chú / Lý do" name="note">
                    <Input.TextArea rows={3} placeholder="Nhập ghi chú hoặc lý do chỉnh sửa..." />
                </Form.Item>
            </Form>
        </Modal>
    );
};
