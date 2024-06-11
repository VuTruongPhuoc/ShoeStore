using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models;

public partial class News
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [StringLength(150)]
    public string? Title { get; set; }
	public string? Postedby { get; set; }

	[Required(ErrorMessage = "Vui lòng nhập mô tả")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Vui lòng nhập chi tiết")]
    public string? Detail { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn ảnh")]
    public string? Image { get; set; }
    public DateTime? CreateAt { get; set; } 

    public DateTime? UpdateAt { get; set; }

    public bool Status { get; set; }
}
