using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyPhongKham.Data;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmCaiDat : Form
    {
        private ComboBox _cboTheme;
        private ComboBox _cboLanguage;
        private Button _btnSave;
        private Button _btnClose;

        public FrmCaiDat()
        {
            Text = "Cài đặt hệ thống";
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;
            Padding = new Padding(20);
            Width = 520;
            Height = 280;
            FormBorderStyle = FormBorderStyle.None;
            TopLevel = false; // sẽ được nhét vào FrmMain

            BuildUI();
            LoadCurrentSettings();
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // title
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // theme
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // language
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // buttons
            Controls.Add(layout);

            // ===== TITLE =====
            var lblTitle = new Label
            {
                Text = "CÀI ĐẶT HỆ THỐNG",
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 142, 60),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.SetColumnSpan(lblTitle, 2);
            layout.Controls.Add(lblTitle, 0, 0);

            // ===== THEME =====
            var lblTheme = new Label
            {
                Text = "Giao diện (Theme):",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(lblTheme, 0, 1);

            _cboTheme = new ComboBox
            {
                Dock = DockStyle.Left,
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboTheme.Items.Add("Xanh phòng khám (mặc định)");
            _cboTheme.Items.Add("Sáng (Light)");
            _cboTheme.Items.Add("Tối (Dark)");
            layout.Controls.Add(_cboTheme, 1, 1);

            // ===== LANGUAGE =====
            var lblLanguage = new Label
            {
                Text = "Ngôn ngữ:",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(lblLanguage, 0, 2);

            _cboLanguage = new ComboBox
            {
                Dock = DockStyle.Left,
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboLanguage.Items.Add("Tiếng Việt");
            _cboLanguage.Items.Add("English");
            layout.Controls.Add(_cboLanguage, 1, 2);

            // ===== BUTTONS =====
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            layout.SetColumnSpan(buttonPanel, 2);
            layout.Controls.Add(buttonPanel, 0, 3);

            _btnSave = new Button
            {
                Text = "Lưu cài đặt",
                Width = 120,
                Height = 32,
                BackColor = Color.FromArgb(56, 142, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 0, 0, 0)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(_btnSave);

            _btnClose = new Button
            {
                Text = "Đóng",
                Width = 80,
                Height = 32,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(56, 142, 60),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 0, 0, 0)
            };
            _btnClose.FlatAppearance.BorderColor = Color.FromArgb(56, 142, 60);
            _btnClose.Click += (s, e) => Close();
            buttonPanel.Controls.Add(_btnClose);
        }

        private void LoadCurrentSettings()
        {
            // Theme
            switch (AppSettings.Theme)
            {
                case AppTheme.Green:
                    _cboTheme.SelectedIndex = 0;
                    break;
                case AppTheme.Light:
                    _cboTheme.SelectedIndex = 1;
                    break;
                case AppTheme.Dark:
                    _cboTheme.SelectedIndex = 2;
                    break;
                default:
                    _cboTheme.SelectedIndex = 0;
                    break;
            }

            // Language
            _cboLanguage.SelectedIndex =
                AppSettings.Language == AppLanguage.English ? 1 : 0;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_cboTheme.SelectedIndex < 0) _cboTheme.SelectedIndex = 0;
            if (_cboLanguage.SelectedIndex < 0) _cboLanguage.SelectedIndex = 0;

            AppTheme theme = _cboTheme.SelectedIndex switch
            {
                1 => AppTheme.Light,
                2 => AppTheme.Dark,
                _ => AppTheme.Green
            };

            AppLanguage lang = _cboLanguage.SelectedIndex == 1
                ? AppLanguage.English
                : AppLanguage.Vietnamese;

            AppSettings.Save(theme, lang);

            MessageBox.Show(
                "Đã lưu cài đặt.\nMột số màn hình sẽ đổi màu ngay, các màn hình khác sẽ đổi khi mở lại.",
                "Cài đặt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
