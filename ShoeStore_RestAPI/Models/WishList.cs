using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models;

public partial class WishList
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("ProductDetail")]
    public int? ProductDetailId { get; set; }

    public int? AccountId { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ProductDetail? Productdetail { get; set; }
}
