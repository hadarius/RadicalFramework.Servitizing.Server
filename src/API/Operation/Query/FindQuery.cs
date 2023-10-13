using System.Linq.Expressions;
using Radical.Uniques;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Radical.Servitizing.Data.Store;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public class FindQuery<TStore, TEntity, TDto> : Query<TStore, TEntity, IQueryable<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
    where TDto : class, IUnique
{
    public FindQuery(params object[] keys) : base(keys) { }

    public FindQuery(object[] keys, params Expression<Func<TEntity, object>>[] expanders)
        : base(keys, expanders) { }

    public FindQuery(Expression<Func<TEntity, bool>> predicate) : base(predicate) { }

    public FindQuery(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(predicate, expanders) { }
}
