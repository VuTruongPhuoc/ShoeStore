using Microsoft.AspNetCore.Mvc;
using ShoeStore.Models;
using ShoeStore.Data;
using X.PagedList;
using ShoeStore.Models.Common;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/product")]
    [Route("admin/product/{action}")]
    [Route("admin/product/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class ProductController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        public ProductController(INotyfService notyf)
        {
            _notyf = notyf;
        }
        public IActionResult Index(string searchtext, int ?page)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList();           
            
            if (page == null)
            {
                page = 1;
            }
            ViewBag.searchtext = searchtext;
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(searchtext))
            {
                items = items.Where(x => x.Name.Contains(searchtext) || x.Code.Contains(searchtext) && x.Status);
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = 5;
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
                    await db.SaveChangesAsync();
					_notyf.Success("Thêm dữ liệu thành công");
					return RedirectToAction("index", "product", new { area = "admin" });
				}
				catch (Exception ex)
				{
					_notyf.Error("Có lỗi khi thêm dữ liệu " + ex.Message);
					return View(model);
				}
			}
			_notyf.Error("Có lỗi khi thêm dữ liệu");
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
                try
                {
                    item.Name = model.Name;
                    item.SupplierId = model.SupplierId;
                    item.CategoryId = model.CategoryId;
                    item.Description = model.Description;
                    item.Detail = model.Detail;
                    item.UpdateAt = DateTime.Now;               
                    item.Status = model.Status;
                    await db.SaveChangesAsync();
					_notyf.Success("Cập nhật dữ liệu thành công");
					return RedirectToAction("index", "product", new { area = "admin" });
				}
				catch (Exception ex)
				{
					_notyf.Error("Có lỗi khi cập nhật dữ liệu " + ex.Message);
					return View(model);
				}
			}
			_notyf.Error("Có lỗi khi cập nhật dữ liệu");
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
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.Products.FindAsync(id);
            if (item != null)
            {
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
      
    }
}
