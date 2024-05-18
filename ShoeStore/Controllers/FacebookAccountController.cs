using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Helpers;
using ShoeStore.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace ShoeStore.Controllers
{
    public class FacebookAccountController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        [AllowAnonymous]
        [Route("/facebook/login")]
        public IActionResult FacebookLogin(string returnUrl)
        {
            return new ChallengeResult(
                FacebookDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(FacebookCallback), new { returnUrl }),
                });
        }
        [AllowAnonymous]
        [Route("/facebook/callback")]
        public async Task<ActionResult> FacebookCallback(string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);
            if (result?.Principal != null && result.Principal.Identity != null && result.Principal.Identity.IsAuthenticated)
            {
                var principal = result.Principal;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = principal.FindFirst(ClaimTypes.Name)?.Value;
                var existingUser = db.Accounts.FirstOrDefault(a => a.Email == email);
                if (existingUser != null)
                {
                    var role = db.Roles.SingleOrDefault(p => p.Id == existingUser.RoleId);
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, existingUser.Username),
                        new Claim(ClaimTypes.Role, role.Name),
                        new Claim("Id", existingUser.Id.ToString()),
                        new Claim("Username", existingUser.Username),
                        new Claim(ClaimTypes.Name, existingUser.Username),
                        new Claim(ClaimTypes.Email, existingUser.Email)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var newprincipal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newprincipal);

                    var checkRoleAdmin = false;
                    var checkRoleEmpoloyee = false;
                    var checkRoles = role.Name;
                    if (checkRoles.StartsWith("Adm"))
                    {
                        checkRoleAdmin = true;
                    }
                    else if (checkRoles.StartsWith("Sta"))
                    {
                        checkRoleEmpoloyee = true;
                    }
                    else
                    {
                        checkRoleAdmin = false;
                    }

                    if (checkRoleAdmin == true)
                    {
                        return Redirect(!string.IsNullOrEmpty(ViewData["ReturnUrl"]?.ToString()) ? ViewData["ReturnUrl"].ToString() : "~/admin/index");

                    }
                    else if (checkRoleEmpoloyee == true)
                    {
                        return Redirect(!string.IsNullOrEmpty(ViewData["ReturnUrl"]?.ToString()) ? ViewData["ReturnUrl"].ToString() : "~/admin/index");
                    }
                    else
                    {
                        return Redirect(!string.IsNullOrEmpty(ViewData["ReturnUrl"]?.ToString()) ? ViewData["ReturnUrl"].ToString() : "~/home/index");
                    }
                }
                else
                {
                    var randomkey = MyUtil.GenerateRandomKey();
                    var acc = new Account()
                    {
                        Username = name,
                        FullName = principal.FindFirst(ClaimTypes.Surname)?.Value + " " + principal.FindFirst(ClaimTypes.GivenName)?.Value,
                        RoleId = 3,
                        PhoneNumber = principal.FindFirst(ClaimTypes.MobilePhone)?.Value,
                        Address = null,
                        Email = email,
                        RandomKey = randomkey,
                        Password = "12345678".ToMd5Hash(randomkey),
                        Status = 1,
                        CreateAt = DateTime.Now,
                        UpdateAt = DateTime.Now
                    };
                    db.Accounts.Add(acc);
                    await db.SaveChangesAsync();

                    var role = db.Roles.SingleOrDefault(p => p.Id == acc.RoleId);
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, acc.Username),
                        new Claim(ClaimTypes.Role, role.Name),
                        new Claim("Id", acc.Id.ToString()),
                        new Claim("Username", acc.Username),
                        new Claim(ClaimTypes.Name, acc.Username),
                        new Claim(ClaimTypes.Email, acc.Email)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var newprincipal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newprincipal);
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["ErrorMessage"] = "Có lỗi khi đăng nhập";
            return Redirect("/Error/AccessDenied");
        }
    }
}
