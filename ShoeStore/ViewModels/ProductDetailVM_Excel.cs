using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels
{
	public class ProductDetailVM_Excel
	{
		public int ProductDetailId { get; set; }
		public string? ProductName { get; set; }
		public decimal Price { get; set; }
		public decimal? PriceSale { get; set; }
		public string? ProductDetailName { get; set; }
		public int Quantity { get; set; }
		public string? SizeName { get; set; }
		public string? ColorName { get; set; }
		public string Status { get; set; }
		public string? CreateAt { get; set; }
		public string? UpdateAt { get; set; }
	}
}
