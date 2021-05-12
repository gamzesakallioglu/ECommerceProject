using ECommerceProject.Data;
using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommerceProject.DataAccess.MainRepository
{
    public class OrderDetailRepository : Repository<OrderDetails>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderDetailRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(OrderDetails orderDetails)
        {
            _db.Update(orderDetails);
          
        }
    }
}
