using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Notification;

using Command;
using DTO;
using Entity;
using Servitizing.Data.Store;

public class UpsertedSet<TStore, TEntity, TDto> : NotificationSet<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>>[] Conditions { get; }

    public UpsertedSet(UpsertSet<TStore, TEntity, TDto> commands)
        : base(
            commands.PublishMode,
            commands
                .ForOnly(
                    c => c.Entity != null,
                    c => new Upserted<TStore, TEntity, TDto>((Upsert<TStore, TEntity, TDto>)c)
                )
                .ToArray()
        )
    {
        Conditions = commands.Conditions;
        Predicate = commands.Predicate;
    }
}
