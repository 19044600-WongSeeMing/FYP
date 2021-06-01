﻿using System;
using System.Collections.Generic;
using FYP01.Models;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace FYP01.Models
{
    public partial class MesahUser
    {
        [Required(ErrorMessage ="Please enter Username!")]
        public string UserId { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter Password!")]
        public String UserPw { get; set; }

        [Required(ErrorMessage = "Please enter Full Name!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter Email!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter Address!")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "Please enter Postal Code!")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Please enter Phone Number!")]
        [RegularExpression(@"[89]\d{7}", ErrorMessage ="Invalid phone number")]
        public string Phone { get; set; }
        public string UserRole { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
