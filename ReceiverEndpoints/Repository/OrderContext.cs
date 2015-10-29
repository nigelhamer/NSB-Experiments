
using System.Data.Entity;
using System.Data;
using System.Data.Common;
using System.Data.Entity.ModelConfiguration;
using ReceiverEndpoints.Model;

namespace ReceiverEndpoints.Repository
{
    class OrderContext : DbContext
    {
        public OrderContext()
            : base(EndpointConfig.DB_BUSINESS_CONNECTION)
        {
        }

        public OrderContext(IDbConnection connection)
            : base((DbConnection)connection, false)
        {
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            EntityTypeConfiguration<Order> orders = modelBuilder.Entity<Order>();
            orders.ToTable("Orders");
            orders.HasKey(x => x.OrderId);
            orders.Property(x => x.Value);
        }
    }
}