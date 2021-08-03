using System;
using System.Collections.Generic;
using FYP01.Models;
using System.ComponentModel.DataAnnotations;


namespace FYP01.Models
{
    public partial class Testimonial
    {
        public int TestID { get; set; }
        
        [Required(ErrorMessage = "Please enter customer name!")]
        public string TestName { get; set; }
        
        [Required(ErrorMessage = "Please enter testimonial!")]
        public string Testi { get; set; }
        public string ProductName { get; set; }

    }
}
