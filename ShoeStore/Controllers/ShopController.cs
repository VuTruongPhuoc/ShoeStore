using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using X.PagedList;

namespace ShoeStore.Controllers
{
	public class ShopController : Controller
	{
		ShoeStoreContext db = new ShoeStoreContext();
		public IActionResult Index(int ? id,int ? page, int[]? sizes)
		{
			ViewBag.sizes = sizes;
			if(page == null)
			{
				page = 1;
			}
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
			if (sizes != null && sizes.Length > 0)
			{
				items = items.Where(p => p.Size != null && sizes.Contains(p.Size.Id)).ToList();
			}
			var pageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
			var pageNumber = 10;
			ViewBag.CategoryId = id;
			return View(items.ToPagedList(pageIndex,pageNumber));
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
