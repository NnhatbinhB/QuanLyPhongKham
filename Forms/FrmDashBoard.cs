using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    internal partial class FrmDashBoard : Form
    {
        private Chart _chartRevenue;      // cột: doanh thu 30 ngày gần nhất
        private Chart _chartGender;       // doughnut: bệnh nhân theo giới tính
        private Chart _chartLoai;         // pie: loại bệnh nhân (thường / nằm giường)

        public FrmDashBoard()
        {
            InitializeComponent();

            Text = "Dashboard";
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false;
            Dock = DockStyle.Fill;

            BuildLayout();
            LoadCharts();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chartRevenue?.Dispose();
                _chartGender?.Dispose();
                _chartLoai?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ================== UI LAYOUT ==================
        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };

            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            _chartRevenue = CreateChart("Doanh thu 30 ngày gần nhất", SeriesChartType.Column);
            _chartGender = CreateChart("Bệnh nhân theo giới tính", SeriesChartType.Doughnut);
            _chartLoai = CreateChart("Tỷ lệ loại bệnh nhân", SeriesChartType.Pie);

            root.Controls.Add(_chartRevenue, 0, 0);
            root.SetColumnSpan(_chartRevenue, 2);

            root.Controls.Add(_chartGender, 0, 1);
            root.Controls.Add(_chartLoai, 1, 1);

            Controls.Clear();
            Controls.Add(root);
        }

        private Chart CreateChart(string title, SeriesChartType type)
        {
            var chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var area = new ChartArea("Main")
            {
                BackColor = Color.White
            };
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro;
            chart.ChartAreas.Add(area);

            var legend = new Legend
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center
            };
            chart.Legends.Add(legend);

            var series = new Series("Series1")
            {
                ChartArea = "Main",
                ChartType = type,
                IsValueShownAsLabel = true
            };
            chart.Series.Add(series);

            chart.Titles.Add(new Title
            {
                Text = title,
                Docking = Docking.Top,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32)
            });

            return chart;
        }

        private void LoadCharts()
        {
            LoadRevenueChart();
            LoadGenderChart();
            LoadLoaiChart();
        }

        // ================== DATA BIND ==================

        /// <summary>
        /// Doanh thu 30 ngày gần nhất từ Hóa đơn bệnh nhân thường + nằm giường.
        /// </summary>
        private void LoadRevenueChart()
        {
            const string sql = @"
;WITH HD_BNT AS (
    SELECT CAST(hd.NgayLap AS date) AS Ngay,
           hd.TongTien              AS DoanhThu
    FROM   HoaDon_BenhNhanThuong hd
    WHERE  hd.NgayLap >= DATEADD(DAY, -30, CAST(GETDATE() AS date))
),
HD_NG AS (
    SELECT CAST(bnng.ngayxep AS date) AS Ngay,
           hd.ThanhTien               AS DoanhThu
    FROM   HoaDon hd
           INNER JOIN BenhNhanNamGiuong_DaXepCho bnng
               ON hd.sttBNNG_DXC = bnng.sttBNNG_DXC
    WHERE  bnng.ngayxep >= DATEADD(DAY, -30, CAST(GETDATE() AS date))
)
SELECT Ngay,
       SUM(DoanhThu) AS DoanhThu
FROM (
    SELECT * FROM HD_BNT
    UNION ALL
    SELECT * FROM HD_NG
) X
GROUP BY Ngay
ORDER BY Ngay;";

            var dt = Database.Query(sql, CommandType.Text) ?? new DataTable();

            var series = _chartRevenue.Series[0];
            series.Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string ngay = row["Ngay"] == DBNull.Value
                    ? ""
                    : Convert.ToDateTime(row["Ngay"]).ToString("dd/MM");

                decimal doanhThu = row["DoanhThu"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(row["DoanhThu"]);

                series.Points.AddXY(ngay, doanhThu);
            }
        }

        /// <summary>
        /// Bệnh nhân theo giới tính từ bảng BenhNhan (Nam/Nữ/...).
        /// </summary>
        private void LoadGenderChart()
        {
            const string sql = @"
SELECT gioitinh AS GioiTinh,
       COUNT(*) AS SoBN
FROM   BenhNhan
GROUP BY gioitinh;";

            var dt = Database.Query(sql, CommandType.Text) ?? new DataTable();

            var s = _chartGender.Series[0];
            s.Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string gioiTinh = row["GioiTinh"]?.ToString() ?? "Không rõ";
                int soBN = row["SoBN"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoBN"]);
                s.Points.AddXY(gioiTinh, soBN);
            }
        }

        /// <summary>
        /// Tỷ lệ loại bệnh nhân: Thường / Nằm giường.
        /// </summary>
        private void LoadLoaiChart()
        {
            const string sql = @"
SELECT N'Thường'      AS Loai, COUNT(*) AS SoBN FROM BenhNhanThuong
UNION ALL
SELECT N'Nằm giường' AS Loai, COUNT(*) AS SoBN FROM BenhNhanNamGiuong_DaXepCho;";

            var dt = Database.Query(sql, CommandType.Text) ?? new DataTable();

            var s = _chartLoai.Series[0];
            s.Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string loai = row["Loai"]?.ToString() ?? "";
                int soBN = row["SoBN"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoBN"]);
                s.Points.AddXY(loai, soBN);
            }
        }
    }
}
