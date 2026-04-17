using MayNghien.Infrastructures.Models;
using MayNghien.Infrastructures.Models.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MayNghien.Infrastructures.Repository
{
    public interface IGenericRepository<T, C, TUser> where T : BaseEntity 
        where C : BaseContext<TUser> where TUser:IdentityUser
    {
        void ClearTracker();
        DbSet<T> GetSet();
        T? Get(Guid id);
        Task<T?> GetAsync(Guid id);
        void Add(T entity);
        Task AddAsync(T entity);
        void Delete(T entity);
        Task DeleteAsync(T entity);
        void Edit(T entity);
        Task EditAsync(T entity);
        void EditRange(List<T> entities);
        void AddRange(List<T> entities, bool isCommit = true);
        Task AddRangeAsync(List<T> entities, bool isCommit = true);
        void DeleteRange(List<T> entities);
        Task DeleteRangeAsync(List<T> entities);
        void SoftDeleteRange(List<T> entities);
        Task SoftDeleteRangeAsync(List<T> entities);
        Task<int> CountRecordsAsync(Expression<Func<T, bool>> predicate);
        void BulkInsert(IList<T> items, int packageSize = 1000);
        Task BulkInsert(IList<T> entities, CancellationToken cancellationToken);
        Task BulkUpdate(IList<T> entities, CancellationToken cancellationToken);
        IQueryable<T> FindByAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> AsQueryable();
        IQueryable<T> FindByPredicate(Expression<Func<T, bool>> predicate);
        int CountRecordsByPredicate(Expression<Func<T, bool>> predicate);
        IQueryable<T> AddSort(IQueryable<T> input, SortByInfo sortByInfo);
    }
}
