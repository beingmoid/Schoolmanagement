using SchoolManagement.Common;
using SchoolManagement.Repositories;
using SchoolManagement.Services.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Services
{
    public class BaseService : IBaseService
    {

    }

    public class BaseService<TEntity, TKey> : BaseService, IBaseService<TEntity, TKey>
        where TEntity : class, IBaseEntity<TKey>, new()
    {
        private static List<IPropertyValidation<TEntity>> _propertyValidations;
        protected IEFRepository<TEntity, TKey> Repository { get; }
        private readonly RequestScope _scopeContext;
        protected readonly IEnumerable<Expression<Func<TEntity, IEnumerable<IBaseEntity>>>> _childExpressions;

        public BaseService(RequestScope scopeContext, IEFRepository<TEntity, TKey> repository)
        {
            this.Repository = repository;
            this._scopeContext = scopeContext;

            if (_propertyValidations == null)
            {
                _propertyValidations = new List<IPropertyValidation<TEntity>>();
                Validation();
            }
        }

        public BaseService(RequestScope scopeContext, IEFRepository<TEntity, TKey> repository, params Expression<Func<TEntity, IEnumerable<IBaseEntity>>>[] childExpressions)
            : this(scopeContext, repository)
        {
            this._childExpressions = childExpressions;
        }


        protected virtual void Validation()
        {

        }

        protected PropertyValidation<TEntity, TProperty> Validate<TProperty>(Expression<Func<TEntity, TProperty>> property, string caption = null)
        {
            var result = new PropertyValidation<TEntity, TProperty>(_scopeContext.ServiceProvider, Repository, property, ValidationType.None, caption ?? (property.Body as MemberExpression).Member.Name);
            _propertyValidations.Add(result);
            return result;
        }

        private async Task Validate(TEntity entity)
        {
            List<string> errors = new List<string>();
            await Task.WhenAll(_propertyValidations
                .Select(async o =>
                {
                    var validateResult = await o.Validate(entity);

                    if (!validateResult.valid)
                    {
                        errors.Add(validateResult.error);
                    }
                }));

            if (errors.Count > 0)
            {
                throw new ServiceException(errors.ToArray());
            }
        }

        #region Get
        public async Task<List<TEntity>> Get()
        {
            return await this.Repository.Get();
        }

        public async Task<List<TEntity>> Get(params Expression<Func<TEntity, bool>>[] predicates)
        {
            return await Repository.Get(predicates);
        }

        public async Task<TEntity> GetOne(params Expression<Func<TEntity, bool>>[] predicates)
        {
            return await Repository.GetOne(predicates);
        }

        #endregion

        public async Task<(TEntity Entity, bool Success)> Insert(TEntity entity)
        {
            var newEntity = new TEntity();
            this.Map(entity, newEntity);
            await this.Validate(newEntity);

            return await Repository.Insert(newEntity) & await Repository.SaveChanges() ? (newEntity, true) : (null, false);
        }

        public async Task<(TEntity Entity, bool Success)> Update(TKey id, TEntity entity)
        {
            var dbEntity = await this.Repository.GetOne(id);
            this.Map(entity, dbEntity);

            return Repository.Update(id, dbEntity) & await Repository.SaveChanges() ? (await this.Repository.GetOne(id), true) : (null, false);
        }

        public async Task<bool> Delete(TKey id)
        {
            return await this.Repository.Delete(id) & await this.Repository.SaveChanges();
        }

        protected void Map(TEntity source, TEntity dest)
        {
            _scopeContext.Mapper.Map(source, dest);

            if (this._childExpressions is null) return;
            foreach (var child in this._childExpressions)
            {
                var func = child.Compile();
                var destLists = func(dest);
                var sourceLists = func(source);
                var toDelete = new List<IBaseEntity>();
                // Delete
                foreach (IBaseEntity destChild in destLists)
                {
                    if (!sourceLists.OfType<IBaseEntity>().Any(o => o.Id.Equals(destChild.Id)))
                    {
                        // Remove
                        destChild.IsDeleted = true;
                    }
                }

                // Add/Modify
                foreach (IBaseEntity sourceChild in sourceLists)
                {
                    IBaseEntity destChild;
                    destChild = destLists.OfType<IBaseEntity>().FirstOrDefault(o => !o.IsNew && o.Id.Equals(sourceChild.Id));

                    if (destChild == null)
                    {
                        destChild = Activator.CreateInstance(destLists.GetType().GetGenericArguments()[0]) as IBaseEntity;
                        destLists.GetType().GetMethod("Add").Invoke(destLists, new[] { destChild });
                    }

                    _scopeContext.Mapper.Map(sourceChild, destChild);
                }
            }
        }
    }

    public interface IBaseService
    {

    }

    public interface IBaseService<TEntity, TKey> : IBaseService
        where TEntity : class, IBaseEntity<TKey>, new()
    {
        #region Get
        Task<List<TEntity>> Get();
        Task<List<TEntity>> Get(params Expression<Func<TEntity, bool>>[] predicates);
        Task<TEntity> GetOne(params Expression<Func<TEntity, bool>>[] predicates);

        #endregion

        Task<(TEntity Entity, bool Success)> Insert(TEntity entity);
        Task<(TEntity Entity, bool Success)> Update(TKey id, TEntity entity);
        Task<bool> Delete(TKey id);
    }
}
