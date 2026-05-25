import React from "react";
import { useDraggable } from "@dnd-kit/core";
import { CSS } from "@dnd-kit/utilities";
import { Task } from "../types";
import TaskCardBody from "./TaskCardBody";

interface DraggableTaskCardProps {
    task: Task;
    isAdmin: boolean;
    onDelete?: (id: string) => void;
    onClick?: (task: Task) => void;
}

export default function DraggableTaskCard({ task, isAdmin, onDelete, onClick }: DraggableTaskCardProps) {
    const { attributes, listeners, setNodeRef, isDragging } = useDraggable({ id: task.id });
    
    // Khi dùng DragOverlay, phần tử gốc chỉ mờ đi chứ không di chuyển theo chuột
    const style = { 
        opacity: isDragging ? 0.4 : 1, 
        marginBottom: 12,
        transition: 'opacity 0.2s'
    };

    return (
        <div ref={setNodeRef} style={style} {...listeners} {...attributes}>
            <TaskCardBody task={task} isAdmin={isAdmin} onDelete={onDelete} onClick={onClick} />
        </div>
    );
}
