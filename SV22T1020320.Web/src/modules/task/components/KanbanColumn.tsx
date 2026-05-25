import React from "react";
import { Badge, Typography, Empty } from "antd";
import { useDroppable } from "@dnd-kit/core";
import { Task } from "../types";
import DraggableTaskCard from "./DraggableTaskCard";

const { Text } = Typography;

interface KanbanColumnProps {
    title: string;
    status: string;
    tasks: Task[];
    color: string;
    isAdmin: boolean;
    onDeleteTask?: (id: string) => void;
    onTaskClick?: (task: Task) => void;
}

export default function KanbanColumn({ title, status, tasks, color, isAdmin, onDeleteTask, onTaskClick }: KanbanColumnProps) {
    const { setNodeRef, isOver } = useDroppable({ id: status });

    return (
        <div
            style={{
                background: isOver ? '#f0f5ff' : '#f5f5f5',
                padding: 12,
                borderRadius: 12,
                minHeight: 600,
                display: 'flex',
                flexDirection: 'column',
                transition: 'background 0.2s'
            }}
        >
            <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', padding: '0 4px' }}>
                <Badge status="processing" color={color} text={<Text strong>{title}</Text>} />
                <Badge count={tasks.length} color={color} />
            </div>

            <div
                ref={setNodeRef}
                style={{
                    flex: 1,
                    padding: '4px'
                }}
            >
                {tasks.map(t => (
                    <DraggableTaskCard
                        key={t.id}
                        task={t}
                        isAdmin={isAdmin}
                        onDelete={onDeleteTask}
                        onClick={onTaskClick}
                    />
                ))}

                {tasks.length === 0 && <Empty description={false} image={Empty.PRESENTED_IMAGE_SIMPLE} style={{ marginTop: 40 }} />}
            </div>
        </div>
    );
}
