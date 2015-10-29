using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence;
using NServiceBus.Transports.SQLServer;
using ReceiverEndpoints.Model;
using System;

namespace ReceiverEndpoints
{
    internal class EndpointConfig
    {
        public const string DB_SENDER_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Sender; Integrated Security = True";
        public const string DB_RECEIVE_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Receiver; Integrated Security = True";
        public const string DB_SHARED_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Shared; Integrated Security = True";
        public const string DB_BUSINESS_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Business; Integrated Security = True";

        public static BusConfiguration CreateAzureBusConfiguration(string azureSBConnection, bool useOutbox, bool disableTx)
        {
            if (disableTx && useOutbox)
                throw new ArgumentException("Cannot use Outbox when transactions are disabled");

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

                // Using different databases only work when transaction are disabled
                ConfigureHibernate(busConfiguration, DB_SHARED_CONNECTION);

                Console.WriteLine("Default Configuration overridden: Using Transport transaction only");
            }
            else
            {
                ConfigureHibernate(busConfiguration, DB_BUSINESS_CONNECTION);

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
            {
                busConfiguration.EnableOutbox();
                Console.WriteLine("Default Configuration overridden: Using Outbox");
            }

            busConfiguration.UseTransport<SqlServerTransport>()
                .UseSpecificConnectionInformation(
                       EndpointConnectionInfo.For("Sender")
                           .UseConnectionString(DB_SENDER_CONNECTION));

            Console.WriteLine("Default Configuration overridden: Using SQL Tranport");

            return busConfiguration;
        }

        public static void ConfigureHibernate(BusConfiguration busConfiguration, string connectionString)
        {
            #region NHibernate

            Configuration hibernateConfig = new Configuration();
            hibernateConfig.DataBaseIntegration(x =>
            {
                x.ConnectionString = connectionString;
                x.Dialect<MsSql2012Dialect>();
            });

            ModelMapper mapper = new ModelMapper();
            mapper.AddMapping<OrderMap>();
            hibernateConfig.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            new SchemaExport(hibernateConfig).Execute(false, true, false);

            #endregion

            busConfiguration.UsePersistence<NHibernatePersistence>()
               .RegisterManagedSessionInTheContainer()
               .UseConfiguration(hibernateConfig); // Don't really need this if use EF     
        }

        public static BusConfiguration CreateAmbientTxConfiguration()
        {           
            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EnableInstallers();
            busConfiguration.ScaleOut().UseSingleBrokerQueue();

            busConfiguration.EndpointName("Receiver");
            busConfiguration.UseSerialization<JsonSerializer>();            

            busConfiguration.DisableFeature<SecondLevelRetries>();

            Console.WriteLine("Default Configuration");

            return busConfiguration;
        }
    }
}
