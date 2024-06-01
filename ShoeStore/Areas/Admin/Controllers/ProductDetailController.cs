﻿using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/productdetail")]
    [Route("admin/productdetail/{action}")]
    [Route("admin/productdetail/{action}/{id}")]
    [Authorize(Roles = "Admin, Employee")]
    public class ProductDetailController : Controller
    {
        private ShoeStoreContext db = new ShoeStoreContext();
        private readonly INotyfService _notyf;
        public ProductDetailController(INotyfService notyf)
        {
            _notyf = notyf;
        }
        public IActionResult GetList(string Searchtext, int? page)
        {
            ViewBag.Searchtext = Searchtext;
            ViewBag.Product = db.Products.ToList().OrderBy(x=>x.Name);
            ViewBag.Size = db.Sizes.ToList().OrderBy(x => x.Name);
            ViewBag.Color = db.Colors.ToList().OrderBy(x => x.Name);
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            ViewBag.Searchtext = Searchtext;
            IEnumerable<ProductDetail> items = db.ProductDetails.ToList();
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.ProductId.ToString().Contains(Searchtext) || x.ColorId.ToString().Contains(Searchtext) && x.Status);
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
            ViewBag.ProductId = id;
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            ViewBag.Searchtext = Searchtext;
            IEnumerable<ProductDetail> items = db.ProductDetails.Where(x=>x.ProductId == id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.ProductId.ToString().Contains(Searchtext) || x.ColorId.ToString().Contains(Searchtext) && x.Status);
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        public IActionResult Add(int id)
        {
            ViewBag.Size = db.Sizes.ToList().OrderBy(x => x.Name); ;
            ViewBag.Color = db.Colors.ToList().OrderBy(x => x.Name);
            ViewBag.Product = db.Products.ToList().OrderBy(x => x.Name);
            ViewBag.ProductId = id;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductDetail model, List<string> Images, List<int> rDefault)
        {
            ViewBag.Size = db.Sizes.ToList();
            ViewBag.Color = db.Colors.ToList();
            ViewBag.Product = db.Products.ToList().OrderBy(x => x.Name);
            model.Id = 0;
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
                    if(Images.Count == 0)
                    {
                        model.Image = "/files/images/product/maugiay.jpg";
                        model.ProductImages.Add(new ProductImage
                        {

                            ProductDetailId = model.Id,
                            Image = "/files/images/product/maugiay.jpg",
                            IsDefault = true
                            
                        });
                    }
                    var color = await db.Colors.FindAsync(model.ColorId);
                    var product = await db.Products.FindAsync(model.ProductId);
                    model.Name = product.Name + " - " + color.ColorCode;
                    model.CreateAt = DateTime.Now;
                    model.UpdateAt = DateTime.Now;
                    var item = db.ProductDetails.FirstOrDefault(x => x.ProductId == model.ProductId && x.SizeId == model.SizeId && x.ColorId == model.ColorId);
                    if(item != null)
                    {
                        item.Quantity += model.Quantity;
                        await db.SaveChangesAsync();
                        _notyf.Success("Thêm dữ liệu thành công");
                        return RedirectToAction("Index", "Product", new { area = "Admin" });
                    }
                    db.ProductDetails.Add(model);
                    await db.SaveChangesAsync();
					_notyf.Success("Thêm dữ liệu thành công");
					return RedirectToAction("index", "product", new { area = "admin" });
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
                try
                {
                    var checkImg = item.ProductImages.Where(x => x.ProductDetailId == item.Id);
                    if (checkImg != null)
                    {
                        foreach (var img in checkImg)
                        {
                            db.ProductImages.Remove(img);
                            db.SaveChanges();
                        }
                    }
                    item.Name = model.Name;
                    item.Price = model.Price;
                    item.Quantity = model.Quantity;
                    item.SizeId = model.SizeId;
                    item.ColorId = model.ColorId;
                    item.PriceSale = model.PriceSale;
                    item.UpdateAt = DateTime.Now;
                    item.Status = model.Status;
                    await db.SaveChangesAsync();
                    _notyf.Success("Cập nhật dữ liệu thành công");
                    return RedirectToAction("index", "product", new { area = "admin" });
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
