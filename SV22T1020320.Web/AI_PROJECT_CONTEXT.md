 # AI Project Context Map

## A. Feature Map
* **attendance** → tracks employee check-ins, check-outs, and leave requests
* **auth** → handles login, account activation, token management, and authentication flow
* **chat** → manages real-time messaging or communication interfaces
* **dashboard** → main landing view, aggregate metrics, and overview statistics
* **departments** → manages organizational hierarchy, department data, and manager assignments
* **employee** → handles employee directories, profiles, and basic user management
* **payroll** → processes salaries, pay periods, and financial distributions
* **task** → kanban-style task management, workflow tracking, and assignment handling

## B. Global Infrastructure Map
* **routing** → `src/routes/` (core file: `index.tsx`)
* **auth state** → `src/contexts/` (core file: `AuthContext.tsx`)
* **api base** → `src/utils/` (core file: `axiosClient.ts`)
* **global store** → `src/store/` (core file: `taskStore.ts`)

## C. File Access Strategy
**RULES FOR FUTURE AGENT RUNS:**
1. Never scan the full project again.
2. Always consult this file first before determining which files to access.
3. Always jump directly to the predicted module based on the Feature Map.
4. Only open a maximum of 3 files per task.
5. If unsure about a file's location, refine the guess *inside the same module*. DO NOT initiate a new project-wide scan.

## D. Confidence Note
*This context is approximate and based on an initial scan. It should be used for routing, not deep reasoning.*
