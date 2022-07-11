using System;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Microsoft.AspNetCore.Builder;

public static class DopplerCorsMiddlewareExtensions
{
    public static IApplicationBuilder UseDopplerCors(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.CacheControl = "public, max-age=86400";
            }
            await next();
        });

        app.UseCors(policy => policy
            .SetIsOriginAllowed(isOriginAllowed: _ => true)
            .SetPreflightMaxAge(TimeSpan.FromHours(24))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());

        return app;
    }
}
