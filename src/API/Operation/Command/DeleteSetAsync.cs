using MediatR;
using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class DeleteSetAsync<TStore, TEntity, TDto>
    : DeleteSet<TStore, TEntity, TDto>,
        IStreamRequest<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    public DeleteSetAsync(EventPublishMode publishPattern, object key) : base(publishPattern, key)
    { }

    public DeleteSetAsync(EventPublishMode publishPattern, TDto input, object key)
        : base(publishPattern, input, key) { }

    public DeleteSetAsync(EventPublishMode publishPattern, TDto[] inputs)
        : base(publishPattern, inputs) { }

    public DeleteSetAsync(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(publishPattern, inputs, predicate) { }
}
