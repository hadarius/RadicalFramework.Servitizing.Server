using MediatR;
using System;
using System.Linq.Expressions;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class ChangeSetAsync<TStore, TEntity, TDto>
    : ChangeSet<TStore, TEntity, TDto>,
        IStreamRequest<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    public ChangeSetAsync(EventPublishMode publishPattern, TDto input, object key)
        : base(publishPattern, input, key) { }

    public ChangeSetAsync(EventPublishMode publishPattern, TDto[] inputs)
        : base(publishPattern, inputs) { }

    public ChangeSetAsync(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TDto, Expression<Func<TEntity, bool>>> predicate
    ) : base(publishPattern, inputs, predicate) { }
}
