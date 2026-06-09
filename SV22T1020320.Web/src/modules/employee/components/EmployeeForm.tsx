import React from "react";
import dayjs, { Dayjs } from "dayjs";
import {
    Button,
    DatePicker,
    Form,
    Input,
    InputNumber,
    Modal,
    Select,
    Space,
} from "antd";
import { EmployeeDto, EmployeeStatus, GenderType } from "../types";

export interface DepartmentOption {
    value: number;
    label: string;
}

export interface EmployeeFormValues {
    employeeCode: string;
    fullName: string;
    email: string;
    gender: GenderType;
    phone?: string;
    position?: string;
    baseSalary: number;
    departmentId?: number;
    joinDate?: Dayjs;
    birthDate?: Dayjs;
    status?: EmployeeStatus;
}

interface Props {
    open: boolean;
    isEdit: boolean;
    submitting: boolean;
    initialData: EmployeeDto | null;
    onCancel: () => void;
    onSubmit: (values: EmployeeFormValues) => Promise<void>;
}

const statusOptions = [
    { value: EmployeeStatus.Active, label: "Active" },
    { value: EmployeeStatus.Probation, label: "Probation" },
    { value: EmployeeStatus.OnLeave, label: "On Leave" },
    { value: EmployeeStatus.Resigned, label: "Resigned" },
    { value: EmployeeStatus.Terminated, label: "Terminated" },
    { value: EmployeeStatus.Retired, label: "Retired" },
    { value: EmployeeStatus.Deceased, label: "Deceased" },
];

const genderOptions = [
    { value: GenderType.Other, label: "Other" },
    { value: GenderType.Male, label: "Male" },
    { value: GenderType.Female, label: "Female" },
];

const normalizeStatus = (status: string): EmployeeStatus => {
    switch (status) {
        case "Active":
            return EmployeeStatus.Active;

        case "Probation":
            return EmployeeStatus.Probation;

        case "OnLeave":
            return EmployeeStatus.OnLeave;

        case "Resigned":
            return EmployeeStatus.Resigned;

        case "Terminated":
            return EmployeeStatus.Terminated;

        case "Retired":
            return EmployeeStatus.Retired;

        case "Deceased":
            return EmployeeStatus.Deceased;

        default:
            return EmployeeStatus.Active; // hoặc giá trị mặc định phù hợp
    }
};

const normalizeGender = (gender: string): GenderType => {
    switch (gender) {
        case "Male":
            return GenderType.Male;
        case "Female":
            return GenderType.Female;
        default:
            return GenderType.Other;
    }
};

export const EmployeeForm = ({
    open,
    isEdit,
    submitting,
    initialData,
    onCancel,
    onSubmit,
}: Props) => {
    const [form] = Form.useForm<EmployeeFormValues>();

    React.useEffect(() => {
        if (!open) return;

        if (initialData) {
            form.setFieldsValue({
                employeeCode: initialData.employeeCode,
                fullName: initialData.fullName,
                email: initialData.email,
                gender: normalizeGender(initialData.gender),
                phone: initialData.phone ?? undefined,
                position: initialData.position ?? undefined,
                baseSalary: initialData.baseSalary,
                departmentId: initialData.departmentId ?? undefined,
                joinDate: initialData.joinDate ? dayjs(initialData.joinDate) : undefined,
                birthDate: initialData.birthDate ? dayjs(initialData.birthDate) : undefined,
                status: normalizeStatus(initialData.status),
            });
            return;
        }

        form.resetFields();
        form.setFieldsValue({
            gender: GenderType.Other,
            baseSalary: 0,
            status: EmployeeStatus.Active,
        });
    }, [form, initialData, open]);

    const handleOk = async () => {
        const values = await form.validateFields();
        await onSubmit(values);
    };

    return (
        <Modal
            title={isEdit ? "Update employee" : "Create employee"}
            open={open}
            onCancel={onCancel}
            destroyOnHidden
            style={{ top: 20 }}
            footer={[
                <Button key="cancel" onClick={onCancel}>
                    Cancel
                </Button>,
                <Button key="submit" type="primary" loading={submitting} onClick={handleOk}>
                    {isEdit ? "Save changes" : "Create"}
                </Button>,
            ]}
        >
            <Form form={form} layout="vertical">
                <Space style={{ width: "100%" }} size={12}>
                    <Form.Item
                        name="employeeCode"
                        label="Employee code"
                        rules={[{ required: true, message: "Employee code is required" }]}
                        style={{ flex: 1 }}
                    >
                        <Input placeholder="NV1001" />
                    </Form.Item>
                    <Form.Item
                        name="fullName"
                        label="Full name"
                        rules={[{ required: true, message: "Full name is required" }]}
                        style={{ flex: 1 }}
                    >
                        <Input placeholder="Nguyen Van A" />
                    </Form.Item>
                </Space>

                <Space style={{ width: "100%" }} size={12}>
                    <Form.Item
                        name="email"
                        label="Email"
                        rules={[
                            { required: true, message: "Email is required" },
                            { type: "email", message: "Invalid email format" },
                        ]}
                        style={{ flex: 1 }}
                    >
                        <Input placeholder="employee@company.com" />
                    </Form.Item>
                    <Form.Item
                        name="phone"
                        label="Phone"
                        style={{ flex: 1 }}
                    >
                        <Input placeholder="09xxxxxxxx" />
                    </Form.Item>
                </Space>

                <Space style={{ width: "100%" }} size={12}>
                    <Form.Item
                        name="gender"
                        label="Gender"
                        rules={[{ required: true, message: "Gender is required" }]}
                        style={{ flex: 1 }}
                    >
                        <Select options={genderOptions} />
                    </Form.Item>
                    <Form.Item
                        name="status"
                        label="Status"
                        style={{ flex: 1 }}
                    >
                        <Select options={statusOptions} disabled={!isEdit} />
                    </Form.Item>
                </Space>

                <Space style={{ width: "100%" }} size={12}>
                    <Form.Item name="position" label="Position" style={{ flex: 1 }}>
                        <Input placeholder="Senior Developer" />
                    </Form.Item>
                    <Form.Item
                        name="baseSalary"
                        label="Base salary"
                        rules={[{ required: true, message: "Base salary is required" }]}
                        style={{ flex: 1 }}
                    >
                        <InputNumber style={{ width: "100%" }} min={0} />
                    </Form.Item>
                </Space>

                <Space style={{ width: "100%" }} size={12}>
                    <Form.Item name="joinDate" label="Join date" style={{ flex: 1 }}>
                        <DatePicker style={{ width: "100%" }} />
                    </Form.Item>
                </Space>

                <Form.Item name="birthDate" label="Birth date">
                    <DatePicker style={{ width: "100%" }} />
                </Form.Item>
            </Form>
        </Modal>
    );
};