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

public class CreateSetHandler<TStore, TEntity, TDto>
    : IRequestHandler<CreateSet<TStore, TEntity, TDto>, CommandSet<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IServicer _uservice;

    public CreateSetHandler(IServicer uservice, IStoreRepository<TStore, TEntity> repository)
    {
        _uservice = uservice;
        _repository = repository;
    }

    public virtual async Task<CommandSet<TDto>> Handle(
        CreateSet<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IEnumerable<TEntity> entities;
            if (request.Predicate == null)
                entities = await _repository.AddBy(request.ForOnly(d => d.IsValid, d => d.Data));
            else
                entities = await _repository.AddBy(
                    request.ForOnly(d => d.IsValid, d => d.Data),
                    request.Predicate
                );

            await entities
                .ForEachAsync(
                    (e, x) =>
                    {
                        request[x].Entity = e;
                    }
                )
                .ConfigureAwait(false);

            _ = _uservice
                .Publish(new CreatedSet<TStore, TEntity, TDto>(request))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Failure<Domainlog>(ex.Message, request.Select(r => r.ErrorMessages).ToArray(), ex);
        }
        return request;
    }
}
