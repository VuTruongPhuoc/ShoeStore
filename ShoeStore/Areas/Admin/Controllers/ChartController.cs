using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly ShoeStoreContext db;
        //private readonly HttpClient _httpClient;
        public ChartController(ShoeStoreContext db)
        {
            this.db = db;
        }
        [HttpGet("{year}")]
        public IActionResult StatisticsByYear(int year)
        {
            try
            {
                var data = db.Orders
                    .Where(b => b.CreateAt.HasValue && b.CreateAt.Value.Year == year && b.StatusOrder == 1)
                    .GroupBy(b => b.CreateAt.Value.Month)  // Nhóm hóa đơn theo tháng
                    .Select(group => new
                    {
                        Month = group.Key,
                        Money = group.Sum(b => b.TotalAmount)
                    })
                    .OrderBy(item => item.Month)
                    .ToList();

                // Chuyển đổi dữ liệu thành định dạng phù hợp cho biểu đồ
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

        [HttpGet("{month}/{year}")]
        public IActionResult StatisticsByMonthOfTheYear(int month, int year)
        {
            try
            {
                var data = db.Orders
                    .Where(b => b.CreateAt.HasValue && b.CreateAt.Value.Year == year && b.CreateAt.Value.Month == month && b.StatusOrder == 1)
                    .GroupBy(b => b.CreateAt.Value.Day)  // Nhóm hóa đơn theo tháng
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
                    .Where(b => b.CreateAt.HasValue && b.CreateAt.Value >= currentDate.AddDays(-7) && b.StatusOrder == 1)
                    .GroupBy(b => b.CreateAt.Value.Date)  // Nhóm theo ngày
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
                    .Where(b => b.CreateAt.HasValue &&
                                (b.CreateAt.Value.Date >= startDate.Date && b.CreateAt.Value.Date <= endDate.Date) &&
                                b.StatusOrder == 1)
                    .GroupBy(b => b.CreateAt.Value.Date)
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
                                         join images in db.ProductImages on details.Id equals images.ProductDetailId
                                         where orders.StatusOrder == 1 && orders.CreateAt.HasValue && orders.CreateAt.Value >= currentDate.AddDays(-30)
                                         group new { products, details, orderdetails, orders, images } by new { products.Name } into grouped
                                         select new
                                         {
                                             ProductName = grouped.Key.Name,
                                             QuantitySold = grouped.Sum(x => x.orderdetails.Quantity),
                                             TotalEarnings = grouped.Sum(x => x.orders.TotalAmount),
                                             ProductImage = grouped.FirstOrDefault().images.Image,
                                         });

                // Sắp xếp theo số lượng bán hoặc giá tiền
                switch (sortBy)
                {
                    case "quantity":
                        soldProductsQuery = soldProductsQuery.OrderByDescending(x => x.QuantitySold);
                        break;
                    case "earnings":
                        soldProductsQuery = soldProductsQuery.OrderByDescending(x => x.TotalEarnings);
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
                    TotalEarnings = item.TotalEarnings,
                    ProductImage = item.ProductImage
                }).ToList();

                var response = new
                {
                    labels = responseData.Select(item => item.ProductName),
                    quantities = responseData.Select(item => item.QuantitySold),
                    earnings = responseData.Select(item => item.TotalEarnings),
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
                var totalQuantity = db.Orders.Where(p => p.StatusOrder == 1).SelectMany(order => order.OrderDetails).Sum(orderDetail => orderDetail.Quantity);
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
                var totalRevenue = db.Orders.Where(p => p.StatusOrder == 1 && p.CreateAt != null).Sum(p => p.TotalAmount);
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
                    .Where(a => a.StatusOrder == 1 && a.CreateAt != null)
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
    }
}
