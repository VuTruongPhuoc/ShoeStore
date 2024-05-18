using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Areas.Admin.Controllers
{
	[Area("admin")]
    [Route("admin")]
    [Route("admin/{action}")]
    [Route("admin/{action}/{id}")]
	[Authorize(Roles = "Admin, Employee")]
    public class HomeAdminController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
