using MediatR;
using Radical.Series;
using Radical.Servitizing.Data.Store;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Operation.Query.Handler;

using Entity;
using Radical.Servitizing.Repository;

public class FilterQueryHandler<TStore, TEntity, TDto>
    : IRequestHandler<Filter<TStore, TEntity, TDto>, ISeries<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;

    public FilterQueryHandler(IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
    }

    public virtual Task<ISeries<TDto>> Handle(
        Filter<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        if (request.Predicate == null)
            return _repository.Filter<TDto>(
                request.Offset,
                request.Limit,
                request.Sort,
                request.Expanders
            );
        return _repository.Filter<TDto>(
            request.Offset,
            request.Limit,
            request.Predicate,
            request.Sort,
            request.Expanders
        );
    }
}
