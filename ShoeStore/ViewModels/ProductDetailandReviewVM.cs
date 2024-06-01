using ShoeStore.Models;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels
{
	public class ProductDetailandReviewVM
	{
		public int? Id { get; set; }
		public string? Name { get; set; }
		public int? ProductId { get; set; }
		public decimal? OriginalPrice { get; set; }
		public decimal? Price { get; set; }
		public decimal? PriceSale { get; set; }
		public string? Image { get; set; }
		public int? Quantity { get; set; }
		public int? SizeId { get; set; }
		public int? ColorId { get; set; }

		public bool? Status { get; set; }
		public virtual Product? Product { get; set; }

		public List<Review> Reviews { get; set; } = new List<Review>();
		public string? Username { get; set; }

		[EmailAddress(ErrorMessage = "Vui lòng nhập đúng định dạng email")]
		public string? Email { get; set; }

		public int? ProductDetailId { get; set; }

		public string? Content { get; set; }

		public int? Rate { get; set; }

		public DateTime? CreateAt { get; set; }
	}
}
