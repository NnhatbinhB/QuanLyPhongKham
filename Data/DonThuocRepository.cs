using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyPhongKham.Data
{
    public static class DonThuocRepository
    {
        // ======== BỆNH NHÂN THƯỜNG ========

        /// <summary>
        /// Lấy danh sách bệnh nhân thường (BenhNhanThuong)
        /// </summary>
        public static DataTable GetAllBenhNhanThuong()
        {
            // SP đã có trong script: sp_SelectAllBenhNhanThuong
            return Database.Query("sp_SelectAllBenhNhanThuong", CommandType.StoredProcedure)
                   ?? new DataTable();
        }

        /// <summary>
        /// Lấy tất cả đơn thuốc của 1 bệnh nhân thường (theo sttBNT)
        /// </summary>
        public static DataTable GetDonThuocBenhNhanThuong(int sttBNT)
        {
            // SP mới: sp_selectAllDonThuoc_BenhNhanThuongByID
            return Database.Query(
                       "sp_selectAllDonThuoc_BenhNhanThuongByID",
                       CommandType.StoredProcedure,
                       new SqlParameter("@sttBNT", sttBNT)
                   ) ?? new DataTable();
        }

        /// <summary>
        /// Thêm đơn thuốc cho bệnh nhân thường
        /// </summary>
        public static void InsertDonThuocBenhNhanThuong(string tenThuoc, int tienThuoc, int sttBNT)
        {
            Database.Execute(
                "sp_InsertDonThuoc_DonThuocBenhNhanThuong",
                CommandType.StoredProcedure,
                new SqlParameter("@TenThuoc", tenThuoc),
                new SqlParameter("@TienThuoc", tienThuoc),
                new SqlParameter("@MaBenhNhanThuong", sttBNT)
            );
        }

        // ======== BỆNH NHÂN NẰM GIƯỜNG ĐÃ XẾP CHỖ ========

        /// <summary>
        /// Lấy danh sách bệnh nhân nằm giường đã xếp chỗ (sp_selectAllBenhNhanNamGiuong_DaXepCho)
        /// </summary>
        public static DataTable GetAllBenhNhanNamGiuong_DaXepCho()
        {
            return Database.Query("sp_selectAllBenhNhanNamGiuong_DaXepCho", CommandType.StoredProcedure)
                   ?? new DataTable();
        }

        /// <summary>
        /// Lấy tất cả đơn thuốc của một bệnh nhân nằm giường (theo sttBNNG_DXC)
        /// Chỉ lấy đơn thuốc "Chưa thanh toán" vì SP đã filter sẵn.
        /// </summary>
        public static DataTable GetDonThuocBenhNhanNamGiuong(int sttBNNG_DXC)
        {
            return Database.Query(
                       "sp_selectAllDonThuoc_BenhNhanNamGiuong_DaXepChoByID",
                       CommandType.StoredProcedure,
                       new SqlParameter("@sttBNNG_DXC", sttBNNG_DXC)
                   ) ?? new DataTable();
        }

        /// <summary>
        /// Thêm đơn thuốc cho bệnh nhân nằm giường (ĐÃ XẾP CHỖ)
        /// Tình trạng mặc định: "Chưa thanh toán" (do SP set).
        /// </summary>
        public static void InsertDonThuocBenhNhanNamGiuong(string tenThuoc, int tienThuoc, int sttBNNG_DXC)
        {
            Database.Execute(
                "sp_InsertDonThuoc_DonThuocBenhNhanNamGiuong_DaXepCho",
                CommandType.StoredProcedure,
                new SqlParameter("@TenThuoc", tenThuoc),
                new SqlParameter("@TienThuoc", tienThuoc),
                new SqlParameter("@MaBenhNhanNamGiuong_DaXepCho", sttBNNG_DXC)
            );
        }
    }
}