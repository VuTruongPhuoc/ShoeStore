using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels
{
	public class ProductVM_Excel
	{
		public int STT {  get; set; }
		public int Id { get; set; }

		[Display(Name= "Tên sản phẩm")]
		public string ProductName { get; set; } = null!;

		public string? CategoryName { get; set; }

		public string? SupplierName { get; set; }

		public string? ProductCode { get; set; }

		public string? Description { get; set; }

		public int? ViewCount { get; set; }

		public string Status { get; set; }

		public string? CreateAt { get; set; }

		public string? UpdateAt { get; set; }

	}
}
