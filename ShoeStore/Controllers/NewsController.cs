using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
