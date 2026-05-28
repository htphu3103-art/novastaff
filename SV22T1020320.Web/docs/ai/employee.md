# employee

**Không có route.** `EmployeePage.tsx` tồn tại nhưng **chưa** gắn `routes/index.tsx`.

| kind | path |
|------|------|
| api | `modules/employee/api/employeeApi.ts` |
| types | `modules/employee/types.ts` |
| ui | `components/EmployeeTable.tsx` `EmployeeForm.tsx` |
| host UI | `departments/DepartmentPage.tsx` (chính) |

## API `/employees`
| op | path |
|----|------|
| GET | / · /:id · /code/:code · /department/:deptId · /:id/subordinates · /managers |
| POST | / |
| PUT | /:id |
| DELETE | /:id |
| PUT | /:id/transfer |

## Open order
1. `employeeApi.ts` 2. `DepartmentPage` hoặc `EmployeeForm`/`EmployeeTable`
