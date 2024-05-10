using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using ShoeStore.Data;
using ShoeStore.Helpers;
using ShoeStore.Models;
using ShoeStore.Services;
using ShoeStore.ViewModels;

namespace ShoeStore.Controllers
{
	public class ShoppingCartController : Controller
	{
		private readonly ISendEmail _sendEmail;
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _config;
		private ShoeStoreContext db = new ShoeStoreContext();
		public ShoppingCartController(IWebHostEnvironment env, ISendEmail sendEmail, IConfiguration config)
		{
			_env = env;
			_sendEmail = sendEmail;
			_config = config;
		}
		public List<ShoppingCartItem> Cart => HttpContext.Session.Get<List<ShoppingCartItem>>(MySetting.CART_KEY) ?? new List<ShoppingCartItem>();
		public IActionResult Index()
		{
			var cart = Cart;
			if (cart != null)
			{
				return View(cart.ToList());
			}
			return View();
		}
		public async Task<ActionResult> Success()
		{
			return View();
		}
		[HttpGet]
		public ActionResult ShowCount()
		{
			var sa = new JsonSerializerSettings();
			var cart = Cart;
			if (cart != null)
			{
				return Json(new { count = cart.Count, sa });
			}
			return Json(new { count = 0, sa });
		}

		public ActionResult Checkout()
		{
            var cart = Cart;
            if (cart != null)
            {
                ViewBag.CheckCart = cart.ToList();
            }
            return View();
        }
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkout(OrderVM ordervm)
		{
			var code = new { success = false, code = -1 };
            var cart = Cart;
            if (cart != null)
            {
                ViewBag.CheckCart = cart.ToList();
                if (ModelState.IsValid)
                {
					Order order = new Order();
					order.CustomerName = ordervm.CustomerName;
					order.Phone = ordervm.Phone;
					order.Address = ordervm.Address + " " + ordervm.City + " " + ordervm.District + " " + ordervm.Ward;
					order.Email = ordervm.Email;
					order.Note = ordervm.Note;
					order.TotalAmount = cart.Sum(x => (x.Price * x.Quantity));
					order.TypePayment = ordervm.TypePayment;
					order.StatusOrder = 1;
					order.ShipFee = 0;
					Random rd = new Random();
					var date = DateTime.Now;
					order.Code = "DH" + date.Year.ToString() + date.Month.ToString() + date.Day.ToString() + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
					order.CreateAt = DateTime.Now;
					order.UpdateAt = DateTime.Now;

					db.Orders.Add(order);
					db.SaveChanges();
                    var orderdetail = new List<OrderDetail>();
                    foreach (var item in Cart)
                    {
                        orderdetail.Add(new OrderDetail
                        {
                            OrderId = order.Id,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            ProductDetailId = item.ProductId,
                            Discount = 0
                        });
                    }
                    db.OrderDetails.AddRange(orderdetail);
                    db.SaveChanges();
					var strSanPham = "";
					var thanhtien = decimal.Zero;
					var TongTien = decimal.Zero;
					foreach (var sp in cart.ToList())
					{
						strSanPham += "<tr>";
						strSanPham += "<td>" + sp.ProductName + "</td>";
						strSanPham += "<td>" + sp.Size + "</td>";
						strSanPham += "<td>" + sp.Quantity + "</td>";
						strSanPham += "<td>" + ShoeStore.Common.Common.FormatNumber(sp.TotalPrice, 0) + "</td>";
						strSanPham += "</tr>";
						thanhtien += sp.Price * sp.Quantity;
					}
					TongTien = thanhtien;
					string contentCustomer;
					string filePath = Path.Combine(_env.WebRootPath, "assets", "templates", "customersend.html");
					if (System.IO.File.Exists(filePath))
					{
						contentCustomer = System.IO.File.ReadAllText(filePath);
						contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
						contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
						contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
						contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
						contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
						contentCustomer = contentCustomer.Replace("{{Email}}", ordervm.Email);
						contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
						contentCustomer = contentCustomer.Replace("{{ThanhTien}}", ShoeStore.Common.Common.FormatNumber(thanhtien, 0));
						contentCustomer = contentCustomer.Replace("{{TongTien}}", ShoeStore.Common.Common.FormatNumber(TongTien, 0));
						_sendEmail.SendEmailAsync(ordervm.Email,"Đơn hàng #" + order.Code , contentCustomer.ToString());
					}
					string filePath2 = Path.Combine(_env.WebRootPath, "assets", "templates", "adminsend.html");
					if (System.IO.File.Exists(filePath2))
					{
						contentCustomer = System.IO.File.ReadAllText(filePath2);
						contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
						contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
						contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
						contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
						contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
						contentCustomer = contentCustomer.Replace("{{Email}}", ordervm.Email);
						contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
						contentCustomer = contentCustomer.Replace("{{ThanhTien}}", ShoeStore.Common.Common.FormatNumber(thanhtien, 0));
						contentCustomer = contentCustomer.Replace("{{TongTien}}", ShoeStore.Common.Common.FormatNumber(TongTien, 0));
						_sendEmail.SendEmailAsync(_config.GetValue<string>("SendEmail:Email"), "Đơn hàng #" + order.Code, contentCustomer.ToString());
					}
					HttpContext.Session.Set<List<ShoppingCartItem>>(MySetting.CART_KEY, new List<ShoppingCartItem>());
					return RedirectToAction("Success", "ShoppingCart");
                }

            }
			return Json(code);
		}
		[HttpPost]
		public async Task<ActionResult> AddToCart(int id, string size, int quantity = 1)
		{
			var code = new { success = false, msg = "", code = -1, count = 0 };
			var db = new ShoeStoreContext();

			var cart = Cart;
			var item = cart.SingleOrDefault(p => p.ProductId == id && p.Size == size);
			if (item == null)
			{
				var product = (from pd in db.ProductDetails
							   join p in db.Products on pd.ProductId equals p.Id
							   join c in db.Categories on p.CategoryId equals c.Id
							   where pd.Id == id && pd.Size.Name == size
							   select new { ProductDetail = pd, Product = p, Category = c })
			  .SingleOrDefault();

				if (product == null)
				{
					return Json(new { success = true, msg = "Sản phẩm không tồn tại!", code = 1, count = cart.Count });
					//cart = new List<ShoppingCartItem>();
				}
				item = new ShoppingCartItem
				{
					ProductId = product.ProductDetail.Id,
					ProductName = product.Product.Name,
					CategoryName = product.Category.Name,
					Size = size,
					Quantity = quantity
				};
				if (product.ProductDetail.Image != null)
				{
					item.ProductImg = product.ProductDetail.Image;
				}
				item.Price = product.ProductDetail.Price;
				if (product.ProductDetail.PriceSale > 0)
				{
					item.Price = (decimal)product.ProductDetail.PriceSale;
				}
				item.TotalPrice = item.Quantity * item.Price;
				cart.Add(item);
				code = new { success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, count = cart.Count };
			}
			else
			{
				item.Quantity += quantity;
				code = new { success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, count = cart.Count };
			}
			HttpContext.Session.Set(MySetting.CART_KEY, cart);
			return Json(code);
		}
        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
			var cart = Cart;
            if (cart != null)
            {
                
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [HttpPost]
		public ActionResult Delete(int id)
		{
			var code = new { success = false, msg = "", code = -1, count = 0 };
			var cart = Cart;
			if (cart != null)
			{
				var item = cart.SingleOrDefault(p => p.ProductId == id);
				if (item != null)
				{
					cart.Remove(item);
                    HttpContext.Session.Set(MySetting.CART_KEY, cart);
                    code = new { success = true, msg = "Xóa sản phẩm khỏi thành công", code = 1, count = cart.Count };
				}
			}
			return Json(code);
		}

    }
}
