namespace ShoeStore.ViewModels
{
    public class SendEmailOrderVM
    {
        public int OrderID { get; set; }    
        public string OrderCode { get; set; }
        public string strSanPham {  get; set; }
        public DateTime NgayDat { get; set; }
        public string TenKhachHang { get; set; }
        public string Phone {  get; set; }
        public string Email { get; set; }
        public string DiaChiNhanHang { get; set; }
        public decimal ThanhTien {  get; set; }
        public decimal? GiamGia {  get; set; }
        public string Voucher { get; set; }
        public decimal? PhiShip { get; set; }
        public decimal? TongTien { get; set; }

        public string HinhThucThanhToan { get; set; }
    }
}
