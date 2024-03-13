using Aspire.Oidc.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var idp = builder.AddOpenIdConnect("oidc", authority:"https://demo.duendesoftware.com");

builder.AddProject<Projects.Aspire_Oidc_Web>("aspire-oidc-web")
    .WithOpenIdConnectClient(idp, 
        clientId:"interactive.confidential",
        clientSecret:"secret",
        scopes: ["openid", "profile", "email"])
    .WithLaunchProfile("https");

builder.Build().Run();
