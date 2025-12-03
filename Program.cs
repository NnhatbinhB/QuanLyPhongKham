using System;
using System.Windows.Forms;
using QuanLyPhongKham.Data;
using QuanLyPhongKham.Forms;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppSettings.Load();

            using (var login = new FrmLogin())
            {
                var result = login.ShowDialog();

                if (result == DialogResult.OK && login.LoggedInUser != null)
                {
                    Application.Run(new FrmMain(login.LoggedInUser));
                }
                else
                {

                    Application.Exit();
                }
            }
        }
    }
}


