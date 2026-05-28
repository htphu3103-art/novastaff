# attendance

**Route:** `/attendance`

| kind | path |
|------|------|
| router | `AttendancePage.tsx` |
| admin | `admin/AdminAttendancePage.tsx` |
| staff | `employee/EmployeeAttendancePage.tsx` |
| api | `api/attendanceApi.ts` `api/leaveRequestApi.ts` |
| types | `types.ts` |

## Components (shared)
`AttendanceStats` `AttendanceTable` `CheckInCard` `AttendanceFormModal` `AttendanceDetailsDrawer`  
`LeaveRequestModal` `LeaveRequestTable`

## API `/Attendance` (PascalCase paths)
| op | path |
|----|------|
| GET | /today/:eid · /today · /employee/:eid · / · /:id · /me/total-hours |
| POST | /check-in · /check-out · /check-in/:eid · /check-out/:eid · / (manual) |
| PUT | /:id |
| DELETE | /:id |

Staff: `checkInSelf` `checkOutSelf` · Admin: `getPaged` HR tabs + leave approve

## API `leave-requests`
| op | path |
|----|------|
| GET | /me · /employee/:eid · /pending · /employee/:eid/approved-days |
| POST | / · /:id/approve · /:id/reject |

## Open order
1. `*Api.ts` 2. `AttendancePage` 3. Admin|Employee page
