// DataLayers/Repositories/UnitOfWork.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.Models.Common;
using System.Collections.Concurrent;
using System.Data;

namespace NovaStaff.DataLayers.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : BaseEntity
    {
        return (IRepository<TEntity, TKey>)_repositories
            .GetOrAdd(typeof(TEntity), _ => new GenericRepository<TEntity, TKey>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel level = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
        => await _context.Database.BeginTransactionAsync(level, ct);
}



