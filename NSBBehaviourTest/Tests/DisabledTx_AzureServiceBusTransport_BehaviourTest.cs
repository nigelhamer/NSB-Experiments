using Microsoft.VisualStudio.TestTools.UnitTesting;
using NServiceBus;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NSBBehaviourTest
{
    [TestClass]
    public class DisabledTx_AzureServiceBusTransport_BehaviourTest
    {
            private static string _azureSBConnection;

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            //Helpers.CleanupOrders();
            //Helpers.CleanupNSBPersistenceTable_FromShared();

            _azureSBConnection = System.Configuration.ConfigurationManager.AppSettings["AzureConnection"];
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helpers.CleanupOrders();
            Helpers.CleanupNSBPersistenceTable_FromShared();
        }

        [TestMethod]
        public async Task DisabledTx_AzureTransport_Successfully_CreateDatabaseRecords()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection, false, true))
            {
                //Act
                client.SubmitOrder(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_OrderTransactionCommitted(orderId);
            Helpers.Assert_SagaDataTransactionCommitted_InSharedDB(orderId);           
            //Assert.AreEqual(string.Format("Order {0} accepted.", orderId), SharedState.HandleSuccessMessage);
        }
        
        [TestMethod]
        public async Task DisabledTx_AzureTransport_RollsbackSagaDataButNotBusinessDataOnTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection, false, true))
            {
                //Act
                client.SubmitOrder_TransportException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_OrderTransactionCommitted(orderId);
            Helpers.Assert_Failed_SagaDataTransactionCommitted_InSharedDB(orderId);            
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);           
        }

        [TestMethod]
        public async Task DisabledTx_AzureTransaction_RollsbackBusinessDataButNotSagaDataOnDataException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection, false, true))
            {
                //Act
                client.SubmitOrder_DataException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_Failed_OrderTransactionCommitted(orderId);
            Helpers.Assert_SagaDataTransactionCommitted_InSharedDB(orderId);
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);           
        }

        [TestMethod]
        public async Task DisabledTx_AzureTransport_RollsbackSagaAndDataOnSagaTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartAzureEndpoint(_azureSBConnection, false, true))
            {
                //Act
                client.SubmitOrder_SagaTransportException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_Failed_OrderTransactionCommitted(orderId);
            Helpers.Assert_Failed_SagaDataTransactionCommitted_InSharedDB(orderId);
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);           
        }
    }
}
