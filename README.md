# RazorX

An ASP.NET and htmx meta-framework

The name RazorX represents the combination of ASP.NET Razor Components on the server with htmx on the client. ASP.NET Minimal APIs provide the request-response processing between the client and server. Razor Components are only used for server-side templating, and there are no dependencies on Blazor for routing or interactivity.

## Getting started

Install the required dependencies, if necessary.

- .NET SDK >= 8
- Node.js or similar JS runtime for tailwindcss

Download and install the template

1. Download the `Package/RazorX.Template.1.0.0-alpha.nupkg`
2. Install the template `dotnet new install RazorX.Template.1.0.0-alpha.nupkg`
3. Create a new app with `dotnet new razorx`

[OR]

Build and install the template

1. Clone the razorx repository.
2. Use the /RxTemplatePack/makefile to build and install the template. If you're on Windows, you may need to create the template manually by copying the RxTemplate folder to RxTemplatePack/content and running the dotnet CLI commands in the makefile.
3. Create a new app with `dotnet new razorx`.
