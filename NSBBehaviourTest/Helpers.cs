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
            Assert.AreEqual(1, Helpers.CountOrderSagaRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
        }

        public static void Assert_OrderTransactionCommitted(string orderId)
        {
            Assert.AreEqual(1, Helpers.CountOrderRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
        }

        public static void Assert_Failed_SagaDataTransactionCommitted(string orderId)
        {
            Assert.AreEqual(0, Helpers.CountOrderSagaRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
        }

        public static void Assert_Failed_OrderTransactionCommitted(string orderId)
        {
            Assert.AreEqual(0, Helpers.CountOrderRecords(Helpers.DB_RECEIVE_CONNECTION, orderId));
        }


        public static async Task PutTaskDelay()
        {
            await Task.Delay(10000);
        }      

        public static int CountOrderRecords(string connection, string orderId)
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand(string.Format("SELECT COUNT(*) FROM Orders WHERE OrderId='{0}'", orderId), conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        public static int CountOrderSagaRecords(string connection, string orderId)
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();
                SqlCommand comm = new SqlCommand(string.Format("SELECT COUNT(*) FROM OrderLifecycleSagaData WHERE OrderId='{0}'", orderId), conn);
                return (Int32)comm.ExecuteScalar();
            }
        }

        public static void CleanUpReceiverData(string connection)
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Orders", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM OrderLifecycleSagaData", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM OutboxRecord", conn);
                comm.ExecuteNonQuery();
            }
        }

        public static void CleanUpReceiverQueues(string connection)
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Receiver", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM [Receiver.DESKTOP-1N3POKE]", conn);
                comm.ExecuteNonQuery();                
            }
        }

        public static void CleanUpSenderData(string connection)
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM OutboxRecord", conn);
                comm.ExecuteNonQuery();
            }
        }

        public static void CleanUpSenderQueues(string connection)
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();

                SqlCommand comm = new SqlCommand("DELETE FROM Sender", conn);
                comm.ExecuteNonQuery();

                comm = new SqlCommand("DELETE FROM [Sender.DESKTOP-1N3POKE]", conn);
                comm.ExecuteNonQuery();
            }
        }
    }
}
