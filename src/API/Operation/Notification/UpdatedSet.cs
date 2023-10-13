using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Notification;

using Command;
using DTO;
using Entity;
using Servitizing.Data.Store;

public class UpdatedSet<TStore, TEntity, TDto> : NotificationSet<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    public UpdatedSet(UpdateSet<TStore, TEntity, TDto> commands)
        : base(
            commands.PublishMode,
            commands
                .ForOnly(
                    c => c.Entity != null,
                    c => new Updated<TStore, TEntity, TDto>((Update<TStore, TEntity, TDto>)c)
                )
                .ToArray()
        )
    {
        Predicate = commands.Predicate;
        Conditions = commands.Conditions;
    }

    [JsonIgnore]
    public Func<TDto, Expression<Func<TEntity, bool>>>[] Conditions { get; }

    [JsonIgnore]
    public Func<TDto, Expression<Func<TEntity, bool>>> Predicate { get; }
}
