using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/supplier")]
    [Route("admin/supplier/{action}")]
    [Route("admin/supplier/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class SupplierController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;

        public SupplierController(INotyfService notyf)
        {
            _notyf = notyf;
        }
        public async Task<IActionResult> Index(string Searchtext, int? page)
        {
            var pageSize = 5;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Supplier> items = db.Suppliers.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Name.Contains(Searchtext) || x.PhoneNumber.Contains(Searchtext));
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
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Supplier model)
        {
			var checkname = await db.Suppliers.FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower());
			// Kiểm tra username đã tồn tại hay chưa
			if (checkname != null)
			{
				_notyf.Warning("Tên nhà cung cấp này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (ModelState.IsValid)
            {
                try
                {
                    db.Suppliers.Add(model);
                    await db.SaveChangesAsync();
					_notyf.Success("Thêm dữ liệu thành công");
					return RedirectToAction("index", "supplier", new { area = "admin" });
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
        public IActionResult Edit(int id)
        {
            var item = db.Suppliers.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier model)
        {
            var item = await db.Suppliers.FindAsync(model.Id);
			var checkname = await db.Sizes.FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower() && c.Id != model.Id);
			// Kiểm tra username đã tồn tại hay chưa
			if (checkname != null)
			{
				_notyf.Warning("Tên nhà cung cấp này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (ModelState.IsValid && item is not null)
            {
                try
                {
                    item.Name = model.Name;
                    item.PhoneNumber = model.PhoneNumber;
                    item.Address = model.Address;
                    await db.SaveChangesAsync();
					_notyf.Success("Cập nhật dữ liệu thành công");
					return RedirectToAction("index", "supplier", new { area = "admin" });
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

            var item = await db.Suppliers.FindAsync(id);
            if (item != null)
            {
                db.Suppliers.Remove(item);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
    }
}
