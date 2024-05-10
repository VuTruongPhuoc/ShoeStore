using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels
{
    public class ResetPass
    {

        [Display(Name = "Nhập email")]
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 ký tự")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
