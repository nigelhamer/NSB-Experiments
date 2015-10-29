using Microsoft.VisualStudio.TestTools.UnitTesting;
using NServiceBus;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NSBBehaviourTest
{
    [TestClass]
    public class Outbox_AzureServiceBusTransport_BehaviourTest
    {
    
        private static string _azureSBConnection;

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            Helpers.CleanUpReceiverData();
            Helpers.CleanUpSenderOutbox();

            _azureSBConnection = System.Configuration.ConfigurationManager.AppSettings["AzureConnection"];
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helpers.CleanUpReceiverData();
            Helpers.CleanUpSenderOutbox();
        }

        [TestMethod]
        public async Task Outbox_AzureTransport_Successfully_CreateDatabaseRecords()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection, true))
            {
                //Act
                client.SubmitOrder(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_OrderTransactionCommitted(orderId);
            Helpers.Assert_SagaDataTransactionCommitted(orderId);
            Helpers.Assert_OutboxWasUsed();
            //Assert.AreEqual(string.Format("Order {0} accepted.", orderId), SharedState.HandleSuccessMessage);
        }
        
        [TestMethod]
        public async Task Outbox_AzureTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection, true))
            {
                //Act
                client.SubmitOrder_TransportException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_Failed_OrderTransactionCommitted(orderId);
            Helpers.Assert_Failed_SagaDataTransactionCommitted(orderId);
            Helpers.Assert_OutboxWasNotUsedOrRollbacked();
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);           
        }

        [TestMethod]
        public async Task Outbox_AzureSagaTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection, true))
            {
                //Act
                client.SubmitOrder_SagaTransportException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_Failed_OrderTransactionCommitted(orderId);
            Helpers.Assert_Failed_SagaDataTransactionCommitted(orderId);
            Helpers.Assert_OutboxWasNotUsedOrRollbacked();
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);           
        }
    }
}
