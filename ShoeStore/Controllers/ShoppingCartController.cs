using AspNetCoreHero.ToastNotification.Abstractions;
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
		private readonly INotyfService _notyf;
		private ShoeStoreContext db = new ShoeStoreContext();
		public ShoppingCartController(IWebHostEnvironment env, ISendEmail sendEmail, IConfiguration config,INotyfService notyf)
		{
			_env = env;
			_sendEmail = sendEmail;
			_config = config;
			_notyf = notyf;
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
					var userid = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
					Order order = new Order();
					order.CustomerName = ordervm.CustomerName;
					order.AccountId = userid;
					order.Phone = ordervm.Phone;
					order.Address = ordervm.Address + " , " + ordervm.City + " - " + ordervm.District + " - " + ordervm.Ward;
					order.Email = ordervm.Email;
					order.Note = ordervm.Note;
					order.TotalAmount = cart.Sum(x => (x.Price * x.Quantity));
					order.TypePayment = ordervm.TypePayment + " - Chưa thanh toán";
					order.StatusOrder = 1;
					order.ShipFee = 0;
					Random rd = new Random();
					var date = DateTime.Now;
					order.Code = "DH" + date.Year.ToString() + date.Month.ToString() + date.Day.ToString() + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
					order.CreateAt = DateTime.Now;
					order.UpdateAt = DateTime.Now;

					db.Orders.Add(order);
					await db.SaveChangesAsync();
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
                    await db.SaveChangesAsync();
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
						_sendEmail.SendEmailAsync(ordervm.Email, "Đơn hàng #" + order.Code, contentCustomer.ToString());
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
			return View(ordervm);
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
				if(quantity > product.ProductDetail.Quantity)
				{
                    return Json(new { success = true, msg = "Số lượng sản phẩm chỉ còn " + product.ProductDetail.Quantity, code = 1, count = cart.Count });
                }
				cart.Add(item);
				code = new { success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, count = cart.Count };
			}
			else
			{
                var productdt = db.ProductDetails.SingleOrDefault(p => p.Id == id && p.Size.Name == size);
                
				if(quantity > productdt.Quantity)
				{
                    return Json(new { success = true, msg = "Số lượng sản phẩm chỉ còn " + productdt.Quantity, code = 1, count = cart.Count });
                }
				
                item.Quantity += quantity;
				if(item.Quantity > productdt.Quantity)
				{
					item.Quantity = productdt.Quantity;
				}
                item.TotalPrice = item.Quantity * item.Price;

            }
            code = new { success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, count = cart.Count };
            HttpContext.Session.Set(MySetting.CART_KEY, cart);
			return Json(code);
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
        public async Task<IActionResult> UpdateQuantity(int productid, int quantity)
        {
            var code = new { success = false,total =decimal.Zero, totalprice = decimal.Zero ,quantity = 0};
            var cart = Cart; // Giả sử Cart là danh sách các sản phẩm trong giỏ hàng của bạn
			var total = decimal.Zero;
            if (cart != null)
            {
				var item = cart.FirstOrDefault(c => c.ProductId == productid);
				if(item != null)
				{
                    item.Quantity = quantity;
                    item.TotalPrice = item.Quantity * item.Price;
                    total = item.TotalPrice;
                }
				
                decimal totalPrice = cart.Sum(item => item.TotalPrice);
                HttpContext.Session.Set(MySetting.CART_KEY, cart);

				// Trả về kết quả
				code = new { success = true,total = total, totalprice = totalPrice ,quantity = quantity};
            }

            return Json(code);
        }
		public async Task<IActionResult> CheckQuantity(int productid, int quantity)
		{
            var code = new { success = true ,quantity = 0};
			var productdt = await db.ProductDetails.FindAsync(productid);
			var slton = productdt.Quantity;
			var cart = Cart;

			if(quantity > slton)
			{
                var item = cart.SingleOrDefault(p => p.ProductId == productid);
                if(item != null)
				{
					item.Quantity = slton;
					ViewBag.quantity = slton;

				}
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
                return Json(new { success = false, quantity = item.Quantity });
            }
			if(quantity == 0)
			{
                return Json(new { success = false, quantity = quantity });
            }
			return Json(code);
        }

    }
}
