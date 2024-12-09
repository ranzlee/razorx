using Microsoft.AspNetCore.Http.Extensions;
using System.Text;
using System.Text.Json;

namespace RxTemplate.Rx;

/// <summary>
/// HX-Trigger response header builder
/// </summary>
public interface IHxTriggers {

    /// <summary>
    /// Adds the response to associate the HX-Triggers with.
    /// </summary>
    /// <param name="response">HttpResponse</param>
    /// <returns>HxTriggerBuilder</returns>
    IHxTriggerBuilder With(HttpResponse response);
}

public static class HxTriggersService {

    /// <summary>
    /// Adds the HxTriggerBuilder to the IServiceCollection.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    public static void AddHxTriggers(this IServiceCollection services) {
        services.AddSingleton<IHxTriggers, HxTriggers>();
    }
}

public sealed class HxTriggers(ILogger<HxTriggers> logger) : IHxTriggers {
    public IHxTriggerBuilder With(HttpResponse response) {
        return new HxTriggerBuilder(response, logger);
    }
}

/// <summary>
/// HX-Trigger response header builder
/// </summary>
public interface IHxTriggerBuilder {

    /// <summary>
    /// Adds an HxTrigger instance to the response.
    /// </summary>
    /// <param name="trigger">IHxTrigger</param>
    /// <returns>IHxTriggerBuilder</returns>
    IHxTriggerBuilder AddTrigger(IHxTrigger trigger);

    /// <summary>
    /// Builds the response headers for the triggers.
    /// </summary>
    void Build();
}

public interface IHxTrigger { }

/// <summary>
/// Triggers a toast alert with the response.
/// </summary>
/// <param name="ToastSelector">The element CSS selector of the toast alert.</param>
/// <param name="Message">The message for the toast alert.</param>
/// <param name="IsError">True to show the error alert instead of the default success alert.</param>
public record HxToastTrigger(string ToastSelector, string Message) : IHxTrigger {

    public string ToastSelector { get; init; } = ToastSelector;
    public string Message { get; init; } = Message;
}

/// <summary>
/// Triggers the close action of an open modal with the response.
/// </summary>
/// <param name="ModalSelector">The element CSS selector of the modal dialog.</param>
public record HxCloseModalTrigger(string ModalSelector) : IHxTrigger {
    public string ModalSelector { get; init; } = ModalSelector;
}

/// <summary>
/// Triggers the focus of an HTML element with the response.
/// </summary>
/// <param name="ElementSelector">The element CSS selector to focus.</param>
public record HxFocusTrigger(string ElementSelector, bool ScrollIntoView = false) : IHxTrigger {
    public string ElementSelector { get; init; } = ElementSelector;
    public bool ScrollIntoView { get; init; } = ScrollIntoView;
}

/// <summary>
/// The type of storage targeted for add/remove, 
/// Transient - persists to a hidden field.
/// Session - persists in SessionStorage only for the active tab while open. 
/// Persistent persists in LocalStorage across browser tabs and sessions.
/// </summary>
public enum HxMetadataScope {
    Transient = 0,
    Session = 1,
    Persistent = 2,

}

/// <summary>
/// Triggers the set of a local or session storage item.
/// </summary>
/// <param name="Scope">The type of storage to target.</param>
/// <param name="Key">The storage key, or hidden field ID if Transient.</param>
/// <param name="Value">The value to store.</param>
public record HxSetMetadataTrigger(HxMetadataScope Scope, string Key, string Value) : IHxTrigger {
    public HxMetadataScope Scope { get; init; } = Scope;
    public string Key { get; init; } = Key;
    public string Value { get; init; } = Value;
}

/// <summary>
/// Triggers the removal of a local or session storage item.
/// </summary>
/// <param name="Scope">The type of storage to target.</param>
/// <param name="Key">The storage key, or hidden field ID is Transient.</param>
public record HxRemoveMetadataTrigger(HxMetadataScope Scope, string Key) : IHxTrigger {
    public HxMetadataScope Scope { get; init; } = Scope;
    public string Key { get; init; } = Key;
}

/// <summary>
/// The type of custom trigger. The order from start to finish is
/// normal (immediate), then after HTMX swaps the target element, then after HTMX settles the DOM.
/// </summary>
public enum HxCustomTriggerType {
    Normal = 0,
    AfterSwap = 1,
    AfterSettle = 2
}

/// <summary>
/// Triggers a custom client JS event with the response. 
/// </summary>
/// <param name="TriggerType">HxCustomTriggerType</param>
/// <param name="EventId">The JS event name.</param>
/// <param name="JsonDetail">The JSON payload that is the evt.detail object.</param>
public record HxCustomTrigger(HxCustomTriggerType TriggerType, string EventId, string JsonDetail) : IHxTrigger {
    public HxCustomTriggerType TriggerType { get; init; } = TriggerType;
    public string EventId { get; init; } = EventId;
    public string JsonDetail { get; init; } = JsonDetail;
}

file sealed class HxTriggerBuilder(HttpResponse response, ILogger logger) : IHxTriggerBuilder {
    private readonly List<IHxTrigger> Triggers = [];
    private bool isBuilt = false;

    public IHxTriggerBuilder AddTrigger(IHxTrigger trigger) {
        logger.LogInformation("Adding htmx response trigger for request {method}:{request}.",
                response.HttpContext.Request.Method,
                response.HttpContext.Request.GetDisplayUrl());
        logger.LogInformation("Trigger {trigger} details: {details}",
            trigger.GetType(),
            trigger.ToString());
        Triggers.Add(trigger);
        return this;
    }

    public void Build() {
        if (isBuilt) {
            logger.LogCritical("HxTriggers build attempted more than once for request {method}:{request}",
                response.HttpContext.Request.Method,
                response.HttpContext.Request.GetDisplayUrl());
            throw new InvalidOperationException("HxTriggers have already been built. Build() may only be called once.");
        }
        isBuilt = true;
        // Immediate response triggers
        StringBuilder header = new();
        foreach (var t in Triggers) {
            header.Append(t switch {
                HxCustomTrigger => (t as HxCustomTrigger)!.TriggerType == HxCustomTriggerType.Normal
                    ? $"\"{(t as HxCustomTrigger)!.EventId}\": {(t as HxCustomTrigger)!.JsonDetail},"
                    : "",
                HxToastTrigger => $"\"razorx-toast-trigger\": {JsonSerializer.Serialize(t as HxToastTrigger)},",
                HxCloseModalTrigger => $"\"razorx-close-modal-trigger\": {JsonSerializer.Serialize(t as HxCloseModalTrigger)},",
                HxSetMetadataTrigger => (t as HxSetMetadataTrigger)!.Scope != HxMetadataScope.Transient
                    ? $"\"razorx-set-metadata-trigger\": {JsonSerializer.Serialize(t as HxSetMetadataTrigger)},"
                    : "",
                HxRemoveMetadataTrigger => (t as HxRemoveMetadataTrigger)!.Scope != HxMetadataScope.Transient
                    ? $"\"razorx-remove-metadata-trigger\": {JsonSerializer.Serialize(t as HxRemoveMetadataTrigger)},"
                    : "",
                _ => ""
            });
        }
        if (header.Length > 0) {
            logger.LogTrace("Added HX-Trigger response header for {method}:{request}.",
                response.HttpContext.Request.Method,
                response.HttpContext.Request.GetDisplayUrl());
            response.Headers.Append("HX-Trigger", $"{{{header.ToString().TrimEnd(',')}}}");
        }
        // After swap triggers
        header.Clear();
        foreach (var t in Triggers) {
            header.Append(t switch {
                HxCustomTrigger => (t as HxCustomTrigger)!.TriggerType == HxCustomTriggerType.AfterSwap
                    ? $"\"{(t as HxCustomTrigger)!.EventId}\": {(t as HxCustomTrigger)!.JsonDetail},"
                    : "",
                _ => ""
            });
        }
        if (header.Length > 0) {
            logger.LogTrace("Added HX-Trigger-After-Swap response header for {method}:{request}.",
                response.HttpContext.Request.Method,
                response.HttpContext.Request.GetDisplayUrl());
            response.Headers.Append("HX-Trigger-After-Swap", $"{{{header.ToString().TrimEnd(',')}}}");
        }
        // After settle triggers
        header.Clear();
        foreach (var t in Triggers) {
            header.Append(t switch {
                HxCustomTrigger => (t as HxCustomTrigger)!.TriggerType == HxCustomTriggerType.AfterSettle
                    ? $"\"{(t as HxCustomTrigger)!.EventId}\": {(t as HxCustomTrigger)!.JsonDetail},"
                    : "",
                HxFocusTrigger => $"\"razorx-focus-trigger\": {JsonSerializer.Serialize(t as HxFocusTrigger)},",
                HxSetMetadataTrigger => (t as HxSetMetadataTrigger)!.Scope == HxMetadataScope.Transient
                    ? $"\"razorx-set-metadata-trigger\": {JsonSerializer.Serialize(t as HxSetMetadataTrigger)},"
                    : "",
                HxRemoveMetadataTrigger => (t as HxRemoveMetadataTrigger)!.Scope == HxMetadataScope.Transient
                    ? $"\"razorx-remove-metadata-trigger\": {JsonSerializer.Serialize(t as HxRemoveMetadataTrigger)},"
                    : "",
                _ => ""
            });
        }
        if (header.Length > 0) {
            logger.LogTrace("Added HX-Trigger-After-Settle response header for {method}:{request}.",
                response.HttpContext.Request.Method,
                response.HttpContext.Request.GetDisplayUrl());
            response.Headers.Append("HX-Trigger-After-Settle", $"{{{header.ToString().TrimEnd(',')}}}");
        }
    }
}