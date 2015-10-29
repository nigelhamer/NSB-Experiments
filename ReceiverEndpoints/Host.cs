using NServiceBus;
using System;
using System.Configuration;

namespace ReceiverEndpoints
{
    class Host
    {        
        static void Main()
        {
            var azure = bool.Parse(ConfigurationManager.AppSettings["UseAzureTransport"]);
            var azureSBConnection = ConfigurationManager.AppSettings["AzureConnection"];

            BusConfiguration busConfiguration = null;
            if (azure)
            {
                busConfiguration = EndpointConfig.CreateAzureBusConfiguration(azureSBConnection, true, false);               
            }
            else
            {
                busConfiguration = EndpointConfig.CreateSQLConfiguration(true);              
            }
            
            using (Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey(); 
            }
        }        
    }
}
