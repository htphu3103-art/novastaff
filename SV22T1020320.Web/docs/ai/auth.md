# auth

**Route:** `/login` `/activate?token=` (public)

| kind | path |
|------|------|
| page | `modules/auth/LoginPage.tsx` `ActivateAccountPage.tsx` |
| api | `modules/auth/api/authApi.ts` |
| types | `modules/auth/types.ts` |

## API (`/api` prefix)
| method | path |
|--------|------|
| POST | /auth/login /auth/logout /auth/refresh /auth/activate |
| GET | /users/me |
| POST | /users/change-password |
| POST | /users |
| GET | /users/:id |
| PUT | /users/:id/role /lock /unlock |
| POST | /users/:id/reset-password |

## Flow
Login → `authApi.login` → `localStorage.token` → `getCurrentUser` → `AuthContext.login`  
Activate → `?token` → `authApi.activateAccount` → `/login`

## Open order
1. `authApi.ts` 2. page 3. `AuthContext.tsx` (nếu session)
