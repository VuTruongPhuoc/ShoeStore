using Microsoft.AspNetCore.Authentication.Cookies;
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
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using AspNetCoreHero.ToastNotification.Abstractions;
using MailKit.Search;
using X.PagedList;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShoeStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly ISendEmail _sendEmail;
        private readonly ShoeStoreContext db;
        public AccountController(ISendEmail sendEmail, ShoeStoreContext db, INotyfService notyf)
        {
            _sendEmail = sendEmail;
            this.db = db;
            _notyf = notyf;
        }
        #region profile
        [HttpGet, Authorize(Roles = "Admin,Employee,Customer")]
        public IActionResult Profile(int id)
        {
            var user = db.Accounts.FirstOrDefault(c => c.Id == id);
            return View(user);
        }
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Profile(Account model)
        {
            var user = await db.Accounts.FindAsync(model.Id);
            if(user != null)
            {
                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    _notyf.Warning("Vui lòng nhập thông tin số điện thoại");
                    return View(model);
                }
                var checkUsername = await db.Accounts.FirstOrDefaultAsync(c => c.Username.ToLower() == model.Username.ToLower() && c.Id != model.Id);
                var checkEmail = await db.Accounts.FirstOrDefaultAsync(c => c.Email.ToLower() == model.Email.ToLower() && c.Id != model.Id);
                var checkPhone = await db.Accounts.FirstOrDefaultAsync(c => c.PhoneNumber == model.PhoneNumber && c.Id != model.Id);

                // Kiểm tra username đã tồn tại hay chưa
                if (checkUsername != null)
                {
                    _notyf.Warning("Username này đã được đăng ký, vui lòng thử lại!");
                    return View( model);
                }

                // Kiểm tra email đã tồn tại hay chưa
                if (checkEmail != null)
                {
                    _notyf.Warning("Email này đã được đăng ký, vui lòng thử lại!");
                    return View(model);
                }

                // Kiểm tra số điện thoại đã tồn tại hay chưa
                if (checkPhone != null)
                {
                    _notyf.Warning("Số điện thoại này đã được đăng ký, vui lòng thử lại!");
                    return View(model);
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        user.Username = model.Username;
                        user.Email = model.Email;
                        user.PhoneNumber = model.PhoneNumber;
                        user.FullName = model.FullName;
                        user.Address = model.Address;
                        await db.SaveChangesAsync();
                        _notyf.Success("Lưu thông tin thành công");
                        return RedirectToAction("profile", "account", new { id = model.Id });
                    }
                    catch (Exception ex)
                    {
                        _notyf.Warning("Có lỗi khi cập nhật dữ liệu" + ex.Message);
                        return View(model);
                    }
                }
            }
            _notyf.Warning("Có lỗi khi cập nhật dữ liệu");
            return View(model);
        }
        #endregion
        #region changepassword
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var userid = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst("Id");
                model.UserId = int.Parse(userid?.Value);

                var user = await db.Accounts.FirstOrDefaultAsync(c => c.Id == model.UserId);
                if (user != null)
                {
                    if (model.OldPassword.ToMd5Hash(user.RandomKey) != user.Password)
                    {
                        ViewData["ErrorMessage"] = "Mật khẩu cũ không đúng";
                        return View("ChangePassword", model);
                    }
                    if (model.NewPassWord != model.ConfirmPassword)
                    {
                        ViewData["ErrorMessage"] = " Mật khẩu không khớp với mật khẩu mới";
                        return View("ChangePassword", model);
                    }
                    user.Password = model.NewPassWord.ToMd5Hash(user.RandomKey);
                    db.Update(user);
                    await db.SaveChangesAsync();
                    _notyf.Success("Thay đổi mật khẩu thành công");
                    
                    return Redirect("~/Home/Index");

                }
            }
            _notyf.Error("Error");
            return View();
        }
        #endregion
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
                        if (role.Name.StartsWith("Adm"))
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
            // Kiểm tra xem có thông tin bị thiếu không
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.PhoneNumber)
                || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
            {
              
                ViewData["ErrorMessage"] = "Vui lòng nhập đủ thông tin.";
                _notyf.Warning("Vui lòng nhập đủ thông tin.");
                return View("Register", model);
            }

            // Kiểm tra xác nhận mật khẩu
            if (model.Password != model.ConfirmPassword)
            {
                ViewData["ErrorMessage"] = "Mật khẩu xác nhận và mật khẩu không khớp nhau, vui lòng thử lại!";               
                return View("Register", model);
            }

            // Kiểm tra định dạng email
            if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewData["ErrorMessage"] = "Định dạng email không hợp lệ, vui lòng nhập lại!";
                return View("Register", model);
            }

            // Kiểm tra định dạng số điện thoại
            if (!Regex.IsMatch(model.PhoneNumber, @"0[987654321]\d{8}"))
            {
                ViewData["ErrorMessage"] = "Định dạng số điện thoại không hợp lệ, vui lòng nhập lại!";
                return View("Register", model);
            }

            // Kiểm tra xem username, email, số điện thoại đã tồn tại chưa
            var checkUsername = await db.Accounts.FirstOrDefaultAsync(c => c.Username.ToLower() == model.Username.ToLower());
            var checkEmail = await db.Accounts.FirstOrDefaultAsync(c => c.Email.ToLower() == model.Email.ToLower());
            var checkPhone = await db.Accounts.FirstOrDefaultAsync(c => c.PhoneNumber == model.PhoneNumber);

            // Kiểm tra username đã tồn tại hay chưa
            if (checkUsername != null)
            {
                ViewData["ErrorMessage"] = "Username này đã được đăng ký, vui lòng thử lại!";
                return View("Register", model);
            }

            // Kiểm tra email đã tồn tại hay chưa
            if (checkEmail != null)
            {
                ViewData["ErrorMessage"] = "Email này đã được đăng ký, vui lòng thử lại!";
                return View("Register", model);
            }

            // Kiểm tra số điện thoại đã tồn tại hay chưa
            if (checkPhone != null)
            {
                ViewData["ErrorMessage"] = "Số điện thoại này đã được đăng ký, vui lòng thử lại!";
                return View("Register", model);
            }

            // Nếu tất cả đều hợp lệ, thì thêm tài khoản mới vào cơ sở dữ liệu và chuyển hướng đến trang đăng nhập
            var randomkey = MyUtil.GenerateRandomKey();
            var acc = new Account()
            {
                Username = model.Username,
                FullName = model.FullName,
                RoleId = 2,
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
            ViewData["Success"] = "Đăng ký tài khoản thành công";
            _notyf.Success("Đăng ký tài khoản thành công");
            return RedirectToAction("Login","Account");
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
        #region orderhistory
        [HttpGet,Authorize(Roles = "Customer")]
        public async Task<IActionResult> OrderHistory(int id , int? page , string? searchtext, int? status, DateTime? startdate , DateTime? enddate )
        {
            ViewBag.searchtext = searchtext;
            ViewBag.status = status;
            ViewBag.startdate = startdate;
            ViewBag.enddate = enddate;
            var items = db.Orders.Where(o=>o.AccountId == id).OrderByDescending(x => x.CreateAt).ToList();
            if (page == null)
            {
                page = 1;
            }
            if (startdate != null)
            {
                items = items.Where(o => o.CreateAt >= startdate).ToList();
            }
            if (enddate != null)
            {
                items = items.Where(o => o.CreateAt <= enddate).ToList();
            }
            if (startdate != null && enddate != null)
            {
                items = items.Where(o => o.CreateAt >= startdate && o.CreateAt <= enddate).ToList();
            }
            if (!searchtext.IsNullOrEmpty())
            {
                items =items.Where(o => o.Code.ToLower().Contains(searchtext.ToLower()) || o.CustomerName.ToLower().Contains(searchtext.ToLower())).ToList();
            }
            if (status != null)
            {
                items = items.Where(o => o.StatusOrder == status).ToList();
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            items = items.OrderByDescending(x => x.CreateAt).ToList();
            return View(items.ToPagedList(pageIndex, pageSize));
        }
        public async Task<IActionResult> OrderHistoryDetail(int? orderid)
        {
            //var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);
            var userid = int.Parse(((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst("Id").Value);
            var userorders = db.Orders.Where(c => c.AccountId == userid && c.Id  == orderid).OrderByDescending(d => d.CreateAt).ToList();
            ViewBag.vieworder = userorders;

            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("~/account/login");
            }
            // Lấy thông tin về voucher
            foreach (var userorder in userorders)
            {
                if (userorder.VoucherId.HasValue)
                {
                    userorder.Voucher = db.Vouchers.FirstOrDefault(v => v.Id == userorder.VoucherId);
                }
            }
            ViewBag.vieworderct = db.OrderDetails.ToList();
            ViewBag.viewprdct = db.ProductDetails.ToList();
            ViewBag.viewprd = db.Products.ToList();
            ViewBag.Voucher = db.Vouchers.ToList();
            ViewBag.size = db.Sizes.ToList();
            ViewBag.color = db.Colors.ToList();
            ViewBag.image = db.ProductImages.ToList();
            return View(userorders);
        }
        #endregion
        public async Task<IActionResult> Received(int id)
        {
            var userid = int.Parse(User.Claims.FirstOrDefault(u=>u.Type == "Id").Value);
            var order = await db.Orders.FindAsync(id);
            order.UpdateAt = DateTime.Now;
            _notyf.Success("Đã xác nhận nhận hàng");
            await db.SaveChangesAsync();
            return RedirectToAction("orderhistorydetail","account", new {orderid = id});
        }
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await db.Orders.FindAsync(id);
            if(order.StatusOrder == 1)
            {
                order.StatusOrder = 0;
                order.UpdateAt = DateTime.Now;
            }else if(order.StatusOrder == 2)
            {
                order.StatusOrder = 5;
                order.UpdateAt = DateTime.Now;
            }
            await db.SaveChangesAsync();
            _notyf.Success("Đã xác nhận hủy đơn");
            return RedirectToAction("orderhistorydetail", "account", new { orderid = id});
        }
        public async Task<IActionResult> Logout()
		{
			//HttpContext.Session.Clear();
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect("~/home/index");
		}
    }
}
