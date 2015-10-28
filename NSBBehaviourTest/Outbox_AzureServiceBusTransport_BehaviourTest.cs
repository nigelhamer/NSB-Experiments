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
            Helpers.CleanUpReceiverData(Helpers.DB_RECEIVE_CONNECTION);
            Helpers.CleanUpSenderData(Helpers.DB_SENDER_CONNECTION);
            _azureSBConnection = System.Configuration.ConfigurationManager.AppSettings["AzureConnection"];
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helpers.CleanUpReceiverData(Helpers.DB_RECEIVE_CONNECTION);
            Helpers.CleanUpSenderData(Helpers.DB_SENDER_CONNECTION);
        }

        [TestMethod]
        public async Task Outbox_AzureTransport_Successfully_CreateDatabaseRecords()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection))
            {
                //Act
                client.SubmitOrder(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_OrderTransactionCommitted(orderId);
            Helpers.Assert_SagaDataTransactionCommitted(orderId);
            //Assert.AreEqual(string.Format("Order {0} accepted.", orderId), SharedState.HandleSuccessMessage);
        }
        
        [TestMethod]
        public async Task Outbox_AzureTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection))
            {
                //Act
                client.SubmitOrder_TransportException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_Failed_OrderTransactionCommitted(orderId);
            Helpers.Assert_Failed_SagaDataTransactionCommitted(orderId);
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);           
        }

        [TestMethod]
        public async Task Outbox_AzureSagaTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection))
            {
                //Act
                client.SubmitOrder_SagaTransportException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_Failed_OrderTransactionCommitted(orderId);
            Helpers.Assert_Failed_SagaDataTransactionCommitted(orderId);
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);           
        }
    }
}
