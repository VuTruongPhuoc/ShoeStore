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

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/account")]
    [Route("admin/account/{action}")]
    [Route("admin/account/{action}/{id}")]
    public class AccountController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();

        [AllowAnonymous]
        public IActionResult Index(string Searchtext, int? page)
        {
            ViewBag.Role = db.Roles.ToList();
            var pageSize = 5;
            if (page == null)
            {
                page = 1;
            }
            ViewBag.Searchtext = Searchtext;
            IEnumerable<Account> items = db.Accounts.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.FullName.Contains(Searchtext) || x.Username.Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
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
        [HttpPost]
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
                    return RedirectToAction("index", "account", new { area = "admin" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu.");
                }
            }
            // Nếu ModelState không hợp lệ, quay lại view với dữ liệu và thông báo lỗi
            return View(model);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
           
            var item = db.Accounts.Find(id);
            return View(item);
        }
        [HttpPost, Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Account model)
        {
           
            var item = await db.Accounts.FindAsync(model.Id);

            if (ModelState.IsValid && item is not null)
            {
                //try
                //{
                item.Username = model.Username;
                item.Password = model.Password.ToMd5Hash(item.RandomKey);
                item.FullName = model.FullName;
                item.Email = model.Email;
                item.PhoneNumber = model.PhoneNumber;
                item.Address = model.Address;
                item.UpdateAt = DateTime.Now;
                item.Status = model.Status;
                await db.SaveChangesAsync();
                return RedirectToAction("index", "account", new { area = "admin" });
                //}
                //catch (Exception ex)
                //{
                //    // Xử lý ngoại lệ một cách thích hợp, có thể ghi log hoặc hiển thị thông báo lỗi
                //    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu.");
                //}
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await db.Accounts.FindAsync(id);
            if (item != null)
            {
                db.Accounts.Remove(item);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
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
							new Claim(ClaimTypes.NameIdentifier, result.Username)
						};
						claims.Add(new Claim(ClaimTypes.Role, role.Name));
						claims.Add(new Claim("Id", result.Id.ToString()));
						claims.Add(new Claim("Username", result.Username));
                        claims.Add(new Claim(ClaimTypes.Name, result.Username));
                        claims.Add(new Claim(ClaimTypes.Email, result.Email));

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
