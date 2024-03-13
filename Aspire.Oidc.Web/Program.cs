using Aspire.Oidc.Web.Components;
using Aspire.Oidc.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();
var auth = app.MapGroup("authentication");
auth.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
    .AllowAnonymous();

auth.MapPost("/logout", ([FromForm] string? returnUrl) => TypedResults.SignOut(GetAuthProperties(returnUrl),
    [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

return;

static AuthenticationProperties GetAuthProperties(string? returnUrl)
{
    // TODO: Use HttpContext.Request.PathBase instead.
    const string pathBase = "/";

    // Prevent open redirects.
    if (string.IsNullOrEmpty(returnUrl))
    {
        returnUrl = pathBase;
    }
    else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
    {
        returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
    }
    else if (returnUrl[0] != '/')
    {
        returnUrl = $"{pathBase}{returnUrl}";
    }

    return new AuthenticationProperties { RedirectUri = returnUrl };
}