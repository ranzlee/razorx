using System.Net;

namespace RxTemplate.Rx;

/// <summary>
/// Interface for a class that contains endpoints.
/// </summary>
public interface IRequestHandler { }

/// <summary>
/// Interface for a component that represents a page layout.
/// </summary>
public interface IRootComponent {
    public Type MainContent { get; set; }
    public Dictionary<string, object?> MainContentParameters { get; set; }
}

/// <summary>
/// Interface for a component with a model.
/// </summary>
/// <typeparam name="TModel">The model to bind to the component.</typeparam>
public interface IComponentModel<TModel> {
    TModel Model { get; set; }
}

public interface ISuspendedComponent {
    bool IsSuspended { get; set; }
    string RenderFromRoute { get; set; }
    string SpinnerId { get; set; }
}

/// <summary>
/// Contains error details for HTTP errors that occur in the pipeline.
/// 400-499 for client and 500+ for server
/// </summary>
/// <param name="StatusCode">HTTP status code</param>
public record ErrorModel(HttpStatusCode StatusCode) {
    public HttpStatusCode StatusCode { get; init; } = StatusCode;
}
