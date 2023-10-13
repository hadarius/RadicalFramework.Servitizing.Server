using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Radical.Servitizing.Server.API.Operation.Command.Handler;

using DTO;
using Entity;
using Logging;
using Notification;
using Repository;
using Servitizing.Data.Store;

public class DeleteSetAsyncHandler<TStore, TEntity, TDto>
    : IStreamRequestHandler<DeleteSetAsync<TStore, TEntity, TDto>, Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IServicer _servicer;

    public DeleteSetAsyncHandler(IServicer servicer, IStoreRepository<TStore, TEntity> repository)
    {
        _servicer = servicer;
        _repository = repository;
    }

    public virtual IAsyncEnumerable<Command<TDto>> Handle(
        DeleteSetAsync<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IAsyncEnumerable<TEntity> entities;
            if (request.Predicate == null)
                entities = _repository.DeleteByAsync(request.ForOnly(d => d.IsValid, d => d.Data));
            else
                entities = _repository.DeleteByAsync(
                    request.ForOnly(d => d.IsValid, d => d.Data),
                    request.Predicate
                );

            var response = entities.ForEachAsync(
                (e) =>
                {
                    var r = request[e.Id];
                    r.Entity = e;
                    return r;
                }
            );

            _ = _servicer
                .Publish(new DeletedSet<TStore, TEntity, TDto>(request))
                .ConfigureAwait(false);

            return response;
        }
        catch (Exception ex)
        {
            this.Failure<Domainlog>(ex.Message, request.Select(r => r.ErrorMessages).ToArray(), ex);
        }
        return null;
    }
}
