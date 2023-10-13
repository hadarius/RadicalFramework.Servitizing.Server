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

public class GetQueryHandler<TStore, TEntity, TDto>
    : IRequestHandler<GetQuery<TStore, TEntity, TDto>, IQueryable<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
    where TDto : class
{
    protected readonly IStoreRepository<TEntity> _repository;

    public GetQueryHandler(IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
    }

    public virtual Task<IQueryable<TDto>> Handle(
        GetQuery<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        var result = _repository.GetQueryAsync<TDto>(request.Sort, request.Expanders);

        result.Wait(30 * 1000);

        return result;
    }
}
