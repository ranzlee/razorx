using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using RxTemplate.Blob;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Blob;

public class BlobHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.MapGet("/examples/blob", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        // single file upload
        router.MapPost("/examples/blob", PostFile)
            .AllowAnonymous();

        // multiple files upload
        router.MapPost("/examples/blobs", PostFiles)
            .AllowAnonymous();

        router.MapDelete("/examples/blob/{path}/{id}", Delete)
            .AllowAnonymous();

        // download
        router.MapGet("/examples/blob/{path}/{id}", Download)
            .AllowAnonymous()
            // This is a [download] href, so we need to skip route handling
            .WithRxSkipRouteHandling();
    }

    public static async Task<IResult> Get(
        HttpResponse response,
        IBlobProvider blobProvider,
        ILogger<BlobHandler> logger,
        CancellationToken cancellationToken) {
        var m = new BlobPageModel {
            SingleBlob = (await blobProvider.ListAsync("single", cancellationToken)).FirstOrDefault(),
            ListBlobs = [.. await blobProvider.ListAsync("multi", cancellationToken)]
        };
        return response.RenderComponent<BlobPage, BlobPageModel>(m, logger);
    }

    [DisableRequestTimeout]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public static async Task<IResult> PostFile(
        HttpResponse response,
        IBlobProvider blobProvider,
        IFormFile file,
        IHxTriggers hxTriggers,
        ILogger<BlobHandler> logger,
        CancellationToken cancellationToken) {
        await using var stream = file.OpenReadStream();
        var m = await blobProvider.UploadAsync(stream, "single", file.FileName, cancellationToken);
        hxTriggers
            .With(response)
            .Add(new HxFocusTrigger("#example-blob-link"))
            .Add(new HxToastTrigger("#blob-toast", "BLOB added"))
            .Build();
        return response.RenderComponent<Blob, BlobModel>(m, logger);
    }

    [DisableRequestTimeout]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public static async Task<IResult> PostFiles(
        HttpResponse response,
        IBlobProvider blobProvider,
        IFormFileCollection files,
        IHxTriggers hxTriggers,
        ILogger<BlobHandler> logger,
        CancellationToken cancellationToken) {
        var m = new List<BlobModel>();
        foreach (var file in files) {
            await using var stream = file.OpenReadStream();
            m.Add(await blobProvider.UploadAsync(stream, "multi", file.FileName, cancellationToken));
        }
        hxTriggers
            .With(response)
            .Add(new HxFocusTrigger($"#example-blobs-input"))
            .Add(new HxToastTrigger("#blob-toast", $"BLOB{(files.Count > 1 ? "s" : "")} added"))
            .Build();
        return response.RenderComponent<BlobList, IEnumerable<BlobModel>>(m, logger);
    }

    public static async Task<IResult> Delete(
        HttpResponse response,
        string path,
        string id,
        IBlobProvider blobProvider,
        IHxTriggers hxTriggers,
        ILogger<BlobHandler> logger,
        CancellationToken cancellationToken) {
        await blobProvider.DeleteAsync($"{path}/{id}", cancellationToken);
        var triggerBuilder = hxTriggers.With(response);
        triggerBuilder.Add(new HxCloseModalTrigger("#delete-modal"));
        triggerBuilder.Add(new HxToastTrigger("#blob-toast", "BLOB removed"));
        response.HxReswap("outerHTML transition:true");
        if (path == "single") {
            triggerBuilder.Add(new HxFocusTrigger("#example-blob-input"));
            triggerBuilder.Build();
            response.HxRetarget("#example-blob");
            return response.RenderComponent<SingleBlob>(logger);
        }
        var l = await blobProvider.ListAsync("multi", cancellationToken);
        triggerBuilder.Add(new HxFocusTrigger("#example-blobs-input"));
        triggerBuilder.Build();
        response.HxRetarget("#blob-list");
        return response.RenderComponent<BlobList, IEnumerable<BlobModel>>(l, logger);
    }

    public static async Task<IResult> Download(
        HttpResponse response,
        string path,
        string id,
        IBlobProvider blobProvider,
        CancellationToken cancellationToken) {
        var blobResult = await blobProvider.GetAsync($"{path}/{id}", cancellationToken);
        if (blobResult is null) {
            return TypedResults.NotFound();
        }
        response.ContentLength = blobResult.Details.ContentLength;
        return Results.File(blobResult.Content, contentType: "application/octet-stream");
    }
}
