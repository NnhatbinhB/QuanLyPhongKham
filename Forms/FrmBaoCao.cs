using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmBaoCao : Form
    {
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Button _btnXem;
        private Button _btnExcel;
        private Button _btnPdf;
        private Label _lblTongDoanhThu;
        private DataGridView _grid;
        private DataTable? _source;

        public FrmBaoCao()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10F);
            BackColor = System.Drawing.Color.FromArgb(232, 245, 233);
            Text = "Báo cáo doanh thu";

            BuildLayout();
            LoadBaoCao();
        }

        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var top = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = System.Drawing.Color.White
            };
            layout.Controls.Add(top, 0, 0);

            int y = 10;

            var lblFrom = new Label
            {
                Text = "Từ ngày:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            top.Controls.Add(lblFrom);

            _dtFrom = new DateTimePicker
            {
                Location = new Point(70, y),
                Width = 130,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value = new DateTime(DateTime.Today.Year, 1, 1)
            };
            top.Controls.Add(_dtFrom);

            var lblTo = new Label
            {
                Text = "Đến ngày:",
                AutoSize = true,
                Location = new Point(220, y + 4)
            };
            top.Controls.Add(lblTo);

            _dtTo = new DateTimePicker
            {
                Location = new Point(300, y),
                Width = 130,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value = DateTime.Today
            };
            top.Controls.Add(_dtTo);

            _btnXem = new Button
            {
                Text = "Xem báo cáo",
                Width = 120,
                Height = 28,
                Location = new Point(450, y - 1),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(56, 142, 60),
                ForeColor = System.Drawing.Color.White
            };
            _btnXem.FlatAppearance.BorderSize = 0;
            _btnXem.Click += (s, e) => LoadBaoCao();
            top.Controls.Add(_btnXem);

            _btnExcel = new Button
            {
                Text = "Xuất Excel (CSV)",
                Width = 140,
                Height = 28,
                Location = new Point(590, y - 1),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(30, 136, 229),
                ForeColor = System.Drawing.Color.White
            };
            _btnExcel.FlatAppearance.BorderSize = 0;
            _btnExcel.Click += (s, e) => ExportToCsv();
            top.Controls.Add(_btnExcel);

            _btnPdf = new Button
            {
                Text = "Xuất PDF",
                Width = 110,
                Height = 28,
                Location = new Point(740, y - 1),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(211, 47, 47),
                ForeColor = System.Drawing.Color.White
            };
            _btnPdf.FlatAppearance.BorderSize = 0;
            _btnPdf.Click += (s, e) => ExportToPdf();
            top.Controls.Add(_btnPdf);

            _lblTongDoanhThu = new Label
            {
                Text = "Tổng doanh thu: 0 đ",
                AutoSize = true,
                Location = new Point(0, 45),
                ForeColor = System.Drawing.Color.FromArgb(27, 94, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            top.Controls.Add(_lblTongDoanhThu);

            var bottom = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 16, 16)
            };
            layout.Controls.Add(bottom, 0, 1);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = System.Drawing.Color.White,
                RowHeadersVisible = false
            };
            bottom.Controls.Add(_grid);
        }

        // ================== LOAD BÁO CÁO ==================
        private void LoadBaoCao()
        {
            try
            {
                DateTime from = _dtFrom.Value.Date;
                // lấy hết đến cuối ngày Đến ngày
                DateTime to = _dtTo.Value.Date.AddDays(1).AddTicks(-1);

                // Doanh thu = HĐ bệnh nhân thường + HĐ bệnh nhân nằm giường
                const string sql = @"
;WITH HD_BNT AS (
    SELECT CAST(hd.NgayLap AS date) AS Ngay,
           hd.TongTien              AS DoanhThu
    FROM   HoaDon_BenhNhanThuong hd
    WHERE  hd.NgayLap BETWEEN @from AND @to
),
HD_NG AS (
    SELECT CAST(bnng.ngayxep AS date) AS Ngay,
           hd.ThanhTien               AS DoanhThu
    FROM   HoaDon hd
           INNER JOIN BenhNhanNamGiuong_DaXepCho bnng
               ON hd.sttBNNG_DXC = bnng.sttBNNG_DXC
    WHERE  bnng.ngayxep BETWEEN @from AND @to
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

                _source = Database.Query(sql, CommandType.Text,
                                new SqlParameter("@from", from),
                                new SqlParameter("@to", to))
                           ?? new DataTable();

                _grid.DataSource = _source;

                try
                {
                    if (_grid.Columns.Contains("Ngay"))
                    {
                        _grid.Columns["Ngay"].HeaderText = "Ngày";
                        _grid.Columns["Ngay"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    }

                    if (_grid.Columns.Contains("DoanhThu"))
                    {
                        _grid.Columns["DoanhThu"].HeaderText = "Doanh thu (đ)";
                        _grid.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                    }
                }
                catch
                {
                    // ignore format error
                }

                long tong = 0;
                foreach (DataRow row in _source.Rows)
                {
                    if (row["DoanhThu"] != DBNull.Value)
                        tong += Convert.ToInt64(row["DoanhThu"]);
                }

                _lblTongDoanhThu.Text = $"Tổng doanh thu: {tong:N0} đ";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải báo cáo doanh thu:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== EXPORT CSV ==================
        private void ExportToCsv()
        {
            if (_source == null || _source.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = $"BaoCao_DoanhThu_{_dtFrom.Value:yyyyMMdd}_{_dtTo.Value:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();

                // header
                for (int i = 0; i < _source.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(_source.Columns[i].ColumnName);
                }
                sb.AppendLine();

                // rows
                foreach (DataRow row in _source.Rows)
                {
                    for (int i = 0; i < _source.Columns.Count; i++)
                    {
                        if (i > 0) sb.Append(",");

                        var val = row[i]?.ToString()?.Replace("\"", "\"\"") ?? "";
                        if (val.Contains(",")) val = "\"" + val + "\"";
                        sb.Append(val);
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show("Đã xuất báo cáo ra file CSV.\nMở bằng Excel để xem.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất CSV:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== EXPORT PDF (QuestPDF) ==================
        private void ExportToPdf()
        {
            if (_source == null || _source.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF file (*.pdf)|*.pdf",
                FileName = $"BaoCao_DoanhThu_{_dtFrom.Value:yyyyMMdd}_{_dtTo.Value:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(40);
                        page.Size(PageSizes.A4);

                        page.Header().Text("BÁO CÁO DOANH THU")
                            .FontSize(18)
                            .Bold()
                            .AlignCenter();

                        page.Content().Column(col =>
                        {
                            col.Spacing(10);

                            col.Item().Text(
                                $"Từ ngày: {_dtFrom.Value:dd/MM/yyyy}  -  Đến ngày: {_dtTo.Value:dd/MM/yyyy}");

                            col.Item().Text(_lblTongDoanhThu.Text).Bold().FontSize(12);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellHeaderStyle)
                                        .Text("Ngày");
                                    header.Cell().Element(CellHeaderStyle)
                                        .Text("Doanh thu (đ)");
                                });

                                foreach (DataRow row in _source.Rows)
                                {
                                    string ngay = row["Ngay"] == DBNull.Value
                                        ? ""
                                        : Convert.ToDateTime(row["Ngay"]).ToString("dd/MM/yyyy");
                                    string doanhThu = row["DoanhThu"] == DBNull.Value
                                        ? "0"
                                        : Convert.ToInt64(row["DoanhThu"]).ToString("N0");

                                    table.Cell().Element(CellBodyStyle).Text(ngay);
                                    table.Cell().Element(CellBodyStyle).Text(doanhThu);
                                }
                            });
                        });

                        page.Footer()
                            .AlignRight()
                            .Text(x =>
                            {
                                x.Span("Ngày in: ").FontSize(9);
                                x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9);
                            });
                    });
                });

                doc.GeneratePdf(sfd.FileName);

                MessageBox.Show("Đã xuất báo cáo ra file PDF.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất PDF (hãy chắc chắn đã cài QuestPDF):\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            static IContainer CellHeaderStyle(IContainer container)
            {
                return container
                    .PaddingVertical(4)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .DefaultTextStyle(x => x.SemiBold().FontSize(10));
            }

            static IContainer CellBodyStyle(IContainer container)
            {
                return container
                    .PaddingVertical(2)
                    .DefaultTextStyle(x => x.FontSize(10));
            }
        }
    }
}
