using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using QuanLyPhongKham.Data;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Forms
{
    public class FrmLogin : Form
    {
        private Panel _leftGradientPanel;
        private PictureBox _logoBox;
        private Panel _card;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Label _lblUser;
        private Label _lblPass;
        private TextBox _txtUser;
        private TextBox _txtPass;
        private Button _btnLogin;
        private Button _btnExit;

        // ==== BÁC SĨ ĐÃ ĐĂNG NHẬP (trả về cho Program / FrmMain) ====
        public BacSiModel? LoggedInUser { get; private set; }

        public FrmLogin()
        {
            AutoScaleMode = AutoScaleMode.None;   // tránh bể DPI
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Đăng nhập - Quản lý phòng khám";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = true;
            ClientSize = new Size(900, 520);
            MinimumSize = new Size(740, 460);
            BackColor = Color.FromArgb(232, 245, 233);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // ===== PANEL TRÁI: GRADIENT XANH =====
            _leftGradientPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 420
            };
            _leftGradientPanel.Paint += LeftGradientPanel_Paint;
            Controls.Add(_leftGradientPanel);

            _logoBox = new PictureBox
            {
                Size = new Size(580, 350),
                SizeMode = PictureBoxSizeMode.Zoom,
                Top = 90,
                Left = -65
            };

            var logoPath = Path.Combine(Application.StartupPath, "Images", "Logo-Clinic.png");
            if (File.Exists(logoPath))
            {
                _logoBox.ImageLocation = logoPath;
            }
            _leftGradientPanel.Controls.Add(_logoBox);

            var leftTextPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 50, 24, 24)
            };
            _leftGradientPanel.Controls.Add(leftTextPanel);

            // ===== CARD TRẮNG Ở GIỮA =====
            _card = new Panel
            {
                Size = new Size(420, 340),
                BackColor = Color.Transparent,
            };
            _card.Paint += Card_Paint;
            Controls.Add(_card);

            Load += (s, e) =>
            {
                CenterCard();
                _txtUser.Focus();
            };
            Resize += (s, e) => CenterCard();

            // ==== ĐẶT CONTROL TRÊN CARD THEO TOẠ ĐỘ ====
            // Title
            _lblTitle = new Label
            {
                Text = "ĐĂNG NHẬP HỆ THỐNG",
                Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                AutoSize = true,
                Location = new Point(30, 30)
            };
            _card.Controls.Add(_lblTitle);

            // Subtitle
            _lblSubtitle = new Label
            {
                Text = "Đăng nhập bằng tài khoản bác sĩ để bắt đầu làm việc.",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.Gray,
                AutoSize = false,
                Location = new Point(30, 72),
                Size = new Size(360, 40)
            };
            _card.Controls.Add(_lblSubtitle);

            // ===== USERNAME =====
            _lblUser = new Label
            {
                Text = "Tên Đăng Nhập",
                AutoSize = true,
                Location = new Point(20, 110),
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _card.Controls.Add(_lblUser);

            _txtUser = new TextBox
            {
                Location = new Point(30, 135),
                Width = 360,
                Height = 28
            };
            _card.Controls.Add(_txtUser);

            // ===== PASSWORD =====
            _lblPass = new Label
            {
                Text = "Mật Khẩu",
                AutoSize = true,
                Location = new Point(20, 180),
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _card.Controls.Add(_lblPass);

            _txtPass = new TextBox
            {
                Location = new Point(30, 205),
                Width = 360,
                Height = 28,
                UseSystemPasswordChar = true,
                Bounds = new Rectangle(30, 205, 360, 28)
            };
            _txtPass.KeyDown += TxtPass_KeyDown;
            _card.Controls.Add(_txtPass);

            // ===== BUTTONS =====
            _btnLogin = new Button
            {
                Text = "Đăng Nhập",
                Width = 160,
                Height = 30,
                Location = new Point(30, 255),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            };
            _btnLogin.FlatAppearance.BorderSize = 1;
            _btnLogin.FlatAppearance.BorderColor = Color.LightGreen;
            _btnLogin.Click += BtnLogin_Click;
            _card.Controls.Add(_btnLogin);

            _btnExit = new Button
            {
                Text = "Thoát",
                Width = 120,
                Height = 30,
                Location = new Point(210, 255),
                BackColor = Color.White,
                ForeColor = Color.DarkGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            };
            _btnExit.FlatAppearance.BorderColor = Color.FromArgb(56, 142, 60);
            _btnExit.Click += BtnExit_Click;
            _card.Controls.Add(_btnExit);
        }

        // Căn giữa card login theo vùng còn lại (sau panel trái)
        private void CenterCard()
        {
            int leftSpace = _leftGradientPanel.Right;
            int usableWidth = ClientSize.Width - leftSpace;
            if (usableWidth < _card.Width) usableWidth = _card.Width;

            int x = leftSpace + (usableWidth - _card.Width) / 2;
            int y = (ClientSize.Height - _card.Height) / 2;
            if (y < 10) y = 10;

            _card.Location = new Point(x, y);
        }

        // Gradient xanh lá bên trái
        private void LeftGradientPanel_Paint(object sender, PaintEventArgs e)
        {
            var rect = _leftGradientPanel.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                       rect,
                       Color.FromArgb(200, 230, 201),
                       Color.FromArgb(56, 142, 60),
                       45f))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        // Vẽ border bo góc cho card trắng
        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var rect = _card.ClientRectangle;
            rect.Inflate(-1, -1);
            int radius = 18;
            int d = radius * 2;

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var bg = new SolidBrush(_card.BackColor))
                using (var border = new Pen(Color.LightGreen, 1.4f))
                {
                    e.Graphics.FillPath(bg, path);
                    e.Graphics.DrawPath(border, path);
                }
            }
        }

        private void TxtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DoLogin();
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            DoLogin();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            // Thoát dialog -> trả về Cancel
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DoLogin()
        {
            var username = _txtUser.Text.Trim();
            var password = _txtPass.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                BacSiModel bs = Database.Login(username, password);
                if (bs == null)
                {
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.",
                        "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _txtPass.SelectAll();
                    _txtPass.Focus();
                    return;
                }

                // >>> QUAN TRỌNG: không mở FrmMain ở đây nữa <<<
                LoggedInUser = bs;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kết nối cơ sở dữ liệu:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
