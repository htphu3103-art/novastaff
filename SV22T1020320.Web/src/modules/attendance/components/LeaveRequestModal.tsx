import React, { useState } from 'react';
import { Modal, Form, DatePicker, Select, Input, message, Row, Col, Divider, Typography, Checkbox, App } from 'antd';
import { CalendarOutlined, SolutionOutlined } from '@ant-design/icons';
import { LeaveType, CreateLeaveRequest } from '../types';
import { leaveRequestApi } from '../api/leaveRequestApi';
import { useAuth } from '../../../contexts/AuthContext';
import dayjs from 'dayjs';

const { Text } = Typography;

interface LeaveRequestModalProps {
    open: boolean;
    onCancel: () => void;
    onSuccess?: () => void;
}

export const LeaveRequestModal = ({ open, onCancel, onSuccess }: LeaveRequestModalProps) => {
    const { message } = App.useApp();
    const [form] = Form.useForm();
    const { user } = useAuth();
    const [loading, setLoading] = useState(false);

    const handleOk = () => {
        if (!user) {
            message.error('Vui lòng đăng nhập lại.');
            return;
        }

        form.validateFields().then(async (values) => {
            setLoading(true);
            try {
                const [fromDate, toDate] = values.range;
                
                const requestData: CreateLeaveRequest = {
                    employeeId: 0, // Gửi 0 để Backend tự lấy từ Token (giống Swagger test)
                    leaveType: values.type,
                    fromDate: fromDate.startOf('day').toISOString(),
                    toDate: toDate.endOf('day').toISOString(),
                    isHalfDayStart: !!values.isHalfDayStart,
                    isHalfDayEnd: !!values.isHalfDayEnd,
                    reason: values.reason
                };

                console.log('Sending Leave Request Payload:', requestData);

                await leaveRequestApi.create(requestData);
                
                message.success('Gửi đơn xin nghỉ thành công!');
                form.resetFields();
                onCancel();
                if (onSuccess) onSuccess();
            } catch (error: any) {
                console.error('Create Leave Request Failed:', error);
                message.error(error.response?.data?.message || 'Có lỗi xảy ra khi gửi đơn.');
            } finally {
                setLoading(false);
            }
        }).catch(info => {
            console.log('Validate Failed:', info);
        });
    };

    return (
        <Modal
            title={
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <SolutionOutlined style={{ color: '#1890ff' }} />
                    <span>Tạo đơn xin nghỉ phép</span>
                </div>
            }
            open={open}
            onCancel={onCancel}
            onOk={handleOk}
            confirmLoading={loading}
            destroyOnHidden
            width={600}
            okText="Gửi đơn"
            cancelText="Hủy bỏ"
            centered
        >
            <Divider style={{ margin: '12px 0 24px 0' }} />

            <Form form={form} layout="vertical">
                <Row gutter={16}>
                    <Col span={12}>
                        <Form.Item
                            name="type"
                            label={<Text strong>Loại hình nghỉ</Text>}
                            rules={[{ required: true, message: 'Vui lòng chọn loại nghỉ!' }]}
                        >
                            <Select placeholder="Chọn loại" suffixIcon={<SolutionOutlined />}>
                                <Select.Option value={LeaveType.Annual}>Nghỉ phép năm</Select.Option>
                                <Select.Option value={LeaveType.Sick}>Nghỉ ốm</Select.Option>
                                <Select.Option value={LeaveType.Unpaid}>Nghỉ không lương</Select.Option>
                                <Select.Option value={LeaveType.Maternity}>Nghỉ thai sản</Select.Option>
                                <Select.Option value={LeaveType.Other}>Khác</Select.Option>
                            </Select>
                        </Form.Item>
                    </Col>
                    <Col span={12}>
                        <Form.Item
                            name="range"
                            label={<Text strong>Thời gian nghỉ</Text>}
                            rules={[{ required: true, message: 'Vui lòng chọn thời gian!' }]}
                        >
                            <DatePicker.RangePicker
                                style={{ width: '100%' }}
                                placeholder={['Bắt đầu', 'Kết thúc']}
                                suffixIcon={<CalendarOutlined />}
                                format="DD/MM/YYYY"
                            />
                        </Form.Item>
                    </Col>
                </Row>
                
                <Row gutter={16}>
                    <Col span={12}>
                        <Form.Item name="isHalfDayStart" valuePropName="checked">
                            <Checkbox>Nghỉ nửa ngày đầu</Checkbox>
                        </Form.Item>
                    </Col>
                    <Col span={12}>
                        <Form.Item name="isHalfDayEnd" valuePropName="checked">
                            <Checkbox>Nghỉ nửa ngày cuối</Checkbox>
                        </Form.Item>
                    </Col>
                </Row>

                <Form.Item
                    name="reason"
                    label={<Text strong>Lý do nghỉ</Text>}
                    rules={[{ required: true, message: 'Vui lòng nhập lý do!' }]}
                >
                    <Input.TextArea
                        rows={4}
                        placeholder="Mô tả chi tiết lý do xin nghỉ của bạn..."
                    />
                </Form.Item>

                <div style={{ background: '#f0f7ff', padding: '12px', borderRadius: '8px', border: '1px solid #bae7ff' }}>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                        * Đơn xin nghỉ của bạn sẽ được gửi tới Quản lý/HR để phê duyệt. Bạn sẽ nhận được thông báo khi trạng thái đơn thay đổi.
                    </Text>
                </div>
            </Form>
        </Modal>
    );
};
