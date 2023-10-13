using Radical.Series;
using Radical.Servitizing.Data.Store;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public class Get<TStore, TEntity, TDto> : Query<TStore, TEntity, ISeries<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    public Get(int offset, int limit, params Expression<Func<TEntity, object>>[] expanders)
        : base(expanders)
    {
        Offset = offset;
        Limit = limit;
    }

    public Get(
        int offset,
        int limit,
        EntitySortExpression<TEntity> sortTerms,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(sortTerms, expanders)
    {
        Offset = offset;
        Limit = limit;
    }
}
