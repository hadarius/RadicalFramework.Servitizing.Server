using FluentValidation;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Radical.Servitizing.Data.Store;
using Radical.Servitizing.Server.Data.Service;
using System;
using System.Linq;
using System.Reflection;

namespace Radical.Servitizing.Server;

using Data.Service;
using API.Documentation;

public partial class ServerSetup : ServiceSetup, IServerSetup
{
    public ServerSetup(IServiceCollection services, IMvcBuilder mvcBuilder = null) : base(services, mvcBuilder)
    {
    }

    public ServerSetup(IServiceCollection services, IConfiguration configuration)
        : base(services, configuration)
    {
    }          

    public IServerSetup AddDataServer<TServiceStore>(DataServiceTypes dataServiceTypes, Action<DataServiceBuilder> builder) where TServiceStore : IDataServiceStore
    {
        DataServiceBuilder.ServiceTypes = dataServiceTypes;
        if ((dataServiceTypes & DataServiceTypes.OData) > 0)
        {
            var ds = new OpenDataServiceBuilder<TServiceStore>();
            builder.Invoke(ds);
            ds.Build();
            ds.AddODataServicer(mvc);
        }
        if ((dataServiceTypes & DataServiceTypes.Grpc) > 0)
        {
            var ds = new GrpcDataServiceBuilder<TServiceStore>();
            builder.Invoke(ds);
            ds.Build();
            ds.AddGrpcServicer();
        }
        return this;
    }        

    public IServerSetup AddIdentityServer()
    {
        var ao = configuration.Identity;

        registry
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Authority = ao.BaseUrl;
                    options.RequireHttpsMetadata = ao.RequireHttpsMetadata;
                    options.SaveToken = true;
                    options.Audience = ao.OidcApiName;
                }
            );

        AddPolicies();

        registry.AddCors(
            options =>
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                })
        );

        return this;
    }      

    public IServerSetup AddPolicies()
    {
        var ic = configuration.Identity;

        registry.AddAuthorization(options =>
        {
            ic.Scopes.ForEach(s => options.AddPolicy(s, policy => policy.RequireScope(s)));

            ic.Roles.ForEach(s => options.AddPolicy(s, policy => policy.RequireRole(s)));

            options.AddPolicy(
                "Administrators",
                policy =>
                    policy.RequireAssertion(
                        context =>
                            context.User.HasClaim(
                                c =>
                                    (
                                        (
                                            c.Type == JwtClaimTypes.Role
                                            && c.Value == ic.AdministrationRole
                                        )
                                        || (
                                            c.Type == $"client_{JwtClaimTypes.Role}"
                                            && c.Value == ic.AdministrationRole
                                        )
                                    )
                            )
                    )
            );
        });
        return this;
    }

    public IServerSetup AddSwagger()
    {
        string ver = configuration.Version;
        var ao = configuration.Identity;
        registry.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                ao.ApiVersion,
                new OpenApiInfo { Title = ao.ApiName, Version = ao.ApiVersion }
            );
            //options.OperationFilter<SwaggerDefaultValues>();
            options.OperationFilter<SwaggerJsonIgnoreFilter>();
            options.DocumentFilter<IgnoreApiDocument>();
            //options.SchemaFilter<SwaggerExcludeFilter>();

            options.AddSecurityDefinition(
                "oauth2",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{ao.BaseUrl}/connect/token"),
                            Scopes = ao.Scopes.ToDictionary(s => s)
                        }
                    }
                }
            );
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });
        return this;
    }

    public IServerSetup ConfigureApiVersions(string[] apiVersions)
    {
        this.apiVersions = apiVersions;
        return this;
    }

    public IServerSetup ConfigureServer(bool includeSwagger = true, Assembly[] assemblies = null)
    {
        Assemblies ??= assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

        base.ConfigureServices(Assemblies)
            .Services
            .AddHttpContextAccessor();

        AddIdentityServer();           

        if (includeSwagger)    
            AddSwagger();            

        Services.MergeServices();

        return this;
    }
}
