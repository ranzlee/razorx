using RxTemplate.Blob;

namespace RxTemplate.Components.Examples.Blob;

public record BlobPageModel(BlobModel? SingleBlob, IEnumerable<BlobModel> ListBlobs);