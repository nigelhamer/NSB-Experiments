using NServiceBus;
using Shared;
using System;
using System.Linq;

namespace NSBBehaviourTest
{
    public class Client
    {
        public static string GetRandomOrderId()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
            Random random = new Random();
            return new string(Enumerable.Range(0, 4).Select(x => letters[random.Next(letters.Length)]).ToArray());
        }
        
        public IBus StartAzureEndpoint(string azureSBConnection, bool useOutbox, bool disableTx)
        {
            return Bus.Create(EndpointConfig.CreateAzureBusConfiguration(azureSBConnection, useOutbox, disableTx)).Start();
        }        

        public IBus StartSQLEndpoint(bool useOutbox)
        {
            return Bus.Create(EndpointConfig.CreateSQLConfiguration(useOutbox)).Start();
        }

        public void SubmitOrder(string orderId, IBus bus)
        {
            Console.WriteLine("Running... Successful Submit Order");

            Random random = new Random();            
            bus.Publish(new OrderSubmitted
            {
                OrderId = orderId,
                Value = random.Next(100),
                ThrowDataException = false,
                ThrowTransportException = false,
                ThrowSagaDataException = false,
                ThrowSagaTransportException = false,
                ThrowSagaTimeoutException = false
            });            
        }
        public void SubmitOrder_DataException(string orderId, IBus bus)
        {
            Console.WriteLine("Running... Failed Submit Order - Data Exception in first handler.");

            Random random = new Random();
            bus.Publish(new OrderSubmitted
            {
                OrderId = orderId,
                Value = random.Next(100),
                ThrowDataException = true,
                ThrowTransportException = false,
                ThrowSagaDataException = false,
                ThrowSagaTransportException = false,
                ThrowSagaTimeoutException = false
            });
        }

        public void SubmitOrder_TransportException(string orderId, IBus bus)
        {
            Console.WriteLine("Running... Failed Submit Order - Transport Exception in first handler.");

            Random random = new Random();
            bus.Publish(new OrderSubmitted
            {
                OrderId = orderId,
                Value = random.Next(100),
                ThrowDataException = false,
                ThrowTransportException = true,
                ThrowSagaDataException = false,
                ThrowSagaTransportException = false,
                ThrowSagaTimeoutException = false
            });
        }

        public void SubmitOrder_SagaTransportException(string orderId, IBus bus)
        {
            Console.WriteLine("Running... Failed Submit Order - Transport Exception in saga handler.");

            Random random = new Random();
            bus.Publish(new OrderSubmitted
            {
                OrderId = orderId,
                Value = random.Next(100),
                ThrowDataException = false,
                ThrowTransportException = false,
                ThrowSagaDataException = false,
                ThrowSagaTransportException = true,
                ThrowSagaTimeoutException = false
            });
        }

        public void SubmitOrder_SagaTimeoutException(string orderId, IBus bus)
        {
            Console.WriteLine("Running... Failed Submit Order - Transport Exception in saga timeout.");
            
            Random random = new Random();
            bus.Publish(new OrderSubmitted
            {
                OrderId = orderId,
                Value = random.Next(100),
                ThrowDataException = false,
                ThrowTransportException = false,
                ThrowSagaDataException = false,
                ThrowSagaTransportException = false,
                ThrowSagaTimeoutException = true
            });
        }
    }
}
