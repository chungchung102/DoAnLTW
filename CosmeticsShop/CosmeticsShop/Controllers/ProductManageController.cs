using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class ProductManageController : Controller
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

        // GET: ProductManage
        public ActionResult Index(string keyword = "")
        {
            if (!CheckRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            List<Product> products = new List<Product>();
            if (keyword != "")
            {
                products = db.Products.Where(x => x.Name.Contains(keyword)).ToList();
            }
            else
            {
                products = db.Products.Where(x => x.Name.Contains(keyword)).ToList();
            }
            return View(products);
        }

        public ActionResult ToggleActive(int ID)
        {
            Product product = db.Products.Find(ID);
            product.IsActive = !product.IsActive;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Details(int ID)
        {
            if (!CheckRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            Product product = db.Products.Find(ID);
            ViewBag.CategoryList = db.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        public ActionResult Edit(Product product, HttpPostedFileBase[] ImageUpload)
        {
            if (!CheckRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            Product productUpdate = db.Products.Find(product.ID);
            if (productUpdate == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm để chỉnh sửa.";
                return RedirectToAction("Index");
            }

            productUpdate.Name = product.Name;
            productUpdate.Price = product.Price;
            productUpdate.Quantity = product.Quantity;
            productUpdate.CategoryID = product.CategoryID;
            productUpdate.Description = product.Description;

            if (ImageUpload != null)
            {
                for (int i = 0; i < ImageUpload.Length; i++)
                {
                    if (ImageUpload[i] != null && ImageUpload[i].ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(ImageUpload[i].FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                        if (!System.IO.File.Exists(path))
                        {
                            ImageUpload[i].SaveAs(path);
                        }
                    }
                }

                if (ImageUpload.Length > 0 && ImageUpload[0] != null)
                    productUpdate.Image1 = ImageUpload[0].FileName;
                if (ImageUpload.Length > 1 && ImageUpload[1] != null)
                    productUpdate.Image2 = ImageUpload[1].FileName;
                if (ImageUpload.Length > 2 && ImageUpload[2] != null)
                    productUpdate.Image3 = ImageUpload[2].FileName;
            }

            db.SaveChanges();

            ViewBag.CategoryList = db.Categories.ToList();
            ViewBag.Message = "Cập nhật thành công";
            return View("Details", productUpdate);
        }

        public ActionResult Edit()
        {
            return RedirectToAction("Index");
        }

        public ActionResult Add()
        {
            if (!CheckRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.CategoryList = db.Categories.Where(x => x.IsActive == true).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Add(Product product, HttpPostedFileBase[] ImageUpload)
        {
            if (ImageUpload != null)
            {
                for (int i = 0; i < ImageUpload.Length; i++)
                {
                    if (ImageUpload[i] != null && ImageUpload[i].ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(ImageUpload[i].FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                        if (!System.IO.File.Exists(path))
                        {
                            ImageUpload[i].SaveAs(path);
                        }

                        // Gán ảnh cho product tương ứng
                        if (i == 0) product.Image1 = fileName;
                        else if (i == 1) product.Image2 = fileName;
                        else if (i == 2) product.Image3 = fileName;
                    }
                }
            }
            product.CreatedBy = (Session["User"] as Models.User).ID;
            product.ViewCount = 0;
            product.PurchasedCount = 0;
            product.IsActive = true;

            db.Products.Add(product);
            db.SaveChanges();

            TempData["Message"] = "Thêm sản phẩm mới thành công!";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int ID)
        {
            if (!CheckRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var product = db.Products.Find(ID);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm để xóa.";
                return RedirectToAction("Index");
            }

            try
            {
                db.Products.Remove(product);
                db.SaveChanges();

                // Lấy ID lớn nhất còn lại trong bảng, nếu không có thì 0
                int maxId = 0;
                if (db.Products.Any())
                {
                    maxId = db.Products.Max(p => p.ID);
                }

                // Reset giá trị identity để ID mới thêm sẽ là maxId + 1
                var sql = $"DBCC CHECKIDENT ('Products', RESEED, {maxId})";
                db.Database.ExecuteSqlCommand(sql);

                TempData["Message"] = $"Đã xóa sản phẩm {product.Name} thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}
