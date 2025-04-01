# RazorX

An ASP.NET and htmx meta-framework

The name RazorX represents the combination of ASP.NET Razor Components on the server with htmx on the client. ASP.NET Minimal APIs provide the request-response processing between the client and server. Razor Components are only used for server-side templating, and there are no dependencies on Blazor for routing or interactivity.

## Getting started

Install the required dependencies, if necessary.

- [.NET SDK](https://dotnet.microsoft.com/en-us/download) >= 9.0
- [Node.js](https://nodejs.org/en) or similar JS runtime for tailwindcss
- [Azurite VS Code extension](https://marketplace.visualstudio.com/items?itemName=Azurite.azurite)

Download and install the template

1. Download the `Package/RazorX.Template.1.0.0-beta.nupkg`
2. Install the template `dotnet new install RazorX.Template.1.0.0-beta.nupkg`
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

### Create a Demo application

1. Create a new `Demo` folder.
2. Run the `dotnet new razorx` command in a terminal in the `Demo` folder to spawn a new RazorX application.
3. Run the `dotnet watch` command in the terminal to launch the app. The browser should launch the app. If not, open a browser and go to `https://localhost:44376/`.
4. Once you verify the app runs, go ahead and shutdown the server using `Ctrl-C` in the terminal.

### Create a new page

1. Create a new `Todos` folder under `Components`.
2. Create a new `TodosPage.razor` file.
3. Create a new `TodosHandler.cs` file.

**_Add the following code to the `TodosPage.razor` file._**

```csharp
<HeadContent>
    <title>TODOs</title>
</HeadContent>

<div id="todos-page">
    <div class="flex justify-center">
        <article class="prose">
            <div class="flex justify-center">
                <h2>TODOs</h2>
            </div>
        </article>
    </div>
    <div class="flex justify-center w-full">
        <div class="w-2xl">
            <div class="flex flex-col justify-center w-full">
            </div>
        </div>
    </div>
</div>
```

**_Add the following code to the `TodosHandler.cs` file._**

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();
    }

    public static IResult Get(
        HttpResponse response,
        ILogger<TodosHandler> logger) {
        return response.RenderComponent<TodosPage>(logger);
    }
}
```

**_Add a nav link to the new page by opening the `Components/Layout/Nav.razor` file and adding the following `@* NEW *@` code._**

```csharp
<div class="z-50 bg-base-100 min-h-full w-96 p-4">
  <div class="flex justify-between items-center">
    <h2 class="text-2xl font-semibold text-primary">Demo Menu</h2>
    @* ...code omitted *@
  </div>
  <ul class="menu w-full">
    <li>
      <RxNavItem
        NavItemRoute="/"
        CurrentRouteClass="font-semibold">
        Home
      </RxNavItem>
    </li>
    @* NEW *@
    <li>
        <RxNavItem
            NavItemRoute="/todos"
            CurrentRouteClass="font-semibold">
            TODOs
        </RxNavItem>
    </li>
    @* END NEW *@
    <li>
      <RxNavItem
        NavItemRoute="/examples"
        MatchPartial="@(true)"
        CurrentRouteClass="font-semibold"
      >
        Examples
      </RxNavItem>
    </li>
    @* ...code omitted *@
  </ul>
</div>
```

**_Save all files and run the `dotnet watch` command in the terminal to launch the app. The `TODOs` page should be available from the nav menu._**

### Add a model

**_Create a new `TodosModel.cs` file in `Components/Todos` and add the following code._**

```csharp
namespace Demo.Components.Todos;

public record TodoModel
(
    string? Id = null,
    string? Title = null,
    string? Description = null,
    bool IsComplete = false,
    DateTime? Created = null
);

//Todo Entity - This would be an EF entity in a real application and would NOT be defined here!
public class Todo {
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime Created { get; set; }
}
```

### Add a Create TODO feature

**_Create a new `TodosForm.razor` file in `Components/Todos` and add the following code._**

```csharp
@implements IComponentModel<TodoModel>

<div id="todos-form" class="flex flex-col w-full gap-y-3">
    <form hx-post="/todos" hx-target="#todos-list" hx-swap="beforeend" novalidate>
        <div>
            <!-- Title Field -->
            <Field
                Id="@(nameof(Model.Title))"
                PropertyName="@(nameof(Model.Title))"
                Value="@(Model.Title)"
                Label="Title"
                InputType="text"
                UseOpacityForValidationErrors="@(true)"
                placeholder="e.g., Learn the RazorX meta-framework!">
            </Field>
        </div>
        <div>
            <!-- Description Memo Field -->
            <MemoField
                Id="@(nameof(Model.Description))"
                PropertyName="@(nameof(Model.Description))"
                Value="@(Model.Description)"
                Label="Description"
                MaxLength="500"
                UseOpacityForValidationErrors="@(true)"
                placeholder="e.g., This includes reading the htmx documentation and checking out Tailwind and daisyUI.">
            </MemoField>
        </div>
        <div class="flex justify-end">
            <button type="submit" class="btn btn-primary">
                Submit
            </button>
        </div>
    </form>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Create a new `TodosItem.razor` file in `Components/Todos` and add the following code._**

```csharp
@implements IComponentModel<TodoModel>

<div id="@($"todos-item-{Model.Id}")" class="card card-border border-base-300 bg-base-100 w-full mb-2">
    <div class="card-body">
        <div class="card-actions justify-between items-center">
            <div>
                <Checkbox
                    PropertyName="@(nameof(Model.IsComplete))"
                    IsChecked="@(Model.IsComplete)"
                    aria-label="Complete">
                </Checkbox>
            </div>
            <div class="flex grow justify-center py-2">
                <h2 class="card-title">@(Model.Title)</h2>
            </div>
            <div>
                <button class="btn btn-square btn-sm btn-error">
                    &#x2715;
                </button>
            </div>
        </div>
        <p class="whitespace-pre">
            @(Model.Description)
        </p>
        <div class="flex justify-end text-xs">
            <RxUtcToLocal DateInput="@(Model.Created!.Value)" />
        </div>
    </div>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Update the TodosPage Component with the `@* New *@` Component Fragments_**

```csharp
@* NEW *@
@implements IComponentModel<IEnumerable<TodoModel>>
@* END NEW *@

<HeadContent>
    <title>TODOs</title>
</HeadContent>

<div id="todos-page">
    <div class="flex justify-center">
        <article class="prose">
            <div class="flex justify-center">
                <h2>TODOs</h2>
            </div>
        </article>
    </div>
    <div class="flex justify-center w-full">
        <div class="w-2xl">
            <div class="flex flex-col justify-center w-full">
                @* NEW *@
                <div id="todos-list">
                    @foreach (var todo in Model) {
                        <TodosItem Model="@(todo)" />
                    }
                </div>
                <TodosForm Model="@(new())"/>
                @* END NEW *@
            </div>
        </div>
    </div>
</div>

@* NEW *@
@code {
    [Parameter] public IEnumerable<TodoModel> Model { get; set; } = null!;
}
@* END NEW *@
```

**_Update the TodosHandler with the `//New` component fragments for creating TODOs_**

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    //NEW
    // This is only here for demonstration. In a real app this would be an EF DbSet
    private static readonly IList<Todo> TodosDbSet = [];
    //END NEW

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        //NEW
        router.MapPost("/todos", Post)
            .AllowAnonymous();
        //END NEW
    }

    public static IResult Get(
        HttpResponse response,
        ILogger<TodosHandler> logger) {
        //NEW
        // return response.RenderComponent<TodosPage>(logger);
        // Inject the (fake) DbContext and project the list of TodoModel records
        var model = TodosDbSet.Select(x => new TodoModel(x.Id, x.Title, x.Description, x.IsComplete, x.Created));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model, logger);
        //END NEW
    }

    //NEW
    public static IResult Post(
        HttpResponse response,
        TodoModel model,
        ILogger<TodosHandler> logger) {
        var entity = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            Created = DateTime.UtcNow,
            IsComplete = false
        };
        // Simulate saving the entity
        TodosDbSet.Add(entity);
        // Update the model
        model = model with { Id = entity.Id, Created = entity.Created };
        return response.RenderComponent<TodosItem, TodoModel>(model, logger);
    }
    //END NEW
}
```

**_Run the `dotnet watch` command in the terminal to launch the app. You should now be able to add TODOs to the list._**

### PROBLEM: The form is not reset after saving a TODO - SOLUTION: Send an new form after saving a TODO

**_Update the TodosModel with the `//New` code._**

```csharp
namespace Demo.Components.Todos;

public record TodoModel
(
    //NEW
    bool ResetForm = false,
    //END NEW
    string? Id = null,
    string? Title = null,
    string? Description = null,
    bool IsComplete = false,
    DateTime? Created = null
);

public class Todo {
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime Created { get; set; }
}
```

**_Update the TodosItem component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@* NEW *@
@if (Model.ResetForm) {
    <TodosForm Model="@(new())" />
}
@* END NEW *@

<div id="@($"todos-item-{Model.Id}")" class="card card-border border-base-300 bg-base-100 w-full mb-2">
    <div class="card-body">
        <div class="card-actions justify-between items-center">
            <div>
                <Checkbox
                    PropertyName="@(nameof(Model.IsComplete))"
                    IsChecked="@(Model.IsComplete)"
                    aria-label="Complete">
                </Checkbox>
            </div>
            <div class="flex grow justify-center py-2">
                <h2 class="card-title">@(Model.Title)</h2>
            </div>
            <div>
                <button class="btn btn-square btn-sm btn-error">
                    &#x2715;
                </button>
            </div>
        </div>
        <p class="whitespace-pre">
            @(Model.Description)
        </p>
        <div class="flex justify-end text-xs">
            <RxUtcToLocal DateInput="@(Model.Created!.Value)" />
        </div>
    </div>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Update the TodosForm component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@* NEW *@
@* <div id="todos-form" class="flex flex-col w-full gap-y-3"> *@
<div id="todos-form" class="flex flex-col w-full gap-y-3" hx-swap-oob="true">
@* END NEW *@
    <form hx-post="/todos" hx-target="#todos-list" hx-swap="beforeend" novalidate>
        <div>
            <!-- Title Field -->
            <Field
                Id="@(nameof(Model.Title))"
                PropertyName="@(nameof(Model.Title))"
                Value="@(Model.Title)"
                Label="Title"
                InputType="text"
                UseOpacityForValidationErrors="@(true)"
                placeholder="e.g., Learn the RazorX meta-framework!">
            </Field>
        </div>
        <div>
            <!-- Description Memo Field -->
            <MemoField
                Id="@(nameof(Model.Description))"
                PropertyName="@(nameof(Model.Description))"
                Value="@(Model.Description)"
                Label="Description"
                MaxLength="500"
                UseOpacityForValidationErrors="@(true)"
                placeholder="e.g., This includes reading the htmx documentation and checking out Tailwind and daisyUI.">
            </MemoField>
        </div>
        <div class="flex justify-end">
            <button type="submit" class="btn btn-primary">
                Submit
            </button>
        </div>
    </form>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Update the `TodosHandler` with the `//NEW` code._**

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    private static readonly IList<Todo> TodosDbSet = [];

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous();
    }

    public static IResult Get(
        HttpResponse response,
        ILogger<TodosHandler> logger) {
        var model = TodosDbSet.Select(x => new TodoModel(
            //NEW
            false,
            //END NEW
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.Created));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model, logger);
    }

    public static IResult Post(
        HttpResponse response,
        TodoModel model,
        ILogger<TodosHandler> logger) {
        var entity = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            Created =
            DateTime.UtcNow,
            IsComplete = false
        };
        TodosDbSet.Add(entity);
        model = model with {
            //NEW
            ResetForm = true,
            //END NEW
            Id = entity.Id,
            Created = entity.Created
        };
        return response.RenderComponent<TodosItem, TodoModel>(model, logger);
    }
}
```

**_Run the `dotnet watch` command in the terminal to launch the app. The form should reset after adding a TODO._**

### PROBLEM: Empty TODOs can be created - SOLUTION: Add model validation

**_Create a new `TodosValidator.cs` file in `Components/Todos` and add the following code._**

```csharp
using FluentValidation;
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosValidator : Validator<TodoModel> {
    public TodosValidator(ValidationContext validationContext, ILogger<TodosValidator> logger)
    : base(validationContext, logger) {

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is Required.");
        RuleFor(x => x.Description)
            .MinimumLength(5)
            .WithMessage("Description has a minimum length requirement of 5 characters.");

    }
}
```

**_Update the `TodosHandler` with the `//NEW` code._**

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    private static readonly IList<Todo> TodosDbSet = [];

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            //NEW
            .WithRxValidation<TodosValidator>()
            //END NEW
            .AllowAnonymous();
    }

    public static IResult Get(
        HttpResponse response,
        ILogger<TodosHandler> logger) {
        var model = TodosDbSet.Select(x => new TodoModel(
            false,
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.Created));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model, logger);
    }

    public static IResult Post(
        HttpResponse response,
        TodoModel model,
        //NEW
        ValidationContext validationContext,
        //END NEW
        ILogger<TodosHandler> logger) {
        //NEW
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todos-form", logger);
            response.HxReswap("outerHTML", logger);
            return response.RenderComponent<TodosForm, TodoModel>(model, logger);
        }
        //END NEW
        var entity = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            Created =
            DateTime.UtcNow,
            IsComplete = false
        };
        TodosDbSet.Add(entity);
        model = model with {
            ResetForm = true,
            Id = entity.Id,
            Created = entity.Created
        };
        return response.RenderComponent<TodosItem, TodoModel>(model, logger);
    }
}
```

**_Run the `dotnet watch` command in the terminal to launch the app. You should not be able to add invalid TODOs._**
