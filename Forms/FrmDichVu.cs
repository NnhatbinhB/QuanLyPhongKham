using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmDichVu : Form
    {
        // ====== CONTROL ======
        private DataGridView _grid;
        private TextBox _txtTenDV;
        private ComboBox _cbLoaiDV;
        private TextBox _txtDonGia;
        private TextBox _txtGhiChu;
        private TextBox _txtSearch;

        private Button _btnThem, _btnSua, _btnXoa, _btnLamMoi;
        private Label _lblTong;

        private int? _currentId;        // MaDV đang chọn
        private DataTable? _table;      // cache dịch vụ

        public FrmDichVu()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F);
            Text = "Quản lý dịch vụ";

            BuildUI();
            LoadDichVu();
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

            var top = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            layout.Controls.Add(top, 0, 0);

            // dòng 1: Tên DV + Loại DV
            var lblTen = new Label { Text = "Tên dịch vụ:", AutoSize = true, Left = 0, Top = 10 };
            _txtTenDV = new TextBox { Left = 110, Top = 6, Width = 280 };

            var lblLoai = new Label { Text = "Loại dịch vụ:", AutoSize = true, Left = 420, Top = 10 };
            _cbLoaiDV = new ComboBox
            {
                Left = 510,
                Top = 6,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cbLoaiDV.Items.AddRange(new object[]
            {
                "Xét nghiệm",
                "X-quang",
                "Nội soi",
                "Siêu âm",
                "Khám chuyên khoa",
                "Khác"
            });

            top.Controls.AddRange(new Control[] { lblTen, _txtTenDV, lblLoai, _cbLoaiDV });

            // dòng 2: Đơn giá + Ghi chú
            var lblDonGia = new Label { Text = "Đơn giá:", AutoSize = true, Left = 0, Top = 45 };
            _txtDonGia = new TextBox { Left = 110, Top = 41, Width = 150 };

            var lblGhiChu = new Label { Text = "Ghi chú:", AutoSize = true, Left = 280, Top = 45 };
            _txtGhiChu = new TextBox { Left = 340, Top = 41, Width = 370 };

            top.Controls.AddRange(new Control[] { lblDonGia, _txtDonGia, lblGhiChu, _txtGhiChu });

            // dòng 3: nút + tìm kiếm + tổng
            _btnThem = new Button
            {
                Text = "Thêm",
                Left = 110,
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
                Left = 210,
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
                Left = 310,
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
                Left = 410,
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
                lblSearch, _txtSearch, _lblTong
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
        private void LoadDichVu()
        {
            try
            {
                const string sql = @"SELECT MaDV, TenDV, LoaiDV, DonGia, GhiChu
                                     FROM DichVu
                                     ORDER BY TenDV;";

                _table = Database.Query(sql, CommandType.Text) ?? new DataTable();
                _grid.DataSource = _table;

                // format cột – có lỗi thì bỏ qua
                try
                {
                    if (_grid.Columns["MaDV"] != null)
                        _grid.Columns["MaDV"].HeaderText = "Mã DV";

                    if (_grid.Columns["TenDV"] != null)
                        _grid.Columns["TenDV"].HeaderText = "Tên dịch vụ";

                    if (_grid.Columns["LoaiDV"] != null)
                        _grid.Columns["LoaiDV"].HeaderText = "Loại dịch vụ";

                    if (_grid.Columns["DonGia"] != null)
                        _grid.Columns["DonGia"].HeaderText = "Đơn giá";

                    if (_grid.Columns["GhiChu"] != null)
                        _grid.Columns["GhiChu"].HeaderText = "Ghi chú";
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách dịch vụ:\n" + ex.Message,
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
                    $"TenDV LIKE '%{keyword}%' " +
                    $"OR LoaiDV LIKE '%{keyword}%' " +
                    $"OR GhiChu LIKE '%{keyword}%' " +
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
                if (row.Cells["MaDV"].Value == null) return;

                _currentId = Convert.ToInt32(row.Cells["MaDV"].Value);
                _txtTenDV.Text = Convert.ToString(row.Cells["TenDV"].Value);
                _cbLoaiDV.SelectedItem = Convert.ToString(row.Cells["LoaiDV"].Value);
                _txtDonGia.Text = Convert.ToString(row.Cells["DonGia"].Value);
                _txtGhiChu.Text = Convert.ToString(row.Cells["GhiChu"].Value);
            }
            catch
            {
                // bỏ qua nếu map lỗi
            }
        }

        // ================== BUTTON HANDLERS ==================
        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(out string tenDV, out string loaiDV,
                    out decimal donGia, out string ghiChu))
                return;

            try
            {
                const string sql = @"INSERT INTO DichVu (TenDV, LoaiDV, DonGia, GhiChu)
                                     VALUES (@TenDV, @LoaiDV, @DonGia, @GhiChu);";

                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@TenDV", tenDV),
                    new SqlParameter("@LoaiDV", (object)loaiDV ?? DBNull.Value),
                    new SqlParameter("@DonGia", donGia),
                    new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value));

                LoadDichVu();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm dịch vụ:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một dịch vụ trong danh sách để sửa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out string tenDV, out string loaiDV,
                    out decimal donGia, out string ghiChu))
                return;

            try
            {
                const string sql = @"UPDATE DichVu
                                     SET TenDV = @TenDV,
                                         LoaiDV = @LoaiDV,
                                         DonGia = @DonGia,
                                         GhiChu = @GhiChu
                                     WHERE MaDV = @Id;";

                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@TenDV", tenDV),
                    new SqlParameter("@LoaiDV", (object)loaiDV ?? DBNull.Value),
                    new SqlParameter("@DonGia", donGia),
                    new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value),
                    new SqlParameter("@Id", _currentId.Value));

                LoadDichVu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật dịch vụ:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một dịch vụ trong danh sách để xóa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dr = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa dịch vụ này?\n" +
                "Lưu ý: nếu đã có hóa đơn / phiếu khám dùng dịch vụ này thì có thể không xóa được.",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes) return;

            try
            {
                const string sql = @"DELETE FROM DichVu WHERE MaDV = @Id;";
                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@Id", _currentId.Value));

                LoadDichVu();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa dịch vụ.\n" +
                                "Có thể đang tồn tại hóa đơn / phiếu khám liên quan.\n\nChi tiết: " + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== HELPER ==================
        private void ClearInputs()
        {
            _currentId = null;
            _txtTenDV.Clear();
            _cbLoaiDV.SelectedIndex = -1;
            _txtDonGia.Clear();
            _txtGhiChu.Clear();
            _grid.ClearSelection();
        }

        private bool ValidateInput(
            out string tenDV,
            out string loaiDV,
            out decimal donGia,
            out string ghiChu)
        {
            tenDV = _txtTenDV.Text.Trim();
            loaiDV = _cbLoaiDV.SelectedItem?.ToString() ?? "";
            ghiChu = _txtGhiChu.Text.Trim();
            donGia = 0;

            if (string.IsNullOrWhiteSpace(tenDV))
            {
                MessageBox.Show("Tên dịch vụ không được để trống.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTenDV.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(loaiDV))
            {
                MessageBox.Show("Hãy chọn loại dịch vụ.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cbLoaiDV.Focus();
                return false;
            }

            var donGiaText = _txtDonGia.Text.Trim();
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
