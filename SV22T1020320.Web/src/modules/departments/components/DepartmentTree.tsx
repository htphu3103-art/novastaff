import React, { useState } from "react";
import { Tree, Input, Button, Typography, Space, Tooltip, Spin, Popconfirm, Tag, Skeleton } from "antd";
import { 
    PlusOutlined, 
    SearchOutlined, 
    ApartmentOutlined, 
    EditOutlined, 
    DeleteOutlined 
} from "@ant-design/icons";
import { TreeNode } from "../types";

const { Title, Text } = Typography;

const DebouncedSearchInput = ({ value, onChange, loading }: { value: string, onChange: (val: string) => void, loading: boolean }) => {
    const [localValue, setLocalValue] = useState(value);
    const timeoutRef = React.useRef<number | null>(null);

    React.useEffect(() => {
        setLocalValue(value);
    }, [value]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = e.target.value;
        setLocalValue(val);
        if (timeoutRef.current) window.clearTimeout(timeoutRef.current);
        timeoutRef.current = window.setTimeout(() => {
            onChange(val);
        }, 300);
    };

    return (
        <div style={{ position: 'relative' }}>
            <Input
                placeholder="Quick search..."
                prefix={<SearchOutlined />}
                onChange={handleChange}
                allowClear
                value={localValue}
            />
            {localValue.trim().length > 0 && loading && (
                <div style={{ position: 'absolute', right: 30, top: 6 }}>
                    <Spin size="small" />
                </div>
            )}
        </div>
    );
};

interface DepartmentTreeProps {
    treeData: any[];
    selectedNodeId: number | null;
    expandedKeys: React.Key[];
    onExpand: (keys: React.Key[]) => void;
    loading: boolean;
    searchText: string;
    onSearchChange: (text: string) => void;
    isSearching: boolean;
    onSelect: (node: TreeNode) => void;
    onEdit: (node: TreeNode) => void;
    onDelete: (id: number) => Promise<void>;
    onAdd: () => void;
    onDropDepartment: (dragId: number, targetId: number) => Promise<void>;
    onDropEmployee: (employeeId: number, targetDepartmentId: number) => Promise<void>;
    draggingEmployeeId: number | null;
    loadChildren: (node: TreeNode) => Promise<void>;
}

export const DepartmentTree: React.FC<DepartmentTreeProps> = ({
    treeData,
    selectedNodeId,
    expandedKeys,
    onExpand,
    loading,
    searchText,
    onSearchChange,
    isSearching,
    onSelect,
    onEdit,
    onDelete,
    onAdd,
    onDropDepartment,
    onDropEmployee,
    draggingEmployeeId,
    loadChildren
}) => {
    const [dragOverNodeId, setDragOverNodeId] = useState<number | null>(null);

    const canDeleteTreeNode = (node: TreeNode) => {
        const hasLoadedChildren = Array.isArray(node.children) && node.children.length > 0;
        return !node.hasChildren && !hasLoadedChildren;
    };

    return (
        <div style={{ 
            background: "#fff", 
            border: "1px solid #eceff3", 
            borderRadius: 10, 
            padding: 16, 
            height: "100%", 
            display: "flex", 
            flexDirection: "column" 
        }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
                <Title level={5} style={{ margin: 0 }}>Organization Chart</Title>
                <Tooltip title="Add department">
                    <Button 
                        type="primary" 
                        shape="circle" 
                        icon={<PlusOutlined />} 
                        onClick={onAdd}
                    />
                </Tooltip>
            </div>

            {/* <DebouncedSearchInput
                value={searchText}
                onChange={onSearchChange}
                loading={loading}
            /> */}

            <div style={{ marginTop: 24, flex: 1 }}>
                {loading && treeData.length === 0 ? (
                    <div style={{ padding: '20px', textAlign: 'center' }}><Spin size="small" description="Loading..." /></div>
                ) : (
                    <Tree
                        treeData={treeData}
                        selectedKeys={selectedNodeId !== null ? [String(selectedNodeId)] : []}
                        expandedKeys={expandedKeys}
                        onExpand={onExpand}
                        autoExpandParent
                        loadData={isSearching ? undefined : (node: any) => loadChildren(node as TreeNode)}
                        draggable
                        onDrop={async (info: any) => {
                            if (isSearching) return;
                            const dragId = Number(info.dragNode?.id ?? info.dragNode?.key);
                            const targetId = Number(info.node?.id ?? info.node?.key);
                            if (Number.isFinite(dragId) && Number.isFinite(targetId) && dragId !== targetId) {
                                await onDropDepartment(dragId, targetId);
                            }
                        }}
                        onSelect={(_keys, info) => onSelect(info.node as unknown as TreeNode)}
                        titleRender={(node: any) => {
                            const isDragOver = dragOverNodeId === node.id;
                            return (
                                <div
                                    onDragOver={(e) => {
                                        if (draggingEmployeeId !== null) {
                                            e.preventDefault();
                                            e.stopPropagation();
                                            if (dragOverNodeId !== node.id) setDragOverNodeId(node.id);
                                        }
                                    }}
                                    onDragLeave={() => {
                                        if (dragOverNodeId === node.id) setDragOverNodeId(null);
                                    }}
                                    onDrop={(e) => {
                                        if (draggingEmployeeId !== null) {
                                            setDragOverNodeId(null);
                                            e.preventDefault();
                                            e.stopPropagation();
                                            onDropEmployee(draggingEmployeeId, node.id);
                                        }
                                    }}
                                    style={{ 
                                        display: "inline-block", 
                                        width: "100%",
                                        backgroundColor: isDragOver ? "#e6f4ff" : "transparent",
                                        borderRadius: 4,
                                        padding: "2px 4px"
                                    }}
                                >
                                    <Space size={6} style={{ width: '100%', justifyContent: 'space-between' }}>
                                        <Space size={6}>
                                            <ApartmentOutlined style={{ color: '#1677ff' }} />
                                            <span>{node.name}</span>
                                        </Space>
                                        
                                        {!isSearching && (
                                            <Space size={2} className="node-actions">
                                                <Button
                                                    type="text"
                                                    size="small"
                                                    icon={<EditOutlined />}
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        onEdit(node as TreeNode);
                                                    }}
                                                />
                                                {canDeleteTreeNode(node as TreeNode) && (
                                                    <Popconfirm
                                                        title="Delete this department?"
                                                        onConfirm={(e) => {
                                                            e?.stopPropagation();
                                                            onDelete(node.id);
                                                        }}
                                                        okButtonProps={{ danger: true }}
                                                        onPopupClick={(e) => e.stopPropagation()}
                                                    >
                                                        <Button
                                                            type="text"
                                                            size="small"
                                                            danger
                                                            icon={<DeleteOutlined />}
                                                            onClick={(e) => e.stopPropagation()}
                                                        />
                                                    </Popconfirm>
                                                )}
                                            </Space>
                                        )}
                                    </Space>
                                </div>
                            );
                        }}
                        blockNode
                    />
                )}
            </div>
        </div>
    );
};
