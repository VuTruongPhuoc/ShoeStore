using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using System.Diagnostics;

namespace ShoeStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ShoeStoreContext db = new ShoeStoreContext();
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
			ViewBag.wishlist = db.WishLists.ToList();
			ViewBag.Product = db.Products.Where(x=>x.Status).ToList();
			//ViewBag.searchtext = searchtext;
			IEnumerable<ProductDetail> items = db.ProductDetails.Where(x=>x.Status).Take(10).ToList();         
            //if(!string.IsNullOrEmpty(searchtext))
            //{
            //    items = items.Where(p=>p.Product.Name.ToLower().Contains(searchtext.ToLower()));
            //}
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
