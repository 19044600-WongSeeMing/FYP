using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FYP01.Models
{
    public class PasswordUpdate
    {
        [Required(ErrorMessage = "Cannot be empty!")]
        [DataType(DataType.Password)]
        [Remote("VerifyCurrentPassword", "Account", ErrorMessage = "Incorrect password!")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Cannot be empty!")]
        [DataType(DataType.Password)]
        [Remote("VerifyNewPassword", "Account", ErrorMessage = "Cannot reuse password!")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Cannot be empty!")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password not confirmed!")]
        public string ConfirmPassword { get; set; }
    }
}
