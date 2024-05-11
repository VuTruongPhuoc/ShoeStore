using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Data;
using ShoeStore.Models;
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
        private readonly INotyfService _notyf;
        public OrderController(INotyfService notyf)
        {
            _notyf = notyf;
        }
		public async Task<IActionResult> Index(int? page, string searchtext, int? status)
		{
			ViewBag.searchtext = searchtext;
            ViewBag.status = status;
            var items = db.Orders.OrderByDescending(x=>x.CreateAt).ToList();
			if (page == null)
			{
				page = 1;
			}
			if (!searchtext.IsNullOrEmpty())
			{
				items = db.Orders.Where(o => o.Code.ToLower().Contains(searchtext.ToLower()) || o.CustomerName.ToLower().Contains(searchtext.ToLower())).ToList();
			}
            if (status != null)
            {
                items = db.Orders.Where(o => o.StatusOrder == status).ToList();
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
			items = items.OrderByDescending(x => x.CreateAt).ToList();
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
        public async Task<IActionResult> Confirm(int id)
        {
            var x = await db.Orders.FindAsync(id);

            var billct = db.OrderDetails.Where(c => c.OrderId == id).ToList();
            foreach (var item in billct)
            {
                var prdct = db.ProductDetails.FirstOrDefault(c => c.Id == item.ProductDetailId);
                var sl = prdct.Quantity = prdct.Quantity - item.Quantity;

                if (sl < 0)
                {
                    _notyf.Warning("Mặt hàng này trong kho không đủ");
                    return RedirectToAction("index");
                }
                else
                {
                   
                    prdct.Quantity = sl;
                    db.SaveChanges();
                    x.StatusOrder = 2;
                    _notyf.Success("Đã xác nhận đơn hàng!");
                    db.SaveChanges();
                    return RedirectToAction("index");

                }
            }

            return RedirectToAction("index");
        }

        public async Task<IActionResult> Delivery(int id)
        {
            var x = await db.Orders.FindAsync(id);
            x.StatusOrder = 3;
            await db.SaveChangesAsync();
            _notyf.Success("Đã xác nhận giao hàng");
            return RedirectToAction("index");
        }
        public async Task<IActionResult> CancelOrder(int id)
        {

            var x = await db.Orders.FindAsync(id);
            var y = db.OrderDetails.Where(c => c.OrderId == id).ToList();
            if (x.StatusOrder == 5)
            {
                foreach (var item in y)
                {
                    var prdt = await db.ProductDetails.FindAsync(item.ProductDetailId);
                    prdt.Quantity += item.Quantity;
                    await db.SaveChangesAsync();
                }
            }
            x.StatusOrder = 0;
            _notyf.Success("Đã xác nhận hủy đơn");
            return RedirectToAction("index");
        }
    }
}
