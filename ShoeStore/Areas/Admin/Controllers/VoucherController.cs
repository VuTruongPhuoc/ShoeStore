using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Areas.Admin.Controllers
{
    public class VoucherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
