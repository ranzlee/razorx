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
    Task<BlobModel> UploadAsync(Stream stream, string prefix, string fileName, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlobModel>> ListAsync(string prefix, CancellationToken cancellationToken = default);
    Task<BlobDownloadStreamingResult?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
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

    public async Task<BlobModel> UploadAsync(Stream stream, string prefix, string fileName, CancellationToken cancellationToken = default) {
        var name = Guid.NewGuid().ToString();
        var blobName = string.IsNullOrWhiteSpace(prefix) ? name : $"{prefix.Replace("\\", "/").TrimEnd('/').ToLower().Trim()}/{name}";
        var length = stream.Length;
        var uploaded = DateTime.UtcNow;
        var metadata = new Dictionary<string, string>
        {
            { "filename", fileName },
            { "filesize", length.ToString() },
            { "uploaded", uploaded.ToString(CultureInfo.InvariantCulture) }
        };
        BlobUploadOptions options = new() {
            Metadata = metadata
        };
        stream.Position = 0;
        var blobClient = _container.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, options, cancellationToken);
        return new BlobModel {
            FileName = fileName,
            FileSize = length,
            Id = blobName,
            Uploaded = uploaded,
        };
    }

    public async Task<IEnumerable<BlobModel>> ListAsync(string? prefix, CancellationToken cancellationToken = default) {
        prefix = prefix is null
            ? ""
            : prefix.Replace("\\", "/").ToLower().Trim();
        var list = new List<BlobItem>();
        var resultSegment = _container.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.None, null, string.IsNullOrWhiteSpace(prefix)
            ? null
            : prefix, cancellationToken).AsPages();
        await foreach (var blobHierarchyItemPage in resultSegment) {
            foreach (var blobHierarchyItem in blobHierarchyItemPage.Values) {
                if (blobHierarchyItem.IsBlob) {
                    list.Add(blobHierarchyItem.Blob);
                }
            }
        }
        return list.Select(x => new BlobModel {
            Id = x.Name,
            FileName = x.Metadata["filename"],
            FileSize = long.Parse(x.Metadata["filesize"]),
            Uploaded = DateTime.SpecifyKind(DateTime.Parse(x.Metadata["uploaded"]), DateTimeKind.Utc)
        }).OrderBy(x => x.Uploaded);
    }

    public async Task<BlobDownloadStreamingResult?> GetAsync(string id, CancellationToken cancellationToken = default) {
        var blobClient = _container.GetBlobClient(id);
        if (!await blobClient.ExistsAsync(cancellationToken)) {
            return null;
        }
        return await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default) {
        var blobClient = _container.GetBlobClient(id);
        return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}