using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;   // dùng Database + EmailHelper

namespace QuanLyPhongKham.Forms
{
    public partial class FrmLichHen : Form
    {
        private DataGridView _grid;
        private ComboBox _cbBenhNhan;
        private ComboBox _cbBacSi;
        private ComboBox _cbTrangThai;
        private DateTimePicker _dtNgayHen;
        private DateTimePicker _dtGioHen;
        private TextBox _txtNoiDung;
        private TextBox _txtSearch;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Button _btnThem, _btnSua, _btnXoa, _btnLamMoi;
        private Label _lblTong;

        // Email bệnh nhân (lấy từ bảng BenhNhan)
        private TextBox _txtEmail;

        // Thông tin phiếu khám (lịch sử khám)
        private TextBox _txtTrieuChung;
        private TextBox _txtChanDoan;
        private TextBox _txtGhiChu;

        private DataTable? _lichHenSource;
        private int? _currentId;

        // Tiền khám mặc định (chỉ dùng nội bộ để lưu DB, KHÔNG gửi cho bệnh nhân)
        private const int DEFAULT_TIEN_KHAM = 200000;

        public FrmLichHen()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(232, 245, 233);
            Text = "Lịch hẹn khám";

            BuildLayout();
            LoadBenhNhan();
            LoadBacSi();
            LoadLichHen();
        }

        // ================== UI ==================
        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220)); // thông tin lịch hẹn + phiếu khám
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // filter
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // grid
            Controls.Add(layout);

            // ====== HÀNG 0: THÔNG TIN LỊCH HẸN + PHIẾU KHÁM ======
            var top = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.White
            };
            layout.Controls.Add(top, 0, 0);

            int y = 6;

            // --- BỆNH NHÂN + BÁC SĨ + TRẠNG THÁI ---
            var lblBN = new Label
            {
                Text = "Bệnh nhân:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            top.Controls.Add(lblBN);

            _cbBenhNhan = new ComboBox
            {
                Location = new Point(90, y),
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            top.Controls.Add(_cbBenhNhan);

            var lblBS = new Label
            {
                Text = "Bác sĩ:",
                AutoSize = true,
                Location = new Point(340, y + 4)
            };
            top.Controls.Add(lblBS);

            _cbBacSi = new ComboBox
            {
                Location = new Point(390, y),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            top.Controls.Add(_cbBacSi);

            var lblTrangThai = new Label
            {
                Text = "Trạng thái:",
                AutoSize = true,
                Location = new Point(640, y + 4)
            };
            top.Controls.Add(lblTrangThai);

            _cbTrangThai = new ComboBox
            {
                Location = new Point(720, y),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cbTrangThai.Items.AddRange(new object[]
            {
                "Chờ khám",
                "Đã xác nhận",
                "Đã khám",
                "Hủy",
                "Vắng"
            });
            _cbTrangThai.SelectedIndex = 0;
            top.Controls.Add(_cbTrangThai);

            y += 32;

            // --- NGÀY / GIỜ HẸN + NỘI DUNG ---
            var lblNgay = new Label
            {
                Text = "Ngày hẹn:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            top.Controls.Add(lblNgay);

            _dtNgayHen = new DateTimePicker
            {
                Location = new Point(90, y),
                Width = 150,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy"
            };
            top.Controls.Add(_dtNgayHen);

            var lblGio = new Label
            {
                Text = "Giờ hẹn:",
                AutoSize = true,
                Location = new Point(260, y + 4)
            };
            top.Controls.Add(lblGio);

            _dtGioHen = new DateTimePicker
            {
                Location = new Point(320, y),
                Width = 100,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true
            };
            top.Controls.Add(_dtGioHen);

            var lblNoiDung = new Label
            {
                Text = "Nội dung:",
                AutoSize = true,
                Location = new Point(440, y + 4)
            };
            top.Controls.Add(lblNoiDung);

            _txtNoiDung = new TextBox
            {
                Location = new Point(510, y),
                Width = 390
            };
            top.Controls.Add(_txtNoiDung);

            y += 32;

            // --- EMAIL BỆNH NHÂN ---
            var lblEmail = new Label
            {
                Text = "Email:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            top.Controls.Add(lblEmail);

            _txtEmail = new TextBox
            {
                Location = new Point(90, y),
                Width = 230,
                ReadOnly = false
            };
            top.Controls.Add(_txtEmail);

            y += 32;

            // --- TRIỆU CHỨNG + CHẨN ĐOÁN ---
            var lblTrieuChung = new Label
            {
                Text = "Triệu chứng:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            top.Controls.Add(lblTrieuChung);

            _txtTrieuChung = new TextBox
            {
                Location = new Point(90, y),
                Width = 310
            };
            top.Controls.Add(_txtTrieuChung);

            var lblChanDoan = new Label
            {
                Text = "Chẩn đoán:",
                AutoSize = true,
                Location = new Point(420, y + 4)
            };
            top.Controls.Add(lblChanDoan);

            _txtChanDoan = new TextBox
            {
                Location = new Point(500, y),
                Width = 400
            };
            top.Controls.Add(_txtChanDoan);

            y += 32;

            // --- GHI CHÚ ---
            var lblGhiChu = new Label
            {
                Text = "Ghi chú:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            top.Controls.Add(lblGhiChu);

            _txtGhiChu = new TextBox
            {
                Location = new Point(90, y),
                Width = 810
            };
            top.Controls.Add(_txtGhiChu);

            y += 36;

            // --- NÚT HÀNH ĐỘNG ---
            _btnThem = MakeActionButton("Thêm", 90, y, Color.FromArgb(56, 142, 60));
            _btnThem.Click += BtnThem_Click;
            top.Controls.Add(_btnThem);

            _btnSua = MakeActionButton("Sửa", 190, y, Color.FromArgb(30, 136, 229));
            _btnSua.Click += BtnSua_Click;
            top.Controls.Add(_btnSua);

            _btnXoa = MakeActionButton("Xóa", 290, y, Color.FromArgb(211, 47, 47));
            _btnXoa.Click += BtnXoa_Click;
            top.Controls.Add(_btnXoa);

            _btnLamMoi = MakeActionButton("Làm mới", 390, y, Color.FromArgb(120, 144, 156));
            _btnLamMoi.Click += (s, e) =>
            {
                ClearInputs();
                LoadLichHen();
            };
            top.Controls.Add(_btnLamMoi);

            var btnGuiPhieuKham = new Button
            {
                Text = "Lập phiếu khám + gửi email",
                Width = 230,
                Height = 36,
                Left = 490,
                Top = y,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuiPhieuKham.FlatAppearance.BorderSize = 0;
            btnGuiPhieuKham.Click += BtnGuiPhieuKham_Click;
            top.Controls.Add(btnGuiPhieuKham);

            _lblTong = new Label
            {
                Text = "Tổng: 0 lịch hẹn",
                AutoSize = true,
                Location = new Point(740, y + 8),
                ForeColor = Color.FromArgb(27, 94, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            top.Controls.Add(_lblTong);

            // ====== HÀNG 1: FILTER ======
            var filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(245, 251, 245)
            };
            layout.Controls.Add(filterPanel, 0, 1);

            int fy = 10;
            var lblFrom = new Label
            {
                Text = "Từ ngày:",
                AutoSize = true,
                Location = new Point(0, fy + 4)
            };
            filterPanel.Controls.Add(lblFrom);

            _dtFrom = new DateTimePicker
            {
                Location = new Point(70, fy),
                Width = 130,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value = DateTime.Today.AddDays(-7)
            };
            filterPanel.Controls.Add(_dtFrom);

            var lblTo = new Label
            {
                Text = "Đến ngày:",
                AutoSize = true,
                Location = new Point(220, fy + 4)
            };
            filterPanel.Controls.Add(lblTo);

            _dtTo = new DateTimePicker
            {
                Location = new Point(300, fy),
                Width = 130,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value = DateTime.Today.AddDays(7)
            };
            filterPanel.Controls.Add(_dtTo);

            var btnReload = new Button
            {
                Text = "Tải dữ liệu",
                Width = 100,
                Height = 28,
                Location = new Point(450, fy - 1),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White
            };
            btnReload.FlatAppearance.BorderSize = 0;
            btnReload.Click += (s, e) => LoadLichHen();
            filterPanel.Controls.Add(btnReload);

            var lblSearch = new Label
            {
                Text = "Tìm kiếm:",
                AutoSize = true,
                Location = new Point(580, fy + 4)
            };
            filterPanel.Controls.Add(lblSearch);

            _txtSearch = new TextBox
            {
                Location = new Point(650, fy),
                Width = 250
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            filterPanel.Controls.Add(_txtSearch);

            // ====== HÀNG 2: GRID ======
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 16, 16)
            };
            layout.Controls.Add(gridPanel, 0, 2);

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
            gridPanel.Controls.Add(_grid);
        }

        private Button MakeActionButton(string text, int left, int top, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Width = 90,
                Height = 36,
                Left = left,
                Top = top,
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ================== LOAD COMBO ==================
        private void LoadBenhNhan()
        {
            try
            {
                const string sql = @"SELECT sttBN, hoten, Email FROM BenhNhan ORDER BY hoten";
                var dt = Database.Query(sql, CommandType.Text) ?? new DataTable();

                _cbBenhNhan.DataSource = dt;
                _cbBenhNhan.DisplayMember = "hoten";
                _cbBenhNhan.ValueMember = "sttBN";
                _cbBenhNhan.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;

                _cbBenhNhan.SelectedIndexChanged += (s, e) =>
                {
                    if (_cbBenhNhan.SelectedItem is DataRowView dv)
                    {
                        _txtEmail.Text = dv["Email"] == DBNull.Value
                            ? string.Empty
                            : Convert.ToString(dv["Email"]);
                    }
                    else
                    {
                        _txtEmail.Text = string.Empty;
                    }
                };

                if (_cbBenhNhan.SelectedItem is DataRowView dv0)
                {
                    _txtEmail.Text = dv0["Email"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(dv0["Email"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bệnh nhân:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBacSi()
        {
            try
            {
                const string sql = @"SELECT sttBS, tenBS FROM BacSi ORDER BY tenBS";
                var dt = Database.Query(sql, CommandType.Text) ?? new DataTable();
                _cbBacSi.DataSource = dt;
                _cbBacSi.DisplayMember = "tenBS";
                _cbBacSi.ValueMember = "sttBS";
                _cbBacSi.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bác sĩ:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== LOAD LỊCH HẸN ==================
        private void LoadLichHen()
        {
            try
            {
                DateTime from = _dtFrom.Value.Date;
                DateTime to = _dtTo.Value.Date.AddDays(1).AddTicks(-1);

                const string sql = @"
SELECT lh.sttLH,
       lh.NgayHen,
       lh.TrangThai,
       lh.NoiDung,
       lh.sttBN,
       lh.sttBS,
       bn.hoten   AS TenBenhNhan,
       bn.CMND,
       bn.Email   AS Email,
       bs.tenBS   AS TenBacSi
FROM   LichHen lh
       JOIN BenhNhan bn ON lh.sttBN = bn.sttBN
       JOIN BacSi   bs ON lh.sttBS = bs.sttBS
WHERE  lh.NgayHen BETWEEN @from AND @to
ORDER BY lh.NgayHen;";

                _lichHenSource = Database.Query(sql, CommandType.Text,
                    new SqlParameter("@from", from),
                    new SqlParameter("@to", to)) ?? new DataTable();

                _grid.DataSource = _lichHenSource;

                try
                {
                    if (_grid.Columns.Contains("sttLH"))
                    {
                        _grid.Columns["sttLH"].HeaderText = "Mã LH";
                        _grid.Columns["sttLH"].Width = 70;
                    }

                    if (_grid.Columns.Contains("NgayHen"))
                        _grid.Columns["NgayHen"].HeaderText = "Ngày giờ hẹn";

                    if (_grid.Columns.Contains("TenBenhNhan"))
                        _grid.Columns["TenBenhNhan"].HeaderText = "Bệnh nhân";

                    if (_grid.Columns.Contains("TenBacSi"))
                        _grid.Columns["TenBacSi"].HeaderText = "Bác sĩ";

                    if (_grid.Columns.Contains("TrangThai"))
                        _grid.Columns["TrangThai"].HeaderText = "Trạng thái";

                    if (_grid.Columns.Contains("NoiDung"))
                        _grid.Columns["NoiDung"].HeaderText = "Nội dung";

                    if (_grid.Columns.Contains("CMND"))
                        _grid.Columns["CMND"].HeaderText = "CMND";

                    if (_grid.Columns.Contains("Email"))
                    {
                        _grid.Columns["Email"].HeaderText = "Email";
                        _grid.Columns["Email"].Width = 200;
                    }

                    if (_grid.Columns.Contains("sttBN"))
                        _grid.Columns["sttBN"].Visible = false;
                    if (_grid.Columns.Contains("sttBS"))
                        _grid.Columns["sttBS"].Visible = false;
                }
                catch
                {
                    // ignore format error
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch hẹn:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_lichHenSource == null) return;

            string keyword = _txtSearch.Text.Trim().Replace("'", "''");

            if (string.IsNullOrEmpty(keyword))
            {
                _lichHenSource.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                _lichHenSource.DefaultView.RowFilter =
                    $"TenBenhNhan LIKE '%{keyword}%' " +
                    $"OR TenBacSi LIKE '%{keyword}%' " +
                    $"OR CMND LIKE '%{keyword}%' " +
                    $"OR TrangThai LIKE '%{keyword}%' " +
                    $"OR NoiDung LIKE '%{keyword}%' " +
                    $"OR Email LIKE '%{keyword}%'";
            }

            UpdateCountLabel();
        }

        private void UpdateCountLabel()
        {
            int count = _lichHenSource?.DefaultView.Count ?? 0;
            _lblTong.Text = $"Tổng: {count} lịch hẹn";
        }

        // ================== GRID EVENT ==================
        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.CurrentRow == null) return;

            var row = _grid.CurrentRow;
            try
            {
                if (row.Cells["sttLH"].Value == null) return;

                _currentId = Convert.ToInt32(row.Cells["sttLH"].Value);

                DateTime ngayHen = Convert.ToDateTime(row.Cells["NgayHen"].Value);
                _dtNgayHen.Value = ngayHen.Date;
                _dtGioHen.Value = DateTime.Today.Date + ngayHen.TimeOfDay;

                _txtNoiDung.Text = Convert.ToString(row.Cells["NoiDung"].Value);

                if (row.Cells["TrangThai"].Value != null)
                {
                    string tt = row.Cells["TrangThai"].Value.ToString() ?? "";
                    int idx = _cbTrangThai.FindStringExact(tt);
                    _cbTrangThai.SelectedIndex = idx >= 0 ? idx : 0;
                }

                if (row.Cells["sttBN"].Value != null)
                    _cbBenhNhan.SelectedValue = Convert.ToInt32(row.Cells["sttBN"].Value);

                if (row.Cells["sttBS"].Value != null)
                    _cbBacSi.SelectedValue = Convert.ToInt32(row.Cells["sttBS"].Value);

                if (_grid.Columns.Contains("Email") && row.Cells["Email"].Value != null)
                {
                    _txtEmail.Text = row.Cells["Email"].Value.ToString();
                }
            }
            catch
            {
                // ignore mapping error
            }
        }

        // ================== BUTTONS ==================
        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(out int sttBN, out int sttBS, out DateTime ngayHen,
                    out string trangThai, out string noiDung))
                return;

            string email = _txtEmail.Text.Trim();

            try
            {
                const string sql = @"
INSERT INTO LichHen(sttBN, sttBS, NgayHen, TrangThai, NoiDung)
VALUES(@sttBN, @sttBS, @NgayHen, @TrangThai, @NoiDung);";

                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@sttBN", sttBN),
                    new SqlParameter("@sttBS", sttBS),
                    new SqlParameter("@NgayHen", ngayHen),
                    new SqlParameter("@TrangThai", trangThai),
                    new SqlParameter("@NoiDung", (object)noiDung ?? DBNull.Value));

                if (!string.IsNullOrEmpty(email))
                {
                    const string sqlEmail = @"UPDATE BenhNhan SET Email = @Email WHERE sttBN = @sttBN;";
                    Database.Execute(sqlEmail, CommandType.Text,
                        new SqlParameter("@Email", email),
                        new SqlParameter("@sttBN", sttBN));
                }

                LoadLichHen();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm lịch hẹn:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một lịch hẹn để sửa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out int sttBN, out int sttBS, out DateTime ngayHen,
                    out string trangThai, out string noiDung))
                return;

            string email = _txtEmail.Text.Trim();

            try
            {
                const string sql = @"
UPDATE LichHen
SET sttBN     = @sttBN,
    sttBS     = @sttBS,
    NgayHen   = @NgayHen,
    TrangThai = @TrangThai,
    NoiDung   = @NoiDung
WHERE sttLH   = @sttLH;";

                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@sttBN", sttBN),
                    new SqlParameter("@sttBS", sttBS),
                    new SqlParameter("@NgayHen", ngayHen),
                    new SqlParameter("@TrangThai", trangThai),
                    new SqlParameter("@NoiDung", (object)noiDung ?? DBNull.Value),
                    new SqlParameter("@sttLH", _currentId.Value));

                if (!string.IsNullOrEmpty(email))
                {
                    const string sqlEmail = @"UPDATE BenhNhan SET Email = @Email WHERE sttBN = @sttBN;";
                    Database.Execute(sqlEmail, CommandType.Text,
                        new SqlParameter("@Email", email),
                        new SqlParameter("@sttBN", sttBN));
                }

                LoadLichHen();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật lịch hẹn:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một lịch hẹn để xóa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dr = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa lịch hẹn này?\n" +
                "Lưu ý: nếu đã khám thì nên dùng nút 'Lập phiếu khám + gửi email' thay vì xóa tay.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes) return;

            try
            {
                const string sql = @"DELETE FROM LichHen WHERE sttLH = @sttLH;";
                Database.Execute(sql, CommandType.Text,
                    new SqlParameter("@sttLH", _currentId.Value));

                LoadLichHen();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa lịch hẹn.\nChi tiết: " + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ======== LẬP PHIẾU KHÁM + GỬI EMAIL ========
        private void BtnGuiPhieuKham_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một lịch hẹn để lập phiếu khám và gửi email.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string trieuChung = _txtTrieuChung.Text.Trim();
            string chanDoan = _txtChanDoan.Text.Trim();
            string ghiChu = _txtGhiChu.Text.Trim();

            // vẫn dùng DEFAULT_TIEN_KHAM để lưu DB, nhưng KHÔNG hiển thị cho bệnh nhân
            int tienKham = DEFAULT_TIEN_KHAM;

            var confirm = MessageBox.Show(
                "Hệ thống sẽ:\n" +
                "- Lưu thông tin vào LỊCH SỬ KHÁM (bảng Phiếu khám)\n" +
                "- Gửi phiếu khám qua email cho bệnh nhân (nếu có email)\n" +
                "- XÓA lịch hẹn này khỏi danh sách lịch hẹn\n\n" +
                "Bạn có chắc chắn muốn thực hiện?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            DataTable? result;
            try
            {
                result = Database.Query(
                    "sp_LichHen_HoanTatVaLuuLichSu",
                    CommandType.StoredProcedure,
                    new SqlParameter("@sttLH", _currentId.Value),
                    new SqlParameter("@TrieuChung", (object)trieuChung ?? DBNull.Value),
                    new SqlParameter("@ChanDoan", (object)chanDoan ?? DBNull.Value),
                    new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value),
                    new SqlParameter("@TienKham", tienKham)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu lịch sử khám / xóa lịch hẹn:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (result == null || result.Rows.Count == 0)
            {
                MessageBox.Show("Không nhận được dữ liệu trả về để gửi email.",
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadLichHen();
                ClearInputs();
                return;
            }

            var row = result.Rows[0];

            // Lấy email từ kết quả SP, nếu rỗng thì fallback sang email đang hiển thị trên UI
            string email = "";
            if (result.Columns.Contains("EmailBenhNhan"))
                email = Convert.ToString(row["EmailBenhNhan"]) ?? "";
            if (string.IsNullOrWhiteSpace(email))
                email = _txtEmail.Text.Trim();

            string tenBN = Convert.ToString(row["TenBenhNhan"]) ?? "";
            string tenBS = Convert.ToString(row["TenBacSi"]) ?? "";
            DateTime ngay = Convert.ToDateTime(row["NgayKham"]);

            string subject = $"Phiếu khám bệnh - {tenBN} - {ngay:dd/MM/yyyy}";
            string body =
                $"Kính gửi {tenBN},\n\n" +
                $"Thông tin khám của bạn:\n" +
                $"- Thời gian khám: {ngay:dd/MM/yyyy HH:mm}\n" +
                $"- Bác sĩ phụ trách: {tenBS}\n" +
                $"- Triệu chứng: {trieuChung}\n" +
                $"- Chẩn đoán: {chanDoan}\n" +
                $"- Ghi chú: {ghiChu}\n\n" +
                "Nếu bạn có thắc mắc về lịch hẹn hoặc kết quả khám, vui lòng liên hệ phòng khám.\n\n" +
                "Trân trọng,\n" +
                "Phòng khám Đa khoa Đức Bình";

            // Không có email bệnh nhân -> chỉ lưu phiếu khám, không gửi mail
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Đã lưu lịch sử khám và xóa lịch hẹn.\n" +
                    "Tuy nhiên bệnh nhân không có email nên không thể gửi phiếu khám.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadLichHen();
                ClearInputs();
                return;
            }

            // ==== GỬI MAIL QUA EMAILHELPER ====
            try
            {
                EmailHelper.SendClinicMail(email, subject, body);

                MessageBox.Show(
                    $"Đã gửi phiếu khám tới email: {email}",
                    "Gửi email", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exMail)
            {
                MessageBox.Show(
                    "Đã lưu lịch sử khám và xóa lịch hẹn, nhưng gửi email thất bại:\n" + exMail.Message,
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            LoadLichHen();
            ClearInputs();
        }

        // ================== HELPER ==================
        private void ClearInputs()
        {
            _currentId = null;
            if (_cbBenhNhan.Items.Count > 0) _cbBenhNhan.SelectedIndex = 0;
            if (_cbBacSi.Items.Count > 0) _cbBacSi.SelectedIndex = 0;
            _cbTrangThai.SelectedIndex = 0;
            _dtNgayHen.Value = DateTime.Today;
            _dtGioHen.Value = DateTime.Today.Date.AddHours(8);
            _txtNoiDung.Clear();
            _txtSearch.Clear();
            _txtEmail.Clear();
            _txtTrieuChung.Clear();
            _txtChanDoan.Clear();
            _txtGhiChu.Clear();
            _grid.ClearSelection();
        }

        private bool ValidateInput(
            out int sttBN,
            out int sttBS,
            out DateTime ngayHen,
            out string trangThai,
            out string noiDung)
        {
            sttBN = 0;
            sttBS = 0;
            ngayHen = DateTime.Now;
            trangThai = "";
            noiDung = _txtNoiDung.Text.Trim();

            if (_cbBenhNhan.SelectedValue == null ||
                !int.TryParse(_cbBenhNhan.SelectedValue.ToString(), out sttBN))
            {
                MessageBox.Show("Hãy chọn bệnh nhân.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cbBacSi.SelectedValue == null ||
                !int.TryParse(_cbBacSi.SelectedValue.ToString(), out sttBS))
            {
                MessageBox.Show("Hãy chọn bác sĩ phụ trách.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            ngayHen = _dtNgayHen.Value.Date + _dtGioHen.Value.TimeOfDay;
            trangThai = _cbTrangThai.SelectedItem?.ToString() ?? "Chờ khám";

            return true;
        }
    }
}
