using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models;

public partial class Vouchers
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage ="Vui lòng nhập giá trị")]
    [Range(1, 50, ErrorMessage = "Giá trị % phải lớn hơn 0 và nhỏ hơn 50")]
    public int Value { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên voucher")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mã voucher")]
    [StringLength(6, MinimumLength = 2, ErrorMessage = "Mã phải có từ 2 đến 6 ký tự")]
    public string Code { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập số lượng")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }
    public bool Status { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập giảm tối đa")]
    [Range(0, double.MaxValue, ErrorMessage = "Giảm tối đa không được âm")]
    public decimal DiscountAmount { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu")]
    public DateTime StartDate { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập ngày kết thúc")]
    public DateTime EndDate { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
