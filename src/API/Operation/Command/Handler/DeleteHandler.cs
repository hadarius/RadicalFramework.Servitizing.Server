using FluentValidation.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Operation.Command.Handler;

using DTO;
using Entity;
using Logging;
using Notification;
using Repository;
using Servitizing.Data.Store;

public class DeleteHandler<TStore, TEntity, TDto>
    : IRequestHandler<Delete<TStore, TEntity, TDto>, Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IServicer _umaker;

    public DeleteHandler(IServicer umaker, IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
        _umaker = umaker;
    }

    public async Task<Command<TDto>> Handle(
        Delete<TStore, TEntity, TDto> request,
        CancellationToken cancellationToken
    )
    {
        return await Task.Run(
            async () =>
            {
                if (!request.Result.IsValid)
                    return request;

                try
                {
                    if (request.Keys != null)
                        request.Entity = await _repository.Delete(request.Keys);
                    else if (request.Data == null && request.Predicate != null)
                        request.Entity = await _repository.Delete(request.Predicate);
                    else
                        request.Entity = await _repository.DeleteBy(
                            request.Data,
                            request.Predicate
                        );

                    if (request.Entity == null)
                        throw new Exception(
                            $"{GetType().Name} for entity"
                                + $" {typeof(TEntity).Name} unable delete entry"
                        );

                    _ = _umaker
                        .Publish(new Deleted<TStore, TEntity, TDto>(request))
                        .ConfigureAwait(false);
                    ;
                }
                catch (Exception ex)
                {
                    request.Result.Errors.Add(new ValidationFailure(string.Empty, ex.Message));
                    this.Failure<Applog>(ex.Message, request.ErrorMessages, ex);
                }

                return request;
            },
            cancellationToken
        );
    }
}
