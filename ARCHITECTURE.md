# NovaStaff — Architecture Decision Record (ADR)

> **Mục đích**: File này là "memory" cho AI assistant và onboarding tài liệu cho team.  
> Paste toàn bộ file này vào đầu mỗi conversation mới để AI follow đúng hướng.  
> Cập nhật file này mỗi khi có quyết định kiến trúc mới được thống nhất.

---

## 1. Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core, C# |
| ORM | Entity Framework Core |
| Database | SQL Server (dùng HierarchyId native) |
| Frontend | React + TypeScript |

---

## 2. Project Structure

```
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

---

## 3. Core Models

### BaseEntity
```csharp
public abstract class BaseEntity
{
    public int?      CreatedBy       { get; set; }
    public string?   CreatedByName   { get; set; }
    public DateTime  CreatedDate     { get; set; }
    public int?      ModifiedBy      { get; set; }
    public string?   ModifiedByName  { get; set; }
    public DateTime? ModifiedDate    { get; set; }
}
```

### PagedResult\<T\>
```csharp
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageIndex,
    int PageSize)
{
    public int  TotalPages  => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageIndex > 0;
    public bool HasNext     => PageIndex < TotalPages - 1;

    // Factory method — dùng khi không có kết quả
    public static PagedResult<T> Empty(int pageIndex, int pageSize)
        => new(new List<T>(), 0, pageIndex, pageSize);
}
```

---

## 4. AppDbContext — Các Convention Global

### Hard Delete & AuditLog Strategy
- Hệ thống sử dụng **Hard Delete** (xóa vật lý) thay vì Soft Delete để giảm tải DB và tăng tốc độ truy vấn (tránh overhead của việc luôn phải check `IsDeleted = 0`).
- Toàn bộ dữ liệu trước khi xóa sẽ được **AuditInterceptor** chụp lại (chụp full dữ liệu OriginalValue) và lưu vào bảng `AuditLogs`.
- Đảm bảo tuân thủ truy vết (Compliance) mà không gây phình to bảng nghiệp vụ.

### Decimal Precision
- Global default: `precision 18, scale 2`
- Override thủ công trong `IEntityTypeConfiguration` nếu cần khác

### Configuration
- Tất cả `IEntityTypeConfiguration<T>` tự động load qua `ApplyConfigurationsFromAssembly`
- Mỗi entity có file Configuration riêng trong `DataLayers/Configurations/`

---

## 5. Generic Repository Pattern

### IRepository\<TEntity, TKey\>
Contract đầy đủ — không thêm method vào đây trừ khi áp dụng cho **mọi** entity:

```
READ:
  GetByIdAsync(id, trackChanges, include, ct)
  GetAllAsync(trackChanges, ct)               -- chỉ dùng bảng nhỏ
  FindAsync(predicate, trackChanges, include, ct)

PAGED:
  GetPagedAsync(pageIndex, pageSize, filter, orderBy, include, trackChanges, ct)

EXISTS / COUNT:
  ExistsAsync(id, ct)
  ExistsAsync(predicate, ct)
  CountAsync(ct)
  CountAsync(predicate, ct)

WRITE:
  AddAsync(entity, ct)
  AddRangeAsync(entities, ct)
  Update(entity)
  Delete(entity)          -- hard delete: xóa vật lý, kết hợp AuditInterceptor
```

### GenericRepository\<TEntity, TKey\> — Implementation Notes
```
- Cache _pkName tại constructor từ EF metadata (tránh reflection lặp lại)
- GetByIdAsync fast path: FindAsync nếu không có include
- GetByIdAsync include path: dùng _pkName đã cache (KHÔNG hardcode "Id")
- Delete(): hard delete — _dbSet.Remove(entity), lịch sử sẽ được AuditInterceptor tự động lưu.
```

---

## 6. Layer Rules

### Repository Layer
```
✅ Làm việc với Entity — KHÔNG trả DTO
✅ Có thể query bảng liên quan cho EXISTS / COUNT
✅ Dùng Filter object thay EF delegates trong specific repository
❌ Không chứa business logic
❌ Không expose IQueryable, Expression<Func<>>, Func<IQueryable<>> ra interface
   → Ngoại lệ duy nhất: IRepository generic (GetPagedAsync, FindAsync)
   → Specific repository interface (IDepartmentRepository, v.v.) phải dùng Filter object
```

### Service Layer
```
✅ Chứa business logic
✅ Quản lý transaction boundary
✅ Quyết định IsolationLevel cho từng operation
✅ Gọi domain logic (ví dụ: HierarchyId.GetDescendant)
❌ Không trực tiếp query DbContext
```

### Filter Object Pattern
Thay vì truyền EF delegates ra ngoài specific repository interface:
```csharp
// ❌ Sai — leak EF abstraction
Task<PagedResult<T>> GetXxxPagedAsync(
    Expression<Func<T, bool>>? predicate,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy, ...)

// ✅ Đúng — persistence-agnostic
Task<PagedResult<T>> GetXxxPagedAsync(
    XxxFilter filter, int pageIndex, int pageSize, ...)
```
Implementation map filter sang EF query **bên trong** repository.

---

## 7. Department — Quyết định đã thống nhất

### Entity
```csharp
// OrgNode setter KHÔNG tự tính OrgLevel
// DB computed column là single source of truth
public HierarchyId OrgNode
{
    get => _orgNode;
    set => _orgNode = value;   // ← chỉ assign, không tính OrgLevel
}
public short? OrgLevel { get; private set; }  // EF tự populate sau query

// Nếu cần OrgLevel trước khi SaveChanges → gọi trực tiếp:
// var level = newNode.GetLevel();
```

### IDepartmentRepository — Method Contract
```
✅ HasEmployeesAsync(int departmentId, ct)
✅ GetDescendantsPagedAsync(int departmentId, DepartmentDescendantFilter, pageIndex, pageSize, ct)
✅ GetPositionAsync(int departmentId, ct)        -- lấy OrgNode theo ID
✅ GetLastRootNodeAsync(ct)
✅ GetLastChildNodeAsync(HierarchyId parentNode, ct)
✅ GetByManagerAsync(int managerEmployeeId, ct)
✅ GenerateNewNodeAsync(int? parentId, ct) -- Tập trung logic sinh node tại Repository để tối ưu round-trip DB
❌ GetParentNodeAsync — ĐÃ XÓA, đã rename thành GetPositionAsync
```

### DepartmentDescendantFilter
```csharp
public sealed class DepartmentDescendantFilter
{
    public string?             NameContains   { get; init; }
    public bool?               IsActive       { get; init; }
    public int?                ManagerId      { get; init; }
    public DepartmentSortField SortBy         { get; init; } = DepartmentSortField.OrgNode;
    public bool                SortDescending { get; init; } = false;
}

public enum DepartmentSortField { OrgNode, Name, CreatedAt }
```

### DepartmentConfiguration — Index quan trọng
```
IX_Departments_OrgNode       — UNIQUE (bắt buộc, safety net cho race condition)
IX_Departments_Code          — UNIQUE, partial: [Code] IS NOT NULL AND [IsDeleted] = 0
IX_Departments_ManagerEmployeeID
IX_Departments_Status        — (IsActive, IsDeleted)
```

---

## 8. HierarchyId Tree — Atomicity (Critical)

### Vấn đề
```
Thread A: GetLastChildNode → /1/2/
Thread B: GetLastChildNode → /1/2/   ← cùng lúc
Thread A: Insert /1/3/ ✅
Thread B: Insert /1/3/ 💥 duplicate OrgNode
```

### Giải pháp đã chọn: Serializable Transaction trong Service

```csharp
// DepartmentService.CreateAsync
await using var tx = await _uow.BeginTransactionAsync(IsolationLevel.Serializable);

var parentNode = await _repo.GetPositionAsync(cmd.ParentId, ct);
var lastChild  = await _repo.GetLastChildNodeAsync(parentNode!, ct);
var newNode    = parentNode!.GetDescendant(lastChild, null);  // domain logic ở đây

var dept = new Department { OrgNode = newNode, ... };
await _repo.AddAsync(dept, ct);
await _uow.SaveChangesAsync(ct);
await tx.CommitAsync(ct);
```

### Rules
```
✅ GenerateNewNodeAsync (tree construction) thuộc Repository để gom các truy vấn nội bộ (LastChild/LastRoot) tránh lộ logic ra ngoài.
✅ UNIQUE constraint trên OrgNode là safety net bắt buộc (đã có)
✅ Serializable isolation bắt buộc cho các hàm Create/Move department (Phòng Phantom Read)
✅ Root department: HierarchyId.GetRoot().GetDescendant(lastRoot, null)
✅ Child department: parentNode.GetDescendant(lastChild, null)
```

---

## 9. Entities Overview

| Entity | PK | Ghi chú |
|---|---|---|
| Employee | EmployeeID (int) | |
| Department | DepartmentID (int) | HierarchyId tree |
| User | UserId | Auth account |
| WorkTask | | |
| LeaveRequest | | |
| AttendanceRecord | | |
| PayrollPeriod | | |
| PayrollDetail | | |
| AuditLog | | |

---

## 10. Checklist khi tạo Repository mới

```
□ Implement GenericRepository<TEntity, TKey>
□ Interface chỉ khai báo method đặc thù — không duplicate method từ IRepository
□ Specific filter/sort → tạo Filter object riêng, không dùng EF delegates
□ Query-only method → AsNoTracking()
□ Cross-table EXISTS → dùng _context.OtherDbSet trực tiếp, không qua repo khác
□ Không throw NotImplementedException — nếu chưa implement thì chưa khai báo vào interface
```

---

## 11. Checklist khi tạo Service mới

```
□ Inject IUnitOfWork, không inject DbContext trực tiếp
□ Business logic nằm ở đây, không ở Repository
□ Tree operation (HierarchyId) → Serializable transaction
□ Validate bằng repository helper (HasEmployeesAsync, ExistsAsync...) trước khi write
□ SaveChangesAsync() gọi qua UnitOfWork, không qua Repository
```

---

## 12. API & Web Rules (Controller & Program.cs)

```
□ Controller KHÔNG dùng try-catch để xử lý lỗi nghiệp vụ (ví dụ: KeyNotFoundException).
□ Cứ throw thẳng Exception từ Service. `GlobalExceptionMiddleware` sẽ tự động bắt và map sang mã HTTP tương ứng (404, 400...).
□ CORS phải được đọc từ `appsettings.json` (AllowedOrigins).
□ Áp dụng nguyên tắc **Fail-Fast**: Quăng `InvalidOperationException` ngay lúc Startup nếu cấu hình CORS bị thiếu, tuyệt đối không dùng Fallback rỗng để tránh lỗi ngầm trên Frontend.
```