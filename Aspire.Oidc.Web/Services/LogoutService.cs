using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;

namespace Aspire.Oidc.Web.Services;

public class LogoutService
{
    public async Task LogOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
}
