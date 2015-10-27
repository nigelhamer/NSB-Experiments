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
            SharedState.HandleSuccessMessage = "";
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
            Random random = new Random();
            string orderId = new string(Enumerable.Range(0, 4).Select(x => letters[random.Next(letters.Length)]).ToArray());

            BusConfiguration busConfiguration = Helpers.CreateCommonConfig();
            busConfiguration.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(_azureSBConnection);

            //Act
            Helpers.SubmitOrder(random, orderId, busConfiguration);

            // Assert
            await Helpers.PutTaskDelay();

            Assert.AreEqual(1, Helpers.CountOrderRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
            Assert.AreEqual(1, Helpers.CountOrderSagaRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
            //Assert.AreEqual(string.Format("Order {0} accepted.", orderId), SharedState.HandleSuccessMessage);
        }

        [TestMethod]
        public async Task Outbox_AzureTransport_RollsbackSagaAndDataOnTransportException()
        {
            //Arrange
            SharedState.HandleSuccessMessage = "";
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
            Random random = new Random();
            string orderId = new string(Enumerable.Range(0, 4).Select(x => letters[random.Next(letters.Length)]).ToArray());

            BusConfiguration busConfiguration = Helpers.CreateCommonConfig();
            busConfiguration.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(_azureSBConnection);

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
