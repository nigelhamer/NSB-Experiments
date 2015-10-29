using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence;
using NServiceBus.Transports.SQLServer;
using ReceiverEndpoints.Model;

namespace ReceiverEndpoints
{
    internal class EndpointConfig
    {
        private const string DB_SENDER_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Sender; Integrated Security = True";
        private const string DB_RECEIVE_CONNECTION = @"Data Source = (localdb)\ProjectsV12;Initial Catalog = Receiver; Integrated Security = True";

        public static BusConfiguration CreateAzureBusConfiguration(string azureSBConnection, bool outbox)
        {
            BusConfiguration busConfiguration = CreateAmbientTxConfiguration();

            if (outbox)
                busConfiguration.EnableOutbox();

            busConfiguration.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(azureSBConnection);

            return busConfiguration;
        }

        public static BusConfiguration CreateSQLConfiguration(bool outbox)
        {
            BusConfiguration busConfiguration = CreateAmbientTxConfiguration();

            if (outbox)
                busConfiguration.EnableOutbox();

            busConfiguration.UseTransport<SqlServerTransport>()
                .UseSpecificConnectionInformation(
                       EndpointConnectionInfo.For("Sender")
                           .UseConnectionString(DB_SENDER_CONNECTION));

            return busConfiguration;
        }

        public static BusConfiguration CreateAmbientTxConfiguration()
        {
            #region NHibernate

            Configuration hibernateConfig = new Configuration();
            hibernateConfig.DataBaseIntegration(x =>
            {
                x.ConnectionStringName = "NServiceBus/Persistence";
                x.Dialect<MsSql2012Dialect>();
            });

            ModelMapper mapper = new ModelMapper();
            mapper.AddMapping<OrderMap>();
            hibernateConfig.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            new SchemaExport(hibernateConfig).Execute(false, true, false);

            #endregion

            #region ReceiverConfiguration

            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EnableInstallers();
            busConfiguration.ScaleOut().UseSingleBrokerQueue();

            busConfiguration.EndpointName("Receiver");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.UsePersistence<NHibernatePersistence>()
               .RegisterManagedSessionInTheContainer()
               .UseConfiguration(hibernateConfig);
            //busConfiguration.UsePersistence<InMemoryPersistence>();

            busConfiguration.DisableFeature<SecondLevelRetries>();

            #endregion

            return busConfiguration;
        }

    }
}
