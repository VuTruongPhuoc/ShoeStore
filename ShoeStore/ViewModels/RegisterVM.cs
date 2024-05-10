using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels
{
    public class RegisterVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "*")]
        [MaxLength(20, ErrorMessage = "Tối đa 30 ký tự")]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "*")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        [Required(ErrorMessage = "*")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 ký tự")]
        public string FullName { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "*")]
        public string Address { get; set; }
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "*")]
        [MaxLength(24, ErrorMessage = "Tối đa 24 ký tự")]
        [RegularExpression(@"0[9875]\d{8}", ErrorMessage = "Chưa đúng định dạng")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "*")]
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
        public string Email { get; set; } = null!;
    }
}
