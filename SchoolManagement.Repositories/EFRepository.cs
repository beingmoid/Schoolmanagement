using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolManagement.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Repositories
{
    public class EFRepository<TEntity, TKey> : EFRepository<SMEfContext, TEntity, TKey>
          where TEntity : class, IBaseEntity<TKey>, new()
    {
        public EFRepository(RequestScope<SMEfContext> requestScope) : base(requestScope)
        {
        }
    }

    public class EFRepository<Context, TEntity, TKey> : IEFRepository<TEntity, TKey>
    where Context : SMEfContext
    where TEntity : class, IBaseEntity<TKey>, new()
    {
        public EFRepository(RequestScope<Context> requestScope)
        {
            this._requestScope = requestScope;
        }

        private readonly RequestScope<Context> _requestScope;
        protected virtual IQueryable<TEntity> Query
            => this._requestScope.DbContext
            .Set<TEntity>();
         

        private IQueryable<TEntity> ApplyPredicates(params Expression<Func<TEntity, bool>>[] predicates)
        {
            var query = this.Query;

            foreach (var predicate in predicates)
            {
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
            }

            return query;
        }

        #region Get
        public async Task<List<TEntity>> Get() => await this.Query.ToListAsync();

        public async Task<List<TEntity>> Get(params Expression<Func<TEntity, bool>>[] predicates) => await ApplyPredicates(predicates).ToListAsync();

        public async Task<TEntity> GetOne(params Expression<Func<TEntity, bool>>[] predicates) => await ApplyPredicates(predicates).FirstOrDefaultAsync();

        public async Task<TEntity> GetOne(TKey id) => await this.Query.FirstAsync(o => o.Id.Equals(id));

        #endregion

        #region Aggregate
        public async Task<int> Count() => await this.Query.CountAsync();
        public async Task<int> Count(params Expression<Func<TEntity, bool>>[] predicates) => await ApplyPredicates(predicates).CountAsync();

        public async Task<bool> Any() => await this.Query.AnyAsync();
        public async Task<bool> Any(params Expression<Func<TEntity, bool>>[] predicates) => await ApplyPredicates(predicates).AnyAsync();

        async Task<bool> IEFRepository.Any(object id) => await this.Query.AnyAsync(o => o.Id.Equals(id));

        #endregion
        public async Task<bool> Insert(TEntity entity) => (await this._requestScope.DbContext.Set<TEntity>().AddAsync(entity)).State == EntityState.Added;

        public bool Update(TKey id, TEntity entity) => this._requestScope.DbContext.Set<TEntity>().Update(entity).State == EntityState.Modified;

        public async Task<bool> SaveChanges()
        {
            var changeTracker = _requestScope.DbContext.ChangeTracker;
            changeTracker.DetectChanges();

            var markedAsDeleted = changeTracker.Entries<IBaseEntity>().Where(x => x.Entity.IsDeleted);

            foreach (EntityEntry<IBaseEntity> item in markedAsDeleted)
            {
                var navigations = item.Navigations.Where(n => !n.Metadata.IsDependentToPrincipal()).ToArray();

                foreach (var navigationEntry in navigations)
                {
                    if (navigationEntry is CollectionEntry collectionEntry)
                    {
                        foreach (IBaseEntity dependentEntry in collectionEntry.CurrentValue)
                        {
                            dependentEntry.IsDeleted = true;
                        }
                    }
                    else if (navigationEntry.CurrentValue is IBaseEntity dependentEntry)
                    {
                        dependentEntry.IsDeleted = true;
                    }
                }
            }

            return await this._requestScope.DbContext.SaveChangesAsync() > 0;
        } //Kindd

        public async Task<bool> Delete(TKey id)
        {
            var dbEntity = await this.GetOne(id);
            dbEntity.IsDeleted = true;
            return this.Update(id, dbEntity);
        }
    }

    public interface IEFRepository<TEntity, TKey> : IEFRepository<TEntity>
        where TEntity : class, IBaseEntity<TKey>, new()
    {
        Task<TEntity> GetOne(TKey id);
        Task<bool> Insert(TEntity entity);
        bool Update(TKey id, TEntity entity);
        Task<bool> Delete(TKey id);
    }

    public interface IEFRepository<TEntity> : IEFRepository
        where TEntity : class, IBaseEntity, new()
    {
        Task<List<TEntity>> Get();
        Task<List<TEntity>> Get(params Expression<Func<TEntity, bool>>[] predicates);
        Task<TEntity> GetOne(params Expression<Func<TEntity, bool>>[] predicates);
        Task<int> Count(params Expression<Func<TEntity, bool>>[] predicates);
        Task<bool> Any(params Expression<Func<TEntity, bool>>[] predicates);
    }

    public interface IEFRepository
    {
        Task<int> Count();
        Task<bool> Any();
        Task<bool> Any(object id);

        Task<bool> SaveChanges();
    }
}
