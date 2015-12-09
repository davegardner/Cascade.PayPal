using Cascade.Paypal.Models;
using Cascade.Paypal.Services;
using Cascade.WebShop.Models;
using Cascade.WebShop.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Mvc;
using System.Web.Mvc;

namespace Cascade.Paypal.Controllers
{
    public class PaypalResponseController : Controller
    {
        private readonly IOrchardServices _services;
        private readonly IPaypalExpressServices _paypalExpressServices;
        private readonly IOrderService _orderService;
        private readonly IPaypalTransactionService _paypalTransactionService;
        private readonly IWebshopSettingsService _webshopSettings;

        public PaypalResponseController(IOrchardServices services, IPaypalExpressServices paypalExpressServices, IOrderService orderService, IPaypalTransactionService paypalTransactionService, IWebshopSettingsService webshopSettings)
        {
            _services = services;
            _paypalExpressServices = paypalExpressServices;
            _orderService = orderService;
            _paypalTransactionService = paypalTransactionService;
            _webshopSettings = webshopSettings;
        }

        public ActionResult Index(string orderReference, int amount)
        {
            PaypalExpressPart paypalExpressPart = _services.WorkContext.CurrentSite.As<PaypalExpressPart>();

            // set the success and cancel urls
            string baseUrl = "http://" + Request.Url.Authority + Request.Path;
            paypalExpressPart.CancelUrl = baseUrl + "/Cancel";
            paypalExpressPart.SuccessUrl = baseUrl + "/Success";

            // retrieve the order from the repo
            OrderPart order = _orderService.GetOrderByNumber(orderReference);

            // call PaymentExpressService
            PaypalExpressResult result = _paypalExpressServices.SetExpressCheckout(paypalExpressPart, order);

            // log paypal transaction
            _paypalTransactionService.LogTransaction(order, result);

            if (string.Compare(result.Ack, "success", true) == 0)
            {
                // Redirect to PayPal
                return new RedirectResult(paypalExpressPart.AuthorizationUrl + "&token=" + result.Token);
            }

            // if we get here then we were unable to get a token from SetExpressCheckout
            // advise user we have a technical problem, please try again/later
            var shape = _services.New.UnableToContactPaypal(RedirectTo: "~/Cascade.WebShop/Checkout/Summary");
            return new ShapeResult(this, shape);
        }

        public ActionResult Success(string token, string PayerID)
        {
            // NOTE:
            // This code calls PayPal web services synchronously
            //

            // Retrieve the order number
            int orderId = _paypalTransactionService.GetOrderId(token);
            string orderReference = (orderId + 1000).ToString();

            // retrieve the order
            OrderPart order = _orderService.GetOrderByNumber(orderReference);

            // Call GetExpressCheckoutDetails
            PaypalExpressPart paypalExpressPart = _services.WorkContext.CurrentSite.As<PaypalExpressPart>();
            var result = _paypalExpressServices.GetExpressCheckoutDetails(paypalExpressPart, token);

            // Log the result
            _paypalTransactionService.LogTransaction(order, result);

            if (string.Compare(result.Ack, "success", true) == 0)
            {
                // Call DoExpressCheckoutPayment
                var doResult = _paypalExpressServices.DoExpressCheckoutPayment(paypalExpressPart, order, token, result.PayerId);

                // Log the result
                _paypalTransactionService.LogTransaction(order, doResult);

                if (string.Compare(doResult.Ack, "success", true) == 0)
                {
                    // Display success page
                    string paymentId = token;
                    string command = "Success";
                    return RedirectToAction("PaymentResponse", "Order", new { area = "Cascade.WebShop", paymentId = paymentId, result = command, orderReference });
                }
            }

            // advise user we have a technical problem, please try again/later
            var shape = _services.New.UnableToContactPaypal(RedirectTo: "~/Cascade.WebShop/Checkout/Summary");
            return new ShapeResult(this, shape);
        }

        public ActionResult Cancel(string token)
        {
            // Todo: obtain payment and error details

            // Display reason for failure and suggestions for how to overcome
            //string paymentId = "";
            //string command = "Failed";
            //string orderReference = token;
            //return RedirectToAction("PaymentResponse", "Order", new { area = "Cascade.WebShop", paymentId = paymentId, result = command, orderReference });

            string baseUrl = "http://" + Request.Url.Authority;
            string catalogUrl = baseUrl + _webshopSettings.GetContinueShoppingUrl();
            return Redirect(catalogUrl);
        }

    }
}