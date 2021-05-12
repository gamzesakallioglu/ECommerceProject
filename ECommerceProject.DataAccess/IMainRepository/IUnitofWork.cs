using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceProject.DataAccess.IMainRepository
{
    public interface IUnitofWork : IDisposable
    {
        ICategoryRepository category { get; }
        ISPCallRepository sp_call { get; }
        ICoverTypeRepository cover_type { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IShoppingCardRepository ShoppingCard { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetail { get; }
        void Save();
    }
}
