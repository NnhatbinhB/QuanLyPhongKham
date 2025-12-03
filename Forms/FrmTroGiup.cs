using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyPhongKham.Forms
{
    public partial class FrmTroGiup : Form
    {
        // ====== UI controls ======
        private TextBox _txtSearch;
        private Button _btnClearSearch;
        private ListBox _lstTopics;

        private TabControl _tabMain;
        private TabPage _tabHuongDan;
        private TabPage _tabUpdate;
        private TabPage _tabLuuY;
        private TabPage _tabLienHe;

        private RichTextBox _rtbTopicContent;
        private RichTextBox _rtbUpdate;
        private RichTextBox _rtbNotes;

        private Label _lblEmailValue;
        private Label _lblPhoneValue;
        private Button _btnCopyEmail;
        private Button _btnCopyPhone;

        // ====== Data ======
        private readonly List<HelpTopic> _allTopics = new();

        private class HelpTopic
        {
            public string Id { get; set; } = "";
            public string Title { get; set; } = "";
            public string Content { get; set; } = "";
            public string[] Keywords { get; set; } = Array.Empty<string>();

            public override string ToString() => Title;
        }

        public FrmTroGiup()
        {
            InitializeComponent();
            InitTopics();
            ApplyFilter();
        }

        private void InitializeComponent()
        {
            Text = "Trợ giúp & Hướng dẫn sử dụng";
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(232, 245, 233);
            Dock = DockStyle.Fill;

            // ====== Layout chính: trái (tìm kiếm + danh sách) / phải (tabs nội dung) ======
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));   // panel trái
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));    // panel phải
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            // ====== PANEL TRÁI ======
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(27, 94, 32)   // xanh sidebar
            };
            layout.Controls.Add(leftPanel, 0, 0);

            var lblSearchTitle = new Label
            {
                Text = "Tìm kiếm tính năng",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(4, 4)
            };
            leftPanel.Controls.Add(lblSearchTitle);

            _txtSearch = new TextBox
            {
                Location = new Point(8, 30),
                Width = 210
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            leftPanel.Controls.Add(_txtSearch);

            _btnClearSearch = new Button
            {
                Text = "Xóa",
                Width = 52,
                Height = 26,
                Location = new Point(224, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 230, 201),
                ForeColor = Color.FromArgb(27, 94, 32)
            };
            _btnClearSearch.FlatAppearance.BorderSize = 0;
            _btnClearSearch.Click += (s, e) =>
            {
                _txtSearch.Clear();
                _txtSearch.Focus();
            };
            leftPanel.Controls.Add(_btnClearSearch);

            var lblHint = new Label
            {
                Text = "Ví dụ: \"hóa đơn\", \"giường\", \"lịch hẹn\"...",
                ForeColor = Color.WhiteSmoke,
                AutoSize = true,
                Location = new Point(8, 60)
            };
            leftPanel.Controls.Add(lblHint);

            _lstTopics = new ListBox
            {
                Top = 90,
                Left = 8,
                Width = leftPanel.Width - 16,
                Height = leftPanel.Height - 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                IntegralHeight = false,
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _lstTopics.SelectedIndexChanged += LstTopics_SelectedIndexChanged;
            leftPanel.Controls.Add(_lstTopics);

            // ====== PANEL PHẢI ======
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.White
            };
            layout.Controls.Add(rightPanel, 1, 0);

            var lblRightTitle = new Label
            {
                Text = "Hướng dẫn sử dụng hệ thống",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32),
                Location = new Point(0, 0)
            };
            rightPanel.Controls.Add(lblRightTitle);

            var lblRightSub = new Label
            {
                Text = "Chọn một chức năng bên trái hoặc dùng ô tìm kiếm để xem hướng dẫn chi tiết.",
                AutoSize = false,
                Width = rightPanel.Width - 32,
                Height = 36,
                Location = new Point(0, 28),
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            rightPanel.Controls.Add(lblRightSub);

            // TabControl chiếm phần còn lại
            _tabMain = new TabControl
            {
                Location = new Point(0, 70),
                Size = new Size(rightPanel.Width - 32, rightPanel.Height - 86),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            rightPanel.Controls.Add(_tabMain);

            // ====== TAB 1: HƯỚNG DẪN SỬ DỤNG ======
            _tabHuongDan = new TabPage("Hướng dẫn sử dụng");
            _tabMain.TabPages.Add(_tabHuongDan);

            _rtbTopicContent = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                DetectUrls = true
            };
            _tabHuongDan.Controls.Add(_rtbTopicContent);

            // ====== TAB 2: CẦN CẬP NHẬT / CẦN SỬA ======
            _tabUpdate = new TabPage("Cần cập nhật / sửa");
            _tabMain.TabPages.Add(_tabUpdate);

            _rtbUpdate = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            _tabUpdate.Controls.Add(_rtbUpdate);

            // ====== TAB 3: LƯU Ý QUAN TRỌNG ======
            _tabLuuY = new TabPage("Lưu ý");
            _tabMain.TabPages.Add(_tabLuuY);

            _rtbNotes = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            _tabLuuY.Controls.Add(_rtbNotes);

            // ====== TAB 4: LIÊN HỆ HỖ TRỢ ======
            _tabLienHe = new TabPage("Liên hệ");
            _tabMain.TabPages.Add(_tabLienHe);

            BuildLienHeTab();
            FillUpdateAndNotes();
        }

        // =========================================================
        // = KHỞI TẠO DỮ LIỆU HƯỚNG DẪN
        // =========================================================
        private void InitTopics()
        {
            _allTopics.Clear();

            _allTopics.Add(new HelpTopic
            {
                Id = "login",
                Title = "Đăng nhập hệ thống",
                Keywords = new[] { "đăng nhập", "login", "bác sĩ", "tài khoản" },
                Content =
@"1. Màn hình đăng nhập
- Nhập Tên đăng nhập và Mật khẩu do quản trị hệ thống cung cấp.
- Nhấn Enter hoặc nút [Đăng Nhập].

2. Thông báo lỗi thường gặp
- ""Sai tên đăng nhập hoặc mật khẩu"": Kiểm tra lại chữ hoa/thường, dấu cách.
- ""Lỗi kết nối cơ sở dữ liệu"": Kiểm tra cấu hình chuỗi kết nối trong App.config / FrmCaiDat.

3. Đăng xuất
- Tại màn hình chính, nhấn nút [Đăng xuất] bên sidebar.
- Hệ thống đóng FrmMain và quay lại màn hình đăng nhập (FrmLogin)."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "doctors",
                Title = "Quản lý Bác sĩ",
                Keywords = new[] { "bác sĩ", "doctor", "user", "tài khoản" },
                Content =
@"Chức năng: Thêm / sửa / xóa thông tin bác sĩ.

Các bước sử dụng:
1. Mở menu [Bác sĩ] ở sidebar.
2. Thêm bác sĩ:
   - Nhập họ tên, địa chỉ, điện thoại, username, password...
   - Nhấn [Thêm] để lưu vào CSDL.
3. Sửa thông tin:
   - Chọn một bác sĩ trên lưới.
   - Chỉnh sửa thông tin và nhấn [Cập nhật].
4. Xóa:
   - Chọn bác sĩ và nhấn [Xóa].
   - Lưu ý: Không nên xóa bác sĩ đang còn lịch hẹn / đơn thuốc đang hoạt động."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "patients",
                Title = "Quản lý Bệnh nhân",
                Keywords = new[] { "bệnh nhân", "patient", "khám", "giường", "thường" },
                Content =
@"Chức năng: Quản lý danh sách bệnh nhân (BN thường / BN nằm giường).

Các bước:
1. Mở menu [Bệnh nhân] ở sidebar.
2. Thêm bệnh nhân:
   - Nhập họ tên, CMND/CCCD, địa chỉ, giới tính, ngày khám...
   - Chọn bác sĩ phụ trách.
3. Phân loại:
   - Bệnh nhân thường: Khám xong, về trong ngày.
   - Bệnh nhân nằm giường: Được xếp giường nội trú (liên quan tới form Giường bệnh, Hóa đơn)."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "beds",
                Title = "Quản lý Giường bệnh",
                Keywords = new[] { "giường", "nằm giường", "xếp giường", "nội trú" },
                Content =
@"Chức năng: Quản lý giường bệnh nội trú và xếp bệnh nhân lên giường.

Giao diện gồm:
- Danh sách bệnh nhân chưa xếp giường (trên).
- Thanh lọc theo tình trạng và khoa (giữa).
- Các card giường bệnh hiển thị trạng thái (dưới).

Màu sắc:
- Màu xanh đậm: Giường đang có bệnh nhân.
- Màu xanh nhạt: Giường còn trống.
- Màu cam: Đang sửa / bảo trì.
- Màu đỏ: Giường đã được đặt trước.
- Màu xám: Trạng thái khác.

Thao tác chính:
1. Xếp giường:
   - Chọn bệnh nhân trong danh sách ""Bệnh nhân chưa xếp giường"".
   - Chọn một card giường đang CÒN TRỐNG.
   - Nhấn [Xếp giường].
2. Sửa trạng thái:
   - Chọn card giường.
   - Nhấn [Sửa] để chuyển giữa Còn trống <-> Đang sửa / bảo trì.
3. Xóa giường:
   - Chỉ thực hiện khi giường đang trống, không còn bệnh nhân."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "medicine",
                Title = "Kho Thuốc và Loại thuốc",
                Keywords = new[] { "thuốc", "loại thuốc", "đơn giá", "kho thuốc" },
                Content =
@"Chức năng: Quản lý danh mục thuốc, đơn giá.

Các bước:
1. Mở menu [Kho Thuốc].
2. Thêm loại thuốc:
   - Nhập tên loại, đơn giá mặc định.
   - Nhấn [Thêm] để lưu.
3. Cập nhật:
   - Chọn thuốc, sửa thông tin, nhấn [Cập nhật].
4. Lưu ý:
   - Đơn giá sẽ được gợi ý tự động khi lập Đơn thuốc.
   - Không nên xóa thuốc đang được sử dụng trong các đơn thuốc đã lập."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "prescription",
                Title = "Quản lý Đơn thuốc",
                Keywords = new[] { "đơn thuốc", "thuốc", "tien thuoc", "prescription" },
                Content =
@"Chức năng: Lập và xem đơn thuốc cho Bệnh nhân thường / Bệnh nhân nằm giường (FrmDonThuoc).

1. Chọn loại bệnh nhân:
   - ""Bệnh nhân thường"" hoặc ""Bệnh nhân nằm giường"" trên combobox.
2. Chọn bệnh nhân:
   - Click một dòng bên lưới bên trái để xem / nhập đơn thuốc.
3. Chọn thuốc:
   - Gõ tên thuốc vào combobox tìm kiếm (tự động gợi ý).
   - Đơn giá được gợi ý theo LoaiThuoc, có thể chỉnh lại nếu cần.
4. Thêm đơn:
   - Nhấn [Thêm đơn] để lưu.
5. Liên kết với Hóa đơn:
   - Tiền thuốc sẽ được cộng vào Hóa đơn tương ứng (BN thường hoặc BN nằm giường)."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "invoice",
                Title = "Hóa đơn & Thanh toán",
                Keywords = new[] { "hóa đơn", "thanh toán", "in hóa đơn", "invoice" },
                Content =
@"Chức năng: Lập hóa đơn cho bệnh nhân và thanh toán.

1. Chọn loại bệnh nhân:
   - ""Bệnh nhân nằm giường"": Có tiền giường + thuốc + dịch vụ.
   - ""Bệnh nhân thường"": Có tiền thuốc + dịch vụ.
2. Chọn bệnh nhân trong danh sách bên trái.
3. Kiểm tra:
   - Ngày xếp / Ngày khám.
   - Ngày ra viện / ngày thanh toán.
4. Tiền giường (BN nằm giường):
   - Tính theo số ngày * đơn giá 1 ngày 1 giường.
5. Dịch vụ:
   - Thêm dịch vụ từ danh sách Dịch vụ.
   - Tổng tiền DV được cộng vào thành tiền.
6. Lập hóa đơn:
   - Nhấn [Lập hóa đơn] để:
     + Đánh dấu đơn thuốc là đã thanh toán.
     + Cập nhật giường về trạng thái Còn trống (BN nằm giường).
     + Xóa bệnh nhân khỏi danh sách BenhNhanThuong (BN thường).
7. (Tuỳ chọn nâng cấp)
   - Gửi hóa đơn qua email.
   - In ra file PDF / Excel."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "appointment",
                Title = "Lịch hẹn khám",
                Keywords = new[] { "lịch hẹn", "appointment", "hẹn khám", "calendar" },
                Content =
@"Chức năng: Quản lý lịch hẹn khám cho bác sĩ và bệnh nhân.

Các thao tác chính:
1. Đặt lịch hẹn mới:
   - Chọn bệnh nhân, bác sĩ, ngày giờ, phòng khám.
2. Sửa / hủy lịch:
   - Chọn lịch hẹn trong danh sách, chỉnh sửa hoặc hủy.
3. Lưu ý:
   - Nên tránh trùng giờ khám cho cùng một bác sĩ.
   - Có thể dùng màu sắc / trạng thái để phân biệt lịch đã hoàn thành / bị hủy."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "report",
                Title = "Báo cáo & Thống kê",
                Keywords = new[] { "báo cáo", "thống kê", "report", "doanh thu" },
                Content =
@"Chức năng: Xem biểu đồ và báo cáo doanh thu, số lượng bệnh nhân, dịch vụ sử dụng...

Ví dụ các báo cáo thường dùng:
- Doanh thu theo ngày / tháng / quý.
- Số lượt khám theo bác sĩ.
- Tỷ lệ bệnh nhân nội trú / ngoại trú.
- Top dịch vụ được sử dụng nhiều nhất.

Nâng cấp thêm:
- Xuất báo cáo ra Excel / PDF.
- Lọc theo khoảng thời gian linh hoạt."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "roles",
                Title = "Phân quyền & Tài khoản",
                Keywords = new[] { "phân quyền", "quyền", "role", "user", "tài khoản" },
                Content =
@"Chức năng: Quản lý quyền sử dụng hệ thống (FrmPhanQuyen).

Gợi ý:
- Tài khoản ADMIN:
  + Được truy cập mọi chức năng, đặc biệt là Cài đặt, Phân quyền, Báo cáo.
- Tài khoản Bác sĩ:
  + Được xem và cập nhật thông tin bệnh nhân, đơn thuốc, lịch hẹn.
  + Không được chỉnh sửa cấu hình hệ thống.
- Tài khoản Lễ tân:
  + Được quản lý lịch hẹn, tiếp nhận bệnh nhân, thu ngân.

Lưu ý:
- Khi phân quyền, đảm bảo không khóa hoàn toàn tài khoản ADMIN.
- Nên có ít nhất 1 tài khoản ADMIN dự phòng."
            });

            _allTopics.Add(new HelpTopic
            {
                Id = "settings",
                Title = "Cài đặt hệ thống",
                Keywords = new[] { "cài đặt", "config", "settings", "ngôn ngữ", "theme" },
                Content =
@"Chức năng: Tùy chỉnh hệ thống theo nhu cầu phòng khám (FrmCaiDat).

Các nhóm cài đặt gợi ý:
1. Giao diện:
   - Màu nền, màu nhấn, logo phòng khám.
   - Ngôn ngữ hiển thị (Việt / Anh).
2. Hệ thống:
   - Chuỗi kết nối CSDL (SQL Server).
   - Thông tin tên phòng khám.
3. Email:
   - Cấu hình SMTP server, user, password.
   - Dùng để gửi hóa đơn, lịch hẹn qua email (khi chức năng này được triển khai).
4. Lưu cài đặt:
   - Khi nhấn [Lưu], cấu hình được ghi vào file appsettings.clinic.json (hoặc App.config).
   - Cần khởi động lại form chính để một số cài đặt giao diện có hiệu lực hoàn toàn."
            });
        }

        // =========================================================
        // = ÁP DỤNG TÌM KIẾM
        // =========================================================
        private void ApplyFilter()
        {
            string keyword = _txtSearch.Text.Trim().ToLowerInvariant();

            IEnumerable<HelpTopic> filtered;
            if (string.IsNullOrEmpty(keyword))
            {
                filtered = _allTopics;
            }
            else
            {
                filtered = _allTopics.Where(t =>
                    t.Title.ToLowerInvariant().Contains(keyword) ||
                    (t.Keywords != null && t.Keywords.Any(k => k.ToLowerInvariant().Contains(keyword))));
            }

            _lstTopics.BeginUpdate();
            _lstTopics.Items.Clear();

            foreach (var topic in filtered)
            {
                _lstTopics.Items.Add(topic);
            }

            _lstTopics.EndUpdate();

            if (_lstTopics.Items.Count > 0)
            {
                _lstTopics.SelectedIndex = 0;
            }
            else
            {
                _rtbTopicContent.Clear();
                _rtbTopicContent.Text =
@"Không tìm thấy tính năng phù hợp với từ khóa vừa nhập.

Hãy thử:
- Gõ từ khóa ngắn hơn (ví dụ: ""giường"", ""bác sĩ"", ""hóa đơn"").
- Bỏ dấu tiếng Việt (ví dụ: ""hoa don"", ""benh nhan"").";
            }
        }

        private void LstTopics_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_lstTopics.SelectedItem is HelpTopic topic)
            {
                _rtbTopicContent.Clear();
                _rtbTopicContent.Text = topic.Content;
                _rtbTopicContent.SelectionStart = 0;
                _rtbTopicContent.ScrollToCaret();
            }
        }

        // =========================================================
        // = TAB CẦN CẬP NHẬT / LƯU Ý
        // =========================================================
        private void FillUpdateAndNotes()
        {
            _rtbUpdate.Text =
@"Các tính năng nên cập nhật / hoàn thiện thêm:

1. Gửi hóa đơn qua email
- Thêm nút ""Gửi hóa đơn qua email"" ở FrmHoaDon.
- Cấu hình SMTP ở FrmCaiDat (máy chủ mail, tài khoản, mật khẩu).
- Nội dung email: Thông tin bệnh nhân, tổng tiền, file PDF đính kèm (nếu có).

2. Thanh toán điện tử
- Tích hợp mã QR thanh toán (Momo, ZaloPay, VietQR...).
- Lưu trạng thái 'Đã thanh toán online' trong bảng Hóa đơn.

3. Báo cáo nâng cao
- Xuất báo cáo ra Excel / PDF.
- Lọc theo khoảng thời gian, theo bác sĩ, theo dịch vụ.
- Vẽ biểu đồ chi tiết (doanh thu, số lượt khám, top dịch vụ).

4. Phân quyền chi tiết hơn
- Phân quyền đến từng chức năng (chỉ xem / thêm / sửa / xóa).
- Ghi log hoạt động của người dùng (ai sửa gì, lúc nào).

5. Hỗ trợ đa ngôn ngữ đầy đủ
- Không chỉ đổi mỗi tiêu đề, mà toàn bộ label / message box / báo cáo.
- Lưu ngôn ngữ ưa thích vào cài đặt và áp dụng khi khởi động.";

            _rtbNotes.Text =
@"Một số lưu ý khi sử dụng và sửa mã nguồn dự án:

1. Sao lưu dữ liệu trước khi nâng cấp
- Trước khi chỉnh sửa bảng CSDL hoặc logic thanh toán, hãy sao lưu database.
- Nên có file script .sql để tạo / cập nhật cấu trúc CSDL.

2. Chuỗi kết nối CSDL
- Đảm bảo chuỗi kết nối SQL Server đúng trong App.config / FrmCaiDat.
- Nếu chạy trên nhiều máy: xem lại tên server, instance, quyền truy cập.

3. Phân quyền người dùng
- Chỉ tài khoản ADMIN mới được truy cập chức năng Cài đặt, Phân quyền.
- Không khóa nhầm tất cả tài khoản ADMIN, tránh bị 'kẹt ngoài hệ thống'.

4. Giao diện
- Hạn chế kéo-thả bằng Designer sau khi đã code tay nhiều form.
- Nếu cần sửa Layout, nên sửa đồng bộ cả code (InitializeComponent / BuildLayout).

5. Lỗi runtime
- Khi gặp lỗi, hãy đọc kỹ thông báo (exception) và stack trace.
- Ghi lại các bước dẫn đến lỗi để dễ debug và sửa sau.

6. Hướng dẫn cho người dùng cuối
- Nên in / xuất file PDF hướng dẫn đơn giản về: đăng nhập, lập đơn thuốc, lập hóa đơn.
- Màn hình Trợ giúp (form này) là nơi tập trung mô tả chi tiết cho nhân viên.";
        }

        // =========================================================
        // = TAB LIÊN HỆ
        // =========================================================
        private void BuildLienHeTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.White
            };
            _tabLienHe.Controls.Add(panel);

            var lblIntro = new Label
            {
                Text = "Thông tin liên hệ hỗ trợ & người phát hành",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32),
                Location = new Point(0, 0)
            };
            panel.Controls.Add(lblIntro);

            var lblDesc = new Label
            {
                Text =
@"Khi cần chỉnh sửa, cập nhật tính năng hoặc báo lỗi,
vui lòng liên hệ theo thông tin dưới đây.",
                AutoSize = false,
                Width = panel.Width - 32,
                Height = 44,
                Location = new Point(0, 26),
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(lblDesc);

            int y = 80;

            var lblDevTitle = new Label
            {
                Text = "Người phát triển / Nhà phát hành:",
                AutoSize = true,
                Location = new Point(0, y)
            };
            panel.Controls.Add(lblDevTitle);

            var lblDevName = new Label
            {
                Text = "Nhóm phát triển phần mềm Quản Lý Phòng Khám",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Location = new Point(0, y + 22)
            };
            panel.Controls.Add(lblDevName);

            y += 60;

            var lblEmail = new Label
            {
                Text = "Email hỗ trợ:",
                AutoSize = true,
                Location = new Point(0, y)
            };
            panel.Controls.Add(lblEmail);

            _lblEmailValue = new Label
            {
                Text = "ducbinh.clinic.support@gmail.com",
                AutoSize = true,
                ForeColor = Color.FromArgb(25, 118, 210),
                Location = new Point(110, y)
            };
            panel.Controls.Add(_lblEmailValue);

            _btnCopyEmail = new Button
            {
                Text = "Copy",
                Width = 60,
                Height = 24,
                Location = new Point(110 + _lblEmailValue.Width + 10, y - 3),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 230, 201),
                ForeColor = Color.FromArgb(27, 94, 32)
            };
            _btnCopyEmail.FlatAppearance.BorderSize = 0;
            _btnCopyEmail.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(_lblEmailValue.Text);
                    MessageBox.Show("Đã copy email hỗ trợ vào clipboard.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    // ignore
                }
            };
            panel.Controls.Add(_btnCopyEmail);

            y += 40;

            var lblPhone = new Label
            {
                Text = "Số điện thoại:",
                AutoSize = true,
                Location = new Point(0, y)
            };
            panel.Controls.Add(lblPhone);

            _lblPhoneValue = new Label
            {
                Text = "0907927415",
                AutoSize = true,
                ForeColor = Color.Black,
                Location = new Point(110, y)
            };
            panel.Controls.Add(_lblPhoneValue);

            _btnCopyPhone = new Button
            {
                Text = "Copy",
                Width = 60,
                Height = 24,
                Location = new Point(110 + _lblPhoneValue.Width + 10, y - 3),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 230, 201),
                ForeColor = Color.FromArgb(27, 94, 32)
            };
            _btnCopyPhone.FlatAppearance.BorderSize = 0;
            _btnCopyPhone.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(_lblPhoneValue.Text);
                    MessageBox.Show("Đã copy số điện thoại vào clipboard.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    // ignore
                }
            };
            panel.Controls.Add(_btnCopyPhone);

            y += 50;

            var lblNote = new Label
            {
                Text =
@"Gợi ý: khi gửi email hỗ trợ, nên đính kèm:
- Ảnh chụp màn hình lỗi (nếu có).
- Mô tả thao tác dẫn đến lỗi.
- Phiên bản chương trình, ngày cập nhật gần nhất.",
                AutoSize = false,
                Width = panel.Width - 32,
                Height = 100,
                Location = new Point(0, y),
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(lblNote);
        }
    }
}
