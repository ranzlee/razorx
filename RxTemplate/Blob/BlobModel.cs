namespace RxTemplate.Blob;

public record BlobModel {
    public string Id { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public long FileSize { get; init; }
    public DateTime Uploaded { get; init; }
}
