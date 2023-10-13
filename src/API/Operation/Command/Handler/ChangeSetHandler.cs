using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Operation.Command.Handler;

using DTO;
using Entity;
using Logging;
using Notification;
using Repository;
using Servitizing.Data.Store;

public class ChangeSetHandler<TStore, TEntity, TDto>
    : IRequestHandler<ChangeSet<TStore, TEntity, TDto>, CommandSet<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IServicer _servicer;

    public ChangeSetHandler(IServicer servicer, IStoreRepository<TStore, TEntity> repository)
    {
        _servicer = servicer;
        _repository = repository;
    }

    public virtual async Task<CommandSet<TDto>> Handle(
        ChangeSet<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IEnumerable<TEntity> entities;
            if (request.Predicate == null)
                entities = _repository.PatchBy(request.ForOnly(d => d.IsValid, d => d.Data));
            else
                entities = _repository.PatchBy(
                    request.ForOnly(d => d.IsValid, d => d.Data),
                    request.Predicate
                );

            await entities
                .ForEachAsync(
                    (e) =>
                    {
                        request[e.Id].Entity = e;
                    }
                )
                .ConfigureAwait(false);

            _ = _servicer
                .Publish(new ChangedSet<TStore, TEntity, TDto>(request))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Failure<Domainlog>(ex.Message, request.Select(r => r.Output).ToArray(), ex);
        }
        return request;
    }
}
