using Cascade.Paypal.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Cascade.Paypal.Drivers
{
    // We define a specific driver instead of using a TemplateFilterForRecord, because we need the model to be the part and not the record.
    // Thus the encryption/decryption will be done when accessing the part's property

    public class PaypalExpressSettingsDriver : ContentPartDriver<PaypalExpressPart>
    {
        private const string TemplateName = "Parts/PaypalExpressSettings";

        public PaypalExpressSettingsDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "PaypalExpressSettings"; } }

        protected override DriverResult Editor(PaypalExpressPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaypalExpressSettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                    .OnGroup("Paypal");
        }

        protected override DriverResult Editor(PaypalExpressPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaypalExpressSettings_Edit", () =>
            {
                var previousPassword = part.Pwd;
                updater.TryUpdateModel(part, Prefix, null, null);

                // restore password if the input is empty, meaning it has not been reset
                if (string.IsNullOrEmpty(part.Pwd))
                {
                    part.Pwd = previousPassword;
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("Paypal");
        }
    }
}