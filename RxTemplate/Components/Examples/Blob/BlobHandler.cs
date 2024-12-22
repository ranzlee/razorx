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
        var m = new BlobPageModel {
            SingleBlob = (await blobProvider.ListAsync("single")).FirstOrDefault(),
            ListBlobs = [.. (await blobProvider.ListAsync("multi"))]
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
        ILogger<BlobHandler> logger) {
        await using var stream = file.OpenReadStream();
        var m = await blobProvider.UploadAsync(stream, "single", file.FileName);
        hxTriggers
            .With(response)
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
        ILogger<BlobHandler> logger) {
        var m = new List<BlobModel>();
        foreach (var file in files) {
            await using var stream = file.OpenReadStream();
            m.Add(await blobProvider.UploadAsync(stream, "multi", file.FileName));
        }
        hxTriggers
            .With(response)
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
        ILogger<BlobHandler> logger) {
        await blobProvider.DeleteAsync($"{path}/{id}");
        hxTriggers
            .With(response)
            .Add(new HxCloseModalTrigger("#delete-modal"))
            .Add(new HxToastTrigger("#blob-toast", "BLOB removed"))
            .Build();
        response.HxReswap("outerHTML transition:true");
        if (path == "single") {
            response.HxRetarget("#example-blob");
            return response.RenderComponent<SingleBlob>(logger);
        }
        var l = await blobProvider.ListAsync("multi");
        response.HxRetarget("#blob-list");
        return response.RenderComponent<BlobList, IEnumerable<BlobModel>>(l, logger);
    }

    public static async Task<IResult> Download(
        HttpResponse response,
        string path,
        string id,
        IBlobProvider blobProvider) {
        var blobResult = await blobProvider.GetAsync($"{path}/{id}");
        if (blobResult is null) {
            return TypedResults.NotFound();
        }
        response.ContentLength = blobResult.Details.ContentLength;
        return Results.File(blobResult.Content, contentType: "application/octet-stream");
    }
}
