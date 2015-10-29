using ReceiverEndpoints.Model;

namespace ReceiverEndpoints.Repository
{
    class OrderRepository
    {
        OrderContext _context;

        public OrderRepository(OrderContext context)
        {
            _context = context;
        }

        public void InsertOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }
    }
}