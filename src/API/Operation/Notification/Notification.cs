using MediatR;
using System.Text.Json;

namespace Radical.Servitizing.Server.API.Operation.Notification;

using Command;
using Entity;
using Event;
using Logging;
using Uniques;

public abstract class Notification<TCommand> : Event, INotification where TCommand : CommandBase
{
    public TCommand Command { get; }

    protected Notification(TCommand command)
    {
        var aggregateTypeFullName = command.Entity.GetProxyEntityFullName();
        var eventTypeFullName = GetType().FullName;

        Command = command;
        Id = (long)Unique.NewKey;
        AggregateId = command.Id;
        AggregateType = aggregateTypeFullName;
        EventType = eventTypeFullName;
        var entity = (Entity)command.Entity;
        OriginKey = entity.OriginKey;
        OriginName = entity.OriginName;
        Modifier = entity.Modifier;
        Modified = entity.Modified;
        Creator = entity.Creator;
        Created = entity.Created;
        PublishStatus = EventPublishStatus.Ready;
        PublishTime = Log.Clock;

        EventData = JsonSerializer.SerializeToUtf8Bytes((CommandBase)command);
    }
}
