using RxTemplate.Components.Rx;
using RxTemplate.Components.Rx.Headless.AutoComplete;

namespace RxTemplate.Components.Examples.Form;

public record FormModel {
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime? AppointmentTime { get; set; }
    public string? AppointmentTimeTimeZone { get; set; }
    public bool IsPublished { get; set; }
    public int? NumberOfDays { get; set; }
    public decimal? Cost { get; set; }
    public SubscriptionType Subscription { get; set; }
    public ReportingStatusType ReportingStatus { get; set; }
    public string? Notes { get; set; }
    public string? Widget { get; set; }
    public string? WidgetId { get; set; }

    public DateTime? GetAppointmentTimeAsUtc(ILogger? logger) {
        return Utilities.GetUtcDateFromTimeZone(AppointmentTime, AppointmentTimeTimeZone, logger);
    }
}

public record WidgetItem : IRxAutoCompleteItem {
    public string Id { get; set; } = null!;
    public string DisplayValue { get; set; } = null!;

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