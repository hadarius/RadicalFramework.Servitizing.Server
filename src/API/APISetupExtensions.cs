using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Radical.Servitizing.Application;
using Radical.Servitizing.Repository;
using System.Threading.Tasks;
using System.Linq;

namespace Radical.Servitizing.Server.API;
 
using Application;
using Repository;

public static class APISetupExtensions
{
    public static IAPISetup UseAppSetup(this IApplicationBuilder app, IWebHostEnvironment env, bool useSwagger = true)
    {
        return new APISetup(app, env, useSwagger);
    }
    public static IAPISetup UseAppSetup(this IApplicationBuilder app, IWebHostEnvironment env, string[] apiVersions)
    {
        return new APISetup(app, env, apiVersions);
    }
    public static IApplicationBuilder UseExternalProvider(this IApplicationBuilder app)
    {
        new APISetup(app).UseExternalProvider();
        return app;
    }

    public static IApplicationBuilder UseInternalProvider(this IApplicationBuilder app)
    {
        new APISetup(app).UseInternalProvider();
        return app;
    }

    public static IApplicationBuilder RebuildProviders(this IApplicationBuilder app)
    {
        new APISetup(app).RebuildProviders();
        return app;
    }

    public static async Task LoadOpenDataEdms(this APISetup app)
    {
        await Task.Run(() =>
        {
            RepositoryManager.Clients.ForEach((client) =>
            {
                client.BuildMetadata();
            });

            ApplicationSetup.AddOpenDataServiceImplementations();
            app.RebuildProviders();
        });
    }
}