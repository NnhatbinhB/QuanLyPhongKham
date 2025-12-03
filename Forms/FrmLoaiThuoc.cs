using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmLoaiThuoc : Form
    {
        // ====== CONTROL ======
        private DataGridView _grid;
        private TextBox _txtTenLoai;
        private TextBox _txtDonGia;
        private TextBox _txtMoTa;
        private TextBox _txtSearch;

        private Button _btnThem, _btnSua, _btnXoa, _btnLamMoi;

        private int? _currentId;        // MaLoaiThuoc đang chọn
        private DataTable? _table;      // cache loại thuốc

        public FrmLoaiThuoc()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F);
            Text = "Quản lý loại thuốc";

            BuildUI();
            LoadLoaiThuoc();
        }

        // ================== UI ==================
        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            // ===== TOP =====
            var top = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            layout.Controls.Add(top, 0, 0);

            // dòng 1: Tên loại
            var lblTen = new Label { Text = "Tên loại thuốc:", AutoSize = true, Left = 0, Top = 10 };
            _txtTenLoai = new TextBox { Left = 120, Top = 6, Width = 280 };

            // dòng 1 (tiếp): Đơn giá
            var lblDonGia = new Label { Text = "Đơn giá:", AutoSize = true, Left = 420, Top = 10 };
            _txtDonGia = new TextBox { Left = 480, Top = 6, Width = 140 };
            var lblVND = new Label { Text = "VND", AutoSize = true, Left = 630, Top = 10 };

            // dòng 2: Mô tả
            var lblMoTa = new Label { Text = "Mô tả:", AutoSize = true, Left = 0, Top = 45 };
            _txtMoTa = new TextBox { Left = 120, Top = 41, Width = 500 };

            top.Controls.AddRange(new Control[]
            {
                lblTen, _txtTenLoai,
                lblDonGia, _txtDonGia, lblVND,
                lblMoTa, _txtMoTa
            });

            // dòng 3: nút + tìm kiếm + tổng
            _btnThem = new Button
            {
                Text = "Thêm",
                Left = 120,
                Top = 80,
                Width = 90,
                Height = 45,
                BackColor = Color.FromArgb(67, 160, 71),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnThem.FlatAppearance.BorderSize = 0;
            _btnThem.Click += BtnThem_Click;

            _btnSua = new Button
            {
                Text = "Sửa",
                Left = 220,
                Top = 80,
                Width = 90,
                Height = 45,
                BackColor = Color.FromArgb(30, 136, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnSua.FlatAppearance.BorderSize = 0;
            _btnSua.Click += BtnSua_Click;

            _btnXoa = new Button
            {
                Text = "Xóa",
                Left = 320,
                Top = 80,
                Width = 90,
                Height = 45,
                BackColor = Color.FromArgb(211, 47, 47),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnXoa.FlatAppearance.BorderSize = 0;
            _btnXoa.Click += BtnXoa_Click;

            _btnLamMoi = new Button
            {
                Text = "Làm mới",
                Left = 420,
                Top = 80,
                Width = 90,
                Height = 45,
                BackColor = Color.FromArgb(120, 144, 156),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnLamMoi.FlatAppearance.BorderSize = 0;
            _btnLamMoi.Click += (_, __) =>
            {
                ClearInputs();
                _txtSearch.Clear();
                ApplyFilter();
            };

            var lblSearch = new Label
            {
                Text = "Tìm kiếm:",
                AutoSize = true,
                Left = 540,
                Top = 96
            };

            _txtSearch = new TextBox
            {
                Left = 610,
                Top = 92,
                Width = 220
            };
            _txtSearch.TextChanged += (_, __) => ApplyFilter();

            top.Controls.AddRange(new Control[]
            {
                _btnThem, _btnSua, _btnXoa, _btnLamMoi,
                lblSearch, _txtSearch
            });

            // ===== GRID =====
            var bottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 16) };
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
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            _grid.CellClick += Grid_CellClick;

            bottom.Controls.Add(_grid);
        }

        // ================== DATA LOAD ==================
        private void LoadLoaiThuoc()
        {
            try
            {
                const string sql = @"SELECT MaLoaiThuoc, TenLoai, DonGia, MoTa
                                     FROM LoaiThuoc
                                     ORDER BY TenLoai;";

                _table = Database.Query(sql, CommandType.Text) ?? new DataTable();
                _grid.DataSource = _table;

                try
                {
                    if (_grid.Columns["MaLoaiThuoc"] != null)
                        _grid.Columns["MaLoaiThuoc"].HeaderText = "Mã Loại";

                    if (_grid.Columns["TenLoai"] != null)
                        _grid.Columns["TenLoai"].HeaderText = "Tên loại thuốc";

                    if (_grid.Columns["DonGia"] != null)
                        _grid.Columns["DonGia"].HeaderText = "Đơn giá";

                    if (_grid.Columns["MoTa"] != null)
                        _grid.Columns["MoTa"].HeaderText = "Mô tả";
                }
                catch
                {
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách loại thuốc:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_table == null) return;

            string keyword = _txtSearch.Text.Trim().Replace("'", "''");

            if (string.IsNullOrEmpty(keyword))
            {
                _table.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                _table.DefaultView.RowFilter =
                    $"TenLoai LIKE '%{keyword}%' " +
                    $"OR MoTa LIKE '%{keyword}%' " +
                    $"OR Convert(DonGia, 'System.String') LIKE '%{keyword}%'";
            }
        }

        // ================== EVENT GRID ==================
        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.CurrentRow == null) return;
            var row = _grid.CurrentRow;

            try
            {
                if (row.Cells["MaLoaiThuoc"].Value == null) return;

                _currentId = Convert.ToInt32(row.Cells["MaLoaiThuoc"].Value);
                _txtTenLoai.Text = Convert.ToString(row.Cells["TenLoai"].Value);
                _txtMoTa.Text = Convert.ToString(row.Cells["MoTa"].Value);

                if (row.Cells["DonGia"].Value != null &&
                    decimal.TryParse(row.Cells["DonGia"].Value.ToString(), out var dg))
                {
                    _txtDonGia.Text = dg.ToString("0");
                }
                else
                {
                    _txtDonGia.Text = "";
                }
            }
            catch
            {
                // bỏ qua lỗi map
            }
        }

        // ================== BUTTON HANDLERS ==================
        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(out string tenLoai, out decimal donGia, out string moTa)) return;

            try
            {
                const string sql = @"INSERT INTO LoaiThuoc (TenLoai, DonGia, MoTa)
                                     VALUES (@TenLoai, @DonGia, @MoTa);";

                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@TenLoai", tenLoai),
                    new SqlParameter("@DonGia", donGia),
                    new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value));

                LoadLoaiThuoc();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm loại thuốc:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một loại thuốc trong danh sách để sửa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out string tenLoai, out decimal donGia, out string moTa)) return;

            try
            {
                const string sql = @"UPDATE LoaiThuoc
                                     SET TenLoai = @TenLoai,
                                         DonGia  = @DonGia,
                                         MoTa    = @MoTa
                                     WHERE MaLoaiThuoc = @Id;";

                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@TenLoai", tenLoai),
                    new SqlParameter("@DonGia", donGia),
                    new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value),
                    new SqlParameter("@Id", _currentId.Value));

                LoadLoaiThuoc();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật loại thuốc:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một loại thuốc trong danh sách để xóa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dr = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa loại thuốc này?\n" +
                "Lưu ý: nếu đang có thuốc thuộc loại này thì có thể không xóa được.",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes) return;

            try
            {
                const string sql = @"DELETE FROM LoaiThuoc WHERE MaLoaiThuoc = @Id;";
                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@Id", _currentId.Value));

                LoadLoaiThuoc();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa loại thuốc.\n" +
                                "Có thể đang tồn tại thuốc tham chiếu đến loại này.\n\nChi tiết: " + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== HELPER ==================
        private void ClearInputs()
        {
            _currentId = null;
            _txtTenLoai.Clear();
            _txtMoTa.Clear();
            _txtDonGia.Clear();
            _grid.ClearSelection();
        }

        private bool ValidateInput(out string tenLoai, out decimal donGia, out string moTa)
        {
            tenLoai = _txtTenLoai.Text.Trim();
            moTa = _txtMoTa.Text.Trim();
            donGia = 0;

            if (string.IsNullOrWhiteSpace(tenLoai))
            {
                MessageBox.Show("Tên loại thuốc không được để trống.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTenLoai.Focus();
                return false;
            }

            var donGiaText = _txtDonGia.Text.Trim().Replace(".", "").Replace(",", "");
            if (!decimal.TryParse(donGiaText, out donGia) || donGia < 0)
            {
                MessageBox.Show("Đơn giá không hợp lệ.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtDonGia.Focus();
                return false;
            }

            return true;
        }
    }
}
