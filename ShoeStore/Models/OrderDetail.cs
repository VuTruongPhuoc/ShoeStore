using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int ProductDetailId { get; set; }

        public decimal? Price { get; set; }

        public int Quantity { get; set; }

        public double? Discount { get; set; }

        public int OrderId { get; set; } 

        public virtual Order Order { get; set; }

        public virtual ProductDetail ProductDetail { get; set; }
    }

}
