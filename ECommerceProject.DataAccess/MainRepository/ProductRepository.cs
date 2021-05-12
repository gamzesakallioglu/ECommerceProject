using ECommerceProject.Data;
using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommerceProject.DataAccess.MainRepository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var data = _db.Products.FirstOrDefault(x => x.Id == product.Id);
            if (data != null)
            {
                if (product.ImageUrl != null)
                {
                    data.ImageUrl = product.ImageUrl;
                }
                data.Title = product.Title;
                data.Description = product.Description;
                data.ISBN = product.ISBN;
                data.Price = product.Price;
                data.Price50 = product.Price50;
                data.ListPrice = product.ListPrice;
                data.Brand = product.Brand;
                data.Price100 = product.Price100;
                data.CategoryId = product.CategoryId;
                data.CoverTypeId = product.CoverTypeId;
            }

        }
    }
}
