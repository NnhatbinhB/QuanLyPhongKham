using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyPhongKham.Data;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmGiuongBenh : Form
    {
        private FlowLayoutPanel _bedFlow;
        private Label _lblSummary;
        private ComboBox _cboFilter;
        private readonly ToolTip _toolTip = new ToolTip();

        // Bệnh nhân chưa xếp giường
        private DataGridView _gridWaiting;
        private Button _btnAssign;

        // Tìm theo tên bệnh nhân đang nằm giường
        private TextBox _txtSearchPatient;

        // dữ liệu
        private List<BedInfo> _allBeds = new();
        private List<WaitingPatient> _waitingPatients = new();

        // selection
        private Panel? _selectedCard;
        private int? _selectedBedId;

        public FrmGiuongBenh()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(232, 245, 233);
            DoubleBuffered = true;
            Text = "Giường bệnh - Quản lý phòng khám";

            BuildLayout();
            LoadBeds();
        }

        // =====================================================
        // DỰNG GIAO DIỆN
        // =====================================================
        private void BuildLayout()
        {
            Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // BN chưa xếp giường
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));  // filter + nút + summary
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // card giường
            Controls.Add(layout);

            // ================== HÀNG 0: BỆNH NHÂN CHƯA XẾP GIƯỜNG ==================
            var waitingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 20, 20, 8),
                BackColor = Color.FromArgb(232, 245, 233)
            };
            layout.Controls.Add(waitingPanel, 0, 0);

            var waitingHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 26
            };
            waitingPanel.Controls.Add(waitingHeader);

            var lblWaiting = new Label
            {
                Text = "Bệnh nhân chưa xếp giường",
                Dock = DockStyle.Left,
                Width = 280,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };
            waitingHeader.Controls.Add(lblWaiting);

            _btnAssign = new Button
            {
                Text = "Xếp giường",
                Width = 120,
                Height = 24,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                Margin = new Padding(2),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(2)
            };
            _btnAssign.FlatAppearance.BorderSize = 0;
            _btnAssign.Click += (s, e) => AssignBedToSelectedPatient();
            waitingHeader.Controls.Add(_btnAssign);

            _gridWaiting = new DataGridView
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
            waitingPanel.Controls.Add(_gridWaiting);

            _gridWaiting.Columns.Add("MaBN", "Mã BN");
            _gridWaiting.Columns.Add("HoTen", "Họ tên");
            _gridWaiting.Columns.Add("CMND", "CMND");
            _gridWaiting.Columns.Add("DiaChi", "Địa chỉ");
            _gridWaiting.Columns.Add("BacSi", "Bác sĩ điều trị");

            // ================== HÀNG 1: THANH FILTER + NÚT + TỔNG ==================
            var middlePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 8, 20, 8)
            };
            layout.Controls.Add(middlePanel, 0, 1);

            var lblTitle = new Label
            {
                Text = "Quản lý giường bệnh",
                AutoSize = true,
                Location = new Point(0, 10),
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32)
            };
            middlePanel.Controls.Add(lblTitle);

            var lblFilter = new Label
            {
                Text = "Lọc theo tình trạng:",
                AutoSize = true,
                Location = new Point(0, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };
            middlePanel.Controls.Add(lblFilter);

            _cboFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(130, 36),
                Width = 160
            };
            _cboFilter.Items.AddRange(new object[]
            {
                "Tất cả",
                "Có bệnh nhân",
                "Còn trống",
                "Đang sửa / bảo trì",
                "Đặt trước",
                "Khác"
            });
            _cboFilter.SelectedIndex = 0;
            _cboFilter.SelectedIndexChanged += (s, e) => LoadBeds();
            middlePanel.Controls.Add(_cboFilter);

            // --- TÌM THEO TÊN BỆNH NHÂN ---
            var lblSearchPatient = new Label
            {
                Text = "Bệnh nhân:",
                AutoSize = true,
                Location = new Point(310, 40)
            };
            middlePanel.Controls.Add(lblSearchPatient);

            _txtSearchPatient = new TextBox
            {
                Location = new Point(390, 36),
                Width = 180
            };
            _txtSearchPatient.TextChanged += (s, e) => LoadBeds();
            middlePanel.Controls.Add(_txtSearchPatient);

            // helper tạo nút
            Button MakeButton(string text, int x, Color color)
            {
                var btn = new Button
                {
                    Text = text,
                    Width = 90,
                    Height = 28,
                    Location = new Point(x, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = color,
                    ForeColor = Color.White
                };
                btn.FlatAppearance.BorderSize = 0;
                return btn;
            }

            var btnRefresh = MakeButton("Làm mới", 600, Color.FromArgb(120, 144, 156));
            btnRefresh.Click += (s, e) =>
            {
                _txtSearchPatient.Text = string.Empty;
                LoadBeds();
            };
            middlePanel.Controls.Add(btnRefresh);

            var btnAdd = MakeButton("Thêm", 700, Color.FromArgb(56, 142, 60));
            btnAdd.Click += (s, e) => AddBed();
            middlePanel.Controls.Add(btnAdd);

            var btnDelete = MakeButton("Xóa", 800, Color.FromArgb(211, 47, 47));
            btnDelete.Click += (s, e) => DeleteSelectedBed();
            middlePanel.Controls.Add(btnDelete);

            var btnUpdate = MakeButton("Sửa", 900, Color.FromArgb(30, 136, 229));
            btnUpdate.Click += (s, e) => EditSelectedBed();
            middlePanel.Controls.Add(btnUpdate);

            _lblSummary = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(middlePanel.Width - 260, 10),
                ForeColor = Color.Gray
            };
            _lblSummary.Text = "Có bệnh nhân: 0 | Còn trống: 0 | Khác: 0";
            middlePanel.Controls.Add(_lblSummary);

            middlePanel.Resize += (s, e) =>
            {
                _lblSummary.Left = middlePanel.Width - _lblSummary.Width - 10;
            };

            // ================== HÀNG 2: KHU CARD GIƯỜNG ==================
            _bedFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(20, 10, 20, 20),
                BackColor = Color.FromArgb(232, 245, 233)
            };
            layout.Controls.Add(_bedFlow, 0, 2);
        }

        // =====================================================
        // LOAD DỮ LIỆU GIƯỜNG
        // =====================================================
        private void LoadBeds()
        {
            try
            {
                // Lấy toàn bộ giường
                _allBeds = BedRepository.GetAllBeds();

                // load lại bệnh nhân chưa xếp giường
                LoadWaitingPatients();

                string statusFilter = _cboFilter.SelectedItem?.ToString() ?? "Tất cả";
                string patientKeyword = _txtSearchPatient?.Text.Trim() ?? string.Empty;

                // Lọc theo trạng thái + tên bệnh nhân
                List<BedInfo> bedsToShow = FilterBeds(statusFilter, patientKeyword);

                // Cập nhật summary theo danh sách đang hiển thị
                UpdateSummaryLabel(bedsToShow);

                _selectedBedId = null;
                _selectedCard = null;

                _bedFlow.SuspendLayout();
                _bedFlow.Controls.Clear();

                int displayIndex = 1;
                foreach (var bed in bedsToShow)
                {
                    var card = CreateBedCard(bed, displayIndex++);
                    _bedFlow.Controls.Add(card);
                }

                _bedFlow.ResumeLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu giường bệnh:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadWaitingPatients()
        {
            try
            {
                _waitingPatients = BedRepository.GetWaitingPatients();
                _gridWaiting.Rows.Clear();

                foreach (var p in _waitingPatients)
                {
                    int idx = _gridWaiting.Rows.Add(
                        p.SttBN,
                        p.HoTen,
                        p.CMND,
                        p.DiaChi,
                        p.BacSi);
                    _gridWaiting.Rows[idx].Tag = p;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bệnh nhân chưa xếp giường:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummaryLabel(IEnumerable<BedInfo> data)
        {
            int coBenhNhan = 0;
            int conTrong = 0;
            int khac = 0;

            foreach (var bed in data)
            {
                string status = (bed.TinhTrang ?? "").Trim();
                bool isEmpty = IsEmptyStatus(status);

                if (status.Equals("Có bệnh nhân", StringComparison.OrdinalIgnoreCase))
                    coBenhNhan++;
                else if (isEmpty)
                    conTrong++;
                else
                    khac++;
            }

            _lblSummary.Text = $"Có bệnh nhân: {coBenhNhan} | Còn trống: {conTrong} | Khác: {khac}";
        }

        private static bool IsEmptyStatus(string status)
        {
            return status.Equals("Còn trống", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Còn Trống", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Trống", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Lọc giường theo trạng thái + tên bệnh nhân.
        /// </summary>
        private List<BedInfo> FilterBeds(string filter, string patientKeyword)
        {
            IEnumerable<BedInfo> source = _allBeds;

            if (!string.IsNullOrWhiteSpace(patientKeyword))
            {
                string kw = patientKeyword.Trim().ToLower();
                source = source.Where(b => (b.BenhNhan ?? "").ToLower().Contains(kw));
            }

            var result = new List<BedInfo>();

            foreach (var bed in source)
            {
                string status = (bed.TinhTrang ?? "").Trim();
                bool isEmpty = IsEmptyStatus(status);
                bool match = false;

                switch (filter)
                {
                    case "Tất cả":
                        match = true;
                        break;
                    case "Có bệnh nhân":
                        match = status.Equals("Có bệnh nhân", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "Còn trống":
                        match = isEmpty;
                        break;
                    case "Đang sửa / bảo trì":
                        match = status.IndexOf("sửa", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                status.IndexOf("bảo trì", StringComparison.OrdinalIgnoreCase) >= 0;
                        break;
                    case "Đặt trước":
                        match = status.IndexOf("đặt", StringComparison.OrdinalIgnoreCase) >= 0;
                        break;
                    case "Khác":
                        bool isCoBN = status.Equals("Có bệnh nhân", StringComparison.OrdinalIgnoreCase);
                        bool isSua = status.IndexOf("sửa", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     status.IndexOf("bảo trì", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isDat = status.IndexOf("đặt", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isTrong = isEmpty;
                        match = !(isCoBN || isTrong || isSua || isDat);
                        break;
                }

                if (match)
                    result.Add(bed);
            }

            return result;
        }

        // =====================================================
        // CARD GIƯỜNG
        // =====================================================
        private Control CreateBedCard(BedInfo bed, int displayIndex)
        {
            var panel = new Panel
            {
                Width = 210,
                Height = 135,
                Margin = new Padding(10),
                BackColor = Color.White,
                Tag = bed
            };

            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                rect.Inflate(-1, -1);
                using (var path = RoundedRect(rect, 12))
                using (var bg = new SolidBrush(panel.BackColor))
                {
                    var borderColor = Color.FromArgb(200, 230, 201);
                    if (ReferenceEquals(panel, _selectedCard))
                        borderColor = Color.FromArgb(25, 118, 210);

                    using (var border = new Pen(borderColor, 1.4f))
                    {
                        e.Graphics.SmoothingMode =
                            System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(bg, path);
                        e.Graphics.DrawPath(border, path);
                    }
                }
            };

            string status = (bed.TinhTrang ?? string.Empty).Trim();
            Color statusColor;
            Color badgeBack;
            Color badgeFore = Color.White;
            string badgeText;

            bool isEmpty = IsEmptyStatus(status);

            if (status.Equals("Có bệnh nhân", StringComparison.OrdinalIgnoreCase))
            {
                statusColor = Color.FromArgb(27, 94, 32);
                badgeBack = Color.FromArgb(67, 160, 71);
                badgeText = "Đang dùng";
            }
            else if (isEmpty)
            {
                statusColor = Color.FromArgb(38, 166, 154);
                badgeBack = Color.FromArgb(200, 230, 201);
                badgeFore = Color.FromArgb(27, 94, 32);
                badgeText = "Còn trống";
            }
            else if (status.IndexOf("sửa", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     status.IndexOf("bảo trì", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                statusColor = Color.FromArgb(245, 124, 0);
                badgeBack = Color.FromArgb(255, 183, 77);
                badgeText = "Bảo trì";
            }
            else if (status.IndexOf("đặt", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                statusColor = Color.FromArgb(183, 28, 28);
                badgeBack = Color.FromArgb(229, 57, 53);
                badgeText = "Đặt trước";
            }
            else
            {
                statusColor = Color.FromArgb(97, 97, 97);
                badgeBack = Color.FromArgb(158, 158, 158);
                badgeText = "Khác";
            }

            string rawPatient = bed.BenhNhan?.Trim() ?? "";
            string displayPatient;

            if (isEmpty)
                displayPatient = "Chưa có bệnh nhân";
            else if (status.Equals("Có bệnh nhân", StringComparison.OrdinalIgnoreCase))
                displayPatient = string.IsNullOrEmpty(rawPatient)
                    ? "Đang cập nhật tên bệnh nhân"
                    : rawPatient;
            else
                displayPatient = string.IsNullOrEmpty(rawPatient)
                    ? "Chưa có bệnh nhân"
                    : rawPatient;

            // ==== TÊN GIƯỜNG = ID GIƯỜNG BỆNH ====
            string bedDisplayName = $"Giường {bed.Id}";

            var lblBed = new Label
            {
                Text = bedDisplayName,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(16, 12),
                Width = panel.Width - 32,
                Height = 24
            };
            panel.Controls.Add(lblBed);

            var lblStatus = new Label
            {
                Text = string.IsNullOrWhiteSpace(status) ? "Không rõ" : status,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = statusColor,
                Location = new Point(16, 40)
            };
            panel.Controls.Add(lblStatus);

            var lblPatient = new Label
            {
                Text = displayPatient,
                AutoSize = false,
                Width = panel.Width - 32,
                Height = 40,
                Location = new Point(16, 60),
                ForeColor = Color.FromArgb(55, 71, 79)
            };
            panel.Controls.Add(lblPatient);

            var badge = new Label
            {
                Text = badgeText,
                AutoSize = false,
                Width = 90,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = badgeBack,
                ForeColor = badgeFore,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(panel.Width - 16 - 90, panel.Height - 16 - 24)
            };
            panel.Controls.Add(badge);

            var statusBar = new Panel
            {
                Height = 4,
                Width = panel.Width - 32,
                Left = 16,
                Top = panel.Height - 8,
                BackColor = statusColor
            };
            panel.Controls.Add(statusBar);
            statusBar.BringToFront();

            string tooltipText =
                $"Giường: {bedDisplayName}\n" +
                $"ID thực: {bed.Id}\n" +
                $"Tình trạng: {status}\n" +
                $"Bệnh nhân: {displayPatient}";
            _toolTip.SetToolTip(panel, tooltipText);

            void OnClick(object? s, EventArgs e)
            {
                _selectedBedId = bed.Id;

                if (_selectedCard != null && !_selectedCard.IsDisposed)
                {
                    _selectedCard.BackColor = Color.White;
                    _selectedCard.Invalidate();
                }

                _selectedCard = panel;
                panel.BackColor = Color.FromArgb(227, 242, 253);
                panel.Invalidate();
            }

            panel.Cursor = Cursors.Hand;
            panel.Click += OnClick;
            lblBed.Click += OnClick;
            lblStatus.Click += OnClick;
            lblPatient.Click += OnClick;
            badge.Click += OnClick;
            statusBar.Click += OnClick;

            panel.DoubleClick += (s, e) =>
            {
                MessageBox.Show(tooltipText, "Chi tiết giường bệnh",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            return panel;
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);                        // Góc trái trên
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);                // Góc phải trên
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);         // Góc phải dưới
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);                // Góc trái dưới
            path.CloseFigure();

            return path;
        }


        // =====================================================
        // THÊM / XÓA / SỬA / XẾP GIƯỜNG 
        // =====================================================
        private void AddBed()
        {
            try
            {
                BedRepository.InsertNewBed("Còn Trống");
                LoadBeds();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể thêm giường mới:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedBed()
        {
            if (_selectedBedId == null)
            {
                MessageBox.Show("Hãy click chọn một card giường trước khi xóa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var bed = _allBeds.FirstOrDefault(b => b.Id == _selectedBedId.Value);
            if (bed == null)
            {
                MessageBox.Show("Không tìm thấy thông tin giường được chọn.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (BedRepository.BedHasPatient(bed.Id))
            {
                MessageBox.Show("Giường đang có bệnh nhân, không thể xóa.\n" +
                                "Hãy chuyển / xuất viện bệnh nhân trước.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dr = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa giường này không?\n(ID thực: {bed.Id})",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                BedRepository.DeleteBed(bed.Id);
                LoadBeds();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa giường:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedBed()
        {
            if (_selectedBedId == null)
            {
                MessageBox.Show("Hãy click chọn một card giường trước khi sửa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var bed = _allBeds.FirstOrDefault(b => b.Id == _selectedBedId.Value);
            if (bed == null)
            {
                MessageBox.Show("Không tìm thấy thông tin giường được chọn.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string status = (bed.TinhTrang ?? "").Trim();
            bool isEmpty = IsEmptyStatus(status);
            bool isRepair = status.IndexOf("sửa", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            status.IndexOf("bảo trì", StringComparison.OrdinalIgnoreCase) >= 0;

            string newStatus;

            if (isEmpty)
            {
                var dr = MessageBox.Show(
                    "Chuyển giường này sang trạng thái \"Đang sửa / bảo trì\"?",
                    "Cập nhật trạng thái",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
                newStatus = "Đang sửa / bảo trì";
            }
            else if (isRepair)
            {
                var dr = MessageBox.Show(
                    "Kết thúc bảo trì, chuyển giường về trạng thái \"Còn Trống\"?",
                    "Cập nhật trạng thái",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
                newStatus = "Còn Trống";
            }
            else
            {
                MessageBox.Show(
                    "Giường đang có bệnh nhân hoặc đang được đặt trước.\n" +
                    "Thao tác chuyển trạng thái sẽ thực hiện ở form xếp / xuất viện.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                BedRepository.UpdateBedStatus(bed.Id, newStatus);
                LoadBeds();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể cập nhật trạng thái giường:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AssignBedToSelectedPatient()
        {
            if (_selectedBedId == null)
            {
                MessageBox.Show("Hãy chọn một giường trong danh sách để xếp.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var bed = _allBeds.FirstOrDefault(b => b.Id == _selectedBedId.Value);
            if (bed == null)
            {
                MessageBox.Show("Không tìm thấy thông tin giường được chọn.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsEmptyStatus(bed.TinhTrang ?? ""))
            {
                MessageBox.Show("Chỉ được xếp bệnh nhân vào giường đang CÒN TRỐNG.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_gridWaiting.CurrentRow == null ||
                _gridWaiting.CurrentRow.Tag is not WaitingPatient patient)
            {
                MessageBox.Show("Hãy chọn một bệnh nhân trong danh sách chưa xếp giường.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dr = MessageBox.Show(
                $"Xếp bệnh nhân {patient.HoTen} (Mã {patient.SttBN})\n" +
                $"lên giường đang chọn?",
                "Xác nhận xếp giường",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                BedRepository.AssignBedToPatient(patient.SttBN, bed.Id, DateTime.Now);
                LoadBeds();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xếp giường cho bệnh nhân:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
