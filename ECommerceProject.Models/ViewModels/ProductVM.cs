using System;
using System.Collections.Generic;
using System.Text;
using ECommerceProject.Models.DbModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceProject.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> CoverTypeList { get; set; }
    }
}
