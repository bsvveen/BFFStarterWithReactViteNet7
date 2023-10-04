using Microsoft.AspNetCore.Authorization;

public static class ServiceExtensions
{
    public static void AddSecurity(this IServiceCollection services, string clientId, string clientSecret)
    {
        services.AddAuthentication(o => {
            o.DefaultScheme = "Application";
            o.DefaultSignInScheme = "External";
            o.DefaultAuthenticateScheme = "External";
        })
        .AddCookie("Application", options => {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddCookie("External")
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = clientId;
            googleOptions.ClientSecret = clientSecret;
        });

        services.AddAuthorization(options => {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });
    }
}

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
        context.Response.Headers.Add("Referrer-Policy", "no-referrer");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Server", "unknown");

        await _next(context);
    }
}

public static class RequestSecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}