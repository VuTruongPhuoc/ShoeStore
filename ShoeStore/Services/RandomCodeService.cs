namespace ShoeStore.Services
{
    public class RandomCodeService
    {
        public static string GenerateRandomCode(int length)
        {
            const string chars = "0123456789"; // Các ký tự sẽ được sử dụng để tạo mã xác nhận
            var random = new Random();

            // Tạo mã xác nhận ngẫu nhiên bằng cách chọn ngẫu nhiên từ chuỗi ký tự đã cho
            var verificationCode = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return verificationCode;
        }
    }
}
