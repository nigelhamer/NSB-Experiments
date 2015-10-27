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
            Helpers.CleanUpReceiverData(Helpers.DB_RECEIVE_CONNECTION);
            Helpers.CleanUpReceiverQueues(Helpers.DB_RECEIVE_CONNECTION);
            Helpers.CleanUpSenderData(Helpers.DB_SENDER_CONNECTION);
            Helpers.CleanUpSenderQueues(Helpers.DB_SENDER_CONNECTION);
        }    

        [TestMethod]
        public async Task Outbox_SQLTransport_Successfully_CreateDatabaseRecords()
        {
            //Arrange
            SharedState.HandleSuccessMessage = "";
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
            Random random = new Random();
            string orderId = new string(Enumerable.Range(0, 4).Select(x => letters[random.Next(letters.Length)]).ToArray());

            BusConfiguration busConfiguration = Helpers.CreateCommonConfig();
            busConfiguration.UseTransport<SqlServerTransport>();

            //Act
            Helpers.SubmitOrder(random, orderId, busConfiguration);

            // Assert
            await Helpers.PutTaskDelay();

            Assert.AreEqual(1, Helpers.CountOrderRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
            Assert.AreEqual(1, Helpers.CountOrderSagaRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
            //Assert.AreEqual(string.Format("Order {0} accepted.", orderId), SharedState.HandleSuccessMessage);
        }

        [TestMethod]
        public async Task Outbox_SQLTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            SharedState.HandleSuccessMessage = "";
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
            Random random = new Random();
            string orderId = new string(Enumerable.Range(0, 4).Select(x => letters[random.Next(letters.Length)]).ToArray());

            BusConfiguration busConfiguration = Helpers.CreateCommonConfig();
            busConfiguration.UseTransport<SqlServerTransport>();

            //Act
            Helpers.SubmitOrder_TransportException(random, orderId, busConfiguration);

            // Assert
            await Helpers.PutTaskDelay();

            Assert.AreEqual(0, Helpers.CountOrderRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
            Assert.AreEqual(0, Helpers.CountOrderSagaRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
            //Assert.AreEqual("", SharedState.HandleSuccessMessage);
        }     
    }
}
