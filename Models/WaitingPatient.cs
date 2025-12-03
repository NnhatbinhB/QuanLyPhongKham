using System;

namespace QuanLyPhongKham.Models
{
    /// <summary>
    /// Bệnh nhân nằm giường nhưng CHƯA xếp chỗ.
    /// </summary>
    public class WaitingPatient
    {
        public int SttBN { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string CMND { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string BacSi { get; set; } = string.Empty;

        // Khoa (nếu có)
        public int? KhoaId { get; set; }
        public string? TenKhoa { get; set; }
    }
}
