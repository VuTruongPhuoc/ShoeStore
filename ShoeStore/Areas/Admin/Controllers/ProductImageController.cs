using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;
using Microsoft.AspNetCore.Authorization;
namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/productimage")]
    [Route("admin/productimage/{action}")]
    [Route("admin/productimage/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class ProductImageController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        public async Task<IActionResult> Index(int id)
        {
            ViewBag.ProductDetailId = id;
            var items = db.ProductImages.Where(x => x.ProductDetailId == id).ToList();
            return View(items);
        }
        [HttpPost]
        public async Task<ActionResult> AddImage(int ProductDetailId, string imageurl)
        {
            db.ProductImages.Add(new ProductImage
            {
                ProductDetailId = ProductDetailId,
                Image = imageurl,
                IsDefault = false
            });
            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var item = db.ProductImages.Find(id);
            db.ProductImages.Remove(item);
            db.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> IsDefault(int id)
        {
            var item = await db.ProductImages.FindAsync(id);
            
            if (item != null)
            {
                var defaultItem = await db.ProductImages.SingleOrDefaultAsync(x => x.IsDefault && x.ProductDetailId == item.ProductDetailId);
                if(item != defaultItem)
                {
                    item.IsDefault = !item.IsDefault;
                    if (defaultItem != null)
                        defaultItem.IsDefault = !item.IsDefault;
                    var productdetail = await db.ProductDetails.FindAsync(item.ProductDetailId);
                    productdetail.Image = item.Image;
                    await db.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false });
        }
    }
}
