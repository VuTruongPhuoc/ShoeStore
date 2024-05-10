using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/productdetail")]
    [Route("admin/productdetail/{action}")]
    [Route("admin/productdetail/{action}/{id}")]
    public class ProductDetailController : Controller
    {
        ShoeStoreContext db = new ShoeStoreContext();
        public IActionResult GetList(string Searchtext, int? page)
        {
            ViewBag.Product = db.Products.ToList().OrderBy(x=>x.Name);
            ViewBag.Size = db.Sizes.ToList().OrderBy(x => x.Name);
            ViewBag.Color = db.Colors.ToList().OrderBy(x => x.Name);
            var pageSize = 3;
            if (page == null)
            {
                page = 1;
            }
            ViewBag.Searchtext = Searchtext;
            IEnumerable<ProductDetail> items = db.ProductDetails.ToList();
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.ProductId.ToString().Contains(Searchtext) || x.ColorId.ToString().Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        public IActionResult Index(string Searchtext, int? page, int id)
        {
            ViewBag.Size = db.Sizes.ToList().OrderBy(x => x.Name); ;
            ViewBag.Color = db.Colors.ToList().OrderBy(x => x.Name); 
            ViewBag.Product = db.Products.ToList().OrderBy(x => x.Name);
            var pageSize = 3;
            if (page == null)
            {
                page = 1;
            }
            ViewBag.Searchtext = Searchtext;
            IEnumerable<ProductDetail> items = db.ProductDetails.Where(x=>x.ProductId == id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.ProductId.ToString().Contains(Searchtext) || x.ColorId.ToString().Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        public IActionResult Add()
        {
            ViewBag.Size = db.Sizes.ToList().OrderBy(x => x.Name); ;
            ViewBag.Color = db.Colors.ToList().OrderBy(x => x.Name); ;
            ViewBag.Product = db.Products.ToList().OrderBy(x => x.Name);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductDetail model, List<string> Images, List<int> rDefault)
        {
            ViewBag.Size = db.Sizes.ToList();
            ViewBag.Color = db.Colors.ToList();
            ViewBag.Product = db.Products.ToList().OrderBy(x => x.Name);

            if (ModelState.IsValid)
            {
                try
                {
                    if (Images != null && Images.Count > 0)
                    {
                        for (int i = 0; i < Images.Count; i++)
                        {
                            if (i + 1 == rDefault[0])
                            {
                                model.Image = Images[i];
                                model.ProductImages.Add(new ProductImage
                                {
                                    ProductDetailId = model.Id,
                                    Image = Images[i],
                                    IsDefault = true
                                });
                            }
                            else
                            {
                                model.ProductImages.Add(new ProductImage
                                {
                                    ProductDetailId = model.Id,
                                    Image = Images[i],
                                    IsDefault = false
                                });
                            }
                        }
                    }
                    model.CreateAt = DateTime.Now;
                    model.UpdateAt = DateTime.Now;
                    var item = db.ProductDetails.FirstOrDefault(x => x.ProductId == model.ProductId && x.SizeId == model.SizeId && x.ColorId == model.ColorId);
                    if(item != null)
                    {
                        item.Quantity += model.Quantity;
                        db.SaveChangesAsync();
                        return RedirectToAction("Index", "Product", new { area = "Admin" });
                    }
                    db.ProductDetails.Add(model);
                    db.SaveChangesAsync();
                    return RedirectToAction("Index", "Product",new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ một cách thích hợp, có thể ghi log hoặc hiển thị thông báo lỗi
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu.");
                }
            }
            // Nếu ModelState không hợp lệ, quay lại view với dữ liệu và thông báo lỗi
            return View(model);
        }
        public IActionResult Edit(int id)
        {
            ViewBag.Size = db.Sizes.ToList();
            ViewBag.Color = db.Colors.ToList();
            ViewBag.Product = db.Products.ToList().OrderBy(x => x.Name);
            var item = db.ProductDetails.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductDetail model)
        {
            ViewBag.Size = db.Sizes.ToList();
            ViewBag.Color = db.Colors.ToList();
            ViewBag.Product = db.Products.ToList().OrderBy(x => x.Name);
            var item = await db.ProductDetails.FindAsync(model.Id);

            if (ModelState.IsValid && item is not null)
            {
                //try
                //{
                var checkImg = item.ProductImages.Where(x => x.ProductDetailId == item.Id);
                if (checkImg != null)
                {
                    foreach (var img in checkImg)
                    {
                        db.ProductImages.Remove(img);
                        db.SaveChanges();
                    }
                }
                item.Price = model.Price;
                item.Quantity = model.Quantity;
                item.SizeId = model.SizeId;
                item.ColorId = model.ColorId;
                item.PriceSale = model.PriceSale;
                item.OriginalPrice = model.OriginalPrice;
                item.UpdateAt = DateTime.Now;
                item.Status = model.Status;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "ProductDetail", new { area = "Admin",id = item.ProductId });
                //}
                //catch (Exception ex)
                //{
                //    // Xử lý ngoại lệ một cách thích hợp, có thể ghi log hoặc hiển thị thông báo lỗi
                //    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu.");
                //}
            }
            // Nếu ModelState không hợp lệ hoặc xảy ra ngoại lệ, quay lại view với dữ liệu và thông báo lỗi
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            var item = await db.ProductDetails.FindAsync(id);
            if (item != null)
            {
                var checkImg = item.ProductImages.Where(x => x.ProductDetailId == item.Id);
                if (checkImg != null)
                {
                    foreach (var img in checkImg)
                    {
                        db.ProductImages.Remove(img);
                        await db.SaveChangesAsync();
                    }
                }
                db.ProductDetails.Remove(item);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
        //[HttpPost]
        //public async Task<IActionResult> DeleteAll(string idstr)
        //{
        //    if (!string.IsNullOrEmpty(idstr))
        //    {
        //        var items = idstr.Split(",");
        //        if (items != null && items.Any())
        //        {
        //            foreach (var i in items)
        //            {
        //                var obj = await db.ProductDetails.FindAsync(Convert.ToInt32(i));
        //                db.ProductDetails.Remove(obj);
        //                await db.SaveChangesAsync();
        //            }
        //        }
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });
        //}
        [HttpPost]
        public async Task<IActionResult> IsActive(int id)
        {
            var item = await db.ProductDetails.FindAsync(id);
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
