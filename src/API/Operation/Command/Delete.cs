using System;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Radical.Servitizing.Server.API.Operation.Command;

using DTO;
using Entity;
using Event;
using Servitizing.Data.Store;

public class Delete<TStore, TEntity, TDto> : Command<TDto>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    [JsonIgnore]
    public Func<TEntity, Expression<Func<TEntity, bool>>> Predicate { get; }

    public Delete(EventPublishMode publishPattern, TDto input)
        : base(CommandMode.Delete, publishPattern, input) { }

    public Delete(
        EventPublishMode publishPattern,
        TDto input,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(CommandMode.Delete, publishPattern, input)
    {
        Predicate = predicate;
    }

    public Delete(
        EventPublishMode publishPattern,
        Func<TEntity, Expression<Func<TEntity, bool>>> predicate
    ) : base(CommandMode.Delete, publishPattern)
    {
        Predicate = predicate;
    }

    public Delete(EventPublishMode publishPattern, params object[] keys)
        : base(CommandMode.Delete, publishPattern, keys) { }
}
