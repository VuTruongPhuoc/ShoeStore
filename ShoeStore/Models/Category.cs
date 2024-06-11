using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models;

public partial class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập bí danh")]
    [StringLength(150)]
    public string Alias { get; set; }
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ảnh")]
    [StringLength(250)]
    public string Image { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
