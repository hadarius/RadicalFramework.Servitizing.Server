using Microsoft.Extensions.DependencyInjection;

namespace Radical.Servitizing.Server
{
    public static class ServerSetupExtensions
    {
        public static IServerSetup AddServerSetup(this IServiceCollection services, IMvcBuilder mvcBuilder = null)
        {
            return new ServerSetup(services);
        }
    }
}
