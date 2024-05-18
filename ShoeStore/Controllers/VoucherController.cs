using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Controllers
{
    public class VoucherController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        public VoucherController(INotyfService notyf)
        {
            _notyf = notyf;
        }
        public IActionResult Index()
        {
            try
            {
                var vouchers = db.Vouchers.Where(v=>v.Status && v.EndDate >= DateTime.Now).ToList();

                if (vouchers != null && vouchers.Any())
                {
                    return View(vouchers);
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi xảy ra
                return StatusCode(500, new { ErrorMessage = $"Lỗi viewáy chủ nội bộ: {ex.Message}" });
            }
        }
        public async Task<IActionResult> YourVouchers()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);
                    // Lấy danh sách voucher cho tài khoản
                    var voucherAcc = db.VoucherForAccs.Where(c => c.IdAccount == userid && c.Status && c.EndDate >= DateTime.Now).ToList();

                    if (voucherAcc != null && voucherAcc.Any())
                    {
                        return View(voucherAcc);
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi xảy ra
                _notyf.Error("Đã xảy ra lỗi khi lưu dữ liệu " + ex.Message);
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveVoucherForUser(int voucherid)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    _notyf.Warning("Vui lòng đăng nhập để sử dụng phiếu giảm giá!");
                    return RedirectToAction("index");
                }
                var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);
                var prodtId = TempData["prodtId"] as int?;
                var voucher = db.Vouchers.FirstOrDefault(v => v.Id == voucherid);
                var voucherAcc = db.VoucherForAccs.FirstOrDefault(v => v.IdVoucher == voucherid && v.IdAccount == userid);

                if (voucher != null)
                {
                    if (voucherAcc == null)
                    {
                        if (voucher.Quantity > 0)
                        {
                            // Tạo mới một đối tượng VoucherForAcc và lưu nó vào cơ sở dữ liệu
                            var voucherForAcc = new VoucherForAcc()
                            {
                                IdAccount = userid,
                                IdVoucher = voucher.Id,
                                Code = voucher.Code,
                                Name = voucher.Name,
                                Value = voucher.Value,
                                DiscountAmount = voucher.DiscountAmount,
                                EndDate = voucher.EndDate,
                                Status = voucher.Status,
                            };

                            db.VoucherForAccs.Add(voucherForAcc);
                            await db.SaveChangesAsync();

                            voucher.Quantity--;
                            await db.SaveChangesAsync();
                            _notyf.Success("Lưu phiếu gỉảm giá thành công!");

                            return RedirectToAction("index");
                        }
                        else
                        {
                            _notyf.Warning("Chúc bạn may mắn lần sau!");
                            return RedirectToAction("index");
                        }
                    }
                    else
                    {
                        _notyf.Success("Phiếu giảm giá đã có trong tài khoản của bạn!");
                        return RedirectToAction("index");

                    }
                }
                else
                {
                    _notyf.Warning("Phiếu giảm giá không hợp lệ!");
                    return RedirectToAction("index");
                }
            }
            catch (Exception ex)
            {
                _notyf.Error("Đã xảy ra lỗi khi lưu phiếu giảm giá!");
                return RedirectToAction("index");
            }
        }
        
        [HttpPost]        
        public async Task<IActionResult> ApplyVoucher()
        {
            return View();

        }
        
    }
}
