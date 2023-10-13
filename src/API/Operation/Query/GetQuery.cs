using Radical.Servitizing.Data.Store;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public class GetQuery<TStore, TEntity, TDto> : Query<TStore, TEntity, IQueryable<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    public GetQuery(params Expression<Func<TEntity, object>>[] expanders) : base(expanders) { }

    public GetQuery(
        EntitySortExpression<TEntity> sortTerms,
        params Expression<Func<TEntity, object>>[] expanders
    ) : base(sortTerms, expanders) { }
}
