﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.ViewModels;
using System.Security.Claims;
using ShoeStore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ShoeStore.Models;
using ShoeStore.Helpers;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using ShoeStore.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace ShoeStore.Controllers
{
    public class AccountController : Controller
    {
        private ISendEmail _sendEmail;
        private ShoeStoreContext db = new ShoeStoreContext();
        public AccountController(ISendEmail sendEmail, ShoeStoreContext db)
        {
            _sendEmail = sendEmail;
            this.db = db;
        }

        public IActionResult Profile()
        {
            return View();
        }
        #region login
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Login(string ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }
        [HttpPost, AllowAnonymous]
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
                            return Redirect(!string.IsNullOrEmpty(ViewData["ReturnUrl"]?.ToString()) ? ViewData["ReturnUrl"].ToString() : "~/Admin/HomeAdmin/Index");

                        }
                        else if (checkRoleEmpoloyee == true)
                        {
                            return Redirect(!string.IsNullOrEmpty(ViewData["ReturnUrl"]?.ToString()) ? ViewData["ReturnUrl"].ToString() : "~/Admin/HomeAdmin/Index");
                        }
                        else
                        {
                            return Redirect(!string.IsNullOrEmpty(ViewData["ReturnUrl"]?.ToString()) ? ViewData["ReturnUrl"].ToString() : "~/Home/Index");
                        }
                        //return RedirectToAction("index", "homeadmin", new { area = "admin" });
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
        #endregion
        #region register
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {

            var checkEmail = await db.Accounts.FirstOrDefaultAsync(c => c.Email == model.Email);
            var checkPhone = await db.Accounts.FirstOrDefaultAsync(c => c.PhoneNumber == model.PhoneNumber);
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.ConfirmPassword))
            {

                ViewData["ErrorMessage"] = "Vui lòng nhập số tài khoản.";
                return View("Register", model);

            }
            if (model.Password != model.ConfirmPassword)
            {
                ViewData["ErrorMessage"] = "Mật khẩu xác nhận và mật khẩu không khớp nhau,hãy thử lại!";
                return View("Register", model);
            }
            else if (checkEmail != null)
            {
                ViewData["ErrorMessage"] = "Email này đã được đăng kí, hãy tạo tài khoản bằng email khác để đăng kí!";
                return View("Register", model);
            }
            else if (checkPhone != null)
            {
                ViewData["ErrorMessage"] = "Số điện thoại này đã được đăng ký !";
                return View("Register", model);
            }
            var randomkey = MyUtil.GenerateRandomKey();
            var acc = new Account()
            {
                Username = model.Username,
                FullName = model.FullName,
                RoleId = 3,
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
            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return View("Login");
        }
        #endregion
        #region forgotpassword
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ResetPass resetPass)
        {
            if (string.IsNullOrWhiteSpace(resetPass.Email))
            {
                ViewData["ErrorMessage"] = "Email bắt buộc!";
                return View(resetPass);

            }
            var user = db.Accounts.FirstOrDefault(u => u.Email == resetPass.Email);

            if (user != null)
            {
                var resetToken = RandomCodeService.GenerateRandomCode(6);
                user.ResetPasswordcode = resetToken;
                await db.SaveChangesAsync();

                // Send reset link  email
                var resetLink = Url.Action("ResetPassword", "Account", new { }, Request.Scheme);
                var emailSubject = "Yêu cầu đặt lại mật khẩu";
                var emailBody = $"Chào {user.Username}, <br/> Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của bạn.Đây là mã đặt lại: <b>{resetToken}</b> Vui lòng click vào đừng dẫn này để có thể đặt lại mật khẩu tài khoản: <a href='{HtmlEncoder.Default.Encode(resetLink)}'>Reset Password</a>";

                await _sendEmail.SendEmailAsync(user.Email, emailSubject, emailBody);

                ViewData["Sucsess"] = "Mã đặt lại tài khoản đã được gửi đến Email của bạn,Hãy kiểm tra Email của bạn";
                return View(resetPass);
            }
            else
            {
                ViewData["ErrorMessage"] = "Không tìm thấy địa chỉ Email này!";
                return View(resetPass);
            }

        }
        public IActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (string.IsNullOrWhiteSpace(model.ConfirmCode) || string.IsNullOrWhiteSpace(model.ConfirmPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ViewData["ErrorMessage"] = "Vui lòng nhập mật khẩu và mật khẩu mới của bạn";
                return View(model);

            }
            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewData["ErrorMessage"] = "Mật khẩu mới và mật khẩu xác nhận của bạn không trùng khớp với nhau vui lòng thử lại!";
                return View(model);
            }
            else
            {
                var user = db.Accounts.FirstOrDefault(s => s.Email == model.Email);
                if (user != null)
                {
                    if (model.ConfirmCode != user.ResetPasswordcode)
                    {
                        ViewData["ErrorMessage"] = "Mã xác nhận không đúng!";
                        return View(model);
                    }
                    else
                    {
                        string randomkey = MyUtil.GenerateRandomKey();
                        user.RandomKey = randomkey;
                        user.Password = model.NewPassword.ToMd5Hash(randomkey);
                        user.ResetPasswordcode = null;
                        await db.SaveChangesAsync();
                        ViewData["Sucsess"] = "Đổi mật khẩu thành công,bây giờ bạn có thể quay lại trang đăng nhập!";
                        return View(model);
                    }

                }
                else
                {
                    ViewData["ErrorMessage"] = "Không tìm thấy địa chỉ Email!";
                    return View(model);
                }

            }

        }
        #endregion
        #region historyorder

        #endregion
        public async Task<IActionResult> Logout()
		{
			HttpContext.Session.Clear();
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect("~/home/index");
		}
    }
}