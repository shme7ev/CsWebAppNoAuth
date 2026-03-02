namespace WebAppNoAuth.Models;

public class LokiConfiguration
{
    public const string SectionName = "Serilog:WriteTo:2:Args"; // Points to GrafanaLoki configuration
    
    public string Uri { get; set; } = string.Empty;
    public List<LokiLabel> Labels { get; set; } = new();
    public List<string> PropertiesAsLabels { get; set; } = new();
}

public class LokiLabel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}