using System;
using System.Linq;
using Cascade.Paypal.Models;
using Cascade.WebShop.Models;
using Orchard.Data;

namespace Cascade.Paypal.Services
{
    public class PaypalTransactionService : IPaypalTransactionService
    {
        private readonly IRepository<TransactionRecord> _paypalTransactions;

        public PaypalTransactionService(IRepository<TransactionRecord> paypalTransactions)
        {
            _paypalTransactions = paypalTransactions;
        }

        public void LogTransaction(OrderPart order, PaypalExpressResult result)
        {
            TransactionRecord transaction = new TransactionRecord { 
                Token = result.Token, 
                Ack = result.Ack, 
                Method = result.Method, 
                OrderRecord_Id=order.Id, 
                DateTime=DateTime.Now,
                Timestamp=result.Timestamp,
                ErrorCodes=result.ErrorCodes,
                ShortMessages=result.ShortMessages,
                LongMessages=result.LongMessages
            };
            _paypalTransactions.Create(transaction);
        }

        public int GetOrderId(string paypalToken)
        {
            var transaction = _paypalTransactions.Table.FirstOrDefault(t => t.Token == paypalToken);
            return transaction.OrderRecord_Id;
        }


    }
}