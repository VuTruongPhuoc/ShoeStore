using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/news")]
    [Route("admin/news/{action}")]
    [Route("admin/news/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class NewsController : Controller
    {
        #region news
        private INotyfService _notyf;

        private ShoeStoreContext db = new ShoeStoreContext();
        public NewsController(INotyfService notyf)
        {
            _notyf= notyf;
        }
        public IActionResult Index(string Searchtext, int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<News> items = db.News.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x=>x.Title.Contains(Searchtext));
            }
            var pageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items); 
        }
        [HttpGet, Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost,Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(News model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string username = ((System.Security.Claims.ClaimsIdentity)User.Identity).Name.ToString();
                    model.Postedby = username;
                    model.CreateAt = DateTime.Now;
                    model.UpdateAt = DateTime.Now;
                    db.News.Add(model);
                    await db.SaveChangesAsync();
                    _notyf.Success("Thêm dữ liệu thành công");
                    return RedirectToAction("Index", "News", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _notyf.Error("Có lỗi khi thêm dữ liệu " + ex.Message);
                    return View(model);
                }
            }
            _notyf.Error("Có lỗi khi thêm dữ liệu ");
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
                try
                {
                    item.UpdateAt = DateTime.Now;
                    item.Title = model.Title;
                    item.Description = model.Description;
                    item.Status = model.Status;
                    item.Detail = model.Detail;
                    item.Image = model.Image;
                    await db.SaveChangesAsync();
					_notyf.Success("Cập nhật dữ liệu thành công");
					return RedirectToAction("index", "news", new { area = "admin" });
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

        #endregion
    }
}
