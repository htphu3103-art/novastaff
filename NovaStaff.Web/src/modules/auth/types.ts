export interface LoginRequest {
    username: string;
    password: string;
}

export interface AuthResponse {
    accessToken: string;
}

export enum UserRole {
    Admin = 1,
    Manager = 2,
    Staff = 3
}

export interface UserProfileDto {
    userId: number;
    username: string;
    displayName: string | null;
    role: UserRole;
}

export interface CreateUserRequest {
    username: string;
    password:  string;
    employeeId: number;
    role: UserRole;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}

export interface UpdateUserRoleRequest {
    role: UserRole;
}

export interface ActivateAccountRequest {
    token: string;
    newPassword: string;
    confirmPassword: string;
}