using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class VoucherForAcc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Status { get; set; }
        [ForeignKey("Account")]
        public int IdAccount { get; set; }
        [ForeignKey("Voucher")]
        public int IdVoucher { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public double DiscountAmount { get; set; }
        public DateTime EndDate { get; set; }
        public virtual Voucher? Voucher { get; set; }
        public virtual Account? Account { get; set; }
        public List<Order>? Order { get; set; }
    }
}
