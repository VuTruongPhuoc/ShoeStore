using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
	[Area("admin")]
	[Route("admin/order")]
	[Route("admin/order/{action}")]
	[Route("admin/order/{action}/{id}")]
	public class OrderController : Controller
	{
		private readonly ShoeStoreContext db =new ShoeStoreContext();
		public async Task<IActionResult> Index(int? page)
		{
			var items = db.Orders.OrderByDescending(x=>x.CreateAt).ToList();
			if (page == null)
			{
				page = 1;
			}
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items.ToPagedList(pageIndex, pageSize));
		}
		public async Task<ActionResult> ViewOrder(int id)
		{
            ViewBag.Productdetail = db.ProductDetails.ToList();
            ViewBag.Product = db.Products.ToList();
			ViewBag.Color = db.Colors.ToList();
            ViewBag.Size = db.Sizes.ToList();
            ViewBag.Orderdetail = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            var item = db.Orders.Find(id);
			return View(item);
		}

	}
}
