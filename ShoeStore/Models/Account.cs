﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using NuGet.Protocol.Plugins;

namespace ShoeStore.Models;

public partial class Account
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int RoleId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    public string? Username { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string? Password { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên đầy đủ")]
    public string? FullName { get; set; }
    //[Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [MaxLength(24, ErrorMessage = "Tối đa 24 ký tự")]
    [RegularExpression(@"0[987563241]\d{8}", ErrorMessage = "Chưa đúng định dạng số điện thoại")]
    public string? PhoneNumber { get; set; }  
    public string? Address { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
    public string Email { get; set; }
    public string? ResetPasswordcode { get; set; }
    public int? Status { get; set; }
    public string? RandomKey { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public Role? Role { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
