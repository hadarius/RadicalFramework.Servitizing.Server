using Radical.Series;
using Radical.Servitizing.Data.Store;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public class Filter<TStore, TEntity, TDto> : Query<TStore, TEntity, ISeries<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    public Filter(int offset, int limit, Expression<Func<TEntity, bool>> predicate)
        : base(predicate)
    {
        Offset = offset;
        Limit = limit;
    }

    public Filter(
        int offset,
        int limit,
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(predicate, expanders)
    {
        Offset = offset;
        Limit = limit;
    }

    public Filter(
        int offset,
        int limit,
        Expression<Func<TEntity, bool>> predicate,
        EntitySortExpression<TEntity> sortTerms,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(predicate, sortTerms, expanders)
    {
        Offset = offset;
        Limit = limit;
    }
}
