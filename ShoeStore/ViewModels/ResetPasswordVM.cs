namespace ShoeStore.ViewModels
{
    public class ResetPasswordVM
    {
        public string Email { get; set; }
        public string ConfirmCode { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
