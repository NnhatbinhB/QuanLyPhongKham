using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmPhanQuyen : Form
    {
        private DataGridView _grid;

        private TextBox _txtMaBS;
        private TextBox _txtTenBS;
        private TextBox _txtKhoa;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private TextBox _txtConfirm;

        private Button _btnLuu;
        private Button _btnReset;
        private Button _btnThemTK;   // nút thêm tài khoản hệ thống

        // id bác sĩ (có thể null nếu là tài khoản hệ thống)
        private int? _currentDoctorId;
        // id tài khoản (MaTK trong bảng TaiKhoan) – null nếu bác sĩ chưa có tài khoản
        private int? _currentAccountId;

        private DataTable? _src;

        public FrmPhanQuyen()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(232, 245, 233);
            Text = "Phân quyền & cấp tài khoản";

            BuildLayout();
            LoadAccounts();
        }

        // ================== UI ==================
        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            Controls.Add(layout);

            // LEFT: danh sách bác sĩ + tài khoản
            var left = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };
            layout.Controls.Add(left, 0, 0);

            var lblLeftTitle = new Label
            {
                Text = "Danh sách bác sĩ && tài khoản",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32),
                TextAlign = ContentAlignment.MiddleLeft
            };
            left.Controls.Add(lblLeftTitle);

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
            left.Controls.Add(_grid);

            // RIGHT: thông tin tài khoản
            var right = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                BackColor = Color.White
            };
            layout.Controls.Add(right, 1, 0);

            int y = 10;

            var lblTitle = new Label
            {
                Text = "Cấp / đổi tài khoản người dùng",
                AutoSize = true,
                Location = new Point(0, y),
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 118, 210)
            };
            right.Controls.Add(lblTitle);
            y += 34;

            // helper tạo 1 dòng label + textbox
            Label NewLineLabel(string caption, ref int yy, out TextBox txt, bool readOnly = false)
            {
                var lbl = new Label
                {
                    Text = caption,
                    AutoSize = true,
                    Location = new Point(0, yy + 5)
                };
                right.Controls.Add(lbl);

                txt = new TextBox
                {
                    Location = new Point(120, yy),
                    Width = 240,
                    ReadOnly = readOnly
                };
                right.Controls.Add(txt);

                yy += 32;
                return lbl;
            }

            NewLineLabel("Mã bác sĩ:", ref y, out _txtMaBS, readOnly: true);
            NewLineLabel("Tên bác sĩ:", ref y, out _txtTenBS, readOnly: true);
            NewLineLabel("Khoa:", ref y, out _txtKhoa, readOnly: true);
            NewLineLabel("Tài khoản:", ref y, out _txtUsername, readOnly: false);
            NewLineLabel("Mật khẩu:", ref y, out _txtPassword, readOnly: false);
            _txtPassword.UseSystemPasswordChar = true;
            NewLineLabel("Nhập lại MK:", ref y, out _txtConfirm, readOnly: false);
            _txtConfirm.UseSystemPasswordChar = true;

            y += 10;

            _btnLuu = new Button
            {
                Text = "Cấp / cập nhật tài khoản",
                Width = 220,
                Height = 34,
                Location = new Point(0, y),
                BackColor = Color.FromArgb(56, 142, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnLuu.FlatAppearance.BorderSize = 0;
            _btnLuu.Click += BtnLuu_Click;
            right.Controls.Add(_btnLuu);

            _btnReset = new Button
            {
                Text = "Đặt lại mật khẩu mặc định",
                Width = 220,
                Height = 34,
                Location = new Point(230, y),
                BackColor = Color.FromArgb(255, 143, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnReset.FlatAppearance.BorderSize = 0;
            _btnReset.Click += BtnReset_Click;
            right.Controls.Add(_btnReset);

            y += 44;

            _btnThemTK = new Button
            {
                Text = "Thêm tài khoản hệ thống",
                Width = 220,
                Height = 34,
                Location = new Point(0, y),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnThemTK.FlatAppearance.BorderSize = 0;
            _btnThemTK.Click += BtnThemTK_Click;
            right.Controls.Add(_btnThemTK);
        }

        // ================== LOAD DATA ==================
        private void LoadAccounts()
        {
            try
            {
                // Bảng TaiKhoan: MaTK, username, password, VaiTro, sttBS (NULL nếu là tài khoản hệ thống)
                const string sql = @"
SELECT * FROM (
    SELECT  b.sttBS,
            b.tenBS,
            ISNULL(k.TenKhoa, N'') AS TenKhoa,
            tk.MaTK,
            tk.username,
            ISNULL(tk.VaiTro, N'User') AS VaiTro
    FROM BacSi b
    LEFT JOIN Khoa     k  ON b.sttKhoa = k.sttKhoa
    LEFT JOIN TaiKhoan tk ON tk.sttBS  = b.sttBS

    UNION ALL

    SELECT NULL AS sttBS,
           N'(Tài khoản hệ thống)' AS tenBS,
           N'' AS TenKhoa,
           tk.MaTK,
           tk.username,
           ISNULL(tk.VaiTro, N'User') AS VaiTro
    FROM TaiKhoan tk
    WHERE tk.sttBS IS NULL
) X
ORDER BY X.tenBS;";

                _src = Database.Query(sql, CommandType.Text) ?? new DataTable();
                _grid.DataSource = _src;

                try
                {
                    DataGridViewColumn? col;

                    col = _grid.Columns["sttBS"];
                    if (col != null)
                    {
                        col.HeaderText = "Mã BS";
                        col.Width = 60;
                    }

                    col = _grid.Columns["tenBS"];
                    if (col != null)
                    {
                        col.HeaderText = "Tên bác sĩ / TK";
                        col.Width = 160;
                    }

                    col = _grid.Columns["TenKhoa"];
                    if (col != null)
                    {
                        col.HeaderText = "Khoa";
                        col.Width = 120;
                    }

                    col = _grid.Columns["username"];
                    if (col != null)
                    {
                        col.HeaderText = "Tài khoản";
                        col.Width = 120;
                    }

                    col = _grid.Columns["VaiTro"];
                    if (col != null)
                    {
                        col.HeaderText = "Vai trò";
                        col.Width = 80;
                    }

                    if (_grid.Columns.Contains("MaTK"))
                        _grid.Columns["MaTK"].Visible = false;
                }
                catch
                {
                    // tránh crash nếu DataGridView bị lỗi style
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách người dùng:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ClearFormOnly();
        }

        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.CurrentRow == null) return;

            var row = _grid.CurrentRow;

            try
            {
                _currentDoctorId = row.Cells["sttBS"].Value == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(row.Cells["sttBS"].Value);

                _currentAccountId = row.Cells["MaTK"].Value == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(row.Cells["MaTK"].Value);

                _txtMaBS.Text = _currentDoctorId?.ToString() ?? "";
                _txtTenBS.Text = row.Cells["tenBS"].Value?.ToString() ?? "";
                _txtKhoa.Text = row.Cells["TenKhoa"].Value?.ToString() ?? "";
                _txtUsername.Text = row.Cells["username"].Value?.ToString() ?? "";

                _txtPassword.Clear();
                _txtConfirm.Clear();
            }
            catch
            {
                // ignore mapping error
            }
        }

        // ================== BUTTON: THÊM TÀI KHOẢN HỆ THỐNG ==================
        private void BtnThemTK_Click(object? sender, EventArgs e)
        {
            // chế độ tạo tài khoản không gắn với bác sĩ (sttBS = NULL)
            _currentDoctorId = null;
            _currentAccountId = null;

            _txtMaBS.Text = "";
            _txtTenBS.Text = "(Tài khoản hệ thống)";
            _txtKhoa.Text = "";
            _txtUsername.Clear();
            _txtPassword.Clear();
            _txtConfirm.Clear();

            _txtUsername.Focus();
        }

        // ================== BUTTON: LƯU TÀI KHOẢN ==================
        private void BtnLuu_Click(object? sender, EventArgs e)
        {
            string username = _txtUsername.Text.Trim();
            string password = _txtPassword.Text.Trim();
            string confirm = _txtConfirm.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Tài khoản không được để trống.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsername.Focus();
                return;
            }

            try
            {
                if (_currentAccountId == null)
                {
                    // ============== TẠO TÀI KHOẢN MỚI ==============
                    if (string.IsNullOrEmpty(password))
                    {
                        MessageBox.Show("Hãy nhập mật khẩu cho tài khoản mới.",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _txtPassword.Focus();
                        return;
                    }

                    if (password != confirm)
                    {
                        MessageBox.Show("Mật khẩu nhập lại không khớp.",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _txtConfirm.Focus();
                        return;
                    }

                    if (_currentDoctorId.HasValue)
                    {
                        // tài khoản gắn với bác sĩ
                        const string sqlInsertDoc = @"
INSERT INTO TaiKhoan(username, password, VaiTro, sttBS)
VALUES(@username, @password, N'User', @sttBS);";

                        Database.Execute(sqlInsertDoc, CommandType.Text,
                            new SqlParameter("@username", username),
                            new SqlParameter("@password", password),
                            new SqlParameter("@sttBS", _currentDoctorId.Value));

                        MessageBox.Show("Đã tạo tài khoản cho bác sĩ.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // tài khoản hệ thống (không gắn bác sĩ)
                        const string sqlInsertSys = @"
INSERT INTO TaiKhoan(username, password, VaiTro, sttBS)
VALUES(@username, @password, N'User', NULL);";

                        Database.Execute(sqlInsertSys, CommandType.Text,
                            new SqlParameter("@username", username),
                            new SqlParameter("@password", password));

                        MessageBox.Show("Đã tạo tài khoản hệ thống mới.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // ============== CẬP NHẬT TÀI KHOẢN ==============
                    if (!string.IsNullOrEmpty(password) && password != confirm)
                    {
                        MessageBox.Show("Mật khẩu nhập lại không khớp.",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _txtConfirm.Focus();
                        return;
                    }

                    if (!string.IsNullOrEmpty(password))
                    {
                        const string sqlUpdateFull = @"
UPDATE TaiKhoan
SET username = @username,
    password     = @password
WHERE MaTK = @id;";

                        Database.Execute(sqlUpdateFull, CommandType.Text,
                            new SqlParameter("@username", username),
                            new SqlParameter("@password", password),
                            new SqlParameter("@id", _currentAccountId.Value));
                    }
                    else
                    {
                        const string sqlUpdateName = @"
UPDATE TaiKhoan
SET username = @username
WHERE MaTK = @id;";

                        Database.Execute(sqlUpdateName, CommandType.Text,
                            new SqlParameter("@username", username),
                            new SqlParameter("@id", _currentAccountId.Value));
                    }

                    MessageBox.Show("Đã cập nhật thông tin tài khoản.",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu thông tin tài khoản:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== BUTTON: RESET MẬT KHẨU ==================
        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (_currentAccountId == null)
            {
                MessageBox.Show("Hãy chọn một tài khoản trong danh sách để đặt lại mật khẩu.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dr = MessageBox.Show(
                "Đặt lại mật khẩu về '123' cho tài khoản này?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                const string sql = @"UPDATE TaiKhoan SET password = @password WHERE MaTK = @id;";
                Database.Execute(
                    sql,
                    CommandType.Text,
                    new SqlParameter("@password", "123"),
                    new SqlParameter("@id", _currentAccountId.Value));

                MessageBox.Show("Đã đặt lại mật khẩu về '123'.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _txtPassword.Clear();
                _txtConfirm.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đặt lại mật khẩu:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== HELPER ==================
        private void ClearFormOnly()
        {
            _currentDoctorId = null;
            _currentAccountId = null;

            _txtMaBS.Clear();
            _txtTenBS.Clear();
            _txtKhoa.Clear();
            _txtUsername.Clear();
            _txtPassword.Clear();
            _txtConfirm.Clear();
        }
    }
}
