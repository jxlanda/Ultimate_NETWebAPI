using Contracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Entities.Models;
using EFCore.BulkExtensions;

namespace Repository
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected RepositoryContext RepositoryContext { get; set; }
        protected DbSet<T> Set;
		// Properties DataShaper
		public PropertyInfo[] Properties { get; set; }

		public RepositoryBase(RepositoryContext repositoryContext)
        {
            this.RepositoryContext = repositoryContext;
            this.Set = this.RepositoryContext.Set<T>();
			Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
		}

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

		public virtual IQueryable<T> Get(
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

		public virtual IQueryable<dynamic> GetBySelect(
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

		public virtual Task<IQueryable<T>> GetAsync(
			Expression<Func<T, bool>> filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
			string includeProperties = "",
			int page = 1,
			int pageSize = 10) =>
			Task.Run(() => this.Get(filter, orderBy, includeProperties, page, pageSize));

		public virtual Task<IQueryable<dynamic>> GetBySelectAsync(
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

        public virtual IQueryable<T> GetWithRawSql(string query)
        {
            return Set.FromSqlRaw(query);
        }

        public virtual IQueryable<T> GetWithRawSql(string query, params object[] parameters) =>
			Set.FromSqlRaw(query, parameters);

		public virtual async Task<IQueryable<T>> GetWithRawSqlAsync(string query) =>
			await Task.Run(() => this.GetWithRawSql(query));

		public virtual async Task<IQueryable<T>> GetWithRawSqlAsync(string query, params object[] parameters) =>
			await Task.Run(() => this.GetWithRawSql(query, parameters));


		// Helpers 

		public IQueryable<T> ApplySort(IQueryable<T> entities, string orderByQueryString)
		{
			if (!entities.Any())
				return entities;
			if (string.IsNullOrWhiteSpace(orderByQueryString))
			{
				return entities;
			}
			var orderParams = orderByQueryString.Trim().Split(',');
			var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var orderQueryBuilder = new StringBuilder();
			foreach (var param in orderParams)
			{
				if (string.IsNullOrWhiteSpace(param))
					continue;
				var propertyFromQueryName = param.Split(" ")[0];
				var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));
				if (objectProperty == null)
					continue;
				var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
				orderQueryBuilder.Append($"{objectProperty.Name} {sortingOrder}, ");
			}
			var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
			return entities.OrderBy(orderQuery);
		}

		public IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString)
		{
			var requiredProperties = GetRequiredProperties(fieldsString);

			return FetchData(entities, requiredProperties);
		}

		public ShapedEntity ShapeData(T entity, string fieldsString)
		{
			var requiredProperties = GetRequiredProperties(fieldsString);

			return FetchDataForEntity(entity, requiredProperties);
		}

		// BulkExtensions Async

		public virtual Task BulkInsertAsync(params T[] entities) =>
			RepositoryContext.BulkInsertAsync(entities);

		public virtual Task BulkUpsertAsync(params T[] entities) =>
			RepositoryContext.BulkInsertOrUpdateAsync(entities);

		public virtual Task BulkUpdateAsync(params T[] entities) =>
			 RepositoryContext.BulkUpdateAsync(entities);

		public virtual Task BulkDeleteAsync(params T[] entities) =>
		   RepositoryContext.BulkDeleteAsync(entities);

		public virtual Task BulkSynchronizeAsync(params T[] entities) =>
			RepositoryContext.BulkInsertOrUpdateOrDeleteAsync(entities);

		public virtual Task BulkTruncateAsync() =>
			RepositoryContext.TruncateAsync<T>();


		// Private methods

		private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
		{
			var requiredProperties = new List<PropertyInfo>();

			if (!string.IsNullOrWhiteSpace(fieldsString))
			{
				var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

				foreach (var field in fields)
				{
					var property = Properties.FirstOrDefault(pi => pi.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));

					if (property == null)
						continue;

					requiredProperties.Add(property);
				}
			}
			else
			{
				requiredProperties = Properties.ToList();
			}

			return requiredProperties;
		}

		private IEnumerable<ShapedEntity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
		{
			var shapedData = new List<ShapedEntity>();

			foreach (var entity in entities)
			{
				var shapedObject = FetchDataForEntity(entity, requiredProperties);
				shapedData.Add(shapedObject);
			}

			return shapedData;
		}

		private ShapedEntity FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
		{
			var shapedObject = new ShapedEntity();

			foreach (var property in requiredProperties)
			{
				var objectPropertyValue = property.GetValue(entity);
				shapedObject.Entity.TryAdd(property.Name, objectPropertyValue);
			}

			var objectProperty = entity.GetType().GetProperty("Id");
			shapedObject.Id = (Guid)objectProperty.GetValue(entity);

			return shapedObject;
		}
	}
}
