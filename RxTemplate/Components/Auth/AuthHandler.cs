using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RxTemplate.Rx;
using System.Security.Claims;

namespace RxTemplate.Components.Auth;

/// <summary>
/// THIS IS AN EXAMPLE ONLY!
/// This handler must be replaced with a real Identity Provider configured
/// for the app. This is just to demonstrate how this meta-framework integrates
/// with authentication / authorization.
/// </summary>
public class AuthHandler : IRequestHandler {

    // Example sign in - must be replaced with OIDC identity provider
    public static async Task<IResult> SignIn(
        HttpContext httpContext,
        IAntiforgery antiforgery) {
        await antiforgery.ValidateRequestAsync(httpContext);
        var identity = new ClaimsIdentity("ExampleIdentityProvider", "Name", "Role");
        identity.AddClaim(new Claim("ExampleIdentityProvider", "RxIdentityProvider"));
        identity.AddClaim(new Claim("Name", "Test"));
        identity.AddClaim(new Claim("Role", "Admin"));
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return TypedResults.Redirect("/auth/complete");
    }

    // OIDC callback - With a real identity provider, there will need to be a callback post configured
    public static IResult SignInCallback() {
        return Results.NoContent();
    }

    // Signin complete - provides the client the opportunity to request a route and sync cookie state
    public static IResult SignInComplete(HttpResponse response) {
        return response.RenderComponent<Authenticated>();
    }

    // Sign out
    public static async Task<IResult> SignOut(
        HttpContext httpContext,
        IAntiforgery antiforgery) {
        await antiforgery.ValidateRequestAsync(httpContext);
        var props = new AuthenticationProperties { RedirectUri = "/" };
        return TypedResults.SignOut(props, [CookieAuthenticationDefaults.AuthenticationScheme]);
    }
}