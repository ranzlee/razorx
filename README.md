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

## How To - The Demo TODO List Tutorial

The following covers the basics of creating a new page, adding a model and validation, and making it reactive with htmx. After working through this, I recommend checking out the included examples. The template includes many components which work well with the htmx hypermedia approach and have client-side JavaScript behaviors for an enhanced experience.

1. [Create a Demo application](#create-a-demo-application)
2. [Create a new page](#create-a-new-page)
3. [Add a model](#add-a-model)
4. [Add a Create TODO feature](#add-a-create-todo-feature)
   - [PROBLEM: The form is not reset after saving a TODO - SOLUTION: Send a new form after saving a TODO](#problem-the-form-is-not-reset-after-saving-a-todo---solution-send-a-new-form-after-saving-a-todo)
   - [PROBLEM: Empty TODOs can be created - SOLUTION: Add model validation](#problem-empty-todos-can-be-created---solution-add-model-validation)
5. [Add a Complete TODO feature](#add-a-complete-todo-feature)
6. [Add a Delete TODO feature](#add-a-delete-todo-feature)
7. [Add a Update TODO feature](#add-a-update-todo-feature)
8. [EXTRA CREDIT - Add a Change Validation feature](#extra-credit---add-a-change-validation-feature)

### Rendering

![Demo TODO Rendering](todos-demo.png "The Demo TODO Rendering")

### Component Anatomy

![Demo TODO Component Anatomy](todos-anatomy.png "The Demo TODO Component Anatomy")

### Create a Demo application

- Create a new `Demo` folder.
- Run the `dotnet new razorx` command in a terminal in the `Demo` folder to spawn a new RazorX application.
- Run the `dotnet watch` command in the terminal to launch the app. The browser should launch the app. If not, open a browser and go to `https://localhost:44376/`.
- Once you verify the app runs, go ahead and shutdown the server using `Ctrl-C` in the terminal.

### Create a new page

- Create a new `Todos` folder under `Components`.
- Create a new `TodosPage.razor` file.
- Create a new `TodosHandler.cs` file.

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
        HttpResponse response) {
        return response.RenderComponent<TodosPage>();
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

**_Create a new `TodosDbContext.cs` file in `Components/Todos` and add the following code._**

```csharp
namespace Demo.Components.Todos;

// Fake DbContext - This would be an EF context in a real application and would NOT be defined here!
// The use of this object in the handler would be different:
// 1. The DbContext would be injected into the handler delegates (i.e., Get, Post, etc.,) and NOT be static.
// 2. The delegate handlers would be async Task<IResult> methods for EF async/await operations.
public static class FakeDbContext {

    public static readonly IList<Todo> Todos = [];

}

// Todo Entity - This would be an EF entity in a real application and would NOT be defined here!
public class Todo {
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

**_Create a new `TodosModel.cs` file in `Components/Todos` and add the following code._**

```csharp
namespace Demo.Components.Todos;

public record TodoModel
(
    string? Id = null,
    string? Title = null,
    string? Description = null,
    bool IsComplete = false,
    DateTime? LastUpdated = null
);
```

### Add a Create TODO feature

**_Create a new `TodosForm.razor` file in `Components/Todos` and add the following code._**

```csharp
@implements IComponentModel<TodoModel>

<div>
    <!-- Title Field -->
    <Field
        Id="@($"{nameof(Model.Title)}{Model.Id}")"
        PropertyName="@(nameof(Model.Title))"
        Value="@(Model.Title)"
        Label="Title"
        InputType="text"
        UseOpacityForValidationErrors="@(true)"
        maxlength="80"
        placeholder="e.g., Learn the RazorX meta-framework!">
    </Field>
</div>
<div>
    <!-- Description Memo Field -->
    <MemoField
        Id="@($"{nameof(Model.Description)}{Model.Id}")"
        PropertyName="@(nameof(Model.Description))"
        Value="@(Model.Description)"
        Label="Description"
        MaxLength="500"
        UseOpacityForValidationErrors="@(true)"
        placeholder="e.g., This includes reading the htmx documentation and checking out Tailwind and daisyUI.">
    </MemoField>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Create a new `TodosNew.razor` file in `Components/Todos` and add the following code._**

```csharp
@implements IComponentModel<TodoModel>

<div id="todos-new" class="flex flex-col w-full gap-y-3">
    <form hx-post="/todos" hx-target="#todos-list" hx-swap="beforeend" novalidate>
        <TodosForm Model="@(Model)" />
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

<div id="@($"todos-item-{Model.Id}")" class="card card-border border-base-300 bg-base-200 w-full mb-2">
    <div class="card-body">
        <div class="flex gap-2 justify-between items-center">
            <div class="flex min-w-20">
            </div>
            <div class="flex grow justify-center py-2">
                <h2 class="card-title">@(Model.Title)</h2>
            </div>
            <div class="flex justify-end gap-x-2 min-w-20">
            </div>
        </div>
        <p class="whitespace-pre">
            @(Model.Description)
        </p>
        <div class="flex justify-end text-xs">
            <RxUtcToLocal DateInput="@(Model.LastUpdated!.Value)" />
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
                <TodosNew Model="@(new())"/>
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
        HttpResponse response) {
        //NEW
        //return response.RenderComponent<TodosPage>();
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
        //END NEW
    }

    //NEW
    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model) {
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }
    //END NEW
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). You should now be able to add TODOs to the list._**

#### PROBLEM: The form is not reset after saving a TODO - SOLUTION: Send a new form after saving a TODO

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
    DateTime? LastUpdated = null
);
```

**_Update the TodosItem component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@* NEW *@
@if (Model.ResetForm) {
    <TodosNew Model="@(new())" />
}
@* END NEW *@

<div id="@($"todos-item-{Model.Id}")" class="card card-border border-base-300 bg-base-200 w-full mb-2">
    <div class="card-body">
        <div class="flex gap-2 justify-between items-center">
            <div class="flex min-w-20">
            </div>
            <div class="flex grow justify-center py-2">
                <h2 class="card-title">@(Model.Title)</h2>
            </div>
            <div class="flex justify-end gap-x-2 min-w-20">
            </div>
        </div>
        <p class="whitespace-pre">
            @(Model.Description)
        </p>
        <div class="flex justify-end text-xs">
            <RxUtcToLocal DateInput="@(Model.LastUpdated!.Value)" />
        </div>
    </div>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Update the TodosNew component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@* NEW *@
@* <div id="todos-new" class="flex flex-col w-full gap-y-3"> *@
<div id="todos-new" class="flex flex-col w-full gap-y-3" hx-swap-oob="true">
@* END NEW *@
    <form hx-post="/todos" hx-target="#todos-list" hx-swap="beforeend" novalidate>
        <TodosForm Model="@(Model)" />
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

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous();
    }

    public static IResult Get(
        HttpResponse response) {
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            //NEW
            false,
            //END NEW
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
    }

    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model) {
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            //NEW
            ResetForm = true,
            //END NEW
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). The form should reset after adding a TODO._**

#### PROBLEM: Empty TODOs can be created - SOLUTION: Add model validation

**_Create a new `TodosValidator.cs` file in `Components/Todos` and add the following code._**

```csharp
using FluentValidation;
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosValidator : Validator<TodoModel> {

    public TodosValidator(ValidationContext validationContext) : base(validationContext) {

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is Required.")
            .MaximumLength(80)
            .WithMessage("Title has a maximum length of 80 characters.");
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is Required.")
            .MaximumLength(500)
            .WithMessage("Description has a maximum length of 500 characters.");

    }
}
```

**_Update the `TodosHandler` with the `//NEW` code._**

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous()//;
            //NEW
            .WithRxValidation<TodosValidator>();
            //END NEW
    }

    public static IResult Get(
        HttpResponse response) {
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            false,
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
    }

    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model,
        //NEW
        ValidationContext validationContext) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todos-new");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosNew, TodoModel>(model);
        }
        //END NEW
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            ResetForm = true,
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). You should not be able to add invalid TODOs._**

### Add a Complete TODO feature

**_Update the TodosItem component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@if (Model.ResetForm) {
    <TodosNew Model="@(new())" />
}

<div id="@($"todos-item-{Model.Id}")" class="card card-border border-base-300 bg-base-200 w-full mb-2">
    <div class="card-body">
        <div class="flex gap-2 justify-between items-center">
            <div class="flex min-w-20">
                @* NEW *@
                <div>
                    <Checkbox
                        Id="@($"todos-item-completed-{Model.Id}")"
                        PropertyName="@(nameof(Model.IsComplete))"
                        IsChecked="@(Model.IsComplete)"
                        aria-label="Complete"
                        hx-patch="@($"/todos/{Model.Id}")"
                        hx-target="@($"#todos-item-{Model.Id}")"
                        hx-swap="outerHTML"
                        >
                    </Checkbox>
                </div>
                @* END NEW *@
            </div>
            <div class="flex grow justify-center py-2">
                <h2 class="card-title">@(Model.Title)</h2>
            </div>
            <div class="flex justify-end gap-x-2 min-w-20">
            </div>
        </div>
        @* NEW *@
        <div class="flex justify-center">
            @if (Model.IsComplete) {
                <span class="bg-success text-success-content rounded-full px-2 font-semibold">COMPLETED</span>
            } else {
                <span>&nbsp;</span>
            }
        </div>
        @* END NEW *@
        <p class="whitespace-pre">
            @(Model.Description)
        </p>
        <div class="flex justify-end text-xs">
            <RxUtcToLocal DateInput="@(Model.LastUpdated!.Value)" />
        </div>
    </div>
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

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();

        //NEW
        router.MapPatch("/todos/{id}", Patch)
            .AllowAnonymous();
        //END NEW
    }

    public static IResult Get(
        HttpResponse response) {
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            false,
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
    }

    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model,
        ValidationContext validationContext) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todos-new");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosNew, TodoModel>(model);
        }
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            ResetForm = true,
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    //NEW
    public static IResult Patch(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NoContent();
        }
        todo.IsComplete = !todo.IsComplete;
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"Updated TODO to {(todo.IsComplete ? "completed" : "not completed")}."))
            .Add(new HxFocusTrigger($"#todos-item-completed-{todo.Id}"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }
    //END NEW
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). You should be able to mark TODOs as complete._**

### Add a Delete TODO feature

**_Update the TodosItem component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@if (Model.ResetForm) {
    <TodosNew Model="@(new())" />
}

<div id="@($"todos-item-{Model.Id}")" class="card card-border border-base-300 bg-base-200 w-full mb-2">
    <div class="card-body">
        <div class="flex gap-2 justify-between items-center">
            <div class="flex min-w-20">
                <div>
                    <Checkbox
                        Id="@($"todos-item-completed-{Model.Id}")"
                        PropertyName="@(nameof(Model.IsComplete))"
                        IsChecked="@(Model.IsComplete)"
                        aria-label="Complete"
                        hx-patch="@($"/todos/{Model.Id}")"
                        hx-target="@($"#todos-item-{Model.Id}")"
                        hx-swap="outerHTML"
                        >
                    </Checkbox>
                </div>
            </div>
            <div class="flex grow justify-center py-2">
                <h2 class="card-title">@(Model.Title)</h2>
            </div>
            <div class="flex justify-end gap-x-2 min-w-20">
                @* NEW *@
                <RxModalTrigger
                    ModalId="delete-todo-modal"
                    RouteValue="@(Model.Id)"
                    TextNodeValue="@($"Delete {Model.Title}?")"
                    class="btn btn-square btn-sm btn-error"
                    aria-label="@($"Delete {Model.Title}?")">
                    <span class="text-xl">&#x2715;</span>
                </RxModalTrigger>
                @* END NEW *@
            </div>
        </div>
        <div class="flex justify-center">
            @if (Model.IsComplete) {
                <span class="bg-success text-success-content rounded-full px-2 font-semibold">COMPLETED</span>
            } else {
                <span>&nbsp;</span>
            }
        </div>
        <p class="whitespace-pre">
            @(Model.Description)
        </p>
        <div class="flex justify-end text-xs">
            <RxUtcToLocal DateInput="@(Model.LastUpdated!.Value)" />
        </div>
    </div>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Update the TodosPage component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<IEnumerable<TodoModel>>

<HeadContent>
    <title>TODOs</title>
</HeadContent>

@* NEW *@
<dialog id="delete-todo-modal" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box">
        <div class="flex justify-between items-center bg-base-300 p-5 rounded-sm">
            <div class="text-lg font-bold">
                <RxModalTextNode ModalId="delete-todo-modal" />
            </div>
        </div>
        <form method="dialog">
            <div class="p-5">
                This is a destructive operation. Are you sure?
            </div>
            <div class="modal-action">
                <RxModalDismiss ModalId="delete-todo-modal" autofocus class="btn btn-neutral">
                    Cancel
                </RxModalDismiss>
                <RxModalAction
                    ModalId="delete-todo-modal"
                    hx-delete="/todos"
                    hx-disabled-elt="this"
                    class="btn btn-error">
                    Delete
                </RxModalAction>
            </div>
        </form>
    </div>
</dialog>
@* END NEW *@

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
                <div id="todos-list">
                    @foreach (var todo in Model) {
                        <TodosItem Model="@(todo)" />
                    }
                </div>
                <TodosNew Model="@(new())"/>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public IEnumerable<TodoModel> Model { get; set; } = null!;
}
```

**_Update the `TodosHandler` with the `//NEW` code._**

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();

        router.MapPatch("/todos/{id}", Patch)
            .AllowAnonymous();

        //NEW
        router.MapDelete("/todos/{id}", Delete)
            .AllowAnonymous();
        //END NEW
    }

    public static IResult Get(
        HttpResponse response) {
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            false,
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
    }

    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model,
        ValidationContext validationContext) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todos-new");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosNew, TodoModel>(model);
        }
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            ResetForm = true,
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    public static IResult Patch(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NoContent();
        }
        todo.IsComplete = !todo.IsComplete;
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"Updated TODO to {(todo.IsComplete ? "completed" : "not completed")}."))
            .Add(new HxFocusTrigger($"#todos-item-completed-{todo.Id}"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    //NEW
    public static IResult Delete(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is not null) {
            FakeDbContext.Todos.Remove(todo);
        }
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#delete-todo-modal"))
            .Add(new HxToastTrigger("#success-toast", $"TODO deleted."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        response.HxRetarget($"#todos-item-{id}");
        response.HxReswap("outerHTML");
        return TypedResults.Ok();
    }
    //END NEW
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). You should be able to delete TODOs._**

### Add a Update TODO feature

**_Update the TodosItem component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@if (Model.ResetForm) {
    <TodosNew Model="@(new())" />
}

<div id="@($"todos-item-{Model.Id}")" class="card card-border border-base-300 bg-base-200 w-full mb-2">
    <div class="card-body">
        <div class="flex gap-2 justify-between items-center">
            <div class="flex min-w-20">
                <div>
                    <Checkbox
                        Id="@($"todos-item-completed-{Model.Id}")"
                        PropertyName="@(nameof(Model.IsComplete))"
                        IsChecked="@(Model.IsComplete)"
                        aria-label="Complete"
                        hx-patch="@($"/todos/{Model.Id}")"
                        hx-target="@($"#todos-item-{Model.Id}")"
                        hx-swap="outerHTML"
                        >
                    </Checkbox>
                </div>
            </div>
            <div class="flex grow justify-center py-2">
                <h2 class="card-title">@(Model.Title)</h2>
            </div>
            <div class="flex justify-end gap-x-2 min-w-20">
                @* NEW *@
                <RxModalTrigger
                    Id="@($"todos-item-edit-{Model.Id}")"
                    ModalId="update-todo-modal"
                    RouteValue="@(Model.Id)"
                    TextNodeValue="@($"Update {Model.Title}?")"
                    class="btn btn-square btn-sm btn-primary"
                    aria-label="@($"Update {Model.Title}?")">
                    <span class="text-xl">&#9998;</span>
                </RxModalTrigger>
                @* END NEW *@
                <RxModalTrigger
                    ModalId="delete-todo-modal"
                    RouteValue="@(Model.Id)"
                    TextNodeValue="@($"Delete {Model.Title}?")"
                    class="btn btn-square btn-sm btn-error"
                    aria-label="@($"Delete {Model.Title}?")">
                    <span class="text-xl">&#x2715;</span>
                </RxModalTrigger>
            </div>
        </div>
        <div class="flex justify-center">
            @if (Model.IsComplete) {
                <span class="bg-success text-success-content rounded-full px-2 font-semibold">COMPLETED</span>
            } else {
                <span>&nbsp;</span>
            }
        </div>
        <p class="whitespace-pre">
            @(Model.Description)
        </p>
        <div class="flex justify-end text-xs">
            <RxUtcToLocal DateInput="@(Model.LastUpdated!.Value)" />
        </div>
    </div>
</div>

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Update the TodosPage component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<IEnumerable<TodoModel>>

<HeadContent>
    <title>TODOs</title>
</HeadContent>

<dialog id="delete-todo-modal" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box">
        <div class="flex justify-between items-center bg-base-300 p-5 rounded-sm">
            <div class="text-lg font-bold">
                <RxModalTextNode ModalId="delete-todo-modal" />
            </div>
        </div>
        <form method="dialog">
            <div class="p-5">
                This is a destructive operation. Are you sure?
            </div>
            <div class="modal-action">
                <RxModalDismiss ModalId="delete-todo-modal" autofocus class="btn btn-neutral">
                    Cancel
                </RxModalDismiss>
                <RxModalAction
                    ModalId="delete-todo-modal"
                    hx-delete="/todos"
                    hx-disabled-elt="this"
                    class="btn btn-error">
                    Delete
                </RxModalAction>
            </div>
        </form>
    </div>
</dialog>

@* NEW *@
<dialog id="update-todo-modal" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box">
        <div class="flex justify-between items-center bg-base-300 p-5 rounded-sm">
            <div class="text-lg font-bold">
                <RxModalTextNode ModalId="update-todo-modal" />
            </div>
        </div>
        <form method="dialog">
            <RxModalAsyncContent
                Id="todo-update-modal-content"
                ModalId="update-todo-modal"
                RenderFromRoute="/todos">
                <FallbackContent />
            </RxModalAsyncContent>
        </form>
    </div>
</dialog>
@* END NEW *@

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
                <div id="todos-list">
                    @foreach (var todo in Model) {
                        <TodosItem Model="@(todo)" />
                    }
                </div>
                <TodosNew Model="@(new())"/>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public IEnumerable<TodoModel> Model { get; set; } = null!;
}
```

**_Create a new `TodosUpdateModal.razor` file in `Components/Todos` and add the following code._**

```csharp
@implements IComponentModel<TodoModel>

<div id="todo-update-modal-content">
    <TodosForm Model="@(Model)" />
    <div class="modal-action">
        <RxModalDismiss ModalId="update-todo-modal" autofocus class="btn btn-neutral">
            Cancel
        </RxModalDismiss>
    </div>
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

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();

        router.MapPatch("/todos/{id}", Patch)
            .AllowAnonymous();

        router.MapDelete("/todos/{id}", Delete)
            .AllowAnonymous();

        //NEW
        router.MapGet("/todos/{id}", GetUpdateModal)
            .AllowAnonymous();
        //END NEW
    }

    public static IResult Get(
        HttpResponse response) {
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            false,
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
    }

    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model,
        ValidationContext validationContext) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todos-new");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosNew, TodoModel>(model);
        }
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            ResetForm = true,
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    public static IResult Patch(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NoContent();
        }
        todo.IsComplete = !todo.IsComplete;
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"Updated TODO to {(todo.IsComplete ? "completed" : "not completed")}."))
            .Add(new HxFocusTrigger($"#todos-item-completed-{todo.Id}"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    public static IResult Delete(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is not null) {
            FakeDbContext.Todos.Remove(todo);
        }
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#delete-todo-modal"))
            .Add(new HxToastTrigger("#success-toast", $"TODO deleted."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        response.HxRetarget($"#todos-item-{id}");
        response.HxReswap("outerHTML");
        return TypedResults.Ok();
    }

    //NEW
    public static IResult GetUpdateModal(
        HttpResponse response,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NotFound();
        }
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        return response.RenderComponent<TodosUpdateModal, TodoModel>(model);
    }
    //END NEW
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). You should be able to trigger the TODO update modal._**

**_Update the TodosUpdateModal component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

<div id="todo-update-modal-content">
    <TodosForm Model="@(Model)" />
    <div class="modal-action">
        <RxModalDismiss ModalId="update-todo-modal" autofocus class="btn btn-neutral">
            Cancel
        </RxModalDismiss>
        @* NEW *@
        <RxModalAction
            ModalId="update-todo-modal"
            hx-put="@($"/todos/{Model.Id}")"
            hx-disabled-elt="this"
            class="btn btn-primary">
            Save
        </RxModalAction>
        @* END NEW *@
    </div>
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

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();

        router.MapPatch("/todos/{id}", Patch)
            .AllowAnonymous();

        router.MapDelete("/todos/{id}", Delete)
            .AllowAnonymous();

        router.MapGet("/todos/{id}", GetUpdateModal)
            .AllowAnonymous();

        //NEW
        router.MapPut("/todos/{id}", Put)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();
        //NEW
    }

    public static IResult Get(
        HttpResponse response) {
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            false,
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
    }

    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model,
        ValidationContext validationContext) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todos-new");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosNew, TodoModel>(model);
        }
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            ResetForm = true,
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    public static IResult Patch(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NoContent();
        }
        todo.IsComplete = !todo.IsComplete;
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"Updated TODO to {(todo.IsComplete ? "completed" : "not completed")}."))
            .Add(new HxFocusTrigger($"#todos-item-completed-{todo.Id}"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    public static IResult Delete(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is not null) {
            FakeDbContext.Todos.Remove(todo);
        }
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#delete-todo-modal"))
            .Add(new HxToastTrigger("#success-toast", $"TODO deleted."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        response.HxRetarget($"#todos-item-{id}");
        response.HxReswap("outerHTML");
        return TypedResults.Ok();
    }

    public static IResult GetUpdateModal(
        HttpResponse response,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NotFound();
        }
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        return response.RenderComponent<TodosUpdateModal, TodoModel>(model);
    }

    //NEW
    public static IResult Put(
        HttpResponse response,
        IHxTriggers hxTriggers,
        ValidationContext validationContext,
        string id,
        TodoModel model) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todo-update-modal-content");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosUpdateModal, TodoModel>(model);
        }
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NotFound();
        }
        todo.Title = model.Title!;
        todo.Description = model.Description!;
        todo.LastUpdated = DateTime.UtcNow;
        model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#update-todo-modal"))
            .Add(new HxToastTrigger("#success-toast", $"TODO updated."))
            .Add(new HxFocusTrigger($"#todos-item-edit-{todo.Id}"))
            .Build();
        response.HxRetarget($"#todos-item-{id}");
        response.HxReswap("outerHTML");
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }
    //END NEW
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). You should be able to update the TODO._**

### EXTRA CREDIT - Add a Change Validation feature

**_Update the TodosForm component with the `@* NEW *@` code._**

```csharp
@implements IComponentModel<TodoModel>

@* NEW *@
<RxChangeValidator
    Id="todos-validator"
    ValidationPostRoute="/todos/validate"
    IsDisabled="@(false)">
    <input type="hidden" name="@(nameof(Model.Id))" value="@(Model.Id)">
@* END NEW *@
    <div>
        <!-- Title Field -->
        <Field
            Id="@($"{nameof(Model.Title)}{Model.Id}")"
            PropertyName="@(nameof(Model.Title))"
            Value="@(Model.Title)"
            Label="Title"
            InputType="text"
            UseOpacityForValidationErrors="@(true)"
            @* NEW *@
            AllowValidateOnChange="@(true)"
            @* END NEW *@
            maxlength="80"
            placeholder="e.g., Learn the RazorX meta-framework!">
        </Field>
    </div>
    <div>
        <!-- Description Memo Field -->
        <MemoField
            Id="@($"{nameof(Model.Description)}{Model.Id}")"
            PropertyName="@(nameof(Model.Description))"
            Value="@(Model.Description)"
            Label="Description"
            MaxLength="500"
            UseOpacityForValidationErrors="@(true)"
            @* NEW *@
            AllowValidateOnChange="@(true)"
            @* END NEW *@
            placeholder="e.g., This includes reading the htmx documentation and checking out Tailwind and daisyUI.">
        </MemoField>
    </div>
@* NEW *@
</RxChangeValidator>
@* END NEW *@

@code {
    [Parameter] public TodoModel Model { get; set; } = null!;
}
```

**_Update the `TodosHandler` with the `//NEW` code._**

```csharp
using Demo.Rx;

namespace Demo.Components.Todos;

public class TodosHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/todos", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/todos", Post)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();

        router.MapPatch("/todos/{id}", Patch)
            .AllowAnonymous();

        router.MapDelete("/todos/{id}", Delete)
            .AllowAnonymous();

        router.MapGet("/todos/{id}", GetUpdateModal)
            .AllowAnonymous();

        router.MapPut("/todos/{id}", Put)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();

        //NEW
        router.MapPost("/todos/validate", Validate)
            .AllowAnonymous()
            .WithRxValidation<TodosValidator>();
        //END NEW
    }

    public static IResult Get(
        HttpResponse response) {
        var model = FakeDbContext.Todos.Select(x => new TodoModel(
            false,
            x.Id,
            x.Title,
            x.Description,
            x.IsComplete,
            x.LastUpdated));
        return response.RenderComponent<TodosPage, IEnumerable<TodoModel>>(model);
    }

    public static IResult Post(
        HttpResponse response,
        IHxTriggers hxTriggers,
        TodoModel model,
        ValidationContext validationContext) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todos-new");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosNew, TodoModel>(model);
        }
        var todo = new Todo {
            Id = Guid.NewGuid().ToString(),
            Title = model.Title!,
            Description = model.Description!,
            LastUpdated = DateTime.UtcNow,
            IsComplete = false
        };
        FakeDbContext.Todos.Add(todo);
        model = model with {
            ResetForm = true,
            Id = todo.Id,
            LastUpdated = todo.LastUpdated
        };
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"New TODO created."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    public static IResult Patch(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NoContent();
        }
        todo.IsComplete = !todo.IsComplete;
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#success-toast", $"Updated TODO to {(todo.IsComplete ? "completed" : "not completed")}."))
            .Add(new HxFocusTrigger($"#todos-item-completed-{todo.Id}"))
            .Build();
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    public static IResult Delete(
        HttpResponse response,
        IHxTriggers hxTriggers,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is not null) {
            FakeDbContext.Todos.Remove(todo);
        }
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#delete-todo-modal"))
            .Add(new HxToastTrigger("#success-toast", $"TODO deleted."))
            .Add(new HxFocusTrigger($"#{nameof(todo.Title)}-input"))
            .Build();
        response.HxRetarget($"#todos-item-{id}");
        response.HxReswap("outerHTML");
        return TypedResults.Ok();
    }

    public static IResult GetUpdateModal(
        HttpResponse response,
        string id) {
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NotFound();
        }
        var model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        return response.RenderComponent<TodosUpdateModal, TodoModel>(model);
    }

    public static IResult Put(
        HttpResponse response,
        IHxTriggers hxTriggers,
        ValidationContext validationContext,
        string id,
        TodoModel model) {
        if (validationContext.Errors.Count != 0) {
            response.HxRetarget("#todo-update-modal-content");
            response.HxReswap("outerHTML");
            return response.RenderComponent<TodosUpdateModal, TodoModel>(model);
        }
        var todo = FakeDbContext.Todos.Where(x => x.Id == id).SingleOrDefault();
        if (todo is null) {
            return TypedResults.NotFound();
        }
        todo.Title = model.Title!;
        todo.Description = model.Description!;
        todo.LastUpdated = DateTime.UtcNow;
        model = new TodoModel(
            false,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsComplete,
            todo.LastUpdated);
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#update-todo-modal"))
            .Add(new HxToastTrigger("#success-toast", $"TODO updated."))
            .Add(new HxFocusTrigger($"#todos-item-edit-{todo.Id}"))
            .Build();
        response.HxRetarget($"#todos-item-{id}");
        response.HxReswap("outerHTML");
        return response.RenderComponent<TodosItem, TodoModel>(model);
    }

    //NEW
    public static IResult Validate(
        HttpResponse response,
        TodoModel model) {
        return response.RenderComponent<TodosForm, TodoModel>(model);
    }
    //END NEW
}
```

**_Run the `dotnet watch` command in the terminal to launch the app (or `Ctrl-R` to rebuild if it is running). TODO form validation errors will display in real-time, as appropriate._**

**_Congratulations! You have successfully completed the tutorial - Happy Coding!!!_**
