import React, { useEffect, useState } from 'react';
import { Modal, Form, DatePicker, Select, Button, message } from 'antd';
import dayjs from 'dayjs';
import { payrollApi } from '../api/payrollApi';
import { CreatePayrollPeriodRequest } from '../types';

interface CreatePeriodModalProps {
    open: boolean;
    onCancel: () => void;
    onSuccess: () => void;
}

export const CreatePeriodModal: React.FC<CreatePeriodModalProps> = ({ open, onCancel, onSuccess }) => {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);

    // Auto-fill when open
    useEffect(() => {
        if (open) {
            const currentMonth = dayjs().month() + 1; // dayjs month is 0-indexed
            const currentYear = dayjs().year();
            
            // Default to start and end of current month
            const startDate = dayjs().startOf('month');
            const endDate = dayjs().endOf('month');

            form.setFieldsValue({
                monthYear: dayjs(),
                dateRange: [startDate, endDate],
            });
        }
    }, [open, form]);

    const handleMonthChange = (date: dayjs.Dayjs | null) => {
        if (date) {
            form.setFieldsValue({
                dateRange: [date.startOf('month'), date.endOf('month')]
            });
        }
    };

    const handleOk = async () => {
        try {
            const values = await form.validateFields();
            setLoading(true);

            const month = values.monthYear.month() + 1;
            const year = values.monthYear.year();
            const [start, end] = values.dateRange;

            const request: CreatePayrollPeriodRequest = {
                month,
                year,
                startDate: start.format('YYYY-MM-DD'),
                endDate: end.format('YYYY-MM-DD')
            };

            await payrollApi.createPeriod(request);
            message.success('Tạo kỳ lương thành công!');
            form.resetFields();
            onSuccess();
        } catch (error: any) {
            console.error('Lỗi khi tạo kỳ lương:', error);
            message.error(error.response?.data || 'Có lỗi xảy ra khi tạo kỳ lương');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal
            title="Tạo kỳ lương mới"
            open={open}
            onCancel={onCancel}
            onOk={handleOk}
            confirmLoading={loading}
            okText="Tạo mới"
            cancelText="Hủy"
        >
            <Form form={form} layout="vertical">
                <Form.Item
                    name="monthYear"
                    label="Tháng/Năm tính lương"
                    rules={[{ required: true, message: 'Vui lòng chọn Tháng/Năm' }]}
                >
                    <DatePicker 
                        picker="month" 
                        format="MM/YYYY" 
                        style={{ width: '100%' }} 
                        onChange={handleMonthChange}
                    />
                </Form.Item>

                <Form.Item
                    name="dateRange"
                    label="Chu kỳ (Từ ngày - Đến ngày)"
                    rules={[{ required: true, message: 'Vui lòng chọn khoảng thời gian' }]}
                >
                    <DatePicker.RangePicker 
                        format="DD/MM/YYYY" 
                        style={{ width: '100%' }} 
                    />
                </Form.Item>
            </Form>
        </Modal>
    );
};
