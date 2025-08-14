using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class CategoryManageController : Controller
    {
        ShoppingEntities db = new ShoppingEntities();
        public bool CheckRole(string type)
        {
            Models.User user = Session["User"] as Models.User;
            if (user != null && user.UserType.Name == type)
            {
                return true;
            }
            return false;
        }
        public ActionResult Index(string keyword = "")
        {
            if (CheckRole("Admin"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
            List<Category> categories = new List<Category>();
            if (keyword != "")
            {
                categories = db.Categories.Where(x => x.Name.Contains(keyword)).ToList();
            }
            else
            {
                categories = db.Categories.Where(x => x.Name.Contains(keyword)).ToList();
            }
            return View(categories);
        }
        public ActionResult ToggleActive(int ID)
        {
            if (!CheckRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            Category category = db.Categories.Find(ID);
            if (category == null)
            {
                TempData["Error"] = $"Không tìm thấy danh mục với ID = {ID}";
                return RedirectToAction("Index");
            }

            category.IsActive = !(category.IsActive ?? false);  // Xử lý null ở đây
            db.SaveChanges();

            TempData["Message"] = $"Đã thay đổi trạng thái danh mục {category.Name} thành {(category.IsActive.Value ? "Kích hoạt" : "Khóa")}";
            return RedirectToAction("Index");
        }


        public ActionResult Details(int ID)
        {
            if (CheckRole("Admin"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
            Category category = db.Categories.Find(ID);
            return View(category);
        }
        [HttpPost]
        public ActionResult Edit(Category category)
        {
            Category categoryUpdate = db.Categories.Find(category.ID);
            if (categoryUpdate == null)
            {
                TempData["Error"] = "Danh mục không tồn tại.";
                return RedirectToAction("Index");
            }
            categoryUpdate.Name = category.Name;
            db.SaveChanges();

            TempData["Message"] = "Cập nhật danh mục thành công";
            return RedirectToAction("Index"); // Chuyển về Index và hiển thị thông báo
        }

        public ActionResult Edit()
        {
            return RedirectToAction("Index");
        }
        public ActionResult Add()
        {
            if (CheckRole("Admin"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Add(Category category)
        {
            int maxID = 0;
            if (db.Categories.Any())
            {
                maxID = db.Categories.Max(c => c.ID);
            }
            category.ID = maxID + 1;
            category.IsActive = true;
            Category cate = db.Categories.Add(category);
            db.SaveChanges();
            ViewBag.Message = "Thêm thành công";
            return View("Details", cate);
        }

        public ActionResult Delete(int ID)
        {
            if (!CheckRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var category = db.Categories.Find(ID);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục để xóa.";
                return RedirectToAction("Index");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Xóa danh mục có ID = ID
                    db.Categories.Remove(category);
                    db.SaveChanges();

                    // Lấy các danh mục có ID lớn hơn ID vừa xóa
                    var categoriesToUpdate = db.Categories.Where(c => c.ID > ID).OrderBy(c => c.ID).ToList();

                    // Giảm ID từng danh mục đi 1
                    foreach (var item in categoriesToUpdate)
                    {
                        item.ID = item.ID - 1;
                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();

                    // Reset lại identity để tránh nhảy số
                    var maxId = db.Categories.Any() ? db.Categories.Max(c => c.ID) : 0;
                    db.Database.ExecuteSqlCommand($"DBCC CHECKIDENT ('Categories', RESEED, {maxId})");

                    transaction.Commit();

                    TempData["Message"] = $"Đã xóa danh mục {category.Name} và cập nhật ID thành công.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi khi xóa danh mục: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }


    }
}