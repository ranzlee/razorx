using RxTemplate.Components.Auth;
using RxTemplate.Components.Layout;
using RxTemplate.Rx;

namespace RxTemplate.Router;

public static class Auth {
    public static RouteGroupBuilder WithAuthRoutes(this RouteGroupBuilder router) {

        router.AddRoutePath(RequestType.POST, "/auth/sign-in", AuthHandler.SignIn)
            .AllowAnonymous();

        // The Identity Provider's (IDP) "Redirect URI" back to the app
        router.AddRoutePath(RequestType.POST, "/signin-oidc", AuthHandler.SignInCallback)
            .AllowAnonymous()
            // This will be a POST from the IDP, so Antiforgery validation must be skipped
            // The token will be validated after the redirect from the IDP
            .WithRxSkipAntiforgeryValidation();

        // Post-authentication processing to sync the app state with the cookie
        // and perhaps request the "ReturnUrl" if the user was attempting to reach 
        // protected route that triggered the authentication.
        router.AddRoutePath(RequestType.GET, "/auth/complete", AuthHandler.SignInComplete)
            .AllowAnonymous()
            .WithRxPageRouteFor<App>();

        router.AddRoutePath(RequestType.POST, "/auth/sign-out", AuthHandler.SignOut)
            .RequireAuthorization();

        return router;
    }
}