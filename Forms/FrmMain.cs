using QuanLyPhongKham.Data;
using QuanLyPhongKham.Models;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace QuanLyPhongKham.Forms
{
    public class FrmMain : Form
    {
        // ===== THEME (màu xanh phòng khám) =====
        // ===== THEME (màu của app, sẽ đổi được) =====
        private Color C_BG;          // nền content
        private Color C_BG2;         // topbar
        private Color C_ACCENT;      // màu nhấn
        private Color C_TEXT;        // chữ chính
        private Color C_MUTED;       // chữ phụ
        private Color C_SIDE;        // sidebar
        private Color C_SIDE_HOVER;  // hover sidebar


        // ===== LAYOUT =====
        const int SIDE_W = 208;
        const int TOP_H = 84;

        // ===== UI =====
        private Panel sidebar, topbar, mainPanel;
        private FlowLayoutPanel sideItems;
        private Label lblTitle, lblHello;

        // **KHÔNG readonly nữa để đăng nhập lại đổi được tên bác sĩ**
        private BacSiModel _currentUser;

        // drag cho borderless
        private bool _dragging;
        private Point _dragStart;

        public FrmMain(BacSiModel user)
        {
            _currentUser = user;

            Text = "Quản Lý Phòng Khám";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1200, 750);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.Dpi;

            ApplyThemeAndLanguage();

            AppSettings.SettingsChanged += ApplyThemeAndLanguage;

            BuildUI();
        }


        private void BuildUI()
        {
            Controls.Clear();

            // ===== SIDEBAR (trái) =====
            sidebar = new Panel { BackColor = C_SIDE };
            Controls.Add(sidebar);

            sideItems = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 12, 0, 12),
                AutoScroll = true
            };
            sidebar.Controls.Add(sideItems);

            // Dashboard: BIỂU ĐỒ DOANH THU, THỐNG KÊ
            AddSidebarButton("Dashboard", "dashboard.png", () => ShowDashboard(), true);
            AddSidebarButton("Bác sĩ", "doctor.png", () => LoadFormInPanel(new FrmBacSi()));
            AddSidebarButton("Bệnh nhân", "patient.png", () => LoadFormInPanel(new FrmBenhNhan()));
            AddSidebarButton("Giường bệnh", "bed.png", () => LoadFormInPanel(new FrmGiuongBenh()));
            AddSidebarButton("Khoa khám", "department.png", () => LoadFormInPanel(new FrmKhoa()));
            AddSidebarButton("Kho Thuốc", "medicine.png", () => LoadFormInPanel(new FrmLoaiThuoc()));
            AddSidebarButton("Dịch vụ", "service.png", () => LoadFormInPanel(new FrmDichVu()));
            AddSidebarButton("Lịch hẹn", "calendar.png", () => LoadFormInPanel(new FrmLichHen()));
            AddSidebarButton("Đơn thuốc", "prescription.png", () => LoadFormInPanel(new FrmDonThuoc()));
            AddSidebarButton("Hóa đơn", "invoice.png", () => LoadFormInPanel(new FrmHoaDon()));
            AddSidebarButton("Báo cáo", "report.png", () => LoadFormInPanel(new FrmBaoCao()));
            AddSidebarButton("Phân quyền", "shield.png", () => LoadFormInPanel(new FrmPhanQuyen()));
            AddSidebarButton("Cài đặt", "settings.png", () => LoadFormInPanel(new FrmCaiDat()));
            AddSidebarButton("Trợ giúp", "help.png", () => LoadFormInPanel(new FrmTroGiup()));

            // ====== ĐĂNG XUẤT: mở lại form đăng nhập, không restart app ======
            AddSidebarButton("Đăng xuất", "exit.png", () => LogoutAndRelogin());

            // ===== TOPBAR (trên, bên phải) =====
            topbar = new Panel { BackColor = C_BG2 };
            Controls.Add(topbar);

            // drag borderless
            topbar.MouseDown += (_, e) =>
            {
                _dragging = true;
                _dragStart = e.Location;
            };
            topbar.MouseMove += (_, e) =>
            {
                if (!_dragging) return;
                var p = PointToScreen(e.Location);
                Location = new Point(p.X - _dragStart.X, p.Y - _dragStart.Y);
            };
            topbar.MouseUp += (_, __) => _dragging = false;

            // viền dưới
            topbar.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(210, 220, 210), 1);
                e.Graphics.DrawLine(p, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
            };

            var logo = new PictureBox
            {
                Image = LoadIcon("Logo-Clinic.png", 60, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Left = 16,
                Top = 12,
                Height = 60,
                Width = 80
            };
            topbar.Controls.Add(logo);

            lblTitle = new Label
            {
                Text = "PHÒNG KHÁM ĐA KHOA",
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                ForeColor = C_ACCENT,
                AutoSize = true,
                Top = 18,
                Left = 86
            };
            topbar.Controls.Add(lblTitle);

            lblHello = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                ForeColor = C_TEXT,
                Text = $"Xin chào {_currentUser.TenBS}!"
            };
            topbar.Controls.Add(lblHello);

            // nút min/max/close
            var btnClose = WinBtn("×", (s, e) => Close());
            var btnMax = WinBtn("□", (s, e) =>
            {
                WindowState = WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            });
            var btnMin = WinBtn("–", (s, e) => WindowState = FormWindowState.Minimized);

            btnClose.Top = btnMax.Top = btnMin.Top = 18;

            topbar.Controls.AddRange(new Control[] { btnMin, btnMax, btnClose });

            void PlaceWinBtns()
            {
                int right = topbar.Width;

                btnClose.Left = right - 48;
                btnMax.Left = right - 96;
                btnMin.Left = right - 144;

                // vị trí "Xin chào bác sĩ ..."
                lblHello.Top = 30;
                lblHello.Left = btnMin.Left - lblHello.Width - 20;
            }

            PlaceWinBtns();
            topbar.Resize += (_, __) => PlaceWinBtns();

            // ===== MAIN (nội dung) =====
            mainPanel = new DoubleBufferedPanel
            {
                BackColor = C_BG,
                Padding = new Padding(20)
            };
            Controls.Add(mainPanel);

            // ===== KHÓA LAYOUT =====
            LayoutHardLock(null, EventArgs.Empty);
            this.Resize -= LayoutHardLock;
            this.Resize += LayoutHardLock;

            // Mặc định mở DASHBOARD (biểu đồ)
            ShowDashboard();
        }

        // Khóa layout theo Bounds, tránh topbar đè sidebar
        private void LayoutHardLock(object? s, EventArgs e)
        {
            sidebar.Dock = DockStyle.None;
            sidebar.Bounds = new Rectangle(0, 0, SIDE_W, ClientSize.Height);

            topbar.Dock = DockStyle.None;
            topbar.Bounds = new Rectangle(SIDE_W, 0, ClientSize.Width - SIDE_W, TOP_H);

            mainPanel.Dock = DockStyle.None;
            mainPanel.Bounds = new Rectangle(SIDE_W, TOP_H, ClientSize.Width - SIDE_W, ClientSize.Height - TOP_H);

            sidebar.BringToFront();
            topbar.BringToFront();
            mainPanel.BringToFront();
        }

        private Button WinBtn(string text, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Width = 36,
                Height = 36,
                BackColor = Color.Transparent,
                ForeColor = C_TEXT,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 240, 220);
            b.Click += onClick;
            return b;
        }

        // ===== SIDEBAR ITEM =====
        private void AddSidebarButton(string text, string icon, Action onClick, bool active = false)
        {
            var panel = new Panel
            {
                Height = 48,
                Width = SIDE_W,
                BackColor = active ? C_SIDE_HOVER : C_SIDE,
                Margin = new Padding(0, 0, 0, 4),
                Cursor = Cursors.Hand
            };

            var bar = new Panel
            {
                BackColor = C_ACCENT,
                Width = 4,
                Dock = DockStyle.Left,
                Visible = active
            };
            panel.Controls.Add(bar);

            var pic = new PictureBox
            {
                Image = LoadIcon(icon, 22, 22),
                SizeMode = PictureBoxSizeMode.Zoom,
                Left = 12,
                Top = 13,
                Width = 22,
                Height = 22
            };
            panel.Controls.Add(pic);

            var lbl = new Label
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                Left = 46,
                Top = 13,
                AutoSize = true
            };
            panel.Controls.Add(lbl);

            void Activate()
            {
                foreach (Control c in sideItems.Controls)
                    if (c is Panel p)
                    {
                        p.BackColor = C_SIDE;
                        if (p.Controls.Count > 0 && p.Controls[0] is Panel leftBar)
                            leftBar.Visible = false;
                    }

                panel.BackColor = C_SIDE_HOVER;
                bar.Visible = true;
                onClick();
            }

            panel.Click += (_, __) => Activate();
            pic.Click += (_, __) => Activate();
            lbl.Click += (_, __) => Activate();

            panel.MouseEnter += (_, __) =>
            {
                if (!bar.Visible) panel.BackColor = C_SIDE_HOVER;
            };
            panel.MouseLeave += (_, __) =>
            {
                if (!bar.Visible) panel.BackColor = C_SIDE;
            };

            sideItems.Controls.Add(panel);
        }

        // ===== LOAD FORM VÀO MAIN =====
        private void LoadFormInPanel(Form f)
        {
            mainPanel.Controls.Clear();
            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(f);
            f.Show();
        }

        // ===== DASHBOARD: BIỂU ĐỒ DOANH THU, THỐNG KÊ =====
        private void ShowDashboard()
        {
            LoadFormInPanel(new FrmDashBoard()); // form biểu đồ
        }

        // ===== ĐĂNG XUẤT & ĐĂNG NHẬP LẠI =====
        private void LogoutAndRelogin()
        {
            var dr = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất và đăng nhập lại?",
                "Đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes)
                return;

            // Xóa nội dung hiện tại
            mainPanel.Controls.Clear();

            // Hiển thị form đăng nhập như dialog
            using (var login = new FrmLogin())
            {
                var result = login.ShowDialog(this);

                // đăng nhập thành công -> cập nhật lại user & dashboard
                if (result == DialogResult.OK && login.LoggedInUser != null)
                {
                    _currentUser = login.LoggedInUser;
                    lblHello.Text = $"Xin chào {_currentUser.TenBS}!";
                    ShowDashboard();
                }
                else
                {
                    // nếu user hủy đăng nhập thì đóng luôn app
                    Close();
                }
            }
        }
        private void ApplyThemeAndLanguage()
        {
            // 1. Chọn màu theo Theme hiện tại
            switch (AppSettings.Theme)
            {
                case AppTheme.Light:
                    C_BG = Color.WhiteSmoke;
                    C_BG2 = Color.FromArgb(245, 245, 245);
                    C_ACCENT = Color.FromArgb(30, 136, 229);
                    C_TEXT = Color.FromArgb(33, 33, 33);
                    C_MUTED = Color.FromArgb(120, 120, 120);
                    C_SIDE = Color.FromArgb(55, 71, 79);
                    C_SIDE_HOVER = Color.FromArgb(69, 90, 100);
                    break;

                case AppTheme.Dark:
                    C_BG = Color.FromArgb(33, 33, 33);
                    C_BG2 = Color.FromArgb(48, 48, 48);
                    C_ACCENT = Color.FromArgb(129, 199, 132);
                    C_TEXT = Color.WhiteSmoke;
                    C_MUTED = Color.FromArgb(189, 189, 189);
                    C_SIDE = Color.FromArgb(38, 50, 56);
                    C_SIDE_HOVER = Color.FromArgb(69, 90, 100);
                    break;

                default: // AppTheme.Green (mặc định)
                    C_BG = Color.FromArgb(232, 245, 233);
                    C_BG2 = Color.FromArgb(200, 230, 201);
                    C_ACCENT = Color.FromArgb(56, 142, 60);
                    C_TEXT = Color.FromArgb(27, 94, 32);
                    C_MUTED = Color.FromArgb(97, 97, 97);
                    C_SIDE = Color.FromArgb(27, 94, 32);
                    C_SIDE_HOVER = Color.FromArgb(46, 125, 50);
                    break;
            }

            // 2. Áp màu vào những control đã tạo (nếu đã build UI)
            BackColor = C_BG;

            if (sidebar != null)
            {
                sidebar.BackColor = C_SIDE;
                if (sideItems != null)
                {
                    foreach (Control c in sideItems.Controls)
                    {
                        if (c is Panel p)
                        {
                            // panel sidebar item
                            p.BackColor = C_SIDE;
                        }
                    }
                }
            }

            if (topbar != null)
            {
                topbar.BackColor = C_BG2;
                topbar.Invalidate();
            }

            if (mainPanel != null)
            {
                mainPanel.BackColor = C_BG;
                mainPanel.Invalidate();
            }

            if (lblTitle != null)
            {
                lblTitle.ForeColor = C_ACCENT;
                lblTitle.Text = AppSettings.Language == AppLanguage.English
                    ? "CLINIC MANAGEMENT"
                    : "PHÒNG KHÁM ĐA KHOA";
            }

            if (lblHello != null && _currentUser != null)
            {
                lblHello.ForeColor = C_TEXT;
                lblHello.Text = AppSettings.Language == AppLanguage.English
                    ? $"Hello, Dr. {_currentUser.TenBS}!"
                    : $"Xin chào {_currentUser.TenBS}!";
            }

            Invalidate();
        }


        // ===== Helper load icon =====
        private Image? LoadIcon(string fileName, int w, int h)
        {
            try
            {
                var path = Path.Combine(Application.StartupPath, "Images", fileName);
                if (!File.Exists(path)) return null;
                using var img = Image.FromFile(path);
                return new Bitmap(img, new Size(w, h));
            }
            catch
            {
                return null;
            }
        }

        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }
    }
}
