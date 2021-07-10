using System;
using System.Collections.Generic;
using FYP01.Models;
using Microsoft.AspNetCore.Http;


namespace FYP01.Models
{
    public partial class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }

        public string Picture { get; set; }

        public IFormFile Photo { get; set; }

        public virtual Category Category { get; set; }
    }
}
