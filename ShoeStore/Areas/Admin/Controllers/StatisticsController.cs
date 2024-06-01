using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Data;
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
        public IActionResult Index()
        {
            return View();
        }
        //[HttpGet("revenuestatistics")]
        public IActionResult RevenueStatistics()
        {
            return View();
        }
        public async Task<IActionResult> InventoryStatistics(int ? page , string ? searchtext)
        {
            var items = db.ProductDetails.OrderBy(p => p.Product.Name).ThenBy(p => p.SizeId).ToList();
            if (!searchtext.IsNullOrEmpty())
            {
                items = db.ProductDetails.Where(p => p.Product.Name.ToLower().Trim().Contains(searchtext.ToLower().Trim())).OrderBy(p => p.Product.Name).ThenBy(p => p.SizeId).ToList();
            }
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
    }
}
