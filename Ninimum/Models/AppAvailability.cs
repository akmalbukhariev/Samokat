namespace Ninimum.Models;

public sealed class ApplicationConfiguration
{
    public string? appVersion { get; set; }
    public string? forceUpdateYn { get; set; }
    public string? maintenanceYn { get; set; }
    public string? maintenanceMessage { get; set; }
}

public enum Availability { Available, Maintenance, UpdateRequired }

public static class AppAvailability
{
    public static Availability Evaluate(ApplicationConfiguration? config, string installed)
    {
        if (config == null) return Availability.Available;
        if (string.Equals(config.maintenanceYn, "Y", StringComparison.OrdinalIgnoreCase)) return Availability.Maintenance;
        if (string.Equals(config.forceUpdateYn,"Y",StringComparison.OrdinalIgnoreCase) &&
            Parse(config.appVersion) is { } minimum && Parse(installed) is { } current && current < minimum)
            return Availability.UpdateRequired;
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
