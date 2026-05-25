export interface DepartmentDto {
    id: number;
    name: string;
    code: string;
    level: number;
    managerName?: string;
    managerId?: number | null;
    isActive: boolean;
    hasChildren: boolean | null;
    parentId?: number | null;
    description?: string;
}

import type { Key } from "react";

export interface TreeNode extends Omit<DepartmentDto, 'hasChildren'> {
    key: Key;
    hasChildren: boolean;
    isLeaf?: boolean;
    isLoaded: boolean;
    children?: TreeNode[];
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages?: number;
    hasNext?: boolean;
    hasPrevious?: boolean;
}
export interface CreateDepartmentRequest {
    name: string;
    code?: string;
    parentId?: number | null;
    description?: string;
    managerEmployeeId?: number | null;
}

export interface UpdateDepartmentRequest {
    name: string;
    code?: string;
    description?: string;
    isActive: boolean;
    managerEmployeeId?: number | null;
}
interface DepartmentTableProps {
    dataSource: TreeNode[];
    loading: boolean;
    onDelete: (id: number) => void;
    onEdit: (record: TreeNode) => void;
    onExpand: (expanded: boolean, record: TreeNode, isSearching?: boolean, setSearchTree?: React.Dispatch<React.SetStateAction<TreeNode[]>>) => void;
    onMove: (id: number, newParentId: number | null) => void;
    searchTree?: TreeNode[];
    setSearchTree?: React.Dispatch<React.SetStateAction<TreeNode[]>>;  // ✅ ĐÃ CÓ
    isSearching?: boolean;
    searchExpandedKeys?: number[];
    keyword?: string;
}

export interface BreadcrumbItem {
    id: number | null;
    name: string;
}