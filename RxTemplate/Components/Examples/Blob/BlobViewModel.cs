using RxTemplate.Blob;

namespace RxTemplate.Components.Examples.Blob;

public record BlobViewModel {
    public BlobModel? SingleBlob { get; set; }
    public IEnumerable<BlobModel> ListBlobs { get; set; } = [];
}