using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class ChangeSet<TStore, TEntity, TDto> : CommandSet<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TDto, Expression<Func<TEntity, bool>>> Predicate { get; }

    public ChangeSet(EventPublishMode publishPattern, TDto input, object key)
        : base(
            CommandMode.Change,
            publishPattern,
            new[] { new Change<TStore, TEntity, TDto>(publishPattern, input, key) }
        ) { }

    public ChangeSet(EventPublishMode publishPattern, TDto[] inputs)
        : base(
            CommandMode.Change,
            publishPattern,
            inputs.Select(c => new Change<TStore, TEntity, TDto>(publishPattern, c, c.Id)).ToArray()
        ) { }

    public ChangeSet(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TDto, Expression<Func<TEntity, bool>>> predicate
    )
        : base(
            CommandMode.Change,
            publishPattern,
            inputs
                .Select(c => new Change<TStore, TEntity, TDto>(publishPattern, c, predicate))
                .ToArray()
        )
    {
        Predicate = predicate;
    }
}
