using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Operation.Notification.Handler;

using DTO;
using Entity;
using Event;
using Logging;
using Repository;
using Servitizing.Data.Store;

public class CreatedSetHandler<TStore, TEntity, TDto>
    : INotificationHandler<CreatedSet<TStore, TEntity, TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IStoreRepository<Event> _eventStore;

    public CreatedSetHandler() { }

    public CreatedSetHandler(
        IStoreRepository<IReportStore, TEntity> repository,
        IStoreRepository<IEventStore, Event> eventStore
    )
    {
        _repository = repository;
        _eventStore = eventStore;
    }

    public virtual Task Handle(
        CreatedSet<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        return Task.Run(
            () =>
            {
                try
                {
                    request.ForOnly(
                        d => !d.Command.IsValid,
                        d =>
                        {
                            request.Remove(d);
                        }
                    );

                    _eventStore.AddAsync(request).ConfigureAwait(true);

                    if (request.PublishMode == EventPublishMode.PropagateCommand)
                    {
                        var entities = _repository
                            .Add(
                                request.Select(d => d.Command.Entity).Cast<TEntity>(),
                                request.Predicate
                            )
                            .ToCatalog();

                        request.ForEach(
                            (r) =>
                            {
                                _ = entities.ContainsKey(r.AggregateId)
                                    ? r.PublishStatus = EventPublishStatus.Complete
                                    : r.PublishStatus = EventPublishStatus.Uncomplete;
                            }
                        );
                    }
                }
                catch (Exception ex)
                {
                    this.Failure<Domainlog>(
                        ex.Message,
                        request.Select(r => r.Command.ErrorMessages).ToArray(),
                        ex
                    );
                    request.ForEach((r) => r.PublishStatus = EventPublishStatus.Error);
                }
            },
            cancellationToken
        );
    }
}
