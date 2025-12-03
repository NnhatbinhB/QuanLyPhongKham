using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmBenhNhan : Form
    {
        // ====== CONTROL ======
        private DataGridView _grid;             // danh sách bệnh nhân
        private DataGridView _gridLichSu;       // lịch sử khám của BN đang chọn

        private TextBox _txtHoTen;
        private TextBox _txtCMND;
        private TextBox _txtDiaChi;
        private ComboBox _cbGioiTinh;
        private ComboBox _cbBacSi;
        private DateTimePicker _dtNgayKham;
        private RadioButton _radThuong;
        private RadioButton _radNamGiuong;
        private Button _btnThem, _btnSua, _btnXoa, _btnLamMoi;
        private Label _lblTong;

        // Khoa (chỉ xem, tự theo bác sĩ – nếu DB không có thì để “(chưa rõ)”)
        private Label _lblKhoaValue;

        // --- TÌM KIẾM ---
        private TextBox _txtTimKiem;
        private Label _lblTimKiem;

        // nguồn dữ liệu cho grid để filter
        private DataTable? _benhNhanSource;

        // nguồn dữ liệu bác sĩ (để lookup khoa – nếu không có TenKhoa thì sẽ bỏ qua)
        private DataTable? _bacSiSource;

        // nguồn dữ liệu lịch sử khám của BN
        private DataTable? _lichSuSource;

        private int? _currentId; // sttBN đang chọn

        public FrmBenhNhan()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F);
            Text = "Quản lý bệnh nhân";

            BuildUI();
            LoadDanhSachBacSi(_cbBacSi);   // phải trước LoadBenhNhan để combo có dữ liệu
            LoadBenhNhan();
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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            // ===== TOP: vùng nhập liệu + nút =====
            var top = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            layout.Controls.Add(top, 0, 0);

            // dòng 1: họ tên + CMND + giới tính
            var lblHoTen = new Label { Text = "Họ tên:", AutoSize = true, Left = 0, Top = 10 };
            _txtHoTen = new TextBox { Left = 80, Top = 6, Width = 220 };

            var lblCMND = new Label { Text = "CMND:", AutoSize = true, Left = 320, Top = 10 };
            _txtCMND = new TextBox { Left = 380, Top = 6, Width = 120 };

            var lblGT = new Label { Text = "Giới tính:", AutoSize = true, Left = 530, Top = 10 };
            _cbGioiTinh = new ComboBox
            {
                Left = 600,
                Top = 6,
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cbGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ" });

            top.Controls.AddRange(new Control[] { lblHoTen, _txtHoTen, lblCMND, _txtCMND, lblGT, _cbGioiTinh });

            // dòng 2: địa chỉ + ngày khám
            var lblDiaChi = new Label { Text = "Địa chỉ:", AutoSize = true, Left = 0, Top = 45 };
            _txtDiaChi = new TextBox { Left = 80, Top = 41, Width = 420 };

            var lblNgay = new Label { Text = "Ngày khám:", AutoSize = true, Left = 520, Top = 45 };
            _dtNgayKham = new DateTimePicker
            {
                Left = 610,
                Top = 41,
                Width = 160,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm"
            };

            top.Controls.AddRange(new Control[] { lblDiaChi, _txtDiaChi, lblNgay, _dtNgayKham });

            // dòng 3: bác sĩ phụ trách + Khoa (hiển thị)
            var lblBS = new Label { Text = "Bác sĩ phụ trách:", AutoSize = true, Left = 0, Top = 80 };
            _cbBacSi = new ComboBox
            {
                Left = 120,
                Top = 76,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _radThuong = new RadioButton
            {
                Text = "Thường",
                Left = 80,
                Top = 120,
                Checked = true
            };
            _radNamGiuong = new RadioButton
            {
                Text = "Nằm giường",
                Left = 250,
                Top = 120
            };

            top.Controls.AddRange(new Control[]
            {
                lblBS, _cbBacSi,
                _radThuong, _radNamGiuong
            });

            // dòng 4: nút + tổng + tìm kiếm
            _btnThem = new Button
            {
                Text = "Thêm",
                Left = 80,
                Top = 150,
                Width = 90,
                Height = 40,
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
                Top = 150,
                Width = 90,
                Height = 40,
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
                Top = 150,
                Width = 90,
                Height = 40,
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
                Top = 150,
                Width = 90,
                Height = 40,
                BackColor = Color.FromArgb(120, 144, 156),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnLamMoi.FlatAppearance.BorderSize = 0;
            _btnLamMoi.Click += (_, __) => ClearInputs();

            _lblTong = new Label
            {
                Text = "Tổng: 0 bệnh nhân",
                AutoSize = true,
                Left = 520,
                Top = 162,
                ForeColor = Color.FromArgb(27, 94, 32),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _lblTimKiem = new Label
            {
                Text = "Tìm kiếm:",
                AutoSize = true,
                Left = 720,
                Top = 162
            };
            _txtTimKiem = new TextBox
            {
                Left = 800,
                Top = 158,
                Width = 220
            };
            _txtTimKiem.TextChanged += (_, __) => ApplyFilter();

            top.Controls.AddRange(new Control[]
            {
                _btnThem, _btnSua, _btnXoa, _btnLamMoi,
                _lblTong, _lblTimKiem, _txtTimKiem
            });

            // ===== GRID BÊN DƯỚI: BỆNH NHÂN + LỊCH SỬ KHÁM =====
            var bottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 16) };
            layout.Controls.Add(bottom, 0, 1);

            var tblBottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            tblBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // grid BN
            tblBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // lịch sử khám
            bottom.Controls.Add(tblBottom);

            // --- Hàng 0: danh sách bệnh nhân ---
            var panelBN = new Panel { Dock = DockStyle.Fill };
            tblBottom.Controls.Add(panelBN, 0, 0);

            var lblGridBN = new Label
            {
                Text = "Danh sách bệnh nhân",
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            panelBN.Controls.Add(lblGridBN);

            _grid = new DataGridView
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
            _grid.CellClick += Grid_CellClick;
            panelBN.Controls.Add(_grid);

            // --- Hàng 1: lịch sử khám của bệnh nhân ---
            var panelLS = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            tblBottom.Controls.Add(panelLS, 0, 1);

            var lblLichSu = new Label
            {
                Text = "Lịch sử khám của bệnh nhân (phiếu khám đã lưu)",
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            panelLS.Controls.Add(lblLichSu);

            _gridLichSu = new DataGridView
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
            panelLS.Controls.Add(_gridLichSu);
        }

        // ================== DATA LOAD ==================
        private void LoadDanhSachBacSi(ComboBox combo)
        {
            // BẢN DB của bạn không có MaKhoa/TenKhoa nên chỉ cần lấy sttBS, tenBS
            const string sql = @"SELECT sttBS, tenBS FROM BacSi ORDER BY tenBS;";

            var dt = Database.Query(sql, CommandType.Text) ?? new DataTable();
            _bacSiSource = dt;           // không có cột TenKhoa cũng không sao, hàm UpdateKhoaLabel sẽ fallback "(chưa rõ)"

            combo.DataSource = dt;
            combo.DisplayMember = "tenBS";
            combo.ValueMember = "sttBS";
            combo.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
        }

        private void LoadBenhNhan()
        {
            try
            {
                var dt = Database.Query("sp_SelectAllBenhNhan", CommandType.StoredProcedure)
                         ?? new DataTable();

                // thêm cột Loại Bệnh Nhân
                AddLoaiBenhNhanColumn(dt);

                _benhNhanSource = dt;
                _grid.DataSource = _benhNhanSource;

                var colLoai = _grid.Columns["Loại Bệnh Nhân"];
                if (colLoai != null)
                {
                    colLoai.HeaderText = "Loại Bệnh Nhân";
                    colLoai.DisplayIndex = 2;
                }

                if (_txtTimKiem != null) _txtTimKiem.Text = string.Empty;

                ApplyFilter();

                // clear lịch sử khám vì chưa chọn bệnh nhân
                _lichSuSource = null;
                _gridLichSu.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bệnh nhân:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Thêm cột "Loại Bệnh Nhân" vào DataTable dựa trên việc
        /// sttBN xuất hiện trong BenhNhanThuong hay BenhNhanNamGiuong_ChuaXepCho.
        /// </summary>
        private void AddLoaiBenhNhanColumn(DataTable dt)
        {
            if (dt == null) return;

            if (!dt.Columns.Contains("Loại Bệnh Nhân"))
                dt.Columns.Add("Loại Bệnh Nhân", typeof(string));

            DataTable? bnThuong = null;
            DataTable? bnNam = null;

            try
            {
                bnThuong = Database.Query("SELECT sttBN FROM BenhNhanThuong", CommandType.Text);
                bnNam = Database.Query("SELECT sttBN FROM BenhNhanNamGiuong_ChuaXepCho", CommandType.Text);
            }
            catch
            {
                return;
            }

            var thuongSet = new System.Collections.Generic.HashSet<int>();
            var namSet = new System.Collections.Generic.HashSet<int>();

            if (bnThuong != null)
            {
                foreach (DataRow r in bnThuong.Rows)
                    if (int.TryParse(r["sttBN"]?.ToString(), out int id))
                        thuongSet.Add(id);
            }

            if (bnNam != null)
            {
                foreach (DataRow r in bnNam.Rows)
                    if (int.TryParse(r["sttBN"]?.ToString(), out int id))
                        namSet.Add(id);
            }

            foreach (DataRow row in dt.Rows)
            {
                if (!int.TryParse(row["Mã Bệnh Nhân"]?.ToString(), out int id))
                    continue;

                string loai = "";
                if (thuongSet.Contains(id)) loai = "Thường";
                else if (namSet.Contains(id)) loai = "Nằm giường";

                row["Loại Bệnh Nhân"] = loai;
            }
        }

        // ================== LỊCH SỬ KHÁM ==================
        private void LoadLichSuKham(int sttBN)
        {
            try
            {
                const string sql = @"
SELECT ls.sttLSK       AS [Mã LS],
       ls.NgayKham    AS [Ngày khám],
       bs.tenBS       AS [Bác sĩ],
       ls.TrieuChung  AS [Triệu chứng],
       ls.ChanDoan    AS [Chẩn đoán],
       ls.GhiChu      AS [Ghi chú]
FROM   LichSukham ls
       JOIN BacSi bs ON ls.sttBS = bs.sttBS
WHERE  ls.sttBN = @sttBN
ORDER BY ls.NgayKham DESC;";

                _lichSuSource = Database.Query(sql, CommandType.Text,
                    new SqlParameter("@sttBN", sttBN)) ?? new DataTable();

                _gridLichSu.DataSource = _lichSuSource;

  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch sử khám:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== EVENT GRID ==================
        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.CurrentRow == null) return;

            var row = _grid.CurrentRow;

            try
            {
                _currentId = Convert.ToInt32(row.Cells["Mã Bệnh Nhân"].Value);
                _txtHoTen.Text = Convert.ToString(row.Cells["Họ Tên Bệnh Nhân"].Value);
                _txtCMND.Text = Convert.ToString(row.Cells["CMND"].Value);
                _txtDiaChi.Text = Convert.ToString(row.Cells["Địa Chỉ"].Value);
                _dtNgayKham.Value = Convert.ToDateTime(row.Cells["Ngày Khám Bệnh"].Value);
                _cbGioiTinh.SelectedItem = Convert.ToString(row.Cells["Giới Tính"].Value);

                var bacSiIdObj = row.Cells["Bác Sĩ Điều Trị"].Value;
                if (bacSiIdObj != null && int.TryParse(bacSiIdObj.ToString(), out int bsId))
                {
                    _cbBacSi.SelectedValue = bsId;
                }

                var loaiObj = row.Cells["Loại Bệnh Nhân"]?.Value?.ToString();
                if (loaiObj == "Thường")
                    _radThuong.Checked = true;
                else if (loaiObj == "Nằm giường")
                    _radNamGiuong.Checked = true;

                // load lịch sử khám cho BN đang chọn
                if (_currentId != null)
                    LoadLichSuKham(_currentId.Value);
            }
            catch
            {
                // ignore mapping error
            }
        }

        // ================== BUTTON HANDLERS ==================
        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(out int cmnd, out int bacSiId)) return;

            try
            {
                var pars = new[]
                {
                    new SqlParameter("@hoten", _txtHoTen.Text.Trim()),
                    new SqlParameter("@CMND", cmnd),
                    new SqlParameter("@diachi", _txtDiaChi.Text.Trim()),
                    new SqlParameter("@gioitinh", _cbGioiTinh.SelectedItem?.ToString() ?? "Nam"),
                    new SqlParameter("@ngaykham", _dtNgayKham.Value),
                    new SqlParameter("@BSphutrach", bacSiId)
                };

                if (_radThuong.Checked)
                    Database.Execute("sp_InsertBenhNhan_BenhNhanThuong", CommandType.StoredProcedure, pars);
                else
                    Database.Execute("sp_InsertBenhNhan_BenhNhanNamGiuong_ChuaXepCho", CommandType.StoredProcedure, pars);

                LoadBenhNhan();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm bệnh nhân:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một bệnh nhân trong danh sách để sửa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out int cmnd, out int bacSiId)) return;

            try
            {
                var pars = new[]
                {
                    new SqlParameter("@sttBN", _currentId.Value),
                    new SqlParameter("@hoten", _txtHoTen.Text.Trim()),
                    new SqlParameter("@CMND", cmnd),
                    new SqlParameter("@diachi", _txtDiaChi.Text.Trim()),
                    new SqlParameter("@gioitinh", _cbGioiTinh.SelectedItem?.ToString() ?? "Nam"),
                    new SqlParameter("@ngaykham", _dtNgayKham.Value),
                    new SqlParameter("@BSphutrach", bacSiId)
                };

                Database.Execute("sp_UpdateThongTinBenhNhan", CommandType.StoredProcedure, pars);
                LoadBenhNhan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật bệnh nhân:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_currentId == null)
            {
                MessageBox.Show("Hãy chọn một bệnh nhân trong danh sách để xóa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dr = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa bệnh nhân này?\n" +
                "Lưu ý: sẽ xóa luôn lịch sử khám, lịch hẹn, giường, đơn thuốc, hóa đơn liên quan.",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr != DialogResult.Yes) return;

            try
            {
                const string sql = @"
-- 1. Lịch sử khám (phiếu khám)
DELETE FROM LichSukham WHERE sttBN = @id;

-- 2. Lịch hẹn còn liên quan
DELETE FROM LichHen WHERE sttBN = @id;

-- 3. Đơn thuốc + hóa đơn liên quan bệnh nhân nằm giường
DELETE FROM DonThuoc_BenhNhanNamGiuong_DaXepCho WHERE MaBenhNhanNamGiuong_DaXepCho IN
    (SELECT sttBNNG_DXC FROM BenhNhanNamGiuong_DaXepCho WHERE sttBN = @id);
DELETE FROM DonThuoc_BenhNhanThuong WHERE MaBenhNhanThuong IN
    (SELECT sttBNT FROM BenhNhanThuong WHERE sttBN = @id);
DELETE FROM HoaDon WHERE sttBNNG_DXC IN
    (SELECT sttBNNG_DXC FROM BenhNhanNamGiuong_DaXepCho WHERE sttBN = @id);

-- 4. Bản ghi nằm giường / thường
DELETE FROM BenhNhanNamGiuong_DaXepCho      WHERE sttBN = @id;
DELETE FROM BenhNhanNamGiuong_ChuaXepCho    WHERE sttBN = @id;
DELETE FROM BenhNhanThuong                  WHERE sttBN = @id;

-- 5. Cuối cùng: xóa bệnh nhân
DELETE FROM BenhNhan                        WHERE sttBN = @id;";

                Database.Execute(sql, CommandType.Text, new SqlParameter("@id", _currentId.Value));

                LoadBenhNhan();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa bệnh nhân.\n" +
                                "Có thể đang tồn tại dữ liệu liên quan chưa xóa hết.\n\nChi tiết: "
                                + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== HELPER ==================
        private void ClearInputs()
        {
            _currentId = null;
            _txtHoTen.Clear();
            _txtCMND.Clear();
            _txtDiaChi.Clear();
            _cbGioiTinh.SelectedIndex = -1;

            if (_cbBacSi.Items.Count > 0)
                _cbBacSi.SelectedIndex = 0;

            _dtNgayKham.Value = DateTime.Now;
            _radThuong.Checked = true;
            _grid.ClearSelection();

            _lichSuSource = null;
            _gridLichSu.DataSource = null;
        }

        private bool ValidateInput(out int cmnd, out int bacSiId)
        {
            cmnd = 0;
            bacSiId = 0;

            if (string.IsNullOrWhiteSpace(_txtHoTen.Text))
            {
                MessageBox.Show("Họ tên không được để trống.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(_txtCMND.Text.Trim(), out cmnd))
            {
                MessageBox.Show("CMND phải là số nguyên.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cbBacSi.SelectedValue == null ||
                !int.TryParse(_cbBacSi.SelectedValue.ToString(), out bacSiId))
            {
                MessageBox.Show("Hãy chọn bác sĩ phụ trách.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // --- ÁP DỤNG FILTER TÌM KIẾM ---
        private void ApplyFilter()
        {
            if (_benhNhanSource == null)
            {
                _lblTong.Text = "Tổng: 0 bệnh nhân";
                return;
            }

            var view = _benhNhanSource.DefaultView;
            string keyword = _txtTimKiem?.Text.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(keyword))
            {
                view.RowFilter = string.Empty;
            }
            else
            {
                var safe = keyword.Replace("'", "''");
                view.RowFilter =
                    $"Convert([Mã Bệnh Nhân], 'System.String') LIKE '%{safe}%' OR " +
                    $"[Họ Tên Bệnh Nhân] LIKE '%{safe}%' OR " +
                    $"Convert([CMND], 'System.String') LIKE '%{safe}%' OR " +
                    $"[Địa Chỉ] LIKE '%{safe}%' OR " +
                    $"[Loại Bệnh Nhân] LIKE '%{safe}%'";
            }

            _lblTong.Text = $"Tổng: {view.Count} bệnh nhân";
        }
    }
}
