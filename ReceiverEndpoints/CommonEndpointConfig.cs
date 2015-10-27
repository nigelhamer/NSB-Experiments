using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence;
using ReceiverEndpoints.Model;

namespace ReceiverEndpoints
{
    class CommonEndpointConfig
    {
        public  static BusConfiguration CreateCommonConfig() {

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
           

            busConfiguration.EnableOutbox();
            busConfiguration.DisableFeature<SecondLevelRetries>();            

            #endregion

            return busConfiguration;
        }    
    }
}
