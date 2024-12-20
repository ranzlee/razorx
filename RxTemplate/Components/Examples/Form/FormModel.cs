using RxTemplate.Components.Rx;

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

    public DateTime? GetAppointmentTimeAsUtc(ILogger? logger) {
        return Utilities.GetUtcDateFromTimeZone(AppointmentTime, AppointmentTimeTimeZone, logger);
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