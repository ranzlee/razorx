using RxTemplate.Components.Rx;
using RxTemplate.Components.Rx.Headless.AutoComplete;

namespace RxTemplate.Components.Examples.Form;

public record FormModel(
    string? Name = null,
    string? Email = null,
    DateOnly? BirthDate = null,
    DateTime? AppointmentTime = null,
    string? AppointmentTimeTimeZone = null,
    bool IsPublished = false,
    int? NumberOfDays = null,
    decimal? Cost = null,
    SubscriptionType Subscription = SubscriptionType.NoRenewal,
    ReportingStatusType ReportingStatus = ReportingStatusType.NotReported,
    string? Notes = null,
    string? Widget = null,
    string? WidgetId = null);

public record WidgetItem(string Id, string DisplayValue) : IRxAutoCompleteItem;

public static class ModelExtensions {
    public static DateTime? GetAppointmentTimeAsUtc(this FormModel formModel, ILogger? logger) {
        return Utilities.GetUtcDateFromTimeZone(formModel.AppointmentTime, formModel.AppointmentTimeTimeZone, logger);
    }
}

public enum ReportingStatusType {
    NotReported,
    InProgress,
    Submitted,
    Completed
}

public enum SubscriptionType {
    NoRenewal,
    MonthlyRenewal,
    AnnualRenewal
}