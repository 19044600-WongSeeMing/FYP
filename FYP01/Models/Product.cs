using System;
using System.Collections.Generic;
using FYP01.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace FYP01.Models
{
    public partial class Product
    {
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Please enter product name!")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Please enter price!")]
        public double Price { get; set; }
        public int CategoryId { get; set; }

        public string Picture { get; set; }

        public IFormFile Photo { get; set; }

        public virtual Category Category { get; set; }
    }
}
