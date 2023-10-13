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

public class UpdateSetHandler<TStore, TEntity, TDto>
    : IRequestHandler<UpdateSet<TStore, TEntity, TDto>, CommandSet<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IServicer _servicer;

    public UpdateSetHandler(IServicer servicer, IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
        _servicer = servicer;
    }

    public async Task<CommandSet<TDto>> Handle(
        UpdateSet<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IEnumerable<TEntity> entities = null;
            if (request.Predicate == null)
                entities = _repository.SetBy(request.ForOnly(d => d.IsValid, d => d.Data));
            else if (request.Conditions == null)
                entities = _repository.SetBy(
                    request.ForOnly(d => d.IsValid, d => d.Data),
                    request.Predicate
                );
            else
                entities = _repository.SetBy(
                    request.ForOnly(d => d.IsValid, d => d.Data),
                    request.Predicate,
                    request.Conditions
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
                .Publish(new UpdatedSet<TStore, TEntity, TDto>(request))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Failure<Domainlog>(ex.Message, request.Select(r => r.ErrorMessages).ToArray(), ex);
        }
        return request;
    }
}
