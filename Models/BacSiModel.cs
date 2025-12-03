namespace QuanLyPhongKham.Models
{
    public class BacSiModel
    {
        public int Id { get; set; }
        public string TenBS { get; set; }
        public string Username { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }

        public BacSiModel()
        {
            TenBS = string.Empty;
            Username = string.Empty;
            DiaChi = string.Empty;
            DienThoai = string.Empty;
        }

        public override string ToString()
        {
            return $"{TenBS} ({Username})";
        }
    }
}
