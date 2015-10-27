using NServiceBus;
using Shared;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NSBBehaviourTest
{
    public static class Helpers
    {
        public const string DB_SENDER_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = sender; Integrated Security = True";
        public const string DB_RECEIVE_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = receiver; Integrated Security = True";

        public static async Task PutTaskDelay()
        {
            await Task.Delay(10000);
        }  

        public static void SubmitOrder(Random random, string orderId, BusConfiguration busConfiguration)
        {
            using (IBus bus = Bus.Create(busConfiguration).Start())
            {
                bus.Publish(new OrderSubmitted
                {
                    OrderId = orderId,
                    Value = random.Next(100),
                    ThrowDataException = false,
                    ThrowTransportException = false
                });
            }
        }
        public static void SubmitOrder_TransportException(Random random, string orderId, BusConfiguration busConfiguration)
        {
            using (IBus bus = Bus.Create(busConfiguration).Start())
            {
                bus.Publish(new OrderSubmitted
                {
                    OrderId = orderId,
                    Value = random.Next(100),
                    ThrowDataException = false,
                    ThrowTransportException = true
                });
            }
        }

        public static BusConfiguration CreateCommonConfig()
        {
            #region Sender Configuration

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
