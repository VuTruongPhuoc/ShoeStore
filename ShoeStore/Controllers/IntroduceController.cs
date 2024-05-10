using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
	public class IntroduceController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
