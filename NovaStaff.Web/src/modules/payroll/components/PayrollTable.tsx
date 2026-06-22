import React from 'react';
import { Table, Button, Tag, Typography, Space, Tooltip } from 'antd';
import { EyeOutlined, EditOutlined } from '@ant-design/icons';
import { PayrollDetailDto, PayrollStatus } from '../types';

const { Text } = Typography;

interface PayrollTableProps {
    dataSource: PayrollDetailDto[];
    loading: boolean;
    onViewPayslip: (record: PayrollDetailDto) => void;
    onRecalculate?: (record: PayrollDetailDto) => void;
    onAdjust?: (record: PayrollDetailDto) => void;
}

const getStatusTag = (status: PayrollStatus) => {
    switch (status) {
        case PayrollStatus.Draft:
            return <Tag color="default">Nháp</Tag>;
        case PayrollStatus.Calculated:
            return <Tag color="blue">Đã tính</Tag>;
        case PayrollStatus.Approved:
            return <Tag color="green">Đã duyệt</Tag>;
        case PayrollStatus.Paid:
            return <Tag color="gold">Đã thanh toán</Tag>;
        default:
            return <Tag>{status}</Tag>;
    }
};

export const PayrollTable: React.FC<PayrollTableProps> = ({ dataSource, loading, onViewPayslip, onRecalculate, onAdjust }) => {
    const columns = [
        { 
            title: 'Mã NV', 
            dataIndex: 'employeeCode', 
            key: 'employeeCode', 
            fixed: 'left' as const, 
            width: 100 
        },
        { 
            title: 'Họ tên', 
            dataIndex: 'fullName', 
            key: 'fullName', 
            fixed: 'left' as const, 
            width: 150 
        },
        { 
            title: 'Phòng ban', 
            dataIndex: 'departmentName', 
            key: 'departmentName', 
            width: 150 
        },
        { 
            title: 'Lương cơ bản', 
            dataIndex: 'baseSalarySnapshot', 
            key: 'baseSalarySnapshot',
            render: (v: number) => v.toLocaleString() + 'đ' 
        },
        { 
            title: 'Ngày công', 
            dataIndex: 'actualWorkDays', 
            key: 'actualWorkDays',
            align: 'center' as const 
        },
        { 
            title: 'Phụ cấp & Thưởng', 
            key: 'bonus', 
            render: (_: any, record: PayrollDetailDto) => {
                const totalBonus = record.bonusAndAllowances?.reduce((sum, item) => sum + item.amount, 0) || 0;
                return <Text type="success">+{totalBonus.toLocaleString()}đ</Text>;
            }
        },
        { 
            title: 'Khấu trừ', 
            key: 'deduct', 
            render: (_: any, record: PayrollDetailDto) => {
                const totalDeduct = record.deductions?.reduce((sum, item) => sum + item.amount, 0) || 0;
                return <Text type="danger">-{totalDeduct.toLocaleString()}đ</Text>;
            }
        },
        {
            title: 'Thực lĩnh',
            dataIndex: 'netSalary',
            key: 'netSalary',
            render: (v: number) => <b style={{ color: '#1890ff' }}>{v.toLocaleString()}đ</b>
        },
        {
            title: 'Trạng thái',
            dataIndex: 'status',
            key: 'status',
            render: (v: PayrollStatus) => getStatusTag(v)
        },
        {
            title: 'Thao tác',
            key: 'action',
            fixed: 'right' as const,
            width: 220,
            align: 'center' as const,
            render: (_: any, record: PayrollDetailDto) => (
                <Space>
                    <Tooltip title="Xem phiếu lương">
                        <Button icon={<EyeOutlined />} size="small" onClick={() => onViewPayslip(record)}>
                            Chi tiết
                        </Button>
                    </Tooltip>
                    
                    {onAdjust && record.status < PayrollStatus.Approved && (
                        <Tooltip title="Điều chỉnh phụ cấp/khấu trừ">
                            <Button icon={<EditOutlined />} size="small" onClick={() => onAdjust(record)}>
                                Điều chỉnh
                            </Button>
                        </Tooltip>
                    )}

                    {onRecalculate && record.status < PayrollStatus.Approved && (
                        <Button size="small" onClick={() => onRecalculate(record)}>
                            Tính lại
                        </Button>
                    )}
                </Space>
            )
        }
    ];

    return (
        <Table 
            columns={columns} 
            dataSource={dataSource} 
            rowKey="detailID" 
            loading={loading}
            scroll={{ x: 1200 }} 
            size="middle" 
            pagination={{ pageSize: 20 }}
        />
    );
};