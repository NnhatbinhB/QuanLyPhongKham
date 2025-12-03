using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmHoaDon : Form
    {
        private enum PatientMode
        {
            NamGiuong,
            BenhNhanThuong
        }

        private PatientMode _mode = PatientMode.NamGiuong;

        // ==== BÊN TRÁI ====
        private DataGridView _gridBenhNhan;
        private ComboBox _cbLoaiBN;

        // ==== THÔNG TIN BỆNH NHÂN ====
        private Label _lblTenBNTitle, _lblMaBNTitle, _lblGiuongTitle, _lblNgayInfoTitle;
        private Label _lblTenBN, _lblMaBN, _lblGiuong, _lblNgayInfo;

        // Ngày ra viện / thanh toán
        private Label _lblNgayRaTitle;
        private DateTimePicker _dtNgayRa;

        // Tiền
        private Label _lblTienGiuongTitle, _lblTienThuocTitle, _lblThanhTienTitle;
        private Label _lblTienGiuong, _lblTienThuoc, _lblThanhTien;

        // Tiền dịch vụ (tính tự động từ danh sách dịch vụ)
        private NumericUpDown _numTienDV;

        // Khu vực chọn dịch vụ
        private ComboBox _cbDichVu;
        private DataGridView _gridDichVu;
        private Button _btnThemDV;
        private Button _btnXoaDV;

        // Dữ liệu dịch vụ
        private DataTable? _dvAll;
        private DataTable? _dvSelected;

        // ==== NÚT LẬP HÓA ĐƠN + GỬI EMAIL ====
        private Button _btnLap;
        private Button _btnGuiEmail;

        // ==== PHƯƠNG THỨC THANH TOÁN ====
        private ComboBox _cbPhuongThucTT;

        // ==== TRẠNG THÁI HIỆN TẠI ====
        // BN nằm giường
        private int? _currentBNNG_DXC; // khóa chính BenhNhanNamGiuong_DaXepCho
        private DateTime _ngayXep;
        private int _tien1Ngay1Giuong;

        // BN thường
        private int? _currentBNT; // khóa chính BenhNhanThuong
        private DateTime _ngayKhamThuong;

        private bool _isSwitchingMode = false;

        public FrmHoaDon()
        {
            Text = "Lập hóa đơn";
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F);

            BuildLayout();
            LoadDichVu();   // load danh sách dịch vụ cho combobox

            _tien1Ngay1Giuong = HoaDonRepository.GetTien1Ngay1Giuong();

            // Sau khi layout KHỞI TẠO XONG thì mới set SelectedIndex
            if (_cbLoaiBN != null)
                _cbLoaiBN.SelectedIndex = 0; // sẽ kích hoạt SwitchMode(NamGiuong)
        }

        // =========================================================
        // = BUILD LAYOUT
        // =========================================================
        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Controls.Add(layout);

            // ==== BÊN TRÁI: DS BỆNH NHÂN ====
            _gridBenhNhan = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            _gridBenhNhan.CellClick += GridBenhNhan_CellClick;
            layout.Controls.Add(_gridBenhNhan, 0, 0);

            // ==== BÊN PHẢI: THÔNG TIN HÓA ĐƠN ====
            var right = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White
            };
            layout.Controls.Add(right, 1, 0);

            int y = 10;

            var lblTitle = new Label
            {
                Text = "Thông tin hóa đơn",
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, y),
                ForeColor = Color.FromArgb(27, 94, 32)
            };
            right.Controls.Add(lblTitle);
            y += 36;

            // Loại bệnh nhân
            var lblLoaiTitle = new Label
            {
                Text = "Loại bệnh nhân:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            right.Controls.Add(lblLoaiTitle);

            _cbLoaiBN = new ComboBox
            {
                Location = new Point(140, y),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cbLoaiBN.Items.Add("Bệnh nhân nằm giường");
            _cbLoaiBN.Items.Add("Bệnh nhân thường");
            _cbLoaiBN.SelectedIndexChanged += (s, e) =>
            {
                if (_cbLoaiBN.SelectedIndex == 1)
                    SwitchMode(PatientMode.BenhNhanThuong);
                else
                    SwitchMode(PatientMode.NamGiuong);
            };
            right.Controls.Add(_cbLoaiBN);

            y += 32;

            // Họ tên, mã, giường, ngày xếp / khám
            _lblTenBN = NewLineLabel(right, "Họ tên bệnh nhân:", ref y, out _lblTenBNTitle);
            _lblMaBN = NewLineLabel(right, "Mã bệnh nhân:", ref y, out _lblMaBNTitle);
            _lblGiuong = NewLineLabel(right, "Giường:", ref y, out _lblGiuongTitle);
            _lblNgayInfo = NewLineLabel(right, "Ngày xếp:", ref y, out _lblNgayInfoTitle);

            // Ngày ra viện / thanh toán
            _lblNgayRaTitle = new Label
            {
                Text = "Ngày ra viện:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            right.Controls.Add(_lblNgayRaTitle);

            _dtNgayRa = new DateTimePicker
            {
                Location = new Point(140, y),
                Width = 200,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy"
            };
            _dtNgayRa.ValueChanged += (s, e) => UpdateTien();
            right.Controls.Add(_dtNgayRa);

            y += 32;
            y += 10;

            // Tiền giường + tiền thuốc
            _lblTienGiuong = NewLineLabel(right, "Tiền giường:", ref y, out _lblTienGiuongTitle);
            _lblTienThuoc = NewLineLabel(right, "Tiền thuốc:", ref y, out _lblTienThuocTitle);

            // ===== CHỌN DỊCH VỤ TỪ BẢNG DICHVU =====
            var lblChonDV = new Label
            {
                Text = "Thêm dịch vụ:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            right.Controls.Add(lblChonDV);

            _cbDichVu = new ComboBox
            {
                Location = new Point(140, y),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            right.Controls.Add(_cbDichVu);

            _btnThemDV = new Button
            {
                Text = "+",
                Width = 30,
                Height = 26,
                Location = new Point(350, y),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnThemDV.FlatAppearance.BorderSize = 0;
            _btnThemDV.Click += BtnThemDV_Click;
            right.Controls.Add(_btnThemDV);

            _btnXoaDV = new Button
            {
                Text = "Xóa",
                Width = 50,
                Height = 26,
                Location = new Point(385, y),
                BackColor = Color.FromArgb(211, 47, 47),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnXoaDV.FlatAppearance.BorderSize = 0;
            _btnXoaDV.Click += BtnXoaDV_Click;
            right.Controls.Add(_btnXoaDV);

            y += 32;

            _gridDichVu = new DataGridView
            {
                Location = new Point(0, y),
                Width = right.Width - 40,
                Height = 100,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            right.Controls.Add(_gridDichVu);

            y += 110;

            // Tiền dịch vụ (hiển thị, không cho nhập tay)
            var lblDVTitle = new Label
            {
                Text = "Tiền dịch vụ:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            right.Controls.Add(lblDVTitle);

            _numTienDV = new NumericUpDown
            {
                Location = new Point(140, y),
                Width = 200,
                Maximum = 1000000000,
                Minimum = 0,
                Increment = 50000,
                ThousandsSeparator = true,
                Enabled = false   // chỉ hiển thị, không cho nhập
            };
            right.Controls.Add(_numTienDV);

            y += 26;

            // ===== PHƯƠNG THỨC THANH TOÁN =====
            var lblPTTT = new Label
            {
                Text = "Phương thức thanh toán:",
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            right.Controls.Add(lblPTTT);

            _cbPhuongThucTT = new ComboBox
            {
                Location = new Point(180, y),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cbPhuongThucTT.Items.AddRange(new object[]
            {
                "Tiền mặt",
                "Chuyển khoản ngân hàng",
                "Quét mã QR",
                "Ví điện tử (Momo/ZaloPay)",
                "Thẻ (Visa/Mastercard)"
            });
            if (_cbPhuongThucTT.Items.Count > 0)
                _cbPhuongThucTT.SelectedIndex = 0;
            right.Controls.Add(_cbPhuongThucTT);

            y += 32;

            // Thành tiền (gồm giường + thuốc + dịch vụ)
            _lblThanhTien = NewLineLabel(right, "Thành tiền:", ref y, out _lblThanhTienTitle, bold: true, bigger: true);

            y += 10;

            _btnLap = new Button
            {
                Text = "Lập hóa đơn",
                Width = 150,
                Height = 36,
                Location = new Point(0, y),
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnLap.FlatAppearance.BorderSize = 0;
            _btnLap.Click += BtnLap_Click;
            right.Controls.Add(_btnLap);

            // Nút gửi email hóa đơn
            _btnGuiEmail = new Button
            {
                Text = "Gửi email hóa đơn",
                Width = 170,
                Height = 36,
                Location = new Point(170, y),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnGuiEmail.FlatAppearance.BorderSize = 0;
            _btnGuiEmail.Click += BtnGuiEmail_Click;
            right.Controls.Add(_btnGuiEmail);

            // Khi panel resize thì grid dịch vụ cũng rộng theo
            right.Resize += (s, e) =>
            {
                _gridDichVu.Width = right.Width - 40;
            };
        }

        private Label NewLineLabel(Panel parent, string caption, ref int y,
            out Label captionLabel,
            bool bold = false, bool bigger = false)
        {
            captionLabel = new Label
            {
                Text = caption,
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            parent.Controls.Add(captionLabel);

            var fontSize = bigger ? 11F : 10F;
            var style = bold ? FontStyle.Bold : FontStyle.Regular;

            var lbl = new Label
            {
                Text = "...",
                AutoSize = true,
                Location = new Point(140, y + 4),
                Font = new Font("Segoe UI", fontSize, style),
                ForeColor = bold ? Color.FromArgb(211, 47, 47) : Color.Black
            };
            parent.Controls.Add(lbl);

            y += 26;
            return lbl;
        }

        // =========================================================
        // = LOAD DATA THEO LOẠI BỆNH NHÂN
        // =========================================================
        private void SwitchMode(PatientMode mode)
        {
            _isSwitchingMode = true;
            _mode = mode;

            // Reset trạng thái chung
            _currentBNNG_DXC = null;
            _currentBNT = null;
            _lblTenBN.Text = _lblMaBN.Text = _lblGiuong.Text = _lblNgayInfo.Text = "...";
            _lblTienGiuong.Text = "0 đ";
            _lblTienThuoc.Text = "0 đ";
            _lblThanhTien.Text = "0 đ";
            _numTienDV.Value = 0;
            _dvSelected?.Rows.Clear();

            if (_mode == PatientMode.NamGiuong)
            {
                _lblNgayInfoTitle.Text = "Ngày xếp:";
                _lblNgayRaTitle.Text = "Ngày ra viện:";
                _lblGiuongTitle.Text = "Giường:";
                LoadBenhNhanNamGiuong();
            }
            else
            {
                _lblNgayInfoTitle.Text = "Ngày khám:";
                _lblNgayRaTitle.Text = "Ngày thanh toán:";
                _lblGiuongTitle.Text = "Giường (BN thường):";
                LoadBenhNhanThuong();
            }

            _isSwitchingMode = false;
        }

        // BN nằm giường: DS có đơn thuốc chưa thanh toán
        private void LoadBenhNhanNamGiuong()
        {
            try
            {
                var dt = HoaDonRepository.GetBenhNhanNamGiuong_CoThuocChuaThanhToan();
                _gridBenhNhan.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bệnh nhân nằm giường:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // BN thường
        private void LoadBenhNhanThuong()
        {
            try
            {
                const string sql = @"
SELECT bnt.sttBNT,
       bn.sttBN      AS [Mã Bệnh Nhân],
       bn.hoten      AS [Họ Tên Bệnh Nhân],
       bn.CMND,
       bn.diachi     AS [Địa Chỉ],
       bn.ngaykham   AS [Ngày Khám]
FROM BenhNhanThuong bnt
JOIN BenhNhan bn ON bn.sttBN = bnt.sttBN
ORDER BY bn.ngaykham DESC;";

                var dt = Database.Query(sql, CommandType.Text) ?? new DataTable();
                _gridBenhNhan.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bệnh nhân thường:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // = LOAD DỊCH VỤ
        // =========================================================
        private void LoadDichVu()
        {
            try
            {
                const string sql = @"SELECT MaDV, TenDV, LoaiDV, DonGia
                                     FROM DichVu
                                     ORDER BY TenDV;";

                _dvAll = Database.Query(sql, CommandType.Text) ?? new DataTable();

                _cbDichVu.DataSource = _dvAll;
                _cbDichVu.DisplayMember = "TenDV";
                _cbDichVu.ValueMember = "MaDV";

                _dvSelected = new DataTable();
                _dvSelected.Columns.Add("MaDV", typeof(int));
                _dvSelected.Columns.Add("TenDV", typeof(string));
                _dvSelected.Columns.Add("DonGia", typeof(int));

                _gridDichVu.DataSource = _dvSelected;

                try
                {
                    if (_gridDichVu.Columns["MaDV"] != null)
                        _gridDichVu.Columns["MaDV"].HeaderText = "Mã DV";

                    if (_gridDichVu.Columns["TenDV"] != null)
                        _gridDichVu.Columns["TenDV"].HeaderText = "Tên dịch vụ";

                    if (_gridDichVu.Columns["DonGia"] != null)
                        _gridDichVu.Columns["DonGia"].HeaderText = "Đơn giá";
                }
                catch
                {
                    // ignore
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách dịch vụ:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // = CHỌN BỆNH NHÂN TRONG GRID
        // =========================================================
        private void GridBenhNhan_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _gridBenhNhan.CurrentRow == null) return;
            if (_isSwitchingMode) return;

            try
            {
                var row = _gridBenhNhan.CurrentRow;

                if (_mode == PatientMode.NamGiuong)
                {
                    _currentBNNG_DXC = Convert.ToInt32(row.Cells["sttBNNG_DXC"].Value);
                    _currentBNT = null;

                    _lblMaBN.Text = row.Cells["Mã Bệnh Nhân"].Value?.ToString() ?? "";
                    _lblTenBN.Text = row.Cells["Họ Tên Bệnh Nhân"].Value?.ToString() ?? "";
                    _lblGiuong.Text = row.Cells["Giường"].Value?.ToString() ?? "";

                    _ngayXep = Convert.ToDateTime(row.Cells["Ngày Xếp"].Value);
                    _lblNgayInfo.Text = _ngayXep.ToString("dd/MM/yyyy");

                    if (_dtNgayRa.Value < _ngayXep)
                        _dtNgayRa.Value = _ngayXep;
                }
                else // Bệnh nhân thường
                {
                    _currentBNNG_DXC = null;
                    _currentBNT = Convert.ToInt32(row.Cells["sttBNT"].Value);

                    _lblMaBN.Text = row.Cells["Mã Bệnh Nhân"].Value?.ToString() ?? "";
                    _lblTenBN.Text = row.Cells["Họ Tên Bệnh Nhân"].Value?.ToString() ?? "";
                    _lblGiuong.Text = "-";

                    _ngayKhamThuong = Convert.ToDateTime(row.Cells["Ngày Khám"].Value);
                    _lblNgayInfo.Text = _ngayKhamThuong.ToString("dd/MM/yyyy");

                    if (_dtNgayRa.Value < _ngayKhamThuong)
                        _dtNgayRa.Value = _ngayKhamThuong;
                }

                // reset dịch vụ khi chọn bệnh nhân mới
                _dvSelected?.Rows.Clear();

                UpdateTien();
            }
            catch
            {
                // ignore
            }
        }

        // =========================================================
        // = TÍNH TIỀN
        // =========================================================
        private int TinhTongTienDichVu()
        {
            if (_dvSelected == null) return 0;

            int sum = 0;
            foreach (DataRow row in _dvSelected.Rows)
            {
                try
                {
                    sum += Convert.ToInt32(row["DonGia"]);
                }
                catch
                {
                    // bỏ qua nếu lỗi convert
                }
            }

            return sum;
        }

        private int GetTongTienThuocBenhNhanThuong(int sttBNT)
        {
            const string sql = @"
SELECT ISNULL(SUM(dt.TienThuoc), 0) AS TongTien
FROM   DonThuoc_BenhNhanThuong dt_bnt
       INNER JOIN DonThuoc dt ON dt_bnt.sttDT = dt.sttDT
WHERE  dt_bnt.MaBenhNhanThuong = @id;";

            var dt = Database.Query(sql, CommandType.Text,
                new SqlParameter("@id", sttBNT)) ?? new DataTable();

            if (dt.Rows.Count == 0) return 0;

            return Convert.ToInt32(dt.Rows[0]["TongTien"]);
        }

        private void UpdateTien()
        {
            if (_mode == PatientMode.NamGiuong)
            {
                if (_currentBNNG_DXC == null)
                {
                    _lblTienGiuong.Text = "0 đ";
                    _lblTienThuoc.Text = "0 đ";
                    _lblThanhTien.Text = "0 đ";
                    _numTienDV.Value = 0;
                    _dvSelected?.Rows.Clear();
                    return;
                }

                try
                {
                    int tienThuoc = HoaDonRepository.GetTongTienThuocChuaThanhToan(_currentBNNG_DXC.Value);
                    _lblTienThuoc.Text = tienThuoc.ToString("N0") + " đ";

                    DateTime ngayRa = _dtNgayRa.Value.Date;
                    if (ngayRa < _ngayXep.Date) ngayRa = _ngayXep.Date;

                    int soNgay = (ngayRa - _ngayXep.Date).Days + 1;
                    if (soNgay < 1) soNgay = 1;

                    int tienGiuong = soNgay * _tien1Ngay1Giuong;
                    _lblTienGiuong.Text =
                        $"{soNgay} ngày x {_tien1Ngay1Giuong:N0} đ = {tienGiuong:N0} đ";

                    int tienDV = TinhTongTienDichVu();
                    decimal dvValue = tienDV;
                    if (dvValue < _numTienDV.Minimum) dvValue = _numTienDV.Minimum;
                    if (dvValue > _numTienDV.Maximum) dvValue = _numTienDV.Maximum;
                    _numTienDV.Value = dvValue;

                    int thanhTien = tienThuoc + tienGiuong + tienDV;
                    _lblThanhTien.Text = thanhTien.ToString("N0") + " đ";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tính tiền hóa đơn (BN nằm giường):\n" + ex.Message,
                        "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else // Bệnh nhân thường
            {
                if (_currentBNT == null)
                {
                    _lblTienGiuong.Text = "0 đ";
                    _lblTienThuoc.Text = "0 đ";
                    _lblThanhTien.Text = "0 đ";
                    _numTienDV.Value = 0;
                    _dvSelected?.Rows.Clear();
                    return;
                }

                try
                {
                    int tienThuoc = GetTongTienThuocBenhNhanThuong(_currentBNT.Value);
                    _lblTienThuoc.Text = tienThuoc.ToString("N0") + " đ";

                    _lblTienGiuong.Text = "0 đ";

                    int tienDV = TinhTongTienDichVu();
                    decimal dvValue = tienDV;
                    if (dvValue < _numTienDV.Minimum) dvValue = _numTienDV.Minimum;
                    if (dvValue > _numTienDV.Maximum) dvValue = _numTienDV.Maximum;
                    _numTienDV.Value = dvValue;

                    int thanhTien = tienThuoc + tienDV;
                    _lblThanhTien.Text = thanhTien.ToString("N0") + " đ";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tính tiền hóa đơn (BN thường):\n" + ex.Message,
                        "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =========================================================
        // = LẬP HÓA ĐƠN
        // =========================================================
        private void BtnLap_Click(object? sender, EventArgs e)
        {
            if (_mode == PatientMode.NamGiuong)
            {
                LapHoaDon_BenhNhanNamGiuong();
            }
            else
            {
                LapHoaDon_BenhNhanThuong();
            }
        }

        private string GetSelectedPaymentMethod()
        {
            return _cbPhuongThucTT?.SelectedItem?.ToString() ?? "Tiền mặt";
        }

        private void LapHoaDon_BenhNhanNamGiuong()
        {
            if (_currentBNNG_DXC == null)
            {
                MessageBox.Show("Hãy chọn một bệnh nhân nằm giường để lập hóa đơn.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int tienThuoc = HoaDonRepository.GetTongTienThuocChuaThanhToan(_currentBNNG_DXC.Value);
            DateTime ngayRa = _dtNgayRa.Value.Date;
            if (ngayRa < _ngayXep.Date) ngayRa = _ngayXep.Date;

            int soNgay = (ngayRa - _ngayXep.Date).Days + 1;
            if (soNgay < 1) soNgay = 1;

            int tienGiuong = soNgay * _tien1Ngay1Giuong;
            int tienDV = TinhTongTienDichVu();
            int thanhTien = tienThuoc + tienGiuong + tienDV;
            string pttt = GetSelectedPaymentMethod();

            var confirm = MessageBox.Show(
                $"(Bệnh nhân nằm giường)\n\n" +
                $"Tiền giường: {tienGiuong:N0} đ\n" +
                $"Tiền thuốc: {tienThuoc:N0} đ\n" +
                $"Tiền dịch vụ: {tienDV:N0} đ\n" +
                $"Thành tiền: {thanhTien:N0} đ\n" +
                $"Phương thức thanh toán: {pttt}\n\n" +
                "Bạn có chắc chắn muốn lập hóa đơn cho bệnh nhân này không?",
                "Xác nhận lập hóa đơn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Database.Execute(
                    "sp_ThanhToan_BenhNhanNamGiuong",
                    CommandType.StoredProcedure,
                    new SqlParameter("@sttBNNG_DXC", _currentBNNG_DXC.Value),
                    new SqlParameter("@NgayRaVien", ngayRa),
                    new SqlParameter("@TienDV", tienDV)
                // Nếu sau này bạn thêm @PhuongThucThanhToan vào SP
                // hãy bổ sung: new SqlParameter("@PhuongThucThanhToan", pttt)
                );

                MessageBox.Show(
                    "Đã lập hóa đơn, đánh dấu đơn thuốc là 'Đã thanh toán', trả giường về 'Còn trống' và đưa bệnh nhân ra khỏi danh sách nằm giường.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadBenhNhanNamGiuong();
                _currentBNNG_DXC = null;
                _lblThanhTien.Text = "0 đ";
                _lblTienThuoc.Text = "0 đ";
                _lblTienGiuong.Text = "0 đ";
                _numTienDV.Value = 0;
                _lblTenBN.Text = _lblMaBN.Text = _lblGiuong.Text = _lblNgayInfo.Text = "...";
                _dvSelected?.Rows.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu hóa đơn / cập nhật dữ liệu (BN nằm giường):\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LapHoaDon_BenhNhanThuong()
        {
            if (_currentBNT == null)
            {
                MessageBox.Show("Hãy chọn một bệnh nhân thường để lập hóa đơn.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int tienThuoc = GetTongTienThuocBenhNhanThuong(_currentBNT.Value);
            int tienDV = TinhTongTienDichVu();
            int thanhTien = tienThuoc + tienDV;
            string pttt = GetSelectedPaymentMethod();

            var confirm = MessageBox.Show(
                $"(Bệnh nhân thường)\n\n" +
                $"Tiền thuốc: {tienThuoc:N0} đ\n" +
                $"Tiền dịch vụ: {tienDV:N0} đ\n" +
                $"Thành tiền: {thanhTien:N0} đ\n" +
                $"Phương thức thanh toán: {pttt}\n\n" +
                "Bạn có chắc chắn muốn lập hóa đơn cho bệnh nhân này không?\n" +
                "(Sau khi thanh toán, bệnh nhân sẽ được xoá khỏi danh sách BenhNhanThuong và các đơn thuốc link.)",
                "Xác nhận lập hóa đơn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Database.Execute(
                    "sp_ThanhToan_BenhNhanThuong",
                    CommandType.StoredProcedure,
                    new SqlParameter("@sttBNT", _currentBNT.Value)
                // Nếu sau này SP có @PhuongThucThanhToan thì thêm param ở đây
                );

                MessageBox.Show(
                    "Đã thanh toán cho bệnh nhân thường và đưa ra khỏi danh sách BenhNhanThuong.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadBenhNhanThuong();
                _currentBNT = null;
                _lblThanhTien.Text = "0 đ";
                _lblTienThuoc.Text = "0 đ";
                _lblTienGiuong.Text = "0 đ";
                _numTienDV.Value = 0;
                _lblTenBN.Text = _lblMaBN.Text = _lblGiuong.Text = _lblNgayInfo.Text = "...";
                _dvSelected?.Rows.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu hóa đơn / cập nhật dữ liệu (BN thường):\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // = GỬI EMAIL HÓA ĐƠN
        // =========================================================
        private void BtnGuiEmail_Click(object? sender, EventArgs e)
        {
            if (_currentBNNG_DXC == null && _currentBNT == null)
            {
                MessageBox.Show("Hãy chọn bệnh nhân cần gửi email hóa đơn.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Lấy mã bệnh nhân từ grid để truy email
            if (_gridBenhNhan.CurrentRow == null)
            {
                MessageBox.Show("Không tìm thấy dòng bệnh nhân đang chọn.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string? maBnStr = _gridBenhNhan.CurrentRow.Cells["Mã Bệnh Nhân"].Value?.ToString();
            if (!int.TryParse(maBnStr, out int sttBN))
            {
                MessageBox.Show("Không lấy được mã bệnh nhân để gửi email.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string email = GetEmailBenhNhan(sttBN);
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Bệnh nhân chưa có email trong hệ thống.\nHãy cập nhật email cho bệnh nhân trước.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Tính lại tiền giống lúc lập hóa đơn
            int tienThuoc;
            int tienGiuong = 0;
            int tienDV = TinhTongTienDichVu();
            int thanhTien;

            string loaiBNText;
            DateTime ngayThanhToan = _dtNgayRa.Value.Date;

            if (_mode == PatientMode.NamGiuong && _currentBNNG_DXC != null)
            {
                loaiBNText = "Bệnh nhân nằm giường";

                tienThuoc = HoaDonRepository.GetTongTienThuocChuaThanhToan(_currentBNNG_DXC.Value);

                if (ngayThanhToan < _ngayXep.Date)
                    ngayThanhToan = _ngayXep.Date;

                int soNgay = (ngayThanhToan - _ngayXep.Date).Days + 1;
                if (soNgay < 1) soNgay = 1;

                tienGiuong = soNgay * _tien1Ngay1Giuong;
                thanhTien = tienThuoc + tienGiuong + tienDV;
            }
            else if (_mode == PatientMode.BenhNhanThuong && _currentBNT != null)
            {
                loaiBNText = "Bệnh nhân thường";

                tienThuoc = GetTongTienThuocBenhNhanThuong(_currentBNT.Value);
                tienGiuong = 0;
                thanhTien = tienThuoc + tienDV;
            }
            else
            {
                MessageBox.Show("Không xác định được loại bệnh nhân để gửi email hóa đơn.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string pttt = GetSelectedPaymentMethod();
            string tenBN = _lblTenBN.Text;
            string maBN = _lblMaBN.Text;

            string subject = $"Hóa đơn thanh toán - {tenBN} - {ngayThanhToan:dd/MM/yyyy}";
            string body =
                $"Kính gửi {tenBN},\n\n" +
                $"Thông tin hóa đơn thanh toán tại Phòng khám Đa khoa Đức Bình:\n\n" +
                $"- Mã bệnh nhân: {maBN}\n" +
                $"- Loại bệnh nhân: {loaiBNText}\n" +
                $"- Ngày thanh toán: {ngayThanhToan:dd/MM/yyyy}\n" +
                $"- Phương thức thanh toán: {pttt}\n\n" +
                $"Chi tiết thanh toán:\n" +
                $"- Tiền giường: {tienGiuong:N0} đ\n" +
                $"- Tiền thuốc: {tienThuoc:N0} đ\n" +
                $"- Tiền dịch vụ: {tienDV:N0} đ\n" +
                $"=> Thành tiền: {thanhTien:N0} đ\n\n" +
                "Nếu có bất kỳ thắc mắc nào về hóa đơn này, quý khách vui lòng liên hệ lại phòng khám để được hỗ trợ.\n\n" +
                "Trân trọng,\n" +
                "Phòng khám Đa khoa Đức Bình";

            try
            {
                EmailHelper.SendClinicMail(email, subject, body);

                MessageBox.Show(
                    $"Đã gửi email hóa đơn tới địa chỉ: {email}",
                    "Gửi email hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gửi email hóa đơn thất bại:\n" + ex.Message,
                    "Lỗi gửi email", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetEmailBenhNhan(int sttBN)
        {
            try
            {
                const string sql = @"SELECT Email FROM BenhNhan WHERE sttBN = @id;";
                var dt = Database.Query(sql, CommandType.Text,
                    new SqlParameter("@id", sttBN)) ?? new DataTable();

                if (dt.Rows.Count == 0) return string.Empty;

                return dt.Rows[0]["Email"]?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // =========================================================
        // = BUTTON DỊCH VỤ
        // =========================================================
        private void BtnThemDV_Click(object? sender, EventArgs e)
        {
            if (_dvAll == null || _dvSelected == null) return;
            if (_cbDichVu.SelectedItem is not DataRowView dvRow) return;

            int maDV = Convert.ToInt32(dvRow["MaDV"]);
            string tenDV = dvRow["TenDV"]?.ToString() ?? "";
            int donGia = 0;
            try
            {
                donGia = Convert.ToInt32(dvRow["DonGia"]);
            }
            catch
            {
                MessageBox.Show("Đơn giá dịch vụ không hợp lệ.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Không thêm trùng một dịch vụ (nếu muốn cho phép trùng thì bỏ đoạn này)
            foreach (DataRow row in _dvSelected.Rows)
            {
                if (Convert.ToInt32(row["MaDV"]) == maDV)
                {
                    MessageBox.Show("Dịch vụ này đã được chọn.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            _dvSelected.Rows.Add(maDV, tenDV, donGia);
            UpdateTien();
        }

        private void BtnXoaDV_Click(object? sender, EventArgs e)
        {
            if (_dvSelected == null || _gridDichVu.CurrentRow == null) return;

            int idx = _gridDichVu.CurrentRow.Index;
            if (idx < 0 || idx >= _dvSelected.Rows.Count) return;

            _dvSelected.Rows.RemoveAt(idx);
            UpdateTien();
        }
    }
}
