using System.Reflection;
using Microsoft.AspNetCore.ResponseCompression;
using FluentValidation;
using RxTemplate.Components.Rx;
using RxTemplate.Router;
using RxTemplate.Rx;

// Change file version so that script endpoint changes for cache busting
// Fingerprinting files is part of .NET 9 with the new MapStaticAssets middleware, so you may 
// remove this custom implementation if you would rather use the now built-in option. 
// The RazorX template will continue to target LTS .NET releases, so we must wait for .NET 10.
// It is recommended to update your template app to .NET 9, if you have the option. There
// are performance improvements in Minimal APIs.
[assembly: AssemblyVersion("1.0.0.*")]

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add the scoped script helper for cache busting
services.AddScriptHelper();

// Add services to the container
services.AddRazorComponents();

// Add response compression
if (!builder.Environment.IsDevelopment()) {
    services.AddResponseCompression(options => {
        options.EnableForHttps = true;
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
            "application/javascript",
            "application/json",
            "text/css",
            "text/html"
        ]);
    });
}

// Add HTMX config
services.AddSingleton(builder.Configuration.GetSection("RxConfig").Get<RxConfig>() ?? new());

// Add HTTP context accessor
services.AddHttpContextAccessor();

// Add API standardized problem details
services.AddProblemDetails();

// Add custom options for deserializing JSON from FORM data
services.ConfigureOptions<HtmxJsonOptions>();

// Add HxTriggers
services.AddHxTriggers();

// Add FluentValidation and validation services
services.AddScoped<ValidationContext>();
services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {

    // Use the default exception handler
    app.UseExceptionHandler();

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    // Use compression all response
    app.UseResponseCompression();
}

// Use HTTPS only
app.UseHttpsRedirection();

// Use static files served from wwwroot
app.UseStaticFiles();

// Use antiforgery middleware
app.UseAntiforgery();

// Use cookie for antiforgery token
app.UseAntiforgeryCookie();

// Map routes
var routeBuilder = app.MapRoot().WithExamplesRoutes();

// Go!
app.Run();