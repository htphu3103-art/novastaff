# NovaStaff BE — AI Router

**Root:** `e:\my_projects\novastaff` · **API:** `NovaStaff.Admin` · **Onboarding người:** [`ARCHITECTURE.md`](ARCHITECTURE.md) — agent **không** đọc nguyên file đó.

**Chỉ đọc file này trước (task BE).** ≤2 file `.cs`/task · Semble khi thiếu chi tiết · không `Grep`/`Glob` toàn solution.

| Task | Mở (ưu tiên) |
|------|----------------|
| Auth, refresh cookie | `NovaStaff.Admin/Controllers/AuthController.cs` |
| User CRUD / role | `UserController.cs` + `NovaStaff.BusinessLayers/Services/*User*` |
| Department tree | `DepartmentService.cs` · `DataLayes/Repositories/DepartmentRepository.cs` |
| Employee | `EmployeeController.cs` · `EmployeeService.cs` |
| Attendance / leave | `AttendanceController.cs` · `LeaveRequestController.cs` |
| Payroll / task / chat | `Payrollcontroller.cs` · `WorkTaskController.cs` · `ChatController.cs` |
| Lỗi HTTP map | `NovaStaff.Admin/Web/Middlewares/GlobalExceptionMiddleware.cs` |

## Projects
| Folder | Vai trò |
|--------|---------|
| `NovaStaff.Admin` | Controllers, `Program.cs`, middleware |
| `NovaStaff.BusinessLayers` | Services (`namespace NovaStaff.Services`) |
| `NovaStaff.DataLayes` | DbContext, repos, `Configurations/`, `AuditInterceptor` *(typo Layes)* |
| `NovaStaff.Model` | Entities, DTOs, `Filters/`, exceptions |
| `NovaStaff.Infrastructure` | DI, infra |
| `Shared/` | `NovaStaff.Shared` cache/helpers |

## Layer (bắt buộc)
- **Controller:** mỏng; không try/catch nghiệp vụ → ném exception → `GlobalExceptionMiddleware`
- **Service:** 100% business; `IUnitOfWork`; `SaveChangesAsync` qua UoW; không DbContext trực tiếp
- **Repository:** chỉ Entity; **Filter** object (không EF delegate public); không trả DTO; không expose `IQueryable`
- **Delete:** hard delete + `AuditInterceptor` → `AuditLogs` (không soft-delete mặc định)

## Department (critical)
- Cây phòng ban: **OrgPath** (materialized path string), unique `IX_Departments_OrgPath` — **không** dùng `HierarchyId`
- Tạo/di chuyển: `ExecuteInTransactionAsync(..., IsolationLevel.Serializable)` + `GenerateNewNodeAsync`
- `OrgLevel` set trong property `OrgPath` của entity

## Semble
`repo: e:\my_projects\novastaff` · `top_k: 3` · ≤2× `search` + 1× `find_related`

| need | query |
|------|-------|
| HTTP errors | GlobalExceptionMiddleware HandleAsync |
| generic repo | GenericRepository GetByIdAsync Delete |
| dept path | GenerateNewNodeAsync OrgPath Serializable |

## Skip
`bin/`, `obj/`, migrations/, `*.http`, test output
