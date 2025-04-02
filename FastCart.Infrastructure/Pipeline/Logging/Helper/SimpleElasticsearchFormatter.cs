using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;

namespace FastCart.Infrastructure.Logging;

public class SimpleElasticsearchFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var log = new Dictionary<string, object?>
        {
            ["@timestamp"] = logEvent.Timestamp.UtcDateTime,
            ["level"] = logEvent.Level.ToString(),
            ["message"] = logEvent.RenderMessage()
        };

        foreach (var prop in logEvent.Properties)
        {
            log[prop.Key] = prop.Value.ToString().Trim('"'); // özel alanları da ekle
        }

        var json = JsonSerializer.Serialize(log);
        output.WriteLine(json);
    }
}
