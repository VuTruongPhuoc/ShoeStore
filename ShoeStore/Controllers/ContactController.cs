using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
	public class ContactController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
