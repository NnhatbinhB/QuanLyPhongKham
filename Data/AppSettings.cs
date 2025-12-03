using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace QuanLyPhongKham.Data
{
    public enum AppTheme
    {
        Green,  // Xanh phòng khám (mặc định)
        Light,  // Nền sáng
        Dark    // Nền tối
    }

    public enum AppLanguage
    {
        Vietnamese,
        English
    }

    public static partial class AppSettings
    {
        private class SettingsDto
        {
            public string Theme { get; set; } = "Green";
            public string Language { get; set; } = "vi";
        }

        private static readonly string ConfigPath =
            Path.Combine(Application.StartupPath, "appsettings.clinic.json");

        public static AppTheme Theme { get; private set; } = AppTheme.Green;
        public static AppLanguage Language { get; private set; } = AppLanguage.Vietnamese;

        public static event Action? SettingsChanged;

        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    // lưu file mặc định lần đầu
                    Save(Theme, Language);
                    return;
                }

                var json = File.ReadAllText(ConfigPath);
                var dto = JsonSerializer.Deserialize<SettingsDto>(json);
                if (dto == null) return;

                Theme = dto.Theme switch
                {
                    "Light" => AppTheme.Light,
                    "Dark" => AppTheme.Dark,
                    "Green" => AppTheme.Green,
                    _ => AppTheme.Green
                };

                Language = dto.Language switch
                {
                    "en" => AppLanguage.English,
                    _ => AppLanguage.Vietnamese
                };
            }
            catch
            {
                // lỗi đọc file thì dùng mặc định, khỏi crash
            }
        }

        public static void Save(AppTheme theme, AppLanguage language)
        {
            Theme = theme;
            Language = language;

            try
            {
                var dto = new SettingsDto
                {
                    Theme = theme.ToString(),
                    Language = language == AppLanguage.English ? "en" : "vi"
                };

                var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // nếu ghi file lỗi thì thôi, vẫn đổi theme trong RAM
            }

            // Báo cho tất cả Form đang nghe biết là setting đã đổi
            SettingsChanged?.Invoke();
        }
    }
}
