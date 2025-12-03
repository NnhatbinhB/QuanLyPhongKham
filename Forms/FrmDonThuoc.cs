using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmDonThuoc : Form
    {
        // ====== CONTROL ======
        private ComboBox _cboLoai;           // Bệnh nhân thường / Nằm giường
        private DataGridView _gridBenhNhan;  // danh sách bệnh nhân
        private DataGridView _gridDonThuoc;  // đơn thuốc của bệnh nhân chọn

        // Khu vực chọn thuốc (vừa nhập vừa gợi ý)
        private ComboBox _cbThuoc;           // lấy từ bảng LoaiThuoc
        private NumericUpDown _numTienThuoc; // đơn giá (có thể chỉnh)
        private Button _btnThem;

        private int? _currentBNT;            // sttBNT
        private int? _currentBNNG_DXC;       // sttBNNG_DXC

        private DataTable? _srcBenhNhan;
        private DataTable? _srcDonThuoc;

        private bool IsNamGiuong => _cboLoai.SelectedIndex == 1;

        // Nguồn dữ liệu thuốc
        private DataTable? _thuocSource;

        public FrmDonThuoc()
        {
            Text = "Quản lý đơn thuốc";
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F);

            BuildLayout();
            LoadLoaiThuoc();             // nạp danh sách thuốc
            _cboLoai.SelectedIndex = 1;  // mặc định: bệnh nhân nằm giường
            LoadBenhNhan();
        }

        // ================== UI ==================
        private void BuildLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            // ===== HÀNG 0 TRÁI: chọn loại bệnh nhân =====
            var topLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 10, 8, 10) };
            layout.Controls.Add(topLeft, 0, 0);

            var lblLoai = new Label
            {
                Text = "Loại bệnh nhân:",
                AutoSize = true,
                Location = new Point(0, 10)
            };
            topLeft.Controls.Add(lblLoai);

            _cboLoai = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(120, 6),
                Width = 200
            };
            _cboLoai.Items.AddRange(new object[]
            {
                "Bệnh nhân thường",
                "Bệnh nhân nằm giường"
            });
            _cboLoai.SelectedIndexChanged += (s, e) => LoadBenhNhan();
            topLeft.Controls.Add(_cboLoai);

            // ===== HÀNG 0 PHẢI: chọn thuốc (vừa nhập vừa gợi ý) =====
            var topRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 10, 16, 10) };
            layout.Controls.Add(topRight, 1, 0);

            var flowRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = false, // 1 dòng
                FlowDirection = FlowDirection.LeftToRight
            };
            topRight.Controls.Add(flowRight);

            var lblThuoc = new Label
            {
                Text = "Thuốc:",
                AutoSize = true,
                Margin = new Padding(0, 6, 4, 0)
            };
            flowRight.Controls.Add(lblThuoc);

            _cbThuoc = new ComboBox
            {
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDown, // cho phép gõ
                Margin = new Padding(0, 2, 12, 0)
            };
            // AutoComplete: gõ là suggest các item trong list
            _cbThuoc.AutoCompleteSource = AutoCompleteSource.ListItems;
            _cbThuoc.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _cbThuoc.SelectedIndexChanged += CbThuoc_SelectedIndexChanged;
            flowRight.Controls.Add(_cbThuoc);

            var lblTien = new Label
            {
                Text = "Đơn giá:",
                AutoSize = true,
                Margin = new Padding(0, 6, 4, 0)
            };
            flowRight.Controls.Add(lblTien);

            _numTienThuoc = new NumericUpDown
            {
                Width = 90,
                Maximum = 100000000,
                Minimum = 0,
                Increment = 1000,
                ThousandsSeparator = true,
                Margin = new Padding(0, 2, 12, 0)
            };
            flowRight.Controls.Add(_numTienThuoc);

            _btnThem = new Button
            {
                Text = "Thêm đơn",
                Width = 90,
                Height = 30,
                BackColor = Color.FromArgb(67, 160, 71),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 2, 0, 0)
            };
            _btnThem.FlatAppearance.BorderSize = 0;
            _btnThem.Click += BtnThem_Click;
            flowRight.Controls.Add(_btnThem);

            // ===== HÀNG 1: 2 GRID =====
            _gridBenhNhan = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
            };
            _gridBenhNhan.CellClick += GridBenhNhan_CellClick;
            layout.Controls.Add(_gridBenhNhan, 0, 1);

            _gridDonThuoc = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            layout.Controls.Add(_gridDonThuoc, 1, 1);
        }

        // ================== LOAD THUỐC TỪ LoaiThuoc ==================
        private void LoadLoaiThuoc()
        {
            try
            {
                const string sql = @"SELECT MaLoaiThuoc, TenLoai, DonGia
                                     FROM LoaiThuoc
                                     ORDER BY TenLoai;";

                _thuocSource = Database.Query(sql, CommandType.Text) ?? new DataTable();

                _cbThuoc.DataSource = _thuocSource;
                _cbThuoc.DisplayMember = "TenLoai";
                _cbThuoc.ValueMember = "MaLoaiThuoc";

                // set đơn giá ban đầu nếu có dòng
                CbThuoc_SelectedIndexChanged(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách loại thuốc:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CbThuoc_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (_cbThuoc.SelectedItem is DataRowView drv &&
                    int.TryParse(drv["DonGia"]?.ToString(), out var gia))
                {
                    if (gia < (int)_numTienThuoc.Minimum) gia = (int)_numTienThuoc.Minimum;
                    if (gia > (int)_numTienThuoc.Maximum) gia = (int)_numTienThuoc.Maximum;
                    _numTienThuoc.Value = gia;
                }
            }
            catch
            {
                // bỏ qua lỗi map
            }
        }

        // ================== LOAD DANH SÁCH BỆNH NHÂN ==================
        private void LoadBenhNhan()
        {
            try
            {
                _currentBNT = null;
                _currentBNNG_DXC = null;
                _srcDonThuoc = null;
                _gridDonThuoc.DataSource = null;

                if (IsNamGiuong)
                {
                    _srcBenhNhan = DonThuocRepository.GetAllBenhNhanNamGiuong_DaXepCho();
                }
                else
                {
                    _srcBenhNhan = DonThuocRepository.GetAllBenhNhanThuong();
                }

                _gridBenhNhan.DataSource = _srcBenhNhan;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bệnh nhân:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridBenhNhan_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _gridBenhNhan.CurrentRow == null) return;

            try
            {
                var row = _gridBenhNhan.CurrentRow;

                if (IsNamGiuong)
                {
                    // sp_SelectAllBenhNhanNamGiuong_DaXepCho trả về cột sttBNNG_DXC
                    _currentBNNG_DXC = Convert.ToInt32(row.Cells["sttBNNG_DXC"].Value);
                    _currentBNT = null;
                    LoadDonThuoc_NamGiuong();
                }
                else
                {
                    // sp_SelectAllBenhNhanThuong trả về cột sttBNT
                    _currentBNT = Convert.ToInt32(row.Cells["sttBNT"].Value);
                    _currentBNNG_DXC = null;
                    LoadDonThuoc_Thuong();
                }
            }
            catch
            {
                // ignore mapping error
            }
        }

        private void LoadDonThuoc_Thuong()
        {
            if (_currentBNT == null) return;
            try
            {
                _srcDonThuoc = DonThuocRepository.GetDonThuocBenhNhanThuong(_currentBNT.Value);
                _gridDonThuoc.DataSource = _srcDonThuoc;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải đơn thuốc bệnh nhân thường:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDonThuoc_NamGiuong()
        {
            if (_currentBNNG_DXC == null) return;
            try
            {
                _srcDonThuoc = DonThuocRepository.GetDonThuocBenhNhanNamGiuong(_currentBNNG_DXC.Value);
                _gridDonThuoc.DataSource = _srcDonThuoc;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải đơn thuốc bệnh nhân nằm giường:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================== THÊM ĐƠN THUỐC ==================
        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!(_cbThuoc.SelectedItem is DataRowView drv))
            {
                MessageBox.Show("Hãy chọn một loại thuốc.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tenThuoc = drv["TenLoai"]?.ToString() ?? "";
            int tienThuoc = (int)_numTienThuoc.Value;

            if (string.IsNullOrWhiteSpace(tenThuoc))
            {
                MessageBox.Show("Tên thuốc không hợp lệ.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (tienThuoc <= 0)
            {
                MessageBox.Show("Đơn giá thuốc phải lớn hơn 0.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (IsNamGiuong)
                {
                    if (_currentBNNG_DXC == null)
                    {
                        MessageBox.Show("Hãy chọn một bệnh nhân nằm giường.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DonThuocRepository.InsertDonThuocBenhNhanNamGiuong(
                        tenThuoc,
                        tienThuoc,
                        _currentBNNG_DXC.Value);

                    LoadDonThuoc_NamGiuong();
                }
                else
                {
                    if (_currentBNT == null)
                    {
                        MessageBox.Show("Hãy chọn một bệnh nhân thường.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DonThuocRepository.InsertDonThuocBenhNhanThuong(
                        tenThuoc,
                        tienThuoc,
                        _currentBNT.Value);

                    LoadDonThuoc_Thuong();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm đơn thuốc:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
