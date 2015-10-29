using NHibernate;
using NServiceBus;
using NServiceBus.Persistence.NHibernate;
using ReceiverEndpoints.Repository;
using Shared;
using System;
using System.Data.Common;
using System.Data.Entity;

namespace ReceiverEndpoints.Handlers
{
    public class OrderSubmittedHandler : IHandleMessages<OrderSubmitted>
    {
        static readonly Random ChaosGenerator = new Random();

        public IBus Bus { get; set; }
        public ISession Session { get; set; }
        public NHibernateStorageContext StorageContext { get; set; }

        public OrderSubmittedHandler(IBus bus, NHibernateStorageContext storageContext)
        {
            this.Bus = bus;
            this.StorageContext = storageContext;
        }

        public void Handle(OrderSubmitted message)
        {
            Console.WriteLine("Order {0} worth {1} submitted", message.OrderId, message.Value);

            #region StoreUserData    

            //Session.Save(new Model.Order
            //{
            //    OrderId = message.OrderId,
            //    Value = message.Value
            //});

            using (OrderContext ctx = new OrderContext(StorageContext.Connection))
            {
                ctx.Database.UseTransaction((DbTransaction)StorageContext.DatabaseTransaction);
                var repo = new OrderRepository(ctx);
                repo.InsertOrder(new Model.Order
                {
                    OrderId = message.OrderId,
                    Value = message.Value
                });
            }

            #endregion

            #region Reply

            Bus.Reply(new OrderAccepted
            {
                OrderId = message.OrderId,
            });

            #endregion

           
            if (message.ThrowTransportException) throw new Exception("Blow up!");
            Console.WriteLine("Finished Submit Handler");

            //if (ChaosGenerator.Next(2) == 0)
            //{
            //    throw new Exception("Boom!");
            //}

        }
    }
}
