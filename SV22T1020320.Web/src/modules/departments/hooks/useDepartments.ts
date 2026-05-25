import { useEffect, useState, useCallback } from "react";
import { DepartmentDto } from "../types";
import { departmentApi } from "../api/departmentApi";
import { message } from "antd";

export const useDepartments = () => {
    const [data, setData] = useState<DepartmentDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);

    const fetchRoots = useCallback(async () => {
        setLoading(true);
        try {
            const res = await departmentApi.getRootsPaged(1, 100);
            if (res.data && res.data.items) {
                setData(res.data.items);
                setTotalCount(res.data.totalCount);
            }
        } catch (err) {
            message.error("Lỗi: Không thể kết nối đến máy chủ!");
        } finally {
            setLoading(false);
        }
    }, []);

    const fetchChildren = async (id: number): Promise<DepartmentDto[]> => {
        try {
            // endpoint: /Departments/{id}/children
            const res = await departmentApi.getChildren(id);
            return res.data;
        } catch (error) {
            console.error("Lỗi gọi API con:", error);
            return [];
        }
    };

    const remove = async (id: number) => {
        try {
            await departmentApi.delete(id);
            message.success("Đã xóa phòng ban thành công");
            return true;
        } catch (error) {
            message.error("Xóa thất bại! Có thể do phòng ban này đang chứa nhân viên.");
            return false;
        }
    };

    useEffect(() => {
        fetchRoots();
    }, [fetchRoots]);

    return { data, totalCount, loading, fetchChildren, remove, refresh: fetchRoots };
};