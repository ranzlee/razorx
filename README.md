# RazorX

An ASP.NET and htmx meta-framework

The name RazorX represents the combination of ASP.NET Razor Components on the server with htmx on the client. ASP.NET Minimal APIs provide the request-response processing between the client and server. Razor Components are only used for server-side templating, and there are no dependencies on Blazor for routing or interactivity.

## Getting started

Install the required dependencies, if necessary.

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) >= 8 - Tested on SDK 8.0.400 and above (Runtime 8.0.8).
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

The following diagram shows the basic flow of a request. The basic concept is to first send the request through a series of middleware, both global and specific to the endpoint, to apply common behaviors like anti-forgery token validation, authorization policy enforcement, and model validation. Next, the request is passed to a handler for processing application logic. The request handler returns a RazorComponentResult for creating an HTML response. Finally, HTMX may modify the DOM to swap in partial content and trigger any events specified in response headers.

![RazorX Request-Response Cycle](razorx-request-response.png "RazorX Request-Response Cycle")
