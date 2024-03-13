namespace Aspire.Oidc.AppHost;

public static class OpenIdConnectResourceExtensions
{
    public static IResourceBuilder<OpenIdConnectResource> AddOpenIdConnect(
        this IDistributedApplicationBuilder builder,
        string name,
        string authority)
    {
        var resource = new OpenIdConnectResource(name, authority);

        return builder.AddResource(resource);
    }

    public static IResourceBuilder<ProjectResource> WithOpenIdConnectClient(
       this IResourceBuilder<ProjectResource> builder,
       IResourceBuilder<OpenIdConnectResource> idp,
       string clientId,
       string[] scopes)
    {
        return builder.WithOpenIdConnectClient(idp, clientId, clientSecret: null, scopes);
    }

    public static IResourceBuilder<ProjectResource> WithOpenIdConnectClient(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<OpenIdConnectResource> idp,
        string clientId,
        string? clientSecret,
        string[] scopes)
    {
        return builder
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables["Identity:Authority"] = idp.Resource.Authority;
                context.EnvironmentVariables["Identity:ClientId"] = clientId;
                context.EnvironmentVariables["Identity:Scopes"] = string.Join(' ', scopes);

                if (clientSecret is not null)
                {
                    context.EnvironmentVariables["Identity:ClientSecret"] = clientSecret;
                }
            });
    }

    public class OpenIdConnectResource(string name, string authority) : Resource(name), IResourceWithServiceDiscovery
    {
        public string Authority { get; } = authority;
    }
}