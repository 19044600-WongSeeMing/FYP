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

namespace FYP01.Controllers
{
    public class AccountController : Controller
    {
        private const string LOGIN_SQL =
           @"SELECT * FROM MesahUser 
            WHERE UserId = '{0}' 
              AND UserPw = HASHBYTES('SHA1', '{1}')";

        private const string LASTLOGIN_SQL =
           @"UPDATE MesahUser SET LastLogin=GETDATE() WHERE UserId='{0}'";

        private const string ROLE_COL = "UserRole";
        private const string NAME_COL = "FullName";

        private const string REDIRECT_CNTR = "Performance";
        private const string REDIRECT_ACTN = "Index";

        private const string LOGIN_VIEW = "Login";

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View(LOGIN_VIEW);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(MesahUser user)
        {
            if (!AuthenticateUser(user.UserId, user.UserPw, out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Incorrect User ID or Password";
                ViewData["MsgType"] = "warning";
                return View(LOGIN_VIEW);
            }
            else
            {
                HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   principal);

                // Update the Last Login Timestamp of the User
                DBUtl.ExecSQL(LASTLOGIN_SQL, user.UserId);

                if (TempData["returnUrl"] != null)
                {
                    string returnUrl = TempData["returnUrl"].ToString();
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                }

                return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
            }
        }

        private bool AuthenticateUser(string userId, byte[] userPw, out ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        public IActionResult Logoff(string returnUrl = null)
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }

        [Authorize(Roles = "manager")]
        public IActionResult Users()
        {
            List<MesahUser> list = DBUtl.GetList<MesahUser>("SELECT * FROM MesahUser WHERE UserRole='member' ");
            return View(list);
        }

        [Authorize(Roles = "manager")]
        public IActionResult Delete(string id)
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

            return RedirectToAction("Users");
        }

        [AllowAnonymous]
        public IActionResult SignUp()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Signup(MesahUser usr)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("UserRegister");
            }
            else
            {
                // TODO: L10 Task 2a - Provide the SQL statement to register new member"
                string insert =
                   @"INSERT INTO MesahUser(UserId, UserPw, FullName, Email, Address, PostalCode, Phone, UserRole) 
                        VALUES('{0}',HASHBYTES('SHA1','{1}'),'{2}','{3}','{4}','{5}','{6}','member')";
                if (DBUtl.ExecSQL(insert, usr.UserId, usr.UserPw, usr.FullName, usr.Email,usr.Address,usr.PostalCode,usr.Phone) == 1)
                {
                    string template = @"Hi {0},<br/><br/>
                               Welcome to Mesah Delicacies!
                               Your userid is <b>{1}</b> and password is <b>{2}</b>.
                               <br/><br/>Manager";
             //       string title = "Registration Successul - Welcome";
                    string message = String.Format(template, usr.FullName, usr.UserId, usr.UserPw);
                  //  string result = "";
                    
            //        bool outcome = false;
                    // TODO: L10 Task 2b - Call EmailUtl.SendEmail to send email
                    //                     Uncomment the following line with you are done
           //         outcome = EmailUtl.SendEmail(usr.Email, title, message, out result);
          /*          if (outcome)
                    {
                        ViewData["Message"] = "User Successfully Registered";
                        ViewData["MsgType"] = "success";
                    }
                    else
                    {
                        ViewData["Message"] = result;
                        ViewData["MsgType"] = "warning";
                    }*/
                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                }
                return View("UserRegister");
            }
        }

        [AllowAnonymous]
        public IActionResult VerifyUserID(string userId)
        {
            string select = $"SELECT * FROM MesahUser WHERE UserId='{userId}'";
            if (DBUtl.GetTable(select).Rows.Count > 0)
            {
                return Json($"[{userId}] already in use");
            }
            return Json(true);
        }

        private bool AuthenticateUser(string uid, string pw, out ClaimsPrincipal principal)
        {
            principal = null;

            DataTable ds = DBUtl.GetTable(LOGIN_SQL, uid, pw);
            if (ds.Rows.Count == 1)
            {
                principal =
                   new ClaimsPrincipal(
                      new ClaimsIdentity(
                         new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, uid),
                        new Claim(ClaimTypes.Name, ds.Rows[0][NAME_COL].ToString()),
                        new Claim(ClaimTypes.Role, ds.Rows[0][ROLE_COL].ToString())
                         }, "Basic"
                      )
                   );
                return true;
            }
            return false;
        }

    }
}