using Cascade.Paypal.Models;
using Cascade.WebShop.Models;
using Orchard;

namespace Cascade.Paypal.Services
{
    public interface IPaypalExpressServices : IDependency
    {
        PaypalExpressResult SetExpressCheckout(PaypalExpressPart paypalExpressPart, OrderRecord order);
        PaypalExpressResult GetExpressCheckoutDetails(PaypalExpressPart paypalExpressPart, string token);
        PaypalExpressResult DoExpressCheckoutPayment(PaypalExpressPart paypalExpressPart, OrderRecord order, string token, string payerId);
        //Todo: refunds
    }
}