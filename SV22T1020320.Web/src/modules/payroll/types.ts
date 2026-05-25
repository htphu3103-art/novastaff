// src/modules/payroll/types.ts

// ============================================================
// ENUMS & VALUE OBJECTS (Mapped from C#)
// ============================================================

export enum PayrollStatus {
    Unknown = 0,
    Draft = 1,
    Calculated = 2,
    Approved = 3,
    Paid = 4
}

export interface BonusAllowanceItem {
    name: string;
    amount: number;
    note?: string; // Mapped from C# note property
}

export interface DeductionItem {
    name: string;
    amount: number;
    note?: string; // Mapped from C# note property
}

export interface UpdatePayrollAdjustmentsRequest {
    bonusAndAllowances: BonusAllowanceItem[];
    deductions: DeductionItem[];
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}


// ============================================================
// PERIOD
// ============================================================

export interface PayrollPeriodSummaryDto {
    periodID: number;
    month: number;
    year: number;
    startDate: string; // ISO string
    endDate: string; // ISO string
    status: PayrollStatus;
    totalEmployees: number;
    totalNetSalary: number;
}

export interface PayrollPeriodDetailDto {
    periodID: number;
    month: number;
    year: number;
    startDate: string;
    endDate: string;
    status: PayrollStatus;
    details: PayrollDetailDto[];
}

export interface CreatePayrollPeriodRequest {
    month: number;
    year: number;
    startDate: string;
    endDate: string;
}

export interface AdvancePeriodStatusRequest {
    periodID: number;
    targetStatus: PayrollStatus;
}

// ============================================================
// DETAIL (PAYSLIP)
// ============================================================

export interface PayrollDetailDto {
    detailID: number;
    periodID: number;
    employeeID: number | null;
    employeeCode: string;
    fullName: string;
    departmentName: string | null;
    baseSalarySnapshot: number;
    actualWorkDays: number;
    bonusAndAllowances: BonusAllowanceItem[];
    deductions: DeductionItem[];
    totalIncome: number;
    netSalary: number;
    status: PayrollStatus;
    paidDate: string | null;
}

export interface CalculatePayrollDetailRequest {
    periodID: number;
    employeeID: number;
}

export interface BatchCalculateRequest {
    periodID: number;
    departmentID: number | null;
}

export interface BatchCalculateResult {
    periodID: number;
    successCount: number;
    skippedCount: number;
    errors: string[];
}

// ============================================================
// UI TEMPORARY TYPES (To be removed after refactoring UI)
// ============================================================

export interface IPayrollItem {
    id: number | string;
    code: string;
    name: string;
    baseSalary: number;
    workDays: number;
    bonus: number;
    deduction: number;
    finalSalary: number;
    status: 'paid' | 'pending';
}

export interface IPayrollStats {
    totalBudget: number;
    paidCount: number;
    totalStaff: number;
    averageSalary: number;
    trend: number;
}