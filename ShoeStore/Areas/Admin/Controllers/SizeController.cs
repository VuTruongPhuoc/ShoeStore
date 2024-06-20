using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/size")]
    [Route("admin/size/{action}")]
    [Route("admin/size/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class SizeController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        public SizeController(INotyfService notyf)
        {
            _notyf = notyf;
        }
        public async Task<IActionResult> Index(string Searchtext, int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Size> items = db.Sizes.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Name.Contains(Searchtext));
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
        public async Task<IActionResult> Add(Size model)
        {
			var checkname = await db.Sizes.FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower());
			// Kiểm tra username đã tồn tại hay chưa
			if (checkname != null)
			{
				_notyf.Warning("Tên size này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (ModelState.IsValid)
            {
                try
                {
                    db.Sizes.Add(model);
                    await db.SaveChangesAsync();
					_notyf.Success("Thêm dữ liệu thành công");
					return RedirectToAction("index", "size", new { area = "admin" });
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
            var item = db.Sizes.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Size model)
        {
            var item = await db.Sizes.FindAsync(model.Id);
			var checkname = await db.Sizes.FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower() && c.Id != model.Id);
			// Kiểm tra username đã tồn tại hay chưa
			if (checkname != null)
			{
				_notyf.Warning("Tên size này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (ModelState.IsValid && item is not null)
            {
                try
                {
                    item.Name = model.Name;
                    item.Status = model.Status;
                    await db.SaveChangesAsync();
					_notyf.Success("Cập nhật dữ liệu thành công");
					return RedirectToAction("index", "size", new { area = "admin" });
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

            var item = await db.Sizes.FindAsync(id);
            if (item != null)
            {
                db.Sizes.Remove(item);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
        [HttpPost]
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.Sizes.FindAsync(id);
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
