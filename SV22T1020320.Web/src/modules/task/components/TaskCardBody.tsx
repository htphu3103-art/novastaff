import React from "react";
import { Card, Tag, Typography, Avatar, Space, Button, Popconfirm } from "antd";
import { MinusOutlined } from "@ant-design/icons";
import { Task } from "../types";

const { Text } = Typography;

interface TaskCardBodyProps {
    task: Task;
    isAdmin: boolean;
    isOverlay?: boolean;
    onDelete?: (id: string) => void;
    onClick?: (task: Task) => void;
}

export default function TaskCardBody({ task, isAdmin, isOverlay, onDelete, onClick }: TaskCardBodyProps) {
    return (
        <Card
            size="small"
            hoverable={!isOverlay}
            onClick={() => !isOverlay && onClick?.(task)}
            style={{ 
                cursor: isOverlay ? 'grabbing' : 'grab',
                border: isOverlay ? '1px solid #1890ff' : undefined,
            }}
            extra={isAdmin && !isOverlay && (
                <Popconfirm
                    title="Xóa công việc?"
                    description="Bạn có chắc chắn muốn xóa công việc này không?"
                    onConfirm={(e) => {
                        e?.stopPropagation();
                        onDelete?.(task.id);
                    }}
                    onCancel={(e) => e?.stopPropagation()}
                    okText="Xóa"
                    cancelText="Hủy"
                >
                    <Button
                        type="text"
                        size="small"
                        danger
                        icon={<MinusOutlined />}
                        onClick={(e) => e.stopPropagation()}
                    />
                </Popconfirm>
            )}
        >
            <div style={{ display: 'flex', flexDirection: 'column', width: '100%', gap: '8px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Tag color={task.priority === 'High' ? 'red' : task.priority === 'Medium' ? 'orange' : 'blue'} style={{ fontSize: 10 }}>
                        {task.priority}
                    </Tag>
                    <Avatar size={20} src={`https://api.dicebear.com/7.x/avataaars/svg?seed=${task.assignee}`} />
                </div>
                <Text strong>{task.title}</Text>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text type="secondary" style={{ fontSize: 11 }}>{task.assignee}</Text>
                    <Tag style={{ fontSize: 9 }}>{task.department}</Tag>
                </div>
            </div>
        </Card>
    );
}
