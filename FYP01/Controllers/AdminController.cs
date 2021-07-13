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

        public IActionResult ListOfProducts()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM Product");
            return View("ListOfProducts", dt.Rows);
        }

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

        [Authorize(Roles = "manager")]
        public IActionResult DeleteProducts(int id)
        {
            string select = @"SELECT * FROM Product 
                              WHERE ProductId={0}";
            DataTable ds = DBUtl.GetTable(select, id);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Products record no longer exists.";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM Product WHERE ProductId={0}";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "Product Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("ListOfProducts");
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

        [Authorize(Roles ="manager")]
        public IActionResult ShowUsers()
        {
            List<MesahUser> list = DBUtl.GetList<MesahUser>("SELECT * FROM MesahUser");
            return View("ShowUsers", list);
        }

        [Authorize(Roles = "manager")]
        public IActionResult DeleteUser(string id)
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userid.Equals(id, StringComparison.InvariantCultureIgnoreCase))
            {
                TempData["Message"] = "Own ID cannot be deleted";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM MesahUser WHERE UserId='{0}'";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "User Record Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("ShowUsers");
        }
        [Authorize(Roles = "manager")]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public IActionResult CreateUser(MesahUser usr)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("CreateUser");
            }
            else
            {
                string insert =
                   @"INSERT INTO MesahUser(UserId, UserPw, FullName, Email, Address, PostalCode, Phone, UserRole) 
                        VALUES('{0}',HASHBYTES('SHA1','{1}'),'{2}','{3}','{4}','{5}','{6}','{7}')";
                if (DBUtl.ExecSQL(insert, usr.UserId, usr.UserPw, usr.FullName, usr.Email, usr.Address, usr.PostalCode, usr.Phone, usr.UserRole) == 1)
                {
                    ViewData["Message"] = "User Created";
                    ViewData["MsgType"] = "success";
                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                }
                return RedirectToAction("ShowUsers");
            }
        }

    }
}
