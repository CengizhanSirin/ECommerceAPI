using Microsoft.OpenApi;

namespace ECommerce.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerGenExt(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce.API", Version = "v1" });

                const string securitySchemeName = "Bearer";

                options.AddSecurityDefinition(securitySchemeName, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT access token giriniz."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(securitySchemeName, document)] = []
                });
            });
            return services;
        }

        public static IApplicationBuilder UseSwaggerExt(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            return app;
        }
    }
}
