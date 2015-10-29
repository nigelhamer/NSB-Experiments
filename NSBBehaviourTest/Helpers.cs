using Microsoft.VisualStudio.TestTools.UnitTesting;
using NServiceBus;
using Shared;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NSBBehaviourTest
{
    internal static class Helpers
    {        
        public static void Assert_SagaDataTransactionCommitted_InSharedDB(string orderId)
        {
            Assert.AreEqual(1, CountSharedSagaRecords(orderId), "Saga Data Missing");
        }

        public static void Assert_SagaDataTransactionCommitted_InBusinessDB(string orderId)
        {
            Assert.AreEqual(1, CountBusinessSagaRecords(orderId), "Saga Data Missing");
        }

        public static void Assert_OrderTransactionCommitted(string orderId)
        {
            Assert.AreEqual(1, CountBusinessOrderRecords(orderId), "Business Data Missing");
        }

        public static void Assert_Failed_SagaDataTransactionCommitted_InSharedDB(string orderId)
        {
            Assert.AreEqual(0, CountSharedSagaRecords(orderId), "Unexpected Saga Data Found");
        }

        public static void Assert_Failed_SagaDataTransactionCommitted_InBusinessDB(string orderId)
        {
            Assert.AreEqual(0, CountBusinessOrderRecords(orderId), "Unexpected Saga Data Found");
        }

        public static void Assert_Failed_OrderTransactionCommitted(string orderId)
        {
            Assert.AreEqual(0, CountBusinessOrderRecords(orderId), "Unexpected Business Data Found");
        }

        public static void Assert_OutboxWasUsed_InBusinessDB()
        {
            Assert.AreEqual(2, CountBusinessOutboxRecords(), "Outbox was not used");
        }

        public static void Assert_OutboxWasNotUsedOrWasRolledBack_InBusinessDB()
        {
            Assert.AreEqual(0, CountBusinessOutboxRecords(), "Outbox contains record where none were expected.");
        }

        public static async Task PutTaskDelay()
        {
            await Task.Delay(10000);
        }

        #region Validate Data  

        public static int CountBusinessOrderRecords(string orderId)
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_BUSINESS_CONNECTION))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand(string.Format("SELECT COUNT(*) FROM Orders WHERE OrderId='{0}'", orderId), conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        public static int CountSharedSagaRecords(string orderId)
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_SHARED_CONNECTION))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand(string.Format("SELECT COUNT(*) FROM OrderLifecycleSagaData WHERE OrderId='{0}'", orderId), conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        public static int CountBusinessSagaRecords(string orderId)
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_BUSINESS_CONNECTION))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand(string.Format("SELECT COUNT(*) FROM OrderLifecycleSagaData WHERE OrderId='{0}'", orderId), conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        public static int CountBusinessOutboxRecords()
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_BUSINESS_CONNECTION))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM OutboxRecord", conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        #endregion

        #region Cleanup 

        public static void CleanupOrders()
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_BUSINESS_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Orders", conn);
                comm.ExecuteNonQuery();                              
            }
        }

        public static void CleanupNSBPersistenceTable_FromBusiness()
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_BUSINESS_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM OrderLifecycleSagaData", conn);
                comm.ExecuteNonQuery();
            }
        }

        public static void CleanupNSBPersistenceTable_FromShared()
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_SHARED_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM OrderLifecycleSagaData", conn);
                comm.ExecuteNonQuery();
            }
        }

        public static void CleanUpReceiverQueues()
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_RECEIVE_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Receiver", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM [Receiver.DESKTOP-1N3POKE]", conn);
                comm.ExecuteNonQuery();                
            }
        }       

        public static void CleanUpSenderQueues()
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_SENDER_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Sender", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM [Sender.DESKTOP-1N3POKE]", conn);
                comm.ExecuteNonQuery();
            }
        }

        public static void CleanUpBusinessOutbox()
        {
            using (SqlConnection conn = new SqlConnection(EndpointConfig.DB_BUSINESS_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM OutboxRecord", conn);
                comm.ExecuteNonQuery();

            }
        }

        #endregion
    }
}
