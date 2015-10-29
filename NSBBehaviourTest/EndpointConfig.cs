using NServiceBus;

namespace NSBBehaviourTest
{
    internal static class EndpointConfig
    {
        private static BusConfiguration CreateAmbientTxConfiguration()
        {
            #region Basic Sender Configuration

            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName("Sender");

            busConfiguration.EnableInstallers();
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.ScaleOut().UseSingleBrokerQueue();
            busConfiguration.UsePersistence<NHibernatePersistence>();            

            #endregion

            return busConfiguration;
        }

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

            busConfiguration.UseTransport<SqlServerTransport>();

            return busConfiguration;
        }
    }
}
