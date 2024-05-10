using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Areas.Admin.Controllers
{
    public class WishListsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
