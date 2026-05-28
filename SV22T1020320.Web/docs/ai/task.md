# task

**Route:** `/tasks`

| kind | path |
|------|------|
| router page | `modules/task/TaskPage.tsx` |
| admin | `admin/AdminTaskPage.tsx` · DnD create/edit/delete |
| staff | `employee/EmployeeTaskPage.tsx` · read/kanban |
| api | `api/taskApi.ts` |
| types | `types.ts` · status BE: `Todo` `InProgress` `Done` → UI keys `todo` `inprogress` `done` |
| kanban | `components/KanbanColumn` `DraggableTaskCard` `TaskCardBody` |
| store | `src/store/taskStore.ts` — **stub rỗng, chưa dùng** |

## API `/work-tasks`
| op | path |
|----|------|
| GET | / · /:id · /assignee/:eid · /overdue · /manager/:mid · /statistics |
| POST | / |
| PUT | /:id |
| PATCH | /:id/status · /:id/complete |
| DELETE | /:id |

Admin: `getByManager`+filter · drag → `changeStatus` · Staff: `getByAssignee`

## Open order
1. `taskApi.ts` 2. `TaskPage.tsx` 3. Admin* hoặc Employee* page
