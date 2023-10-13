using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Notification;

using Command;
using DTO;
using Entity;
using Servitizing.Data.Store;

public class Upserted<TStore, TEntity, TDto> : Notification<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>>[] Conditions { get; }

    public Upserted(Upsert<TStore, TEntity, TDto> command) : base(command)
    {
        Conditions = command.Conditions;
        Predicate = command.Predicate;
    }
}
