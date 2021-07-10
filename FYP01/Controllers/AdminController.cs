using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FYP01.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace FYP01.Controllers
{
    public class AdminController : Controller
    {

        public IActionResult AddProducts()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public IActionResult AddProducts(Product product, IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("AddProducts");
            }
            else
            {
                string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                string picfilename = DoPhotoUpload(product.Photo);

                string sql = @"INSERT INTO Product (ProductName, Price, Picture)
                          VALUES('{0}', '{1}', '{2}')";

                string insert = String.Format(sql, product.ProductName.EscQuote(), product.Price, picfilename);

                if (DBUtl.ExecSQL(insert) == 1)
                {
                    TempData["Message"] = "Trip Successfully Added.";
                    TempData["MsgType"] = "success";
                    return RedirectToAction("AddProducts");
                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                    return View("AddProducts");
                }
            }
        }

        private string DoPhotoUpload(IFormFile photo)
        {
            string fext = Path.GetExtension(photo.FileName);
            string uname = Guid.NewGuid().ToString();
            string fname = uname + fext;
            string fullpath = Path.Combine(_env.WebRootPath, "photos/" + fname);
            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                photo.CopyTo(fs);
            }
            return fname;
        }

        private IWebHostEnvironment _env;
        public AdminController(IWebHostEnvironment environment)
        {
            _env = environment;
        }
    }
}
