using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace QuanLyPhongKham.Data
{
    public static class EmailHelper
    {
        public static void SendClinicMail(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email người nhận không hợp lệ.", nameof(toEmail));

            var host = ConfigurationManager.AppSettings["SmtpHost"];
            var portStr = ConfigurationManager.AppSettings["SmtpPort"] ?? "587";
            var user = ConfigurationManager.AppSettings["SmtpUser"];
            var pass = ConfigurationManager.AppSettings["SmtpPass"];
            var enableSslStr = ConfigurationManager.AppSettings["SmtpEnableSsl"] ?? "true";
            var fromEmail = ConfigurationManager.AppSettings["SmtpFrom"] ?? user;
            var displayName = ConfigurationManager.AppSettings["ClinicDisplayName"] ?? "Phòng khám Đa khoa";

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("Chưa cấu hình đầy đủ thông tin SMTP trong App.config.");
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;
            bool enableSsl = bool.TryParse(enableSslStr, out var ssl) ? ssl : true;

            var from = new MailAddress(fromEmail, displayName);
            var to = new MailAddress(toEmail);

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(user, pass);

                using (var msg = new MailMessage(from, to))
                {
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = false; 

                    client.Send(msg);
                }
            }
        }
    }
}
