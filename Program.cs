using System;
using System.Windows.Forms;
using StudentManagement.Forms;
using StudentManagement.Helpers;

namespace StudentManagement
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new ScheduleManagementForm());

            // Show login form
            LoginForm loginForm = new LoginForm();
            DialogResult loginResult = loginForm.ShowDialog();

            if (loginResult == DialogResult.OK && SessionManager.IsLoggedIn)
            {
                // Open appropriate dashboard based on user role
                Form dashboard = null;

                switch (SessionManager.CurrentUser.Role)
                {
                    case Models.UserRole.Admin:
                        dashboard = new AdminDashboardModern();
                        break;

                    case Models.UserRole.Teacher:
                        dashboard = new TeacherDashboard();
                        break;

                    case Models.UserRole.Student:
                        dashboard = new StudentDashboard();
                        break;

                    default:
                        MessageBox.Show("Vai trò người dùng không hợp lệ!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                if (dashboard != null)
                {
                    Application.Run(dashboard);
                }
            }
        }
    }
}
