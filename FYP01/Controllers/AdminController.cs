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

                string picfilename = DoPhotoUpload(product.Photo);

                string sql = @"INSERT INTO Product (ProductName, Price, Picture)
                          VALUES('{0}', '{1}', '{2}')";

                string insert = String.Format(sql, product.ProductName.EscQuote(), product.Price, picfilename);

                if (DBUtl.ExecSQL(insert) == 1)
                {
                    TempData["Message"] = "Product Successfully Added.";
                    TempData["MsgType"] = "success";
                    return RedirectToAction("ListOfProducts");
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
            string sql = @"SELECT * FROM Product 
                              WHERE ProductID={0}";

            string select = String.Format(sql, id);

            DataTable ds = DBUtl.GetTable(select);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Product record no longer exists.";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string photoFile = ds.Rows[0]["picture"].ToString();
                string fullpath = Path.Combine(_env.WebRootPath, "FoodPics/" + photoFile);
                System.IO.File.Delete(fullpath);

                string delete = @"DELETE FROM Product WHERE ProductID={0}";
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

        [Authorize]
        public IActionResult ProductEdit(String id)
        {
            string sql = "SELECT * FROM Product WHERE ProductId={0}";
            string select = String.Format(sql, id);
            DataTable dt = DBUtl.GetTable(select);
            if (dt.Rows.Count == 1)
            {
                Product product = new Product
                {
                    ProductId = (int)dt.Rows[0]["ProductId"],
                    ProductName = dt.Rows[0]["ProductName"].ToString(),
                    Price = (double)dt.Rows[0]["Price"],
                    Photo = (IFormFile)dt.Rows[0]["Photo"],
                };
                return View(product);
            }
            else
            {
                TempData["Message"] = "Product Not Found";
                TempData["MsgType"] = "warning";
                return RedirectToAction("ListOfProducts");
            }
        }


        public IActionResult ProductEditPost(String id)
        {
            IFormCollection form = HttpContext.Request.Form;
            string ProductId = form["ProductID"].ToString().Trim();
            string ProductName = form["ProductName"].ToString().Trim();
            string Price = form["Price"].ToString().Trim();
            string Photo = form["Photo"].ToString().Trim();


            string sql = @"UPDATE Product
                           SET ProductName = '{1}',
                               Price   = {2},
                               Photo ={3}
                         WHERE ProductID = {0}";

            string update = String.Format(sql, ProductId, ProductName, Price, Photo);
            int res = DBUtl.ExecSQL(update);
            if (res == 1)
            {
                TempData["Message"] = "Product Updated";
                TempData["MsgType"] = "success";
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
            }
            return RedirectToAction("ListOfProducts");
        }

        private string DoPhotoUpload(IFormFile photo)
        {
            string fext = Path.GetExtension(photo.FileName);
            string uname = Guid.NewGuid().ToString();
            string fname = uname + fext;
            string fullpath = Path.Combine(_env.WebRootPath, "FoodPics/" + fname);
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

        [Authorize(Roles = "manager")]
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
        [Authorize(Roles = "manager")]
        public IActionResult EditUser(string id)
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string select = @"SELECT * FROM MesahUser WHERE UserId = '{0}'";
            List<MesahUser> list = DBUtl.GetList<MesahUser>(select, id);
            if (list.Count == 1)
            {
                MesahUser user = list[0];
                return View("EditUser",user);
            }
            else
            {
                TempData["Message"] = "Data not found";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public IActionResult EditUser(string id, MesahUser mesah)
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string sql = @"UPDATE MesahUser
                                    SET FullName ='{1}', UserRole ='{2}',
                                  Email = '{3}', Phone ='{4}'
                            WHERE UserId = '{0}'";

            if (DBUtl.ExecSQL(sql, id, mesah.FullName, mesah.UserRole,mesah.Email, mesah.Phone) == 1)
            {
                ViewData["Message"] = "Profile Updated";
                ViewData["MsgType"] = "success";
            }
            else
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "danger";
            }
            return RedirectToAction("ShowUsers");
        }

        public IActionResult TestimonialList()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM Testimonial");
            return View("TestimonialList", dt.Rows);
        }

        public IActionResult AddTestimonial()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public IActionResult AddTestimonial(Testimonial testimo)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("AddProducts");
            }
            else
            {
                string sql = @"INSERT INTO Testimonial (TestName, ProductName, Testi)
                          VALUES('{0}', '{1}', '{2}')";

                string insert = String.Format(sql, testimo.TestName.EscQuote(), testimo.ProductName.EscQuote(), testimo.Testi.EscQuote());

                if (DBUtl.ExecSQL(insert) == 1)
                {
                    TempData["Message"] = "Testimonial Successfully Added.";
                    TempData["MsgType"] = "success";
                    return RedirectToAction("TestimonialList");
                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                    return View("AddTestimonial");
                }
            }
        }

        [Authorize(Roles = "manager")]
        public IActionResult DeleteTestimonial(int id)
        {
            string sql = @"SELECT * FROM Testimonial 
                              WHERE TestID={0}";

            string select = String.Format(sql, id);

            DataTable ds = DBUtl.GetTable(select);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Product record no longer exists.";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = @"DELETE FROM Testimonial WHERE TestID={0}";
                int res = DBUtl.ExecSQL(delete, id);

                if (res == 1)
                {
                    TempData["Message"] = "Testimonial Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("TestminialList");
        }

       //Update Testimonial havent

    }
}
