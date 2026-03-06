using System;
using System.Threading;
using System.Web.UI;

namespace WebForms
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected async void BtnFireEvent_Click(object sender, EventArgs e)
        {
            await Global.WebhookDispatcher.DispatchAsync(
                eventType: "webform.fired",
                data: new { Source = "WebForms", Page = Request.Url.AbsolutePath, Timestamp = DateTime.UtcNow },
                cancellationToken: CancellationToken.None);
        }
    }
}
