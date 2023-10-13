using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class Change<TStore, TEntity, TDto> : Command<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TDto, Expression<Func<TEntity, bool>>> Predicate { get; }

    public Change(EventPublishMode publishMode, TDto input, params object[] keys)
        : base(CommandMode.Change, publishMode, input, keys) { }

    public Change(
        EventPublishMode publishMode,
        TDto input,
        Func<TDto, Expression<Func<TEntity, bool>>> predicate
    ) : base(CommandMode.Change, publishMode, input)
    {
        Predicate = predicate;
    }
}
