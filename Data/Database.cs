using System;
using System.Data;
using System.Configuration;                // đọc connection string từ App.config
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Data
{
    public static class Database
    {
        // Ưu tiên lấy từ App.config (ClinicDb). Nếu không có thì dùng chuỗi mặc định bên dưới.
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["ClinicDb"]?.ConnectionString
            ?? "Server=DESKTOP-AG5CQP2\\MSSQLSERVER2;Database=QuanLyPhongKham;User Id=sa;Password=123;TrustServerCertificate=True;";

        // ================== CORE ==================       
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static DataTable Query(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = type;

                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                return dt;
            }
        }

        public static int Execute(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = type;

                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // ================== LOGIN (dùng bảng TaiKhoan) ==================
        public static BacSiModel? Login(string username, string password)
        {
            const string sql = @"
SELECT  tk.MaTK,
        tk.Username,
        tk.VaiTro,
        tk.sttBS,
        tk.HoTen,
        bs.tenBS,
        bs.DiaChi,
        bs.DienThoai
FROM    TaiKhoan tk
LEFT JOIN BacSi bs ON tk.sttBS = bs.sttBS
WHERE   tk.Username = @username
    AND tk.[Password] = @password
    AND tk.IsActive = 1;";

            var dt = Query(
                sql,
                CommandType.Text,
                new SqlParameter("@username", username),
                new SqlParameter("@password", password));

            if (dt.Rows.Count == 0)
                return null;

            var row = dt.Rows[0];

            // Nếu là tài khoản gắn với bác sĩ: dùng thông tin bác sĩ
            // Nếu là admin / user khác: Id = 0, TenBS = HoTen trong TaiKhoan
            int doctorId = 0;
            string tenBs = string.Empty;

            if (row.Table.Columns.Contains("sttBS") && row["sttBS"] != DBNull.Value)
            {
                doctorId = Convert.ToInt32(row["sttBS"]);
            }

            if (row.Table.Columns.Contains("tenBS") && row["tenBS"] != DBNull.Value)
            {
                tenBs = Convert.ToString(row["tenBS"]);
            }
            else if (row.Table.Columns.Contains("HoTen") && row["HoTen"] != DBNull.Value)
            {
                tenBs = Convert.ToString(row["HoTen"]);
            }

            var bs = new BacSiModel
            {
                Id = doctorId,                                   // 0 nếu là admin / user khác
                TenBS = tenBs,                                   // tên bác sĩ hoặc tên hiển thị tài khoản
                Username = Convert.ToString(row["Username"]),
                DiaChi = row.Table.Columns.Contains("DiaChi") && row["DiaChi"] != DBNull.Value
                            ? Convert.ToString(row["DiaChi"])
                            : string.Empty,
                DienThoai = row.Table.Columns.Contains("DienThoai") && row["DienThoai"] != DBNull.Value
                            ? Convert.ToString(row["DienThoai"])
                            : string.Empty
                
            };

            return bs;
        }

        // ================== GIƯỜNG BỆNH ==================
        public static BedInfo[] GetBedInfos()
        {
            const string sql = @"
                SELECT gb.sttGB,
                       gb.tinhtrang,
                       MAX(bn.hoten) AS hoten
                FROM GiuongBenh gb
                LEFT JOIN BenhNhanNamGiuong_DaXepCho dxc ON gb.sttGB = dxc.sttGB
                LEFT JOIN BenhNhan bn ON dxc.sttBN = bn.sttBN
                GROUP BY gb.sttGB, gb.tinhtrang
                ORDER BY gb.sttGB;";

            var dt = Query(sql);

            var result = new BedInfo[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                result[i] = new BedInfo
                {
                    Id = Convert.ToInt32(row["sttGB"]),
                    TinhTrang = Convert.ToString(row["tinhtrang"]),
                    BenhNhan = row["hoten"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(row["hoten"])
                };
            }

            return result;
        }

        public static (int coBenhNhan, int conTrong, int khac) GetBedSummary()
        {
            var beds = GetBedInfos();
            int co = 0, trong = 0, khac = 0;

            foreach (var b in beds)
            {
                var status = (b.TinhTrang ?? string.Empty)
                             .Trim()
                             .ToLowerInvariant();

                if (status.Contains("có bệnh nhân"))
                    co++;
                else if (status.Contains("trống"))
                    trong++;
                else
                    khac++;
            }

            return (co, trong, khac);
        }
    }
}
