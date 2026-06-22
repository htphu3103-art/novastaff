import { axiosClient as api } from "../../../utils/axiosClient";
import {
    PayrollPeriodSummaryDto,
    PayrollPeriodDetailDto,
    CreatePayrollPeriodRequest,
    AdvancePeriodStatusRequest,
    PayrollDetailDto,
    BatchCalculateRequest,
    BatchCalculateResult,
    UpdatePayrollAdjustmentsRequest,
    PagedResult
} from "../types";

export const BASE_URL = "payroll";

export const payrollApi = {
    // =========================================================
    // PERIOD
    // =========================================================

    getPeriods: (pageIndex: number = 1, pageSize: number = 12) =>
        api.get<PagedResult<PayrollPeriodSummaryDto>>(`/${BASE_URL}/periods`, {
            params: { pageIndex, pageSize }
        }),

    getActivePeriod: () =>
        api.get<PayrollPeriodSummaryDto | null>(`/${BASE_URL}/periods/active`),

    getPeriodDetail: (periodId: number, departmentId?: number) =>
        api.get<PayrollPeriodDetailDto>(`/${BASE_URL}/periods/${periodId}`, {
            params: { departmentId }
        }),

    createPeriod: (request: CreatePayrollPeriodRequest) =>
        api.post<PayrollPeriodDetailDto>(`/${BASE_URL}/periods`, request),

    advancePeriodStatus: (periodId: number, request: AdvancePeriodStatusRequest) =>
        api.put(`/${BASE_URL}/periods/${periodId}/advance`, request),

    getStatusSummary: (periodId: number) =>
        api.get<any>(`/${BASE_URL}/periods/${periodId}/summary`),

    getTotalNetSalary: (periodId: number) =>
        api.get<{ periodId: number; totalNetSalary: number }>(`/${BASE_URL}/periods/${periodId}/total`),

    // =========================================================
    // DETAIL (PAYSLIP)
    // =========================================================

    getPayslip: (periodId: number, employeeId: number) =>
        api.get<PayrollDetailDto>(`/${BASE_URL}/periods/${periodId}/employees/${employeeId}/payslip`),

    calculatePayslip: (periodId: number, employeeId: number) =>
        api.post<PayrollDetailDto>(`/${BASE_URL}/periods/${periodId}/employees/${employeeId}/calculate`),

    batchCalculate: (periodId: number, request: BatchCalculateRequest) =>
        api.post<BatchCalculateResult>(`/${BASE_URL}/periods/${periodId}/calculate-batch`, request),

    updateAdjustments: (periodId: number, employeeId: number, request: UpdatePayrollAdjustmentsRequest) =>
        api.put<PayrollDetailDto>(`/${BASE_URL}/periods/${periodId}/employees/${employeeId}/adjustments`, request),
};
