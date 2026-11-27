namespace Microsoft.Extensions.DependencyInjection.Persistence;

public class LogDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}