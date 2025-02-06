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
            .PageRouteFor<App>();

        // Counter example routes
        router.AddRoutePath(RequestType.GET, "/examples/counter", CounterHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();

        router.AddRoutePath(RequestType.POST, "/examples/counter/update", CounterHandler.UpdateCounter)
            // The WithValidation filter execute the model's Validator
            // before the handler is invoked.
            .WithValidation<CounterValidator>()
            .AllowAnonymous();

        // Form example routes
        router.AddRoutePath(RequestType.GET, "/examples/form", FormHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();
        
        router.AddRoutePath(RequestType.PATCH, "/examples/form/validate", FormHandler.ValidateForm)
            .WithValidation<FormValidator>()
            .AllowAnonymous();

        router.AddRoutePath(RequestType.POST, "/examples/form/submit", FormHandler.SubmitForm)
            .WithValidation<FormValidator>()
            .AllowAnonymous();

        // CRUD example routes
        router.AddRoutePath(RequestType.GET, "/examples/crud/blocking", CrudHandler.GetBlocking)
            .AllowAnonymous()
            .PageRouteFor<App>();

        router.AddRoutePath(RequestType.GET, "/examples/crud/non-blocking", CrudHandler.GetNonBlocking)
            .AllowAnonymous()
            .PageRouteFor<App>();

        router.AddRoutePath(RequestType.GET, "/examples/crud/filter", CrudHandler.GetFilter)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.GET, "/examples/crud/delete-modal/{id:int}", CrudHandler.GetDeleteModal)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.GET, "/examples/crud/save-modal/{id:int}", CrudHandler.GetSaveModal)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.GET, "/examples/crud/grid", CrudHandler.GetGrid)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.DELETE, "/examples/crud/{id:int}", CrudHandler.DeleteItem)
            .AllowAnonymous();

        router.AddRoutePath(RequestType.PUT, "/examples/crud/{id:int}", CrudHandler.SaveItem)
            .WithValidation<ItemValidator>()
            .AllowAnonymous();

        router.AddRoutePath(RequestType.POST, "/examples/crud", CrudHandler.SaveItem)
            .WithValidation<ItemValidator>()
            .AllowAnonymous();

        // BLOBs
        router.AddRoutePath(RequestType.GET, "/examples/blob", BlobHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();

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
            // This is a [download] href, so we need to skip route filtering
            .SkipRouteFilter();

        return router;
    }
}