using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Cascade.Paypal.Models;
using Cascade.WebShop.Models;
using Cascade.WebShop.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Users.Models;
using Cascade.WebShop.ViewModels;

namespace Cascade.Paypal.Services
{
    /// <summary>
    /// Class to wrap the Paypal Express API
    /// </summary>
    public class PaypalExpressServices : IPaypalExpressServices
    {
        private readonly IOrderService _orderService;
        private readonly IOrchardServices _services;
        private readonly ICustomerService _customerService;

        public PaypalExpressServices(IOrderService orderService, IRepository<ProductRecord> repository, IOrchardServices services, ICustomerService customerService)
        {
            _orderService = orderService;
            _services = services;
            _customerService = customerService;
        }

        /// <summary>
        /// Call the Paypal SetExpressCheckout API (https://www.x.com/developers/paypal/documentation-tools/api/setexpresscheckout-api-operation-nvp)
        /// </summary>
        /// <param name="paypalExpressPart">part</param>
        /// <param name="order">WebShop order</param>
        /// <returns>result containing token and ack</returns>
        public PaypalExpressResult SetExpressCheckout(PaypalExpressPart paypalExpressPart, OrderPart order)
        {
            PaypalExpressResult result = new PaypalExpressResult { Method = "SetExpressCheckout" };
            HttpWebRequest request = BuildSetRequest(paypalExpressPart, order);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                string[] lines = GetResponseLines(response);
                result.Ack = GetValue(lines, "Ack").FirstOrDefault();
                result.Token = GetValue(lines, "Token").FirstOrDefault();
                result.CorrelationId = GetValue(lines, "CORRELATIONID").FirstOrDefault();
                if (string.Compare(result.Ack, "success", true) != 0)
                    ExtractErrors(lines, result);
            }

            return result;
        }

        /// <summary>
        /// Call the Paypal GetExpressCheckoutDetails API (https://www.x.com/developers/paypal/documentation-tools/api/getexpresscheckoutdetails-api-operation-nvp)
        /// </summary>
        /// <param name="paypalExpressPart">part</param>
        /// <param name="token">Paypal token</param>
        /// <returns>result containing token, ack and payerId</returns>
        public PaypalExpressResult GetExpressCheckoutDetails(PaypalExpressPart paypalExpressPart, string token)
        {
            PaypalExpressResult result = new PaypalExpressResult { Method = "GetExpressCheckoutDetails" };
            HttpWebRequest request = BuildGetDetailsRequest(paypalExpressPart, token);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                string[] lines = GetResponseLines(response);
                result.Ack = GetValue(lines, "Ack").FirstOrDefault();
                result.Token = GetValue(lines, "Token").FirstOrDefault();
                result.PayerId = GetValue(lines, "PayerId").FirstOrDefault();
                result.CorrelationId = GetValue(lines, "CORRELATIONID").FirstOrDefault();

                // TODO: parse out useful information about the payer, address, etc
                // and store somewhere: update the order? add to PaypalTransaction? customer?

                if (string.Compare(result.Ack, "success", true) != 0)
                    ExtractErrors(lines, result);
            }

            return result;
        }

        /// <summary>
        /// Call the Paypal DoExpressCheckoutPayment API (https://www.x.com/developers/paypal/documentation-tools/api/doexpresscheckoutpayment-api-operation-nvp)
        /// </summary>
        /// <param name="paypalExpressPart">part</param>
        /// <param name="order">Webshop order</param>
        /// <param name="token">Paypal token</param>
        /// <param name="payerId">Paypal PayerId</param>
        /// <returns>result containing token and ack</returns>
        public PaypalExpressResult DoExpressCheckoutPayment(PaypalExpressPart paypalExpressPart, OrderPart order, string token, string payerId)
        {
            PaypalExpressResult result = new PaypalExpressResult { Method = "DoExpressCheckoutPayment" };
            HttpWebRequest request = BuildExpressCheckoutPaymentRequest(paypalExpressPart, order, token, payerId);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                string[] lines = GetResponseLines(response);
                result.Ack = GetValue(lines, "Ack").FirstOrDefault();
                result.Token = GetValue(lines, "Token").FirstOrDefault();
                result.CorrelationId = GetValue(lines, "CORRELATIONID").FirstOrDefault();

                // TODO: parse out useful information about the payer, address, etc
                // and store somewhere: update the order? add to PaypalTransaction? customer?

                if (string.Compare(result.Ack, "success", true) != 0)
                    ExtractErrors(lines, result);
            }

            return result;

        }

        private HttpWebRequest BuildExpressCheckoutPaymentRequest(PaypalExpressPart paypalExpressPart, OrderPart order, string token, string payerId)
        {
            // Create the web request  
            HttpWebRequest request = WebRequest.Create(paypalExpressPart.ApiUrl) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // Create a paypal request
            PaypalRequest pr = new PaypalRequest(paypalExpressPart, "DoExpressCheckoutPayment", token);

            // Add data
            pr.Add("PAYMENTREQUEST_0_PAYMENTACTION", "Sale");
            pr.Add("PAYERID", payerId);
            pr.Add("PAYMENTREQUEST_0_AMT", order.SubTotal.ToString("F2")); // before shipping and tax
            pr.Add("PAYMENTREQUEST_0_ITEMAMT", order.SubTotal.ToString("F2")); // including shipping and tax
            pr.Add("PAYMENTREQUEST_0_CURRENCYCODE", paypalExpressPart.Currency);

            // order details
            AddAllItems(pr, order);

            // format the request with data from the paypal request
            pr.SetData(request);

            return request;
        }

        private HttpWebRequest BuildSetRequest(PaypalExpressPart paypalExpressPart, OrderPart order)
        {
            // Create the web request  
            HttpWebRequest request = WebRequest.Create(paypalExpressPart.ApiUrl) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // Ensure order totals are up to date
            order.UpdateTotals();

            var pr = new PaypalRequest(paypalExpressPart, "SetExpressCheckout");
            pr.Add("PAYMENTREQUEST_0_PAYMENTACTION", "Sale");
            pr.Add("PAYMENTREQUEST_0_AMT", order.SubTotal.ToString("F2")); // before shipping and tax
            pr.Add("PAYMENTREQUEST_0_ITEMAMT", order.SubTotal.ToString("F2")); // including shipping and tax
            pr.Add("PAYMENTREQUEST_0_CURRENCYCODE", paypalExpressPart.Currency);
            pr.Add("returnUrl", paypalExpressPart.SuccessUrl);
            pr.Add("cancelUrl", paypalExpressPart.CancelUrl);

            // order details
            AddAllItems(pr, order);

            // Set Address
            SetShippingAddress(pr, order);

            // format the data
            pr.SetData(request);

            return request;
        }

        private void SetShippingAddress(PaypalRequest pr, OrderPart order)
        {
            bool addrOverride = true;
            CustomerPart customer = _customerService.GetCustomer(order.CustomerId);

            // determine which address to use
            var address = _customerService.GetShippingAddress(order.CustomerId, order.Id);
            if (address == null || String.IsNullOrWhiteSpace(address.Address))
                address = _customerService.GetInvoiceAddress(order.CustomerId);
            if (address == null || String.IsNullOrWhiteSpace(address.Address))
                addrOverride = false;

            pr.Add("ADDROVERRIDE", addrOverride ? "1" : "0");
            pr.Add("NOSHIPPING", "0");
            if (addrOverride)
            {
                if (string.IsNullOrWhiteSpace(address.Name))
                    pr.Add("PAYMENTREQUEST_0_SHIPTONAME", customer.FirstName + " " + customer.LastName);
                else
                    pr.Add("PAYMENTREQUEST_0_SHIPTONAME", address.Name);
                pr.Add("PAYMENTREQUEST_0_SHIPTOSTREET", address.Address);
                pr.Add("PAYMENTREQUEST_0_SHIPTOCITY", address.City);
                pr.Add("PAYMENTREQUEST_0_SHIPTOSTATE", address.State);
                pr.Add("PAYMENTREQUEST_0_SHIPTOZIP", address.Postcode);
                pr.Add("PAYMENTREQUEST_0_SHIPTOCOUNTRYCODE", address.CountryCode);

                // Set email
                UserPart user = customer.As<UserPart>();
                pr.Add("PAYMENTREQUEST_0_EMAIL", user.Email);
            }
        }

    
        private void AddAllItems(PaypalRequest pr, OrderPart order)
        {
            int lineNumber = 0;
            foreach (var item in order.Details)
                AddItem(pr, ++lineNumber, item);
        }

        private void AddItem(PaypalRequest pr, int lineNumber, OrderDetail detail)
        {
            //var productPart = _services.ContentManager.Get<ProductPart>(detail.ProductPartRecord_Id);

            pr.Add("L_PAYMENTREQUEST_0_NAME" + lineNumber, detail.Sku);
            pr.Add("L_PAYMENTREQUEST_0_DESC" + lineNumber, detail.Description);
            pr.Add("L_PAYMENTREQUEST_0_QTY" + lineNumber, detail.Quantity.ToString("f2"));
            pr.Add("L_PAYMENTREQUEST_0_AMT" + lineNumber, detail.UnitPrice.ToString("f2"));
        }

        private HttpWebRequest BuildGetDetailsRequest(PaypalExpressPart paypalExpressPart, string token)
        {
            // Create the web request  
            HttpWebRequest request = WebRequest.Create(paypalExpressPart.ApiUrl) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // Create a paypal request
            PaypalRequest pr = new PaypalRequest(paypalExpressPart, "GetExpressCheckoutDetails", token);

            // format the request with data from the paypal request
            pr.SetData(request);

            return request;
        }

        private string[] GetResponseLines(WebResponse response)
        {
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string body = reader.ReadToEnd();
            string[] lines = body.Split('&');
            return lines;
        }

        private IEnumerable<string> GetValue(IEnumerable<string> lines, string key)
        {
            // finds ALL lines that start with key
            IEnumerable<string> result = lines
                .Where(l => l.ToLower()
                .StartsWith(key.ToLower()))
                .Select(s => HttpUtility.UrlDecode(s.Split('=')[1]))
                .ToArray();

            return result;
        }

        private void ExtractErrors(IEnumerable<string> lines, PaypalExpressResult result)
        {
            // log error messages
            DateTime timestamp;
            if (DateTime.TryParse(GetValue(lines, "TIMESTAMP").FirstOrDefault(), out timestamp))
                result.Timestamp = timestamp;
            result.ErrorCodes = string.Join("; ", GetValue(lines, "L_ERRORCODE").ToArray());
            result.ShortMessages = string.Join("; ", GetValue(lines, "L_SHORTMESSAGE").ToArray());
            result.LongMessages = string.Join("; ", GetValue(lines, "L_LONGMESSAGE").ToArray());
        }

        //private string GetDisplayText(ProductPart productPart)
        //{
        //    const string missingTitlePartMessage = "(No TitlePart attached)";

        //    if (productPart == null)
        //        return missingTitlePartMessage;

        //    string displayText = _services.ContentManager.GetItemMetadata(productPart).DisplayText;

        //    if (string.IsNullOrEmpty(displayText))
        //        return missingTitlePartMessage;

        //    return displayText;
        //}
    }

    /// <summary>
    /// Contains the 'form' that is to be submitted with the request
    /// </summary>
    public class PaypalRequest
    {
        // A collection of values that will be submitted
        readonly private Dictionary<string, string> values;

        /// <summary>
        /// Create a PaypalRequest with some basic data already set up
        /// </summary>
        /// <param name="part">Settings</param>
        /// <param name="method">The Paypal Method(SetExpressCheckout, GetExpressCheckoutDetails, or DoExpressCheckoutPayment)</param>
        /// <param name="token">Paypal Token (not required for SetExpressCheckout)</param>
        public PaypalRequest(PaypalExpressPart part, string method, string token = null)
        {
            values = new Dictionary<string, string>();

            // https://cms.paypal.com/us/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_nvp_NVPAPIOverview
            // Method must be first (although doesn't seem to make any diff)
            Add("METHOD", method);
            Add("VERSION", part.Version);

            // Credentials
            Add("USER", part.User);
            Add("PWD", part.Pwd);
            Add("SIGNATURE", part.Signature);
            if (!string.IsNullOrEmpty(token))
                Add("TOKEN", token);
        }

        /// <summary>
        /// Add a key and a value to the form collection
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(string key, string value)
        {
            values.Add(key, value);
        }

        /// <summary>
        /// Set the request up with the form values contained in the internal collection
        /// </summary>
        /// <param name="request"></param>
        public void SetData(HttpWebRequest request)
        {
            // Create a byte array of the data we want to send  
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(ToString());

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }
        }

        public override string ToString()
        {
            StringBuilder data = new StringBuilder();
            string prefix = "";
            foreach (var item in values)
            {
                data.Append(prefix + item.Key + "=" + HttpUtility.UrlEncode(item.Value));
                if (prefix == "")
                    prefix = "&";
            }
            return data.ToString();
        }

    }
}