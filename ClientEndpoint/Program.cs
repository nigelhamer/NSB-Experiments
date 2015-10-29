using NSBBehaviourTest;
using NServiceBus;
using System;

namespace ClientEndpoint
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();           
            string azureSBConnection = System.Configuration.ConfigurationManager.AppSettings["AzureConnection"];

            using (IBus bus = client.StartAzureEndpoint(azureSBConnection, true))
            //using (IBus bus = client.StartSQLEndpoint())
            {
                Console.WriteLine("Press enter to publish a message");
                Console.WriteLine("Press any key to exit");
                while (true)
                {
                    Console.WriteLine();
                    ConsoleKeyInfo key = Console.ReadKey();
                    Console.WriteLine();
                    if (key.Key != ConsoleKey.Enter)
                    {
                        return;
                    }
                    string orderId = Client.GetRandomOrderId();

                    client.SubmitOrder(orderId, bus);
                    //client.SubmitOrder_TransportException(orderId, bus);
                    //client.SubmitOrder_SagaTransportException(orderId, bus);
                    //client.SubmitOrder_SagaTimeoutException(orderId, bus);
                }

            }
        }
    }
}