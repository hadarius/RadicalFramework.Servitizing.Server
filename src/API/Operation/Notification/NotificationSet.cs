using MediatR;

namespace Radical.Servitizing.Server.API.Operation.Notification;

using Command;
using Event;
using Series;

public abstract class NotificationSet<TCommand> : Catalog<Notification<TCommand>>, INotification
    where TCommand : CommandBase
{
    public EventPublishMode PublishMode { get; set; }

    protected NotificationSet(EventPublishMode publishPattern, Notification<TCommand>[] commands)
        : base(commands)
    {
        PublishMode = publishPattern;
    }
}
