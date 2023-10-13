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

public class UpsertSetHandler<TStore, TEntity, TDto>
    : IRequestHandler<UpsertSet<TStore, TEntity, TDto>, CommandSet<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IServicer _uservice;

    public UpsertSetHandler(IServicer uservice, IStoreRepository<TStore, TEntity> repository)
    {
        _uservice = uservice;
        _repository = repository;
    }

    public virtual async Task<CommandSet<TDto>> Handle(
        UpsertSet<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IEnumerable<TEntity> entities;
            if (request.Conditions == null)
                entities = _repository.PutBy(
                    request.ForOnly(d => d.IsValid, d => d.Data),
                    request.Predicate
                );
            else
                entities = _repository.PutBy(
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

            _ = _uservice
                .Publish(new UpsertedSet<TStore, TEntity, TDto>(request))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Failure<Domainlog>(ex.Message, request.Select(r => r.ErrorMessages).ToArray(), ex);
        }
        return request;
    }
}
