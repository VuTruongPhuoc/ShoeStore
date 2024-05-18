using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? AccountId { get; set; }

        public int? VoucherId { get; set; }

        public string CustomerName { get; set; }
        public string Phone {  get; set; }
        public string Email { get; set; }

        public string? Code { get; set; }

        public string? Address { get; set; }

        public int? StatusOrder { get; set; }

        public decimal? ShipFee { get; set; }

        public decimal? TotalAmount { get; set; }

        public string? TypePayment { get; set; }
        public string? Note { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public virtual Account? Account { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual Vouchers? Voucher { get; set; }
    }
}
