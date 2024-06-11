namespace ShoeStore.ViewModels
{
    public class RevenueListVM_Excel
    {
        public string? Code { get; set; }
        public string? CreateAt { get; set; }
        public string? PaymentDate { get; set; }
        public decimal? ShipFee { get; set; }
        public string? VoucherName { get; set; }

        public string? TypePayment {  get; set; }
        public string? StatusOrder { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
