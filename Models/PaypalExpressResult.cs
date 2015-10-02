using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cascade.Paypal.Models
{
    public class PaypalExpressResult 
    {
        public PaypalExpressResult()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Paypal method: SetExpressCheckout, GetExpressCheckout, DoExpressCheckout
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The returned Paypal token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The returned response for the method: Success, Failure, Cancel
        /// </summary>
        public string Ack { get; set; }

        /// <summary>
        /// Paypal payer ID
        /// </summary>
        public string PayerId { get; set; }

        /// <summary>
        /// For DoExpressCheckout only, the first request TransactionId (should only be one)
        /// </summary>
        public string RequestTransactionId { get; set; }

        /// <summary>
        /// For DoExpressCheckout only, the first SecureMerchantAccountId (should only be one)
        /// </summary>
        public string RequestSecureMerchantAccountId { get; set; }

        /// <summary>
        /// For DoExpressCheckout only, the Request status (should only be one : Success, Failure, Cancel)
        /// </summary>
        public string RequestAck { get; set; }

        //////////////////// Error info ///////////////////////////
        public DateTime Timestamp { get; set; }
        public string ErrorCodes { get; set; }
        public string ShortMessages { get; set; }
        public string LongMessages { get; set; }
        public string CorrelationId { get; set; }

    }
}