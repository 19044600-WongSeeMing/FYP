using System;
using System.Collections.Generic;
using FYP01.Models;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace FYP01.Models
{
    public partial class MesahUser
    {
        [Required(ErrorMessage ="Please enter UserName!")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Please enter User Password!")]
        public byte[] UserPw { get; set; }

        [Required(ErrorMessage = "Please enter Full Name!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter Email!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter Address!")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "Please enter Postal Code!")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Please enter Phone Number!")]
        public string Phone { get; set; }
        public string UserRole { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
