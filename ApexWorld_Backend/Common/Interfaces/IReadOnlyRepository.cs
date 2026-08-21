using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Common.Interfaces
{
    public interface IReadOnlyRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, string includeProperties = "");
    }
}
