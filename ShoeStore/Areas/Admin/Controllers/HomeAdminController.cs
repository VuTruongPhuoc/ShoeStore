using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Route("Admin")]
	[Route("Admin/HomeAdmin")]
    [Route("Admin/HomeAdmin/{action}")]
    public class HomeAdminController : Controller
	{
		[Authorize]
		public IActionResult Index()
		{
			return View();
		}
	}
}
