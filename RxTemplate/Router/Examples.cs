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
        router.AddRoutePath(HTTP.GET, "/examples", ExamplesHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();

        // Counter example routes
        router.AddRoutePath(HTTP.GET, "/examples/counter", CounterHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();

        router.AddRoutePath(HTTP.POST, "/examples/counter/update", CounterHandler.UpdateCounter)
            // The WithValidation filter execute the model's Validator
            // before the handler is invoked.
            .WithValidation<CounterValidator>()
            .AllowAnonymous();

        // Form example routes
        router.AddRoutePath(HTTP.GET, "/examples/form", FormHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();

        router.AddRoutePath(HTTP.POST, "/examples/form/submit", FormHandler.SubmitForm)
            .WithValidation<FormValidator>()
            .AllowAnonymous();

        // CRUD example routes
        router.AddRoutePath(HTTP.GET, "/examples/crud", CrudHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();

        router.AddRoutePath(HTTP.GET, "/examples/crud/filter", CrudHandler.GetFilter)
            .AllowAnonymous();

        router.AddRoutePath(HTTP.GET, "/examples/crud/delete-modal/{id:int}", CrudHandler.GetDeleteModal)
            .AllowAnonymous();

        router.AddRoutePath(HTTP.GET, "/examples/crud/save-modal/{id:int}", CrudHandler.GetSaveModal)
            .AllowAnonymous();

        router.AddRoutePath(HTTP.GET, "/examples/crud/grid", CrudHandler.GetGrid)
            .AllowAnonymous();

        router.AddRoutePath(HTTP.DELETE, "/examples/crud/{id:int}", CrudHandler.DeleteItem)
            .AllowAnonymous();

        router.AddRoutePath(HTTP.PUT, "/examples/crud/{id:int}", CrudHandler.SaveItem)
            .WithValidation<ItemValidator>()
            .AllowAnonymous();

        router.AddRoutePath(HTTP.POST, "/examples/crud", CrudHandler.SaveItem)
            .WithValidation<ItemValidator>()
            .AllowAnonymous();

        // BLOBs
        router.AddRoutePath(HTTP.GET, "/examples/blob", BlobHandler.Get)
            .AllowAnonymous()
            .PageRouteFor<App>();

        // single file upload
        router.AddRoutePath(HTTP.POST, "/examples/blob", BlobHandler.PostFile)
            .AllowAnonymous();

        // multiple files upload
        router.AddRoutePath(HTTP.POST, "/examples/blobs", BlobHandler.PostFiles)
            .AllowAnonymous();

        router.AddRoutePath(HTTP.DELETE, "/examples/blob/{path}/{id}", BlobHandler.Delete)
            .AllowAnonymous();

        // download
        router.AddRoutePath(HTTP.GET, "/examples/blob/{path}/{id}", BlobHandler.Download)
            .AllowAnonymous()
            // This is a [download] href, so we need to skip route filtering
            .SkipRouteFilter();

        return router;
    }
}