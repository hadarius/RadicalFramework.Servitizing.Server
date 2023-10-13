using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class Create<TStore, TEntity, TDto> : Command<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    public Create(EventPublishMode publishPattern, TDto input)
        : base(CommandMode.Create, publishPattern, input)
    {
        input.AutoId();
    }

    public Create(EventPublishMode publishPattern, TDto input, object key)
        : base(CommandMode.Create, publishPattern, input)
    {
        input.SetId(key);
    }

    public Create(
        EventPublishMode publishPattern,
        TDto input,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(CommandMode.Create, publishPattern, input)
    {
        input.AutoId();
        Predicate = predicate;
    }
}
