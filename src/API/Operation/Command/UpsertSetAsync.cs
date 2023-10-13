using MediatR;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class UpsertSetAsync<TStore, TEntity, TDto>
    : UpsertSet<TStore, TEntity, TDto>,
        IStreamRequest<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    public UpsertSetAsync(EventPublishMode publishPattern, TDto input, object key)
        : base(publishPattern, input, key) { }

    public UpsertSetAsync(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(publishPattern, inputs, predicate) { }

    public UpsertSetAsync(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate,
        params Func<TEntity, Expression<Func<TEntity, bool>>>[] conditions
    ) : base(publishPattern, inputs, predicate, conditions) { }
}
