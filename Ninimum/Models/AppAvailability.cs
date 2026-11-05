namespace Ninimum.Models;

public sealed class ApplicationConfiguration
{
    public string? appVersion { get; set; }
    public string? forceUpdateYn { get; set; }
    public string? maintenanceYn { get; set; }
    public string? maintenanceMessage { get; set; }
}

public enum Availability { Available, Maintenance, UpdateRequired, UpdateOptional }

public static class AppAvailability
{
    public static Availability Evaluate(ApplicationConfiguration? config, string installed)
    {
        if (config == null) return Availability.Available;
        if (string.Equals(config.maintenanceYn, "Y", StringComparison.OrdinalIgnoreCase)) return Availability.Maintenance;
        // Web admin: Y = required update, O = optional update (with "Later"), N = no message.
        if (Parse(config.appVersion) is { } latest && Parse(installed) is { } current && current < latest)
        {
            if (string.Equals(config.forceUpdateYn,"Y",StringComparison.OrdinalIgnoreCase)) return Availability.UpdateRequired;
            if (string.Equals(config.forceUpdateYn,"O",StringComparison.OrdinalIgnoreCase)) return Availability.UpdateOptional;
        }
        return Availability.Available;
    }
    private static Version? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var parts=value.Trim().Split('.');
        if (parts.Length is < 1 or > 4 || parts.Any(p=>p.Length==0||p.Any(c=>c<'0'||c>'9'))) return null;
        return Version.TryParse(string.Join('.',parts.Concat(Enumerable.Repeat("0",4-parts.Length))),out var version)?version:null;
    }
}
