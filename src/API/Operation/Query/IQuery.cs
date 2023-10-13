using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Query;

using Entity;

public interface IQuery<TEntity> : IOperation where TEntity : Entity
{
    Expression<Func<TEntity, object>>[] Expanders { get; }
    Expression<Func<TEntity, bool>> Predicate { get; }
    EntitySortExpression<TEntity> Sort { get; }
}