// DataLayers/Repositories/GenericRepository.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.Models.Common;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NovaStaff.DataLayers.Repositories;

public class GenericRepository<TEntity, TKey>
    : IRepository<TEntity, TKey>
    where TEntity : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    // Cache tên PK t?i constructor — đ?c 1 l?n duy nh?t
    private readonly string _pkName;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();

        // Đ?c PK name 1 l?n, tránh reflection l?p l?i
        _pkName = context.Model
            .FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!
            .Properties[0].Name;
    }

    // =========================================================
    // BASE QUERY BUILDER
    // =========================================================

    protected IQueryable<TEntity> BuildQuery(
        bool trackChanges = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
    {
        IQueryable<TEntity> query = trackChanges
            ? _dbSet
            : _dbSet.AsNoTracking();

        return include != null ? include(query) : query;
    }

    // =========================================================
    // READ
    // =========================================================

    public async Task<TEntity?> GetByIdAsync(
    TKey id,
    bool trackChanges = false,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
    CancellationToken ct = default)
    {
        // ✅ Fast path: không include → dùng FindAsync (tối ưu)
        if (include == null)
        {
            var entity = await _dbSet.FindAsync(new object[] { id! }, ct);

            if (entity == null)
                return null;

            if (!trackChanges)
                _context.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        // ✅ Build query khi có include
        var query = BuildQuery(trackChanges, include);

        // ✅ Build expression e => EF.Property<TKey>(e, _pkName) == id
        var parameter = Expression.Parameter(typeof(TEntity), "e");

        var property = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { typeof(TKey) },
            parameter,
            Expression.Constant(_pkName)
        );

        var equals = Expression.Equal(
            property,
            Expression.Constant(id)
        );

        var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        return await query.FirstOrDefaultAsync(lambda, ct);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        bool trackChanges = false,
        CancellationToken ct = default)
    {
        return await BuildQuery(trackChanges).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken ct = default)
    {
        return await BuildQuery(trackChanges, include)
            .Where(predicate)
            .ToListAsync(ct);
    }

    // =========================================================
    // PAGED
    // =========================================================

    public async Task<PagedResult<TEntity>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default)
    {
        var query = BuildQuery(trackChanges, include);

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync(ct);

        query = orderBy != null
            ? orderBy(query)
            : query; // caller ch?u trách nhi?m sort n?u c?n

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<TEntity>(items, totalCount, pageIndex, pageSize);
    }

    // =========================================================
    // EXISTS / COUNT
    // =========================================================

    public async Task<bool> ExistsAsync(TKey id, CancellationToken ct = default)
    {
        // Dùng _pkName đ? cache thay v? đ?c metadata m?i l?n
        return await _dbSet.AnyAsync(
            e => EF.Property<TKey>(e, _pkName)!.Equals(id), ct);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(predicate, ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await _dbSet.CountAsync(ct);

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
        => await _dbSet.CountAsync(predicate, ct);

    // =========================================================
    // WRITE
    // =========================================================

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken ct = default)
        => await _dbSet.AddRangeAsync(entities, ct);

    public void Update(TEntity entity)
        => _dbSet.Update(entity);

    public void Delete(TEntity entity)
    {
        // Chuy?n sang xóa c?ng (Hard Delete)
        _dbSet.Remove(entity);
    }

    // DataLayers/Repositories/GenericRepository.cs
    public async Task ReloadAsync(TEntity entity, CancellationToken ct = default)
        => await _context.Entry(entity).ReloadAsync(ct);
}



