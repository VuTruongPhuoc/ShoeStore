using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;
using ShoeStore.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using X.PagedList;
using static ShoeStore.ViewModels.FilterDataVM;

namespace ShoeStore.Controllers
{
	public class ShopController : Controller
	{
		private readonly INotyfService _notyf;
		ShoeStoreContext db = new ShoeStoreContext();
		public ShopController(INotyfService notyf, ShoeStoreContext db)
		{
			_notyf = notyf;
			this.db = db;
		}
		public IActionResult Index(int? id,int? page, string? searchtext)
		{
			if(page == null)
			{
				page = 1;
			}
			ViewBag.Product = db.Products.ToList();
			ViewBag.Size = db.Sizes.ToList();
			ViewBag.Color = db.Colors.ToList();
			ViewBag.wishlist = db.WishLists.ToList();
            ViewBag.Category = db.Categories.ToList();
			ViewBag.CategoryId = id;
            IEnumerable<ProductDetail> items = db.ProductDetails.Where(p=>p.Status).ToList();
			var product = db.Products.ToList();
			if(searchtext != null)
			{
				items = items.Where(p=>p.Product.Name.ToLower().Contains(searchtext.ToLower()));
			}
			if(id != null)
			{
				items = items.Where(x=>x.Product.CategoryId == id).ToList();
			}
			items = items
				.Where(x => x.Status)
				.GroupBy(x => new { x.ColorId, x.ProductId }) // Nhóm theo  màu và productid
				.Select(group => group.First()) // Chọn 1 mục tiêu mỗi nhóm
				.ToList();
			var pageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
			var pageNumber = 12;
			
			return View(items.ToPagedList(pageIndex,pageNumber));
		}
		[HttpPost]
		public async Task<IActionResult> GetFilterProducts([FromBody] FilterDataVM filter)
		{
			try
			{
				if (filter == null)
				{
					return BadRequest("Invalid filter data");
				}
				var filterresults = (from pd in db.ProductDetails
									 join p in db.Products on pd.ProductId equals p.Id
									 join c in db.Categories on p.CategoryId equals c.Id
									 where pd.Status == true && p.Status == true
                                     select new { productdetail = pd, product = p, category = c }).ToList();
                //var filterresults = db.ProductDetails.ToList();
                // Áp dụng bộ lọc
                if (filter.Categories != null && filter.Categories.Count > 0 && !filter.Categories.Contains("all"))
				{
					filterresults = filterresults.Where(p => filter.Categories.Contains(p.product.CategoryId.ToString())).ToList();

				} 
				if (filter.Colors != null && filter.Colors.Count > 0 && !filter.Colors.Contains("all"))
				{
					filterresults = filterresults.Where(p => filter.Colors.Contains(p.productdetail.ColorId.ToString())).ToList();

				}
				if (filter.Sizes != null && filter.Sizes.Count > 0 && !filter.Sizes.Contains("all"))
				{
					filterresults = filterresults.Where(p => filter.Sizes.Contains(p.productdetail.SizeId.ToString())).ToList();

				}
				if (filter.PriceRanges != null && filter.PriceRanges.Count > 0 && !filter.PriceRanges.Contains("all"))
				{
					List<PriceRange> priceRanges = new List<PriceRange>();
					foreach (var range in filter.PriceRanges)
					{
						var value = range.Split("-").ToArray();
						if (value.Length == 2)
						{
							PriceRange priceRange = new PriceRange();
							if (Int32.TryParse(value[0], out int minValue) && Int32.TryParse(value[1], out int maxValue))
							{
								priceRange.Min = minValue;
								priceRange.Max = maxValue;
								priceRanges.Add(priceRange);
							}
						}
					}
					if (priceRanges.Any(c => c.Max == 0))
					{
						filterresults = filterresults.Where(p => priceRanges.Any(r => (p.productdetail.PriceSale ?? p.productdetail.Price) >= r.Min)).ToList();

					}
					else
					{
						filterresults = filterresults.Where(p => priceRanges.Any(r => (p.productdetail.PriceSale ?? p.productdetail.Price) >= r.Min && (p.productdetail.PriceSale ?? p.productdetail.Price) <= r.Max)).ToList();
					}

				}
                filterresults = filterresults
                     .GroupBy(p => new { p.productdetail.ProductId, p.productdetail.ColorId })
                     .Select(group => group.First()) 
                     .ToList();
                ViewBag.Productlists = filterresults;
				ViewBag.Product = db.Products.ToList();
				ViewBag.Color = db.Colors.ToList();
                ViewBag.wishlist = db.WishLists.ToList();
                ViewBag.Category = db.Categories.ToList();

                return PartialView("_ReturnProducts", filterresults);
			}
			catch (Exception ex)
			{				
				return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
			}

		}
		public async Task<ActionResult> Detail(int id)
		{
			ViewBag.productdetailid = id;
			ViewBag.Product = db.Products.ToList();
			ViewBag.Category = db.Categories.ToList();
			ViewBag.ProductImage = db.ProductImages.ToList();
			ViewBag.Size = db.Sizes.OrderBy(x => x.Name).ToList();
			ViewBag.ProductDetail = db.ProductDetails.ToList();
			ViewBag.wishlist = db.WishLists.ToList();
			var item = await db.ProductDetails.FindAsync(id);
			ViewBag.productdetaillist = db.ProductDetails.Where(c => c.ProductId == item.ProductId).OrderBy(x=>x.SizeId);
			ViewBag.productid = item.ProductId;
            var reviews = await db.Reviews.Where(r => r.ProductId == item.ProductId).ToListAsync();
			ViewBag.count = reviews.Count;
			var productitem = await db.Products.FindAsync(item.ProductId);
			if(productitem != null)
			{
				productitem.ViewCount = productitem.ViewCount + 1;
				await db.SaveChangesAsync();
			}
			var productdtandreview = new ProductDetailandReviewVM()
			{
				Id = item.Id,
				Name = item.Name,
				Image = item.Image,
				ProductId = item.ProductId,
				Price = item.Price,
				PriceSale = item.PriceSale,
				Quantity = item.Quantity,
				SizeId = item.SizeId,
				ColorId = item.ColorId,
				Status = item.Status,
				Product = item.Product,
				ProductDetailId = item.Id,
				Reviews = reviews,
				
			};
			return View(productdtandreview);
		}
		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> PostReview(ProductDetailandReviewVM model)
		{
			if (ModelState.IsValid)
			{
				model.CreateAt = DateTime.Now;
				var review = new Review()
				{
					Username = model.Username,
					Email = model.Email,
					Rate = model.Rate,
					Content = model.Content,
					ProductId = model.ProductId,
					CreateAt = DateTime.Now,
				};
				model.Reviews.Add(review);
				db.Reviews.Add(review);
				await db.SaveChangesAsync();
				_notyf.Success("Đánh giá sản phẩm thành công");
				return RedirectToAction("detail", new {id = model.ProductDetailId});
			}
			return View(model);
		}
		[HttpPost,Authorize(Roles = "Admin,Employee")]
		public async Task<ActionResult> DeleteReview(int id ,int productid)
		{
			var review = await db.Reviews.FindAsync(id);
			//var username = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
			
			if(review != null)
			{
				db.Reviews.Remove(review);
				await db.SaveChangesAsync();
				var countreview = db.Reviews.Where(x => x.ProductId == productid).ToList();
				return Json(new { success = true , reviewquantity = countreview.Count() });
			}
			return Json(new {success = false });
		}
		public async Task<IActionResult> GetNumberOfProductDetail(int id,int sizeid,int colorid)
		{
			var product = await db.ProductDetails.FindAsync(id);
			var productid = db.Products.FirstOrDefault(p => p.Id == product.ProductId).Id;
			var item = db.ProductDetails.FirstOrDefault(x => x.ProductId == productid && x.SizeId == sizeid && x.ColorId == colorid && x.Status);
			if(item != null)
			{
				return Json(new { success = true, quantity = item.Quantity , productdetailid = item.Id , price = item.Price, pricesale = item.PriceSale != null && item.PriceSale != 0 ? item.PriceSale : item.Price});
			}
			return Json(new { success = false });
		}
	}
}
