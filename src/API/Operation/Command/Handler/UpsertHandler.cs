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

public class UpsertHandler<TStore, TEntity, TDto>
    : IRequestHandler<Upsert<TStore, TEntity, TDto>, Command<TDto>>
    where TEntity : Entity
    where TDto : DTO
    where TStore : IDatabaseStore
{
    protected readonly IStoreRepository<TEntity> _repository;
    protected readonly IServicer _umaker;

    public UpsertHandler(IServicer umaker, IStoreRepository<TStore, TEntity> repository)
    {
        _repository = repository;
        _umaker = umaker;
    }

    public async Task<Command<TDto>> Handle(
        Upsert<TStore, TEntity, TDto> request,
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
                    if (request.Conditions != null)
                        request.Entity = await _repository.PutBy(
                            request.Data,
                            request.Predicate,
                            request.Conditions
                        );
                    else
                        request.Entity = await _repository.PutBy(request.Data, request.Predicate);

                    if (request.Entity == null)
                        throw new Exception(
                            $"{GetType().Name} "
                                + $"for entity {typeof(TEntity).Name} unable renew entry"
                        );

                    _ = _umaker
                        .Publish(new Upserted<TStore, TEntity, TDto>(request))
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
