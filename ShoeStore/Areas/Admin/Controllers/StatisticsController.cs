using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Data;
using ShoeStore.Models;
using ShoeStore.Services;
using ShoeStore.ViewModels;
using System.IO;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/statistics")]
    [Route("admin/statistics/{action}")]
	[Authorize(Roles = "Admin,Employee")]
	public class StatisticsController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();
		private readonly IExcelHandler _excelHandler;
		public StatisticsController(ShoeStoreContext db, IExcelHandler excelHandler)
		{
			this.db = db;
			_excelHandler = excelHandler;
		}
        public IActionResult Index()
        {
            return View();
        }
		public IEnumerable<ProductDetail> search(string searchtext)
		{
			IEnumerable<ProductDetail> items = db.ProductDetails.OrderBy(x => x.Id);
			if (!searchtext.IsNullOrEmpty())
			{
				items = db.ProductDetails.Where(p => p.Name.ToLower().Contains(searchtext.ToLower())).OrderBy(p => p.Name).ThenBy(p => p.SizeId).ToList();
			}
			return items;
		}
		//[HttpGet("revenuestatistics")]
		public IActionResult RevenueStatistics()
        {
            return View();
        }
        public async Task<IActionResult> InventoryStatistics(int ? page , string ? searchtext)
        {
            var items = search(searchtext);
            ViewBag.searchtext = searchtext;
            ViewBag.Size = db.Sizes.ToList();
            ViewBag.Color = db.Colors.ToList();
            ViewBag.Supplier = db.Suppliers.ToList();
            ViewBag.Category = db.Categories.ToList();
            if (page == null)
            {
                page = 1;
            }
            var pageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
            var pageNumber = 10;
            ViewBag.Page = page;    
            ViewBag.PageSize = pageNumber;
          
            ViewBag.products = db.Products.ToList();
            return View(items.ToPagedList(pageIndex,pageNumber));
        }
		public async Task<IActionResult> ExportDataToExcel(string? searchtext)
		{
			var items = search(searchtext).OrderBy(p => p.Name).ThenBy(p => p.SizeId).ToList(); ; // Gọi phương thức search đúng cách và chuyển kết quả thành một danh sách
			List<Inventory_Excel> inventoryexcel = new List<Inventory_Excel>();
            var stt = 1;
			foreach (var item in items)
			{
				var product = db.Products.FirstOrDefault(c => c.Id == item.ProductId);
				var category = db.Categories.FirstOrDefault(s => s.Id == product.CategoryId);
				var supplier = db.Suppliers.FirstOrDefault(s => s.Id == product.SupplierId);
				var size = db.Sizes.FirstOrDefault(c => c.Id == item.SizeId);
				var color = db.Colors.FirstOrDefault(c => c.Id == item.ColorId);
                Inventory_Excel excelitem = new Inventory_Excel
                {
                    STT = stt++,
					Quantity = item.Quantity,
					SizeName = size.Name,
					ColorName = color.Name,
					ProductDetailName = item.Name,
					CategoryName = category.Name,
					SupplierName = supplier.Name

				};
				inventoryexcel.Add(excelitem);
			}
			var memoryStream = await _excelHandler.Export(inventoryexcel); // Sử dụng phương thức Export của IExcelHandler

			return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Hang_ton_Report_Data_.xlsx");
		}
		public async Task<IActionResult> ExportDataToExeclForRevenueList(string? currentmonth, string? currentyear)
		{
            IEnumerable<Order> items =  db.Orders.ToList();
            var filename = "";
            if (currentmonth != null && currentmonth.ToString() != "0")
            {
                filename = $"Danh_sach_doanh_thu_thang_{currentmonth}_nam_{currentyear}_Report_Data.xlsx";
                items = items.Where(o => o.PaymentDate.HasValue && o.PaymentDate.Value.Month.ToString() == currentmonth && o.PaymentDate.Value.Year.ToString() == currentyear).ToList();
            }
            else
            {
                filename = $"Danh_sach_doanh_thu_nam_{currentyear}_Report_Data.xlsx";
                items = items.Where(o => o.PaymentDate.HasValue && o.PaymentDate.Value.Year.ToString() == currentyear).ToList();
            }
            List<RevenueListVM_Excel> revenuelistexcel = new List<RevenueListVM_Excel>();
            var stt = 1;
            foreach (var item in items)
            {
                var voucher = db.Vouchers.FirstOrDefault(v => v.Id == item.VoucherId);
                var vouchername = "Không có voucher áp dụng";
                if (voucher != null)
                {
                    vouchername = voucher.Name;
                }
                RevenueListVM_Excel excelitem = new RevenueListVM_Excel()
                {
                    STT = stt++,
                    Code = item.Code,
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
                    CreateAt = item.CreateAt.HasValue ? item.CreateAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : "",
                    PaymentDate = item.PaymentDate.HasValue ? item.PaymentDate.Value.ToString("dd/MM/yyyy hh:mm:ss") : ""
                };
                revenuelistexcel.Add(excelitem);
            }
            var totalRevenue = revenuelistexcel.Sum(item => item.TotalAmount);
            RevenueListVM_Excel totalRevenueItem = new RevenueListVM_Excel()
            {
                STT = stt++,
                Code = "Tổng doanh thu:",
                TotalAmount = totalRevenue
            };

            // Thêm đối tượng tổng doanh thu vào danh sách
            revenuelistexcel.Add(totalRevenueItem);

            var memoryStream = await _excelHandler.Export(revenuelistexcel); // Sử dụng phương thức Export của IExcelHandler

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }
	}
}
