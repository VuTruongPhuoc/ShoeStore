namespace ShoeStore.ViewModels
{
	public class ShoppingCart
	{
		public List<ShoppingCartItem> Items { get; set; }

	}

	public class ShoppingCartItem
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string CategoryName { get; set; }
		public string ProductImg { get; set; }
		public int Quantity { get; set; }
		public string Size {  get; set; }
		public decimal Price { get; set; }
		public decimal TotalPrice { get; set; }
	}
}
