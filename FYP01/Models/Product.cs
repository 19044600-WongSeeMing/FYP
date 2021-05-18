using System;
using System.Collections.Generic;
using FYP01.Models;


namespace FYP01.Models
{
    public partial class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double? Price { get; set; }
        public int? StockAvailable { get; set; }
        public byte[] ProductPicture { get; set; }
        public int? CategoryId { get; set; }

        public virtual Category Category { get; set; }
    }
}
