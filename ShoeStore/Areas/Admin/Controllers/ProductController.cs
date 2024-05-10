using Microsoft.AspNetCore.Mvc;
using ShoeStore.Models;
using ShoeStore.Data;
using X.PagedList;
using ShoeStore.Models.Common;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Product")]
    [Route("Admin/Product/{action}")]
    [Route("Admin/Product/{action}/{id}")]
    public class ProductController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        public IActionResult Index(string Searchtext, int ?page)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList();           
            var pageSize = 3;
            if (page == null)
            {
                page = 1;
            }
            ViewBag.Searchtext = Searchtext;
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Name.Contains(Searchtext) || x.Code.Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product model)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreateAt = DateTime.Now;
                    model.UpdateAt = DateTime.Now;
                    model.ViewCount = 0;
                    db.Products.Add(model);
                    db.SaveChangesAsync();
                    return RedirectToAction("Index", "Product", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ một cách thích hợp, có thể ghi log hoặc hiển thị thông báo lỗi
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu.");
                }
            }
            // Nếu ModelState không hợp lệ, quay lại view với dữ liệu và thông báo lỗi
            return View(model);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            var item = db.Products.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            var item = await db.Products.FindAsync(model.Id);

            if (ModelState.IsValid && item is not null)
            {
                //try
                //{
                item.Name = model.Name;
                item.SupplierId = model.SupplierId;
                item.CategoryId = model.CategoryId;
                item.Description = model.Description;
                item.Detail = model.Detail;
                item.UpdateAt = DateTime.Now;
                item.Status = model.Status;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Product", new { area = "Admin" });
                //}
                //catch (Exception ex)
                //{
                //    // Xử lý ngoại lệ một cách thích hợp, có thể ghi log hoặc hiển thị thông báo lỗi
                //    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu.");
                //}
            }
            // Nếu ModelState không hợp lệ hoặc xảy ra ngoại lệ, quay lại view với dữ liệu và thông báo lỗi
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            var item = await db.Products.FindAsync(id);
            if (item != null)
            {
                db.Products.Remove(item);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
        [HttpPost]
        public async Task<IActionResult> DeleteAll(string idstr)
        {
            if (!string.IsNullOrEmpty(idstr))
            {
                var items = idstr.Split(",");
                if (items != null && items.Any())
                {
                    foreach (var i in items)
                    {
                        var obj = await db.Products.FindAsync(Convert.ToInt32(i));
                        db.Products.Remove(obj);
                        await db.SaveChangesAsync();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.Products.FindAsync(id);
            if (item != null)
            {
                item.Status = !item.Status;
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
