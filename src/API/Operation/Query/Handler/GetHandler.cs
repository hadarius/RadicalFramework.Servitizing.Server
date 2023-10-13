using MediatR;
using Radical.Series;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Radical.Servitizing.Data.Store;

namespace Radical.Servitizing.Server.API.Operation.Query.Handler;

using Entity;
using Radical.Servitizing.Repository;

public class GetHandler<TStore, TEntity, TDto>
    : IRequestHandler<Get<TStore, TEntity, TDto>, ISeries<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;

    public GetHandler(IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
    }

    public virtual Task<ISeries<TDto>> Handle(
        Get<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        return _repository.Get<TDto>(
            request.Offset,
            request.Limit,
            request.Sort,
            request.Expanders
        );
    }
}
