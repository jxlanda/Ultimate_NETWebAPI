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
using System.ComponentModel.DataAnnotations;

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

        public virtual bool Exists(Expression<Func<T, bool>> expression) => Set.Any(expression);

        public virtual Task<bool> ExistsAsync(Expression<Func<T, bool>> expression) => Task.Run(() => Exists(expression));

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

        public IQueryable<T> GetAll() => Set.AsNoTracking();

        public IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> expression)
            => Set.Where(expression).AsNoTracking();

        public Task<IQueryable<T>> GetAllAsync() => Task.Run(() => Set.AsNoTracking());

        public Task<IQueryable<T>> GetAllByConditionAsync(Expression<Func<T, bool>> expression)
            => Task.Run(() => Set.Where(expression).AsNoTracking());

        public virtual IQueryable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int page = 0,
            int pageSize = 0)
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

            return (pageSize != 0) ? query?.Skip(page * pageSize).Take(pageSize) : query;
        }

        public virtual Task<IQueryable<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int page = 0,
            int pageSize = 0) =>
            Task.Run(() => this.Get(filter, orderBy, includeProperties, page, pageSize));

        public virtual T GetByKey(params object[] id) =>
            Set.Find(id);

        public virtual Task<T> GetByKeyAsync(params object[] id) =>
            Set.FindAsync(id).AsTask();

        public virtual PagedList<ShapedEntity> GetPaged(
          Expression<Func<T, bool>> filter = null,
          string orderBy = null,
          string includeProperties = "",
          string onlyFields = "",
          string searchTerm = null,
          string includeSearch = null,
          int page = 0,
          int pageSize = 10)
        {
            Dictionary<string, string> mapChildFields = new Dictionary<string, string>();

            IQueryable<T> query = Set;
            if (filter != null) query = query.Where(filter);
            if (searchTerm != null) query = SearchText(query, searchTerm, includeSearch);
            if (includeProperties != null && includeProperties != string.Empty && includeProperties != "")
            {
                includeProperties.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(entityName =>
                    {
                        // Add to map include properties
                        mapChildFields.Add(entityName.Trim(), null);
                        bool propertyExists = Properties.Any(pi => pi.Name.Equals(entityName.Trim(), StringComparison.InvariantCultureIgnoreCase));
                        if (!propertyExists) return;
                        // Include to Query
                        query = query.Include(entityName.Trim());
                    });
            }

            if (!string.IsNullOrWhiteSpace(orderBy)) query = ApplySort(query, orderBy);

            var shaped = ShapeData(query, onlyFields, childFields: mapChildFields);
            return PagedList<ShapedEntity>.ToPagedList(shaped,
                page,
                pageSize);
        }

        public virtual PagedList<ShapedEntity> GetQueryPaged(
          IQueryable<object> baseQuery,
          Expression<Func<T, bool>> filter = null,
          string orderBy = null,
          string includeProperties = "",
          string onlyFields = "",
          string searchTerm = null,
          string includeSearch = null,
          int page = 0,
          int pageSize = 10)
        {
            Dictionary<string, string> mapChildFields = new Dictionary<string, string>();
            IQueryable<object> query = baseQuery;
            IEnumerable<PropertyInfo> propertiesQuery = baseQuery?.ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (filter != null) query = query.Where(filter);
            if (searchTerm != null) query = SearchText(query, searchTerm, includeSearch);
            if (includeProperties != null && includeProperties != string.Empty && includeProperties != "")
            {
                includeProperties.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(entityName =>
                    {
                        // Add to map include properties
                        mapChildFields.Add(entityName.Trim(), null);
                    });
            }

            if (!string.IsNullOrWhiteSpace(orderBy)) query = ApplySort(query, orderBy);

            IEnumerable<ShapedEntity> shaped = ShapeData(query, onlyFields, childFields: mapChildFields);
            return PagedList<ShapedEntity>.ToPagedList(shaped,
                page,
                pageSize);
        }


        public virtual Task<PagedList<ShapedEntity>> GetQueryPagedAsync(
            IQueryable<object> baseQuery,
            Expression<Func<T, bool>> filter = null,
            string orderBy = null,
            string includeProperties = "",
            string onlyFields = "",
            string searchTerm = null,
            string includeSearch = null,
            int page = 0,
            int pageSize = 10) =>
            Task.Run(() => GetQueryPaged(baseQuery, filter, orderBy, includeProperties, onlyFields, searchTerm, includeSearch, page, pageSize));

        public virtual Task<PagedList<ShapedEntity>> GetPagedAsync(
            Expression<Func<T, bool>> filter = null,
            string orderBy = null,
            string includeProperties = "",
            string onlyFields = "",
            string searchTerm = null,
            string includeSearch = null,
            int page = 0,
            int pageSize = 10) =>
            Task.Run(() => GetPaged(filter, orderBy, includeProperties, onlyFields, searchTerm, includeSearch, page, pageSize));

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

        public virtual void Update(T entity, params Expression<Func<T, object>>[] onlyProperties)
        {
            Set.Attach(entity);
            if (onlyProperties.Any())
            {
                foreach (var includeProperty in onlyProperties)
                {
                    RepositoryContext.Entry<T>(entity).Property(includeProperty).IsModified = true;
                }

                return;
            }

            RepositoryContext.Entry<T>(entity).State = EntityState.Modified;

        }

        public virtual Task UpdateAsync(T entity, params Expression<Func<T, object>>[] onlyProperties) =>
            Task.Run(() => this.Update(entity, onlyProperties));

        public virtual void UpdateRange(T[] entities, params Expression<Func<T, object>>[] onlyProperties)
        {
            Set.AttachRange(entities);
            if (onlyProperties.Any())
            {
                entities.ToList().ForEach(e =>
                {
                    foreach (var includeProperty in onlyProperties)
                    {
                        RepositoryContext.Entry<T>(e).Property(includeProperty).IsModified = true;
                    }
                });

                return;
            }

            entities.ToList().ForEach(e => RepositoryContext.Entry<T>(e).State = EntityState.Modified);

        }

        public virtual Task UpdateRangeAsync(T[] entities, params Expression<Func<T, object>>[] onlyProperties) =>
            Task.Run(() => this.UpdateRange(entities, onlyProperties));

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

        // BulkExtensions Async
        public virtual Task BulkInsertAsync(T[] entities, BulkConfig config = null) =>
            RepositoryContext.BulkInsertAsync(entities, config);

        public virtual Task BulkUpsertAsync(T[] entities, BulkConfig config = null) =>
            RepositoryContext.BulkInsertOrUpdateAsync(entities, config);

        public virtual Task BulkUpdateAsync(T[] entities, BulkConfig config = null) =>
             RepositoryContext.BulkUpdateAsync(entities, config);

        public virtual Task BulkDeleteAsync(T[] entities, BulkConfig config = null) =>
           RepositoryContext.BulkDeleteAsync(entities, config);

        public virtual Task BulkSynchronizeAsync(T[] entities, BulkConfig config = null) =>
            RepositoryContext.BulkInsertOrUpdateOrDeleteAsync(entities, config);

        public virtual Task BulkTruncateAsync() =>
            RepositoryContext.TruncateAsync<T>();

        public virtual Task BulkReadAsync(T[] entities, BulkConfig config = null) =>
            RepositoryContext.BulkReadAsync<T>(entities, config);

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

        public IQueryable<object> ApplySort(IQueryable<object> entities, string orderByQueryString)
        {
            if (!entities.Any())
                return entities;
            if (string.IsNullOrWhiteSpace(orderByQueryString))
            {
                return entities;
            }
            var orderParams = orderByQueryString.Trim().Split(',');
            var propertyInfos = entities.ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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

        IQueryable<T> SearchText(IQueryable<T> source, string term, string includeSearch)
        {
            if (string.IsNullOrEmpty(term)) { return source; }

            Type elementType = source.ElementType;

            if (int.TryParse(term.Trim(), out int ex))
            {
                // Get all the int property names on this specific type.
                PropertyInfo[] integerProperties =
                elementType.GetProperties()
                    .Where(x => x.PropertyType == typeof(int))
                    .ToArray();
                if (!integerProperties.Any() && includeSearch == null) { return source; }

                string filterExprInt = string.Join(
                " || ",
                integerProperties.Select(prp => $"{prp.Name} == (@0)")
                );

                return source.Where(filterExprInt, term);

            }

            // Get all the string property names on this specific type.
            PropertyInfo[] stringProperties =
                elementType.GetProperties()
                    .Where(x => x.PropertyType == typeof(string))
                    .ToArray();
            if (!stringProperties.Any() && includeSearch == null) { return source; }

            // Build the string expression
            string filterExpr = string.Join(
                " || ",
                stringProperties.Select(prp => $"{prp.Name}.Contains(@0)")
            );

            if (includeSearch != null)
            {
                if (String.IsNullOrEmpty(filterExpr))
                    filterExpr += SearchTextInclude(source, term, includeSearch).Remove(0, 4);
            }

            return source.Where(filterExpr, term);
        }
        IQueryable<object> SearchText(IQueryable<object> source, string term, string includeSearch)
        {
            if (string.IsNullOrEmpty(term)) { return source; }

            Type elementType = source.ElementType;

            if (int.TryParse(term.Trim(), out int ex))
            {
                var idProperty = elementType.GetProperties()
                    .Where(x => x.Name == "ID" || x.Name == "id")
                    .FirstOrDefault();
                if (idProperty != null)
                {
                    string filterNumberExpr = $"{idProperty.Name}=(@0)";
                    return source.Where(filterNumberExpr, term);
                }
            }

            // Get all the string property names on this specific type.
            PropertyInfo[] stringProperties =
                elementType.GetProperties()
                    .Where(x => x.PropertyType == typeof(string))
                    .ToArray();
            if (!stringProperties.Any()) { return source; }

            // Build the string expression
            string filterExpr = string.Join(
                " || ",
                stringProperties.Select(prp => $"{prp.Name}.Contains(@0)")
            );


            if (includeSearch != null)
            {
                if (String.IsNullOrEmpty(filterExpr))
                    filterExpr += SearchTextInclude(source, term, includeSearch).Remove(0, 4);
            }

            return source.Where(filterExpr, term);
        }

        string SearchTextInclude(IQueryable<T> source, string term, string includeSearch)
        {
            string filterExpresion = String.Empty;
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(includeSearch)) return filterExpresion;

            PropertyInfo[] stringProperties = source.ElementType.GetProperties().ToArray();
            includeSearch.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(entityName =>
                    {
                        bool propertyExists = stringProperties.Any(pi => pi.Name.Equals(entityName.Trim(), StringComparison.InvariantCultureIgnoreCase));
                        if (!propertyExists) return;
                        PropertyInfo propertyChild =
                            stringProperties.Where(x => x.Name.Equals(entityName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                            .First();
                        PropertyInfo[] propertiesChild = propertyChild.PropertyType.GetProperties().Where(x => x.PropertyType == typeof(string)).ToArray();
                        if (!propertiesChild.Any()) { return; }
                        // Build the string expression
                        string filterExpr = string.Join(
                            " || ",
                            propertiesChild.Select(prp => $"{entityName}.{prp.Name}.Contains(@0)")
                        );
                        filterExpresion += " || " + filterExpr;
                    });

            return filterExpresion;
        }

        string SearchTextInclude(IQueryable<object> source, string term, string includeSearch)
        {
            string filterExpresion = String.Empty;
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(includeSearch)) return filterExpresion;

            PropertyInfo[] stringProperties = source.ElementType.GetProperties().ToArray();
            includeSearch.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(entityName =>
                    {
                        bool propertyExists = stringProperties.Any(pi => pi.Name.Equals(entityName.Trim(), StringComparison.InvariantCultureIgnoreCase));
                        if (!propertyExists) return;
                        PropertyInfo propertyChild =
                            stringProperties.Where(x => x.Name.Equals(entityName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                            .First();
                        PropertyInfo[] propertiesChild = propertyChild.PropertyType.GetProperties().Where(x => x.PropertyType == typeof(string)).ToArray();
                        if (!propertiesChild.Any()) { return; }
                        // Build the string expression
                        string filterExpr = string.Join(
                            " || ",
                            propertiesChild.Select(prp => $"{entityName}.{prp.Name}.Contains(@0)")
                        );
                        filterExpresion += " || " + filterExpr;
                    });

            return filterExpresion;
        }

        public IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString, Dictionary<string, string> childFields = null)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);

            if (childFields != null && fieldsString != null)
            {
                string[] fieldsArray = fieldsString.Split(",");
                requiredProperties.ToList().ForEach(property =>
                {
                    if (childFields.ContainsKey(property.Name))
                    {
                        var found = fieldsArray.Where(field => field.Contains($"{property.Name}."));
                        string fieldsFound = String.Join(",", found);
                        if (fieldsFound != null)
                        {
                            childFields[property.Name] = fieldsFound.Replace($"{property.Name}.", string.Empty);
                        }
                    }
                });
            }

            return FetchData(entities, requiredProperties, childFields);
        }

        public IEnumerable<ShapedEntity> ShapeData(IEnumerable<object> entities, string fieldsString, Dictionary<string, string> childFields = null)
        {
            PropertyInfo[] objectProperties = entities.AsQueryable().ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var requiredProperties = GetRequiredProperties(fieldsString, objectProperties);

            if (childFields != null && fieldsString != null)
            {
                string[] fieldsArray = fieldsString.Split(",");
                requiredProperties.ToList().ForEach(property =>
                {
                    if (childFields.ContainsKey(property.Name))
                    {
                        var found = fieldsArray.Where(field => field.Contains($"{property.Name}."));
                        string fieldsFound = String.Join(",", found);
                        if (fieldsFound != null)
                        {
                            childFields[property.Name] = fieldsFound.Replace($"{property.Name}.", string.Empty);
                        }
                    }
                });
            }

            return FetchData(entities, requiredProperties, childFields);
        }

        public ShapedEntity ShapeDataSingle(object entity, string fieldsString)
        {
            var requiredProperties = GetRequiredPropertiesGeneric(fieldsString, entity);

            return FetchDataForEntity(entity, requiredProperties);
        }


        // Private methods

        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString, PropertyInfo[] objectProperties = null)
        {
            var requiredProperties = new List<PropertyInfo>();

            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields)
                {
                    var property = (objectProperties == null) ?
                        Properties.FirstOrDefault(pi => pi.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase)) :
                        objectProperties.FirstOrDefault(pi => pi.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                        continue;

                    requiredProperties.Add(property);
                }
            }
            else
            {
                requiredProperties = (objectProperties == null) ? Properties.ToList() : objectProperties.ToList();
            }

            return requiredProperties;
        }

        private IEnumerable<PropertyInfo> GetRequiredPropertiesGeneric(string fieldsString, object entity)
        {
            var GenericProperties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var requiredProperties = new List<PropertyInfo>();

            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields)
                {
                    var property = GenericProperties.FirstOrDefault(pi => pi.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                        continue;

                    requiredProperties.Add(property);
                }
            }
            else
            {
                requiredProperties = GenericProperties.ToList();
            }

            return requiredProperties;
        }

        private IEnumerable<ShapedEntity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties, Dictionary<string, string> childFields = null)
        {
            var shapedData = new List<ShapedEntity>();

            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties, childFields);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        private IEnumerable<ShapedEntity> FetchData(IEnumerable<object> entities, IEnumerable<PropertyInfo> requiredProperties, Dictionary<string, string> childFields = null)
        {
            var shapedData = new List<ShapedEntity>();

            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties, childFields);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        private ShapedEntity FetchDataForEntity(object entity, IEnumerable<PropertyInfo> requiredProperties, Dictionary<string, string> childFields = null)
        {
            ShapedEntity shapedObject = new();

            foreach (var property in requiredProperties)
            {
                var objectPropertyValue = property.GetValue(entity);

                if (childFields != null && objectPropertyValue != null)
                {
                    if (childFields.ContainsKey(property.Name))
                    {
                        string propertiesChild = childFields[property.Name];
                        if (propertiesChild != null)
                        {
                            ShapedEntity shapedEntity = ShapeDataSingle(objectPropertyValue, childFields[property.Name]);
                            objectPropertyValue = shapedEntity.Entity;
                        }
                    }
                }

                shapedObject.Entity.TryAdd(property.Name, objectPropertyValue);
            }

            // Get Attribute with [Key] Tag
            PropertyInfo objectKeyProperty = entity.GetType().GetProperties().FirstOrDefault(x => x.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));
            if (objectKeyProperty != null)
            {
                shapedObject.Id = objectKeyProperty.GetValue(entity);
                return shapedObject;
            }
            // Get Attribute named as ID
            PropertyInfo objectIDProperty = entity.GetType().GetProperty("ID", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (objectIDProperty != null)
            {
                shapedObject.Id = objectIDProperty.GetValue(entity);
                return shapedObject;
            }
            // Get Attribute named as EntityNameID
            PropertyInfo objectNameIDProperty = entity.GetType().GetProperty($"{entity.GetType().Name}ID", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (objectNameIDProperty != null)
            {
                shapedObject.Id = objectNameIDProperty.GetValue(entity);
                return shapedObject;
            }

            shapedObject.Id = Guid.NewGuid();
            return shapedObject;
        }
    }
}
