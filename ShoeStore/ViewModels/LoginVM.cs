using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels
{
    public class LoginVM
    {
        [Display(Name = "Tài khoản")]
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 ký tự")]
        public string? Email { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "*")]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
