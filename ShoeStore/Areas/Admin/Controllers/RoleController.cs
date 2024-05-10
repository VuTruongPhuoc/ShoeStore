using elFinder.NetCore.Helpers;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/role")]
    [Route("admin/role/{action}")]
    [Route("admin/role/{action}/{id}")]
    //[Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();

        public async Task<IActionResult> Index()
        {
            var items = db.Roles.ToList();
            return View(items);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Roles.Add(role);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu.");
                }
            }
            return RedirectToAction("index", "role", new { area = "admin" });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var item = db.Roles.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Role model)
        {
            var item = await db.Roles.FindAsync(model.Id);
            if(item != null)
            {
                item.Name = model.Name;
                await db.SaveChangesAsync();
                return RedirectToAction("index", "role", new { area = "admin" });
            }
            return View(item);
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await db.Roles.FindAsync(id);
            if (item != null)
            {
                db.Roles.Remove(item);
                await db.SaveChangesAsync();
                return Json(new {success = true});
            }
            return View();
        }
}
}
