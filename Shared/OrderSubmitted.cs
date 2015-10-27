using NServiceBus;

namespace Shared
{
    public class OrderSubmitted : IEvent
    {
        public string OrderId { get; set; }
        public decimal Value { get; set; }
        public bool ThrowTransportException { get; set; }
        public bool ThrowDataException { get; set; }
    }
}
