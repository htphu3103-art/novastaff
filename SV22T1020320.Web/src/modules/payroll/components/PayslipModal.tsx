import React from 'react';
import { Modal, Descriptions, Divider, Typography, Row, Col } from 'antd';
import { PayrollDetailDto } from '../types';

const { Title, Text } = Typography;

interface PayslipModalProps {
    open: boolean;
    data: PayrollDetailDto | null;
    onCancel: () => void;
    periodInfo?: { month: number; year: number };
}

export const PayslipModal: React.FC<PayslipModalProps> = ({ open, data, onCancel, periodInfo }) => {
    
    const renderSimpleList = (title: string, items: any[] | undefined, type: 'success' | 'danger') => {
        const isSuccess = type === 'success';
        const prefix = isSuccess ? '+' : '-';
        
        return (
            <div style={{ 
                border: '1px solid #d9d9d9', 
                borderRadius: '8px', 
                overflow: 'hidden' 
            }}>
                <div style={{ 
                    padding: '8px 16px', 
                    backgroundColor: isSuccess ? '#f6ffed' : '#fff1f0', 
                    borderBottom: '1px solid #d9d9d9' 
                }}>
                    <Text strong type={type}>{title}</Text>
                </div>
                <div style={{ maxHeight: '200px', overflowY: 'auto' }}>
                    {items && items.length > 0 ? (
                        items.map((item, idx) => (
                            <div key={idx} style={{ 
                                padding: '8px 16px', 
                                display: 'flex', 
                                justifyContent: 'space-between',
                                borderBottom: idx < items.length - 1 ? '1px solid #f0f0f0' : 'none'
                            }}>
                                <Text>{item.name}</Text>
                                <Text type={type}>{prefix}{item.amount.toLocaleString()}đ</Text>
                            </div>
                        ))
                    ) : (
                        <div style={{ padding: '16px', textAlign: 'center', color: '#bfbfbf' }}>
                            Không có
                        </div>
                    )}
                </div>
            </div>
        );
    };

    return (
        <Modal
            title="PHIẾU LƯƠNG CHI TIẾT"
            open={open}
            onCancel={onCancel}
            footer={null}
            width={700}
            {...({ destroyOnHidden: true } as any)}
        >
            <div style={{ textAlign: 'center', marginBottom: 24 }}>
                <Title level={4} style={{ margin: 0, color: '#003eb3' }}>CÔNG TY PHÚ ADMIN TECH</Title>
                <Text type="secondary">
                    Bảng thanh toán lương tháng {periodInfo?.month || '...'}/{periodInfo?.year || '...'}
                </Text>
            </div>

            <Descriptions 
                column={1} 
                {...({ variant: 'bordered' } as any)}
                size="small"
                {...({ styles: { label: { width: '150px', fontWeight: 'bold' } } } as any)}
            >
                <Descriptions.Item label="Họ tên"><b>{data?.fullName}</b></Descriptions.Item>
                <Descriptions.Item label="Mã nhân viên">{data?.employeeCode}</Descriptions.Item>
                <Descriptions.Item label="Phòng ban">{data?.departmentName || '---'}</Descriptions.Item>
                <Descriptions.Item label="Lương cơ bản">
                    {data?.baseSalarySnapshot?.toLocaleString() || 0}đ
                </Descriptions.Item>
                <Descriptions.Item label="Ngày công thực tế">
                    {data?.actualWorkDays || 0} ngày
                </Descriptions.Item>
            </Descriptions>

            <Row gutter={20} style={{ marginTop: 24 }}>
                <Col span={12}>
                    {renderSimpleList('Các khoản cộng / Thưởng', data?.bonusAndAllowances, 'success')}
                </Col>
                <Col span={12}>
                    {renderSimpleList('Các khoản trừ', data?.deductions, 'danger')}
                </Col>
            </Row>

            <Divider style={{ margin: '24px 0' }} />

            <div style={{ 
                background: '#e6f4ff', 
                padding: '16px 24px', 
                borderRadius: '8px',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                border: '1px solid #bae0ff'
            }}>
                <Title level={4} style={{ margin: 0, color: '#003eb3' }}>THỰC LĨNH:</Title>
                <Title level={3} style={{ margin: 0, color: '#cf1322' }}>
                    {(data?.netSalary || 0).toLocaleString()}đ
                </Title>
            </div>

            <div style={{ textAlign: 'center', marginTop: 16 }}>
                <Text italic style={{ fontSize: '12px', color: '#8c8c8c' }}>
                    (Ghi chú: Lương được chuyển khoản vào ngày 05 hàng tháng. Mọi thắc mắc vui lòng liên hệ phòng HC-NS)
                </Text>
            </div>
        </Modal>
    );
};