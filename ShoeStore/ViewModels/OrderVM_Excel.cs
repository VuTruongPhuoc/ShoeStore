namespace ShoeStore.ViewModels
{
	public class OrderVM_Excel
    {
		public int Id { get; set; }
		
		public string CustomerName { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }

		public string? Code { get; set; }

		
		public string? VoucherName { get; set; }

		public decimal? ShipFee { get; set; }

		public decimal? TotalAmount { get; set; }
		public string? StatusOrder { get; set; }
		public string? Address { get; set; }
		public string? TypePayment { get; set; }
		public string? Note { get; set; }
		public string? CreateAt { get; set; }
		public string? UpdateAt { get; set; }
		public string? PaymentDate { get; set; }
	}
}
