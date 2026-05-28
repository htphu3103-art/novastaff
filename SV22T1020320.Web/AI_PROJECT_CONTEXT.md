# NovaStaff Web — AI Router

**FE root:** `e:\my_projects\novastaff\SV22T1020320.Web` · **src:** `...\src\` · **shards:** `...\docs\ai\` · **BE:** [`..\AI_BE_CONTEXT.md`](../AI_BE_CONTEXT.md)

**Chỉ đọc file này trước (task FE).** 1 shard `docs/ai/` + ≤2 file `src/`. Không `Glob`/`Grep` toàn `src/`.

| Nhiệm vụ | Shard |
|----------|-------|
| Route, auth, axios, layout | [`docs/ai/00-core.md`](docs/ai/00-core.md) |
| Login, activate, users | [`docs/ai/auth.md`](docs/ai/auth.md) |
| Trang chủ, thống kê | [`docs/ai/dashboard.md`](docs/ai/dashboard.md) |
| Kanban, work-tasks | [`docs/ai/task.md`](docs/ai/task.md) |
| Chấm công, đơn nghỉ | [`docs/ai/attendance.md`](docs/ai/attendance.md) |
| Kỳ lương, payslip | [`docs/ai/payroll.md`](docs/ai/payroll.md) |
| Phòng ban (+ NV trong page) | [`docs/ai/departments.md`](docs/ai/departments.md) |
| API nhân viên (không route) | [`docs/ai/employee.md`](docs/ai/employee.md) |
| Chat REST + SignalR | [`docs/ai/chat.md`](docs/ai/chat.md) |

**Semble:** `top_k: 3` · ≤2× `search` + 1× `find_related` · `repo` FE `...\SV22T1020320.Web` · BE `e:\my_projects\novastaff`

**Rules:** ≤2 `src/`/task · NV → shard `departments` trước · ignore `src/modules/chat/pages/ChatPage.tsx` · skip `node_modules` `dist` `package-lock.json` · file >400 dòng: `Read` offset/limit hoặc Semble line

**Cursor (bạn):** Rules gọn, trùng router → xóa · MCP: Semble on · task mới → chat mới
