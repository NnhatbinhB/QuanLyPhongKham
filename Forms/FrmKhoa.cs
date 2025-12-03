using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmKhoa : Form
    {
        private DataGridView _gridKhoa;
        private TextBox _txtTenKhoa;
        private Button _btnThem, _btnSua, _btnXoa, _btnLamMoi;
        private ListBox _lstBacSi, _lstBenhNhan, _lstGiuong;
        private Label _lblSummary;

        private int? _currentKhoaId;

        public FrmKhoa()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(232, 245, 233);
            Text = "Quản lý khoa";

            BuildLayout();
            LoadKhoa();
        }

        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            // ===== HÀNG 0: INPUT + BUTTON =====
            var top = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.White
            };
            layout.Controls.Add(top, 0, 0);

            var lblTitle = new Label
            {
                Text = "Quản lý khoa",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32),
                Location = new Point(0, 5)
            };
            top.Controls.Add(lblTitle);

            var lblTen = new Label
            {
                Text = "Tên khoa:",
                AutoSize = true,
                Location = new Point(0, 45)
            };
            top.Controls.Add(lblTen);

            _txtTenKhoa = new TextBox
            {
                Left = 80,
                Top = 40,
                Width = 250
            };
            top.Controls.Add(_txtTenKhoa);

            _btnThem = MakeButton("Thêm", 350, 36, Color.FromArgb(56, 142, 60));
            _btnThem.Click += BtnThem_Click;
            top.Controls.Add(_btnThem);

            _btnSua = MakeButton("Sửa", 450, 36, Color.FromArgb(30, 136, 229));
            _btnSua.Click += BtnSua_Click;
            top.Controls.Add(_btnSua);

            _btnXoa = MakeButton("Xóa", 550, 36, Color.FromArgb(211, 47, 47));
            _btnXoa.Click += BtnXoa_Click;
            top.Controls.Add(_btnXoa);

            _btnLamMoi = MakeButton("Làm mới", 650, 36, Color.FromArgb(120, 144, 156));
            _btnLamMoi.Click += (s, e) => ClearInputs();
            top.Controls.Add(_btnLamMoi);

            _lblSummary = new Label
            {
                AutoSize = true,
                ForeColor = Color.Gray,
                Location = new Point(350, 8)
            };
            top.Controls.Add(_lblSummary);

            top.Resize += (s, e) =>
            {
                _lblSummary.Left = top.Width - _lblSummary.Width - 16;
            };

            // ===== HÀNG 1: GRID + DETAIL =====
            var bottom = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 450,
                BackColor = Color.FromArgb(232, 245, 233)
            };
            layout.Controls.Add(bottom, 0, 1);

            // LEFT: GRID KHOA
            _gridKhoa = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            _gridKhoa.CellClick += GridKhoa_CellClick;
            bottom.Panel1.Padding = new Padding(16, 8, 8, 16);
            bottom.Panel1.Controls.Add(_gridKhoa);

            // RIGHT: DETAIL (BS / BN / GIƯỜNG)
            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
            bottom.Panel2.Padding = new Padding(8, 8, 16, 16);
            bottom.Panel2.Controls.Add(rightLayout);

            _lstBacSi = MakeGroupWithList(rightLayout, 0, "Bác sĩ trong khoa");
            _lstBenhNhan = MakeGroupWithList(rightLayout, 1, "Bệnh nhân trong khoa");
            _lstGiuong = MakeGroupWithList(rightLayout, 2, "Giường bệnh trong khoa");
        }

        private Button MakeButton(string text, int left, int top, Color back)
        {
            var btn = new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private ListBox MakeGroupWithList(TableLayoutPanel parent, int row, string title)
        {
            var group = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            parent.Controls.Add(group, 0, row);

            var list = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };
            group.Controls.Add(list);
            return list;
        }

        // ================== LOAD DATA ==================
        // ================== LOAD DATA ==================
        private void LoadKhoa()
        {
            const string sql = @"
SELECT k.sttKhoa,
       k.TenKhoa,
       COUNT(DISTINCT bs.sttBS)  AS SoBacSi,
       COUNT(DISTINCT bn.sttBN)  AS SoBenhNhan,
       COUNT(DISTINCT gb.sttGB)  AS SoGiuong
FROM   Khoa k
LEFT JOIN BacSi      bs ON bs.sttKhoa = k.sttKhoa
LEFT JOIN BenhNhan   bn ON bn.sttKhoa = k.sttKhoa
LEFT JOIN GiuongBenh gb ON gb.sttKhoa = k.sttKhoa
GROUP BY k.sttKhoa, k.TenKhoa
ORDER BY k.TenKhoa;";

            var dt = Database.Query(sql, CommandType.Text);

            // Tự khai báo cột cho grid để không bị lệ thuộc tên alias tiếng Việt
            _gridKhoa.AutoGenerateColumns = false;
            _gridKhoa.Columns.Clear();

            _gridKhoa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMaKhoa",
                DataPropertyName = "sttKhoa",   // tên cột trong DataTable
                HeaderText = "Mã Khoa",
                Width = 80,
                ReadOnly = true
            });

            _gridKhoa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTenKhoa",
                DataPropertyName = "TenKhoa",
                HeaderText = "Tên khoa",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            _gridKhoa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSoBacSi",
                DataPropertyName = "SoBacSi",
                HeaderText = "Số bác sĩ",
                Width = 90,
                ReadOnly = true
            });

            _gridKhoa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSoBenhNhan",
                DataPropertyName = "SoBenhNhan",
                HeaderText = "Số bệnh nhân",
                Width = 110,
                ReadOnly = true
            });

            _gridKhoa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSoGiuong",
                DataPropertyName = "SoGiuong",
                HeaderText = "Số giường",
                Width = 90,
                ReadOnly = true
            });

            _gridKhoa.DataSource = dt;

            _currentKhoaId = null;
            ClearDetail();

            _lblSummary.Text = $"Tổng số khoa: {dt.Rows.Count}";
        }

        private void LoadDetailForKhoa(int khoaId)
        {
            // Bác sĩ
            const string sqlBs = @"
SELECT tenBS
FROM BacSi
WHERE sttKhoa = @id
ORDER BY tenBS;";

            var dtBs = Database.Query(sqlBs, CommandType.Text,
                new SqlParameter("@id", khoaId));

            _lstBacSi.Items.Clear();
            foreach (DataRow row in dtBs.Rows)
                _lstBacSi.Items.Add(row["tenBS"]?.ToString());

            // Bệnh nhân
            const string sqlBn = @"
SELECT hoten
FROM BenhNhan
WHERE sttKhoa = @id
ORDER BY hoten;";

            var dtBn = Database.Query(sqlBn, CommandType.Text,
                new SqlParameter("@id", khoaId));

            _lstBenhNhan.Items.Clear();
            foreach (DataRow row in dtBn.Rows)
                _lstBenhNhan.Items.Add(row["hoten"]?.ToString());

            // Giường
            const string sqlGb = @"
SELECT TenGiuong, tinhtrang
FROM   GiuongBenh
WHERE  sttKhoa = @id
ORDER BY TenGiuong;";

            var dtGb = Database.Query(sqlGb, CommandType.Text,
                new SqlParameter("@id", khoaId));

            _lstGiuong.Items.Clear();
            foreach (DataRow row in dtGb.Rows)
            {
                var ten = row["TenGiuong"]?.ToString();
                var tt = row["tinhtrang"]?.ToString();
                _lstGiuong.Items.Add($"{ten}: {tt}");
            }
        }

        // ================== EVENT HANDLERS ==================
        private void GridKhoa_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_gridKhoa.CurrentRow?.DataBoundItem is not DataRowView drv) return;

            // Lấy dữ liệu trực tiếp từ DataRowView, KHÔNG dùng tên cột tiếng Việt
            _currentKhoaId = Convert.ToInt32(drv["sttKhoa"]);
            _txtTenKhoa.Text = drv["TenKhoa"]?.ToString() ?? string.Empty;

            LoadDetailForKhoa(_currentKhoaId.Value);
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            string ten = _txtTenKhoa.Text.Trim();
            if (ten == "")
            {
                MessageBox.Show("Tên khoa không được để trống.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string sql = "INSERT INTO Khoa(TenKhoa) VALUES(@ten);";
            try
            {
                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@ten", ten));

                LoadKhoa();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể thêm khoa:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_currentKhoaId == null)
            {
                MessageBox.Show("Hãy chọn một khoa để sửa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string ten = _txtTenKhoa.Text.Trim();
            if (ten == "")
            {
                MessageBox.Show("Tên khoa không được để trống.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string sql = "UPDATE Khoa SET TenKhoa = @ten WHERE sttKhoa = @id;";
            try
            {
                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@ten", ten),
                    new SqlParameter("@id", _currentKhoaId.Value));

                LoadKhoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể cập nhật khoa:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_currentKhoaId == null)
            {
                MessageBox.Show("Hãy chọn một khoa để xóa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            const string sqlCheck = @"
SELECT
    (SELECT COUNT(*) FROM BacSi      WHERE sttKhoa = @id) AS SoBacSi,
    (SELECT COUNT(*) FROM BenhNhan   WHERE sttKhoa = @id) AS SoBenhNhan,
    (SELECT COUNT(*) FROM GiuongBenh WHERE sttKhoa = @id) AS SoGiuong;";

            var dt = Database.Query(sqlCheck, CommandType.Text,
                new SqlParameter("@id", _currentKhoaId.Value));

            int bs = Convert.ToInt32(dt.Rows[0]["SoBacSi"]);
            int bn = Convert.ToInt32(dt.Rows[0]["SoBenhNhan"]);
            int gb = Convert.ToInt32(dt.Rows[0]["SoGiuong"]);

            if (bs > 0 || bn > 0 || gb > 0)
            {
                MessageBox.Show(
                    "Không thể xóa khoa vì vẫn còn dữ liệu liên quan:\n" +
                    $"- Bác sĩ: {bs}\n" +
                    $"- Bệnh nhân: {bn}\n" +
                    $"- Giường bệnh: {gb}\n" +
                    "Hãy chuyển hoặc xóa dữ liệu trước.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dr = MessageBox.Show("Bạn có chắc chắn muốn xóa khoa này?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            const string sqlDelete = "DELETE FROM Khoa WHERE sttKhoa = @id;";
            try
            {
                Database.Execute(sqlDelete, CommandType.Text,
                    new SqlParameter("@id", _currentKhoaId.Value));

                LoadKhoa();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa khoa:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== HELPER ==================
        private void ClearInputs()
        {
            _currentKhoaId = null;
            _txtTenKhoa.Clear();
            _gridKhoa.ClearSelection();
            ClearDetail();
        }

        private void ClearDetail()
        {
            _lstBacSi.Items.Clear();
            _lstBenhNhan.Items.Clear();
            _lstGiuong.Items.Clear();
        }
    }
}
