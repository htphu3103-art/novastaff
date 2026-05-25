import React, { useMemo, useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Button, Space, Typography, Divider, Row, Col, Card, message } from 'antd';
import { PlusOutlined, DeleteOutlined, DollarOutlined, InfoCircleOutlined } from '@ant-design/icons';
import { PayrollDetailDto } from '../types';

const { Title, Text } = Typography;

interface AdjustmentModalProps {
    open: boolean;
    onCancel: () => void;
    onSave?: (values: any) => void;
    data: PayrollDetailDto | null;
    loading?: boolean;
}

export const AdjustmentModal: React.FC<AdjustmentModalProps> = ({ open, onCancel, onSave, data, loading }) => {
    const [form] = Form.useForm();

    // Reset form khi mở modal hoặc khi dữ liệu nhân viên thay đổi
    useEffect(() => {
        if (open && data) {
            form.setFieldsValue({
                bonusAndAllowances: data.bonusAndAllowances || [],
                deductions: data.deductions || []
            });
        } else if (!open) {
            form.resetFields();
        }
    }, [open, data, form]);

    // Theo dõi giá trị của form để tính toán real-time
    const bonusAndAllowances = Form.useWatch('bonusAndAllowances', form) || [];
    const deductions = Form.useWatch('deductions', form) || [];

    // Tính toán tổng thu nhập và khấu trừ
    const totals = useMemo(() => {
        const totalBonus = bonusAndAllowances.reduce((sum: number, item: any) => sum + (Number(item?.amount) || 0), 0);
        const totalDeduction = deductions.reduce((sum: number, item: any) => sum + (Number(item?.amount) || 0), 0);
        const baseSalary = data?.baseSalarySnapshot || 0;
        const estimatedNet = baseSalary + totalBonus - totalDeduction;

        return { totalBonus, totalDeduction, estimatedNet };
    }, [bonusAndAllowances, deductions, data]);

    const handleFinish = (values: any) => {
        onSave?.(values);
    };

    const handleFinishFailed = () => {
        message.error('Vui lòng kiểm tra lại thông tin các khoản điều chỉnh (Tên và Số tiền không được để trống)');
    };

    return (
        <Modal
            title={
                <Space>
                    <DollarOutlined style={{ color: '#1890ff' }} />
                    <span style={{ fontSize: 18 }}>Điều chỉnh lương nhân viên</span>
                </Space>
            }
            open={open}
            onCancel={onCancel}
            onOk={() => form.submit()}
            width={850}
            confirmLoading={loading}
            okText="Lưu thay đổi"
            cancelText="Hủy bỏ"
            destroyOnClose
        >
            <div style={{ marginBottom: 20, padding: '12px 16px', background: '#f0f7ff', borderRadius: 8, border: '1px solid #bae0ff' }}>
                <Row gutter={16}>
                    <Col span={12}>
                        <Text type="secondary">Nhân viên:</Text>
                        <Title level={5} style={{ margin: 0 }}>{data?.fullName} ({data?.employeeCode})</Title>
                    </Col>
                    <Col span={12} style={{ textAlign: 'right' }}>
                        <Text type="secondary">Lương cơ bản (Snapshot):</Text>
                        <div style={{ fontSize: 18, fontWeight: 700, color: '#003eb3' }}>
                            {data?.baseSalarySnapshot.toLocaleString()}đ
                        </div>
                    </Col>
                </Row>
            </div>

            <Form
                form={form}
                layout="vertical"
                onFinish={handleFinish}
                onFinishFailed={handleFinishFailed}
            >
                <Row gutter={32}>
                    {/* Cột Phụ cấp & Thưởng */}
                    <Col span={12}>
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
                            <Title level={5} style={{ margin: 0, color: '#389e0d' }}>
                                <PlusOutlined /> Phụ cấp & Thưởng
                            </Title>
                            <Text strong style={{ color: '#389e0d' }}>+{totals.totalBonus.toLocaleString()}đ</Text>
                        </div>
                        
                        <Form.List name="bonusAndAllowances">
                            {(fields, { add, remove }) => (
                                <>
                                    <div style={{ maxHeight: '400px', overflowY: 'auto', paddingRight: 8 }}>
                                        {fields.map(({ key, name, ...restField }) => (
                                            <Card key={key} size="small" style={{ marginBottom: 12, borderLeft: '3px solid #389e0d' }}>
                                                <Row gutter={8}>
                                                    <Col span={14}>
                                                        <Form.Item {...restField} name={[name, 'name']} rules={[{ required: true, message: 'Nhập tên khoản' }]}>
                                                            <Input placeholder="Tên khoản (vd: Thưởng KPI)" />
                                                        </Form.Item>
                                                    </Col>
                                                    <Col span={10}>
                                                        <Form.Item {...restField} name={[name, 'amount']} rules={[{ required: true, message: 'Nhập số tiền' }]}>
                                                            <InputNumber
                                                                style={{ width: '100%' }}
                                                                placeholder="Số tiền"
                                                                formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                                                                parser={value => value!.replace(/\$\s?|(,*)/g, '')}
                                                                addonAfter="đ"
                                                            />
                                                        </Form.Item>
                                                    </Col>
                                                    <Col span={22}>
                                                        <Form.Item {...restField} name={[name, 'note']} style={{ marginBottom: 0 }}>
                                                            <Input.TextArea placeholder="Ghi chú lý do..." autoSize={{ minRows: 1 }} />
                                                        </Form.Item>
                                                    </Col>
                                                    <Col span={2} style={{ display: 'flex', alignItems: 'flex-end', justifyContent: 'center' }}>
                                                        <Button type="text" danger icon={<DeleteOutlined />} onClick={() => remove(name)} />
                                                    </Col>
                                                </Row>
                                            </Card>
                                        ))}
                                    </div>
                                    <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />} style={{ color: '#389e0d', borderColor: '#389e0d' }}>
                                        Thêm khoản thu nhập
                                    </Button>
                                </>
                            )}
                        </Form.List>
                    </Col>

                    {/* Cột Khấu trừ */}
                    <Col span={12}>
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
                            <Title level={5} style={{ margin: 0, color: '#cf1322' }}>
                                <DeleteOutlined /> Các khoản khấu trừ
                            </Title>
                            <Text strong style={{ color: '#cf1322' }}>-{totals.totalDeduction.toLocaleString()}đ</Text>
                        </div>

                        <Form.List name="deductions">
                            {(fields, { add, remove }) => (
                                <>
                                    <div style={{ maxHeight: '400px', overflowY: 'auto', paddingRight: 8 }}>
                                        {fields.map(({ key, name, ...restField }) => (
                                            <Card key={key} size="small" style={{ marginBottom: 12, borderLeft: '3px solid #cf1322' }}>
                                                <Row gutter={8}>
                                                    <Col span={14}>
                                                        <Form.Item {...restField} name={[name, 'name']} rules={[{ required: true, message: 'Nhập tên khoản' }]}>
                                                            <Input placeholder="Tên khoản (vd: Phạt đi muộn)" />
                                                        </Form.Item>
                                                    </Col>
                                                    <Col span={10}>
                                                        <Form.Item {...restField} name={[name, 'amount']} rules={[{ required: true, message: 'Nhập số tiền' }]}>
                                                            <InputNumber
                                                                style={{ width: '100%' }}
                                                                placeholder="Số tiền"
                                                                formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                                                                parser={value => value!.replace(/\$\s?|(,*)/g, '')}
                                                                addonAfter="đ"
                                                            />
                                                        </Form.Item>
                                                    </Col>
                                                    <Col span={22}>
                                                        <Form.Item {...restField} name={[name, 'note']} style={{ marginBottom: 0 }}>
                                                            <Input.TextArea placeholder="Ghi chú lý do..." autoSize={{ minRows: 1 }} />
                                                        </Form.Item>
                                                    </Col>
                                                    <Col span={2} style={{ display: 'flex', alignItems: 'flex-end', justifyContent: 'center' }}>
                                                        <Button type="text" danger icon={<DeleteOutlined />} onClick={() => remove(name)} />
                                                    </Col>
                                                </Row>
                                            </Card>
                                        ))}
                                    </div>
                                    <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />} style={{ color: '#cf1322', borderColor: '#cf1322' }}>
                                        Thêm khoản khấu trừ
                                    </Button>
                                </>
                            )}
                        </Form.List>
                    </Col>
                </Row>
            </Form>

            <Divider style={{ margin: '24px 0 16px 0' }} />

            <div style={{ background: '#fafafa', padding: '16px 24px', borderRadius: 12, border: '1px solid #f0f0f0' }}>
                <Row justify="space-between" align="middle">
                    <Col>
                        <Space>
                            <InfoCircleOutlined style={{ color: '#8c8c8c' }} />
                            <Text type="secondary">Thay đổi này sẽ ảnh hưởng trực tiếp đến lương thực lĩnh tháng này của nhân viên.</Text>
                        </Space>
                    </Col>
                    <Col style={{ textAlign: 'right' }}>
                        <div style={{ fontSize: '13px', color: '#8c8c8c' }}>Tổng thực lĩnh dự kiến:</div>
                        <div style={{ fontSize: '24px', fontWeight: 800, color: '#1890ff' }}>
                            {totals.estimatedNet.toLocaleString()}đ
                        </div>
                    </Col>
                </Row>
            </div>
        </Modal>
    );
};
