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
using Series;
using Servitizing.Data.Store;

public class UpsertedSetHandler<TStore, TEntity, TDto>
    : INotificationHandler<UpsertedSet<TStore, TEntity, TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IStoreRepository<Event> _eventStore;

    public UpsertedSetHandler() { }

    public UpsertedSetHandler(
        IStoreRepository<IReportStore, TEntity> repository,
        IStoreRepository<IEventStore, Event> eventStore
    )
    {
        _repository = repository;
        _eventStore = eventStore;
    }

    public virtual Task Handle(
        UpsertedSet<TStore, TEntity, TDto> request,
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
                        ISeries<TEntity> entities;
                        if (request.Conditions != null)
                            entities = _repository
                                .PutBy(
                                    request.Select(d => d.Command.Data),
                                    request.Predicate,
                                    request.Conditions
                                )
                                .ToCatalog();
                        else
                            entities = _repository
                                .PutBy(request.Select(d => d.Command.Data), request.Predicate)
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
