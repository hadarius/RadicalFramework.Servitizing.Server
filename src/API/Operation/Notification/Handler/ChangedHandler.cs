using FluentValidation.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Operation.Notification.Handler;

using DTO;
using Entity;
using Event;
using Logging;
using Repository;
using Servitizing.Data.Store;

public class ChangedHandler<TStore, TEntity, TCommand>
    : INotificationHandler<Changed<TStore, TEntity, TCommand>>
    where TEntity : Entity
    where TCommand : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<Event> _eventStore;
    protected readonly IStoreRepository<TEntity> _repository;

    public ChangedHandler() { }

    public ChangedHandler(
        IStoreRepository<IReportStore, TEntity> repository,
        IStoreRepository<IEventStore, Event> eventStore
    )
    {
        _repository = repository;
        _eventStore = eventStore;
    }

    public virtual Task Handle(
        Changed<TStore, TEntity, TCommand> request,
        CancellationToken cancellationToken
    )
    {
        return Task.Run(
            async () =>
            {
                try
                {
                    if (_eventStore.Add(request) == null)
                        throw new Exception(
                            $"{$"{GetType().Name} for entity "}{$"{typeof(TEntity).Name} unable add event"}"
                        );

                    if (request.Command.PublishMode == EventPublishMode.PropagateCommand)
                    {
                        TEntity entity;
                        if (request.Command.Keys != null)
                            entity = await _repository.PatchBy(
                                request.Command.Data,
                                request.Command.Keys
                            );
                        else
                            entity = await _repository.PatchBy(
                                request.Command.Data,
                                request.Predicate
                            );

                        if (entity == null)
                            throw new Exception(
                                $"{$"{GetType().Name} for entity "}{$"{typeof(TEntity).Name} unable change report"}"
                            );

                        request.PublishStatus = EventPublishStatus.Complete;
                    }
                }
                catch (Exception ex)
                {
                    request.Command.Result.Errors.Add(
                        new ValidationFailure(string.Empty, ex.Message)
                    );
                    this.Failure<Domainlog>(ex.Message, request.Command.ErrorMessages, ex);
                    request.PublishStatus = EventPublishStatus.Error;
                }
            },
            cancellationToken
        );
    }
}
