# NovaStaff — Architecture Decision Record (ADR)

> **Mục đích**: File này đóng vai trò là "memory" (bộ nhớ dài hạn) cho AI assistant và tài liệu onboarding cho team development.
> **Quy tắc sử dụng**: Cung cấp toàn bộ nội dung file này vào đầu mỗi conversation mới để AI nắm bắt đúng context, các quy tắc kiến trúc đã chốt, tránh đi sai hướng.
> **Bảo trì**: Cập nhật file này ngay lập tức mỗi khi có quyết định kiến trúc hoặc convention mới được thống nhất ở cả Backend lẫn Frontend.

---

## 1. Technology Stack

| Layer | Technology | Chú thích |
|---|---|---|
| **Backend** | ASP.NET Core, C# | RESTful API, Global Exception Handling |
| **ORM** | Entity Framework Core | Code-first, Configurations mapping riêng biệt |
| **Database** | SQL Server | Sử dụng kiểu `HierarchyId` native cho dữ liệu phân cấp |
| **Frontend** | React + TypeScript | Strict mode, Modular Architecture |
| **Networking** | Axios | Tích hợp Interceptors, Credential support (Cookies) |

---

## 2. Project Structure

### 2.1. Backend Structure
```text
NovaStaff/
├── Models/
│   ├── Common/          # BaseEntity, PagedResult<T>
│   ├── Entities/        # Domain entities
│   ├── DTOs/            # Request/Response objects
│   └── Filters/         # Filter objects (thay thế EF delegates)
├── DataLayers/
│   ├── AppDbContext.cs
│   ├── Configurations/  # IEntityTypeConfiguration<T>
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── Repositories/
│   └── Repositories/
│       ├── GenericRepository.cs
│       └── [Entity]Repository.cs
└── Services/
    ├── Interfaces/
    └── [Entity]Service.cs
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
- ✅ Gọi domain logic (ví dụ: `HierarchyId.GetDescendant`).
- ✅ Gọi `SaveChangesAsync()` thông qua UnitOfWork, không qua Repository.

### 5.4. Controller & Web API Rules (Program.cs)
- ✅ **KHÔNG** dùng try-catch ở Controller để xử lý lỗi nghiệp vụ. Controller phải mỏng.
- ✅ Bắt buộc ném thẳng Exception từ Service (VD: `KeyNotFoundException`, `ArgumentException`). `GlobalExceptionMiddleware` sẽ tự động bắt và map sang mã HTTP status code tương ứng (404, 400...).
- ✅ Đọc cấu hình CORS (`AllowedOrigins`) từ `appsettings.json`.
- ✅ **Fail-Fast Principle**: Quăng `InvalidOperationException` ngay lúc Startup (Program.cs) nếu cấu hình CORS bị thiếu. Tuyệt đối không dùng Fallback rỗng để tránh lỗi ngầm nguy hiểm trên Frontend.

---

## 6. Backend: Department & HierarchyId (Critical Domain)

### 6.1. Entity & Repository
- **OrgNode**: Property setter **KHÔNG** tự tính `OrgLevel`. DB computed column là single source of truth.
- Tập trung logic truy vấn Tree Node tại Repository để tối ưu round-trip DB (`GetPositionAsync`, `GetLastRootNodeAsync`, `GetLastChildNodeAsync`, `GenerateNewNodeAsync`).
- XÓA bỏ `GetParentNodeAsync` (đã rename thành `GetPositionAsync`).

### 6.2. Indexes (Safety Net)
- Bắt buộc phải có **UNIQUE INDEX** trên `OrgNode` (`IX_Departments_OrgNode`) làm safety net phòng chống race condition.
- Có partial UNIQUE INDEX trên `Code` (`[Code] IS NOT NULL AND [IsDeleted] = 0`).

### 6.3. Concurrency & Atomicity
- **Vấn đề**: Khi nhiều thread cùng lúc (Thread A, Thread B) tạo child node sẽ sinh ra trùng lặp OrgNode.
- **Giải pháp**: Bắt buộc bọc bằng `Serializable Transaction` trong Service.
```csharp
// Mẫu trong DepartmentService.CreateAsync
await using var tx = await _uow.BeginTransactionAsync(IsolationLevel.Serializable);
// Lấy parent, lấy lastChild, sinh newNode (domain logic), SaveChanges
await tx.CommitAsync(ct);
```

---

## 7. Version Control & Professional Git Workflow
- Cập nhật quy trình làm việc chuẩn Senior GitHub Workflows:
  - **Trunk-based Development**: Merge code thường xuyên, giảm xung đột.
  - **Atomic Commits/PRs**: Mỗi PR hoặc commit chỉ giải quyết một chức năng/bug cụ thể, không dồn cục (No massive commits).
  - **Conventional Commits**: Đặt tên commit có ý nghĩa (`feat:`, `fix:`, `chore:`, `refactor:`, `style:`).
  - Không push thẳng code rác, test logs hoặc build artifacts. Đảm bảo lịch sử Git sạch sẽ và dễ maintain.

---

> **Note**: Hệ thống đang áp dụng mô hình phân tách rõ ràng giữa DB Performance (Hard Delete + AuditInterceptor), Concurrency Control (Serializable), và Frontend UX cực kỳ mượt mà. Đảm bảo tuân thủ 100% trong quá trình thêm mới code.