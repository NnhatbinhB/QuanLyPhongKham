using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyPhongKham.Data
{
    public static class HoaDonRepository
    {
        /// <summary>
        /// Lấy giá tiền 1 ngày 1 giường từ bảng ThamSo
        /// </summary>
        public static int GetTien1Ngay1Giuong()
        {
            var dt = Database.Query("sp_selectThamSo", CommandType.StoredProcedure)
                     ?? new DataTable();

            if (dt.Rows.Count == 0) return 0;

            var obj = dt.Rows[0]["Tien1Ngay1Giuong"];
            if (obj == null || obj == DBNull.Value) return 0;

            return Convert.ToInt32(obj);
        }

        /// <summary>
        /// Lấy ngày xếp giường của bệnh nhân nằm giường (sttBNNG_DXC)
        /// </summary>
        public static DateTime GetNgayXep(int sttBNNG_DXC)
        {
            const string sql = @"
SELECT ngayxep
FROM   BenhNhanNamGiuong_DaXepCho
WHERE  sttBNNG_DXC = @id;";

            var dt = Database.Query(sql, CommandType.Text,
                new SqlParameter("@id", sttBNNG_DXC)) ?? new DataTable();

            if (dt.Rows.Count == 0) return DateTime.Today;

            return Convert.ToDateTime(dt.Rows[0]["ngayxep"]);
        }

        /// <summary>
        /// Tổng tiền thuốc CHƯA THANH TOÁN của bệnh nhân nằm giường
        /// (Join DonThuoc + DonThuoc_BenhNhanNamGiuong_DaXepCho)
        /// </summary>
        public static int GetTongTienThuocChuaThanhToan(int sttBNNG_DXC)
        {
            const string sql = @"
SELECT ISNULL(SUM(dt.TienThuoc), 0) AS TongTien
FROM DonThuoc_BenhNhanNamGiuong_DaXepCho dt_bnng
JOIN DonThuoc dt ON dt.sttDT = dt_bnng.sttDT
WHERE dt_bnng.MaBenhNhanNamGiuong_DaXepCho = @id
  AND dt_bnng.TinhTrang = N'Chưa thanh toán';";

            var dt = Database.Query(sql, CommandType.Text,
                new SqlParameter("@id", sttBNNG_DXC)) ?? new DataTable();

            if (dt.Rows.Count == 0) return 0;

            return Convert.ToInt32(dt.Rows[0]["TongTien"]);
        }

        /// <summary>
        /// List những bệnh nhân nằm giường CÓ đơn thuốc chưa thanh toán
        /// (dùng cho FrmHoaDon, để lập hóa đơn)
        /// </summary>
        public static DataTable GetBenhNhanNamGiuong_CoThuocChuaThanhToan()
        {
            const string sql = @"
SELECT DISTINCT
       bnng.sttBNNG_DXC,
       bnng.sttBN      AS [Mã Bệnh Nhân],
       bn.hoten        AS [Họ Tên Bệnh Nhân],
       bn.CMND,
       bn.diachi       AS [Địa Chỉ],
       bnng.sttGB      AS [Giường],
       bnng.ngayxep    AS [Ngày Xếp]
FROM BenhNhanNamGiuong_DaXepCho bnng
JOIN BenhNhan bn
     ON bn.sttBN = bnng.sttBN
WHERE EXISTS (
    SELECT 1
    FROM DonThuoc_BenhNhanNamGiuong_DaXepCho dt_bnng
    WHERE dt_bnng.MaBenhNhanNamGiuong_DaXepCho = bnng.sttBNNG_DXC
      AND dt_bnng.TinhTrang = N'Chưa thanh toán'
);";

            return Database.Query(sql, CommandType.Text) ?? new DataTable();
        }

        /// <summary>
        /// Lập hóa đơn mới
        /// </summary>
        public static void InsertHoaDon(int sttBNNG_DXC, int thanhTien)
        {
            Database.Execute(
                "sp_InsertHoaDon",
                CommandType.StoredProcedure,
                new SqlParameter("@sttBNNG_DXC", sttBNNG_DXC),
                new SqlParameter("@ThanhTien", thanhTien)
            );
        }

        /// <summary>
        /// Đánh dấu tất cả đơn thuốc của bệnh nhân này đã thanh toán
        /// (sau khi lập hóa đơn)
        /// </summary>
        public static void MarkThuocDaThanhToan(int sttBNNG_DXC)
        {
            const string sql = @"
UPDATE DonThuoc_BenhNhanNamGiuong_DaXepCho
SET    TinhTrang = N'Đã thanh toán'
WHERE  MaBenhNhanNamGiuong_DaXepCho = @id
  AND  TinhTrang = N'Chưa thanh toán';";

            Database.Execute(sql, CommandType.Text, new SqlParameter("@id", sttBNNG_DXC));
        }
    }
}
