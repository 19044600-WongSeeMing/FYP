using System;
using System.Collections.Generic;
using FYP01.Models;


namespace FYP01.Models
{
    public partial class MesahUser
    {
        public string UserId { get; set; }
        public byte[] UserPw { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string UserRole { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
