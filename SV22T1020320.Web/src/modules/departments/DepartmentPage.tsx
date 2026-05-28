import React, { useEffect, useState, useCallback } from "react";
import {
    Button, Card, Typography, Form, App, Input, Row, Col, Layout, Space, Breadcrumb, Tag, Table, Tooltip, Select, Modal
} from "antd";
import { PlusOutlined, SearchOutlined, EditOutlined, DeleteOutlined, HomeOutlined } from "@ant-design/icons";

import { useDepartmentTree } from "./hooks/useDepartmentTree";
import { useDepartmentBrowser } from "./hooks/useDepartmentBrowser";
import { useDepartmentSearch } from "./hooks/useDepartmentSearch";
import { DepartmentForm } from "./components/DepartmentForm";
import { DepartmentStats } from "./components/DepartmentStats";
import { DepartmentTree } from "./components/DepartmentTree";
import { TreeNode, CreateDepartmentRequest, UpdateDepartmentRequest } from "./types";
import { departmentApi } from "./api/departmentApi";
import { employeeApi } from "../employee/api/employeeApi";
import { authApi } from "../auth/api/authApi";
import { EmployeeTable } from "../employee/components/EmployeeTable";
import { EmployeeForm, EmployeeFormValues } from "../employee/components/EmployeeForm";
import {
    CreateEmployeeRequest,
    EmployeeDto,
    EmployeeStatus,
    GenderType,
    UpdateEmployeeRequest,
} from "../employee/types";
import { UserRole } from "../auth/types";

const { Title, Text } = Typography;
const { Sider, Content } = Layout;

const getApiErrorMessage = (error: unknown, fallback: string): string => {
    const apiError = error as {
        response?: {
            data?: {
                message?: string;
                detail?: string;
                title?: string;
                errors?: Record<string, string[]>;
            };
        };
    };

    const errors = apiError?.response?.data?.errors;
    const firstValidationError = errors
        ? Object.values(errors).flat().find(Boolean)
        : undefined;

    return (
        firstValidationError ||
        apiError?.response?.data?.detail ||
        apiError?.response?.data?.message ||
        apiError?.response?.data?.title ||
        fallback
    );
};

export default function DepartmentPage() {
    const { message } = App.useApp();
    const [form] = Form.useForm();

    const tree = useDepartmentTree();
    const browser = useDepartmentBrowser();
    const searcher = useDepartmentSearch();

    const [modalOpen, setModalOpen] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [editingManagerEmployeeId, setEditingManagerEmployeeId] = useState<number | null | undefined>(undefined);
    const [saving, setSaving] = useState(false);
    const [expandedKeys, setExpandedKeys] = useState<React.Key[]>([]);
    const [searchExpandedKeys, setSearchExpandedKeys] = useState<React.Key[]>([]);
    const [treeRenderVersion, setTreeRenderVersion] = useState(0);
    const [searchText, setSearchText] = useState("");
    const [displayMode, setDisplayMode] = useState<"browse" | "search">("browse");
    const [employees, setEmployees] = useState<EmployeeDto[]>([]);
    const [employeeLoading, setEmployeeLoading] = useState(true);
    const [employeeModalOpen, setEmployeeModalOpen] = useState(false);
    const [editingEmployee, setEditingEmployee] = useState<EmployeeDto | null>(null);
    const [employeeSubmitting, setEmployeeSubmitting] = useState(false);
    const [employeeSearchText, setEmployeeSearchText] = useState("");
    const [employeeStatusFilter, setEmployeeStatusFilter] = useState<EmployeeStatus | undefined>();
    const [employeeGenderFilter, setEmployeeGenderFilter] = useState<GenderType | undefined>();
    const [draggingEmployeeId, setDraggingEmployeeId] = useState<number | null>(null);
    const searchDebounceRef = React.useRef<number | null>(null);
    const searchRunIdRef = React.useRef(0);
    const pendingSnapshotsRef = React.useRef<Map<number, EmployeeDto[]>>(new Map());
    const employeesRef = React.useRef(employees);

    useEffect(() => {
        employeesRef.current = employees;
    }, [employees]);

    useEffect(() => {
        return () => {
            pendingSnapshotsRef.current.clear();
        };
    }, []);

    useEffect(() => {
        tree.loadRoots();
    }, [tree.loadRoots]);

    useEffect(() => {
        if (displayMode === "search") {
            setSearchExpandedKeys(searcher.expandedKeys);
        } else {
            setSearchExpandedKeys([]);
        }
    }, [displayMode, searcher.expandedKeys]);

    // Search (no-submit): keep input ultra responsive.
    // - onChange only updates `searchText`
    // - debounce triggers API
    // - keep showing old tree/table until results arrive (mode A)
    useEffect(() => {
        const scopeId = browser.currentNode.id;
        const trimmed = searchText.trim();

        // If user clears search, immediately go back to browse.
        if (!trimmed) {
            searcher.clear();
            setDisplayMode("browse");
            return;
        }

        const runId = ++searchRunIdRef.current;
        (async () => {
            await searcher.search(scopeId, trimmed, 1);
            // Only latest run can switch the UI to search mode
            if (searchRunIdRef.current === runId) {
                setDisplayMode("search");
            }
        })();
    }, [browser.currentNode.id, searchText, searcher.clear, searcher.search]);

    const onTreeSelect = useCallback((node: TreeNode) => {
        if (!node) return;
        // Nếu đang ở chế độ search: click vào kết quả nghĩa là "đi tới node đó"
        // và thoát khỏi trạng thái cây bị lọc theo keyword.
        if (displayMode === "search") {
            if (searchDebounceRef.current) window.clearTimeout(searchDebounceRef.current);
            searchRunIdRef.current++;
            setSearchText("");
            searcher.clear();
            setDisplayMode("browse");
        }
        browser.navigateFromTree(node);
    }, [browser, displayMode, searcher.clear]);

    const onTreeExpand = useCallback((keys: React.Key[]) => {
        setExpandedKeys(keys);
    }, []);

    const onSearchTreeExpand = useCallback((keys: React.Key[]) => {
        setSearchExpandedKeys(keys);
    }, []);

    const hardRefreshBrowseTree = useCallback(async () => {
        await tree.loadRoots();
        setExpandedKeys([]);
        setTreeRenderVersion(v => v + 1);
    }, [tree]);

    const handleEdit = useCallback(async (record: TreeNode) => {
        setEditingId(record.id);
        setEditingManagerEmployeeId(undefined);
        setModalOpen(true);
        try {
            const res = await departmentApi.getById(record.id);
            setEditingManagerEmployeeId(res.data.managerId);
            form.setFieldsValue({
                ...res.data,
                managerEmployeeId: res.data.managerId
            });
        } catch (error) {
            message.error(getApiErrorMessage(error, "Failed to load department data"));
            setModalOpen(false);
        }
    }, [form, message]);

    const handleDelete = useCallback(async (id: number) => {
        try {
            await departmentApi.delete(id);
            message.success("Department deleted successfully");
            tree.removeNodeLocal(id);
            await browser.refresh();
            searcher.clear();
        } catch (error) {
            message.error(getApiErrorMessage(error, "Failed to delete department"));
        }
    }, [browser, tree, message, searcher]);

    const canDeleteTreeNode = useCallback((node: TreeNode) => {
        const hasLoadedChildren = Array.isArray(node.children) && node.children.length > 0;
        return !node.hasChildren && !hasLoadedChildren;
    }, []);

    const handleSave = useCallback(async (values: any) => {
        setSaving(true);
        try {
            if (editingId) {
                const payload: UpdateDepartmentRequest = {
                    name: values.name,
                    code: values.code,
                    description: values.description,
                    isActive: values.isActive ?? true,
                    managerEmployeeId: values.managerEmployeeId
                };
                if (payload.managerEmployeeId === undefined && editingManagerEmployeeId !== undefined) {
                    payload.managerEmployeeId = editingManagerEmployeeId;
                }
                const res = await departmentApi.update(editingId, payload);
                message.success("Department updated successfully");
                tree.updateNodeLocal(editingId, res.data);
            } else {
                const hasExplicitParentInput = Object.prototype.hasOwnProperty.call(values, "parentId");
                const payload: CreateDepartmentRequest = {
                    name: values.name,
                    code: values.code,
                    description: values.description,
                    managerEmployeeId: values.managerEmployeeId,
                    parentId: hasExplicitParentInput
                        ? (values.parentId ?? null)
                        : (browser.currentNode.id ?? null)
                };
                const res = await departmentApi.create(payload);
                message.success("Department created successfully");
                tree.addNodeLocal(payload.parentId ?? null, res.data);
            }
            setModalOpen(false);
            await browser.refresh();
            searcher.clear();
        } catch (error) {
            message.error(getApiErrorMessage(error, "Operation failed"));
        } finally {
            setSaving(false);
        }
    }, [browser.currentNode.id, editingId, editingManagerEmployeeId, browser, tree, message, searcher]);

    const handleClearSearch = useCallback(() => {
        if (searchDebounceRef.current) window.clearTimeout(searchDebounceRef.current);
        searchRunIdRef.current++;
        setSearchText("");
        searcher.clear();
        setDisplayMode("browse");
    }, [searcher.clear]);

    const handleOpenCreateDepartment = useCallback(() => {
        setEditingId(null);
        setEditingManagerEmployeeId(undefined);
        form.resetFields();
        form.setFieldsValue({ parentId: browser.currentNode.id });
        setModalOpen(true);
    }, [browser.currentNode.id, form]);

    const handleDropDepartment = useCallback(async (dragId: number, targetId: number) => {
        if (dragId === targetId) return;
        try {
            await tree.move(dragId, targetId);
            message.success("Department moved successfully");
            await browser.refresh();
            searcher.clear();
        } catch (error) {
            message.error(getApiErrorMessage(error, "Failed to move department"));
        }
    }, [tree, browser, searcher, message]);

    const isShowingSearch = displayMode === "search";
    const loading = tree.loading || browser.loading || searcher.loading;
    const totalCount = isShowingSearch ? searcher.totalCount : browser.totalCount;
    const titleText = isShowingSearch
        ? `Search results: "${searcher.keyword}"`
        : browser.currentNode.name;

    const loadEmployeesByDepartment = useCallback(async () => {
        setEmployeeLoading(true);
        try {
            if (browser.currentNode.id == null) {
                const res = await employeeApi.getPaged({}, 1, 50);
                setEmployees(res.data.items);
                return;
            }

            const res = await employeeApi.getByDepartment(browser.currentNode.id);
            setEmployees(res.data);
        } catch (error) {
            message.error(getApiErrorMessage(error, "Failed to load employee list"));
            setEmployees([]);
        } finally {
            setEmployeeLoading(false);
        }
    }, [browser.currentNode.id, message]);

    const handleTransferEmployee = useCallback(async (employeeId: number, targetDepartmentId: number) => {
        const employee = employeesRef.current.find(e => e.id === employeeId);
        if (!employee) return;
        if (employee.departmentId === targetDepartmentId) {
            message.info("Nhân viên này đã thuộc phòng ban được chọn.");
            return;
        }

        setEmployees(prev => {
            pendingSnapshotsRef.current.set(employeeId, [...prev]);

            if (browser.currentNode.id != null) {
                return prev.filter(e => e.id !== employeeId);
            }
            return prev.map(e =>
                e.id === employeeId
                    ? { ...e, departmentId: targetDepartmentId }
                    : e
            );
        });

        try {
            await employeeApi.transfer(employeeId, { newDepartmentId: targetDepartmentId });
            message.success("Chuyển phòng ban thành công");
            pendingSnapshotsRef.current.delete(employeeId);

            // Background sync (non-blocking) - Only sync if no other pending transfers to avoid overwriting optimistic states
            if (pendingSnapshotsRef.current.size === 0) {
                void loadEmployeesByDepartment();
            }
        } catch (error) {
            const snapshot = pendingSnapshotsRef.current.get(employeeId);
            if (snapshot) {
                setEmployees(snapshot);
                pendingSnapshotsRef.current.delete(employeeId);
            }
            message.error(getApiErrorMessage(error, "Lỗi khi chuyển phòng ban"));
        }
    }, [browser.currentNode.id, loadEmployeesByDepartment, message]);

    useEffect(() => {
        void loadEmployeesByDepartment();
    }, [loadEmployeesByDepartment]);

    const handleOpenCreateEmployee = useCallback(() => {
        setEditingEmployee(null);
        setEmployeeModalOpen(true);
    }, []);

    const handleOpenEditEmployee = useCallback((record: EmployeeDto) => {
        setEditingEmployee(record);
        setEmployeeModalOpen(true);
    }, []);

    const handleDeleteEmployee = useCallback(async (id: number) => {
        try {
            await employeeApi.delete(id);
            message.success("Employee deleted successfully");
            await loadEmployeesByDepartment();
        } catch (error) {
            message.error(getApiErrorMessage(error, "Failed to delete employee"));
        }
    }, [loadEmployeesByDepartment, message]);

    const handleSaveEmployee = useCallback(async (values: EmployeeFormValues) => {
        setEmployeeSubmitting(true);
        try {
            const payload: CreateEmployeeRequest = {
                employeeCode: values.employeeCode.trim(),
                fullName: values.fullName.trim(),
                email: values.email.trim(),
                gender: values.gender,
                phone: values.phone?.trim() || null,
                position: values.position?.trim() || null,
                baseSalary: values.baseSalary,
                // Create: always bind department by selected node; root means null
                departmentId: browser.currentNode.id ?? null,
                joinDate: values.joinDate ? values.joinDate.toISOString() : null,
                birthDate: values.birthDate ? values.birthDate.toISOString() : null,
            };

            if (editingEmployee) {
                const updatePayload: UpdateEmployeeRequest = {
                    ...payload,
                    // Update: ignore department selection for now, keep current department
                    departmentId: editingEmployee.departmentId ?? null,
                    status: values.status ?? EmployeeStatus.Active,
                };
                await employeeApi.update(editingEmployee.id, updatePayload);
                message.success("Employee updated successfully");
            } else {
                await employeeApi.create(payload);
                message.success("Employee created successfully");
            }

            setEmployeeModalOpen(false);
            setEditingEmployee(null);
            await loadEmployeesByDepartment();
        } catch (error) {
            message.error(getApiErrorMessage(error, "Failed to save employee"));
        } finally {
            setEmployeeSubmitting(false);
        }
    }, [browser.currentNode.id, editingEmployee, loadEmployeesByDepartment, message]);

    const handleResetPassword = useCallback(async (id: number) => {
        try {
            const res = await authApi.resetPassword(id);

            message.success("Reset password thành công");

            Modal.info({
                title: "Mật khẩu mới",
                content: res.data.password,
            });
        } catch (error) {
            message.error(getApiErrorMessage(error, "Reset password failed"));
        }
    }, [message]);

    const handleUpdateRole = useCallback(async (id: number, role: UserRole) => {
        try {
            await authApi.updateRole(id, role);
            message.success("Cập nhật quyền thành công");
        } catch (error) {
            message.error(getApiErrorMessage(error, "Update role failed"));
        }
    }, [message]);

    const filteredEmployees = React.useMemo(() => {
        return employees.filter((item) => {
            const byName = item.fullName.toLowerCase().includes(employeeSearchText.trim().toLowerCase());

            const byStatus = (() => {
                if (employeeStatusFilter === undefined) return true;
                if (employeeStatusFilter === EmployeeStatus.Active) return item.status === "Active";
                if (employeeStatusFilter === EmployeeStatus.Inactive) return item.status === "Inactive";
                if (employeeStatusFilter === EmployeeStatus.Resigned) return item.status === "Resigned";
                if (employeeStatusFilter === EmployeeStatus.OnLeave) return item.status === "OnLeave";
                return true;
            })();

            const byGender = (() => {
                if (employeeGenderFilter === undefined) return true;
                if (employeeGenderFilter === GenderType.Male) return item.gender === "Male";
                if (employeeGenderFilter === GenderType.Female) return item.gender === "Female";
                if (employeeGenderFilter === GenderType.Other) return item.gender === "Other";
                return true;
            })();

            return byName && byStatus && byGender;
        });
    }, [employeeGenderFilter, employeeSearchText, employeeStatusFilter, employees]);



    return (
        <Layout style={{ minHeight: "100vh", background: "#f0f2f5" }}>
            <Sider
                width="25%"
                style={{
                    minWidth: 240,
                    maxWidth: 360,
                    flex: "0 0 auto",
                    background: "#f0f2f5",
                    padding: "24px 16px",
                    height: "100vh",
                    position: "sticky",
                    top: 0,
                    overflow: "auto",
                    scrollbarGutter: 'stable'
                }}
            >
                <div style={{ width: "100%", height: "100%", display: "flex", flexDirection: "column" }}>
                    <DepartmentTree
                        treeData={isShowingSearch ? searcher.treeResults : tree.treeData}
                        selectedNodeId={browser.currentNode.id}
                        expandedKeys={isShowingSearch ? searchExpandedKeys : expandedKeys}
                        onExpand={isShowingSearch ? onSearchTreeExpand : onTreeExpand}
                        loading={loading}
                        searchText={searchText}
                        onSearchChange={setSearchText}
                        isSearching={isShowingSearch}
                        onSelect={onTreeSelect}
                        onEdit={handleEdit}
                        onDelete={handleDelete}
                        onAdd={handleOpenCreateDepartment}
                        onDropDepartment={handleDropDepartment}
                        onDropEmployee={handleTransferEmployee}
                        draggingEmployeeId={draggingEmployeeId}
                        loadChildren={tree.loadChildren}
                    />
                </div>
            </Sider>

            <Content style={{ padding: "24px" }}>
                <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
                    <Col>
                        <Breadcrumb
                            separator=">"
                            items={browser.breadcrumbs.map((item, index) => {
                                const isCurrent = index === browser.breadcrumbs.length - 1;
                                const isRoot = item.id === null;
                                const label = isRoot ? "All Departments" : item.name;

                                if (isCurrent) {
                                    return {
                                        title: (
                                            <Space
                                                size={4}
                                                style={{
                                                    display: "inline-flex",
                                                    alignItems: "center",
                                                    lineHeight: 1,
                                                }}
                                            >
                                                {isRoot && <HomeOutlined />}
                                                <Text strong>{label}</Text>
                                            </Space>
                                        ),
                                    };
                                }

                                return {
                                    title: (
                                        <Button
                                            type="link"
                                            onClick={() => browser.navigateToBreadcrumb(index)}
                                            style={{
                                                padding: 0,
                                                height: "auto",
                                                display: "inline-flex",
                                                alignItems: "center",
                                                lineHeight: 1,
                                            }}
                                            icon={isRoot ? <HomeOutlined /> : undefined}
                                        >
                                            {label}
                                        </Button>
                                    ),
                                };
                            })}
                        />
                    </Col>
                </Row>

                <DepartmentStats totalDepts={totalCount} totalMembers={employees.length} loading={loading || employeeLoading} />

                <Card style={{ marginBottom: 16, borderRadius: 8 }}>
                    <Space wrap>
                        <Input
                            placeholder="Search by name..."
                            prefix={<SearchOutlined />}
                            style={{ width: 280 }}
                            value={employeeSearchText}
                            onChange={(e) => setEmployeeSearchText(e.target.value)}
                            allowClear
                        />
                        <Select
                            allowClear
                            placeholder="Filter status"
                            style={{ width: 170 }}
                            value={employeeStatusFilter}
                            onChange={setEmployeeStatusFilter}
                            options={[
                                { value: EmployeeStatus.Active, label: "Active" },
                                { value: EmployeeStatus.Inactive, label: "Inactive" },
                                { value: EmployeeStatus.Resigned, label: "Resigned" },
                                { value: EmployeeStatus.OnLeave, label: "On Leave" },
                            ]}
                        />
                        <Select
                            allowClear
                            placeholder="Filter gender"
                            style={{ width: 170 }}
                            value={employeeGenderFilter}
                            onChange={setEmployeeGenderFilter}
                            options={[
                                { value: GenderType.Male, label: "Male" },
                                { value: GenderType.Female, label: "Female" },
                                { value: GenderType.Other, label: "Other" },
                            ]}
                        />
                        <Button
                            onClick={() => {
                                setEmployeeSearchText("");
                                setEmployeeStatusFilter(undefined);
                                setEmployeeGenderFilter(undefined);
                            }}
                        >
                            Reset
                        </Button>
                    </Space>
                </Card>

                <Card
                    title={
                        <Space>
                            <Text strong>{titleText}</Text>
                            {isShowingSearch && <Tag color="blue">{totalCount} items</Tag>}
                        </Space>
                    }
                    extra={
                        <Space>
                            {isShowingSearch && <Button type="link" onClick={handleClearSearch}>Back</Button>}
                            <Tooltip
                                title={
                                    browser.currentNode.id == null
                                        ? "Add employee without department"
                                        : `Add employee to ${browser.currentNode.name}`
                                }
                            >
                                <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreateEmployee}>
                                    Add Employee
                                </Button>
                            </Tooltip>
                        </Space>
                    }
                    style={{ borderRadius: 8 }}
                >
                    <EmployeeTable
                        loading={employeeLoading}
                        dataSource={filteredEmployees}
                        pagination={{ pageSize: 10 }}
                        onEdit={handleOpenEditEmployee}
                        onDelete={handleDeleteEmployee}
                        onResetPassword={handleResetPassword}
                        onUpdateRole={handleUpdateRole}
                        draggable
                        onDragStart={(record, e) => {
                            e.dataTransfer.setData("employeeId", record.id.toString());
                            setDraggingEmployeeId(record.id);
                        }}
                        onDragEnd={() => setDraggingEmployeeId(null)}
                    />
                </Card>
            </Content>

            <DepartmentForm
                open={modalOpen}
                form={form}
                isEdit={!!editingId}
                onCancel={() => setModalOpen(false)}
                onSave={handleSave}
                saving={saving}
            />

            <EmployeeForm
                open={employeeModalOpen}
                isEdit={!!editingEmployee}
                submitting={employeeSubmitting}
                initialData={editingEmployee}
                onCancel={() => setEmployeeModalOpen(false)}
                onSubmit={handleSaveEmployee}
            />
        </Layout>
    );
}
