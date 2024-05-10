using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Color")]
    [Route("Admin/Color/{action}")]
    [Route("Admin/Color/{action}/{id}")]
    public class ColorController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        public IActionResult Index(string Searchtext,int? page)
        {
            var pageSize = 3;
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
			if (ModelState.IsValid)
			{
				try
				{
					db.Colors.Add(model);
					await db.SaveChangesAsync();
					return RedirectToAction("Index", "Color", new { area = "Admin" });
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

            if (ModelState.IsValid && item is not null)
            {
                //try
                //{
                item.ColorCode = model.ColorCode;
                item.Name = model.Name;
                item.Status = model.Status;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Color", new { area = "Admin" });
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
        [HttpPost]
        public async Task<IActionResult> DeleteAll(string idstr)
        {
            if(!string.IsNullOrEmpty(idstr))
            {
                var items = idstr.Split(",");
                if(items != null && items.Any())
                {
                    foreach (var i in items)
                    {
                        var obj = await db.Colors.FindAsync(Convert.ToInt32(i));
                        db.Colors.Remove(obj);
                        await db.SaveChangesAsync();                       
                    }                  
                }
                return Json(new { succes = true });  
            }
            return Json(new { success = false });
        }
    }
}
