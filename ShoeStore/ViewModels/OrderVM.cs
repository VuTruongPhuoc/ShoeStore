using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ShoeStore.ViewModels
{
    public class OrderVM
    {
        [Required(ErrorMessage = "Tên khách hàng không để trống")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Số điện thoại không để trống")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Email không để trống")]
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Địa chỉ không để trống")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Thành phố không để trống")]
        public string City { get; set; }
        [Required(ErrorMessage = "Quận huyện không để trống")]
        public string District { get; set; }
        [Required(ErrorMessage = "Xã không để trống")]
        public string Ward { get; set; }
       
        public int TypePayment { get; set; }
        public string? Note { get; set; }
    }
}
