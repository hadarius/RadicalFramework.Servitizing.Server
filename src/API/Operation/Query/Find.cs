using Radical.Servitizing.Data.Store;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public class Find<TStore, TEntity, TDto> : Query<TStore, TEntity, TDto>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    public Find(params object[] keys) : base(keys) { }

    public Find(object[] keys, params Expression<Func<TEntity, object>>[] expanders)
        : base(keys, expanders) { }

    public Find(Expression<Func<TEntity, bool>> predicate) : base(predicate) { }

    public Find(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(predicate, expanders) { }
}
