using MediatR;
using Radical.Servitizing.Server.API.Operation;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API.Behaviour;

using Logging;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IOperation where TResponse : IOperation
{
    public LoggingBehaviour()
    {
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        request.Info<Apilog>($"Request data entry", request.Input);

        var response = await next();

        response.Info<Apilog>($"Response data result", response.Output);

        return response;
    }
}