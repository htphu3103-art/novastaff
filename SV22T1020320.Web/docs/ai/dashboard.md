# dashboard

**Route:** `/`

| kind | path |
|------|------|
| page | `modules/dashboard/DashboardPage.tsx` |
| admin UI | `components/AdminView.tsx` |
| staff UI | `components/EmployeeView.tsx` |
| widget | `components/StatCard.tsx` |

**Split:** `isAdmin` (Admin|Manager) → AdminView else EmployeeView · không API riêng

## Open order
1. `DashboardPage.tsx` 2. AdminView hoặc EmployeeView
