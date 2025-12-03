using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Data
{
    /// <summary>
    /// Toàn bộ truy vấn liên quan giường bệnh.
    /// </summary>
    public static class BedRepository
    {
        // -------------------------------------------------
        // Lấy toàn bộ giường bệnh + tên bệnh nhân (nếu có)
        // -------------------------------------------------
        public static List<BedInfo> GetAllBeds()
        {
            var beds = new List<BedInfo>();
            DataTable dt;

            // Thử query có Khoa (nếu em đã thêm bảng Khoa + cột sttKhoa)
            const string sqlWithKhoa = @"
SELECT gb.sttGB,
       gb.tinhtrang,
       bn.hoten AS TenBenhNhan,
       gb.sttKhoa,
       k.TenKhoa
FROM GiuongBenh gb
LEFT JOIN BenhNhanNamGiuong_DaXepCho dxc
       ON gb.sttGB = dxc.sttGB
LEFT JOIN BenhNhan bn
       ON dxc.sttBN = bn.sttBN
LEFT JOIN Khoa k
       ON gb.sttKhoa = k.sttKhoa
ORDER BY gb.sttGB;";

            // Fallback: nếu chưa có bảng Khoa thì dùng query đơn giản
            const string sqlSimple = @"
SELECT gb.sttGB,
       gb.tinhtrang,
       bn.hoten AS TenBenhNhan
FROM GiuongBenh gb
LEFT JOIN BenhNhanNamGiuong_DaXepCho dxc
       ON gb.sttGB = dxc.sttGB
LEFT JOIN BenhNhan bn
       ON dxc.sttBN = bn.sttBN
ORDER BY gb.sttGB;";

            try
            {
                dt = Database.Query(sqlWithKhoa, CommandType.Text);
            }
            catch
            {
                dt = Database.Query(sqlSimple, CommandType.Text);
            }

            foreach (DataRow row in dt.Rows)
            {
                var info = new BedInfo
                {
                    Id = Convert.ToInt32(row["sttGB"]),
                    TinhTrang = row["tinhtrang"]?.ToString(),
                    BenhNhan = row.Table.Columns.Contains("TenBenhNhan")
                                   ? row["TenBenhNhan"]?.ToString()
                                   : null
                };

                if (row.Table.Columns.Contains("sttKhoa") && row["sttKhoa"] != DBNull.Value)
                    info.KhoaId = Convert.ToInt32(row["sttKhoa"]);

                if (row.Table.Columns.Contains("TenKhoa"))
                    info.TenKhoa = row["TenKhoa"]?.ToString();

                beds.Add(info);
            }

            return beds;
        }
        public static List<BedInfo> GetBedsByKhoa(int maKhoa)
        {
            const string sql = @"
        SELECT g.sttGB,
               g.TenGiuong,
               g.tinhtrang,
               ISNULL(bn.hoten, N'') AS BenhNhan
        FROM   GiuongBenh g
        LEFT JOIN BenhNhanNamGiuong_DaXepCho bnng
               ON bnng.sttGB = g.sttGB
        LEFT JOIN BenhNhan bn
               ON bn.sttBN = bnng.sttBN
        WHERE  g.MaKhoa = @maKhoa";

            var dt = Database.Query(sql, CommandType.Text,
                new SqlParameter("@maKhoa", maKhoa)) ?? new DataTable();

            var list = new List<BedInfo>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new BedInfo
                {
                    Id = Convert.ToInt32(r["sttGB"]),
                    TenGiuong = r["TenGiuong"].ToString() ?? "",
                    TinhTrang = r["tinhtrang"]?.ToString(),
                    BenhNhan = r["BenhNhan"]?.ToString(),
                    MaKhoa = maKhoa
                });
            }
            return list;
        }

        // -------------------------------------------------
        // Lấy danh sách BN nằm giường CHƯA xếp chỗ
        // -------------------------------------------------
        public static List<WaitingPatient> GetWaitingPatients()
        {
            var list = new List<WaitingPatient>();
            DataTable dt;

            const string sqlWithKhoa = @"
SELECT c.sttBN,
       bn.hoten,
       bn.CMND,
       bn.diachi,
       bs.tenBS AS BacSi,
       bn.sttKhoa,
       k.TenKhoa
FROM BenhNhanNamGiuong_ChuaXepCho c
JOIN BenhNhan bn ON c.sttBN = bn.sttBN
JOIN BacSi bs   ON bn.BSphutrach = bs.sttBS
LEFT JOIN Khoa k ON bn.sttKhoa = k.sttKhoa
ORDER BY c.sttBN;";

            const string sqlSimple = @"
SELECT c.sttBN,
       bn.hoten,
       bn.CMND,
       bn.diachi,
       bs.tenBS AS BacSi
FROM BenhNhanNamGiuong_ChuaXepCho c
JOIN BenhNhan bn ON c.sttBN = bn.sttBN
JOIN BacSi bs   ON bn.BSphutrach = bs.sttBS
ORDER BY c.sttBN;";

            try
            {
                dt = Database.Query(sqlWithKhoa, CommandType.Text);
            }
            catch
            {
                dt = Database.Query(sqlSimple, CommandType.Text);
            }

            foreach (DataRow row in dt.Rows)
            {
                var p = new WaitingPatient
                {
                    SttBN = Convert.ToInt32(row["sttBN"]),
                    HoTen = row["hoten"]?.ToString() ?? string.Empty,
                    CMND = row["CMND"]?.ToString() ?? string.Empty,
                    DiaChi = row["diachi"]?.ToString() ?? string.Empty,
                    BacSi = row["BacSi"]?.ToString() ?? string.Empty
                };

                if (row.Table.Columns.Contains("sttKhoa") && row["sttKhoa"] != DBNull.Value)
                    p.KhoaId = Convert.ToInt32(row["sttKhoa"]);
                if (row.Table.Columns.Contains("TenKhoa"))
                    p.TenKhoa = row["TenKhoa"]?.ToString();

                list.Add(p);
            }

            return list;
        }

        // -------------------------------------------------
        // Thêm giường mới (nếu có chọn Khoa thì gán Khoa)
        // -------------------------------------------------
        public static void InsertNewBed(string tinhTrang, int? khoaId = null)
        {
            if (khoaId.HasValue)
            {
                const string sql = "INSERT INTO GiuongBenh(tinhtrang, sttKhoa) VALUES(@tt, @khoa)";
                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@tt", tinhTrang),
                    new SqlParameter("@khoa", khoaId.Value));
            }
            else
            {
                const string sql = "INSERT INTO GiuongBenh(tinhtrang) VALUES(@tt)";
                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@tt", tinhTrang));
            }
        }

        // -------------------------------------------------
        // Kiểm tra giường đang có BN hay không
        // -------------------------------------------------
        public static bool BedHasPatient(int bedId)
        {
            const string sql = "SELECT COUNT(*) AS Cnt FROM BenhNhanNamGiuong_DaXepCho WHERE sttGB = @id";
            var dt = Database.Query(sql, CommandType.Text,
                new SqlParameter("@id", bedId));

            if (dt.Rows.Count == 0) return false;
            var cntObj = dt.Rows[0]["Cnt"];
            int cnt = 0;
            if (cntObj != null && cntObj != DBNull.Value)
                int.TryParse(cntObj.ToString(), out cnt);
            return cnt > 0;
        }

        // -------------------------------------------------
        // Xóa giường
        // -------------------------------------------------
        public static void DeleteBed(int bedId)
        {
            const string sql = "DELETE FROM GiuongBenh WHERE sttGB = @id";
            Database.Execute(sql, CommandType.Text,
                new SqlParameter("@id", bedId));
        }

        // -------------------------------------------------
        // Cập nhật trạng thái giường
        // -------------------------------------------------
        public static void UpdateBedStatus(int bedId, string tinhTrang)
        {
            const string sql = "UPDATE GiuongBenh SET tinhtrang = @tt WHERE sttGB = @id";
            Database.Execute(sql, CommandType.Text,
                new SqlParameter("@tt", tinhTrang),
                new SqlParameter("@id", bedId));
        }

        // -------------------------------------------------
        // Xếp giường cho bệnh nhân (gọi đúng SP em đã có)
        // -------------------------------------------------
        public static void AssignBedToPatient(int sttBN, int sttGB, DateTime ngayXep)
        {
            Database.Execute("sp_InsertBenhNhanNamGiuong_DaXepCho",
                CommandType.StoredProcedure,
                new SqlParameter("@sttBN", sttBN),
                new SqlParameter("@sttGB", sttGB),
                new SqlParameter("@ngayxep", ngayXep));
        }
    }
}