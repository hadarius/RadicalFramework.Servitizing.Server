using FluentValidation;
using MediatR;
using Radical.Servitizing.Server.API.Operation.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Behaviour
{
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommandSet, IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TResponse>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TResponse>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                (await Task.WhenAll(_validators
                              .Select(v => v
                              .ValidateAsync(context, cancellationToken))))
                                 .SelectMany(r => r.Errors)
                                 .ForEach(f => request.Commands
                                 .ElementAt((int)f
                                    .FormattedMessagePlaceholderValues
                                     ["CollectionIndex"])
                                    .Result.Errors.Add(f));
            }

            return await next();
        }
    }
}