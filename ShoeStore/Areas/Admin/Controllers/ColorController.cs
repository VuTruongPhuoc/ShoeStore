using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/color")]
    [Route("admin/color/{action}")]
    [Route("admin/color/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class ColorController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        public ColorController(INotyfService notyf)
        {
            _notyf = notyf;
        }
        public IActionResult Index(string Searchtext,int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Color> items = db.Colors.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Name.Contains(Searchtext) || x.ColorCode.Contains(Searchtext));
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
		public async Task<IActionResult> Add(Color model)
		{
			var checkname = await db.Colors.FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower());
			var checkcode = await db.Colors.FirstOrDefaultAsync(c => c.ColorCode.ToLower() == model.ColorCode.ToLower());
			// Kiểm tra username đã tồn tại hay chưa
			if (checkname != null)
			{
				_notyf.Warning("Tên Màu này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (checkcode != null)
			{
				_notyf.Warning("Tên mã màu này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (ModelState.IsValid)
			{
				try
				{
					db.Colors.Add(model);
					await db.SaveChangesAsync();
					_notyf.Success("Thêm dữ liệu thành công");
					return RedirectToAction("index", "color", new { area = "admin" });
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
            var item = db.Colors.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Color model)
        {
            var item = await db.Colors.FindAsync(model.Id);
			var checkname = await db.Colors.FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower() && c.Id != model.Id);
			var checkcode = await db.Colors.FirstOrDefaultAsync(c => c.ColorCode.ToLower() == model.ColorCode.ToLower() && c.Id != model.Id);
			// Kiểm tra username đã tồn tại hay chưa
			if (checkname != null)
			{
				_notyf.Warning("Tên Màu này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (checkcode != null)
			{
				_notyf.Warning("Tên mã màu này đã được đăng ký, vui lòng thử lại!");
				return View(model);
			}
			if (ModelState.IsValid && item is not null)
            {
                try
                {
                    item.ColorCode = model.ColorCode;
                    item.Name = model.Name;
                    item.Status = model.Status;
                    await db.SaveChangesAsync();
				    _notyf.Success("Cập nhật dữ liệu thành công");
				return RedirectToAction("index", "color", new { area = "admin" });
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

            var item = await db.Colors.FindAsync(id);
            if (item != null)
            {
                db.Colors.Remove(item);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
        [HttpPost]
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.Colors.FindAsync(id);
            if(item != null)
            {
                item.Status = !item.Status;
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
      
    }
}
