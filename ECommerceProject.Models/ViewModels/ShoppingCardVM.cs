using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceProject.Models.ViewModels
{
    public class ShoppingCardVM
    {
        public IEnumerable<ShoppingCard> ListCard { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
