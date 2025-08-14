using System;
using System.Linq;
using System.Web.Mvc;
using CosmeticsShop.Models;

namespace CosmeticsShop.Controllers
{
    public class ChatController : Controller
    {
        private ShoppingEntities db = new ShoppingEntities();

        // Trang danh sách chat
        public ActionResult Index()
        {
            var messages = db.Messages
                .GroupBy(m => m.FromUserID)
                .Select(g => g.OrderByDescending(m => m.CreatedDate).FirstOrDefault())
                .ToList();

            return View(messages);
        }

        // Trang chatting
        public ActionResult Chating(int WithUserID)
        {
            var currentUser = Session["User"] as User;
            if (currentUser == null) return RedirectToAction("Login", "Account");

            ViewBag.WithUserID = WithUserID;
            ViewBag.CurrentUserID = currentUser.ID;

            return View();
        }

        // API lấy toàn bộ tin nhắn giữa 2 người
        public JsonResult GetMessages(int userId)
        {
            var currentUser = Session["User"] as User;
            var messages = db.Messages
                .Where(m => (m.FromUserID == currentUser.ID && m.ToUserID == userId) ||
                            (m.FromUserID == userId && m.ToUserID == currentUser.ID))
                .OrderBy(m => m.CreatedDate)
                .Select(m => new
                {
                    m.FromUserID,
                    m.ToUserID,
                    m.Content,
                    CreatedDate = m.CreatedDate.Value.ToString("HH:mm"),
                    FromUserName = m.User.Name,
                    FromUserAvatar = m.User.Avatar
                })
                .ToList();

            return Json(messages, JsonRequestBehavior.AllowGet);
        }
    }
}
