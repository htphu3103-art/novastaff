# payroll

**Route:** `/payroll` · **1 file** `PayrollPage.tsx` (admin vs staff UI inline)

| kind | path |
|------|------|
| api | `api/payrollApi.ts` · BASE `payroll` |
| types | `types.ts` · `PayrollStatus`: Draft·Calculated·Approved·Paid |

## Components
`PayrollStats` `PeriodListTable` `CreatePeriodModal` `PeriodDetailView` `PayrollTable` `PayslipModal` `AdjustmentModal`

## API `/payroll`
| op | path |
|----|------|
| GET | /periods · /periods/active · /periods/:id · /periods/:id/summary · /periods/:id/total |
| POST | /periods · /periods/:id/employees/:eid/calculate · /periods/:id/calculate-batch |
| PUT | /periods/:id/advance · /periods/:id/employees/:eid/adjustments |
| GET | /periods/:id/employees/:eid/payslip |

Admin: quản lý kỳ + batch calc · Staff: xem payslip cá nhân

## Open order
1. `payrollApi.ts` 2. `PayrollPage.tsx` hoặc component đích (PeriodDetailView…)
