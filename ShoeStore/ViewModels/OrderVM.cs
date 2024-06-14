using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShoeStore.Models;
namespace ShoeStore.ViewModels
{
    public class OrderVM
    {
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }       
        public string? TypePayment { get; set; }
        public string? Note { get; set; }
        public decimal ? ShipFee { get; set; }
        public decimal ? discountValue { get; set; }
        public string ? VoucherCode { get; set; }
        public decimal ? totalAmount { get; set; }

        public bool LikeCustomers {  get; set; }
        public List<VoucherForAcc>? VoucherForAccs { get; set; } = new List<VoucherForAcc>();   
    }
}
