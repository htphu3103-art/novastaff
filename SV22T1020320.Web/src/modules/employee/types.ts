// ============================================================
// 📁 ENUMS (Đã cập nhật khớp chính xác với C#)
// ============================================================

export enum GenderType {
    Other = 0,
    Male = 1,
    Female = 2
}

export enum EmployeeStatus {
    Active = 1,
    Probation = 2,
    OnLeave = 3,
    Resigned = 4,
    Terminated = 5,
    Retired = 6,
    Deceased = 7,
}

export enum EmployeeSortField {
    FullName = "FullName",
    EmployeeCode = "EmployeeCode",
    JoinDate = "JoinDate",
    BaseSalary = "BaseSalary",
    Department = "Department"
}

// ============================================================
// 📄 DTOs (Data Transfer Objects)
// ============================================================

export interface EmployeeDto {
    id: number;
    employeeCode: string;
    fullName: string;
    gender: string;           // "Male", "Female", "Other" (ToString từ Backend)
    birthDate: string | null;  // ISO string
    email: string;
    phone: string | null;
    address: string | null;
    position: string | null;
    jobLevel: number | null;
    baseSalary: number;
    joinDate: string | null;   // ISO string
    contractType: string | null;
    status: string;            // "Active", "Resigned"... (ToString từ Backend)
    departmentId: number | null;
    departmentName: string | null;
    supervisorId: number | null;
    supervisorName: string | null;
}

export interface EmployeeManagerDto {
    employeeID: number;
    employeeCode: string;
    fullName: string;
    position: string | null;
    departmentId: number | null;
    departmentName: string | null;
    email: string;
    phone: string | null;
}


// ============================================================
// 📥 REQUESTS
// ============================================================

export interface CreateEmployeeRequest {
    employeeCode: string;
    fullName: string;
    gender: GenderType;        // Gửi số 0, 1, 2
    birthDate?: string | null;
    email: string;
    phone?: string | null;
    address?: string | null;
    departmentId?: number | null;
    supervisorId?: number | null;
    position?: string | null;
    jobLevel?: number | null;
    baseSalary: number;
    joinDate?: string | null;
    contractType?: string | null;
}

export interface UpdateEmployeeRequest extends CreateEmployeeRequest {
    status: EmployeeStatus;    // Gửi số 0, 1, 2, 3, 4
}

export interface TransferDepartmentRequest {
    newDepartmentId: number;
}

// ============================================================
// 🔍 FILTERS & PAGINATION
// ============================================================

export interface EmployeeFilter {
    nameContains?: string;
    codeContains?: string;
    departmentId?: number;
    supervisorId?: number;
    status?: EmployeeStatus;
    gender?: GenderType;
    contractType?: string;
    sortBy?: EmployeeSortField;
    sortDescending?: boolean;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
}