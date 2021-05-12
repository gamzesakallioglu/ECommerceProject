using ECommerceProject.DataAccess.IRepository;
using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceProject.DataAccess.IMainRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetails>
    {
        public void Update(OrderDetails orderDetails);
    }
}
