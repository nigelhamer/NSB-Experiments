using NServiceBus;

namespace Shared
{
    public class OrderAccepted : IMessage
    {
        public string OrderId { get; set; }
        
    }
}
