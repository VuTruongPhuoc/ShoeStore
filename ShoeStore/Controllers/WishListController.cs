using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using ShoeStore.Data;
using ShoeStore.Models;
using System.Security.Claims;
using X.PagedList;

namespace ShoeStore.Controllers
{
    public class WishListController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();
        public WishListController(ShoeStoreContext db)
        {
            this.db = db;
        }
        public async  Task<IActionResult> Index(int? page)
        {
            ViewBag.product = db.Products.ToList();
            ViewBag.productdetail = db.ProductDetails.ToList();
            ViewBag.category = db.Categories.ToList();
            ViewBag.size = db.Sizes.ToList();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "account");
            }
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);
            IEnumerable<WishList> items = db.WishLists.Where(x => x.AccountId == userid).OrderByDescending(x => x.CreateAt);
            var pageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            
            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> AddWishlist(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, msg = "Bạn chưa đăng nhập." });
            }
            //var userid = ((ClaimsIdentity)User.Identity).FindFirst("Id");
            var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);
            var item = new WishList()
            {
                ProductDetailId = id,
                AccountId = userid,
                CreateAt = DateTime.Now,
            };
            db.WishLists.Add(item);
            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteWishlist(int id)
        {
            var userid = int.Parse(((ClaimsIdentity)User.Identity).FindFirst("Id").Value);
            var checkItem = db.WishLists.FirstOrDefault(x => x.ProductDetailId == id && x.AccountId == userid);
            if (checkItem != null)
            {
                var item =  await db.WishLists.FindAsync(checkItem.Id);
                db.WishLists.Remove(item);
                var i = db.SaveChanges();
                return Json(new { success = true, message = "Xóa thành công." });
            }
            return Json(new { success = false, message = "Xóa thất bại." });
        }
    }
}
