// DataLayers/Interfaces/IUnitOfWork.cs
using Microsoft.EntityFrameworkCore.Storage;
using NovaStaff.Models.Common;
using System.Data;

namespace NovaStaff.DataLayers.Interfaces;

public interface IUnitOfWork
{
    IRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// M? transaction th? công v?i IsolationLevel tùy ch?n.
    /// Dùng cho: HierarchyId tree operation (Serializable)
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel level = IsolationLevel.ReadCommitted,
        CancellationToken ct = default);
}



