using Microsoft.AspNetCore.Mvc;
using ShoeStore.Models;
using ShoeStore.Data;
using X.PagedList;
using ShoeStore.Models.Common;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using ShoeStore.Services;
using MailKit.Search;
using System.IO;
using ShoeStore.ViewModels;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/product")]
    [Route("admin/product/{action}")]
    [Route("admin/product/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class ProductController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        private readonly IExcelHandler _excelHandler;
        public ProductController(ShoeStoreContext db,INotyfService notyf, IExcelHandler excelHandler)
        {
            this.db = db;
            _notyf = notyf;
            _excelHandler = excelHandler;
        }
        public IEnumerable<Product> search(string searchtext)
        {
            IEnumerable<Product> items = db.Products.OrderBy(x => x.Id);
            if (!string.IsNullOrEmpty(searchtext))
            {
                items = items.Where(x => x.Name.ToLower().Contains(searchtext.ToLower()) || x.Code.ToLower().Contains(searchtext.ToLower()) && x.Status);
            }
            return items;
        }
        public IActionResult Index(string searchtext, int ?page)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList();           
            
            if (page == null)
            {
                page = 1;
            }
            ViewBag.searchtext = searchtext;
            var items = search(searchtext);
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = 10;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product model)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreateAt = DateTime.Now;
                    model.UpdateAt = DateTime.Now;
                    model.ViewCount = 0;
                    db.Products.Add(model);
                    await db.SaveChangesAsync();
					_notyf.Success("Thêm dữ liệu thành công");
					return RedirectToAction("index", "product", new { area = "admin" });
				}
				catch (Exception ex)
				{
					_notyf.Error("Có lỗi khi thêm dữ liệu " + ex.Message);
					return View(model);
				}
			}
			_notyf.Error("Có lỗi khi thêm dữ liệu");
			return View(model);
		}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            var item = db.Products.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model)
        {
            ViewBag.Category = db.Categories.ToList().OrderByDescending(x => x.Id);
            ViewBag.Supplier = db.Suppliers.ToList().OrderByDescending(x => x.Id);
            var item = await db.Products.FindAsync(model.Id);
           
            if (ModelState.IsValid && item is not null)
            {
                try
                {
                    item.Name = model.Name;
                    item.SupplierId = model.SupplierId;
                    item.CategoryId = model.CategoryId;
                    item.Description = model.Description;
                    item.Detail = model.Detail;
                    item.UpdateAt = DateTime.Now;               
                    item.Status = model.Status;
                    await db.SaveChangesAsync();
					var productdts = db.ProductDetails.Where(p => p.ProductId == model.Id).ToList();
					foreach (var prds in productdts)
					{
                        var spl = prds.Name.Split("-");
						if (spl.Length > 1)
						{
							prds.Name = model.Name + " - " + spl[spl.Length-1].Trim(); // Lấy giá trị sau dấu '-' và loại bỏ khoảng trắng ở đầu và cuối chuỗi
						}
						else
						{
							prds.Name = prds.Name; // Nếu không có dấu '-' thì giữ nguyên giá trị
						}
					}
                    await db.SaveChangesAsync();
					_notyf.Success("Cập nhật dữ liệu thành công");
					return RedirectToAction("index", "product", new { area = "admin" });
				}
				catch (Exception ex)
				{
					_notyf.Error("Có lỗi khi cập nhật dữ liệu " + ex.Message);
					return View(model);
				}
			}
			_notyf.Error("Có lỗi khi cập nhật dữ liệu");
			return View(model);
		}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try 
            { 
                var item = await db.Products.FindAsync(id);
                if (item != null)
                {
                    db.Products.Remove(item);
                    await db.SaveChangesAsync();
                    return Json(new { success = true });
                }
			    return Json(new { success = false, msg = "Đã xảy ra lỗi khi xóa dữ liệu" });
		    }
            catch(Exception ex)
            {
                return Json(new {success = false, msg = "Đã xảy ra lỗi khi xóa dữ liệu " + ex.Message});
            }

        }
        [HttpPost]
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.Products.FindAsync(id);
            if (item != null)
            {
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        public async Task<IActionResult> ExportDataToExecl(string searchtext)
        {
            var items = search(searchtext).ToList(); // Gọi phương thức search đúng cách và chuyển kết quả thành một danh sách
			List<ProductVM_Excel> productexcel = new List<ProductVM_Excel>();
            
			foreach (var item in items)
			{
                var category = db.Categories.FirstOrDefault(c=>c.Id == item.CategoryId);
				var supplier = db.Suppliers.FirstOrDefault(c => c.Id == item.SupplierId);
                ProductVM_Excel excelitem = new ProductVM_Excel
                {
                    Id = item.Id,
                    ProductName = item.Name,
                    CategoryName = category.Name,
                    SupplierName = supplier.Name,
                    ProductCode = item.Code,
                    Description = item.Description,
                    ViewCount = item.ViewCount,
                    Status = item.Status ? "Sử dụng" : "Không sử dụng",
					CreateAt = item.CreateAt.HasValue ? item.CreateAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : "",
					UpdateAt = item.UpdateAt.HasValue ? item.UpdateAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : ""
				};

				productexcel.Add(excelitem);
			}
			var memoryStream = await _excelHandler.Export(productexcel); // Sử dụng phương thức Export của IExcelHandler

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"San_pham_Report_Data.xlsx");
        }
    }
}
