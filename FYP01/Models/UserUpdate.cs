using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FYP01.Models
{
    public class UserUpdate
    {
        public string CurrentUsername { get; set; }

        [Required(ErrorMessage = "Cannot be empty!")]
        [Remote(action: "VerifyUserID", controller: "Account")]
        public string NewUsername { get; set; }

        [Required(ErrorMessage = "Cannot be empty!")]
        [Compare("NewUsername", ErrorMessage = "Username not confirmed!")]
        public string ConfirmNewUsername { get; set; }
    }
}
