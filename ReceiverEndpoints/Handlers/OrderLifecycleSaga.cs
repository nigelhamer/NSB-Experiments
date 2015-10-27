using NServiceBus.Saga;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiverEndpoints.Handlers
{
    public class OrderLifecycleSaga : Saga<OrderLifecycleSagaData>,
        IAmStartedByMessages<OrderSubmitted>,
        IHandleTimeouts<OrderTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderLifecycleSagaData> mapper)
        {
        }

        public void Handle(OrderSubmitted message)
        {
            Data.OrderId = message.OrderId;

            #region Timeout

            RequestTimeout<OrderTimeout>(TimeSpan.FromSeconds(5));

            #endregion
        }

        public void Timeout(OrderTimeout state)
        {
            var throwException = false;
            if (throwException) throw new Exception("Blow up!");
            Console.WriteLine("Got timeout");
        }
    }
}
