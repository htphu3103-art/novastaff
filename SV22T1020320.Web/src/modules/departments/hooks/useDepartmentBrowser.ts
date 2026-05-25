import { useState, useCallback, useEffect } from "react";
import { App } from "antd";
import { BreadcrumbItem, TreeNode } from "../types";
import { departmentApi } from "../api/departmentApi";
import { mapNode } from "./useDepartmentTree";

const PAGE_SIZE = 10;

export const useDepartmentBrowser = () => {
    const { message } = App.useApp();

    // Breadcrumb stack: [{ id: null, name: "Toàn bộ" }, { id: 1, name: "Kỹ thuật" }, ...]
    const [breadcrumbs, setBreadcrumbs] = useState<BreadcrumbItem[]>([
        { id: null, name: "Toàn bộ đơn vị" },
    ]);

    const [tableData, setTableData] = useState<TreeNode[]>([]);
    const [loading, setLoading] = useState(false);
    const [page, setPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);

    const currentNode = breadcrumbs[breadcrumbs.length - 1];

    const loadTable = useCallback(async (nodeId: number | null, p: number) => {
        setLoading(true);
        try {
            if (nodeId === null) {
                // Load roots (trang gốc)
                const res = await departmentApi.getRootsPaged(p, PAGE_SIZE);
                setTableData(res.data.items.map(mapNode));
                setTotalCount(res.data.totalCount);
            } else {
                const res = await departmentApi.getChildrenPaged(nodeId, p, PAGE_SIZE);
                setTableData(res.data.items.map(mapNode));
                setTotalCount(res.data.totalCount);
            }
        } catch {
            message.error("Failed to load department data");
        } finally {
            setLoading(false);
        }
    }, [message]);

    useEffect(() => {
        loadTable(currentNode.id, page);
    }, [currentNode.id, page, loadTable]);

    // Click node trên sider → navigate vào
    const navigateTo = useCallback((node: TreeNode) => {
        setBreadcrumbs(prev => {
            // Tránh push trùng
            if (prev[prev.length - 1].id === node.id) return prev;
            return [...prev, { id: node.id, name: node.name }];
        });
        setPage(1);
    }, []);

    // Click node trên cây (Tree) → rebuild breadcrumb theo parentId để tránh /a/b/c/a/b bị lặp
    const navigateFromTree = useCallback(async (node: TreeNode) => {
        if (node.id == null) return;
        // Optimistic: update ngay để table/breadcrumb đổi lập tức
        setBreadcrumbs([{ id: null, name: "Toàn bộ đơn vị" }, { id: node.id, name: node.name }]);
        setPage(1);

        setLoading(true);
        try {
            const chain: BreadcrumbItem[] = [];
            let cursorId: number | null | undefined = node.id;

            // Walk up using getById → parentId
            while (cursorId != null) {
                const res = await departmentApi.getById(cursorId);
                chain.push({ id: res.data.id, name: res.data.name });
                cursorId = res.data.parentId ?? null;
            }

            chain.reverse();
            setBreadcrumbs([{ id: null, name: "Toàn bộ đơn vị" }, ...chain]);
        } catch {
            message.error("Failed to build department breadcrumb");
        } finally {
            setLoading(false);
        }
    }, [message]);

    // Click breadcrumb item → pop stack về vị trí đó
    const navigateToBreadcrumb = useCallback((index: number) => {
        setBreadcrumbs(prev => prev.slice(0, index + 1));
        setPage(1);
    }, []);

    // Double-click row trong table → navigate vào
    const navigateInto = useCallback((node: TreeNode) => {
        if (!node.hasChildren) return;
        setBreadcrumbs(prev => [...prev, { id: node.id, name: node.name }]);
        setPage(1);
    }, []);

    const refresh = useCallback(() => {
        loadTable(currentNode.id, page);
    }, [currentNode.id, page, loadTable]);

    return {
        breadcrumbs,
        tableData,
        loading,
        page,
        pageSize: PAGE_SIZE,
        totalCount,
        currentNode,
        navigateTo,
        navigateFromTree,
        navigateToBreadcrumb,
        navigateInto,
        setPage,
        refresh,
    };
};