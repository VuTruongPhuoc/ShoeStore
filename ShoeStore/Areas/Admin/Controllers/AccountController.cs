using ShoeStore.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;
using System.Security.Claims;
using X.PagedList;
using ShoeStore.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using NuGet.Common;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/account")]
    [Route("admin/account/{action}")]
    [Route("admin/account/{action}/{id}")]
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        public AccountController(INotyfService notyf, ShoeStoreContext db)
        {
            this.db = db;
            _notyf = notyf;
        }
        [AllowAnonymous]
        public IActionResult Index(string searchtext, int? page)
        {
            ViewBag.Role = db.Roles.ToList();
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            ViewBag.searchtext = searchtext;
            IEnumerable<Account> items = db.Accounts.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(searchtext))
            {
                items = items.Where(x => x.FullName.Contains(searchtext) || x.Username.Contains(searchtext));
            }
            var pageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        public IActionResult Add()
        {
            ViewBag.Role = db.Roles.ToList();
            return View();
        }
        [HttpPost, Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Account model)
        {

            ViewBag.Role = db.Roles.ToList();
            if (ModelState.IsValid)
            {
                try
                {
                    var randomkey = MyUtil.GenerateRandomKey();
                    var acc = new Account()
                    {
                        Id = model.Id,
                        Username = model.Username,
                        FullName = model.FullName,   
                        RoleId = model.RoleId,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        Email = model.Email,
                        RandomKey = randomkey,
                        Password = model.Password.ToMd5Hash(randomkey),
                        Status = 1,
                        CreateAt = DateTime.Now,
                        UpdateAt = DateTime.Now
                     };
                    db.Accounts.Add(acc);
                    await db.SaveChangesAsync();
                    _notyf.Success("Thêm dữ liệu thành công");
                    return RedirectToAction("index", "account", new { area = "admin" });
                }
                catch (Exception ex)
                {
                    _notyf.Error("Có lỗi khi thêm dữ liệu " + ex.Message);
                    return View(model);
                }
            }
            _notyf.Error("Có lỗi khi thêm dữ liệu");
            return View(model);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.Role = db.Roles.ToList();
            var item = db.Accounts.Find(id);
            return View(item);
        }
        [HttpPost, Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Account model)
        {
            var item = await db.Accounts.FindAsync(model.Id);
            if (ModelState.IsValid && item is not null)
            {
                try
                {
                    item.Username = model.Username;
                    item.RoleId = model.RoleId;
                    item.FullName = model.FullName;
                    item.Email = model.Email;
                    item.Password = model.Password;
                    item.PhoneNumber = model.PhoneNumber;
                    item.UpdateAt = DateTime.Now;
                    await db.SaveChangesAsync();
                    _notyf.Success("Cập nhật dữ liệu thành công");
                    return RedirectToAction("index", "account", new { area = "admin" });
                }
                catch (Exception ex)
                {
                    _notyf.Error("Có lỗi khi cập nhật dữ liệu " + ex.Message);
                    return View(model);
                }
            }
            ViewBag.Role = db.Roles.ToList();
            _notyf.Error("Có lỗi khi cập nhật dữ liệu");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
				var item = await db.Accounts.FindAsync(id);
				if (item != null)
				{
					db.Accounts.Remove(item);
					await db.SaveChangesAsync();
					return Json(new { success = true });
				}
				return Json(new { success = false , msg = "Đã xảy ra lỗi khi xóa dữ liệu" });
            }
            catch(Exception ex)
            {
				return Json(new { success = false , msg = "Đã xảy ra lỗi khi xóa dữ liệu " + ex.Message });
			}
          
        }
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string ReturnUrl)
        {
			ViewData["ReturnUrl"] = ReturnUrl;
			var checkacc = await db.Accounts.FirstOrDefaultAsync(a => a.Username == model.Email || a.Email == model.Email);
			if (checkacc != null)
			{
				var result = await db.Accounts.SingleOrDefaultAsync(a => a.Password == model.Password.ToMd5Hash(checkacc.RandomKey));
				{
					if (result != null)
					{
						var role = db.Roles.SingleOrDefault(p => p.Id == result.RoleId);
						var claims = new List<Claim>()
						{
                            new Claim(ClaimTypes.NameIdentifier, result.Username),
                            new Claim(ClaimTypes.Role, role.Name),
                            new Claim("Id", result.Id.ToString()),
                            new Claim("Username", result.Username),
                            new Claim(ClaimTypes.Name, result.Username),
                            new Claim(ClaimTypes.Email, result.Email)
                        };
						var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
						var principal = new ClaimsPrincipal(identity);

						await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        return RedirectToAction("index", "homeadmin", new { area = "admin" });
                    }
					else
					{
						ViewData["ErrorMessage"] = "Tên mật khẩu hoặc tài khoản không chính xác";
						return View(model);
					}

				}

			}
			ViewData["ErrorMessage"] = "Tên mật khẩu hoặc tài khoản không chính xác";
			return View(model);
		}
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/home/index");
        }
    }
}
