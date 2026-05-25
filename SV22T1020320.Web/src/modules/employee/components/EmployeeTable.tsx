import React, { useMemo } from "react";
import { Button, Popconfirm, Space, Table, Tag, Typography, Dropdown, Skeleton } from "antd";
import type { TablePaginationConfig, ColumnsType } from "antd/es/table";
import { DeleteOutlined, EditOutlined, DragOutlined, LockOutlined, SafetyOutlined } from "@ant-design/icons";
import { EmployeeDto } from "../types";
import { UserRole } from "../../auth/types";
import { motion } from "framer-motion";

const { Text } = Typography;

interface Props {
    loading: boolean;
    dataSource: EmployeeDto[];
    pagination?: TablePaginationConfig | false;
    onEdit: (record: EmployeeDto) => void;
    onDelete: (id: number) => Promise<void>;
    onResetPassword?: (id: number) => void;
    onUpdateRole?: (id: number, role: UserRole) => void;
    draggable?: boolean;
    onDragStart?: (record: EmployeeDto, e: React.DragEvent) => void;
    onDragEnd?: () => void;
}

const statusColorMap: Record<string, string> = {
    Active: "green",
    Inactive: "default",
    Resigned: "red",
    OnLeave: "orange",
};

export const EmployeeTable = ({ 
    loading, 
    dataSource, 
    pagination, 
    onEdit, 
    onDelete,
    onResetPassword,
    onUpdateRole,
    draggable = false,
    onDragStart,
    onDragEnd
}: Props) => {
    // Row animation configuration
    const rowComponents = useMemo(() => ({
        body: {
            row: (props: any) => {
                const { children, ...restProps } = props;
                // Don't animate if it's a skeleton row or empty row
                const isDataRow = restProps['data-row-key'] && !restProps['data-row-key'].toString().startsWith('skeleton');
                
                if (!isDataRow) return <tr {...props} />;

                return (
                    <motion.tr
                        {...restProps}
                        initial={{ opacity: 0, y: 4 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ 
                            duration: 0.4, 
                            ease: [0.16, 1, 0.3, 1]
                        }}
                    >
                        {children}
                    </motion.tr>
                );
            }
        }
    }), []);

    const columns: ColumnsType<EmployeeDto> = [
        ...(draggable ? [{
            key: "drag",
            width: 50,
            align: "center" as const,
            render: (_: unknown, record: EmployeeDto) => (
                <div
                    draggable
                    onDragStart={(e) => onDragStart?.(record, e)}
                    onDragEnd={() => onDragEnd?.()}
                    style={{ cursor: "grab", display: "inline-flex", alignItems: "center", justifyContent: "center" }}
                    title="Kéo để chuyển phòng ban"
                >
                    <DragOutlined style={{ color: "#999", fontSize: 16 }} />
                </div>
            ),
        }] : []),
        {
            title: "Code",
            dataIndex: "employeeCode",
            width: 110,
            render: (value: string, record: any) => record.isSkeleton ? <Skeleton.Button active size="small" block /> : <Text strong>{value}</Text>,
        },
        {
            title: "Full name",
            dataIndex: "fullName",
            width: 180,
            render: (value: string, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" block /> : value,
        },
        {
            title: "Email",
            dataIndex: "email",
            width: 200,
            render: (value: string, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" block /> : value,
        },
        {
            title: "Phone",
            dataIndex: "phone",
            width: 120,
            render: (value: string | null, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" block /> : (value ?? "-"),
        },
        {
            title: "Department",
            dataIndex: "departmentName",
            width: 150,
            render: (value: string | null, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" block /> : (value ?? "-"),
            hidden: draggable,
        },
        {
            title: "Position",
            dataIndex: "position",
            width: 150,
            render: (value: string | null, record: any) => record.isSkeleton ? <Skeleton.Input active size="small" block /> : (value ?? "-"),
        },
        {
            title: "Status",
            dataIndex: "status",
            width: 110,
            render: (status: string, record: any) => record.isSkeleton ? <Skeleton.Button active size="small" style={{ width: 60 }} /> : (
                <Tag color={statusColorMap[status] ?? "blue"}>{status}</Tag>
            ),
        },
        {
            title: "Action",
            key: "action",
            width: 160,
            render: (_: any, record: any) => record.isSkeleton ? <Space size={4}><Skeleton.Avatar active size="small" shape="square" /><Skeleton.Avatar active size="small" shape="square" /></Space> : (
                <Space size={4}>
                    <Button type="text" size="small" icon={<EditOutlined />} onClick={() => onEdit(record)} title="Edit" />
                    
                    {onResetPassword && (
                        <Popconfirm
                            title="Reset Password?"
                            description={`Are you sure to reset password for ${record.fullName}?`}
                            onConfirm={() => onResetPassword(record.id)}
                        >
                            <Button type="text" size="small" icon={<LockOutlined />} title="Reset Password" />
                        </Popconfirm>
                    )}

                    {onUpdateRole && (
                        <Dropdown
                            menu={{
                                items: [
                                    { key: UserRole.Admin.toString(), label: "Admin" },
                                    { key: UserRole.Manager.toString(), label: "Manager" },
                                    { key: UserRole.Staff.toString(), label: "Staff" },
                                ],
                                onClick: (e) => onUpdateRole?.(record.id, parseInt(e.key) as UserRole),
                            }}
                            trigger={["click"]}
                        >
                            <Button type="text" size="small" icon={<SafetyOutlined />} title="Update Role" />
                        </Dropdown>
                    )}

                    <Popconfirm
                        title="Delete employee?"
                        description={`Are you sure to delete ${record.fullName}?`}
                        okButtonProps={{ danger: true }}
                        onConfirm={() => onDelete(record.id)}
                    >
                        <Button type="text" size="small" danger icon={<DeleteOutlined />} title="Delete" />
                    </Popconfirm>
                </Space>
            ),
        },
    ].filter(c => !(c as any).hidden);

    // Prepare skeleton data if loading and no data
    const displayData = useMemo(() => {
        if (loading && dataSource.length === 0) {
            return Array.from({ length: pagination ? (typeof pagination === 'object' ? (pagination.pageSize || 5) : 5) : 5 }).map((_, i) => ({
                id: `skeleton-${i}`,
                isSkeleton: true
            } as any));
        }
        return dataSource;
    }, [loading, dataSource, pagination]);

    return (
        <Table
            rowKey="id"
            loading={loading && dataSource.length > 0} // Only show native loading if we have data (refreshing)
            columns={columns}
            dataSource={displayData}
            pagination={pagination}
            scroll={{ x: "max-content" }}
            components={rowComponents}
        />
    );
};