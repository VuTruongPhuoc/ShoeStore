using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using ShoeStore.Services;
using ShoeStore.ViewModels;

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
            var order = await db.Orders.FindAsync(id);

            var orderdt = db.OrderDetails.Where(c => c.OrderId == id).ToList();
            foreach (var item in orderdt)
            {
                var productdt = db.ProductDetails.FirstOrDefault(c => c.Id == item.ProductDetailId);
                var quantity = productdt.Quantity = productdt.Quantity - item.Quantity;

                if (quantity < 0)
                {
                    _notyf.Warning("Mặt hàng này trong kho không đủ");
                    return RedirectToAction("index");
                }
                else
                {         
                    productdt.Quantity = quantity;             
                }
            }
            order.StatusOrder = 2;
            order.UpdateAt = DateTime.Now;
            await db.SaveChangesAsync();
            _notyf.Success("Đã xác nhận đơn hàng!");
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
            var order = await db.Orders.FindAsync(id);
            order.StatusOrder = 4;
            order.UpdateAt = DateTime.Now;
            order.PaymentDate = DateTime.Now;
            if(order.TypePayment.ToLower().Trim() == "Online - Chưa thanh toán".ToLower().Trim() || order.TypePayment.ToLower().Trim() == "Online - Đã thanh toán".ToLower().Trim())
            {
                order.TypePayment = "Online - Đã thanh toán";
            }
            else
            {
                order.TypePayment = "shipCod - Đã thanh toán";
            }            
            await db.SaveChangesAsync();
            _notyf.Success("Đã giao hàng thành công");
            return RedirectToAction("index");
        }
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await db.Orders.FindAsync(id);
            var orderdt = db.OrderDetails.Where(c => c.OrderId == id).ToList();
            if (order.StatusOrder == 5)
            {
                foreach (var item in orderdt)
                {
                    var prdt = await db.ProductDetails.FindAsync(item.ProductDetailId);
                    prdt.Quantity += item.Quantity;
                    await db.SaveChangesAsync();
                }
            }
            order.StatusOrder = 0;
            order.UpdateAt = DateTime.Now;
            await db.SaveChangesAsync();
            _notyf.Success("Đã xác nhận hủy đơn");
            return RedirectToAction("index");
        }
        public async Task<IActionResult> ExportDataToExecl(string searchtext,int ? status)
        {
            var items = GetOrders(searchtext,status).ToList(); // Gọi phương thức search đúng cách và chuyển kết quả thành một danh sách

            List<OrderVM_Excel> orderexcel = new List<OrderVM_Excel>();

            var stt = 1;
            foreach (var item in items)
            {
                var voucher = db.Vouchers.FirstOrDefault(v=>v.Id == item.VoucherId);
                var vouchername = "Không có voucher áp dụng";
                if(voucher != null)
                {
                    vouchername = voucher.Name;
                }
                OrderVM_Excel excelitem = new OrderVM_Excel()
                {
                    STT = stt++,
                    Code = item.Code,
                    CustomerName = item.CustomerName,
                    Phone = item.Phone,
                    Email = item.Email,
                    Address = item.Address ?? "",      
                    VoucherName = vouchername,
                    ShipFee = item.ShipFee,
                    TotalAmount = item.TotalAmount,
                    TypePayment = item.TypePayment,
                    StatusOrder = item.StatusOrder switch
                    {
                        0 => "Đơn đã hủy",
                        1 => "Chờ xác nhận",
                        2 => "Đã xác nhận đơn hàng",
                        3 => "Đang giao hàng",
                        4 => "Giao hàng thành công",
                        5 => "Đơn chờ hủy",
                        _ => item.StatusOrder.ToString()
                    },
                    Note = item.Note ?? "",
                    CreateAt = item.CreateAt.HasValue ? item.CreateAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : "",
                    UpdateAt = item.UpdateAt.HasValue ? item.UpdateAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : "",
					PaymentDate = item.PaymentDate.HasValue ? item.PaymentDate.Value.ToString("dd/MM/yyyy hh:mm:ss") : ""


				};
                orderexcel.Add(excelitem);
            }

            var memoryStream = await _excelHandler.Export(orderexcel); // Sử dụng phương thức Export của IExcelHandler

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Don_hang_Report_Data.xlsx");
        }
    }
}
