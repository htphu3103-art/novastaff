import React from 'react';
import { Table, Space, Button, Typography, Popconfirm, Tooltip, Avatar, Badge } from "antd";
import { EditOutlined, DeleteOutlined, ApartmentOutlined, UserOutlined } from "@ant-design/icons";
import { TreeNode } from "../types";

const { Text } = Typography;

interface DepartmentTableProps {
    dataSource: TreeNode[];
    loading: boolean;
    onDelete: (id: number) => void;
    onEdit: (record: TreeNode) => void;
    highlightedIds?: number[];
    onRowDoubleClick?: (record: TreeNode) => void;
    pagination?: false | {
        current: number;
        total: number;
        pageSize: number;
        onChange: (page: number) => void;
    };
}

export const DepartmentTable: React.FC<DepartmentTableProps> = ({
    dataSource,
    loading,
    onDelete,
    onEdit,
    highlightedIds = [],
    onRowDoubleClick,
    pagination,
}) => {
    const columns = [
        {
            title: "Department / Unit",
            dataIndex: "name",
            key: "name",
            render: (text: string, r: TreeNode) => {
                const isHighlighted = highlightedIds.includes(r.id);
                return (
                    <Space>
                        <ApartmentOutlined style={{ color: r.isActive ? '#1677ff' : '#d9d9d9' }} />
                        <div style={{
                            backgroundColor: isHighlighted ? '#fff7e6' : 'transparent',
                            padding: '2px 4px',
                            borderRadius: 4,
                            border: isHighlighted ? '1px solid #ffd591' : 'none'
                        }}>
                            <Text strong>{text}</Text>
                        </div>
                    </Space>
                );
            }
        },
        {
            title: "Code",
            dataIndex: "code",
            key: "code",
            width: 120,
            render: (code: string) => <Text code>{code || '-'}</Text>
        },
        {
            title: "Manager",
            dataIndex: "managerName",
            key: "managerName",
            width: 200,
            render: (name: string) => name ? (
                <Space>
                    <Avatar size="small" icon={<UserOutlined />} style={{ backgroundColor: '#87d068' }} />
                    <Text type="secondary">{name}</Text>
                </Space>
            ) : <Text type="secondary" italic>- Empty -</Text>
        },
        {
            title: "Status",
            dataIndex: "isActive",
            key: "isActive",
            width: 130,
            render: (active: boolean) => (
                <Badge status={active ? "success" : "default"} text={active ? "Active" : "Disabled"} />
            )
        },
        {
            title: "Actions",
            key: "action",
            fixed: 'right' as const,
            width: 100,
            render: (_: any, r: TreeNode) => (
                <Space>
                    <Tooltip title="Edit">
                        <Button type="text" size="small" icon={<EditOutlined />} onClick={() => onEdit(r)} />
                    </Tooltip>
                    <Popconfirm title="Confirm delete?" onConfirm={() => onDelete(r.id)} okButtonProps={{ danger: true }}>
                        <Button type="text" size="small" danger icon={<DeleteOutlined />} />
                    </Popconfirm>
                </Space>
            )
        }
    ];

    return (
        <Table
            columns={columns}
            dataSource={dataSource}
            loading={loading}
            rowKey="id"
            pagination={pagination ?? false}
            size="middle"
            onRow={(record: TreeNode) => ({
                onDoubleClick: () => onRowDoubleClick?.(record),
            })}
            scroll={{ x: 800 }}
        />
    );
};