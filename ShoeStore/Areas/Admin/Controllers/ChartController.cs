using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using ShoeStore.Data;
using ShoeStore.Models;
using System.Drawing;
using System.Net.Http;


namespace ShoeStore.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly ShoeStoreContext db;
        private readonly HttpClient _httpClient;
        public ChartController(ShoeStoreContext db, HttpClient httpClient)
        {
            this.db = db;
            _httpClient = httpClient;
        }
        [HttpGet("StatisticsByYear/{year}")]
        public IActionResult StatisticsByYear(int year)
        {
            try
            {
                var data = db.Orders
                    .Where(b => b.PaymentDate.HasValue && b.PaymentDate.Value.Year == year && b.StatusOrder == 4)
                    .GroupBy(b => b.PaymentDate.Value.Month) 
                    .Select(group => new
                    {
                        Month = group.Key,
                        Money = group.Sum(b => b.TotalAmount)
                    })
                    .OrderBy(item => item.Month)
                    .ToList();
                var chartData = new
                {
                    labels = data.Select(item => item.Month),
                    values = data.Select(item => item.Money),
                    label = "Doanh Thu Năm " + year
                };

                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("StatisticsByMonthOfTheYear/{month}/{year}")]
        public IActionResult StatisticsByMonthOfTheYear(int month, int year)
        {
            try
            {
                var data = db.Orders
                    .Where(b => b.PaymentDate.HasValue && b.PaymentDate.Value.Year == year && b.PaymentDate.Value.Month == month && b.StatusOrder == 4)
                    .GroupBy(b => b.PaymentDate.Value.Day)  // Nhóm hóa đơn theo tháng
                    .Select(group => new
                    {
                        Day = group.Key,
                        Money = group.Sum(b => b.TotalAmount)
                    })
                    .OrderBy(item => item.Day)
                    .ToList();
                // Chuyển đổi dữ liệu thành định dạng phù hợp cho biểu đồ

                var chartData = new
                {
                    labels = data.Select(item => item.Day),
                    values = data.Select(item => item.Money),
                    label = "Doanh Thu Tháng " + month + " Năm " + year + "."
                };

                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }
        [HttpGet("weekly")]
        public IActionResult WeeklyStatistics()
        {
            try
            {
                // Lấy ngày hiện tại
                DateTime currentDate = DateTime.Now;

                // Lấy tất cả các ngày trong tuần qua
                var allDatesInWeek = Enumerable.Range(0, 7)
                    .Select(offset => currentDate.AddDays(-offset).Date)
                    .ToList();

                // Lấy dữ liệu từ database cho doanh thu trong 1 tuần qua
                var data = db.Orders
                    .Where(b => b.PaymentDate.HasValue && b.PaymentDate.Value >= currentDate.AddDays(-7) && b.StatusOrder == 4)
                    .GroupBy(b => b.PaymentDate.Value.Date)  // Nhóm theo ngày
                    .OrderBy(group => group.Key)
                    .Select(group => new
                    {
                        Date = group.Key,
                        Money = group.Sum(b => b.TotalAmount)
                    })
                    .ToList();

                // Đảm bảo rằng tất cả các ngày trong tuần đều có dữ liệu
                var mergedData = allDatesInWeek
                    .GroupJoin(data, date => date, group => group.Date, (date, group) => new
                    {
                        Date = date,
                        Money = group.Sum(b => b?.Money ?? 0)
                    })
                    .OrderBy(item => item.Date)
                    .ToList();

                // Chuyển đổi dữ liệu thành định dạng phù hợp cho biểu đồ
                var chartData = new
                {
                    labels = mergedData.Select(item => item.Date.ToShortDateString()), // Format ngày thành chuỗi
                    values = mergedData.Select(item => item.Money),
                    label = "Doanh Thu Tuần Qua"
                };

                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("DatePicker")]
        public IActionResult StatisticsByDatePicker(DateTime startDate, DateTime endDate)
        {
            try
            {
                var data = db.Orders
                    .Where(b => b.PaymentDate.HasValue &&
                                (b.PaymentDate.Value.Date >= startDate.Date && b.PaymentDate.Value.Date <= endDate.Date) &&
                                b.StatusOrder == 4)
                    .GroupBy(b => b.PaymentDate.Value.Date)
                    .Select(group => new
                    {
                        Dates = group.Key,
                        Moneys = group.Sum(b => b.TotalAmount)
                    })
                    .OrderBy(item => item.Dates)
                    .ToList();

                var chartData = new
                {
                    labels = data.Select(item => item.Dates),
                    values = data.Select(item => item.Moneys),
                    label = "Doanh Thu trong năm"
                };

                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private ApiData GetDataFromApi(string apiUrl)
        {
            try
            {
                // Gọi API để lấy dữ liệu
                var response = _httpClient.GetAsync(apiUrl).Result;

                // Kiểm tra xem yêu cầu đã thành công hay không
                if (response.IsSuccessStatusCode)
                {
                    // Đọc nội dung từ phản hồi
                    var content = response.Content.ReadAsStringAsync().Result;

                    // Deserializing JSON thành danh sách DataItem (sử dụng thư viện Newtonsoft.Json)
                    var apiData = JsonConvert.DeserializeObject<ApiData>(content);

                    return apiData;
                }
                else
                {
                    throw new Exception($"API request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khác nếu có
                throw new Exception("Error calling API: " + ex.Message);
            }
        }
        public class ApiData
        {
            public List<string> Labels { get; set; }
            public List<decimal> Values { get; set; }
            public string Label { get; set; }
        }
        [HttpGet("ExportDataToExcel/{label}")]
        public IActionResult ExportDataToExcel(string? currentmonth,string? currentyear, string label, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Tạo một tệp Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excelPackage = new ExcelPackage();
                var worksheet = excelPackage.Workbook.Worksheets.Add("ChartData");
                string fileName = "";
                ApiData apiData = null;  // Đặt biến ở ngoài để tránh xung đột

                if (label == "yearly")
                {
                    fileName = "Doanh_thu_" + currentyear;
                    apiData = GetDataFromApi($"https://localhost:7162/api/Chart/StatisticsByYear/" + currentyear);
                    worksheet.Cells["A1"].Value = "STT";
                    worksheet.Cells["B1"].Value = "Tháng";
                    worksheet.Cells["C1"].Value = "Doanh thu";
                }
                else if (label == "monthly")
                {
                    fileName = "Doanh_thu_thang_" + currentmonth + "_nam_" + currentyear;
                    apiData = GetDataFromApi($"https://localhost:7162/api/Chart/StatisticsByMonthOfTheYear/" + currentmonth + "/" + currentyear);
                    worksheet.Cells["A1"].Value = "STT";
                    worksheet.Cells["B1"].Value = "Ngày";
                    worksheet.Cells["C1"].Value = "Doanh thu";
                }
                else if (label == "weekly")
                {
                    fileName = "7_ngay_gan_nhat";
                    apiData = GetDataFromApi($"https://localhost:7162/api/Chart/weekly");
                    worksheet.Cells["A1"].Value = "STT";
                    worksheet.Cells["B1"].Value = "Ngày";
                    worksheet.Cells["C1"].Value = "Doanh thu";
                }
                else if (label == "DatePicker")
                {
                    fileName = "Doanh_thu_tu_ngay_"+startDate+"_den_ngay_"+endDate;
                    apiData = GetDataFromApi($"https://localhost:7162/api/Chart/DatePicker?startDate={startDate}&endDate={endDate}");
                    worksheet.Cells["A1"].Value = "STT";
                    worksheet.Cells["B1"].Value = "Ngày";
                    worksheet.Cells["C1"].Value = "Doanh thu";
                }
                else
                {
                    // Xử lý trường hợp không hỗ trợ
                    return BadRequest(new { error = "Invalid label specified" });
                }

                // Điền dữ liệu vào tệp Excel từ dữ liệu API

                for (int i = 0; i < apiData.Labels.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = i + 1;


                    if (label == "weekly")
                    {
                        worksheet.Cells[i + 2, 2].Value = apiData.Labels[i];
                    }
                    else if(label == "monthly")
                    {
                        worksheet.Cells[i + 2, 2].Value = apiData.Labels[i] + "/" + currentmonth +  "/" + currentyear;
                    }
                    else if(label == "yearly")
                    {
                        worksheet.Cells[i + 2, 2].Value = apiData.Labels[i] + "/" + currentyear;
                    }
                    else
                    {
                        worksheet.Cells[i + 2, 2].Value = apiData.Labels[i];
                    }
                    
                    worksheet.Cells[i + 2, 3].Value = apiData.Values[i];
                    worksheet.Column(i + 1).AutoFit();
                    var y = 0;
                    while(y < 3)
                    {
                        worksheet.Cells[1, y + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, y + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, y + 1].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#F5F5DC"));
                        y++;
                    }
                    // Màu cho hàng đầu tiên
                   


                }

                // Lưu tệp Excel vào một MemoryStream
                using (var memoryStream = new MemoryStream())
                {
                    excelPackage.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Trả về tệp Excel như là một phản hồi HTTP
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}_Chart_Data.xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("SoldProductsByMonth")]
        public IActionResult GetSoldProductsByMonth(string sortBy)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                int currentYear = currentDate.Year;

                var soldProductsQuery = (from products in db.Products
                                         join details in db.ProductDetails on products.Id equals details.ProductId
                                         join orderdetails in db.OrderDetails on details.Id equals orderdetails.ProductDetailId
                                         join orders in db.Orders on orderdetails.OrderId equals orders.Id                     
                                         where orders.StatusOrder == 4 && orders.PaymentDate.HasValue && orders.PaymentDate.Value >= currentDate.AddDays(-30)
                                         group new { products, details, orderdetails, orders } by new { details.Name } into grouped
                                         select new
                                         {
                                             ProductName = grouped.Key.Name,
                                             QuantitySold = grouped.Sum(x => x.orderdetails.Quantity),
                                             ProductImage = grouped.FirstOrDefault().details.Image,
                                         });

                // Sắp xếp theo số lượng bán hoặc giá tiền
                switch (sortBy)
                {
                    case "quantity":
                        soldProductsQuery = soldProductsQuery.OrderByDescending(x => x.QuantitySold);
                        break;
                    default:
                        // Mặc định sắp xếp theo số lượng bán nếu không có hoặc không hợp lệ sortBy
                        soldProductsQuery = soldProductsQuery.OrderByDescending(x => x.QuantitySold);
                        break;
                }

                var soldProducts = soldProductsQuery.Take(5).ToList();

                var responseData = soldProducts.Select(item => new
                {
                    ProductName = item.ProductName,
                    QuantitySold = item.QuantitySold,
                    ProductImage = item.ProductImage
                }).ToList();

                var response = new
                {
                    labels = responseData.Select(item => item.ProductName),
                    quantities = responseData.Select(item => item.QuantitySold),
                    images = responseData.Select(item => item.ProductImage),
                    label = "Sản Phẩm Đã Bán Tháng Này"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Customers/Count")]
        public IActionResult GetCustomers()
        {
            try
            {
                var count = db.Accounts
                    .Where(a => a.Status == 1 && a.Role.Name == "Customer")
                    .Count();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("Products/Count")]
        public IActionResult GetProducts()
        {
            try
            {
                var totalQuantity = db.Orders.Where(p => p.StatusOrder == 4).SelectMany(order => order.OrderDetails).Sum(orderDetail => orderDetail.Quantity);
                string formattedQuantity;

                if (totalQuantity > 1000000)
                {
                    formattedQuantity = $"{totalQuantity / 1000000:F2}M";
                }
                else if (totalQuantity > 1000)
                {
                    formattedQuantity = $"{totalQuantity / 1000:F2}K";
                }
                else
                {
                    formattedQuantity = totalQuantity.ToString();
                }
                return Ok(new { totalQuantity = formattedQuantity });
            }
            catch (Exception ex)
            {   
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("Revenues/Count")]
        public IActionResult GetRevenues()
        {
            try
            {
                var totalRevenue = db.Orders.Where(p => p.StatusOrder == 4 && p.PaymentDate != null).Sum(p => p.TotalAmount);
                string formatted;
                if (totalRevenue > 1000000000)
                {
                    formatted = $"{totalRevenue / 1000000000:F2}Tỷ";
                }
                else if (totalRevenue > 1000000)
                {
                    formatted = $"{totalRevenue / 1000000:F2}Tr";
                }
                else if (totalRevenue > 1000)
                {
                    formatted = $"{totalRevenue / 1000:F2}K";
                }
                else
                {
                    formatted = totalRevenue.ToString();
                }
                return Ok(new { totalRevenue = formatted });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("Countorders/Count")]
        public IActionResult Countorder()
        {
            try
            {
                var countorder = db.Orders
                    .Where(a => a.StatusOrder == 4 && a.PaymentDate != null)
                    .Count();
                string formattedCount;

                if (countorder > 1000000)
                {
                    formattedCount = $"{countorder / 1000000:F2}M";
                }
                else if (countorder > 1000)
                {
                    formattedCount = $"{countorder / 1000:F2}K";
                }
                else
                {
                    formattedCount = countorder.ToString();
                }
                return Ok(new { countorder = formattedCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("ListsStatistics/{year}")]
        public IActionResult ListsStatisticsByYear(int year)
        {
            try
            {
                var data = (from orders in db.Orders
                            join vouchers in db.Vouchers on orders.VoucherId equals vouchers.Id into voucherGroup
                            from voucher in voucherGroup.DefaultIfEmpty()
                            where orders.PaymentDate.HasValue && orders.PaymentDate.Value.Year == year && orders.StatusOrder == 4 &&
                                  (orders.PaymentDate.HasValue || voucher != null)
                            select new
                            {
                                OrderCode = orders.Code,
                                OrderCreateAt = orders.CreateAt,
                                OrderPaymentDate = orders.PaymentDate,
                                OrderShipFee = orders.ShipFee,
                                OrderVoucherName = voucher != null ? voucher.Name : null, // Handle NULL voucher.Name
                                OrderTypePayment = orders.TypePayment,
                                OrderStatus = orders.StatusOrder,
                                OrderTotalAmount = orders.TotalAmount
                            }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("ListsStatistics/{month}/{year}")]
        public IActionResult ListsStatisticsByMonth(int month,int year)
        {
            try
            {
                var data = (from orders in db.Orders
                            join vouchers in db.Vouchers on orders.VoucherId equals vouchers.Id into voucherGroup
                            from voucher in voucherGroup.DefaultIfEmpty()
                            where orders.PaymentDate.HasValue && orders.PaymentDate.Value.Month == month && orders.PaymentDate.Value.Year == year && orders.StatusOrder == 4 &&
                                  (orders.PaymentDate.HasValue || voucher != null)
                            select new
                            {
                                OrderCode = orders.Code,
                                OrderCreateAt = orders.CreateAt,
                                OrderPaymentDate = orders.PaymentDate,
                                OrderShipFee = orders.ShipFee,
                                OrderVoucherName = voucher != null ? voucher.Name : null, // Handle NULL voucher.Name
                                OrderTypePayment = orders.TypePayment,
                                OrderStatus = orders.StatusOrder,
                                OrderTotalAmount = orders.TotalAmount
                            }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
