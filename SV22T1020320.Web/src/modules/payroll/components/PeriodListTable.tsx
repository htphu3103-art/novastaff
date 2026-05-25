import React, { useMemo } from 'react';
import { Table, Tag, Button, Typography, Space, Skeleton } from 'antd';
import { EyeOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { PayrollPeriodSummaryDto, PayrollStatus } from '../types';
import { motion, Variants } from 'framer-motion';

const rowVariants: Variants = {
    hidden: { opacity: 0, y: 20 },
    visible: (i: number) => ({
        opacity: 1,
        y: 0,
        transition: {
            delay: i * 0.05,
            duration: 0.6,
            ease: [0.215, 0.61, 0.355, 1]
        }
    })
};

const { Text } = Typography;

interface PeriodListTableProps {
    dataSource: PayrollPeriodSummaryDto[];
    loading: boolean;
    onViewDetail: (periodId: number) => void;
}

const getStatusTag = (status: PayrollStatus) => {
    switch (status) {
        case PayrollStatus.Draft:
            return <Tag color="default">Nháp</Tag>;
        case PayrollStatus.Calculated:
            return <Tag color="blue">Đã tính toán</Tag>;
        case PayrollStatus.Approved:
            return <Tag color="green">Đã duyệt</Tag>;
        case PayrollStatus.Paid:
            return <Tag color="gold">Đã thanh toán</Tag>;
        default:
            return <Tag>{status}</Tag>;
    }
};

export const PeriodListTable: React.FC<PeriodListTableProps> = ({ dataSource, loading, onViewDetail }) => {
    const columns = [
        {
            title: 'Kỳ lương',
            key: 'period',
            render: (_: any, record: any) => record.isSkeleton ? (
                <Skeleton.Input active size="small" style={{ width: 100 }} />
            ) : (
                <Text strong>Tháng {record.month}/{record.year}</Text>
            ),
        },
        {
            title: 'Thời gian',
            key: 'duration',
            render: (_: any, record: any) => record.isSkeleton ? (
                <Skeleton.Input active size="small" style={{ width: 150 }} />
            ) : (
                <Text>
                    {dayjs(record.startDate).format('DD/MM/YYYY')} - {dayjs(record.endDate).format('DD/MM/YYYY')}
                </Text>
            ),
        },
        {
            title: 'Trạng thái',
            dataIndex: 'status',
            key: 'status',
            render: (status: PayrollStatus, record: any) => record.isSkeleton ? (
                <Skeleton.Button active size="small" style={{ width: 80 }} />
            ) : getStatusTag(status),
        },
        {
            title: 'Số nhân viên',
            dataIndex: 'totalEmployees',
            key: 'totalEmployees',
            align: 'center' as const,
            render: (val: number, record: any) => record.isSkeleton ? (
                <Skeleton.Input active size="small" style={{ width: 30 }} />
            ) : val,
        },
        {
            title: 'Tổng quỹ lương',
            dataIndex: 'totalNetSalary',
            key: 'totalNetSalary',
            align: 'right' as const,
            render: (value: number, record: any) => record.isSkeleton ? (
                <Skeleton.Input active size="small" style={{ width: 100 }} />
            ) : (
                <Text strong type="success">
                    {value.toLocaleString()} đ
                </Text>
            ),
        },
        {
            title: 'Thao tác',
            key: 'action',
            align: 'center' as const,
            render: (_: any, record: any) => record.isSkeleton ? (
                <Skeleton.Avatar active size="small" shape="circle" />
            ) : (
                <Space>
                    <Button 
                        type="primary" 
                        size="small" 
                        icon={<EyeOutlined />}
                        onClick={() => onViewDetail(record.periodID)}
                    >
                        Chi tiết
                    </Button>
                </Space>
            ),
        },
    ];

    const displayData = dataSource;

    return (
        <Table
            key={loading ? 'loading' : 'ready'}
            columns={columns as any}
            dataSource={(loading ? Array.from({ length: 5 }).map((_, i) => ({ periodID: `skeleton-${i}`, isSkeleton: true })) : displayData) as any[]}
            rowKey="periodID"
            loading={false}
            pagination={{ pageSize: 12 }}
            scroll={{ x: 800 }}
            components={{
                body: {
                    row: (props: any) => {
                        const isSkeleton = props['data-row-key']?.toString().startsWith('skeleton');
                        if (isSkeleton) return <tr {...props} />;

                        const index = dataSource.findIndex(x => x.periodID === props['data-row-key']);
                        return (
                            <motion.tr
                                {...props}
                                variants={rowVariants}
                                initial="hidden"
                                animate="visible"
                                custom={index >= 0 ? index : 0}
                            />
                        );
                    }
                }
            }}
        />
    );
};
