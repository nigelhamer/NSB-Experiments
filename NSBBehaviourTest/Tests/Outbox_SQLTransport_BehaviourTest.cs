using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NServiceBus;
using System.Linq;
using System.Threading.Tasks;

namespace NSBBehaviourTest
{
    [TestClass]
    public class Outbox_SQLTransport_BehaviourTest
    {        
        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            //Helpers.CleanUpReceiverData(Helpers.DB_RECEIVE_CONNECTION);
            //Helpers.CleanUpReceiverQueues(Helpers.DB_RECEIVE_CONNECTION);
            //Helpers.CleanUpSenderData(Helpers.DB_SENDER_CONNECTION);
            //Helpers.CleanUpSenderQueues(Helpers.DB_SENDER_CONNECTION);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helpers.CleanupOrders();
            Helpers.CleanUpReceiverQueues();           
            Helpers.CleanUpSenderQueues();
        }    

        [TestMethod]
        public async Task Outbox_SQLTransport_Successfully_CreateDatabaseRecords()
        {
            //Arrange
            SharedState.HandleSuccessMessage = "";

            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartSQLEndpoint(true))
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
        public async Task Outbox_SQLTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartSQLEndpoint(true))
            {
                //Act
                client.SubmitOrder_TransportException(orderId, bus);
            }

            // Assert
            await Helpers.PutTaskDelay();

            Helpers.Assert_Failed_OrderTransactionCommitted(orderId);
            Helpers.Assert_Failed_SagaDataTransactionCommitted_InSharedDB(orderId);
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);
        }

        [TestMethod]
        public async Task Outbox_SQLSagaTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            Client client = new Client();
            string orderId = Client.GetRandomOrderId();

            using (IBus bus = client.StartSQLEndpoint(true))
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
