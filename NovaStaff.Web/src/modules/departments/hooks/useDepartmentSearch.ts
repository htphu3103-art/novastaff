import { useMemo, useRef, useState, useCallback } from "react";
import { App } from "antd";
import { TreeNode } from "../types";
import { departmentApi } from "../api/departmentApi";
import { mapNode } from "./useDepartmentTree";

const PAGE_SIZE = 10;
const TREE_PAGE_SIZE = 100;
const ROOTS_PAGE_SIZE = 50;
const ROOTS_SEARCH_MIN_CHARS = 3;
const ROOTS_SEARCH_CONCURRENCY = 6;
const ROOTS_SEARCH_PAGE_SIZE_PER_ROOT = 100;

const buildTreeFromFlatList = (nodes: TreeNode[], rootId?: number): TreeNode[] => {
    const byId = new Map<number, TreeNode>();
    for (const n of nodes) byId.set(n.id, { ...n, children: undefined });

    const roots: TreeNode[] = [];
    for (const n of nodes) {
        const current = byId.get(n.id)!;
        const parentId = current.parentId ?? null;
        const parent = parentId != null ? byId.get(parentId) : undefined;

        // If we are building inside a scope root, treat nodes whose parent is rootId as roots (children of scope)
        if (rootId != null && parentId === rootId) {
            roots.push(current);
            continue;
        }

        if (!parent) {
            if (rootId == null) roots.push(current);
        } else {
            if (!parent.children) parent.children = [];
            parent.children.push(current);
        }
    }

    const sortRec = (arr: TreeNode[]) => {
        arr.sort((a, b) => a.name.localeCompare(b.name));
        for (const x of arr) if (x.children?.length) sortRec(x.children);
    };
    sortRec(roots);
    return roots;
};

export const useDepartmentSearch = () => {
    const { message } = App.useApp();
    const [keyword, setKeyword] = useState("");
    const [results, setResults] = useState<TreeNode[]>([]);
    const [treeResults, setTreeResults] = useState<TreeNode[]>([]);
    const [loading, setLoading] = useState(false);
    const [page, setPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);

    const inflightAbortRef = useRef<AbortController | null>(null);
    const cacheRef = useRef<Map<string, { items: TreeNode[]; totalCount: number }>>(new Map());
    const nodeByIdCacheRef = useRef<Map<number, TreeNode>>(new Map());

    const getNodeByIdCached = useCallback(async (id: number): Promise<TreeNode> => {
        const cached = nodeByIdCacheRef.current.get(id);
        if (cached) return cached;
        const res = await departmentApi.getById(id);
        const node = mapNode(res.data);
        nodeByIdCacheRef.current.set(id, node);
        return node;
    }, []);

    const ensureAncestors = useCallback(async (nodes: TreeNode[], stopAtParentId: number | null, signal?: AbortSignal) => {
        const merged = new Map<number, TreeNode>();
        for (const n of nodes) merged.set(n.id, n);

        const queue: number[] = [];
        for (const n of nodes) {
            const pid = n.parentId ?? null;
            if (pid != null && !merged.has(pid) && pid !== stopAtParentId) queue.push(pid);
        }

        while (queue.length) {
            if (signal?.aborted) break;
            const id = queue.pop()!;
            if (merged.has(id)) continue;

            try {
                const parent = await getNodeByIdCached(id);
                merged.set(parent.id, parent);
                const nextPid = parent.parentId ?? null;
                if (nextPid != null && !merged.has(nextPid) && nextPid !== stopAtParentId) queue.push(nextPid);
            } catch {
                // ignore missing nodes; still render partial tree
            }
        }

        return Array.from(merged.values());
    }, [getNodeByIdCached]);

    const searchAllRoots = useCallback(async (q: string, p: number, signal: AbortSignal) => {
        // Load roots then query each root subtree in parallel (with a concurrency limit).
        const rootsRes = await departmentApi.getRootsPaged(1, ROOTS_PAGE_SIZE);
        const rootIds = rootsRes.data.items
            .map(x => x.id)
            .filter((x): x is number => Number.isFinite(x));

        const allMatches: TreeNode[] = [];

        let cursor = 0;
        const worker = async () => {
            while (cursor < rootIds.length && !signal.aborted) {
                const rid = rootIds[cursor++];
                try {
                    const resTree = await departmentApi.searchInSubtree(
                        rid,
                        q,
                        1,
                        ROOTS_SEARCH_PAGE_SIZE_PER_ROOT,
                        signal
                    );
                    allMatches.push(...resTree.data.items.map(mapNode));
                } catch (err) {
                    const anyErr = err as any;
                    if (anyErr?.name !== "CanceledError" && anyErr?.code !== "ERR_CANCELED") {
                        // ignore & continue other roots
                    }
                }
            }
        };

        const workers = Array.from({ length: Math.min(ROOTS_SEARCH_CONCURRENCY, rootIds.length) }, worker);
        await Promise.all(workers);

        const byId = new Map<number, TreeNode>();
        for (const n of allMatches) byId.set(n.id, n);
        const mergedMatches = Array.from(byId.values());

        // Expand with ancestors up to true roots
        const withAncestors = await ensureAncestors(mergedMatches, null, signal);
        const tree = buildTreeFromFlatList(withAncestors);

        // Right panel paging is done client-side for the merged set
        const sorted = [...mergedMatches].sort((a, b) => a.name.localeCompare(b.name));
        const start = (p - 1) * PAGE_SIZE;
        const pageItems = sorted.slice(start, start + PAGE_SIZE);

        return {
            tableItems: pageItems,
            totalCount: mergedMatches.length,
            treeItems: tree,
        };
    }, [ensureAncestors]);

    const search = useCallback(async (scopeId: number | null, q: string, p = 1) => {
        const trimmed = q.trim();
        setKeyword(trimmed);
        setPage(p);

        if (!trimmed) {
            setResults([]);
            setTreeResults([]);
            setTotalCount(0);
            return;
        }

        // UX: search only when keyword is long enough (reduce lag/spam)
        if (scopeId == null ? trimmed.length < ROOTS_SEARCH_MIN_CHARS : trimmed.length < 2) {
            setResults([]);
            setTreeResults([]);
            setTotalCount(0);
            return;
        }

        const cacheKey = `${scopeId ?? "ALL"}|${trimmed}|${p}|${PAGE_SIZE}`;
        const cached = cacheRef.current.get(cacheKey);
        if (cached) {
            setResults(cached.items);
            setTotalCount(cached.totalCount);
            return;
        }

        setLoading(true);
        try {
            // Cancel previous request
            inflightAbortRef.current?.abort();
            const ac = new AbortController();
            inflightAbortRef.current = ac;

            if (scopeId == null) {
                const all = await searchAllRoots(trimmed, p, ac.signal);
                setResults(all.tableItems);
                setTotalCount(all.totalCount);
                cacheRef.current.set(cacheKey, { items: all.tableItems, totalCount: all.totalCount });
                setTreeResults(all.treeItems);
                return;
            }

            // Table: small server-side paging
            const res = await departmentApi.searchInSubtree(scopeId, trimmed, p, PAGE_SIZE, ac.signal);
            const tableItems = res.data.items.map(mapNode);
            setResults(tableItems);
            setTotalCount(res.data.totalCount);
            cacheRef.current.set(cacheKey, { items: tableItems, totalCount: res.data.totalCount });

            // Tree: fetch more items to build tree + ancestors and avoid dropped branches
            const treeKey = `${scopeId}|${trimmed}|tree|1|${TREE_PAGE_SIZE}`;
            const cachedTree = cacheRef.current.get(treeKey);
            let rawTreeItems: TreeNode[];
            let scopeNode: TreeNode | null = null;
            if (cachedTree) {
                rawTreeItems = cachedTree.items;
            } else {
                const resTree = await departmentApi.searchInSubtree(scopeId, trimmed, 1, TREE_PAGE_SIZE, ac.signal);
                rawTreeItems = resTree.data.items.map(mapNode);
                cacheRef.current.set(treeKey, { items: rawTreeItems, totalCount: resTree.data.totalCount });
            }

            try {
                scopeNode = await getNodeByIdCached(scopeId);
            } catch {
                scopeNode = null;
            }

            const withAncestors = await ensureAncestors(rawTreeItems, scopeId, ac.signal);
            const scopedChildren = buildTreeFromFlatList(withAncestors, scopeId);
            setTreeResults(scopeNode ? [{ ...scopeNode, children: scopedChildren, isLoaded: true }] : scopedChildren);
        } catch (err) {
            // Ignore abort/cancel
            const anyErr = err as any;
            if (anyErr?.name !== "CanceledError" && anyErr?.code !== "ERR_CANCELED") {
                message.error("Search failed");
            }
        } finally {
            setLoading(false);
        }
    }, [ensureAncestors, getNodeByIdCached, message, searchAllRoots]);

    const changePage = useCallback((scopeId: number | null, p: number) => {
        search(scopeId, keyword, p);
    }, [keyword, search]);

    const clear = useCallback(() => {
        inflightAbortRef.current?.abort();
        setKeyword("");
        setResults([]);
        setTreeResults([]);
        setTotalCount(0);
        setPage(1);
    }, []);

    const expandedKeys = useMemo(() => {
        const keys: string[] = [];
        const walk = (arr: TreeNode[]) => {
            for (const n of arr) {
                if (n.children?.length) {
                    keys.push(String(n.id));
                    walk(n.children);
                }
            }
        };
        walk(treeResults);
        return keys;
    }, [treeResults]);

    return {
        keyword,
        results,
        treeResults,
        expandedKeys,
        loading,
        page,
        pageSize: PAGE_SIZE,
        totalCount,
        isSearching: !!keyword,
        search,
        changePage,
        clear,
    };
};