using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using ShoeStore.Services;

namespace ShoeStore.Areas.Admin.Controllers
{
	[Area("admin")]
	[Route("admin/order")]
	[Route("admin/order/{action}")]
	[Route("admin/order/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class OrderController : Controller
	{
		private readonly ShoeStoreContext db =new ShoeStoreContext();
        private readonly INotyfService _notyf;
        private readonly IExcelHandler _excelHandler;
        public OrderController(ShoeStoreContext db,INotyfService notyf, IExcelHandler excelHandler)
        {
            this.db = db;
            _notyf = notyf;
            _excelHandler = excelHandler;
        }
        public IEnumerable<Order> GetOrders(string? searchtext, int ? status)
        {
            var items = db.Orders.OrderByDescending(x => x.CreateAt).ToList();
            if (!searchtext.IsNullOrEmpty())
            {
                items = items.Where(o => o.Code.ToLower().Contains(searchtext.ToLower()) || o.CustomerName.ToLower().Contains(searchtext.ToLower())).ToList();
            }
            if (status != null)
            {
                items = items.Where(o => o.StatusOrder == status).ToList();
            }
            return items;
        }
		public async Task<IActionResult> Index(int? page, string? searchtext, int? status, DateTime? startdate, DateTime? enddate)
		{
			ViewBag.searchtext = searchtext;
            ViewBag.status = status;
            ViewBag.startdate = startdate;
            ViewBag.enddate = enddate;
			if (page == null)
			{
				page = 1;
			}

			var items = GetOrders(searchtext, status);
            if (startdate != null)
            {
                items = items.Where(o => o.CreateAt >= startdate).ToList();
            }
            if (enddate != null)
            {
                items = items.Where(o => o.CreateAt <= enddate).ToList();
            }
            if (startdate != null && enddate != null)
            {
                items = items.Where(o => o.CreateAt >= startdate && o.CreateAt <= enddate).ToList();
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
            ViewBag.Voucher = db.Vouchers.ToList();
            ViewBag.Productdetail = db.ProductDetails.ToList();
            ViewBag.Product = db.Products.ToList();
			ViewBag.Color = db.Colors.ToList();
            ViewBag.Size = db.Sizes.ToList();
            var orderdetail = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            var item = db.Orders.Find(id);
            ViewBag.order = item;
			return View(orderdetail);
		}
        public async Task<IActionResult> Confirm(int id)
        {
            var x = await db.Orders.FindAsync(id);

            var orderdt = db.OrderDetails.Where(c => c.OrderId == id).ToList();
            foreach (var item in orderdt)
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
                    x.UpdateAt = DateTime.Now;
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
            x.UpdateAt = DateTime.Now;
            await db.SaveChangesAsync();
            _notyf.Success("Đã xác nhận giao hàng");
            return RedirectToAction("index");
        }
        public async Task<IActionResult> SuccessfulDelivery(int id)
        {
            var x = await db.Orders.FindAsync(id);
            x.StatusOrder = 4;
            x.UpdateAt = DateTime.Now;
            x.PaymentDate = DateTime.Now;
            if(x.TypePayment.ToLower().Trim() == "Online - Chưa thanh toán".ToLower().Trim() || x.TypePayment.ToLower().Trim() == "Online - Đã thanh toán".ToLower().Trim())
            {
                x.TypePayment = "Online - Đã thanh toán";
            }
            else
            {
                x.TypePayment = "shipCod - Đã thanh toán";
            }            
            await db.SaveChangesAsync();
            _notyf.Success("Đã giao hàng thành công");
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
            x.UpdateAt = DateTime.Now;
            await db.SaveChangesAsync();
            _notyf.Success("Đã xác nhận hủy đơn");
            return RedirectToAction("index");
        }
        public async Task<IActionResult> ExportToExcel(string searchtext,int ? status)
        {
            var items = GetOrders(searchtext,status).ToList(); // Gọi phương thức search đúng cách và chuyển kết quả thành một danh sách

            var stream = await _excelHandler.Export(items); // Sử dụng phương thức Export của IExcelHandler

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{DateTime.Now.Ticks}_Report_Data_Order.xlsx");
        }
    }
}
