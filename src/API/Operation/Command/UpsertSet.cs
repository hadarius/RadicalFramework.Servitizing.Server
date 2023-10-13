using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class UpsertSet<TStore, TEntity, TDto> : CommandSet<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>>[] Conditions { get; }

    public UpsertSet(EventPublishMode publishPattern, TDto input, object key)
        : base(
            CommandMode.Change,
            publishPattern,
            new[]
            {
                new Upsert<TStore, TEntity, TDto>(
                    publishPattern,
                    input,
                    e => e => e.Id == (long)key
                )
            }
        ) { }

    public UpsertSet(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    )
        : base(
            CommandMode.Upsert,
            publishPattern,
            inputs
                .Select(
                    input => new Upsert<TStore, TEntity, TDto>(publishPattern, input, predicate)
                )
                .ToArray()
        )
    {
        Predicate = predicate;
    }

    public UpsertSet(
        EventPublishMode publishPattern,
        TDto[] inputs,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate,
        params Func<TEntity, Expression<Func<TEntity, bool>>>[] conditions
    )
        : base(
            CommandMode.Upsert,
            publishPattern,
            inputs
                .Select(
                    input =>
                        new Upsert<TStore, TEntity, TDto>(
                            publishPattern,
                            input,
                            predicate,
                            conditions
                        )
                )
                .ToArray()
        )
    {
        Predicate = predicate;
        Conditions = conditions;
    }
}
