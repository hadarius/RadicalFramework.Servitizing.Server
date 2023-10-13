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

public class CreatedHandler<TStore, TEntity, TDto>
    : INotificationHandler<Created<TStore, TEntity, TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<Event> _eventStore;
    protected readonly IStoreRepository<TEntity> _repository;

    public CreatedHandler() { }

    public CreatedHandler(
        IStoreRepository<IReportStore, TEntity> repository,
        IStoreRepository<IEventStore, Event> eventStore
    )
    {
        _repository = repository;
        _eventStore = eventStore;
    }

    public virtual Task Handle(
        Created<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        return Task.Run(
            () =>
            {
                try
                {
                    if (_eventStore.Add(request) == null)
                        throw new Exception(
                            $"{$"{GetType().Name} "}{$"for entity {typeof(TEntity).Name} unable add event"}"
                        );

                    if (request.Command.PublishMode == EventPublishMode.PropagateCommand)
                    {
                        if (
                            _repository.Add((TEntity)request.Command.Entity, request.Predicate)
                            == null
                        )
                            throw new Exception(
                                $"{$"{GetType().Name} "}{$"for entity {typeof(TEntity).Name} unable create report"}"
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
