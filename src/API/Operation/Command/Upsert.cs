using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class Upsert<TStore, TEntity, TDto> : Command<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>>[] Conditions { get; }

    public Upsert(
        EventPublishMode publishPattern,
        TDto input,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(CommandMode.Upsert, publishPattern, input)
    {
        Predicate = predicate;
    }

    public Upsert(
        EventPublishMode publishPattern,
        TDto input,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate,
        params Func<TEntity, Expression<Func<TEntity, bool>>>[] conditions
    ) : base(CommandMode.Upsert, publishPattern, input)
    {
        Predicate = predicate;
        Conditions = conditions;
    }
}
