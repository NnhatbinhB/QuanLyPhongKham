using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmBacSi : Form
    {
        // ====== CONTROL ======
        private DataGridView _grid;
        private TextBox _txtTenBS;
        private TextBox _txtDiaChi;
        private TextBox _txtDienThoai;
        private TextBox _txtSearch;
        private ComboBox _cbKhoa;

        private Button _btnThem, _btnSua, _btnXoa, _btnLamMoi;
        private Label _lblTong;

        private int? _currentId;              // sttBS đang chọn
        private DataTable? _doctorTable;      // cache danh sách bác sĩ

        public FrmBacSi()
        {
            BuildUI();
            LoadKhoa();
            LoadBacSi();
        }

        // ================== UI ==================
        private void BuildUI()
        {
            Text = "Quản lý bác sĩ";
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F);

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

            // dòng 1: Họ tên
            var lblTen = new Label { Text = "Họ tên:", AutoSize = true, Left = 0, Top = 10 };
            _txtTenBS = new TextBox { Left = 80, Top = 6, Width = 260 };
            top.Controls.AddRange(new Control[] { lblTen, _txtTenBS });

            // dòng 2: Địa chỉ + Điện thoại
            var lblDiaChi = new Label { Text = "Địa chỉ:", AutoSize = true, Left = 0, Top = 45 };
            _txtDiaChi = new TextBox { Left = 80, Top = 41, Width = 420 };

            var lblDT = new Label { Text = "Điện thoại:", AutoSize = true, Left = 520, Top = 45 };
            _txtDienThoai = new TextBox { Left = 610, Top = 41, Width = 210 };

            top.Controls.AddRange(new Control[] { lblDiaChi, _txtDiaChi, lblDT, _txtDienThoai });

            // dòng 3: Khoa
            var lblKhoa = new Label { Text = "Khoa:", AutoSize = true, Left = 0, Top = 80 };
            _cbKhoa = new ComboBox
            {
                Left = 80,
                Top = 76,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            top.Controls.AddRange(new Control[] { lblKhoa, _cbKhoa });

            // dòng 4: nút + tìm kiếm + tổng
            _btnThem = new Button
            {
                Text = "Thêm",
                Left = 80,
                Top = 115,
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
                Left = 180,
                Top = 115,
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
                Left = 280,
                Top = 115,
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
                Left = 380,
                Top = 115,
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
                Left = 520,
                Top = 131
            };

            _txtSearch = new TextBox
            {
                Left = 590,
                Top = 127,
                Width = 230
            };
            _txtSearch.TextChanged += (_, __) => ApplyFilter();

            _lblTong = new Label
            {
                Text = "Tổng: 0 bác sĩ",
                AutoSize = true,
                Left = 520,
                Top = 90,
                ForeColor = Color.FromArgb(27, 94, 32),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

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

        private void LoadKhoa()
        {
            try
            {
                var dt = Database.Query("sp_SelectAllKhoa", CommandType.StoredProcedure);
                _cbKhoa.DataSource = dt;
                _cbKhoa.DisplayMember = "TenKhoa";
                _cbKhoa.ValueMember = "sttKhoa";
                _cbKhoa.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách khoa:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== DATA LOAD ==================
        private void LoadBacSi()
        {
            try
            {
                const string sql = @"
SELECT  b.sttBS,
        b.tenBS,
        b.DiaChi,
        b.DienThoai,
        k.sttKhoa,
        k.TenKhoa
FROM    BacSi b
LEFT JOIN Khoa k ON b.sttKhoa = k.sttKhoa
ORDER BY b.sttBS;";

                _doctorTable = Database.Query(sql, CommandType.Text) ?? new DataTable();

                _grid.DataSource = _doctorTable;

                try
                {
                    DataGridViewColumn? col;

                    col = _grid.Columns["sttBS"];
                    if (col != null)
                    {
                        col.HeaderText = "Mã Bác Sĩ";
                        col.Width = 80;
                    }

                    col = _grid.Columns["tenBS"];
                    if (col != null) col.HeaderText = "Tên Bác Sĩ";

                    col = _grid.Columns["DiaChi"];
                    if (col != null) col.HeaderText = "Địa chỉ";

                    col = _grid.Columns["DienThoai"];
                    if (col != null) col.HeaderText = "Điện thoại";

                    col = _grid.Columns["TenKhoa"];
                    if (col != null) col.HeaderText = "Khoa";

                    col = _grid.Columns["sttKhoa"];
                    if (col != null) col.Visible = false;
                }
                catch
                {
                    // ignore
                }

                UpdateCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bác sĩ:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCountLabel()
        {
            int count = _doctorTable?.DefaultView.Count ?? 0;
            _lblTong.Text = $"Tổng: {count} bác sĩ";
        }

        private void ApplyFilter()
        {
            if (_doctorTable == null) return;

            string keyword = _txtSearch.Text.Trim().Replace("'", "''");

            if (string.IsNullOrEmpty(keyword))
            {
                _doctorTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                _doctorTable.DefaultView.RowFilter =
                    $"tenBS LIKE '%{keyword}%' " +
                    $"OR DiaChi LIKE '%{keyword}%' " +
                    $"OR DienThoai LIKE '%{keyword}%' " +
                    $"OR TenKhoa LIKE '%{keyword}%'";
            }

            UpdateCountLabel();
        }

        // ================== EVENT GRID ==================
        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.CurrentRow == null) return;
            var row = _grid.CurrentRow;

            try
            {
                if (row.Cells["sttBS"].Value == null) return;

                _currentId = Convert.ToInt32(row.Cells["sttBS"].Value);
                _txtTenBS.Text = Convert.ToString(row.Cells["tenBS"].Value);
                _txtDiaChi.Text = Convert.ToString(row.Cells["DiaChi"].Value);
                _txtDienThoai.Text = Convert.ToString(row.Cells["DienThoai"].Value);

                if (_cbKhoa.DataSource != null && _grid.Columns.Contains("sttKhoa"))
                {
                    var khoaVal = row.Cells["sttKhoa"].Value;
                    if (khoaVal != null && khoaVal != DBNull.Value &&
                        int.TryParse(khoaVal.ToString(), out int khoaId))
                    {
                        _cbKhoa.SelectedValue = khoaId;
                    }
                    else
                    {
                        _cbKhoa.SelectedIndex = -1;
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        // ================== BUTTON HANDLERS ==================
        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(out string tenBS,
                               out string diaChi,
                               out string dienThoai,
                               out int? khoaId))
                return;

            try
            {
                const string sql = @"
INSERT INTO BacSi(tenBS, DiaChi, DienThoai, sttKhoa)
VALUES(@tenBS, @DiaChi, @DienThoai, @sttKhoa);";

                Database.Execute(
                    sql,
                    CommandType.Text,
                    new SqlParameter("@tenBS", tenBS),
                    new SqlParameter("@DiaChi", (object)diaChi ?? DBNull.Value),
                    new SqlParameter("@DienThoai", (object)dienThoai ?? DBNull.Value),
                    new SqlParameter("@sttKhoa", (object?)khoaId ?? DBNull.Value));

                LoadBacSi();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm bác sĩ:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một bác sĩ trong danh sách để sửa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out string tenBS,
                               out string diaChi,
                               out string dienThoai,
                               out int? khoaId))
                return;

            try
            {
                const string sql = @"
UPDATE BacSi
SET tenBS   = @tenBS,
    DiaChi  = @DiaChi,
    DienThoai = @DienThoai,
    sttKhoa = @sttKhoa
WHERE sttBS = @sttBS;";

                Database.Execute(
                    sql,
                    CommandType.Text,
                    new SqlParameter("@tenBS", tenBS),
                    new SqlParameter("@DiaChi", (object)diaChi ?? DBNull.Value),
                    new SqlParameter("@DienThoai", (object)dienThoai ?? DBNull.Value),
                    new SqlParameter("@sttKhoa", (object?)khoaId ?? DBNull.Value),
                    new SqlParameter("@sttBS", _currentId.Value));

                LoadBacSi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật bác sĩ:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một bác sĩ trong danh sách để xóa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dr = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa bác sĩ này?\n" +
                "Lưu ý: nếu bác sĩ đang phụ trách bệnh nhân thì có thể không xóa được.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes) return;

            try
            {
                const string sql = "DELETE FROM BacSi WHERE sttBS = @id";
                Database.Execute(sql, CommandType.Text, new SqlParameter("@id", _currentId.Value));

                LoadBacSi();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa bác sĩ.\n" +
                                "Có thể đang tồn tại bệnh nhân do bác sĩ này phụ trách.\n\nChi tiết: " + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== HELPER ==================
        private void ClearInputs()
        {
            _currentId = null;
            _txtTenBS.Clear();
            _txtDiaChi.Clear();
            _txtDienThoai.Clear();
            if (_cbKhoa.Items.Count > 0) _cbKhoa.SelectedIndex = -1;
            _grid.ClearSelection();
        }

        private bool ValidateInput(
            out string tenBS,
            out string diaChi,
            out string dienThoai,
            out int? khoaId)
        {
            tenBS = _txtTenBS.Text.Trim();
            diaChi = _txtDiaChi.Text.Trim();
            dienThoai = _txtDienThoai.Text.Trim();
            khoaId = null;

            if (string.IsNullOrWhiteSpace(tenBS))
            {
                MessageBox.Show("Họ tên bác sĩ không được để trống.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTenBS.Focus();
                return false;
            }

            if (_cbKhoa.SelectedValue != null &&
                int.TryParse(_cbKhoa.SelectedValue.ToString(), out int tmpKhoa))
            {
                khoaId = tmpKhoa;
            }
            else
            {
                MessageBox.Show("Hãy chọn khoa cho bác sĩ.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
