﻿using OfficeOpenXml;
using System.Drawing;

namespace ShoeStore.Services
{

    public class ExcelHandler : IExcelHandler
    {
        public ExcelHandler()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
        public async Task<byte[]> Export<T>(List<T> dataItems) where T : class, new()
        {
            if (!dataItems.Any())
            {
                return default;
            }
            var memoryStream = new MemoryStream();
            using (var package = new ExcelPackage(memoryStream))
            {
                var workbook = package.Workbook.Worksheets.Add("ReportData");

                T obj = new T();

                var properties = obj.GetType().GetProperties();

                for (int row = 0; row < dataItems.Count(); row++)
                {
                    for (int col = 0; col < properties.Count(); col++)
                    {
                        var rowData = dataItems[row];

                        workbook.Cells[row + 2, col + 1].Value = rowData.GetType().GetProperty(properties[col].Name).GetValue(rowData);
                    }
                }
                for (int i = 0; i < properties.Count(); i++)
                {
                    workbook.Cells[1, i + 1].Value = properties[i].Name switch
					{
                        //order
						"Id" => "Mã", // Thay đổi tiêu đề cột từ "Id" sang "Mã"
                        "CustomerName" => "Tên khách hàng",
                        "Phone" => "Số điện thoại",
                        "Code" => "Mã đơn hàng",
                        "VoucherName" => "Tên voucher",
                        "ShipFee" => "Phí ship",
                        "TotalAmount" => "Tổng tiền",
                        "StatusOrder" => "Trạng thái đơn hàng",
                        "Address" => "Địa chỉ",
                        "TypePayment" => "Phương thức thanh toán",
                        "Note" => "Ghi chú",
                        "PaymentDate" => "Ngày thanh toán",
                        //proudct
                        "ProductName" => "Tên sản phẩm",
                        "CategoryName" => "Tên danh mục",
                        "SupplierName" => "Tên NCC",
                        "ProductCode" => "ProductCode",
                        "Description" => "Mô tả",
                        "ViewCount" => "Lượt xem",                     
						//productdetail
						"ProductDetailId" => "Mã chi tiết sản phẩm",
                        "Price" => "Giá sản phẩm",
                        "PriceSale" => "Giá khuyến mãi",
                        "ProductDetailName" => "Tên sản phẩm chi tiết",
                        "Quantity" => "Số lượng",
                        "SizeName" => "Tên kích cỡ",
                        "ColorName" => "Tên màu",
                        //inventory

						//shared
                        "Status" => "Trạng thái",
						"CreateAt" => "Ngày tạo",
						"UpdateAt" => "Ngày cập nhật",
						_ => properties[i].Name
					};
					workbook.Column(i + 1).AutoFit();
                    workbook.Cells[1, i + 1].Style.Font.Bold = true;
                    workbook.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    workbook.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#F5F5DC"));
				}
                await package.SaveAsync();
            }
            return memoryStream.ToArray();
        }
    }
    public interface IExcelHandler
    {
        Task<byte[]> Export<T>(List<T> dataItems) where T : class, new();
    }
}
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using OfficeOpenXml;
//using OfficeOpenXml.Style;

//namespace ShoeStore.Services
//{
//    public class ExcelHandler : IExcelHandler
//    {
//        public ExcelHandler()
//        {
//            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//        }

//        public async Task<byte[]> Export<T>(IEnumerable<T> dataItems) where T : class, new()
//        {
//            var dataList = dataItems.ToList();

//            if (!dataList.Any())
//            {
//                return default;
//            }

//            var stream = new MemoryStream();
//            using (var package = new ExcelPackage(stream))
//            {
//                var workbook = package.Workbook.Worksheets.Add("ReportData");

//                // Lấy danh sách các thuộc tính từ phần tử đầu tiên (nếu có)
//                var properties = dataList.FirstOrDefault()?.GetType().GetProperties();

//                // Kiểm tra xem properties có null hay không
//                if (properties != null)
//                {
//                    // Duyệt qua các thuộc tính để đặt tên cột
//                    for (int i = 0; i < properties.Length; i++)
//                    {
//                        workbook.Cells[1, i + 1].Value = properties[i].Name;
//                        workbook.Column(i + 1).AutoFit();
//                        workbook.Cells[1, i + 1].Style.Font.Bold = true;
//                        workbook.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
//                        workbook.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#000"));
//                    }

//                    // Duyệt qua mỗi đối tượng và thuộc tính của nó để điền dữ liệu vào bảng
//                    for (int row = 0; row < dataList.Count; row++)
//                    {
//                        var rowData = dataList[row];

//                        for (int col = 0; col < properties.Length; col++)
//                        {
//                            workbook.Cells[row + 2, col + 1].Value = properties[col].GetValue(rowData);
//                        }
//                    }
//                }

//                await package.SaveAsync();
//            }

//            return stream.ToArray();
//        }
//    }

//    public interface IExcelHandler
//    {
//        Task<byte[]> Export<T>(IEnumerable<T> dataItems) where T : class, new();
//    }
//}

