# Core

## Stack
React19·Vite8·TS·AntD6·router7·axios·@dnd-kit·SignalR·framer-motion·recharts · alias `@` → `src/`

## BE (dev)
`vite.config.ts` proxy `/api` `/chathub` → `localhost:8081`

## Roles (`modules/auth/types.ts`)
`Admin=1` `Manager=2` `Staff=3` · **isAdmin** = Admin|Manager

## Routes (`src/routes/index.tsx`)
| path | page |
|------|------|
| /login | auth/LoginPage |
| /activate | auth/ActivateAccountPage |
| / | dashboard/DashboardPage |
| /tasks | task/TaskPage |
| /attendance | attendance/AttendancePage |
| /payroll | payroll/PayrollPage |
| /chat | **chat/ChatPage** (not chat/pages/) |
| /departments | departments/DepartmentPage · ProtectedRoute Admin+Manager |
| * | Navigate → / |

Layout `layouts/MainLayout/MainLayout.tsx` · Guard `routes/ProtectedRoute.tsx`

## HTTP (`src/utils/axiosClient.ts`)
`baseURL=/api` · `withCredentials` · access `localStorage.token` · refresh POST `/auth/refresh` (HttpOnly cookie) · 401 queue → `auth:logout`

## Auth
| file | role |
|------|------|
| contexts/AuthContext.tsx | login·logout·hydrate |
| components/GlobalAuthInit.tsx | boot hydrate·logout listener |
| App.tsx | BrowserRouter·AuthProvider |

## Pattern
`Page` → `useAuth()` → Admin|Manager vs Staff subpage (task, attendance, dashboard)

## API
`modules/*/api/*Api.ts` hoặc `chat/services/*` · `import { axiosClient } from '.../utils/axiosClient'`

## Shared
`components/common/PageWrapper` `TableToolbar`

## Semble (repo FE)
| need | query |
|------|-------|
| routes | AppRoutes Route path |
| auth | AuthContext login token |
| http | axios interceptors refresh |
| realtime | SignalR chathub |
