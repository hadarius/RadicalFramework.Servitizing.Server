using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Notification;

using Command;
using DTO;
using Entity;
using Servitizing.Data.Store;

public class Updated<TStore, TEntity, TDto> : Notification<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    public Updated(Update<TStore, TEntity, TDto> command) : base(command)
    {
        Predicate = command.Predicate;
        Conditions = command.Conditions;
    }

    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>>[] Conditions { get; }

    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }
}
