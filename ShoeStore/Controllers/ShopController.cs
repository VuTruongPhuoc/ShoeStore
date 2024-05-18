using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;
using ShoeStore.ViewModels;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace ShoeStore.Controllers
{
	public class ShopController : Controller
	{
		private readonly INotyfService _notyf;
		ShoeStoreContext db = new ShoeStoreContext();

		public ShopController(INotyfService notyf)
		{
			_notyf = notyf;
		}
		public IActionResult Index(int ? id,int ? page, int[]? sizes, string? searchtext)
		{
			ViewBag.sizes = sizes;
			if(page == null)
			{
				page = 1;
			}
			ViewBag.Product = db.Products.ToList();
			ViewBag.Size = db.Sizes.ToList();
			ViewBag.wishlist = db.WishLists.ToList();
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
			var cate = db.Categories.Find(id);
			if(cate != null)
			{
				ViewBag.Category = cate.Name;
			}
			if (sizes != null && sizes.Length > 0)
			{
				items = items.Where(p => p.Size != null && sizes.Contains(int.Parse(p.Size.Name))).ToList();
			}
			var pageIndex = page.HasValue && page > 0 ? Convert.ToInt32(page) : 1;
			var pageNumber = 10;
			ViewBag.CategoryId = id;
			return View(items.ToPagedList(pageIndex,pageNumber));
		}
		public async Task<ActionResult> Detail(int id)
		{
			ViewBag.productdetailid = id;
			ViewBag.Product = db.Products.ToList();
			ViewBag.Category = db.Categories.ToList();
			ViewBag.ProductImage = db.ProductImages.ToList();
			ViewBag.Size = db.Sizes.OrderBy(x => x.Name).ToList();
			ViewBag.wishlist = db.WishLists.ToList();
			var item = await db.ProductDetails.FindAsync(id);
	
            ViewBag.productid = item.ProductId;
            var reviews = await db.Reviews.Where(r => r.ProductId == item.ProductId).ToListAsync();
			ViewBag.count = reviews.Count;
			
			var productdtandreview = new ProductDetailandReviewVM()
			{
				Id = item.Id,
				Image = item.Image,
				ProductId = item.ProductId,
				OriginalPrice = item.OriginalPrice,
				Price = item.Price,
				PriceSale = item.PriceSale,
				Quantity = item.Quantity,
				SizeId = item.SizeId,
				ColorId = item.ColorId,
				Status = item.Status,
				Product = item.Product,
				//Username = review.Username ?? "",
				//Email = review.Email ?? "",
				//Content = review.Content ?? "",
				//ProductDetailId = review.ProductId ?? 0,
				//Rate = review.Rate ?? 0,
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
		public async Task<ActionResult> DeleteReview(int id )
		{
			var review = await db.Reviews.FindAsync(id);
			if(review != null)
			{
				db.Reviews.Remove(review);
				await db.SaveChangesAsync();
				return Json(new { success = true });
			}
			return Json(new {success = false});
		}
	}
}
