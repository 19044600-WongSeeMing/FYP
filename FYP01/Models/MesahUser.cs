using System;
using System.Collections.Generic;
using FYP01.Models;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;


namespace FYP01.Models
{
    public partial class MesahUser
    {
        [Required(ErrorMessage ="Please enter Username!")]
        [Remote(action: "VerifyUserID", controller: "Account")]
        public string UserId { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter Password!")]
        [StringLength(maximumLength:12,ErrorMessage ="Minimum 5 characters, Maximum 12 characters",MinimumLength =5)]
        public string UserPw { get; set; }

        [Required(ErrorMessage = "Please enter Full Name!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter Email!")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage ="Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter Address!")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "Please enter Postal Code!")]
        [StringLength(maximumLength:7,ErrorMessage ="Please enter correct postal code format")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Please enter Phone Number!")]
        [RegularExpression(@"[89]\d{7}", ErrorMessage ="Invalid phone number")]
        public string Phone { get; set; }
        public string UserRole { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
