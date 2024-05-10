using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models;

public partial class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int? SupplierId { get; set; }

    public string? Code { get; set; }

    public string? Description { get; set; }

    public string? Detail { get; set; }

    public int? ViewCount { get; set; }

    public bool Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
