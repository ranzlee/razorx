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

file sealed class HxTriggers(ILogger<HxTriggers> logger) : IHxTriggers {
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
    IHxTriggerBuilder Add(IHxTrigger trigger);

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
public record HxToastTrigger(string ToastSelector, string Message) : IHxTrigger;

/// <summary>
/// Triggers the close action of an open modal with the response.
/// </summary>
/// <param name="ModalSelector">The element CSS selector of the modal dialog.</param>
public record HxCloseModalTrigger(string ModalSelector) : IHxTrigger;

/// <summary>
/// Triggers the focus of an HTML element with the response.
/// </summary>
/// <param name="ElementSelector">The element CSS selector to focus.</param>
public record HxFocusTrigger(string ElementSelector, bool ScrollIntoView = false) : IHxTrigger;

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
public record HxSetMetadataTrigger(HxMetadataScope Scope, string Key, string Value) : IHxTrigger;

/// <summary>
/// Triggers the removal of a local or session storage item.
/// </summary>
/// <param name="Scope">The type of storage to target.</param>
/// <param name="Key">The storage key, or hidden field ID is Transient.</param>
public record HxRemoveMetadataTrigger(HxMetadataScope Scope, string Key) : IHxTrigger;

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
public record HxCustomTrigger(HxCustomTriggerType TriggerType, string EventId, string JsonDetail) : IHxTrigger;

file sealed class HxTriggerBuilder(HttpResponse response, ILogger logger) : IHxTriggerBuilder {
    private readonly List<IHxTrigger> Triggers = [];
    private const string HX_TRIGGER_BUILDER_KEY = "HX-Trigger-Builder";

    public IHxTriggerBuilder Add(IHxTrigger trigger) {
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
        var isBuilt = response.Headers.ContainsKey(HX_TRIGGER_BUILDER_KEY);
        if (isBuilt) {
            logger.LogError("HxTriggers build attempted more than once for request {method}:{request}.",
                response.HttpContext.Request.Method,
                response.HttpContext.Request.GetDisplayUrl());
            throw new InvalidOperationException("HxTriggers have already been built. Build() may only be called once.");
        }
        response.Headers.Append(HX_TRIGGER_BUILDER_KEY, true.ToString());
        BuildHxTriggerHeader();
        BuildHxTriggerAfterSwapHeader();
        BuildHxTriggerAfterSettleHeader();
    }

    private void BuildHxTriggerHeader() {
        // Immediate response triggers
        StringBuilder header = new();
        foreach (var t in Triggers) {
            header.Append(t switch {
                HxCustomTrigger customTrigger => customTrigger.TriggerType == HxCustomTriggerType.Normal
                    ? $"\"{customTrigger.EventId}\": {customTrigger.JsonDetail},"
                    : "",
                HxToastTrigger toastTrigger => $"\"razorx-toast-trigger\": {JsonSerializer.Serialize(toastTrigger)},",
                HxCloseModalTrigger closeModalTrigger => $"\"razorx-close-modal-trigger\": {JsonSerializer.Serialize(closeModalTrigger)},",
                HxSetMetadataTrigger setMetadataTrigger => setMetadataTrigger.Scope != HxMetadataScope.Transient
                    ? $"\"razorx-set-metadata-trigger\": {JsonSerializer.Serialize(setMetadataTrigger)},"
                    : "",
                HxRemoveMetadataTrigger removeMetadataTrigger => removeMetadataTrigger.Scope != HxMetadataScope.Transient
                    ? $"\"razorx-remove-metadata-trigger\": {JsonSerializer.Serialize(removeMetadataTrigger)},"
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
    }

    private void BuildHxTriggerAfterSwapHeader() {
        StringBuilder header = new();
        foreach (var t in Triggers) {
            header.Append(t switch {
                HxCustomTrigger customTrigger => customTrigger.TriggerType == HxCustomTriggerType.AfterSwap
                    ? $"\"{customTrigger.EventId}\": {customTrigger.JsonDetail},"
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
    }

    private void BuildHxTriggerAfterSettleHeader() {
        StringBuilder header = new();
        foreach (var t in Triggers) {
            header.Append(t switch {
                HxCustomTrigger customTrigger => customTrigger.TriggerType == HxCustomTriggerType.AfterSettle
                    ? $"\"{customTrigger.EventId}\": {customTrigger.JsonDetail},"
                    : "",
                HxFocusTrigger focusTrigger => $"\"razorx-focus-trigger\": {JsonSerializer.Serialize(focusTrigger)},",
                HxSetMetadataTrigger setMetadataTrigger => setMetadataTrigger.Scope == HxMetadataScope.Transient
                    ? $"\"razorx-set-metadata-trigger\": {JsonSerializer.Serialize(setMetadataTrigger)},"
                    : "",
                HxRemoveMetadataTrigger removeMetadataTrigger => removeMetadataTrigger.Scope == HxMetadataScope.Transient
                    ? $"\"razorx-remove-metadata-trigger\": {JsonSerializer.Serialize(removeMetadataTrigger)},"
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