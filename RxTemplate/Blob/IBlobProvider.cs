using System.Globalization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RxTemplate.Blob;

/// <summary>
/// This is an example basic BLOB service implementation. While it is correct for Azure
/// BLOB storage, it is incomplete. You will need to implement a BLOB service that meets 
/// your specific requirements using Azure, AWS, a file share, or whatever storage provider
/// you choose.
/// </summary>
public interface IBlobProvider {
    Task<BlobModel> UploadAsync(Stream stream, string fileName, string group);
    Task<BlobDownloadStreamingResult?> GetAsync(string id);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<BlobModel>> ListAsync(string group);
}

public static class ProviderConfig {
    public static void AddBlobProvider(this IServiceCollection services) {
        services.AddSingleton<IBlobProvider, BlobProvider>();
    }
}

file sealed class BlobProvider : IBlobProvider {
    private readonly BlobContainerClient _container;

    public BlobProvider() {
        _container = new BlobContainerClient("UseDevelopmentStorage=true", "rxtemplate");
        _container.CreateIfNotExists();
    }

    public async Task<BlobModel> UploadAsync(Stream stream, string fileName, string group) {
        var name = Guid.NewGuid().ToString();
        var blobName = name;
        var length = stream.Length;
        var uploaded = DateTime.UtcNow;
        var metadata = new Dictionary<string, string>
        {
            { "group", group },
            { "filename", fileName },
            { "filesize", length.ToString() },
            { "uploaded", uploaded.ToString(CultureInfo.InvariantCulture) }
        };
        BlobUploadOptions options = new() {
            Metadata = metadata
        };
        stream.Position = 0;
        var blobClient = _container.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, options);
        return new BlobModel {
            FileName = fileName,
            FileSize = length,
            Id = blobName,
            Uploaded = uploaded,
        };
    }

    public async Task<BlobDownloadStreamingResult?> GetAsync(string id) {
        var blobClient = _container.GetBlobClient(id);
        if (!await blobClient.ExistsAsync()) {
            return null;
        }
        return await blobClient.DownloadStreamingAsync();
    }

    public async Task<bool> DeleteAsync(string id) {
        var blobClient = _container.GetBlobClient(id);
        return await blobClient.DeleteIfExistsAsync();
    }

    public async Task<IEnumerable<BlobModel>> ListAsync(string group) {
        var list = new List<BlobItem>();
        var resultSegment = _container.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.None).AsPages();
        await foreach (var page in resultSegment) {
            foreach (var item in page.Values) {
                if (item.IsBlob && item.Blob.Metadata.Contains(new KeyValuePair<string, string>("group", group))) {
                    list.Add(item.Blob);
                }
            }
        }
        return list.Select(x => new BlobModel {
            Id = x.Name,
            FileName = x.Metadata["filename"],
            FileSize = long.Parse(x.Metadata["filesize"]),
            Uploaded = DateTime.SpecifyKind(DateTime.Parse(x.Metadata["uploaded"]), DateTimeKind.Utc)
        }).OrderByDescending(x => x.Uploaded);
    }
}