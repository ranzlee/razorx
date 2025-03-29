using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RxTemplate.Rx;

/// <summary>
/// Adds the JSON converters necessary for converting the request JSON payload created from FORM data.
/// ASP.NET Minimal APIs have much better support for JSON binding compared to FORM data. On the client, the 
/// json-enc.js HTMX extension is used to convert request FORM data into JSON. Form data values are always
/// strings. The custom converters coerce the string values into the correct data types for model binding.
/// </summary>
/// <param name="httpContextAccessor">IHttpContextAccessor</param>
/// <param name="logger">ILogger</param>
public class HxJsonOptions(IHttpContextAccessor httpContextAccessor, ILogger<HxJsonOptions> logger) : IConfigureOptions<JsonOptions> {

    public void Configure(JsonOptions options) {
        // form values
        options.SerializerOptions.Converters.Add(new CharJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new DateTimeJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new BooleanJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new IntJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new LongJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new ShortJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new DecimalJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new DoubleJsonConverter(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new FloatJsonConverter(httpContextAccessor, logger));
        // form value collections (arrays)
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<string>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<char>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<DateOnly>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<DateTime>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<bool>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<int>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<long>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<short>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<decimal>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<double>(httpContextAccessor, logger));
        options.SerializerOptions.Converters.Add(new SingleOrArrayConverter<float>(httpContextAccessor, logger));
        // enum converter
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}

file sealed class SingleOrArrayConverter<T>(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<IEnumerable<T>> {
    public override IEnumerable<T>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) {
                logger.LogTrace("{converter}.{method} returned null.",
                    nameof(SingleOrArrayConverter<T>),
                    nameof(Read));
                return null;
            }
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for \"{val}\".",
                nameof(SingleOrArrayConverter<T>),
                nameof(Read),
                typeof(T),
                s);
            return JsonSerializer.Deserialize<IEnumerable<T>>(s);
        }
        List<T> list = null!;
        switch (reader.TokenType) {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.StartArray:
                list = [];
                while (reader.Read()) {
                    if (reader.TokenType == JsonTokenType.EndArray) {
                        break;
                    }
                    list.Add(JsonSerializer.Deserialize<T>(ref reader, options)!);
                }
                return list;
            default:
                list = [JsonSerializer.Deserialize<T>(ref reader, options)!];
                return list;
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        IEnumerable<T>? objectToWrite,
        JsonSerializerOptions options) {
        if (objectToWrite is null) {
            logger.LogTrace("{converter}.{method} writing null.",
                nameof(SingleOrArrayConverter<T>),
                nameof(Write));
            writer.WriteNullValue();
            return;
        }
        logger.LogTrace("{converter}.{method} writing {type}.",
            nameof(SingleOrArrayConverter<T>),
            nameof(Write),
            objectToWrite.GetType());
        JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
    }

}

file sealed class DateOnlyJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<DateOnly?> {
    public override DateOnly? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(DateOnlyJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for \"{val}\".",
                nameof(DateOnlyJsonConverter),
                nameof(Read),
                typeof(DateOnly),
                s);
            return JsonSerializer.Deserialize<DateOnly>(s);
        }
        var isValid = DateOnly.TryParse(s, out var dt);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(DateOnlyJsonConverter),
            nameof(Read),
            s,
            isValid ? dt : "null - TryParse() failed");
        return isValid ? dt : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateOnly? dateOnlyValue,
        JsonSerializerOptions options) {
        if (dateOnlyValue.HasValue) {
            var val = dateOnlyValue.Value.ToDateTime(TimeOnly.MinValue);
            logger.LogTrace("{converter}.{method} writing \"{result}\".",
                nameof(DateOnlyJsonConverter),
                nameof(Write),
                val);
            writer.WriteStringValue(val);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(DateOnlyJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class DateTimeJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<DateTime?> {
    public override DateTime? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(DateTimeJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for \"{val}\".",
                nameof(DateTimeJsonConverter),
                nameof(Read),
                typeof(DateTime),
                s);
            return JsonSerializer.Deserialize<DateTime>(s);
        }
        var isValid = DateTime.TryParse(s, out var dt);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(DateTimeJsonConverter),
            nameof(Read),
            s,
            isValid ? dt : "null - TryParse() failed");
        return isValid ? dt : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime? dateTimeValue,
        JsonSerializerOptions options) {
        if (dateTimeValue.HasValue) {
            var val = dateTimeValue.Value;
            logger.LogTrace("{converter}.{method} writing \"{result}\".",
                nameof(DateTimeJsonConverter),
                nameof(Write),
                val);
            writer.WriteStringValue(val);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(DateTimeJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class BooleanJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<bool> {
    public override bool Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(BooleanJsonConverter),
                nameof(Read));
            return false;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(BooleanJsonConverter),
                nameof(Read),
                typeof(bool),
                s);
            return JsonSerializer.Deserialize<bool>(s);
        }
        var isValid = bool.TryParse(s, out var b);
        if (!isValid && s == "on") {
            isValid = true;
            b = true;
        }
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(BooleanJsonConverter),
            nameof(Read),
            s,
            isValid ? b : "null - TryParse() failed");
        return isValid && b;
    }

    public override void Write(
        Utf8JsonWriter writer,
        bool boolValue,
        JsonSerializerOptions options) {
        logger.LogTrace("{converter}.{method} writing {result}.",
            nameof(BooleanJsonConverter),
            nameof(Write),
            boolValue);
        writer.WriteBooleanValue(boolValue);
    }
}

file sealed class IntJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<int?> {
    public override int? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(IntJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(IntJsonConverter),
                nameof(Read),
                typeof(int),
                s);
            return JsonSerializer.Deserialize<int>(s);
        }
        var isValid = int.TryParse(s, out var i);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(IntJsonConverter),
            nameof(Read),
            s,
            isValid ? i : "null - TryParse() failed");
        return isValid ? i : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        int? intValue,
        JsonSerializerOptions options) {
        if (intValue.HasValue) {
            var val = intValue.Value;
            logger.LogTrace("{converter}.{method} writing {result}.",
                nameof(IntJsonConverter),
                nameof(Write),
                val);
            writer.WriteNumberValue(val);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(IntJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class LongJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<long?> {
    public override long? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(LongJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(LongJsonConverter),
                nameof(Read),
                typeof(long),
                s);
            return JsonSerializer.Deserialize<long>(s);
        }
        var isValid = long.TryParse(s, out var l);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(LongJsonConverter),
            nameof(Read),
            s,
            isValid ? l : "null - TryParse() failed");
        return isValid ? l : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        long? longValue,
        JsonSerializerOptions options) {
        if (longValue.HasValue) {
            var val = longValue.Value;
            logger.LogTrace("{converter}.{method} writing {result}.",
                nameof(LongJsonConverter),
                nameof(Write),
                val);
            writer.WriteNumberValue(val);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(LongJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class ShortJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<short?> {
    public override short? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(ShortJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(ShortJsonConverter),
                nameof(Read),
                typeof(short),
                s);
            return JsonSerializer.Deserialize<short>(s);
        }
        var isValid = short.TryParse(s, out var i);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(ShortJsonConverter),
            nameof(Read),
            s,
            isValid ? i : "null - TryParse() failed");
        return isValid ? i : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        short? shortValue,
        JsonSerializerOptions options) {
        if (shortValue.HasValue) {
            var val = shortValue.Value;
            logger.LogTrace("{converter}.{method} writing {result}.",
                nameof(ShortJsonConverter),
                nameof(Write),
                val);
            writer.WriteNumberValue(val);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(ShortJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class DecimalJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<decimal?> {
    public override decimal? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(DecimalJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(DecimalJsonConverter),
                nameof(Read),
                typeof(decimal),
                s);
            return JsonSerializer.Deserialize<decimal>(s);
        }
        var isValid = decimal.TryParse(s, out var d);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(DecimalJsonConverter),
            nameof(Read),
            s,
            isValid ? d : "null - TryParse() failed");
        return isValid ? d : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        decimal? decimalValue,
        JsonSerializerOptions options) {
        if (decimalValue.HasValue) {
            var val = decimalValue.Value;
            logger.LogTrace("{converter}.{method} writing {result}.",
                nameof(DecimalJsonConverter),
                nameof(Write),
                val);
            writer.WriteNumberValue(decimalValue.Value);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(DecimalJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class DoubleJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<double?> {
    public override double? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(DoubleJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(DoubleJsonConverter),
                nameof(Read),
                typeof(double),
                s);
            return JsonSerializer.Deserialize<double>(s);
        }
        var isValid = double.TryParse(s, out var d);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(DoubleJsonConverter),
            nameof(Read),
            s,
            isValid ? d : "null - TryParse() failed");
        return isValid ? d : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        double? doubleValue,
        JsonSerializerOptions options) {
        if (doubleValue.HasValue) {
            var val = doubleValue.Value;
            logger.LogTrace("{converter}.{method} writing {result}.",
                nameof(DoubleJsonConverter),
                nameof(Write),
                val);
            writer.WriteNumberValue(doubleValue.Value);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(DoubleJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class FloatJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<float?> {
    public override float? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(FloatJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(FloatJsonConverter),
                nameof(Read),
                typeof(float),
                s);
            return JsonSerializer.Deserialize<float>(s);
        }
        var isValid = float.TryParse(s, out var f);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(FloatJsonConverter),
            nameof(Read),
            s,
            isValid ? f : "null - TryParse() failed");
        return isValid ? f : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        float? floatValue,
        JsonSerializerOptions options) {
        if (floatValue.HasValue) {
            var val = floatValue.Value;
            logger.LogTrace("{converter}.{method} writing {result}.",
                nameof(FloatJsonConverter),
                nameof(Write),
                val);
            writer.WriteNumberValue(floatValue.Value);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(FloatJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}

file sealed class CharJsonConverter(IHttpContextAccessor httpContextAccessor, ILogger logger) : JsonConverter<char?> {
    public override char? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        var s = reader.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) {
            logger.LogTrace("{converter}.{method} returned null.",
                nameof(CharJsonConverter),
                nameof(Read));
            return null;
        }
        if (httpContextAccessor.HttpContext is null || !httpContextAccessor.HttpContext.Request.IsHxRequest()) {
            logger.LogTrace("No HttpContext or is not HX-Request - {converter}.{method} called default JsonSerializer.Deserialize<{type}>() for {val}.",
                nameof(CharJsonConverter),
                nameof(Read),
                typeof(char),
                s);
            return JsonSerializer.Deserialize<char>(s);
        }
        var isValid = char.TryParse(s, out var c);
        logger.LogTrace("{converter}.{method} overriding Deserialize for \"{val}\" results: {result}.",
            nameof(CharJsonConverter),
            nameof(Read),
            s,
            isValid ? c : "null - TryParse() failed");
        return isValid ? c : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        char? charValue,
        JsonSerializerOptions options) {
        if (charValue.HasValue) {
            var val = charValue.Value;
            logger.LogTrace("{converter}.{method} writing {result}.",
                nameof(CharJsonConverter),
                nameof(Write),
                val);
            writer.WriteNumberValue(charValue.Value);
            return;
        }
        logger.LogTrace("{converter}.{method} writing null.",
            nameof(CharJsonConverter),
            nameof(Write));
        writer.WriteNullValue();
    }
}