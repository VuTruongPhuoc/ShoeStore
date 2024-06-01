using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using System.Diagnostics;
using System.Linq;

namespace ShoeStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ShoeStoreContext db = new ShoeStoreContext();
        public HomeController(ILogger<HomeController> logger, ShoeStoreContext db)
        {
            _logger = logger;
            this.db = db;
        }

        public IActionResult Index()
        {
			ViewBag.wishlist = db.WishLists.ToList();
			ViewBag.Product = db.Products.Where(x=>x.Status).ToList();
            ViewBag.Category = db.Categories.ToList();
			// Nhóm các s?n ph?m theo kích th??c và l?y m?t m?c t? m?i nhóm
			var items = db.ProductDetails
	            .Where(x => x.Status)
	            .GroupBy(x => new { x.ColorId, x.ProductId }) // Nhóm theo  màu s?c và ID s?n ph?m
	            .Select(group => group.First()) // Ch?n m?t m?c t? m?i nhóm
	            .Take(10) 
	            .ToList();
			return View(items);
        }
        public ActionResult Introduce()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
