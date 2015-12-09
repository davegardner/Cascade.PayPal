using Cascade.Paypal.Models;
using Cascade.WebShop.Models;
using Orchard;

namespace Cascade.Paypal.Services
{
    public interface IPaypalExpressServices : IDependency
    {
        PaypalExpressResult SetExpressCheckout(PaypalExpressPart paypalExpressPart, OrderPart order);
        PaypalExpressResult GetExpressCheckoutDetails(PaypalExpressPart paypalExpressPart, string token);
        PaypalExpressResult DoExpressCheckoutPayment(PaypalExpressPart paypalExpressPart, OrderPart order, string token, string payerId);
        //Todo: refunds
    }
}