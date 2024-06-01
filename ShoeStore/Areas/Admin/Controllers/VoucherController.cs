using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("/admin/voucher")]
    [Route("/admin/voucher/{action}")]
    [Route("/admin/voucher/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class VoucherController : Controller
    {
        private readonly ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        public VoucherController(INotyfService notyf)
        {
            _notyf  = notyf;
        }
        public IActionResult Index(int ?page, string ? searchtext)
        {
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Vouchers> items = db.Vouchers.Where(v=>v.Status).ToList();
            if(searchtext != null)
            {
                items = items.Where(v=>v.Name.ToLower().Contains(searchtext.ToLower()));
            }
			var PageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
            var PageSize = 5;
			ViewBag.Page = PageIndex;
            ViewBag.PageSize = PageSize;
            items = items.ToPagedList(PageIndex, PageSize);
            return View(items);
        }
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(Vouchers model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(model.StartDate.Date < DateTime.Now.Date)
                    {
                        _notyf.Warning("Ngày bắt đầu không được nhỏ hơn ngày hiện tại",10);
                        return View(model);
                    }
                    if (model.EndDate.Date < model.StartDate.Date)
                    {
                        _notyf.Warning("Ngày kết thúc không được nhỏ hơn ngày bắt đầu", 10);
                        return View(model);
                    }
                        var voucher = db.Vouchers.FirstOrDefault(v=>v.Code == model.Code);
                        if(voucher != null)
                        {
						    if (model.Code == voucher.Code && model.Value != voucher.Value)
						    {
							    _notyf.Warning("Hiện tại đã có mã giảm giá này rồi vui lòng chọn mã khác", 10);
							    return View(model);
						    }
						    if (model.Code == voucher.Code && model.Value == voucher.Value)
						    {
							    voucher.Quantity += model.Quantity;
							    db.Vouchers.Add(voucher);
							    await db.SaveChangesAsync();
							    _notyf.Success("Thêm số lượng của voucher thành công");
							    return RedirectToAction("index", "voucher", new { area = "admin" });
						    }
                        }					                                 
                    model.CreateAt = DateTime.Now;
					model.UpdateAt = DateTime.Now;
					db.Vouchers.Add(model);
					await db.SaveChangesAsync();
					_notyf.Success("Thêm dữ liệu thành công");
					return RedirectToAction("index", "voucher", new { area = "admin" });
				}catch (Exception ex)
                {
					_notyf.Error("Có lỗi khi thêm dữ liệu " + ex.Message);
					return View(model);
				}
               
            }
            _notyf.Error("Có lỗi khi thêm dữ liệu");
            return View(model);
        }
        public IActionResult Edit(int id)
        {
            var item = db.Vouchers.Find(id);
            return View(item);
        }
        [HttpPost, Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(Vouchers model)
        {
            var item = await db.Vouchers.FindAsync(model.Id);
            if(ModelState.IsValid && item is not null)
            {
                try
                {
                    if (model.StartDate.Date < DateTime.Now.Date)
                    {
                        _notyf.Warning("Ngày bắt đầu không được nhỏ hơn ngày hiện tại", 10);
                        return View(model);
                    }
                    if (model.EndDate.Date < model.StartDate.Date)
                    {
                        _notyf.Warning("Ngày kết thúc không được nhỏ hơn ngày bắt đầu",10);
                        return View(model);
                    }
                    item.Name = model.Name;
					item.UpdateAt = DateTime.Now;
					item.Code = model.Code;
                    item.DiscountAmount = model.DiscountAmount;
                    item.StartDate = model.StartDate;
                    item.EndDate = model.EndDate;
                    item.Quantity = model.Quantity;
                    item.Value = model.Value;
					await db.SaveChangesAsync();
					_notyf.Success("Cập nhật dữ liệu thành công");
					return RedirectToAction("index", "voucher", new { area = "admin" });
				}
				catch (Exception ex)
				{
					_notyf.Error("Có lỗi khi cập nhật dữ liệu " + ex.Message);
					return View(model);
				}
			}
			_notyf.Error("Có lỗi khi cập nhật dữ liệu");
			return View(model);
		}
        public async Task<IActionResult> Delete(int id)
        {
            var item = await db.Vouchers.FindAsync(id);
            if(item != null)
            {
                db.Vouchers.Remove(item);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new {success = false});
        }
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.Vouchers.FindAsync(id);
            if (item != null)
            {
                item.Status = !item.Status;
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
    
}
