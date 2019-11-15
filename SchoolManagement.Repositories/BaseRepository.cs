using SchoolManagement.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Repositories
{
    public interface IBaseRepository<TEntity, TKey>
         where TEntity : class, IBaseEntity<TKey>, new()
    {
        #region Get
        Task<List<TEntity>> Get();
        Task<List<TEntity>> Get(params Expression<Func<TEntity, bool>>[] predicates);
        Task<TEntity> GetOne(params Expression<Func<TEntity, bool>>[] predicates);
        Task<TEntity> GetOne(TKey id);
        #endregion

        Task<bool> Insert(TEntity entity);

        Task<bool> SaveChanges();
        bool Update(TKey id, TEntity entity);
        Task<bool> Delete(TKey id);
    }
}
