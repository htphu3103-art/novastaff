import React, { useMemo } from 'react';
import { Table, Tag, Button, Space, Tooltip, Typography, Skeleton } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined, EyeOutlined } from '@ant-design/icons';
import { LeaveRequestDto, LeaveRequestStatus, LeaveType } from '../types';
import dayjs from 'dayjs';
import { motion } from 'framer-motion';

const { Text } = Typography;

interface LeaveRequestTableProps {
    dataSource: LeaveRequestDto[];
    loading?: boolean;
    onApprove: (id: number) => void;
    onReject: (id: number) => void;
    onView: (record: LeaveRequestDto) => void;
    showEmployeeInfo?: boolean;
}

export const LeaveRequestTable: React.FC<LeaveRequestTableProps> = ({
    dataSource,
    loading = false,
    onApprove,
    onReject,
    onView,
    showEmployeeInfo = true
}) => {
    const scrollX = showEmployeeInfo ? 910 : 630;

    const rowComponents = useMemo(() => ({
        body: {
            row: (props: any) => {
                const { children, ...restProps } = props;
                const isDataRow = restProps['data-row-key'] && !restProps['data-row-key'].toString().startsWith('skeleton');
                
                if (!isDataRow) return <tr {...props} />;

                return (
                    <motion.tr
                        {...restProps}
                        initial={{ opacity: 0, y: 4 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.4, ease: [0.16, 1, 0.3, 1] }}
                    >
                        {children}
                    </motion.tr>
                );
            }
        }
    }), []);

    const getStatusColor = (status: LeaveRequestStatus) => {
        switch (status) {
            case LeaveRequestStatus.Approved: return 'success';
            case LeaveRequestStatus.Rejected: return 'error';
            case LeaveRequestStatus.Pending: return 'processing';
            case LeaveRequestStatus.Cancelled: return 'default';
            default: return 'default';
        }
    };

    const getLeaveTypeLabel = (type: LeaveType) => {
        switch (type) {
            case LeaveType.Annual: return 'Nghỉ phép năm';
            case LeaveType.Sick: return 'Nghỉ ốm';
            case LeaveType.Unpaid: return 'Nghỉ không lương';
            case LeaveType.Maternity: return 'Nghỉ thai sản';
            case LeaveType.Other: return 'Khác';
            default: return type;
        }
    };

    const columns = [
        ...(showEmployeeInfo ? [
            {
                title: 'Mã NV',
                dataIndex: 'employeeCode',
                key: 'employeeCode',
                width: 100,
                render: (code: string, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" style={{ width: 60 }} /> : <Text strong>{code}</Text>
            },
            {
                title: 'Họ tên',
                dataIndex: 'employeeName',
                key: 'employeeName',
                width: 180,
                render: (name: string, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" style={{ width: 140 }} /> : name
            }
        ] : []),
        {
            title: 'Loại nghỉ',
            dataIndex: 'leaveType',
            key: 'leaveType',
            width: 140,
            render: (type: LeaveType, record: any) => record.isSkeleton ? <Skeleton.Button active size="small" style={{ width: 100 }} /> : <Tag color="blue">{getLeaveTypeLabel(type)}</Tag>
        },
        {
            title: 'Thời gian',
            key: 'time',
            width: 170,
            render: (_: any, record: any) => record.isSkeleton ? (
                <Space orientation="vertical" size={0}>
                    <Skeleton.Input active size="small" style={{ width: 150, height: 16 }} />
                    <Skeleton.Input active size="small" style={{ width: 100, height: 12 }} />
                </Space>
            ) : (
                <Space orientation="vertical" size={0}>
                    <Text style={{ fontSize: 13 }}>{dayjs(record.fromDate).format('DD/MM/YYYY')} - {dayjs(record.toDate).format('DD/MM/YYYY')}</Text>
                    <Text type="secondary" style={{ fontSize: 12 }}>Tổng: {record.totalDays} ngày</Text>
                </Space>
            )
        },
        {
            title: 'Lý do',
            dataIndex: 'reason',
            key: 'reason',
            width: 150,
            ellipsis: true,
            render: (val: string, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" block /> : val
        },
        {
            title: 'Trạng thái',
            dataIndex: 'status',
            key: 'status',
            width: 90,
            render: (status: LeaveRequestStatus, record: any) => {
                if (record.isSkeleton) return <Skeleton.Button active size="small" style={{ width: 70 }} />;
                let label = 'UNKNOWN';
                switch (status) {
                    case LeaveRequestStatus.Pending: label = 'PENDING'; break;
                    case LeaveRequestStatus.Approved: label = 'APPROVED'; break;
                    case LeaveRequestStatus.Rejected: label = 'REJECTED'; break;
                    case LeaveRequestStatus.Cancelled: label = 'CANCELLED'; break;
                }
                return <Tag color={getStatusColor(status)}>{label}</Tag>;
            }
        },
        {
            title: 'Thao tác',
            key: 'action',
            fixed: 'right' as const,
            width: 80,
            render: (_: any, record: any) => record.isSkeleton ? <Skeleton.Avatar active size="small" shape="circle" /> : (
                <Space size={-8}>
                    <Tooltip title="Xem chi tiết">
                        <Button
                            type="text"
                            size="small"
                            icon={<EyeOutlined />}
                            onClick={() => onView(record)}
                        />
                    </Tooltip>
                    {record.status === LeaveRequestStatus.Pending && (
                        <>
                            <Tooltip title="Phê duyệt">
                                <Button
                                    type="text"
                                    size="small"
                                    style={{ color: '#52c41a' }}
                                    icon={<CheckCircleOutlined />}
                                    onClick={() => onApprove(record.requestId)}
                                />
                            </Tooltip>
                            <Tooltip title="Từ chối">
                                <Button
                                    type="text"
                                    size="small"
                                    danger
                                    icon={<CloseCircleOutlined />}
                                    onClick={() => onReject(record.requestId)}
                                />
                            </Tooltip>
                        </>
                    )}
                </Space>
            )
        }
    ];

    const displayData = useMemo(() => {
        if (loading && dataSource.length === 0) {
            return Array.from({ length: 5 }).map((_, i) => ({
                requestId: `skeleton-${i}`,
                isSkeleton: true
            } as any));
        }
        return dataSource;
    }, [loading, dataSource]);

    return (
        <Table
            columns={columns}
            dataSource={displayData}
            loading={loading && dataSource.length > 0}
            rowKey="requestId"
            pagination={{ pageSize: 10 }}
            scroll={{ x: scrollX }}
            style={{ borderRadius: 12, overflow: 'hidden' }}
            components={rowComponents}
        />
    );
};
