# NovaStaff - Human Resource Management System

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-Vite-cyan.svg)

## Overview
NovaStaff is a comprehensive, enterprise-grade Human Resource Management System. This repository contains the official release (Version 1) of the product.

## Architecture
The system is built using a modern architecture separating the backend API and frontend SPA:
- **Backend**: .NET Web API (`NovaStaff.Api`), following Layered Architecture (Controllers, Services, Repositories).
- **Frontend**: React application built with Vite (`SV22T1020320.Web`).

### Key Modules
- **Authentication & Authorization**: Secure login, account activation, and role-based access control.
- **Employee & Department Management**: Hierarchical department tree (Materialized Path) and detailed employee profiles.
- **Attendance & Leave Requests**: Real-time tracking and leave approval workflows.
- **Payroll**: Automated salary and adjustment calculations.
- **Task Management**: Kanban boards for work tasks.
- **Real-time Chat**: Integrated REST and SignalR chat.

## Setup & Run

### Prerequisites
- .NET 8.0 SDK
- Node.js (v18+) & npm
- PostgreSQL (or Docker for `docker-compose`)
- Redis

### Backend Setup
1. Navigate to the root directory.
2. Start the database and Redis services using Docker (optional if running locally): `docker-compose up -d db redis`
3. Ensure connection strings are configured in `NovaStaff.Api/appsettings.json`.
4. Apply migrations: `dotnet ef database update --project NovaStaff.DataLayes --startup-project NovaStaff.Api`
5. Run the API: `dotnet run --project NovaStaff.Api`

### Frontend Setup
1. Navigate to the frontend directory: `cd SV22T1020320.Web`
2. Install dependencies: `npm install`
3. Start the dev server: `npm run dev`

## Release Notes
- **Version 1.0.0**: Official Report Release
