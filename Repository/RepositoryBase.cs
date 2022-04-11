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
        private PropertyInfo[] Properties { get; set; }

        public RepositoryBase(RepositoryContext repositoryContext)
        {
            this.RepositoryContext = repositoryContext;
            this.Set = this.RepositoryContext.Set<T>();
            Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        #region Exists
        public virtual bool Exists(Expression<Func<T, bool>> expression) => Set.Any(expression);

        public virtual Task<bool> ExistsAsync(Expression<Func<T, bool>> expression) => Task.Run(() => Exists(expression));

        #endregion Exists

        #region Delete

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

        #endregion Delete

        #region Get

        public IQueryable<T> GetAll() => Set.AsNoTracking();

        public IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> expression)
            => Set.Where(expression).AsNoTracking();

        public Task<IQueryable<T>> GetAllAsync() => Task.Run(() => Set.AsNoTracking());

        public Task<IQueryable<T>> GetAllByConditionAsync(Expression<Func<T, bool>> expression)
            => Task.Run(() => Set.Where(expression).AsNoTracking());

        public virtual T GetByKey(params object[] id) =>
            Set.Find(id);

        public virtual Task<T> GetByKeyAsync(params object[] id) =>
            Set.FindAsync(id).AsTask();

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

        #endregion Get

        #region ShapeData

        public virtual PagedList<ShapedEntity> GetPaged(
          Expression<Func<T, bool>> filter = null,
          string orderBy = null,
          string includeProperties = null,
          string onlyFields = null,
          string searchTerm = null,
          string includeSearch = null,
          int page = 0,
          int pageSize = 10)
        {
            Dictionary<string, string> mapChildFields = new();
            IQueryable<T> query = Set;
            // Filter
            if (filter != null) query = query.Where(filter);
            // Search
            if (searchTerm != null) query = SearchText(query, searchTerm, includeSearch);
            // EF Include
            if (includeProperties != null && includeProperties != string.Empty)
            {
                List<string> queryIncludeList = new();
                includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(field =>
                    {
                        string fieldTrim = field.Trim();
                        // If field is not a child property
                        PropertyInfo childFound = Properties.FirstOrDefault(p => p.Name.Equals(fieldTrim, StringComparison.InvariantCultureIgnoreCase));
                        if (childFound == null) return;
                        mapChildFields.Add(childFound.Name, null);
                        queryIncludeList.Add(childFound.Name);

                    });

                // EF Include related entity
                if (queryIncludeList.Any())
                {
                    query = query.Include(string.Join(',', queryIncludeList));
                }
            }
            // Sort
            if (!string.IsNullOrWhiteSpace(orderBy)) query = ApplySort(query, orderBy);
            // Shape Data
            var shaped = ShapeData(query, onlyFields, childFields: mapChildFields);
            return PagedList<ShapedEntity>.ToPagedList(shaped,
                page,
                pageSize);
        }

        public virtual PagedList<ShapedEntity> GetQueryPaged(
          IQueryable<object> baseQuery,
          string includeProperties = null,
          string onlyFields = null,
          string searchTerm = null,
          string includeSearch = null,
          int page = 0,
          int pageSize = 10)
        {
            Dictionary<string, string> mapChildFields = new();
            IQueryable<object> query = baseQuery;
            IEnumerable<PropertyInfo> propertiesQuery = baseQuery?.ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

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

            IEnumerable<ShapedEntity> shaped = ShapeData(query, onlyFields, childFields: mapChildFields);
            return PagedList<ShapedEntity>.ToPagedList(shaped,
                page,
                pageSize);
        }


        public virtual Task<PagedList<ShapedEntity>> GetQueryPagedAsync(
            IQueryable<object> baseQuery,
            string includeProperties = null,
            string onlyFields = null,
            string searchTerm = null,
            string includeSearch = null,
            int page = 0,
            int pageSize = 10) =>
            Task.Run(() => GetQueryPaged(baseQuery, includeProperties, onlyFields, searchTerm, includeSearch, page, pageSize));

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

        #endregion ShapeData

        #region Insert

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

        #endregion Insert

        #region Update

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

        public virtual void Update(T entity, string onlyProperties)
        {
            Set.Attach(entity);
            if (onlyProperties != null)
            {
                onlyProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(field =>
                    {
                        string fieldTrim = field.Trim();
                        // If field exists
                        PropertyInfo propertyFound = Properties.FirstOrDefault(p => p.Name.Equals(fieldTrim, StringComparison.InvariantCultureIgnoreCase));
                        if (propertyFound == null) return;
                        RepositoryContext.Entry<T>(entity).Property(propertyFound.Name).IsModified = true;

                    });
            }

            RepositoryContext.Entry<T>(entity).State = EntityState.Modified;

        }

        public virtual Task UpdateAsync(T entity, params Expression<Func<T, object>>[] onlyProperties) =>
            Task.Run(() => this.Update(entity, onlyProperties));

        public virtual Task UpdateAsync(T entity, string onlyProperties) =>
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

        #endregion Update

        #region BulkExtensionsAsync
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

        #endregion BulkExtensionsAsync

        #region Helpers
        public IQueryable<T> ApplySort(IQueryable<T> entities, string orderByQueryString)
        {
            if (!entities.Any())
                return entities;

            if (string.IsNullOrWhiteSpace(orderByQueryString))
            {
                return entities;
            }

            string[] orderParams = orderByQueryString.Trim().Split(',');
            StringBuilder orderQueryBuilder = new();

            foreach (var param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                string propertyFromQueryName = param.Split(" ")[0];
                PropertyInfo objectProperty = Properties.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));
                if (objectProperty == null)
                    continue;

                string sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
                orderQueryBuilder.Append($"{objectProperty.Name} {sortingOrder}, ");
            }

            string orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
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

        public IQueryable<T> SearchText(IQueryable<T> source, string term, string includeSearch)
        {
            if (string.IsNullOrEmpty(term)) { return source; }

            if (int.TryParse(term.Trim(), out int ex))
            {
                // Get all the int property names on this specific type.
                PropertyInfo[] integerProperties = Properties
                    .Where(x => x.PropertyType == typeof(int))
                    .ToArray();

                if (!integerProperties.Any() && includeSearch == null) { return source; }

                string filterExprInt = string.Join(" || ", integerProperties.Select(prp => $"{prp.Name} == (@0)"));

                return source.Where(filterExprInt, term);

            }

            // Get all the string property names on this specific type.
            PropertyInfo[] stringProperties = Properties
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
                if (string.IsNullOrEmpty(filterExpr))
                    filterExpr += SearchTextInclude(stringProperties, term, includeSearch).Remove(0, 4);
            }

            return source.Where(filterExpr, term);
        }
        public IQueryable<object> SearchText(IQueryable<object> source, string term, string includeSearch)
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

        public string SearchTextInclude(PropertyInfo[] stringProperties, string term, string includeSearch)
        {
            string filterExpresion = string.Empty;
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(includeSearch)) return filterExpresion;

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

        public string SearchTextInclude(IQueryable<object> source, string term, string includeSearch)
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
            IEnumerable<PropertyInfo> requiredProperties = GetRequiredProperties(fieldsString);

            if (childFields != null && fieldsString != null)
            {
                string[] fieldsArray = fieldsString.Split(",");
                requiredProperties.ToList().ForEach(property =>
                {
                    if (childFields.ContainsKey(property.Name))
                    {
                        IEnumerable<string> found = fieldsArray.Where(field => field.Contains($"{property.Name}.", StringComparison.InvariantCultureIgnoreCase));
                        if (found.Any())
                        {
                            string fieldsFound = String.Join(',', found);
                            childFields[property.Name] = fieldsFound.Replace($"{property.Name}.", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                });
            }

            return FetchData(entities, requiredProperties, childFields);
        }

        public IEnumerable<ShapedEntity> ShapeData(IEnumerable<object> entities, string fieldsString, Dictionary<string, string> childFields = null)
        {
            PropertyInfo[] objectProperties = entities.AsQueryable().ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            IEnumerable<PropertyInfo> requiredProperties = GetRequiredProperties(fieldsString, objectProperties);

            if (childFields != null && fieldsString != null)
            {
                string[] fieldsArray = fieldsString.Split(",");
                requiredProperties.ToList().ForEach(property =>
                {
                    if (childFields.ContainsKey(property.Name))
                    {
                        IEnumerable<string> found = fieldsArray.Where(field => field.Contains($"{property.Name}.", StringComparison.InvariantCultureIgnoreCase));
                        string fieldsFound = String.Join(",", found);
                        if (fieldsFound != null)
                        {
                            childFields[property.Name] = fieldsFound.Replace($"{property.Name}.", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                });
            }

            return FetchData(entities, requiredProperties, childFields);
        }

        public ShapedEntity ShapeDataSingle(object entity, string fieldsString)
        {
            IEnumerable<PropertyInfo> requiredProperties = GetRequiredPropertiesGeneric(fieldsString, entity);

            return FetchDataForEntity(entity, requiredProperties);
        }

        #endregion Helpers

        #region Utils
        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString, PropertyInfo[] objectProperties = null)
        {
            List<PropertyInfo> requiredProperties = new();

            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                string[] fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields)
                {
                    PropertyInfo property = (objectProperties == null) ?
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
            PropertyInfo[] genericProperties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<PropertyInfo> requiredProperties = new ();

            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                string[] fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields)
                {
                    var property = genericProperties.FirstOrDefault(pi => pi.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                        continue;

                    requiredProperties.Add(property);
                }
            }
            else
            {
                requiredProperties = genericProperties.ToList();
            }

            return requiredProperties;
        }

        private IEnumerable<ShapedEntity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties, Dictionary<string, string> childFields = null)
        {
            List<ShapedEntity> shapedData = new();

            foreach (var entity in entities)
            {
                ShapedEntity shapedObject = FetchDataForEntity(entity, requiredProperties, childFields);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        private IEnumerable<ShapedEntity> FetchData(IEnumerable<object> entities, IEnumerable<PropertyInfo> requiredProperties, Dictionary<string, string> childFields = null)
        {
            List<ShapedEntity> shapedData = new ();

            foreach (var entity in entities)
            {
                ShapedEntity shapedObject = FetchDataForEntity(entity, requiredProperties, childFields);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        private ShapedEntity FetchDataForEntity(object entity, IEnumerable<PropertyInfo> requiredProperties, Dictionary<string, string> childFields = null)
        {
            ShapedEntity shapedObject = new();

            foreach (var property in requiredProperties)
            {
                object objectPropertyValue = property.GetValue(entity);

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

        #endregion Utils
    }
}
