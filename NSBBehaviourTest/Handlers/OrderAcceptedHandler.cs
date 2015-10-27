using NServiceBus;
using Shared;
using System;

namespace NSBBehaviourTest.Handlers
{
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        public void Handle(OrderAccepted message)
        {
            Console.WriteLine("Order {0} accepted.", message.OrderId);
            SharedState.HandleSuccessMessage = string.Format("Order {0} accepted.", message.OrderId);
        }
    }
}
