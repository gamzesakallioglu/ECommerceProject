using ECommerceProject.Data;
using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommerceProject.DataAccess.MainRepository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company company)
        {
            _db.Update(company);


        }
    }
}
