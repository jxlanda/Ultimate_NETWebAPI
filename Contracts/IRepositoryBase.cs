using EFCore.BulkExtensions;
using Entities.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IRepositoryBase<T>
	{
        void Delete(T entity);

        void Delete(params object[] id);

        Task DeleteAsync(T entity);

        Task DeleteAsync(params object[] id);

        void DeleteRange(params T[] entities);

        Task DeleteRangeAsync(params T[] entities);

        // GET 

        IQueryable<T> GetAll();

        IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> expression);

        Task<IQueryable<T>> GetAllAsync();

        Task<IQueryable<T>> GetAllByConditionAsync(Expression<Func<T, bool>> expression);

        bool Exists(Expression<Func<T, bool>> expression);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);

        // GET with Filter, Order and Paging

        IQueryable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int page = 0,
            int pageSize = 0
        );

        Task<IQueryable<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int page = 0,
            int pageSize = 0
        );

        PagedList<ShapedEntity> GetPaged(
            Expression<Func<T, bool>> filter = null,
            string orderBy = null,
            string includeProperties = "",
            string onlyFields = "",
            string searchTerm = null,
            string includeSearch = null,
            int page = 0,
            int pageSize = 10);

        Task<PagedList<ShapedEntity>> GetPagedAsync(
            Expression<Func<T, bool>> filter = null,
            string orderBy = null,
            string includeProperties = "",
            string onlyFields = "",
            string searchTerm = null,
            string includeSearch = null,
            int page = 0,
            int pageSize = 10);

        PagedList<ShapedEntity> GetQueryPaged(
          IQueryable<object> baseQuery,
          Expression<Func<T, bool>> filter = null,
          string orderBy = null,
          string includeProperties = "",
          string onlyFields = "",
          string searchTerm = null,
          string includeSearch = null,
          int page = 0,
          int pageSize = 10);

        Task<PagedList<ShapedEntity>> GetQueryPagedAsync(
            IQueryable<object> baseQuery,
            Expression<Func<T, bool>> filter = null,
            string orderBy = null,
            string includeProperties = "",
            string onlyFields = "",
            string searchTerm = null,
            string includeSearch = null,
            int page = 0,
            int pageSize = 10);

        T GetByKey(params object[] id);

        Task<T> GetByKeyAsync(params object[] id);

        void Insert(T entity);

        Task InsertAsync(T entity);

        void InsertRange(params T[] entities);

        Task InsertRangeAsync(params T[] entities);

        void Update(T entity, params Expression<Func<T, object>>[] onlyProperties);

        Task UpdateAsync(T entity, params Expression<Func<T, object>>[] onlyProperties);

        void UpdateRange(T[] entities, params Expression<Func<T, object>>[] onlyProperties);

        Task UpdateRangeAsync(T[] entities, params Expression<Func<T, object>>[] onlyProperties);


        // Bulk Extensions 
        Task BulkInsertAsync(T[] entities, BulkConfig config = null);
        Task BulkUpsertAsync(T[] entities, BulkConfig config = null);
        Task BulkUpdateAsync(T[] entities, BulkConfig config = null);
        Task BulkDeleteAsync(T[] entities, BulkConfig config = null);
        Task BulkSynchronizeAsync(T[] entities, BulkConfig config = null);
        Task BulkReadAsync(T[] entities, BulkConfig config = null);
        Task BulkTruncateAsync();

        // Raw SQL
        IQueryable<T> GetWithRawSql(string query);

        IQueryable<T> GetWithRawSql(string query, params object[] parameters);

        Task<IQueryable<T>> GetWithRawSqlAsync(string query);

        Task<IQueryable<T>> GetWithRawSqlAsync(string query, params object[] parameters);

    }
}
