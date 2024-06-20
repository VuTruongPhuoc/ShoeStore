using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using ShoeStore.Data;
using ShoeStore.Helpers;
using ShoeStore.Models;
using ShoeStore.Services;
using ShoeStore.ViewModels;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Security.Policy;
using System.Web;
using System.Security.Claims;
using OfficeOpenXml.FormulaParsing.Excel.Functions;

namespace ShoeStore.Controllers
{
	public class ShoppingCartController : Controller
	{
		private readonly ISendEmail _sendEmail;
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _config;
		private readonly INotyfService _notyf;
        
        public string tmnCode = "L4F2PXZX";
        public string hashSecret = "GHT0O7GN8PWAECBZYPURV2KL0ITF5C4T";
        private ShoeStoreContext db = new ShoeStoreContext();
		public ShoppingCartController(IWebHostEnvironment env, ISendEmail sendEmail, IConfiguration config,INotyfService notyf, ShoeStoreContext db)
		{
			_env = env;
			_sendEmail = sendEmail;
			_config = config;
			_notyf = notyf;
			this.db = db;
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
		public IActionResult Success()
		{
			return View();
		}
        #region checkout
        [HttpGet, Authorize(Roles ="Customer")]
		public ActionResult Checkout(string selectedVoucher)
		{
			var cart = Cart;
			if (cart != null)
			{
				ViewBag.CheckCart = cart.ToList();
			}
			ViewBag.selectedvoucher = selectedVoucher;
			if(cart.Count == 0)
			{
                _notyf.Warning("Hiện tại không có sản phẩm trong giỏ hàng,vui lòng chọn thêm sản phẩm");
                return RedirectToAction("index", "shop");
            }
			var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);
			var vouchers = db.VoucherForAccs.Where(v => v.Status && v.EndDate > DateTime.Now && v.IdAccount == userid).ToList();
			var ordervm = new OrderVM
			{
				VoucherForAccs = vouchers,
                discountValue = ViewBag.discountValue ?? 0
			};
			return View(ordervm);
		}
        [ValidateAntiForgeryToken]
        [HttpPost,Authorize(Roles = "Customer")]
		public async Task<IActionResult> Checkout(OrderVM ordervm)
		{
			var cart = Cart;
			if (cart != null)
			{
				ViewBag.CheckCart = cart.ToList();
				if (ModelState.IsValid)
				{
                    var userid = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                    var order = new Order();
                    var customer = new Account();
                    if (ordervm.LikeCustomers)
                    {
                        customer = db.Accounts.SingleOrDefault(p => p.Id == userid);
                        if (customer.PhoneNumber != null)
                        {
                            order.Phone = ordervm.Phone ?? customer.PhoneNumber;
                        }
                        else
                        {
                            _notyf.Warning("Vui lòng cập nhật số điện thoại trong tài khoản của bạn");
                            return RedirectToAction("profile", "account", new { id = userid });
                        }
                        if (customer.Address != null)
                        {
                            order.Address = ordervm.Address ?? customer.Address;
                        }
                        else
                        {
                            _notyf.Warning("Vui lòng cập nhật địa chỉ trong tài khoản của bạn");
                            return RedirectToAction("profile", "account", new { id = userid });
                        }
                    }                   
					order.CustomerName = ordervm.CustomerName ?? customer.FullName;
					order.AccountId = userid;					
                    if (ordervm.Address != null)
					{
						order.Address = ordervm.Address + " , " + ordervm.Ward + " - " + ordervm.District + " - " + ordervm.City;
					}
					else {
						order.Address = customer.Address;
                    }
                    #region addorder
                    order.Phone = ordervm.Phone ?? customer.PhoneNumber;
					order.Email = ordervm.Email ?? customer.Email;
					order.Note = ordervm.Note;
					order.TotalAmount = ordervm.totalAmount;
					order.TypePayment = ordervm.TypePayment + " - Chưa thanh toán";
					order.StatusOrder = 1;
					order.ShipFee = ordervm.ShipFee;
					var voucher = db.Vouchers.FirstOrDefault(x => x.Code == ordervm.VoucherCode);
					if(voucher != null)
					{
                        order.VoucherId = voucher.Id;
                        await db.SaveChangesAsync();
                    }                  
					var voucherforacc = db.VoucherForAccs.FirstOrDefault(x => x.Code == ordervm.VoucherCode && x.IdAccount == userid);
					if (voucherforacc != null)
					{
						voucherforacc.Status = false;
						await db.SaveChangesAsync();
					}
					Random rd = new Random();
					var date = DateTime.Now;
					order.Code = "DH" + date.Year.ToString() + date.Month.ToString() + date.Day.ToString() + rd.Next(0, 9) + rd.Next(0, 9) 
						+ rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
					order.CreateAt = DateTime.Now;
					order.UpdateAt = DateTime.Now;
                    #endregion
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
					var TongTien = ordervm.totalAmount;
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
					var checkvoucher = voucher?.Name ?? "Không có voucher áp dụng";
					var sendemailord = new SendEmailOrderVM()
					{
						OrderID = order.Id,
						OrderCode = order.Code,
						strSanPham = strSanPham,
						TenKhachHang = order.CustomerName,
						Phone = order.Phone,
						Email = ordervm.Email ?? customer.Email,
						DiaChiNhanHang = order.Address,
						ThanhTien = thanhtien,
						GiamGia = ordervm.discountValue ?? 0,
						Voucher = checkvoucher,
						PhiShip = ordervm.ShipFee ?? 0,
						TongTien = TongTien ?? 0,
						HinhThucThanhToan = "Thanh toán khi nhận hàng"
					};
                    if (ordervm.TypePayment == "Online")
                    {
                        return await Payment(sendemailord);
                    }
                    SendEmailOrder(sendemailord);
                    HttpContext.Session.Set<List<ShoppingCartItem>>(MySetting.CART_KEY, new List<ShoppingCartItem>());
                    _notyf.Success("Đặt hàng thành công");
					return RedirectToAction("Success", "ShoppingCart");
				}
			}					
            if (cart.Count == 0)
            {
				_notyf.Warning("Hiện tại không có sản phẩm,vui lòng chọn thêm sản phẩm");
				return RedirectToAction("index", "shop");
			}
            _notyf.Warning("Đã xảy ra lỗi khi thêm dữ liệu");
            ordervm.VoucherForAccs = db.VoucherForAccs.Where(v => v.Status && v.EndDate > DateTime.Now).ToList();
            return View(ordervm);
		}
        #endregion
        #region payment
        public async Task<IActionResult> Payment(SendEmailOrderVM sendemailord)
        {
            string url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            int port = 7162; // Port của ứng dụng của bạn
            string domain;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                // Đây là production, sử dụng tên miền của website thực tế
                domain = "phshop.azurewebsites.net";
            }
            else
            {
                // Đây là local development, sử dụng localhost và cổng port
                domain = $"localhost:{port}";
            }
            //string encodedData = HttpUtility.UrlEncode(serializedData);
            //string returnUrl = $"https://{domain}/shoppingcart/paymentconfirm?id={order.Id}";
            //string returnUrl = $"https://phshop.azurewebsites.net/shoppingcart/paymentconfirm?id={sendemailord.OrderID}&email={sendemailord.Email}&voucher={sendemailord.Voucher}&discountamount={sendemailord.GiamGia}";

            string returnUrl = $"https://localhost:{7162}/shoppingcart/paymentconfirm?id={sendemailord.OrderID}&email={sendemailord.Email}&voucher={sendemailord.Voucher}&discountamount={sendemailord.GiamGia}";
            string hostName = System.Net.Dns.GetHostName();
            string clientIPAddress = System.Net.Dns.GetHostAddresses(hostName).GetValue(0).ToString();
            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", ((long)(sendemailord.TongTien * 100)).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", clientIPAddress); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return Redirect(paymentUrl);
        }
        public async Task<IActionResult> PaymentConfirm(int id ,string email, string voucher, string discountamount)
        {
            var order = await db.Orders.FindAsync(id);
            if (Request.QueryString.HasValue)
            {
				PayLib pay = new PayLib();
                var vnpayData = Request.Query;
                foreach (var (key, value) in vnpayData)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(key, value);
                    }
                }
                //lấy toàn bộ dữ liệu trả về
                var queryString = Request.QueryString.Value;
                var json = HttpUtility.ParseQueryString(queryString);

                long orderId = Convert.ToInt64(json["vnp_TxnRef"]); //mã hóa đơn
                string orderInfor = json["vnp_OrderInfo"].ToString(); //Thông tin giao dịch
                long vnpayTranId = Convert.ToInt64(json["vnp_TransactionNo"]); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = json["vnp_ResponseCode"].ToString(); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.Query["vnp_SecureHash"]; //hash của dữ liệu trả về
                var pos = Request.QueryString.Value.IndexOf("&vnp_SecureHash");

                //return Ok(Request.QueryString.Value.Substring(1, pos-1) + "\n" + vnp_SecureHash + "\n"+ PayLib.HmacSHA512(hashSecret, Request.QueryString.Value.Substring(1, pos-1)));
                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                      
                        if (order != null)
                        {
                            var orderdt = db.OrderDetails.Where(c => c.OrderId == id).ToList();
                            foreach (var item in orderdt)
                            {
                                var productdt = db.ProductDetails.FirstOrDefault(c => c.Id == item.ProductDetailId);
                                var quantity = productdt.Quantity = productdt.Quantity - item.Quantity;

                                if (quantity < 0)
                                {
                                    _notyf.Warning("Mặt hàng này trong kho không đủ");
                                    return RedirectToAction("index");
                                }
                                else
                                {
                                    productdt.Quantity = quantity; 
                                }
                               
                            }
                            var thanhtien = decimal.Zero;
                            var strSanPham = "";
                            var cart = Cart;
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
                            order.Code = orderId.ToString();
                            order.StatusOrder = 2;
                            order.TypePayment = "Online - Đã thanh toán";
                            order.CreateAt = DateTime.Now;
                            order.UpdateAt = DateTime.Now;
                            await db.SaveChangesAsync();

                            var sendemailord = new SendEmailOrderVM()
                            {
                                OrderID = order.Id,
                                OrderCode = order.Code,
                                strSanPham = strSanPham,
                                TenKhachHang = order.CustomerName,
                                Phone = order.Phone,
                                Email = email,
                                DiaChiNhanHang = order.Address,
                                ThanhTien = thanhtien,
                                GiamGia = decimal.Parse(discountamount),
                                Voucher = voucher,
                                PhiShip = order.ShipFee ?? 0,
                                TongTien = order.TotalAmount ?? 0,
                                HinhThucThanhToan = "Thanh toán online"
                            };
                            SendEmailOrder(sendemailord);
                            HttpContext.Session.Set<List<ShoppingCartItem>>(MySetting.CART_KEY, new List<ShoppingCartItem>());
                            _notyf.Success("Đặt hàng thành công");
                            ViewData["Success"] = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                            if (!User.Identity.IsAuthenticated)
                            {
                                return RedirectToAction("Index");
                            }
                            var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);
                            return RedirectToAction("orderhistory", "account", new { id = userid });
                        }
                    }
                    else
                    {
						//Thanh toán không thành công.Mã lỗi: vnp_ResponseCode
						var orderdt = db.OrderDetails.Where(c => c.OrderId == id).ToList();
						foreach (var item in orderdt)
						{
							db.OrderDetails.Remove(item);
							await db.SaveChangesAsync();
						}
						db.Orders.Remove(order);
						await db.SaveChangesAsync();
                        //Thanh toán không thành công.Mã lỗi: vnp_ResponseCode

                        ViewData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                        _notyf.Error("Đặt hàng thất bại");
						return RedirectToAction("checkout","shoppingcart");
                    }
                }
                else
                {
					var orderdt = db.OrderDetails.Where(c => c.OrderId == id);
					foreach (var item in orderdt)
					{
						db.OrderDetails.Remove(item);
						await db.SaveChangesAsync();
					}
					db.Orders.Remove(order);
					await db.SaveChangesAsync();
                    //Thanh toán không thành công.Mã lỗi: vnp_ResponseCode

                    ViewData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    _notyf.Error("Đặt hàng thất bại" );
                    return RedirectToAction("checkout", "shoppingcart");
                }
            }
            //phản hồi không hợp lệ
            return RedirectToAction("orderhistory", "account");

        }
        #endregion
        #region function sendemail
        public void SendEmailOrder(SendEmailOrderVM sendemailordervm)
		{
			string contentCustomer ;
			string filePath = Path.Combine(_env.WebRootPath, "assets", "templates", "customersend.html");
            if (System.IO.File.Exists(filePath))
            {
                contentCustomer = System.IO.File.ReadAllText(filePath);
                contentCustomer = contentCustomer.Replace("{{MaDon}}", sendemailordervm.OrderCode);
                contentCustomer = contentCustomer.Replace("{{SanPham}}", sendemailordervm.strSanPham);
                contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", sendemailordervm.TenKhachHang);
                contentCustomer = contentCustomer.Replace("{{Phone}}", sendemailordervm.Phone);
                contentCustomer = contentCustomer.Replace("{{Email}}", sendemailordervm.Email);
                contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", sendemailordervm.DiaChiNhanHang);
                contentCustomer = contentCustomer.Replace("{{ThanhTien}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.ThanhTien, 0));
                contentCustomer = contentCustomer.Replace("{{GiamGia}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.GiamGia, 0));
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", sendemailordervm.HinhThucThanhToan);
                contentCustomer = contentCustomer.Replace("{{Voucher}}", sendemailordervm.Voucher);
                contentCustomer = contentCustomer.Replace("{{PhiShip}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.PhiShip, 0));
                contentCustomer = contentCustomer.Replace("{{TongTien}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.TongTien, 0));
                _sendEmail.SendEmailAsync(sendemailordervm.Email, "Đơn hàng #" + sendemailordervm.OrderCode, contentCustomer.ToString());
            }
            string filePath2 = Path.Combine(_env.WebRootPath, "assets", "templates", "adminsend.html");
            if (System.IO.File.Exists(filePath2))
            {
				contentCustomer = System.IO.File.ReadAllText(filePath2);
                contentCustomer = contentCustomer.Replace("{{MaDon}}", sendemailordervm.OrderCode);
                contentCustomer = contentCustomer.Replace("{{SanPham}}", sendemailordervm.strSanPham);
                contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", sendemailordervm.TenKhachHang);
                contentCustomer = contentCustomer.Replace("{{Phone}}", sendemailordervm.Phone);
                contentCustomer = contentCustomer.Replace("{{Email}}", sendemailordervm.Email);
                contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", sendemailordervm.DiaChiNhanHang);
                contentCustomer = contentCustomer.Replace("{{ThanhTien}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.ThanhTien, 0));
                contentCustomer = contentCustomer.Replace("{{GiamGia}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.GiamGia, 0));
				contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", sendemailordervm.HinhThucThanhToan);
				contentCustomer = contentCustomer.Replace("{{Voucher}}", sendemailordervm.Voucher);
                contentCustomer = contentCustomer.Replace("{{PhiShip}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.PhiShip, 0));
                contentCustomer = contentCustomer.Replace("{{TongTien}}", ShoeStore.Common.Common.FormatNumber(sendemailordervm.TongTien, 0));
                _sendEmail.SendEmailAsync(_config.GetValue<string>("SendEmail:Email"), "Đơn hàng #" + sendemailordervm.OrderCode, contentCustomer.ToString());
            }

		}
        #endregion
        #region checkcart     
        [HttpPost]
		public async Task<ActionResult> AddToCart(int id, string size,int colorid, int quantity = 1)
		{
			var code = new { success = false, msg = "", code = -1, count = 0 };
			var cart = Cart;
			var item = cart.SingleOrDefault(p => p.ProductId == id && p.Size == size);
			if (item == null)
			{
				var checkproduct = await db.ProductDetails.FindAsync(id);
				var productid = db.Products.FirstOrDefault(p => p.Id == checkproduct.ProductId).Id;
				var product = (from pd in db.ProductDetails
							   join p in db.Products on pd.ProductId equals p.Id
							   join c in db.Categories on p.CategoryId equals c.Id
							   where p.Id == productid && pd.Size.Name == size && pd.ColorId == colorid
							   select new { ProductDetail = pd, Product = p, Category = c })
			    .SingleOrDefault();

				if (product == null)
				{
					return Json(new { success = true, msg = "Sản phẩm không tồn tại!", code = 1, count = cart.Count });
				}
				item = new ShoppingCartItem
				{
					ProductId = product.ProductDetail.Id,
					ProductName = checkproduct.Name,
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
					code = new { success = true, msg = "Xóa sản phẩm khỏi khỏi giỏ hàng thành công", code = 1, count = cart.Count };
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
			var inventorynum = productdt.Quantity;
			var cart = Cart;

			if(quantity > inventorynum)
			{
                var item = cart.SingleOrDefault(p => p.ProductId == productid);
                if(item != null)
				{
					item.Quantity = inventorynum;
					ViewBag.quantity = inventorynum;

				}
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
                return Json(new { success = false, quantity = item.Quantity });
            }
			if(quantity <= 0)
			{
                return Json(new { success = false, quantity = quantity });
            }
			return Json(code);
        }
        #endregion
        #region applydiscount
        [HttpPost]
		public async Task<IActionResult> ApplyDiscountFromYourVou(string selectedVoucher, OrderVM ordervm)
		{
			if (!User.Identity.IsAuthenticated)
			{
				_notyf.Warning("Vui lòng đăng nhập để sử dụng mã giảm giá!");
				return RedirectToAction("login", "account");
			}

			var voucher = db.VoucherForAccs.FirstOrDefault(v => v.Code == selectedVoucher && v.Status && v.EndDate > DateTime.Now);

			var cart = Cart.ToList();
			if (cart != null)
			{
				ViewBag.CheckCart = cart.ToList();
			}
			if (voucher != null && cart.Count > 0)
			{
				var totalMoney = decimal.Zero;

				foreach (var item in cart)
				{
					totalMoney += (item.Quantity * item.Price);
				}

				var discountValue = (totalMoney * voucher.Value) / 100;
				discountValue = Math.Min(discountValue, voucher.DiscountAmount);
				discountValue = Math.Max(discountValue, 0);

				var total = totalMoney - discountValue;

				ViewBag.discountvalue = discountValue;
				ViewBag.total = total;
				var vouchers = db.VoucherForAccs.Where(v => v.Status && v.EndDate > DateTime.Now).ToList();
				ordervm = new OrderVM
				{
					VoucherForAccs = vouchers,
					discountValue = discountValue
				};

				return View("checkout", ordervm);
			}
			else
			{
				_notyf.Warning("Voucher không tồn tại hoặc không có sản phẩm trong giỏ hàng", 10);
				return RedirectToAction("checkout", "shoppingcart", ordervm);
			}
		}
		[HttpPost]
        public async Task<IActionResult> ApplyDiscount(string selectedVoucher)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng mã giảm giá!" });
            }
            var voucher = db.Vouchers
                .FirstOrDefault(v => v.Code == selectedVoucher && v.Status && v.EndDate > DateTime.Now);
			var userid = int.Parse(User.Claims.FirstOrDefault(u => u.Type == "Id").Value);

            var cart = Cart.ToList();
            if (voucher != null)
            {
				var voucherforaccstatus = db.VoucherForAccs
                    .FirstOrDefault(v => v.Code == voucher.Code && v.Status && v.IdAccount == userid && v.EndDate > DateTime.Now);
				if (voucherforaccstatus == null)
				{
					return Json(new { success = false, message = "Voucher không tồn tại hoặc đã được sử dụng" });
				}
				var totalMoney = decimal.Zero;
                foreach (var item in cart)
                {
                    totalMoney += (item.Quantity * item.Price);
                }
                var discountRate = (decimal)voucher.Value / 100;
                var discountValue = totalMoney * discountRate;
                discountValue = Math.Max(discountValue, 0);
                discountValue = Math.Min(discountValue, voucher.DiscountAmount);
                var total = totalMoney - discountValue;
                return Json(new { success = true, discountValue = discountValue, total = total });
            }
            else
            {
                return Json(new { success = false, message = "Voucher không tồn tại hoặc quá thời gian sử dụng" });
            }
        }
        #endregion

    }
}
