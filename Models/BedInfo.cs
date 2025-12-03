using System;

namespace QuanLyPhongKham.Models
{
    public class BedInfo
    {
        // Mã giường: sttGB
        public int Id { get; set; }

        // Tình trạng trong bảng GiuongBenh.tinhtrang
        public string TinhTrang { get; set; } = string.Empty;

        // Tên bệnh nhân đang nằm (lấy từ BenhNhan.hoten, có thể null)
        public string? BenhNhan { get; set; }
        public string TenGiuong { get; set; } = "";
        public int? KhoaId { get; set; }
        public int? MaKhoa { get; set; }
        public string?TenKhoa { get; set; }


        }
}
