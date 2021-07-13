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
        private const string NAME_COL = "UserId";

        private const string REDIRECT_CNTR = "Home";
        private const string REDIRECT_ACTN = "Index";

        private const string LOGIN_VIEW = "Login";

        private AppDbContext _dbContext;

        public AccountController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View(LOGIN_VIEW);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(UserLogin user)
        {
            if (!AuthenticateUser(user.UserID, user.Password, out ClaimsPrincipal principal))
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
                DBUtl.ExecSQL(LASTLOGIN_SQL, user.UserID);

                if (TempData["returnUrl"] != null)
                {
                    string returnUrl = TempData["returnUrl"].ToString();
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                }
                return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
            }
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
                return View("SignUp");
            }
            else
            {
                string insert =
                   @"INSERT INTO MesahUser(UserId, UserPw, FullName, Email, Address, PostalCode, Phone, UserRole) 
                        VALUES('{0}',HASHBYTES('SHA1','{1}'),'{2}','{3}','{4}','{5}','{6}','member')";
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
                return View("Login");
            }
        }

        [AllowAnonymous]
        public IActionResult VerifyUserID(string UserId)
        {
            string select = $"SELECT * FROM MesahUser WHERE UserId='{UserId}'";
            if (DBUtl.GetTable(select).Rows.Count > 0)
            {
                return Json($"[{UserId}] already in use");
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

        [Authorize]
        public IActionResult EditProfile()
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string select = @"SELECT * FROM MesahUser WHERE UserId = '{0}'";
            List<MesahUser> list = DBUtl.GetList<MesahUser>(select, userid);
            if (list.Count == 1)
            {
                return View(list[0]);
            }
            else
            {
                TempData["Message"] = "Data not found";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
        }
        [Authorize]
        [HttpPost]
        public IActionResult EditProfile(MesahUser mesah)
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string sql = @"UPDATE MesahUser
                                    SET FullName ='{1}', Email ='{2}',
                                  Address = '{3}', PostalCode = '{4}', Phone ='{5}'
                            WHERE UserId = '{0}'";

            if (DBUtl.ExecSQL(sql, userid, mesah.FullName, mesah.Email, mesah.Address, mesah.PostalCode, mesah.Phone) == 1)
            {
                ViewData["Message"] = "Profile Updated";
                ViewData["MsgType"] = "success";
            }
            else
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "danger";
            }
            return View("EditProfile");
        }
        // member & manager change password
        [Authorize]
        public JsonResult VerifyCurrentPassword(string CurrentPassword)
        {
            DbSet<MesahUser> dbs = _dbContext.MesahUser;
            var userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var pw_bytes = System.Text.Encoding.ASCII.GetBytes(CurrentPassword);
            MesahUser user = dbs.FromSqlInterpolated($"SELECT * FROM MesahUser WHERE UserId = {userid} AND UserPw= HASHBYTES('SHA1', {pw_bytes})").FirstOrDefault();

            if (user != null)
                return Json(true);
            else
                return Json(false);
        }

        [Authorize]
        public JsonResult VerifyNewPassword(string NewPassword)
        {

            DbSet<MesahUser> dbs = _dbContext.MesahUser;
            var userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var npw_bytes = System.Text.Encoding.ASCII.GetBytes(NewPassword);

            MesahUser user = dbs.FromSqlInterpolated($"SELECT * FROM MesahUser WHERE UserId = {userid} AND UserPw = HASHBYTES('SHA1', {npw_bytes})").FirstOrDefault();

            if (user != null)
                return Json(false);
            else
                return Json(true);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(PasswordUpdate pw)
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var npw_bytes = System.Text.Encoding.ASCII.GetBytes(pw.NewPassword);
            var cpw_bytes = System.Text.Encoding.ASCII.GetBytes(pw.CurrentPassword);

            if (_dbContext.Database.ExecuteSqlInterpolated($"UPDATE MesahUser SET UserPw = HASHBYTES('SHA1', {npw_bytes}) WHERE UserId={userid} AND UserPw = HASHBYTES('SHA1', {cpw_bytes})") == 1)

                ViewData["Msg"] = "Password Successfully Updated!";
            else

                ViewData["Msg"] = "Failed to update password!";

            return View();
        }
        //member & manager change username
        public IActionResult ChangeUsername()
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewData["userid"] = userid;
            DbSet<MesahUser> dbs = _dbContext.MesahUser;
            MesahUser user = dbs.FromSqlInterpolated($"SELECT * FROM MesahUser WHERE UserId = {userid}").FirstOrDefault();
            ViewData["Username"] = user.UserId;
            return View();
        }

        [HttpPost]
        public IActionResult ChangeUsername(UserUpdate userUpdate)
        {
            var userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            int num_affected = _dbContext.Database.ExecuteSqlInterpolated($"UPDATE MesahUser SET UserId = {userUpdate.NewUsername} WHERE UserId = {userid}");

            if (num_affected == 1)
            {
                return RedirectToAction("Login");
            }
            else
            {
                ViewData["Msg"] = "Failed to update username!";
                return View();
            }
        }

        [Authorize]
        public JsonResult VerifyNewUsername(string NewUserName)
        {
            DbSet<MesahUser> dbs = _dbContext.MesahUser;
            var userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            MesahUser user = dbs.FromSqlInterpolated($"SELECT * FROM MesahUser WHERE UserId = {NewUserName}").FirstOrDefault();

            if (user != null)
                return Json(false);
            else
                return Json(true);
        }



        // member & manager forgot password
        [AllowAnonymous]
        public JsonResult VerifyEmail(string Email)
        {
            DbSet<MesahUser> dbs = _dbContext.MesahUser;

            MesahUser user = dbs.FromSqlInterpolated($"SELECT * FROM MesahUser WHERE Email= {Email}").FirstOrDefault();

            if (user != null)
                return Json(true);
            else
                return Json(false);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(PasswordUpdate pw)
        {
            var email = pw.Email;
            var npw_bytes = System.Text.Encoding.ASCII.GetBytes(pw.ForgotPassword);

            if (_dbContext.Database.ExecuteSqlInterpolated($"UPDATE MesahUser SET UserPw = HASHBYTES('SHA1', {npw_bytes}) WHERE Email ={email}") == 1)

                ViewData["Msg"] = "Password Successfully Updated!";
            else

                ViewData["Msg"] = "Failed to update password!";

            return View();
        }

    }
}