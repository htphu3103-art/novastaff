import React, { useState, useEffect } from "react";
import {
    Row, Col, Button, Select, Typography, Space, Modal, Form, Input,
    Drawer, Tag, Divider, Spin, DatePicker, App, Avatar, Descriptions
} from "antd";
import {
    PlusOutlined, SaveOutlined, ReloadOutlined, DeleteOutlined, EditOutlined,
    CheckCircleOutlined, UserOutlined, CalendarOutlined, FlagOutlined
} from "@ant-design/icons";
import {
    DndContext, useSensor, useSensors, MouseSensor, TouchSensor,
    DragOverlay, defaultDropAnimationSideEffects
} from "@dnd-kit/core";
import {
    WorkTaskDto, COLUMNS, WorkTaskStatus, WorkTaskPriority,
    CreateWorkTaskRequest
} from "../types";
import { taskApi } from "../api/taskApi";
import KanbanColumn from "../components/KanbanColumn";
import TaskCardBody from "../components/TaskCardBody";

const { Title, Text } = Typography;

// Helper: Map chuỗi status từ Backend ("Todo", "InProgress", "Done") sang key UI ("todo", "inprogress", "done")
const statusToUiKey = (status: string): string => {
    switch (status) {
        case "Todo": return "todo";
        case "InProgress": return "inprogress";
        case "Done": return "done";
        default: return status.toLowerCase();
    }
};

// Helper: Map key UI sang WorkTaskStatus enum (số) để gửi lên Backend
const uiKeyToStatusEnum = (uiKey: string): WorkTaskStatus => {
    switch (uiKey) {
        case "todo": return WorkTaskStatus.Todo;
        case "inprogress": return WorkTaskStatus.InProgress;
        case "done": return WorkTaskStatus.Done;
        default: return WorkTaskStatus.Todo;
    }
};

const priorityToEnum = (priority: string | number | undefined): WorkTaskPriority => {
    if (priority === undefined || priority === null) return WorkTaskPriority.Low;
    
    // Nếu là số, trả về luôn
    if (typeof priority === 'number') return priority as WorkTaskPriority;
    
    // Nếu là chuỗi số "1", "2", "3"
    const parsed = parseInt(priority, 10);
    if (!isNaN(parsed) && (parsed === 1 || parsed === 2 || parsed === 3)) {
        return parsed as WorkTaskPriority;
    }

    switch (priority) {
        case "High": return WorkTaskPriority.High;
        case "Medium": return WorkTaskPriority.Medium;
        case "Low": return WorkTaskPriority.Low;
        default: return WorkTaskPriority.Low;
    }
};

export default function AdminTaskPage({ user }: any) {
    const { message } = App.useApp();
    const [tasks, setTasks] = useState<WorkTaskDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [activeId, setActiveId] = useState<string | null>(null);
    const [filterEmployeeId, setFilterEmployeeId] = useState<number | null>(null);

    // Form và Modal state
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [createForm] = Form.useForm();

    const [selectedTask, setSelectedTask] = useState<WorkTaskDto | null>(null);
    const [isDetailDrawerOpen, setIsDetailDrawerOpen] = useState(false);
    const [editForm] = Form.useForm();

    const sensors = useSensors(
        useSensor(MouseSensor, {
            activationConstraint: {
                distance: 8
            }
        }),
        useSensor(TouchSensor, {
            activationConstraint: {
                delay: 250,
                tolerance: 5
            }
        })
    );

    // ===================================================================
    // 1. FETCH DỮ LIỆU
    // ===================================================================
    const fetchTasks = async () => {
        setIsLoading(true);
        try {
            const response = await taskApi.getPaged(
                filterEmployeeId ? { employeeId: filterEmployeeId } : {},
                1,
                200
            );
            setTasks(response.data.items);
        } catch (err) {
            message.error("Không thể tải danh sách công việc. Vui lòng thử lại.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchTasks();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // ===================================================================
    // 2. KÉO THẢ — OPTIMISTIC UPDATE + ROLLBACK
    // ===================================================================
    const handleDragEnd = async (event: any) => {
        const { active, over } = event;
        setActiveId(null);
        if (!over) return;

        const taskId = Number(active.id);
        const newUiStatus = over.id as string;
        const task = tasks.find(t => t.id === taskId);
        if (!task) return;

        const oldStatus = task.status;
        if (statusToUiKey(oldStatus) === newUiStatus) return;

        // Optimistic Update: Cập nhật UI trước
        setTasks(prev =>
            prev.map(t =>
                t.id === taskId
                    ? { ...t, status: newUiStatus.charAt(0).toUpperCase() + newUiStatus.slice(1) }
                    : t
            )
        );

        try {
            await taskApi.changeStatus(taskId, { status: uiKeyToStatusEnum(newUiStatus) });
        } catch (err) {
            // Rollback về trạng thái cũ nếu API thất bại
            message.error("Cập nhật trạng thái thất bại. Đang hoàn tác...");
            setTasks(prev =>
                prev.map(t => (t.id === taskId ? { ...t, status: oldStatus } : t))
            );
        }
    };

    // ===================================================================
    // 3. TẠO MỚI TASK
    // ===================================================================
    const handleCreateTask = async (values: any) => {
        setIsSubmitting(true);
        try {
            const payload: CreateWorkTaskRequest = {
                title: values.title,
                description: values.description ?? null,
                priority: priorityToEnum(values.priority),
                dueDate: values.dueDate ? values.dueDate.toISOString() : null,
                employeeId: values.employeeId ?? null,
            };
            const response = await taskApi.create(payload);
            // Push thẳng task mới vào danh sách hiện tại
            setTasks(prev => [...prev, response.data]);
            setIsCreateModalOpen(false);
            createForm.resetFields();
            message.success("Đã tạo công việc mới thành công!");
        } catch (err) {
            message.error("Tạo công việc thất bại. Vui lòng thử lại.");
        } finally {
            setIsSubmitting(false);
        }
    };

    // ===================================================================
    // 4. CẬP NHẬT TASK
    // ===================================================================
    const handleUpdateTask = async (values: any) => {
        if (!selectedTask) return;
        setIsSubmitting(true);
        try {
            const statusEnum = uiKeyToStatusEnum(values.status);
            const priorityEnum = priorityToEnum(values.priority);
            
            const payload: any = {
                title: values.title,
                description: values.description ?? null,
                priority: priorityEnum,
                status: statusEnum,
                employeeId: selectedTask.employeeId,
                dueDate: selectedTask.dueDate,
            };

            const response = await taskApi.update(selectedTask.id, payload);
            const updatedTask = response.data;

            // Cập nhật local state bằng dữ liệu trả về từ API
            setTasks(prev => prev.map(t => t.id === updatedTask.id ? updatedTask : t));

            setIsDetailDrawerOpen(false);
            message.success("Đã cập nhật công việc!");
        } catch (err) {
            console.error("Update task error:", err);
            message.error("Cập nhật thất bại. Vui lòng thử lại.");
        } finally {
            setIsSubmitting(false);
        }
    };

    // ===================================================================
    // 5. XÓA TASK
    // ===================================================================
    const handleDeleteTask = async (id: string) => {
        const numId = Number(id);
        try {
            await taskApi.delete(numId);
            setTasks(prev => prev.filter(t => t.id !== numId));
            if (selectedTask?.id === numId) {
                setIsDetailDrawerOpen(false);
                setSelectedTask(null);
            }
            message.info("Đã xóa công việc.");
        } catch (err) {
            message.error("Xóa công việc thất bại. Vui lòng thử lại.");
        }
    };

    // Chốt hoàn thành công việc
    const handleFinalComplete = async () => {
        if (!selectedTask) return;
        setIsSubmitting(true);
        try {
            await taskApi.complete(selectedTask.id);
            message.success("Đã chốt hoàn thành công việc thành công!");
            setIsDetailDrawerOpen(false);
            fetchTasks(); // Reload để cập nhật dữ liệu mới nhất
        } catch (err) {
            message.error("Không thể chốt hoàn thành. Vui lòng thử lại.");
        } finally {
            setIsSubmitting(false);
        }
    };

    // Mở chi tiết task
    const handleTaskClick = async (task: WorkTaskDto) => {
        setSelectedTask(task);
        editForm.setFieldsValue({
            title: task.title,
            description: task.description,
            status: statusToUiKey(task.status),
            priority: priorityToEnum(task.priority),
        });
        setIsDetailDrawerOpen(true);

        try {
            const res = await taskApi.getById(task.id);
            const freshTask = res.data;
            setSelectedTask(freshTask);
            editForm.setFieldsValue({
                title: freshTask.title,
                description: freshTask.description,
                status: statusToUiKey(freshTask.status),
                priority: priorityToEnum(freshTask.priority),
            });
        } catch (err) {
            message.warning("Đang hiển thị dữ liệu cũ. Không thể tải bản cập nhật mới nhất.");
        }
    };

    // Xử lý Đặt lại (Reset Filter)
    const handleReset = () => {
        setFilterEmployeeId(null);
    };

    const displayTasks = filterEmployeeId
        ? tasks.filter(t => t.employeeId === filterEmployeeId)
        : tasks;

    const activeTask = tasks.find(t => String(t.id) === activeId);

    return (
        <DndContext
            sensors={sensors}
            onDragStart={(e) => {
                setActiveId(String(e.active.id));
            }}
            onDragEnd={handleDragEnd}
        >
            <div style={{ marginBottom: 24, background: '#fff', padding: 20, borderRadius: 12, border: '1px solid #f0f0f0' }}>
                <Row justify="space-between" align="middle">
                    <Col>
                        <Title level={4} style={{ margin: 0 }}>Admin Production Board</Title>
                        <Text type="secondary">Quản lý và điều phối công việc toàn hệ thống</Text>
                    </Col>
                    <Col>
                        <Space>
                            <Button icon={<ReloadOutlined />} onClick={handleReset}>Đặt lại</Button>
                            <Button type="primary" icon={<PlusOutlined />} onClick={() => setIsCreateModalOpen(true)}>New Task</Button>
                            <Button icon={<SaveOutlined />} onClick={fetchTasks}>Làm mới</Button>
                        </Space>
                    </Col>
                </Row>
            </div>

            <Spin spinning={isLoading} description="Đang tải công việc...">
                <Row gutter={16}>
                    {COLUMNS.map(col => (
                        <Col span={8} key={col.key}>
                            <KanbanColumn
                                title={col.label}
                                status={col.key}
                                color={col.color}
                                tasks={displayTasks.filter(t => statusToUiKey(t.status) === col.key && (col.key === 'done' ? !t.completedDate : true)) as any}
                                isAdmin={true}
                                onDeleteTask={handleDeleteTask}
                                onTaskClick={handleTaskClick as any}
                            />
                        </Col>
                    ))}
                </Row>
            </Spin>

            <DragOverlay
                zIndex={10000}
                dropAnimation={null}
            >
                {activeId && activeTask ? (
                    <div style={{
                        width: 320,
                        transform: 'scale(1.05) rotate(2deg)',
                        boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
                        borderRadius: '8px',
                        cursor: 'grabbing',
                        opacity: 0.95
                    }}>
                        <TaskCardBody task={activeTask as any} isAdmin={true} isOverlay />
                    </div>
                ) : null}
            </DragOverlay>

            {/* MODAL TẠO TASK MỚI */}
            <Modal
                title="Tạo công việc mới"
                open={isCreateModalOpen}
                onOk={() => createForm.submit()}
                onCancel={() => { setIsCreateModalOpen(false); createForm.resetFields(); }}
                centered
                okText="Tạo mới"
                cancelText="Hủy"
                confirmLoading={isSubmitting}
            >
                <Form form={createForm} layout="vertical" onFinish={handleCreateTask}>
                    <Form.Item name="title" label="Tiêu đề công việc" rules={[{ required: true, message: 'Vui lòng nhập tiêu đề!' }]}>
                        <Input placeholder="Ví dụ: Cập nhật giao diện trang chủ" />
                    </Form.Item>
                    <Form.Item name="description" label="Mô tả">
                        <Input.TextArea rows={3} placeholder="Mô tả chi tiết công việc..." />
                    </Form.Item>
                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item name="employeeId" label="ID Nhân viên phụ trách">
                                <Input type="number" placeholder="Nhập ID nhân viên" />
                            </Form.Item>
                        </Col>
                        <Col span={12}>
                            <Form.Item name="priority" label="Mức độ ưu tiên" rules={[{ required: true, message: 'Vui lòng chọn mức độ!' }]}>
                                <Select placeholder="Chọn ưu tiên" options={[
                                    { value: WorkTaskPriority.High, label: 'Cao (High)' },
                                    { value: WorkTaskPriority.Medium, label: 'Trung bình (Medium)' },
                                    { value: WorkTaskPriority.Low, label: 'Thấp (Low)' },
                                ]} />
                            </Form.Item>
                        </Col>
                    </Row>
                    <Form.Item name="dueDate" label="Hạn chót">
                        <DatePicker style={{ width: '100%' }} placeholder="Chọn ngày hết hạn" />
                    </Form.Item>
                </Form>
            </Modal>

            {/* DRAWER CHI TIẾT TASK */}
            <Drawer
                title={<span><EditOutlined /> Chi tiết công việc</span>}
                placement="right"
                styles={{ wrapper: { width: 480 } }}
                onClose={() => setIsDetailDrawerOpen(false)}
                open={isDetailDrawerOpen}
                footer={
                    <div style={{ display: 'flex', justifyContent: 'flex-end', padding: '8px 16px' }}>
                        <Space>
                            <Button type="primary" danger icon={<DeleteOutlined />} onClick={() => handleDeleteTask(String(selectedTask!.id))}>Xóa</Button>
                            {selectedTask && statusToUiKey(selectedTask.status) === 'done' && (
                                <Button
                                    type="primary"
                                    icon={<CheckCircleOutlined />}
                                    style={{ background: '#52c41a', borderColor: '#52c41a' }}
                                    onClick={handleFinalComplete}
                                    loading={isSubmitting}
                                >
                                    Chốt hoàn thành
                                </Button>
                            )}
                            <Button type="primary" icon={<SaveOutlined />} loading={isSubmitting} onClick={() => editForm.submit()}>Lưu thay đổi</Button>
                        </Space>
                    </div>
                }
            >
                {selectedTask && (
                    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
                        <div style={{ display: 'flex', alignItems: 'center', marginBottom: 16, padding: '12px 16px', background: '#f8fafc', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
                            <Avatar size={48} src={`https://api.dicebear.com/7.x/avataaars/svg?seed=${selectedTask.assigneeName ?? 'Unassigned'}`} style={{ marginRight: 16, border: '2px solid #fff', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }} />
                            <div style={{ flex: 1, minWidth: 0 }}>
                                <Title level={5} style={{ margin: 0, color: '#1e293b' }}>{selectedTask.title}</Title>
                                <Text type="secondary" style={{ fontSize: '13px' }}>Phụ trách: <strong>{selectedTask.assigneeName ?? "Chưa phân công"}</strong></Text>
                            </div>
                        </div>

                        <Space style={{ marginBottom: 24, padding: '0 4px' }}>
                            <Tag color={COLUMNS.find(c => c.key === statusToUiKey(selectedTask.status))?.color} style={{ padding: '4px 12px', borderRadius: '4px', border: 'none', fontWeight: 600 }}>
                                {COLUMNS.find(c => c.key === statusToUiKey(selectedTask.status))?.label ?? selectedTask.status}
                            </Tag>
                            <Tag color={selectedTask.priority === 'High' ? 'error' : selectedTask.priority === 'Medium' ? 'warning' : 'success'} style={{ padding: '4px 12px', borderRadius: '4px', border: 'none' }}>
                                {selectedTask.priority} Priority
                            </Tag>
                        </Space>

                        <div style={{ flex: 1, overflowX: 'hidden', overflowY: 'auto', paddingRight: '8px', paddingLeft: '2px' }}>
                            <Form form={editForm} layout="vertical" onFinish={handleUpdateTask}>
                                <Form.Item name="title" label={<span style={{ fontWeight: 500 }}>Tiêu đề công việc</span>} rules={[{ required: true, message: 'Vui lòng nhập tiêu đề!' }]}>
                                    <Input placeholder="Nhập tiêu đề" size="large" style={{ borderRadius: '6px' }} />
                                </Form.Item>

                                <Row gutter={16}>
                                    <Col span={12}>
                                        <Form.Item name="status" label={<span style={{ fontWeight: 500 }}>Trạng thái</span>}>
                                            <Select size="large" options={[
                                                { value: 'todo', label: 'Chờ thực hiện' },
                                                { value: 'inprogress', label: 'Đang thực hiện' },
                                                { value: 'done', label: 'Đã hoàn thành' },
                                            ]} style={{ borderRadius: '6px' }} />
                                        </Form.Item>
                                    </Col>
                                    <Col span={12}>
                                        <Form.Item name="priority" label={<span style={{ fontWeight: 500 }}>Mức độ ưu tiên</span>}>
                                            <Select size="large" options={[
                                                { value: WorkTaskPriority.High, label: 'Cao' },
                                                { value: WorkTaskPriority.Medium, label: 'Trung bình' },
                                                { value: WorkTaskPriority.Low, label: 'Thấp' },
                                            ]} style={{ borderRadius: '6px' }} />
                                        </Form.Item>
                                    </Col>
                                </Row>

                                <Form.Item name="description" label={<span style={{ fontWeight: 500 }}>Mô tả chi tiết</span>}>
                                    <Input.TextArea rows={5} placeholder="Nhập chi tiết yêu cầu công việc..." style={{ resize: 'none', borderRadius: '6px' }} />
                                </Form.Item>
                            </Form>

                            <Divider plain style={{ margin: '12px 0 24px 0', borderColor: '#e2e8f0', color: '#64748b' }}>Thông tin hệ thống</Divider>

                            <Descriptions column={1} bordered size="small" styles={{ label: { width: '120px', background: '#f8fafc', color: '#475569', fontWeight: 500 }, content: { background: '#fff' } }}>
                                <Descriptions.Item label={<span><CalendarOutlined style={{ marginRight: 6 }} /> Ngày tạo</span>}>
                                    {new Date(selectedTask.createdDate).toLocaleDateString("vi-VN")}
                                </Descriptions.Item>
                                {selectedTask.dueDate && (
                                    <Descriptions.Item label={<span><FlagOutlined style={{ marginRight: 6, color: '#ff4d4f' }} /> Hạn chót</span>}>
                                        <Text type="danger" strong>{new Date(selectedTask.dueDate).toLocaleDateString("vi-VN")}</Text>
                                    </Descriptions.Item>
                                )}
                                <Descriptions.Item label={<span><UserOutlined style={{ marginRight: 6 }} /> ID Nhân viên</span>}>
                                    {selectedTask.employeeId ?? <Text type="secondary">N/A</Text>}
                                </Descriptions.Item>
                            </Descriptions>
                        </div>
                    </div>
                )}
            </Drawer>
        </DndContext>
    );
}
