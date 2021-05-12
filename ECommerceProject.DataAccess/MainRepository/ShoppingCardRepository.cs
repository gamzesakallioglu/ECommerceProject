using ECommerceProject.Data;
using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommerceProject.DataAccess.MainRepository
{
    public class ShoppingCardRepository : Repository<ShoppingCard>, IShoppingCardRepository
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCardRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCard shoppingCard)
        {
            _db.Update(shoppingCard);
          
        }
    }
}
