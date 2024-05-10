using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/News")]
    [Route("Admin/News/{action}")]
    [Route("Admin/News/{action}/{id}")]
    public class NewsController : Controller
    {
        #region news
        private ShoeStoreContext db = new ShoeStoreContext();

        public IActionResult Index(string Searchtext, int? page)
        {
            var pageSize = 3;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<News> items = db.News.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
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
        public async Task<IActionResult> Add(News model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreateAt = DateTime.Now;
                    model.UpdateAt = DateTime.Now;
                    model.Alias = ShoeStore.Models.Common.Filter.FilterChar(model.Title);
                    db.News.Add(model);
                    db.SaveChangesAsync();
                    return RedirectToAction("Index", "News", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu.");
                }
            }
            return View(model);
        }
        public IActionResult Edit(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(News model)
        {
            var item = await db.News.FindAsync(model.Id);
            
            if (ModelState.IsValid && item is not null)
            {
                //try
                //{
                    item.UpdateAt = DateTime.Now;
                    item.Alias = ShoeStore.Models.Common.Filter.FilterChar(model.Title);
                    item.Title = model.Title;
                    item.Description = model.Description;
                    item.Status = model.Status;
                    item.Detail = model.Detail;
                    item.Image = model.Image;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index", "News", new { area = "Admin" });
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

            var item = await db.News.FindAsync(id);
            if (item != null)
            {
                db.News.Remove(item); 
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
        [HttpPost]
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.News.FindAsync(id);
            if (item != null)
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
            if (!string.IsNullOrEmpty(idstr))
            {
                var items = idstr.Split(",");
                if (items != null && items.Any())
                {
                    foreach( var i in items)
                    {
                        var obj = await db.News.FindAsync(Convert.ToInt32(i));
                        db.News.Remove(obj);
                        await db.SaveChangesAsync();
                    }    
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
        #endregion
    }
}
