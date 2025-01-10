using System.ComponentModel;
using System.Reflection;

namespace RxTemplate.Components.Rx;

public static class Extensions {
    public static string GetDescription(this Enum value) {
        var fi = value
                .GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Single(x => x.GetValue(null)!.Equals(value));
        if (fi is null) {
            return Utilities.SplitCamelCaseWords(value.ToString());
        }
        if (Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) is not DescriptionAttribute attr) {
            return Utilities.SplitCamelCaseWords(value.ToString());
        }
        return string.IsNullOrWhiteSpace(attr.Description)
            ? Utilities.SplitCamelCaseWords(value.ToString())
            : attr.Description;
    }

    public static T? AsEnum<T>(this string value) where T : Enum {
        return Utilities.Converter<T>(value);
    }
}