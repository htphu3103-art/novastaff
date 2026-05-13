// Interfaces/IRepository.cs
using NovaStaff.Models.Common;
using System.Linq.Expressions;

namespace NovaStaff.DataLayers.Interfaces;

/// <summary>
/// Generic Repository — thao tác d? li?u chu?n cho m?i entity.
///
/// Rule:
///   - Không ch?a business logic
///   - Respect global query filter (IsDeleted = false)
///   - Service quy?t ð?nh filter / sort / include / tracking
/// </summary>
public interface IRepository<TEntity, TKey>
    where TEntity : BaseEntity
{
    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// M?c ðích:
    ///   L?y entity theo Id.
    ///
    /// Query:
    ///   WHERE Id = @id AND IsDeleted = 0
    ///
    /// Tracking:
    ///   Controlled by trackChanges
    ///
    /// Include:
    ///   Optional
    ///
    /// Rule:
    ///   Return null n?u không t?n t?i
    /// </summary>
    Task<TEntity?> GetByIdAsync(
        TKey id,
        bool trackChanges = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken ct = default);

    /// <summary>
    /// M?c ðích:
    ///   L?y toàn b? d? li?u (ch? dùng cho b?ng nh?).
    ///
    /// Tracking:
    ///   Default: NoTracking
    ///
    /// Rule:
    ///   Không dùng cho b?ng l?n
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        bool trackChanges = false,
        CancellationToken ct = default);

    /// <summary>
    /// M?c ðích:
    ///   Query theo ði?u ki?n ðõn gi?n.
    ///
    /// Query:
    ///   WHERE predicate AND IsDeleted = 0
    ///
    /// Include:
    ///   Optional
    ///
    /// Rule:
    ///   Không thay th? paging query
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken ct = default);

    // =========================================================
    // PAGED
    // =========================================================

    /// <summary>
    /// M?c ðích:
    ///   Query có phân trang.
    ///
    /// Query:
    ///   WHERE filter AND IsDeleted = 0
    ///
    /// Sort:
    ///   Controlled by orderBy
    ///
    /// Include:
    ///   Optional
    ///
    /// Tracking:
    ///   Controlled by trackChanges
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default);

    // =========================================================
    // EXISTS / COUNT
    // =========================================================

    /// <summary>
    /// M?c ðích:
    ///   Ki?m tra t?n t?i theo Id.
    ///
    /// Query:
    ///   WHERE Id = @id AND IsDeleted = 0
    /// </summary>
    Task<bool> ExistsAsync(
        TKey id,
        CancellationToken ct = default);

    /// <summary>
    /// M?c ðích:
    ///   Ki?m tra t?n t?i theo ði?u ki?n.
    ///
    /// Query:
    ///   WHERE predicate AND IsDeleted = 0
    ///
    /// Rule:
    ///   - Dùng cho validation
    ///   - Không load entity (SELECT TOP 1)
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>
    /// M?c ðích:
    ///   Ð?m t?ng s? record.
    ///
    /// Query:
    ///   SELECT COUNT(*) WHERE IsDeleted = 0
    /// </summary>
    Task<int> CountAsync(
        CancellationToken ct = default);

    /// <summary>
    /// M?c ðích:
    ///   Ð?m theo ði?u ki?n.
    ///
    /// Query:
    ///   SELECT COUNT(*) WHERE predicate AND IsDeleted = 0
    ///
    /// Rule:
    ///   Dùng cho dashboard, KPI
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    // =========================================================
    // WRITE
    // =========================================================

    /// <summary>
    /// M?c ðích:
    ///   Thêm entity (chýa commit DB).
    ///
    /// Rule:
    ///   Ph?i g?i SaveChangesAsync()
    /// </summary>
    Task AddAsync(
        TEntity entity,
        CancellationToken ct = default);

    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken ct = default);

    /// <summary>
    /// M?c ðích:
    ///   C?p nh?t entity.
    ///
    /// Rule:
    ///   Entity ph?i ðang ðý?c tracking
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// M?c ðích:
    ///   Soft delete entity.
    ///
    /// Behavior:
    ///   IsDeleted = true
    /// </summary>
    void Delete(TEntity entity);

    Task ReloadAsync(TEntity entity, CancellationToken ct = default);
}



