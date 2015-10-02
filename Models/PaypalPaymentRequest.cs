using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cascade.Paypal.Models
{
    public class PaypalPaymentRequest
    {
        public string PaymentAction { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }

    }
}