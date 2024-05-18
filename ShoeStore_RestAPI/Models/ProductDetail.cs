using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models;

public partial class ProductDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? ProductId { get; set; }
    
    public decimal OriginalPrice { get; set; }
    [DisplayFormat(DataFormatString = "{0:#,###.00}", ApplyFormatInEditMode = true)]
    public decimal Price { get; set; }
    public decimal? PriceSale { get; set; }
    public string? Image { get; set; }
    public int Quantity { get; set; }
    public int? SizeId { get; set; }
    public int? ColorId { get; set; }
    public bool Status { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public virtual Color? Color { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
    public virtual Product? Product { get; set; }

    public virtual Size? Size { get; set; }
}
