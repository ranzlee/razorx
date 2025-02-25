using RxTemplate.Components.Examples;
using RxTemplate.Components.Examples.Blob;
using RxTemplate.Components.Examples.Counter;
using RxTemplate.Components.Examples.Crud;
using RxTemplate.Components.Examples.Form;
using RxTemplate.Components.Layout;
using RxTemplate.Rx;

namespace RxTemplate.Router;

public static class Examples {
    public static RouteGroupBuilder WithExamplesRoutes(this RouteGroupBuilder router) {

        // Examples page
        router.AddRoutePath(RequestType.GET, "/examples", ExamplesHandler.Get)
            .AllowAnonymous()
            .WithRxRootComponent<App>();

        // Counter example routes
        router.AddRoutePath(RequestType.GET, "/examples/counter", CounterHandler.Get)
            .AllowAnonymous()
            .WithRxRootComponent<App>();

        router.AddRoutePath(RequestType.POST, "/examples/counter/update", CounterHandler.UpdateCounter)
            // The WithRxValidation filter execute the model's Validator
            // before the handler is invoked.
            .WithRxValidation<CounterValidator>()
            .AllowAnonymous();

        // Form example routes
        router.AddRoutePath(RequestType.GET, "/examples/form", FormHandler.Get)
            .AllowAnonymous()
            .WithRxRootComponent<App>();

        router.AddRoutePath(RequestType.PATCH, "/examples/form/validate", FormHandler.ValidateForm)
            .WithRxValidation<FormValidator>()
            .AllowAnonymous();

        router.AddRoutePath(RequestType.POST, "/examples/form/submit", FormHandler.SubmitForm)
            .WithRxValidation<FormValidator>()
            .AllowAnonymous();

        // CRUD example routes
        router.AddRoutePath(RequestType.GET, "/examples/crud/blocking", CrudHandler.GetBlocking)
            .AllowAnonymous()
            .WithRxRootComponent<App>();

        router.AddRoutePath(RequestType.GET, "/examples/crud/non-blocking", CrudHandler.GetNonBlocking)
            .AllowAnonymous()
            .WithRxRootComponent<App>();

        router.AddRoutePath(RequestType.GET, "/examples/crud/filter", CrudHandler.GetFilter)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.GET, "/examples/crud/delete-modal/{id:int}", CrudHandler.GetDeleteModal)
            .RequireAuthorization();

        router.AddRoutePath(RequestType.GET, "/examples/crud/save-modal/{id:int}", CrudHandler.GetSaveModal)
            .RequireAuthorization();

        router.AddRoutePath(RequestType.GET, "/examples/crud/grid", CrudHandler.GetGrid)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.DELETE, "/examples/crud/{id:int}", CrudHandler.DeleteItem)
            .RequireAuthorization();

        router.AddRoutePath(RequestType.PUT, "/examples/crud/{id:int}", CrudHandler.SaveItem)
            .WithRxValidation<ItemValidator>()
            .RequireAuthorization();

        router.AddRoutePath(RequestType.POST, "/examples/crud", CrudHandler.SaveItem)
            .WithRxValidation<ItemValidator>()
            .RequireAuthorization();

        // BLOBs
        router.AddRoutePath(RequestType.GET, "/examples/blob", BlobHandler.Get)
            .AllowAnonymous()
            .WithRxRootComponent<App>();

        // single file upload
        router.AddRoutePath(RequestType.POST, "/examples/blob", BlobHandler.PostFile)
            .AllowAnonymous();

        // multiple files upload
        router.AddRoutePath(RequestType.POST, "/examples/blobs", BlobHandler.PostFiles)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.DELETE, "/examples/blob/{path}/{id}", BlobHandler.Delete)
            .AllowAnonymous();

        // download
        router.AddRoutePath(RequestType.GET, "/examples/blob/{path}/{id}", BlobHandler.Download)
            .AllowAnonymous()
            // This is a [download] href, so we need to skip route handling
            .WithRxSkipRouteHandling();

        return router;
    }
}