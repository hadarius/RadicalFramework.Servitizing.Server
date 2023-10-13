using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.Server;
using Radical.Logging;
using Radical.Series;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Radical.Servitizing.Server.Data.Service;
using Radical.Servitizing.Server;

namespace Radical.Servitizing.Server.API
{
    public class APISetup : IAPISetup
    {
        public static bool externalProviderUsed;

        IApplicationBuilder app;
        IWebHostEnvironment env;

        public APISetup(IApplicationBuilder application) { app = application; }

        public APISetup(IApplicationBuilder application, IWebHostEnvironment environment, bool useSwagger)
        {
            app = application;
            env = environment;
            UseStandardSetup(useSwagger ? new string[] { "1" } : null);
        }

        public APISetup(IApplicationBuilder application, IWebHostEnvironment environment, string[] apiVersions = null)
        {
            app = application;
            env = environment;
            UseStandardSetup(apiVersions);
        }

        public IAPISetup RebuildProviders()
        {
            if (externalProviderUsed)
            {
                UseExternalProvider();
            }
            else
            {
                UseInternalProvider();
            }

            return this;
        }

        public void UseEndpoints()
        {
            app.UseEndpoints(endpoints =>
            {
                var method = typeof(GrpcEndpointRouteBuilderExtensions).GetMethods().Where(m => m.Name.Contains("MapGrpcService")).FirstOrDefault().GetGenericMethodDefinition();
                ISeries<Type> serviceContracts = GrpcDataServiceRegistry.ServiceContracts;
                if (serviceContracts.Any())
                {
                    foreach (var serviceContract in serviceContracts)
                        method.MakeGenericMethod(serviceContract).Invoke(endpoints, new object[] { endpoints });

                    endpoints.MapCodeFirstGrpcReflectionService();
                }
                endpoints.MapControllers();
            });
        }

        public IAPISetup UseDataServices()
        {
            this.LoadOpenDataEdms().ConfigureAwait(true);
            return this;
        }

        public IAPISetup UseDataMigrations()
        {
            using (IServiceScope scope = ServiceManager.GetProvider().CreateScope())
            {
                try
                {
                    IServicer us = scope.ServiceProvider.GetRequiredService<IServicer>();
                    us.GetEndpoints().ForEach(e => ((DbContext)e.Context).Database.Migrate());
                }
                catch (Exception ex)
                {
                    this.Error<Applog>("Data migration initial create - unable to connect the database engine", null, ex);
                }
            }

            return this;
        }

        public IAPISetup UseExternalProvider()
        {
            IServiceManager sm = ServiceManager.GetManager();
            sm.Registry.MergeServices(false);
            ServiceManager.SetProvider(app.ApplicationServices);
            externalProviderUsed = true;
            return this;
        }

        public IAPISetup UseInternalProvider()
        {
            IServiceManager sm = ServiceManager.GetManager();
            sm.Registry.MergeServices();
            app.ApplicationServices = ServiceManager.BuildInternalProvider();
            externalProviderUsed = false;
            return this;
        }

        public IAPISetup UseStandardSetup(string[] apiVersions = null)
        {
            UseHeaderForwarding();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseODataBatching();
            app.UseODataQueryRequest();

            app.UseRouting();

            app.UseCors();

            if (apiVersions != null)
                UseSwaggerSetup(apiVersions);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            UseEndpoints();

            return this;
        }

        public IAPISetup UseSwaggerSetup(string[] apiVersions)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var ao = ServiceManager.GetConfiguration().Identity;

            app.UseSwagger();
            app.UseSwaggerUI(
                s =>
                {
                    s.SwaggerEndpoint($"{ao.ApiBaseUrl}/swagger/v1/swagger.json", ao.ApiName);
                    s.OAuthClientId(ao.OidcSwaggerUIClientId);
                    s.OAuthAppName(ao.ApiName);
                });
            return this;
        }

        public IAPISetup UseHeaderForwarding()
        {
            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All
            };

            forwardingOptions.KnownNetworks.Clear();
            forwardingOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardingOptions);

            return this;
        }

        public IAPISetup UseJwtUserInfo()
        {
            app.UseMiddleware<APISetupJwtMiddleware>();

            return this;
        }
    }
}