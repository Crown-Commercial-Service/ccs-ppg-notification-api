using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Ccs.Ppg.Utility.Swagger
{
    public static class Startup
    {
        public static void AddSwagger(this IServiceCollection services, string xmlPath, string title, string version )
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                c.EnableAnnotations();
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Name = "X-API-KEY",
                    Type = SecuritySchemeType.ApiKey,
                });
                var openApiSecuritySchema = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    },
                    In = ParameterLocation.Header
                };
                var openApiSecurityRequirement = new OpenApiSecurityRequirement
                                                 {
                                                    { openApiSecuritySchema, new List<string>() }
                                                 };
                c.AddSecurityRequirement(openApiSecurityRequirement);
            });
        }

        public static void ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
