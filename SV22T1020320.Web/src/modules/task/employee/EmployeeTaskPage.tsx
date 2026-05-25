import React, { useState, useEffect } from "react";
import { Row, Col, Typography, Drawer, Space, Tag, Divider, Descriptions, Spin, App, Avatar } from "antd";
import { InfoCircleOutlined, UserOutlined, CalendarOutlined, FlagOutlined } from "@ant-design/icons";
import { WorkTaskDto, COLUMNS } from "../types";
import { taskApi } from "../api/taskApi";
import KanbanColumn from "../components/KanbanColumn";

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

export default function EmployeeTaskPage({ user }: any) {
    const { message } = App.useApp();
    const [tasks, setTasks] = useState<WorkTaskDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    const [selectedTask, setSelectedTask] = useState<WorkTaskDto | null>(null);
    const [isDetailDrawerOpen, setIsDetailDrawerOpen] = useState(false);

    // ===================================================================
    // 1. FETCH DỮ LIỆU
    // ===================================================================
    const fetchTasks = async () => {
        setIsLoading(true);
        try {
            const employeeId = user?.userId ?? user?.id;
            let response;
            if (employeeId) {
                response = await taskApi.getByAssignee(employeeId, 1, 100);
            } else {
                response = await taskApi.getPaged({}, 1, 100);
            }
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
    }, [user?.id]);

    // Mở chi tiết task (Read-only)
    const handleTaskClick = async (task: WorkTaskDto) => {
        setSelectedTask(task);
        setIsDetailDrawerOpen(true);

        try {
            const res = await taskApi.getById(task.id);
            setSelectedTask(res.data);
        } catch (err) {
            message.warning("Đang hiển thị dữ liệu cũ. Không thể tải bản cập nhật mới nhất.");
        }
    };

    return (
        <div>
            <div style={{ marginBottom: 20 }}>
                <Title level={4} style={{ margin: 0 }}>My Tasks ({user?.name || 'Employee'})</Title>
                <Text type="secondary">Danh sách công việc được phân công. Click để xem chi tiết.</Text>
            </div>

            <Spin spinning={isLoading} description="Đang tải công việc...">
                <Row gutter={16}>
                    {COLUMNS.map(col => (
                        <Col span={8} key={col.key}>
                            <KanbanColumn
                                title={col.label}
                                status={col.key}
                                color={col.color}
                                tasks={tasks.filter(t => statusToUiKey(t.status) === col.key && (col.key === 'done' ? !t.completedDate : true)) as any}
                                isAdmin={false}
                                onTaskClick={handleTaskClick as any}
                            />
                        </Col>
                    ))}
                </Row>
            </Spin>

            {/* DRAWER CHI TIẾT TASK (READ-ONLY) */}
            <Drawer
                title={<span><InfoCircleOutlined /> Chi tiết công việc</span>}
                placement="right"
                styles={{ wrapper: { width: 450 } }}
                onClose={() => setIsDetailDrawerOpen(false)}
                open={isDetailDrawerOpen}
            >
                {selectedTask && (
                    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
                        <div style={{ display: 'flex', alignItems: 'center', marginBottom: 16, padding: '12px 16px', background: '#f8fafc', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
                            <Avatar size={48} src={`https://api.dicebear.com/7.x/avataaars/svg?seed=${selectedTask.assigneeName ?? 'Unassigned'}`} style={{ marginRight: 16, border: '2px solid #fff', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }} />
                            <div style={{ flex: 1, minWidth: 0 }}>
                                <Title level={5} style={{ margin: 0, color: '#1e293b' }}>{selectedTask.title}</Title>
                                <Text type="secondary" style={{ fontSize: '13px' }}>ID Công việc: <strong>#{selectedTask.id}</strong></Text>
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
                            <Divider plain style={{ margin: '0 0 16px 0', borderColor: '#e2e8f0', color: '#64748b' }}>Mô tả chi tiết</Divider>
                            <div style={{ padding: '16px', background: '#f8fafc', borderRadius: '8px', border: '1px solid #e2e8f0', whiteSpace: 'pre-wrap', minHeight: '120px', color: '#334155' }}>
                                {selectedTask.description || <Text type="secondary" italic>Không có mô tả chi tiết.</Text>}
                            </div>

                            <Divider plain style={{ margin: '24px 0 16px 0', borderColor: '#e2e8f0', color: '#64748b' }}>Thông tin thời gian</Divider>

                            <Descriptions column={1} bordered size="small" styles={{ label: { width: '120px', background: '#f8fafc', color: '#475569', fontWeight: 500 }, content: { background: '#fff' } }}>
                                <Descriptions.Item label={<span><CalendarOutlined style={{ marginRight: 6 }} /> Ngày tạo</span>}>
                                    {new Date(selectedTask.createdDate).toLocaleDateString("vi-VN")}
                                </Descriptions.Item>
                                {selectedTask.dueDate && (
                                    <Descriptions.Item label={<span><FlagOutlined style={{ marginRight: 6, color: '#ff4d4f' }} /> Hạn chót</span>}>
                                        <Text type="danger" strong>{new Date(selectedTask.dueDate).toLocaleDateString("vi-VN")}</Text>
                                    </Descriptions.Item>
                                )}
                            </Descriptions>
                        </div>
                    </div>
                )}
            </Drawer>
        </div>
    );
}
