import React, { useEffect, useState } from 'react';
import { Modal, Form, Input, Select, Typography, FormInstance, Switch, App, Tag } from "antd";
import { departmentApi } from "../api/departmentApi";
import { employeeApi } from "../../employee/api/employeeApi";
import { DepartmentDto } from "../types";
import { EmployeeManagerDto } from "../../employee/types";
import { UserOutlined, ApartmentOutlined } from '@ant-design/icons';

const { Text } = Typography;

interface Props {
    open: boolean;
    isEdit: boolean;
    onCancel: () => void;
    onSave: (values: any) => void;
    form: FormInstance;
    saving?: boolean;
}

export const DepartmentForm = ({ open, isEdit, onCancel, onSave, form, saving }: Props) => {
    const { message } = App.useApp();
    const [roots, setRoots] = useState<DepartmentDto[]>([]);
    const [loadingRoots, setLoadingRoots] = useState(false);
    const [managers, setManagers] = useState<EmployeeManagerDto[]>([]);
    const [loadingManagers, setLoadingManagers] = useState(false);

    // Load root departments when opening modal
    useEffect(() => {
        if (!open) return;

        const fetchData = async () => {
            if (!isEdit) {
                setLoadingRoots(true);
                try {
                    const res = await departmentApi.getRootsPaged(1, 100);
                    setRoots(res.data.items);
                } catch {
                    message.error("Failed to load department list");
                } finally {
                    setLoadingRoots(false);
                }
            }

            setLoadingManagers(true);
            try {
                const res = await employeeApi.getManagers();
                setManagers(res.data);
            } catch {
                message.error("Failed to load managers list");
            } finally {
                setLoadingManagers(false);
            }
        };

        fetchData();
    }, [open, isEdit, message]);

    return (
        <Modal
            title={
                <Text strong style={{ fontSize: 18 }}>
                    {isEdit ? "Update Department" : "Add New Department"}
                </Text>
            }
            open={open}
            onOk={() => form.submit()}
            onCancel={onCancel}
            okText={isEdit ? "Update" : "Create"}
            cancelText="Cancel"
            confirmLoading={saving}
            afterClose={() => form.resetFields()}
            style={{ top: 20 }}
        >
            <Form form={form} layout="vertical" onFinish={onSave} style={{ paddingTop: 16 }}>
                <Form.Item
                    name="name"
                    label="Department Name"
                    rules={[{ required: true, message: "Please enter department name" }]}
                >
                    <Input placeholder="e.g. Engineering Department" />
                </Form.Item>

                <Form.Item
                    name="code"
                    label="Department Code"
                    rules={[{ required: true, message: "Please enter department code" }]}
                >
                    <Input
                        placeholder="e.g. DEPT001"
                        disabled={isEdit}
                        style={{ textTransform: 'uppercase' }}
                    />
                </Form.Item>

                {/* Show only when creating */}
                {!isEdit && (
                    <Form.Item
                        name="parentId"
                        label="Parent Department"
                    >
                        <Select
                            allowClear
                            placeholder="Leave empty for root department"
                            loading={loadingRoots}
                            showSearch
                            optionFilterProp="label"
                            options={roots.map(d => ({
                                value: d.id,
                                label: d.code ? `${d.name} (${d.code})` : d.name,
                            }))}
                        />
                    </Form.Item>
                )}

                <Form.Item
                    name="managerEmployeeId"
                    label="Department Manager"
                >
                    <Select
                        allowClear
                        placeholder="Select a manager"
                        loading={loadingManagers}
                        showSearch
                        optionFilterProp="filter"
                        style={{ width: '100%' }}
                        dropdownStyle={{ minWidth: 300 }}
                        options={managers.map(m => ({
                            value: m.employeeID,
                            filter: `${m.fullName} ${m.employeeCode} ${m.position}`,
                            label: (
                                <div style={{ display: 'flex', flexDirection: 'column', padding: '4px 0' }}>
                                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                        <Text strong style={{ fontSize: '14px' }}>
                                            <UserOutlined style={{ marginRight: 8, color: '#1890ff' }} />
                                            {m.fullName}
                                        </Text>
                                        <Tag color="blue" style={{ margin: 0 }}>{m.employeeCode}</Tag>
                                    </div>
                                    <div style={{ display: 'flex', alignItems: 'center', marginTop: 4 }}>
                                        <Text type="secondary" style={{ fontSize: '12px', display: 'flex', alignItems: 'center' }}>
                                            <ApartmentOutlined style={{ marginRight: 4, fontSize: '12px' }} />
                                            {m.position || "Staff"} • {m.departmentName || "No Dept"}
                                        </Text>
                                    </div>
                                </div>
                            )
                        }))}
                    />
                </Form.Item>

                <Form.Item name="description" label="Description / Notes">
                    <Input.TextArea placeholder="Enter description..." rows={2} />
                </Form.Item>

                <Form.Item
                    name="isActive"
                    label="Active Status"
                    valuePropName="checked"
                    initialValue={true}
                >
                    <Switch />
                </Form.Item>
            </Form>
        </Modal>
    );
};