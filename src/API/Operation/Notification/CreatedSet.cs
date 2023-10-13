using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Notification;
using Command;
using DTO;
using Entity;
using Servitizing.Data.Store;

public class CreatedSet<TStore, TEntity, TDto> : NotificationSet<Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    public CreatedSet(CreateSet<TStore, TEntity, TDto> commands)
        : base(
            commands.PublishMode,
            commands
                .ForOnly(
                    c => c.Entity != null,
                    c => new Created<TStore, TEntity, TDto>((Create<TStore, TEntity, TDto>)c)
                )
                .ToArray()
        )
    {
        Predicate = commands.Predicate;
    }
}
