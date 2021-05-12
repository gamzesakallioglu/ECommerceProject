using ECommerceProject.Data;
using ECommerceProject.DataAccess.IMainRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceProject.DataAccess.MainRepository
{
    public class UnitofWork : IUnitofWork
    {
        private readonly ApplicationDbContext _db;
        public UnitofWork(ApplicationDbContext db)
        {
            _db = db;
            category = new CategoryRepository(_db);
            sp_call = new SPCallRepository(_db);
            cover_type = new CoverTypeRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            ShoppingCard = new ShoppingCardRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
        }

        public ICategoryRepository category { get; private set; }
        public ISPCallRepository sp_call { get; private set; }
        public ICoverTypeRepository cover_type { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IShoppingCardRepository ShoppingCard { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
