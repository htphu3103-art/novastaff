// DataLayers/Interfaces/IUnitOfWork.cs
using NovaStaff.Models.Common;
using System.Data;

namespace NovaStaff.DataLayers.Interfaces;

public interface IUnitOfWork
{
    IRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        IsolationLevel level = IsolationLevel.ReadCommitted,
        CancellationToken ct = default);
}



