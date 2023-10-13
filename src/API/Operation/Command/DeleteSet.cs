using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class DeleteSet<TStore, TEntity, TDto> : CommandSet<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    public DeleteSet(EventPublishMode publishPattern, object key)
        : base(
            CommandMode.Create,
            publishPattern,
            new[] { new Delete<TStore, TEntity, TDto>(publishPattern, key) }
        ) { }

    public DeleteSet(EventPublishMode publishPattern, TDto input, object key)
        : base(
            CommandMode.Create,
            publishPattern,
            new[] { new Delete<TStore, TEntity, TDto>(publishPattern, input, key) }
        ) { }

    public DeleteSet(EventPublishMode publishPattern, TDto[] inputs)
        : base(
            CommandMode.Delete,
            publishPattern,
            inputs
                .Select(input => new Delete<TStore, TEntity, TDto>(publishPattern, input))
                .ToArray()
        ) { }

    public DeleteSet(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    )
        : base(
            CommandMode.Delete,
            publishPattern,
            inputs
                .Select(
                    input => new Delete<TStore, TEntity, TDto>(publishPattern, input, predicate)
                )
                .ToArray()
        )
    {
        Predicate = predicate;
    }
}
