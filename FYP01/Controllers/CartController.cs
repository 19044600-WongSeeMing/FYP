using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FYP01.Controllers
{
    public class CartController : Controller
    {
        public IActionResult ShopCart()
        {
            return View();
        }
    }
}
