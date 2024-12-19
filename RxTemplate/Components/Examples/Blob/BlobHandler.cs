using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using RxTemplate.Blob;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Blob;

public class BlobHandler : IRequestHandler {
    public static async Task<IResult> Get(
        HttpResponse response,
        IBlobProvider blobProvider,
        ILogger<BlobHandler> logger) {
        var m = new BlobViewModel {
            SingleBlob = (await blobProvider.ListAsync("single")).FirstOrDefault(),
            ListBlobs = [.. (await blobProvider.ListAsync("multi"))]
        };
        return response.RenderComponent<BlobPage, BlobViewModel>(m, logger);
    }

    [DisableRequestTimeout]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public static async Task<IResult> PostFile(
        HttpResponse response,
        IBlobProvider blobProvider,
        IFormFile file,
        IHxTriggers hxTriggers) {
        await using var stream = file.OpenReadStream();
        var m = await blobProvider.UploadAsync(stream, file.FileName, "single");
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#blob-toast", "BLOB added"))
            .Build();
        return response.RenderComponent<Blob, BlobModel>(m);
    }

    [DisableRequestTimeout]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public static async Task<IResult> PostFiles(
        HttpResponse response,
        IBlobProvider blobProvider,
        IFormFileCollection files,
        IHxTriggers hxTriggers) {
        var m = new List<BlobModel>();
        foreach (var file in files) {
            await using var stream = file.OpenReadStream();
            m.Add(await blobProvider.UploadAsync(stream, file.FileName, "multi"));
        }
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#blob-toast", $"BLOB{(files.Count > 1 ? "s" : "")} added"))
            .Build();
        return response.RenderComponent<BlobList, IEnumerable<BlobModel>>(m);
    }

    public static async Task<IResult> Delete(
        HttpResponse response,
        string id,
        IBlobProvider blobProvider,
        IHxTriggers hxTriggers) {
        await blobProvider.DeleteAsync(id);
        var m = new BlobViewModel {
            SingleBlob = (await blobProvider.ListAsync("single")).FirstOrDefault(),
            ListBlobs = [.. (await blobProvider.ListAsync("multi"))]
        };
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#delete-modal"))
            .Add(new HxToastTrigger("#blob-toast", "BLOB removed"))
            .Build();
        return response.RenderComponent<BlobPage, BlobViewModel>(m);
    }

    public static async Task<IResult> Download(
        HttpResponse response,
        string id,
        IBlobProvider blobProvider) {
        var blobResult = await blobProvider.GetAsync(id);
        if (blobResult == null) {
            return TypedResults.NotFound();
        }
        response.ContentLength = blobResult.Details.ContentLength;
        return Results.File(blobResult.Content, contentType: "application/octet-stream");
    }
}
