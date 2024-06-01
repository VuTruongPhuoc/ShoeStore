namespace ShoeStore.ViewModels
{
	public class FilterDataVM
	{
		public List<string> Categories { get; set; } = new List<string>();
		public List<string> Colors { get; set; } = new List<string>();
		public List<string> Sizes { get; set; } = new List<string>();
		public List<string> PriceRanges { get; set; } = new List<string>();
		public class PriceRange
		{
			public int Min { get; set; }
			public int Max { get; set; }
		}
	}
}
