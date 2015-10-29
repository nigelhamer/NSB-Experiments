using NHibernate;
using NServiceBus;
using NServiceBus.Persistence.NHibernate;
using ReceiverEndpoints.Repository;
using Shared;
using System;
using System.Data.Common;
using System.Transactions;

namespace ReceiverEndpoints.Handlers
{
    public class OrderSubmittedHandler : IHandleMessages<OrderSubmitted>
    {
        public IBus Bus { get; set; }
        public ISession Session { get; set; }
        public NHibernateStorageContext StorageContext { get; set; }
        public bool IsTransactionEnabled { get; set; }

        public OrderSubmittedHandler(IBus bus, NHibernateStorageContext storageContext)
        {
            this.Bus = bus;
            this.StorageContext = storageContext;

            // Extract this transaction here for reuse as when transactions are disabled the call to storageContext.DatabaseTransaction
            this.IsTransactionEnabled = CheckTransactionEnabled(storageContext);
        }

        private bool CheckTransactionEnabled(NHibernateStorageContext storageContext)
        {
            // When transaction are disabled DatabaseTransaction throws an exception
            // Therefore this helper hide this.
            try
            {
                var tx = storageContext.DatabaseTransaction;
                return true;
            }
            catch (Exception)
            {
                return false;
            }            
        }

        public void Handle(OrderSubmitted message)
        {
            Console.WriteLine("Order {0} worth {1} submitted", message.OrderId, message.Value);

            #region StoreUserData    

            // This is how you share the NServiceBus Transaction scope with NHibernate code.
            //Session.Save(new Model.Order
            //{
            //    OrderId = message.OrderId,
            //    Value = message.Value
            //});


            if (IsTransactionEnabled)
            {
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
            }
            else
            {
                using (OrderContext ctx = new OrderContext())
                {
                    using (var tx = ctx.Database.BeginTransaction())
                    {
                        var repo = new OrderRepository(ctx);
                        repo.InsertOrder(new Model.Order
                        {
                            OrderId = message.OrderId,
                            Value = message.Value
                        });

                        if (message.ThrowDataException)
                        {
                            tx.Rollback();
                            Console.WriteLine("Simulate Data Exception");
                        }
                        else
                            tx.Commit();
                    }
                }
            }
            

            #endregion

            #region Reply

            Bus.Reply(new OrderAccepted
            {
                OrderId = message.OrderId,
            });

            #endregion

            if (message.ThrowTransportException)
            {
                Console.WriteLine("Simulate Transform Exception");
                throw new Exception("Blow up!");

            }
            Console.WriteLine("Finished Submit Handler");

        }
    }
}
