using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class CartController : Controller
    {
        ShoppingEntities db = new ShoppingEntities();

        private string NormalizeColor(string color)
        {
            return string.IsNullOrWhiteSpace(color) ? "" : color.Trim();
        }

        [HttpPost]
        public JsonResult AddItem(int ProductID, string Color = null, int Quantity = 1)
        {
            Product product = db.Products.SingleOrDefault(x => x.ID == ProductID);
            if (product == null)
            {
                return Json(new { status = false, message = "Sản phẩm không tồn tại." }, JsonRequestBehavior.AllowGet);
            }

            if (product.Quantity < Quantity)
            {
                return Json(new { status = false, message = "Số lượng không đủ! Chỉ còn " + product.Quantity + " sản phẩm." }, JsonRequestBehavior.AllowGet);
            }

            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<ItemCart>();
            }
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;

            string safeColor = NormalizeColor(Color);

            // Kiểm tra sản phẩm cùng màu đã tồn tại trong giỏ chưa
            ItemCart check = itemCarts.FirstOrDefault(x => x.ProductID == ProductID && NormalizeColor(x.Color) == safeColor);

            if (check != null)
            {
                if (product.Quantity < check.Quantity + Quantity)
                {
                    return Json(new { status = false, message = "Số lượng không đủ! Chỉ còn " + product.Quantity + " sản phẩm." }, JsonRequestBehavior.AllowGet);
                }

                check.Quantity += Quantity;
            }
            else
            {
                itemCarts.Add(new ItemCart()
                {
                    ProductID = product.ID,
                    ProductName = product.Name,
                    ProductPrice = product.Price.Value,
                    ProductImage = product.Image1,
                    Quantity = Quantity,
                    Color = Color
                });
            }

            Session["Cart"] = itemCarts;
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetTotalCart()
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            if (itemCarts == null || itemCarts.Count == 0)
            {
                return Json(new { TotalPrice = "0", TotalQuantity = 0 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                TotalPrice = itemCarts.Sum(x => x.ProductPrice * x.Quantity).ToString("#,##"),
                TotalQuantity = itemCarts.Sum(x => x.Quantity)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateQuantity(int ProductID, int Quantity, string Color = null)
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            if (itemCarts == null)
            {
                return Json(new { update = false }, JsonRequestBehavior.AllowGet);
            }

            if (Quantity > 0)
            {
                Product product = db.Products.SingleOrDefault(x => x.ID == ProductID);
                if (product == null)
                    return Json(new { update = false }, JsonRequestBehavior.AllowGet);

                if (product.Quantity < Quantity)
                {
                    return Json(new { update = false, message = "Số lượng không đủ" }, JsonRequestBehavior.AllowGet);
                }
            }

            string safeColor = NormalizeColor(Color);

            for (int i = 0; i < itemCarts.Count; i++)
            {
                if (itemCarts[i].ProductID == ProductID && NormalizeColor(itemCarts[i].Color) == safeColor)
                {
                    if (Quantity > 0)
                    {
                        itemCarts[i].Quantity = Quantity;
                        break;
                    }
                    else
                    {
                        itemCarts.RemoveAt(i);
                        break;
                    }
                }
            }

            Session["Cart"] = itemCarts;

            if (Quantity > 0)
            {
                return Json(new { update = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { remove = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSubTotal(int ProductID = 1, string Color = null)
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            if (itemCarts == null)
            {
                return Json(new { SubTotal = "0" }, JsonRequestBehavior.AllowGet);
            }
            string safeColor = NormalizeColor(Color);
            var subTotal = itemCarts
                .Where(x => x.ProductID == ProductID && NormalizeColor(x.Color) == safeColor)
                .Sum(x => x.ProductPrice * x.Quantity)
                .ToString("#,##");

            return Json(new { SubTotal = subTotal }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTotal()
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            if (itemCarts == null)
            {
                return Json(new { Total = "0" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Total = itemCarts.Sum(x => x.ProductPrice * x.Quantity).ToString("#,##") }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddOrder(string payment = "")
        {
            Models.User user = Session["User"] as Models.User;

            Models.Order order = new Models.Order
            {
                DateOrder = DateTime.Now,
                DateShip = DateTime.Now.AddDays(3),
                Status = "Processing",
                UserID = user.ID,
                IsPaid = false
            };
            db.Orders.Add(order);
            db.SaveChanges();

            int o = db.Orders.OrderByDescending(p => p.ID).FirstOrDefault().ID;
            Session["OrderId"] = o;

            List<ItemCart> listCart = Session["Cart"] as List<ItemCart>;
            foreach (ItemCart item in listCart)
            {
                OrderDetail orderDetail = new OrderDetail
                {
                    OrderID = order.ID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    ProductPrice = item.ProductPrice,
                    ProductName = item.ProductName,
                    ProductImage = item.ProductImage,
                    Color = item.Color
                };
                db.OrderDetails.Add(orderDetail);
            }
            db.SaveChanges();

            if (payment == "momo")
            {
                return RedirectToAction("PaymentWithMomo", "Payment");
            }

            SentMail(
                "Đặt hàng thành công",
                user.Email,
                "hoahuongduong05124@gmail.com",
                "ytotxwzbrwkoddjd",
                "<p style=\"font-size:20px\">Cảm ơn bạn đã đặt hàng<br/>Mã đơn hàng của bạn là: " + order.ID + "</p>"
            );

            Session.Remove("Cart");
            Session.Remove("OrderID");
            return RedirectToAction("Message", new { mess = "Đặt hàng thành công" });
        }

        public void SentMail(string Title, string ToEmail, string FromEmail, string Password, string Content)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(ToEmail);
            mail.From = new MailAddress(FromEmail);
            mail.Subject = Title;
            mail.Body = Content;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(FromEmail, Password);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

        public ActionResult Message(string mess)
        {
            ViewBag.Message = mess;
            return View();
        }
    }
}
