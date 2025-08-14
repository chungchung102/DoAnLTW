using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
namespace CosmeticsShop.Controllers
{
    public class HomeController : Controller
    {
        ShoppingEntities db = new ShoppingEntities();
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        public ActionResult Index()
        {
            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<ItemCart>();
            }
            ViewBag.ListProduct = db.Products.Where(x => x.IsActive == true && x.PurchasedCount > 0).OrderByDescending(x => x.PurchasedCount).ToList();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "Giới thiệu";
            return View();
        }

        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(Models.User user)
        {
            Models.User check = db.Users.SingleOrDefault(x => x.Email == user.Email);
            if (check != null)
            {
                ViewBag.Message = "Email đã tồn tại";
                return View();
            }

            Models.User userAdded = new Models.User();
            try
            {
                // Hash the password before saving
                user.Password = HashPassword(user.Password);
                user.Captcha = new Random().Next(100000, 999999).ToString();
                user.IsConfirm = false;
                user.UserTypeID = 2;
                user.Address = "pr.jpg";
                userAdded = db.Users.Add(user);
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                ViewBag.Message = "Đăng ký thất bại: " + string.Join("; ", errorMessages);
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Đăng ký thất bại: " + ex.Message;
                return View();
            }
            return RedirectToAction("ConfirmEmail", "User", new { ID = userAdded.ID });
        }
        public ActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignIn(string Email, string Password)
        {
            // Hash the input password before checking
            string hashedPassword = HashPassword(Password);
            Models.User check = db.Users.SingleOrDefault(x => x.Email == Email && x.Password == hashedPassword);
            if (check != null)
            {
                Session["User"] = check;
                if (check.UserType != null && check.UserType.Name == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "Email hoặc mật khẩu không đúng";
            return View();
        }

        public ActionResult SignOut()
        {
            Session.Remove("User");
            return RedirectToAction("Index");
        }
    }
}
