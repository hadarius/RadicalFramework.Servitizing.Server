using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class Update<TStore, TEntity, TDto> : Command<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>>[] Conditions { get; }

    public Update(EventPublishMode publishPattern, TDto input, params object[] keys)
        : base(CommandMode.Update, publishPattern, input, keys) { }

    public Update(
        EventPublishMode publishPattern,
        TDto input,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(CommandMode.Update, publishPattern, input)
    {
        Predicate = predicate;
    }

    public Update(
        EventPublishMode publishPattern,
        TDto input,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate,
        params Func<TEntity, Expression<Func<TEntity, bool>>>[] conditions
    ) : base(CommandMode.Update, publishPattern, input)
    {
        Predicate = predicate;
        Conditions = conditions;
    }
}
