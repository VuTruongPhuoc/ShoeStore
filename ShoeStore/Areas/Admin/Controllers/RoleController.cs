using AspNetCoreHero.ToastNotification.Abstractions;
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
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;

        public RoleController(INotyfService notyf)
        {
            _notyf = notyf;
        }
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
        public async Task<ActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Roles.Add(role);
                    db.SaveChanges();
                    _notyf.Success("Thêm dữ liệu thành công");
                    return RedirectToAction("index", "role", new { area = "admin" });
                }
                catch (Exception ex)
                {
                    _notyf.Error("Đã xảy ra lỗi khi thêm dữ liệu " + ex.Message);
                    return View(role);
                }
            }
            _notyf.Error("Đã xảy ra lỗi khi thêm dữ liệu");
            return View(role);
           
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
            if (ModelState.IsValid && item is not null)
        {
                try
                {
                    item.Name = model.Name;
                    await db.SaveChangesAsync();
                    _notyf.Success("Cập nhật dữ liệu thành công");
                    return RedirectToAction("index", "role", new { area = "admin" });
                }
                catch (Exception ex)
                {
                    _notyf.Error("Đã xảy ra lỗi khi cập nhật dữ liệu " + ex.Message);
                    return View(model);
                }
                   
                
            }
            _notyf.Error("Đã xảy ra lỗi khi cập nhật dữ liệu");
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
