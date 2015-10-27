using NServiceBus;
using NServiceBus.Transports.SQLServer;
using System;
using System.Configuration;

namespace ReceiverEndpoints
{
    class Host
    {
        private const string DB_SENDER_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Sender; Integrated Security = True";
        private const string DB_RECEIVE_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Receiver; Integrated Security = True";

        static void Main()
        {
            var azure = bool.Parse(ConfigurationManager.AppSettings["UseAzureTransport"]);
            var azureSBConnection = ConfigurationManager.AppSettings["AzureConnection"];

            BusConfiguration busConfiguration = CommonEndpointConfig.CreateCommonConfig();

            if (azure)
            {
                busConfiguration.UseTransport<AzureServiceBusTransport>()
                    .ConnectionString(azureSBConnection);
            }
            else
            {
                busConfiguration.UseTransport<SqlServerTransport>()
                   .UseSpecificConnectionInformation(
                       EndpointConnectionInfo.For("Sender")
                           .UseConnectionString(DB_SENDER_CONNECTION));
            }
            
            using (Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }        
    }


}
