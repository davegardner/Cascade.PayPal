using System.Collections.Generic;
using System.Web.Mvc;
using Cascade.Paypal.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Cascade.Paypal.Services
{
    public class MissingSettingsBanner : INotificationProvider
    {
        private readonly IOrchardServices _orchardServices;

        public MissingSettingsBanner(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var workContext = _orchardServices.WorkContext;
            var paypalExpressPart = workContext.CurrentSite.As<PaypalExpressPart>();

            if (paypalExpressPart == null || !paypalExpressPart.IsValid())
            {
                var urlHelper = new UrlHelper(workContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("PayPal", "Admin", new { Area = "Settings" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">Paypal Express settings</a> need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}
