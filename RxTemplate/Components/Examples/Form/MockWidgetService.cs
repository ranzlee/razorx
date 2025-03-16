namespace RxTemplate.Components.Examples.Form;

public static class MockWidgetService {
    private static readonly IEnumerable<WidgetItem> Data;

    static MockWidgetService() {
        var data = new List<WidgetItem>();
        // Generate some random data
        for (var i = 0; i < 1024; i++) {
            var d = new WidgetItem {
                Id = (i + 1).ToString(),
                DisplayName = Random.Shared.Next(1, 100) switch {
                    < 26 => $"CNT-{Random.Shared.Next(1, 999999999).ToString().PadLeft(9, '0')}",
                    < 51 => $"INV-{Random.Shared.Next(1, 999999999).ToString().PadLeft(9, '0')}",
                    < 76 => $"RFQ-{Random.Shared.Next(1, 999999999).ToString().PadLeft(9, '0')}",
                    _ => $"PO-{Random.Shared.Next(1, 999999999).ToString().PadLeft(9, '0')}"
                }

            };
            data.Add(d);
        }
        Data = data;
    }

    public static WidgetItem? Get(string id) {
        return Data.FirstOrDefault(x => x.Id == id);
    }

    public static IEnumerable<WidgetItem> Find(string name) {
        return [.. Data.Where(x => x.DisplayName.Contains(name, StringComparison.CurrentCultureIgnoreCase)).OrderBy(x => x.DisplayName)];
    }
}