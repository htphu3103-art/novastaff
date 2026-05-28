# NovaStaff — Architecture Decision Record (ADR)

> **AI agents:** Đọc [`AI_BE_CONTEXT.md`](AI_BE_CONTEXT.md) (BE) hoặc [`SV22T1020320.Web/AI_PROJECT_CONTEXT.md`](SV22T1020320.Web/AI_PROJECT_CONTEXT.md) (FE) — **không** nạp nguyên file ADR (~150 dòng). Dùng Semble `top_k: 3` khi cần chi tiết code.
> **Onboarding người:** File này là bộ nhớ dài hạn team. Cập nhật khi có quyết định kiến trúc mới (đồng bộ router AI tương ứng).

---

## 1. Technology Stack

| Layer | Technology | Chú thích |
|---|---|---|
| **Backend** | ASP.NET Core, C# | RESTful API, Global Exception Handling |
| **ORM** | Entity Framework Core | Code-first, Configurations mapping riêng biệt |
| **Database** | SQL Server | Phòng ban: **OrgPath** materialized path (string), unique index |
| **Frontend** | React + TypeScript | Strict mode, Modular Architecture |
| **Networking** | Axios | Tích hợp Interceptors, Credential support (Cookies) |

---

## 2. Project Structure

### 2.1. Backend Structure (solution thực tế)
```text
novastaff/
├── NovaStaff.Model/           # Entities, DTOs, Filters, Exceptions
├── NovaStaff.DataLayes/       # DbContext, Configurations, Repositories, Interceptors
├── NovaStaff.BusinessLayers/  # Service interfaces + implementations (namespace NovaStaff.Services)
├── NovaStaff.Admin/           # API Controllers, Program.cs, Middleware
├── NovaStaff.Infrastructure/
└── Shared/NovaStaff.Shared/
```

### 2.2. Frontend Structure (Isolation & Modular)
- **Quy tắc**: Môi trường phát triển frontend phải được cô lập (isolated). Bắt buộc sửa đổi code theo dạng module, tránh "context bloating" (phình to context khi AI quét toàn dự án).
- Bỏ qua các build artifacts (như `node_modules`, `dist`) thông qua cấu hình `.gitignore` và Git exclusions chặt chẽ.

---

## 3. Frontend: Kiến trúc & Convention (Mới cập nhật)

### 3.1. Authentication & API Communication
- **Axios Instance**: Bắt buộc cấu hình Axios với `withCredentials: true`.
- **Token Management**: Tách biệt hoàn toàn storage concerns. **TUYỆT ĐỐI KHÔNG** quản lý/lưu trữ Refresh Token ở client-side (localStorage/sessionStorage). Refresh Token phải được set an toàn qua HttpOnly Cookie từ backend.
- **Auto Refresh (401 Retry Logic)**: Tích hợp logic tự động refresh token trong Axios Interceptor khi nhận lỗi 401 (Unauthorized), sau đó tự động retry lại original request bị lỗi một cách trong suốt với người dùng.
- **Login Payload**: Giao tiếp chuẩn với API qua payload Username/Password tương thích với backend contract.

### 3.2. UI/UX & Transitions (Senior-Grade)
- **UI Stability**: Triệt tiêu triệt để layout shifts (flickering) khi loading data hoặc điều hướng. Bắt buộc sử dụng **fixed-height containers** cho các skeleton states.
- **Loading Delay**: Áp dụng thống nhất delay loading nhân tạo (ví dụ: **300ms**) cho các thao tác API phản hồi quá nhanh. Đảm bảo smooth transition cho người dùng (đặc biệt ở Dashboard, Department, Attendance modules).
- **Scrollbar Aesthetics**: Hạn chế dùng thanh cuộn mặc định thô kệch của trình duyệt. Yêu cầu ẩn track scrollbar hoặc dùng CSS custom scrollbar mang phong cách minimalist, tinh tế. Tránh để layout bị vỡ do overflow.
- **Activation Flow**: Route `/activate` xử lý logic thiết lập mật khẩu an toàn, redirect chuẩn xác và không được conflict/premature redirect về Login page.

---

## 4. Backend: Core Models & Conventions

### 4.1. Base Models
- **BaseEntity**: Chứa thông tin audit cơ bản (`CreatedBy`, `CreatedByName`, `CreatedDate`, `ModifiedBy`, `ModifiedByName`, `ModifiedDate`).
- **PagedResult\<T\>**: Record class tiêu chuẩn hóa việc phân trang (`Items`, `TotalCount`, `PageIndex`, `PageSize`). Hỗ trợ thuộc tính tính toán (`TotalPages`, `HasNext`) và factory method `Empty()`.

### 4.2. Database & Entity Framework
- **Hard Delete & AuditLog Strategy**: Hệ thống sử dụng **Hard Delete** (xóa vật lý) thay vì Soft Delete để giảm tải DB và tăng tốc độ truy vấn (tránh overhead của việc luôn phải check `IsDeleted = 0`). Toàn bộ dữ liệu trước khi xóa sẽ được `AuditInterceptor` (chụp full dữ liệu OriginalValue) lưu tự động vào bảng `AuditLogs` để đảm bảo tuân thủ truy vết (Compliance) mà không làm phình bảng nghiệp vụ.
- **Decimal Precision**: Global default là `precision 18, scale 2`. Chỉ override thủ công trong `IEntityTypeConfiguration` nếu thực sự cần thiết.
- **Configuration**: Tất cả `IEntityTypeConfiguration<T>` tự động load qua `ApplyConfigurationsFromAssembly`. Mỗi entity có file Configuration riêng trong `DataLayers/Configurations/`.

---

## 5. Backend: Layer Rules & Patterns

### 5.1. Generic Repository Pattern (`IRepository<TEntity, TKey>`)
- **Nguyên tắc**: Interface Generic chứa contract đầy đủ (`Read`, `Paged`, `Exists`, `Count`, `Write`). **KHÔNG** thêm method vào interface này trừ khi logic đó áp dụng cho **mọi** entity.
- **Implementation Notes (`GenericRepository`)**:
  - Cache `_pkName` tại constructor từ EF metadata (tránh reflection lặp lại).
  - `GetByIdAsync` fast path: Dùng `FindAsync` nếu không có include.
  - `GetByIdAsync` include path: Dùng `_pkName` đã cache (KHÔNG hardcode tên trường là "Id").
  - `Delete()`: Hard delete qua `_dbSet.Remove(entity)`.

### 5.2. Repository Layer Checklist
- ✅ Chỉ làm việc và trả về Entity — **KHÔNG** trả về DTO.
- ✅ Có thể query các bảng liên quan cho nghiệp vụ `EXISTS` / `COUNT`.
- ❌ **KHÔNG** chứa business logic.
- ❌ **KHÔNG** expose `IQueryable`, `Expression<Func<>>`, `Func<IQueryable<>>` ra interface cụ thể.
- ✅ Phải dùng **Filter Object Pattern** (ví dụ `XxxFilter filter`) để truyền tham số lọc thay vì dùng EF delegates. Map filter sang EF query ở bên trong repository.

### 5.3. Service Layer Checklist
- ✅ Inject `IUnitOfWork`, không inject `DbContext` trực tiếp.
- ✅ Chứa 100% Business Logic.
- ✅ Quản lý Transaction boundary và quyết định `IsolationLevel` cho từng operation.
- ❌ **KHÔNG** trực tiếp query `DbContext`.
- ✅ Gọi domain logic repo (ví dụ: `GenerateNewNodeAsync` cho `OrgPath`).
- ✅ Gọi `SaveChangesAsync()` thông qua UnitOfWork, không qua Repository.

### 5.4. Controller & Web API Rules (Program.cs)
- ✅ **KHÔNG** dùng try-catch ở Controller để xử lý lỗi nghiệp vụ. Controller phải mỏng.
- ✅ Bắt buộc ném thẳng Exception từ Service (VD: `KeyNotFoundException`, `ArgumentException`). `GlobalExceptionMiddleware` sẽ tự động bắt và map sang mã HTTP status code tương ứng (404, 400...).
- ✅ Đọc cấu hình CORS (`AllowedOrigins`) từ `appsettings.json`.
- ✅ **Fail-Fast Principle**: Quăng `InvalidOperationException` ngay lúc Startup (Program.cs) nếu cấu hình CORS bị thiếu. Tuyệt đối không dùng Fallback rỗng để tránh lỗi ngầm nguy hiểm trên Frontend.

---

## 6. Backend: Department & OrgPath (Critical Domain)

### 6.1. Entity & Repository
- **OrgPath**: Materialized path (string). Setter cập nhật `OrgLevel` từ độ sâu path.
- Logic cây tập trung Repository: `GenerateNewNodeAsync`, descendants/children queries.

### 6.2. Indexes (Safety Net)
- **UNIQUE** trên `OrgPath` (`IX_Departments_OrgPath`).
- **UNIQUE** trên `Code` (`IX_Departments_Code`) khi có mã.

### 6.3. Concurrency & Atomicity
- Nhiều request tạo node con đồng thời → trùng path nếu không khóa.
- **Giải pháp:** `DepartmentService` dùng `ExecuteInTransactionAsync(..., IsolationLevel.Serializable)` + `GenerateNewNodeAsync`.

---

## 7. Version Control & Professional Git Workflow
- Cập nhật quy trình làm việc chuẩn Senior GitHub Workflows:
  - **Trunk-based Development**: Merge code thường xuyên, giảm xung đột.
  - **Atomic Commits/PRs**: Mỗi PR hoặc commit chỉ giải quyết một chức năng/bug cụ thể, không dồn cục (No massive commits).
  - **Conventional Commits**: Đặt tên commit có ý nghĩa (`feat:`, `fix:`, `chore:`, `refactor:`, `style:`).
  - Không push thẳng code rác, test logs hoặc build artifacts. Đảm bảo lịch sử Git sạch sẽ và dễ maintain.

---

> **Note**: Hệ thống đang áp dụng mô hình phân tách rõ ràng giữa DB Performance (Hard Delete + AuditInterceptor), Concurrency Control (Serializable), và Frontend UX cực kỳ mượt mà. Đảm bảo tuân thủ 100% trong quá trình thêm mới code.