using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FYP01.Models
{
    public class OrderDetails
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public double? Price { get; set; }
        public int Quantity { get; set; }

    }
}
