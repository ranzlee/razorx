using System.Text.RegularExpressions;

namespace RxTemplate.Components.Rx;

public partial class Utilities() {

    [GeneratedRegex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])")]
    private static partial Regex _splitEnumName();

    public static string AddSpacesToCamelCase(string? val, ILogger? logger = null) {
        if (string.IsNullOrWhiteSpace(val)) {
            logger?.LogTrace("{method} string not provided.", nameof(AddSpacesToCamelCase));
            return "";
        }
        var s = _splitEnumName().Replace(val, " ");
        logger?.LogTrace("{method} converted {val} to {s}.",
            nameof(AddSpacesToCamelCase),
            val,
            s);
        return s;
    }

    public static string GenerateElementId(ILogger? logger = null) {
        // Fun fact: Ids that are valid JS variable names automatically create DOM objects, so we add the "Rx-" to avoid pollution.
        var id = $"Rx-{Guid.NewGuid():N}";
        logger?.LogTrace("{method} generated {id}.", nameof(GenerateElementId), id);
        return id;
    }

    public static DateTime? GetUtcDateFromTimeZone(DateTime? localDate, string? timeZoneId, ILogger? logger = null) {
        if (!localDate.HasValue || string.IsNullOrWhiteSpace(timeZoneId)) {
            logger?.LogTrace("{method} proper inputs not provided.", nameof(GetUtcDateFromTimeZone));
            return null;
        }
        if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz)) {
            var ud = TimeZoneInfo.ConvertTimeToUtc(localDate.Value, tz);
            logger?.LogTrace("{method} converted Date:[{localDate}] with time zone:[{timeZone}] to UTC:[{utcDate}].",
                nameof(GetUtcDateFromTimeZone),
                localDate.Value,
                timeZoneId,
                ud);
            return ud;
        }
        logger?.LogTrace("{method} provided invalid time zone:[{timeZone}].", nameof(GetUtcDateFromTimeZone), timeZoneId);
        return null;
    }

    public static DateTime? GetLocalDateFromUtc(DateTime? utcDate, string? timeZoneId, ILogger? logger = null) {
        if (!utcDate.HasValue || string.IsNullOrWhiteSpace(timeZoneId)) {
            logger?.LogTrace("{method} proper inputs not provided.", nameof(GetLocalDateFromUtc));
            return null;
        }
        if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz)) {
            var ld = TimeZoneInfo.ConvertTimeFromUtc(utcDate.Value, tz);
            logger?.LogTrace("{method} converted UTC:[{utcDate}] to Date:[{localDate}] with time zone:[{timeZone}].",
                nameof(GetLocalDateFromUtc),
                utcDate.Value,
                ld,
                timeZoneId);
            return ld;
        }
        logger?.LogTrace("{method} provided invalid time zone:[{timeZone}].", nameof(GetLocalDateFromUtc), timeZoneId);
        return null;
    }

    public static T? Converter<T>(string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return default;
        }
        if (typeof(T).IsEnum) {
            return Enum.TryParse(typeof(T), value, out var result)
                ? (T)result
                : throw new InvalidCastException();
        }
        return (T)Convert.ChangeType(value, Type.GetTypeCode(typeof(T)));
    }
}