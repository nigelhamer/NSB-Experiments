using NServiceBus;
using System;

namespace NSBBehaviourTest
{
    internal static class EndpointConfig
    {
        public const string DB_SENDER_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Sender; Integrated Security = True";
        public const string DB_RECEIVE_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Receiver; Integrated Security = True";
        public const string DB_SHARED_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Shared; Integrated Security = True";
        public const string DB_BUSINESS_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Business; Integrated Security = True";


        private static BusConfiguration CreateAmbientTxConfiguration()
        {
            #region Basic Sender Configuration

            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName("Sender");

            busConfiguration.EnableInstallers();
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.ScaleOut().UseSingleBrokerQueue();
            busConfiguration.UsePersistence<NHibernatePersistence>();

            Console.WriteLine("Default Configuration");

            #endregion

            return busConfiguration;
        }

        public static BusConfiguration CreateAzureBusConfiguration(string azureSBConnection, bool useOutbox, bool disableTx)
        {
            if (disableTx && useOutbox)
                throw new ArgumentException("Cannot use Outbox when transaction are disabled");

            BusConfiguration busConfiguration = CreateAmbientTxConfiguration();           

            if (useOutbox)
            {
                busConfiguration.EnableOutbox();
                Console.WriteLine("Default Configuration overridden: Using Outbox");
            }

            if (disableTx)
            {
                // A validation error will occur if NServiceBus detects SQL Transport, even if it not being used.
                // Explicitly remove NServiceBus SQL Transport from the assembly scanning
                busConfiguration.Transactions().DisableDistributedTransactions();
                    IExcludesBuilder excludesBuilder = AllAssemblies
                        .Except("NServiceBus.Transports.SQLServer.dll");
                    busConfiguration.AssembliesToScan(excludesBuilder);

                Console.WriteLine("Default Configuration overridden: UsingTransport transaction only");
            }
            else
            {
                busConfiguration.Transactions().Enable().EnableDistributedTransactions();

                Console.WriteLine("Default Configuration: Using Ambient Transactions");
            }

            busConfiguration.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(azureSBConnection);

            Console.WriteLine("Default Configuration overridden: Using Azure Tranport");

            return busConfiguration;
        }

        public static BusConfiguration CreateSQLConfiguration(bool outbox)
        {
            BusConfiguration busConfiguration = CreateAmbientTxConfiguration();

            if (outbox)
                busConfiguration.EnableOutbox();

            busConfiguration.UseTransport<SqlServerTransport>();

            Console.WriteLine("Default Configuration overridden: Using SQL Tranport");

            return busConfiguration;
        }
    }
}
