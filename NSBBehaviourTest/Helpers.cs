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
        public const string DB_SENDER_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = sender; Integrated Security = True";
        public const string DB_RECEIVE_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = receiver; Integrated Security = True";


        public static void Assert_SagaDataTransactionCommitted(string orderId)
        {
            Assert.AreEqual(1, CountReceiverSagaRecords(orderId), "Saga Data Missing");
        }

        public static void Assert_OrderTransactionCommitted(string orderId)
        {
            Assert.AreEqual(1, CountReceiverOrderRecords(orderId), "Business Data Missing");
        }

        public static void Assert_Failed_SagaDataTransactionCommitted(string orderId)
        {
            Assert.AreEqual(0, CountReceiverSagaRecords(orderId), "Unexpected Saga Data Found");
        }

        public static void Assert_Failed_OrderTransactionCommitted(string orderId)
        {
            Assert.AreEqual(0, CountReceiverOrderRecords(orderId), "Unexpected Business Data Found");
        }

        public static void Assert_OutboxWasUsed()
        {
            Assert.AreEqual(1, CountSenderOutboxRecords(), "Outbox was not used");
        }

        public static void Assert_OutboxWasNotUsedOrRollbacked()
        {
            Assert.AreEqual(0, CountSenderOutboxRecords(), "Outbox contains record where none were expected.");
        }

        public static async Task PutTaskDelay()
        {
            await Task.Delay(10000);
        }

        #region Validate Data  

        public static int CountReceiverOrderRecords(string orderId)
        {
            using (SqlConnection conn = new SqlConnection(DB_RECEIVE_CONNECTION))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand(string.Format("SELECT COUNT(*) FROM Orders WHERE OrderId='{0}'", orderId), conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        public static int CountReceiverSagaRecords(string orderId)
        {
            using (SqlConnection conn = new SqlConnection(DB_RECEIVE_CONNECTION))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand(string.Format("SELECT COUNT(*) FROM OrderLifecycleSagaData WHERE OrderId='{0}'", orderId), conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        public static int CountSenderOutboxRecords()
        {
            using (SqlConnection conn = new SqlConnection(DB_SENDER_CONNECTION))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM OutboxRecord", conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        #endregion

        #region Cleanup 

        public static void CleanUpReceiverData()
        {
            using (SqlConnection conn = new SqlConnection(DB_RECEIVE_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Orders", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM OrderLifecycleSagaData", conn);
                comm.ExecuteNonQuery();                
            }
        }

        public static void CleanUpReceiverOutbox()
        {
            using (SqlConnection conn = new SqlConnection(DB_RECEIVE_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM OutboxRecord", conn);
                comm.ExecuteNonQuery();
            }
        }

        public static void CleanUpReceiverQueues()
        {
            using (SqlConnection conn = new SqlConnection(DB_RECEIVE_CONNECTION))
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
            using (SqlConnection conn = new SqlConnection(DB_SENDER_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Sender", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM [Sender.DESKTOP-1N3POKE]", conn);
                comm.ExecuteNonQuery();
            }
        }

        public static void CleanUpSenderOutbox()
        {
            using (SqlConnection conn = new SqlConnection(DB_SENDER_CONNECTION))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM OutboxRecord", conn);
                comm.ExecuteNonQuery();

            }
        }

        #endregion
    }
}
