using MediatR;
using Radical.Servitizing.Data.Store;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public class GetAsync<TStore, TEntity, TDto> : Get<TStore, TEntity, TDto>, IStreamRequest<TDto>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    public GetAsync(int offset, int limit, params Expression<Func<TEntity, object>>[] expanders)
        : base(offset, limit, expanders) { }

    public GetAsync(
        int offset,
        int limit,
        EntitySortExpression<TEntity> sortTerms,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(offset, limit, sortTerms, expanders) { }
}
