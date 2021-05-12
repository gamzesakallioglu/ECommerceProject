﻿using ECommerceProject.Data;
using ECommerceProject.DataAccess.IMainRepository;
using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommerceProject.DataAccess.MainRepository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly ApplicationDbContext _db;

        public CoverTypeRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(CoverType coverType)
        {
            var data = _db.CoverTypes.FirstOrDefault(x => x.Id == coverType.Id);
            if (data != null)
            {
                data.Name = coverType.Name;
            }
          
        }
    }
}
