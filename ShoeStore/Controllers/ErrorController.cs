using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
