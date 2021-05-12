﻿using ECommerceProject.Models.DbModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerceProject.Models.DbModels
{
    public class ShoppingCard
    {
        public ShoppingCard()
        {
            Count = 1;
        }
        
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        [Range(1, 1000, ErrorMessage ="Lütfen geçerli bir değer giriniz..")]
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
