using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models;

public partial class Supplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhà cung cấp")]
    public string? PhoneNumber { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhà cung cấp")]
    public string? Address { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
