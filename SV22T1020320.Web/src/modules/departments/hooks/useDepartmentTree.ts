import { useCallback, useState } from "react";
import { App } from "antd";
import { DepartmentDto, TreeNode } from "../types";
import { departmentApi } from "../api/departmentApi";

export const mapNode = (d: DepartmentDto): TreeNode => {
    const hasChildren = d.hasChildren !== false;

    return {
        ...d,
        key: String(d.id),
        hasChildren,
        isLeaf: d.hasChildren === false,
        isLoaded: false,
        children: undefined,
    };
};

const updateNodeInTree = (
    nodes: TreeNode[],
    targetId: number,
    updater: (n: TreeNode) => TreeNode
): TreeNode[] =>
    nodes.map(n => {
        const targetKey = String(targetId);
        const nIdNum = Number((n as any).id);
        const matches =
            n.id === targetId ||
            (Number.isFinite(nIdNum) && nIdNum === targetId) ||
            String(n.key) === targetKey ||
            String((n as any).id) === targetKey;
        if (matches) return updater(n);
        if (n.children) return { ...n, children: updateNodeInTree(n.children, targetId, updater) };
        return n;
    });

export const useDepartmentTree = () => {
    const { message } = App.useApp();
    const [treeData, setTreeData] = useState<TreeNode[]>([]);
    const [loading, setLoading] = useState(false);

    const loadRoots = useCallback(async () => {
        setLoading(true);
        try {
            await new Promise(resolve => setTimeout(resolve, 300));
            const res = await departmentApi.getRootsPaged(1, 100);
            setTreeData(res.data.items.map(mapNode));
        } catch {
            message.error("Failed to load organization tree");
        } finally {
            setLoading(false);
        }
    }, [message]);

    const loadChildren = useCallback(async (node: TreeNode) => {
        // Antd Tree loadData passes an EventDataNode which reliably has `key`,
        // while `id` may be missing depending on how the node was created.
        const nodeId = Number((node as any)?.id ?? (node as any)?.key);
        if (!Number.isFinite(nodeId)) return;
        if ((node as any)?.isLoaded) return;
        try {
            const res = await departmentApi.getChildren(nodeId);
            let children = res.data.map(mapNode);

            // Fallback: backend /children may return empty while hasChildren=true.
            // In that case, use descendants and infer direct children by parentId.
            if (children.length === 0 && (node as any)?.hasChildren) {
                const resDesc = await departmentApi.getDescendants(nodeId, 1, 100);
                const all = resDesc.data.items.map(mapNode);
                children = all.filter((x: TreeNode) => (x.parentId ?? null) === nodeId);
            }

            setTreeData(prev =>
                updateNodeInTree(prev, nodeId, n => ({
                    ...n,
                    children: children.length ? children : undefined,
                    isLeaf: children.length === 0,
                    isLoaded: true,
                }))
            );
        } catch {
            message.error("Failed to load child departments");
        }
    }, [message]);

    const move = useCallback(async (id: number, newParentId: number | null) => {
        await departmentApi.move(id, newParentId);
        setTreeData(prev => {
            const findNode = (nodes: TreeNode[]): TreeNode | null => {
                for (const n of nodes) {
                    if (n.id === id) return n;
                    if (n.children) {
                        const found = findNode(n.children);
                        if (found) return found;
                    }
                }
                return null;
            };
            const nodeToMove = findNode(prev);

            if (!nodeToMove) return prev;
            if (nodeToMove.parentId === newParentId) return prev;

            const updatedNode = { ...(nodeToMove as TreeNode), parentId: newParentId } as TreeNode;

            const removeFn = (nodes: TreeNode[]): TreeNode[] => {
                return nodes.filter(n => n.id !== id).map(n => {
                    if (n.children) {
                        const newChildren = removeFn(n.children);
                        return {
                            ...n,
                            children: newChildren.length ? newChildren : undefined,
                            isLeaf: newChildren.length === 0,
                            hasChildren: newChildren.length > 0
                        };
                    }
                    return n;
                });
            };
            let newTree = removeFn(prev);

            if (newParentId === null) {
                newTree = [...newTree, updatedNode];
            } else {
                newTree = updateNodeInTree(newTree, newParentId, n => {
                    if (n.children || n.isLoaded) {
                        return { ...n, children: [...(n.children || []), updatedNode], isLeaf: false, hasChildren: true };
                    } else {
                        return { ...n, isLeaf: false, hasChildren: true };
                    }
                });
            }
            return newTree;
        });
    }, []);

    const addNodeLocal = useCallback((parentId: number | null, newNodeDto: DepartmentDto) => {
        const newNode = mapNode(newNodeDto);
        setTreeData(prev => {
            if (parentId === null) {
                return [...prev, newNode];
            }
            return updateNodeInTree(prev, parentId, n => {
                if (n.children || n.isLoaded) {
                    return { ...n, children: [...(n.children || []), newNode], isLeaf: false, hasChildren: true };
                } else {
                    return { ...n, isLeaf: false, hasChildren: true };
                }
            });
        });
    }, []);

    const updateNodeLocal = useCallback((id: number, data: Partial<DepartmentDto>) => {
        setTreeData(prev => updateNodeInTree(prev, id, n => {
            const hasChildren = data.hasChildren !== undefined && data.hasChildren !== null ? data.hasChildren : n.hasChildren;
            return { ...n, ...data, hasChildren } as TreeNode;
        }));
    }, []);

    const removeNodeLocal = useCallback((id: number) => {
        const removeFn = (nodes: TreeNode[]): TreeNode[] => {
            return nodes.filter(n => n.id !== id).map(n => {
                if (n.children) {
                    const newChildren = removeFn(n.children);
                    return {
                        ...n,
                        children: newChildren.length ? newChildren : undefined,
                        isLeaf: newChildren.length === 0,
                        hasChildren: newChildren.length > 0
                    };
                }
                return n;
            });
        };
        setTreeData(prev => removeFn(prev));
    }, []);

    return { treeData, loading, loadRoots, loadChildren, move, addNodeLocal, updateNodeLocal, removeNodeLocal };
};
