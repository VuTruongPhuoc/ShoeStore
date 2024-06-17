using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Controllers
{
    public class NewsController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();
        public NewsController(ShoeStoreContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index(int ? page)
        {
            var pageSize = 5;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<News> items = db.News.OrderByDescending(x => x.CreateAt).Where(n=>n.Status);
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        public async Task<ActionResult> Detail(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }
    }
}
