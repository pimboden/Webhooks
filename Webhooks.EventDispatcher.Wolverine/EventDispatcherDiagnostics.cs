using System.Diagnostics;

namespace Webhooks.EventDispatcher.Wolverine;

public static class EventDispatcherDiagnostics
{
    /// <summary>
    /// The OpenTelemetry ActivitySource name. Add this to your tracing configuration:
    /// <code>
    /// builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(EventDispatcherDiagnostics.ActivitySourceName));
    /// </code>
    /// </summary>
    public const string ActivitySourceName = "Webhooks.EventDispatcher";

    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
