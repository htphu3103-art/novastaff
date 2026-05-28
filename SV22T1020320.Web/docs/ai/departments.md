# departments (+ employee UI)

**Route:** `/departments` · **ProtectedRoute** Admin+Manager only

| kind | path |
|------|------|
| page | `DepartmentPage.tsx` — tree + table + **embedded employee CRUD** |
| api | `api/departmentApi.ts` |
| types | `types.ts` |
| hooks | `useDepartmentTree` `useDepartmentBrowser` `useDepartmentSearch` `useDepartments` |
| ui | `DepartmentTree` `DepartmentForm` `DepartmentTable` `DepartmentStats` |

**Employee UI trong page:** `../employee/components/EmployeeTable` `EmployeeForm` · `employeeApi` `authApi`

## API `/Departments` (PascalCase) · BE tree field: **OrgPath**
| op | path |
|----|------|
| GET | / · /:id · /:id/children · /:id/descendants |
| POST | / |
| PUT | /:id |
| DELETE | /:id |

`searchInSubtree` = GET `/:id/descendants?NameContains=`

## Open order
1. `departmentApi.ts` 2. `DepartmentPage.tsx` 3. hook hoặc component  
**NV thuần:** xem shard [`employee.md`](employee.md) nhưng UI thường ở đây
