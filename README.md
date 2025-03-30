# RazorX

An ASP.NET and htmx meta-framework

The name RazorX represents the combination of ASP.NET Razor Components on the server with htmx on the client. ASP.NET Minimal APIs provide the request-response processing between the client and server. Razor Components are only used for server-side templating, and there are no dependencies on Blazor for routing or interactivity.

## Getting started

Install the required dependencies, if necessary.

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) >= 9.0
- [Node.js](https://nodejs.org/en) or similar JS runtime for tailwindcss
- [Azurite VS Code extension](https://marketplace.visualstudio.com/items?itemName=Azurite.azurite)

Download and install the template

1. Download the `Package/RazorX.Template.1.0.0-alpha.nupkg`
2. Install the template `dotnet new install RazorX.Template.1.0.0-alpha.nupkg`
3. Create a new app with `dotnet new razorx`

[OR]

Build and install the template

1. Clone the razorx repository.
2. Use the /RxTemplatePack/makefile to build and install the template. If you're on Windows, you may need to create the template manually by copying the RxTemplate folder to RxTemplatePack/content and running the dotnet CLI commands in the makefile.
3. Create a new app with `dotnet new razorx`.

## Request/Response Cycle

The following diagram describes the flow of a request. The basic concept is to first route the request through a series of middleware, both global and endpoint specific (`IEndpointFilters`). This applies common behaviors like anti-forgery token validation, authorization policy enforcement, and model validation. Next, the request is routed to a specific handler (`IRequestHandler Delegate`) for processing application logic. The request handler will usually return a `RazorComponentResult` for creating an HTML response. Finally, htmx may modify the DOM to swap partial content and trigger events specified in response headers. The events are handled with JavaScript event handlers in `razorx.js`.

![RazorX Request-Response Cycle](razorx-request-response.png "RazorX Request-Response Cycle")

## How To

The following covers the basics of creating a new page, adding a model and validation, and making it reactive with htmx. After working through this, I recommend checking out the included examples. The template includes many components which work well with the htmx hypermedia approach and have client-side JavaScript behaviors for an enhanced experience.

### Create a Demo App

1. Create a new `Demo` folder.
2. Run the `dotnet new razorx` command in a terminal in the `Demo` folder to spawn a new RazorX application.
3. Run the `dotnet watch` command in the terminal to launch the app. The browser should launch the app. If not, open a browser and go to `https://localhost:44376/`.
4. Once you verify the app runs, go ahead and shutdown the server using `Ctrl-C` in the terminal.

### Create a New Page

1. Create a new `Todos` folder under `Components`.
2. Create a new `TodosPage.razor` file.
3. Create a new `TodosHandler.cs` file.

Add the following code to the `TodosPage.razor` file.

```html
<HeadContent>
  <title>TODOs</title>
</HeadContent>

<div class="flex justify-center">
  <article class="prose">
    <div class="flex justify-center">
      <h2>TODOs</h2>
    </div>
  </article>
</div>
<div class="flex justify-center"></div>
```

Add the following code to the `TodosHandler.cs` file.

- The `IRequestHandler` will handle all requests to the page, like a controller.
- The `MapRoutes` method is where the routes are defined.
- Each route is associated to a delegate handler method and may have `IEndpointFilters` that define behaviors for the route.
- The `WithRxRootComponent` filter will render the `TodosPage.razor` component in the `Components\Layout\App.razor` layout.
- `App.razor` implements the `IRootComponent` interface.

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();
    }

    public static IResult Get(HttpResponse response, ILogger<TodosHandler> logger) {
        return response.RenderComponent<TodosPage>(logger);
    }
}
```

Add a nav link to the new page by opening the `Components\Layout\Nav.razor` file and adding the code between the `// DEMO:` comments.

```html
<div class="z-50 bg-base-100 min-h-full w-96 p-4">
  <div class="flex justify-between items-center">
    <h2 class="text-2xl font-semibold text-primary">Demo Menu</h2>
    <button
      id="close-sidebar"
      type="button"
      aria-label="close sidebar"
      class="btn btn-ghost"
    >
      ...
    </button>
    <script>
      ...
    </script>
  </div>
  <ul class="menu w-full">
    <li>
      <RxNavItem NavItemRoute="/" CurrentRouteClass="font-semibold">
        Home
      </RxNavItem>
    </li>
    // DEMO: Add a nav link to the TODOs page
    <li>
      <RxNavItem NavItemRoute="/todos" CurrentRouteClass="font-semibold">
        TODOs
      </RxNavItem>
    </li>
    // DEMO: Done!
    <li>
      <RxNavItem
        NavItemRoute="/examples"
        MatchPartial="@(true)"
        CurrentRouteClass="font-semibold"
      >
        Examples
      </RxNavItem>
      ...
    </li>
  </ul>
</div>
```

Run the `dotnet watch` command in the terminal to launch the app. The `TODOs` page should be available from the nav menu.
