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

        private  BusConfiguration CreateCommonConfig()
        {
            #region Basic Sender Configuration

            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName("Sender");

            busConfiguration.EnableInstallers();
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.ScaleOut().UseSingleBrokerQueue();
            busConfiguration.UsePersistence<NHibernatePersistence>();
            busConfiguration.EnableOutbox();

            #endregion

            return busConfiguration;
        }

        private BusConfiguration CreateAzureBusConfiguration(string azureSBConnection)
        {
            BusConfiguration busConfiguration = CreateCommonConfig();
            busConfiguration.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(azureSBConnection);

            return busConfiguration;
        }
        
        public IBus StartAzureEndpoint(string azureSBConnection) {
            return Bus.Create(CreateAzureBusConfiguration(azureSBConnection)).Start();
        }

        private BusConfiguration CreateSQLConfiguration()
        {
            BusConfiguration busConfiguration = CreateCommonConfig();
            busConfiguration.UseTransport<SqlServerTransport>();

            return busConfiguration;
        }

        public IBus StartSQLEndpoint()
        {
            return Bus.Create(CreateSQLConfiguration()).Start();
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

        public void SubmitOrder_TransportException(string orderId, IBus bus)
        {
            Console.WriteLine("Running... Failed Submit Order - Transport Exeception in first handler.");

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
            Console.WriteLine("Running... Failed Submit Order - Transport Exeception in saga handler.");

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
            Console.WriteLine("Running... Failed Submit Order - Transport Exeception in saga timeout.");
            
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
