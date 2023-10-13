using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Radical.Servitizing.Server.API.Documentation
{
    public class IgnoreApiDocument : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                var ignore = apiDescription.CustomAttributes().Any(c => c.GetType() == typeof(IgnoreApiAttribute));
                apiDescription.TryGetMethodInfo(out MethodInfo info);
                if (ignore || info.GetCustomAttributes<IgnoreApiAttribute>().Distinct().Any())
                {
                    string kepath = apiDescription.RelativePath;
                    var remRoutes = swaggerDoc.Paths
                                        .Where(x => x.Key.ToLower()
                                        .Contains(kepath.ToString()
                                        .ToLower())).ToArray();

                    var a = remRoutes.ForEach(x => swaggerDoc.Paths.Remove(x.Key)).ToList();
                }
            }
        }
    }

    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        //private readonly ServiceApiOptions _apiOptions;

        public AuthorizeCheckOperationFilter()
        {
            //_apiOptions = apiOptions;
        }
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.MethodInfo.DeclaringType != null && (context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                                                                            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());

            if (hasAuthorize)
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "oauth2"
                                }
                            }
                        ] = new string[] {  }
                    }
                };

            }
        }
    }

    public class SwaggerJsonIgnoreFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var ignoredProperties = context.MethodInfo.GetParameters()
                .SelectMany(p => p.ParameterType.GetProperties()
                .Where(prop => prop.GetCustomAttribute<JsonIgnoreAttribute>() != null))
                .ToList();

            if (!ignoredProperties.Any()) return;

            foreach (var property in ignoredProperties)
            {
                operation.Parameters = operation.Parameters
                    .Where(p => !p.Name.Equals(property.Name, StringComparison.InvariantCulture))
                    .ToList();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerExcludeAttribute : Attribute
    {
    }

    public class SwaggerExcludeFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (!schema.Properties.Any() || type == null)
            {
                return;
            }

            var excludedPropertyNames = type
                    .GetProperties()
                    .Where(
                        t => t.GetCustomAttribute<SwaggerExcludeAttribute>() != null
                    ).Select(d => d.Name).ToList();

            if (!excludedPropertyNames.Any())
            {
                return;
            }

            var excludedSchemaPropertyKey = schema.Properties
                   .Where(
                        ap => excludedPropertyNames.Any(
                            pn => pn.ToLower() == ap.Key
                        )
                    ).Select(ap => ap.Key);

            foreach (var propertyToExclude in excludedSchemaPropertyKey)
            {
                schema.Properties.Remove(propertyToExclude);
            }
        }
    }

    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given
        /// context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.
        ///     </param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            //operation.Deprecated |= apiDescription.IsDeprecated();

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
            foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
            {
                // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
                var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
                var response = operation.Responses[responseKey];

                foreach (var contentType in response.Content.Keys)
                {
                    if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                    {
                        response.Content.Remove(contentType);
                    }
                }
            }

            if (operation.Parameters == null)
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            //foreach (var parameter in operation.Parameters)
            //{
            //    var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            //    parameter.Description ??= description.ModelMetadata?.Description;

            //    if (parameter.Schema.Default == null && description.DefaultValue != null)
            //    {
            //        // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
            //        var json = JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType);
            //        parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
            //    }

            //    parameter.Required |= description.IsRequired;
            //}
        }
    }


}

