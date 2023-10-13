using MediatR;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class UpdateSetAsync<TStore, TEntity, TDto>
    : UpdateSet<TStore, TEntity, TDto>,
        IStreamRequest<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    public UpdateSetAsync(EventPublishMode publishPattern, TDto input, object key)
        : base(publishPattern, input, key) { }

    public UpdateSetAsync(EventPublishMode publishPattern, TDto[] inputs)
        : base(publishPattern, inputs) { }

    public UpdateSetAsync(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TDto, Expression<Func<TEntity, bool>>> predicate
    ) : base(publishPattern, inputs, predicate) { }

    public UpdateSetAsync(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TDto, Expression<Func<TEntity, bool>>> predicate,
        params Func<TDto, Expression<Func<TEntity, bool>>>[] conditions
    ) : base(publishPattern, inputs, predicate, conditions) { }
}
