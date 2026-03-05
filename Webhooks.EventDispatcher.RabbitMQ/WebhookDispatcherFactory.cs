namespace Webhooks.EventDispatcher.RabbitMQ;

/// <summary>
/// Creates <see cref="IWebhookDispatcher"/> instances without a DI container.
/// Use this in .NET Framework apps (WebForms, WCF, Console) that cannot use
/// <see cref="ServiceCollectionExtensions.AddEventDispatcher"/>.
///
/// The returned dispatcher holds a long-lived RabbitMQ connection.
/// Store it as a static/singleton field and reuse it for the lifetime of the app.
/// </summary>
public static class WebhookDispatcherFactory
{
    /// <example>
    /// private static readonly IWebhookDispatcher _dispatcher = WebhookDispatcherFactory.Create(opts =>
    /// {
    ///     opts.RabbitMqConnectionString = ConfigurationManager.ConnectionStrings["rabbitmq"].ConnectionString;
    /// });
    /// </example>
    public static IWebhookDispatcher Create(Action<EventDispatcherOptions> configure)
    {
        var options = new EventDispatcherOptions();
        configure(options);
        return new WebhookDispatcher(options);
    }
}
