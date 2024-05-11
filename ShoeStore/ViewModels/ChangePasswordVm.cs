namespace ShoeStore.ViewModels
{
    public class ChangePasswordVM
    {
        public int  UserId { get; set; }
        public string OldPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string NewPassWord { get; set; }
    }
}
