using System;
using System.Text;
using Cascade.Paypal.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;

namespace Cascade.Paypal.Handlers
{
    public class PaypalSettingsHandler : ContentHandler
    {
        private readonly IEncryptionService _encryptionService;

        public PaypalSettingsHandler(IRepository<PaypalExpressRecord> repository, IEncryptionService encryptionService)
        {
            T = NullLocalizer.Instance;

            _encryptionService = encryptionService;

            Filters.Add(new ActivatingFilter<PaypalExpressPart>("Site"));
            Filters.Add(StorageFilter.For(repository));

            // set up en/de-cryption
            OnLoaded<PaypalExpressPart>(LazyLoadHandlers);
        }

        public Localizer T { get; set; }
        public new ILogger Logger { get; set; }

        void LazyLoadHandlers(LoadContentContext context, PaypalExpressPart part)
        {
            part.PwdField.Getter(() =>
            {
                try
                {
                    return String.IsNullOrWhiteSpace(part.Record.PaypalPwd) ? String.Empty : Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(part.Record.PaypalPwd)));
                }
                catch
                {
                    Logger.Error("The PayPal password could not be decrypted. It might be corrupted, try to reset it.");
                    return null;
                }
            });

            part.PwdField.Setter(value => part.Record.PaypalPwd = String.IsNullOrWhiteSpace(value) ? String.Empty : Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(value))));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Paypal")));
        }

    }
}