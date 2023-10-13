using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Radical.Servitizing.Data.Store;

namespace Radical.Servitizing.Server.API.Operation.Query.Handler;

using Entity;
using Radical.Servitizing.Repository;

public class GetAsyncHandler<TStore, TEntity, TDto>
    : IStreamRequestHandler<GetAsync<TStore, TEntity, TDto>, TDto>
    where TEntity : Entity
    where TStore : IDatabaseStore
    where TDto : class
{
    protected readonly IStoreRepository<TEntity> _repository;

    public GetAsyncHandler(IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
    }

    public virtual IAsyncEnumerable<TDto> Handle(
        GetAsync<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetAsync<TDto>(
            request.Offset,
            request.Limit,
            request.Sort,
            request.Expanders
        );
    }
}
