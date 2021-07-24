using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FYP01.Models;
using Microsoft.EntityFrameworkCore;

//add the required namespaces regarding EF Core and models

namespace FYP01.Controllers
{
    public class HomeController : Controller
    {
        //add dependency injection to this controller

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Menu()
        {
            List<Product> list = DBUtl.GetList<Product>("SELECT * FROM Product");
            return View("Menu", list);
        }

        public IActionResult Testi()
        {
            List<Testimonial> list = DBUtl.GetList<Testimonial>("SELECT * FROM Testimonial");
            return View("Testi", list);
        }

       

    }
}