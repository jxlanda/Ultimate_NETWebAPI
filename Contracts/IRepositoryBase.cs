using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IRepositoryBase<T>
    {
        //IQueryable<T> FindAll();
        //IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
        //void Create(T entity);
        //void Update(T entity);
        //void Delete(T entity);

        // New Methods

		void Delete(T entity);

		void Delete(params object[] id);

		Task DeleteAsync(T entity);

		Task DeleteAsync(params object[] id);

		void DeleteRange(params T[] entities);

		Task DeleteRangeAsync(params T[] entities);

		IEnumerable<T> Get(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 0,
			int pageSize = 10
		);

		IEnumerable<dynamic> GetBySelect(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 0,
			int pageSize = 10,
			Expression<Func<T, int, object>> select = null
		);

		Task<IEnumerable<T>> GetAsync(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 0,
			int pageSize = 10
		);

		Task<IEnumerable<dynamic>> GetBySelectAsync(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 0,
			int pageSize = 10,
			Expression<Func<T, int, object>> select = null
		);

		T GetByKey(params object[] id);

		Task<T> GetByKeyAsync(params object[] id);

		void Insert(T entity);

		Task InsertAsync(T entity);

		void InsertRange(params T[] entities);

		Task InsertRangeAsync(params T[] entities);

		void Update(T entity);

		Task UpdateAsync(T entity);

		void UpdateRange(params T[] entities);

		Task UpdateRangeAsync(params T[] entities);

		// Raw SQL
		IEnumerable<T> GetWithRawSql(string query);

		IEnumerable<T> GetWithRawSql(string query, params object[] parameters);

		Task<IEnumerable<T>> GetWithRawSqlAsync(string query);

		Task<IEnumerable<T>> GetWithRawSqlAsync(string query, params object[] parameters);

	}
}
