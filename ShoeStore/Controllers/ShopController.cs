using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;

namespace ShoeStore.Controllers
{
	public class ShopController : Controller
	{
		ShoeStoreContext db = new ShoeStoreContext();
		public IActionResult Index(int ? id)
		{
			ViewBag.Product = db.Products.ToList();
			ViewBag.Size = db.Sizes.ToList();

			var items = db.ProductDetails.ToList();
			if(id != null)
			{
				items = items.Where(x=>x.Product.CategoryId == id).ToList();
			}
			var cate = db.Categories.Find(id);
			if(cate != null)
			{
				ViewBag.Category = cate.Name;
			}
			ViewBag.CategoryId = id;
			return View(items);
		}
		public async Task<ActionResult> Detail(int id)
		{
			ViewBag.Product = db.Products.ToList();
			ViewBag.Category = db.Categories.ToList();
			ViewBag.ProductImage = db.ProductImages.ToList();
			ViewBag.Size = db.Sizes.OrderBy(x => x.Name).ToList();

			var item = await db.ProductDetails.FindAsync(id);	
			return View(item);
		}
	}
}
