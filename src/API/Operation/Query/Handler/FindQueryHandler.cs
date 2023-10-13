using MediatR;
using Radical.Servitizing.Data.Store;
using Radical.Uniques;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Operation.Query.Handler;

using Entity;
using Radical.Servitizing.Repository;

public class FindQueryHandler<TStore, TEntity, TDto>
    : IRequestHandler<FindQuery<TStore, TEntity, TDto>, IQueryable<TDto>>
    where TEntity : Entity
    where TStore : IDatabaseStore
    where TDto : class, IUnique
{
    protected readonly IStoreRepository<TEntity> _repository;

    public FindQueryHandler(IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
    }

    public virtual Task<IQueryable<TDto>> Handle(
        FindQuery<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        IQueryable<TDto> result = null;
        if (request.Keys != null)
            result = _repository.FindOneAsync<TDto>(request.Keys, request.Expanders);
        else
            result = _repository.FindOneAsync<TDto>(request.Predicate, request.Expanders);

        return Task.FromResult(result);
    }
}
