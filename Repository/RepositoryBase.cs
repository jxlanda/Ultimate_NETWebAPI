using Contracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repository
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected RepositoryContext RepositoryContext { get; set; }
        protected DbSet<T> Set;
        public RepositoryBase(RepositoryContext repositoryContext)
        {
            this.RepositoryContext = repositoryContext;
            this.Set = this.RepositoryContext.Set<T>();
        }

		//public IQueryable<T> FindAll()
		//{
		//    return this.RepositoryContext.Set<T>().AsNoTracking();
		//}
		//public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
		//{
		//    return this.RepositoryContext.Set<T>().Where(expression).AsNoTracking();
		//}
		//public void Create(T entity)
		//{
		//    this.RepositoryContext.Set<T>().Add(entity);
		//}
		//public void Update(T entity)
		//{
		//    this.RepositoryContext.Set<T>().Update(entity);
		//}
		//public void Delete(T entity)
		//{
		//    this.RepositoryContext.Set<T>().Remove(entity);
		//}

		public virtual void Delete(T entity)
		{
			if (RepositoryContext.Entry<T>(entity).State == EntityState.Detached)
			{
				Set.Attach(entity);
			}
			Set.Remove(entity);
		}

		public virtual void Delete(params object[] id) =>
			this.Delete(this.GetByKey(id));

		public virtual Task DeleteAsync(T entity) =>
			Task.Run(() => this.Delete(entity));

		public virtual Task DeleteAsync(params object[] id) =>
			Task.Run(() => this.Delete(id));

		public virtual void DeleteRange(params T[] entities)
		{
			(entities.AsEnumerable() as List<T>)?.ForEach(e =>
			{
				if (RepositoryContext.Entry<T>(e).State == EntityState.Detached)
				{
					Set.Attach(e);
				}
			});
			Set.RemoveRange(entities);
		}

		public virtual Task DeleteRangeAsync(params T[] entities) =>
			Task.Run(() => this.DeleteRange(entities));

		public virtual IEnumerable<T> Get(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 0,
			int pageSize = 10)
		{
			IQueryable<T> query = Set;
			if (filter != null) query = query.Where(filter);
			if (includeProperties != null || includeProperties != string.Empty || includeProperties != "")
			{
				includeProperties.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
					.ToList()
					.ForEach(x => query = query.Include(x));
			}
			if (orderBy != null) query = orderBy(query);
			return query?.Skip(page * pageSize).Take(pageSize);
		}

		public virtual IEnumerable<dynamic> GetBySelect(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 0,
			int pageSize = 10,
			Expression<Func<T, int, object>> select = null)
		{
			IQueryable<T> query = Set;
			if (filter != null) query = query.Where(filter);
			if (includeProperties != null || includeProperties != string.Empty || includeProperties != "")
			{
				includeProperties.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
					.ToList()
					.ForEach(x => query = query.Include(x));
			}
			if (orderBy != null) query = orderBy(query);
			return query?.Skip(page * pageSize).Take(pageSize).Select(select);
		}

		public virtual Task<IEnumerable<T>> GetAsync(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 1,
			int pageSize = 10) =>
			Task.Run(() => this.Get(filter, orderBy, includeProperties, page, pageSize));

		public virtual Task<IEnumerable<dynamic>> GetBySelectAsync(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 0,
			int pageSize = 10,
			Expression<Func<T, int, object>> select = null) =>
			Task.Run(() => this.GetBySelect(filter, orderBy, includeProperties, page, pageSize, select));

		public virtual T GetByKey(params object[] id) =>
			Set.Find(id);

		public virtual Task<T> GetByKeyAsync(params object[] id) =>
			Set.FindAsync(id).AsTask();

		public virtual void Insert(T entity)
		{
			Set.Add(entity);
		}

		public virtual Task InsertAsync(T entity)
		{
			return Set.AddAsync(entity).AsTask();
		}

		public virtual void InsertRange(params T[] entities)
		{
			foreach (T entity in entities)
			{
				this.Insert(entity);
			}
		}

		public virtual Task InsertRangeAsync(params T[] entities) =>
			Task.Run(() => this.InsertRange(entities));

		public virtual void Update(T entity)
		{
			Set.Attach(entity);
			RepositoryContext.Entry<T>(entity).State = EntityState.Modified;
		}

		public virtual Task UpdateAsync(T entity) =>
			Task.Run(() => this.Update(entity));

		public virtual void UpdateRange(params T[] entities)
		{
			Set.AttachRange(entities);
			(entities.AsEnumerable() as List<T>)?.ForEach(e => RepositoryContext.Entry<T>(e).State = EntityState.Modified);
		}

		public virtual Task UpdateRangeAsync(params T[] entities) =>
			Task.Run(() => this.UpdateRange(entities));

        public virtual IEnumerable<T> GetWithRawSql(string query)
        {
            return Set.FromSqlRaw(query);
        }

        public virtual IEnumerable<T> GetWithRawSql(string query, params object[] parameters) =>
			Set.FromSqlRaw(query, parameters);

		public virtual async Task<IEnumerable<T>> GetWithRawSqlAsync(string query) =>
			await Task.Run(() => this.GetWithRawSql(query));

		public virtual async Task<IEnumerable<T>> GetWithRawSqlAsync(string query, params object[] parameters) =>
			await Task.Run(() => this.GetWithRawSql(query, parameters));
	}
}
