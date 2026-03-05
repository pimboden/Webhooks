using System;
using System.Configuration;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Webhooks.EventDispatcher;
using Webhooks.EventDispatcher.RabbitMQ;

namespace WebForms
{
    public class Global : HttpApplication
    {
        /// <summary>
        /// Single shared IWebhookDispatcher for the whole application lifetime.
        /// Initialized once in Application_Start, disposed in Application_End.
        /// Any page or handler can call: await Global.WebhookDispatcher.DispatchAsync(...)
        /// </summary>
        public static IWebhookDispatcher WebhookDispatcher { get; private set; }

        void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            WebhookDispatcher = WebhookDispatcherFactory.Create(opts =>
            {
                opts.RabbitMqConnectionString =
                    ConfigurationManager.ConnectionStrings["rabbitmq"].ConnectionString;
                // opts.ExchangeName defaults to "webhook.dispatched.exchange"
            });
        }

        void Application_End(object sender, EventArgs e)
        {
            // Cleanly close the RabbitMQ connection when IIS recycles the app pool
            (WebhookDispatcher as IDisposable)?.Dispose();
        }
    }
}
