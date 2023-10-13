using MediatR;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class CreateSetAsync<TStore, TEntity, TDto>
    : CreateSet<TStore, TEntity, TDto>,
        IStreamRequest<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    public CreateSetAsync(EventPublishMode publishPattern, TDto input, object key)
        : base(publishPattern, input, key) { }

    public CreateSetAsync(EventPublishMode publishPattern, TDto[] inputs)
        : base(publishPattern, inputs) { }

    public CreateSetAsync(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(publishPattern, inputs, predicate) { }
}
