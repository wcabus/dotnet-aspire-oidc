using Aspire.Oidc.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Aspire.Oidc.Web.Extensions;

internal static class HostingExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder) 
    {
        builder.AddAuthenticationServices();

        // Application services
        builder.Services.AddScoped<LogoutService>();

        return builder;
    }

    public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

        // Add Authentication services
        services.AddAuthorization();
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))
            .AddOpenIdConnect()
            .ConfigureWebAppOpenIdConnect();

        // Blazor auth services
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();
    }

    private static void ConfigureWebAppOpenIdConnect(this AuthenticationBuilder authentication)
    {
        // Named options
        authentication.Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<IConfiguration>(configure);

        // Unnamed options
        authentication.Services.AddOptions<OpenIdConnectOptions>()
            .Configure<IConfiguration>(configure);

        static void configure(OpenIdConnectOptions options, IConfiguration configuration)
        {
            var configSection = configuration.GetRequiredSection("Identity");
            var authority = configSection.GetRequiredValue("Authority");
            var clientId = configSection.GetRequiredValue("ClientId");
            var clientSecret = configSection.GetRequiredValue("ClientSecret");
            var scopes = configSection.GetRequiredValue("Scopes");

            options.Authority = authority;
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;

            options.Scope.Clear();
            foreach (var scope in scopes.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries))
            {
                options.Scope.Add(scope);
            }

            options.ResponseType = OpenIdConnectResponseType.Code;
            options.ResponseMode = OpenIdConnectResponseMode.Query;
            options.UsePkce = true;

            options.SaveTokens = true; // Preserve the access token so it can be used to call backend APIs
            options.RequireHttpsMetadata = true;
            options.MapInboundClaims = false; // Prevent from mapping "sub" claim to nameidentifier.

            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.RoleClaimType = "role";
        }
    }
}
