using System.ComponentModel;
using System.Reflection;

namespace RxTemplate.Components.Rx;

public static class Extensions {
    public static string? GetDescription(this Enum value) {
        var fi = value.GetType().GetFields(BindingFlags.Public | BindingFlags.Static).Single(x => x.GetValue(null)!.Equals(value));
        if (fi is null) {
            return null;
        }
        if (Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) is not DescriptionAttribute attr) {
            return null;
        }
        return attr.Description ?? null;
    }
}