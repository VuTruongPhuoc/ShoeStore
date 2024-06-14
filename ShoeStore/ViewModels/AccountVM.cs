using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels
{
    public class AccountVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int AccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string? Username { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên đầy đủ")]
        public string? FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [MaxLength(24, ErrorMessage = "Tối đa 24 ký tự")]
        [RegularExpression(@"0[987654321]\d{8}", ErrorMessage = "Chưa đúng định dạng số điện thoại")]
        public string? PhoneNumber { get; set; }
        public string? SpecificAddress { get; set; }
        public string? Ward { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
        public string? Address { get; set; }
        public int? Status { get; set; }
    }
}
