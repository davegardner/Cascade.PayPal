using Cascade.WebShop.Models;
using Orchard;
using Cascade.Paypal.Models;

namespace Cascade.Paypal.Services
{
    public interface IPaypalTransactionService : IDependency
    {
        void LogTransaction(OrderPart order, PaypalExpressResult result);
        int GetOrderId(string paypalToken);
    }
}