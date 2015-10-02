using System;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;

namespace Cascade.Paypal.Models
{
    public class PaypalExpressRecord : ContentPartRecord
    {
        public virtual bool Sandbox { get; set; }
        public virtual string SandboxApiBaseUrl { get; set; }
        public virtual string LiveApiBaseUrl { get; set; }
        public virtual string SandboxAuthorizationBaseUrl { get; set; }
        public virtual string LiveAuthorizationBaseUrl { get; set; }
        public virtual string PaypalUser { get; set; }
        public virtual string PaypalPwd { get;  set; }
        public virtual string PaypalSignature { get; set; }
        public virtual string Version { get; set; }
        //public virtual string CancelUrl { get; set; }
        //public virtual string SuccessUrl { get; set; }
        public virtual string Currency { get; set; }
    }

    public class PaypalExpressPart : ContentPart<PaypalExpressRecord>
    {
        // Password is encrypted
        private readonly ComputedField<string> _password = new ComputedField<string>();
        public ComputedField<string> PwdField { get { return _password; } }
        [Required]
        public string Pwd { get { return _password.Value; } set { _password.Value = value; } }

        [Required]
        public bool Sandbox { get { return Record.Sandbox; } set { Record.Sandbox = value; } }
        [Required]
        public string SandboxApiBaseUrl { get { return Record.SandboxApiBaseUrl; } set { Record.SandboxApiBaseUrl = value; } }
        [Required]
        public string LiveApiBaseUrl { get { return Record.LiveApiBaseUrl; } set { Record.LiveApiBaseUrl = value; } }
        [Required]
        public string SandboxAuthorizationBaseUrl { get { return Record.SandboxAuthorizationBaseUrl; } set { Record.SandboxAuthorizationBaseUrl = value; } }
        [Required]
        public string LiveAuthorizationBaseUrl { get { return Record.LiveAuthorizationBaseUrl; } set { Record.LiveAuthorizationBaseUrl = value; } }
        [Required]
        public string ApiUrl { get { return Sandbox ? SandboxApiBaseUrl : LiveApiBaseUrl; } }
        [Required]
        public string AuthorizationUrl { get { return Sandbox ? SandboxAuthorizationBaseUrl : LiveAuthorizationBaseUrl; } }
        [Required]
        public string User { get { return Record.PaypalUser; } set { Record.PaypalUser = value; } }
        [Required]
        public string Signature { get { return Record.PaypalSignature; } set { Record.PaypalSignature = value; } }
        [Required]
        public string Version { get { return Record.Version; } set { Record.Version = value; } }

        // The following 2 properties are not persisted 
        public string CancelUrl { get; set; }
        public string SuccessUrl { get; set; }

        [Required]
        public string Currency { get { return Record.Currency; } set { Record.Currency = value; } }

        public bool IsValid()
        {
            return !(Record == null)
                && !String.IsNullOrWhiteSpace(Record.PaypalUser)
                && !String.IsNullOrWhiteSpace(Record.PaypalPwd)
                && !String.IsNullOrWhiteSpace(Record.PaypalSignature)
                && !String.IsNullOrWhiteSpace(Record.Currency)
                && !String.IsNullOrWhiteSpace(Record.Version);
        }
    }
}