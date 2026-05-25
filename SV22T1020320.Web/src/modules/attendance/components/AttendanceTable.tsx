import React, { useState, useMemo } from 'react';
import { Table, Tag, Space, Button, App, Tooltip, Popconfirm, Skeleton } from 'antd';
import { EditOutlined, DeleteOutlined, InfoCircleOutlined } from '@ant-design/icons';
import { AttendanceDto, AttendanceStatus } from '../types';
import dayjs from 'dayjs';
import { AttendanceFormModal } from './AttendanceFormModal';
import { AttendanceDetailsDrawer } from './AttendanceDetailsDrawer';
import { motion } from 'framer-motion';

interface AttendanceTableProps {
    isAdmin: boolean;
    dataSource: AttendanceDto[];
    loading?: boolean;
    onDelete?: (id: number) => void;
    onRefresh?: () => void;
}

export const AttendanceTable = ({ isAdmin, dataSource, loading = false, onDelete, onRefresh }: AttendanceTableProps) => {
    const { modal } = App.useApp();
    const [editRecord, setEditRecord] = useState<AttendanceDto | null>(null);
    const [viewRecord, setViewRecord] = useState<AttendanceDto | null>(null);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [isViewDrawerOpen, setIsViewDrawerOpen] = useState(false);

    const handleEdit = (record: AttendanceDto) => {
        setEditRecord(record);
        setIsEditModalOpen(true);
    };

    const handleView = (record: AttendanceDto) => {
        setViewRecord(record);
        setIsViewDrawerOpen(true);
    };

    const handleDelete = (recordId: number) => {
        onDelete?.(recordId);
    };

    const rowComponents = useMemo(() => ({
        body: {
            row: (props: any) => {
                const { children, ...restProps } = props;
                const isDataRow = restProps['data-row-key'] && !restProps['data-row-key'].toString().startsWith('skeleton');
                
                if (!isDataRow) return <tr {...props} />;

                const index = dataSource.findIndex(x => x.recordId === restProps['data-row-key']);

                return (
                    <motion.tr
                        {...restProps}
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ 
                            duration: 0.6, 
                            delay: (index >= 0 ? index : 0) * 0.03,
                            ease: [0.215, 0.61, 0.355, 1] 
                        }}
                    >
                        {children}
                    </motion.tr>
                );
            }
        }
    }), [dataSource]);

    const columns = [
        {
            title: <span style={{ paddingLeft: 12 }}>Work Date</span>,
            dataIndex: 'workDate',
            key: 'workDate',
            render: (val: string, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" style={{ marginLeft: 12, width: 80 }} /> : <span style={{ paddingLeft: 12 }}>{dayjs(val).format('DD/MM/YYYY')}</span>
        },
        {
            title: 'Employee',
            dataIndex: 'employeeName',
            key: 'employeeName',
            hidden: !isAdmin,
            render: (text: string, record: any) => record.isSkeleton ? (
                <Space direction="vertical" size={0}>
                    <Skeleton.Input active size="small" style={{ width: 120 }} />
                    <Skeleton.Input active size="small" style={{ width: 60, height: 12 }} />
                </Space>
            ) : (
                <Space direction="vertical" size={0}>
                    <strong>{text}</strong>
                    <span style={{ fontSize: '12px', color: '#888' }}>{record.employeeCode}</span>
                </Space>
            )
        },
        {
            title: 'Time',
            key: 'time',
            render: (_: any, record: any) => record.isSkeleton ? (
                <Space direction="vertical" size={0}>
                    <Skeleton.Input active size="small" style={{ width: 60, height: 14 }} />
                    <Skeleton.Input active size="small" style={{ width: 60, height: 14 }} />
                </Space>
            ) : (
                <Space direction="vertical" size={0}>
                    <span>In: {record.checkIn ? dayjs(record.checkIn).format('HH:mm') : '--:--'}</span>
                    <span>Out: {record.checkOut ? dayjs(record.checkOut).format('HH:mm') : '--:--'}</span>
                </Space>
            )
        },
        {
            title: 'Hours',
            dataIndex: 'workHours',
            key: 'workHours',
            render: (val: number, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" style={{ width: 30 }} /> : (val ? `${val}h` : '-')
        },
        {
            title: 'Status',
            dataIndex: 'status',
            key: 'status',
            render: (status: AttendanceStatus, record: any) => {
                if (record.isSkeleton) return <Skeleton.Button active size="small" style={{ width: 60 }} />;
                let color = 'default';
                switch (status) {
                    case AttendanceStatus.Present: color = 'green'; break;
                    case AttendanceStatus.Late: color = 'warning'; break;
                    case AttendanceStatus.Absent: color = 'error'; break;
                    case AttendanceStatus.HalfDay: color = 'blue'; break;
                    case AttendanceStatus.Leave: color = 'purple'; break;
                }
                return <Tag color={color}>{record.statusDisplay}</Tag>;
            }
        },
        {
            title: 'Note',
            dataIndex: 'note',
            key: 'note',
            ellipsis: true,
            render: (val: string, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" block /> : val
        },
        {
            title: 'Action',
            key: 'action',
            width: 70,
            fixed: 'right' as const,
            render: (_: any, record: any) => {
                if (record.isSkeleton) return <Skeleton.Avatar active size="small" shape="circle" />;
                return isAdmin ? (
                    <Space size={-8}>
                        <Tooltip title="Edit">
                            <Button type="text" shape="circle" size="small" icon={<EditOutlined />} onClick={() => handleEdit(record)} />
                        </Tooltip>
                        <Tooltip title="Delete">
                            <Popconfirm
                                title="Delete this record?"
                                description="Are you sure to delete this attendance record?"
                                onConfirm={() => handleDelete(record.recordId)}
                                okText="Yes"
                                cancelText="No"
                                placement="left"
                            >
                                <Button type="text" danger shape="circle" size="small" icon={<DeleteOutlined />} />
                            </Popconfirm>
                        </Tooltip>
                    </Space>
                ) : (
                    <Tooltip title="Details">
                        <Button type="text" shape="circle" icon={<InfoCircleOutlined />} onClick={() => handleView(record)} />
                    </Tooltip>
                );
            }
        }
    ].filter(c => !c.hidden);

    const displayData = useMemo(() => {
        if (loading && dataSource.length === 0) {
            return Array.from({ length: 5 }).map((_, i) => ({
                recordId: `skeleton-${i}`,
                isSkeleton: true
            } as any));
        }
        return dataSource;
    }, [loading, dataSource]);

    return (
        <>
            <Table
                key={loading ? 'loading' : 'ready'}
                dataSource={displayData}
                columns={columns as any}
                rowKey="recordId"
                pagination={{ pageSize: 10 }}
                size="middle"
                scroll={{ x: 'max-content' }}
                loading={loading && dataSource.length > 0}
                components={rowComponents}
            />
            <AttendanceFormModal
                open={isEditModalOpen}
                onClose={() => setIsEditModalOpen(false)}
                onSuccess={onRefresh}
                record={editRecord}
            />
            <AttendanceDetailsDrawer
                open={isViewDrawerOpen}
                onClose={() => setIsViewDrawerOpen(false)}
                record={viewRecord}
            />
        </>
    );
};