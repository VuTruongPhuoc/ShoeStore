﻿using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/supplier")]
    [Route("admin/supplier/{action}")]
    [Route("admin/supplier/{action}/{id}")]
    public class SupplierController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        public async Task<IActionResult> Index(string Searchtext, int? page)
        {
            var pageSize = 3;
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
            if (ModelState.IsValid)
            {
                try
                {
                    db.Suppliers.Add(model);
                    db.SaveChangesAsync();
                    return RedirectToAction("Index", "Supplier", new { area = "Admin" });
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
            var item = db.Suppliers.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier model)
        {
            var item = await db.Suppliers.FindAsync(model.Id);

            if (ModelState.IsValid && item is not null)
            {
                //try
                //{
                item.Name = model.Name;
                item.PhoneNumber = model.PhoneNumber;
                item.Address = model.Address;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Supplier", new { area = "Admin" });
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

            var item = await db.Suppliers.FindAsync(id);
            if (item != null)
            {
                db.Suppliers.Remove(item);
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
                    foreach (var i in items)
                    {
                        var obj = await db.Suppliers.FindAsync(Convert.ToInt32(i));
                        db.Suppliers.Remove(obj);
                        await db.SaveChangesAsync();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
