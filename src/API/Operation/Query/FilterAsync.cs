using MediatR;
using Radical.Series;
using Radical.Servitizing.Data.Store;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public class FilterAsync<TStore, TEntity, TDto>
    : Filter<TStore, TEntity, ISeries<TDto>>,
        IStreamRequest<TDto>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    public FilterAsync(int offset, int limit, Expression<Func<TEntity, bool>> predicate)
        : base(offset, limit, predicate) { }

    public FilterAsync(
        int offset,
        int limit,
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(offset, limit, predicate, expanders) { }

    public FilterAsync(
        int offset,
        int limit,
        Expression<Func<TEntity, bool>> predicate,
        EntitySortExpression<TEntity> sortTerms,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(offset, limit, predicate, sortTerms, expanders) { }
}
