using NServiceBus.Saga;

namespace ReceiverEndpoints.Handlers
{
    public class OrderLifecycleSagaData : ContainSagaData
    {
        public virtual string OrderId { get; set; }
    }
}
