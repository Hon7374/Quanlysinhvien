using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;
using StudentManagement.Models;

namespace StudentManagement.Forms
{
    public partial class SettingsForm : Form
    {
        private int currentUserId;
        private Panel panelProfile;
        private Panel panelPassword;
        private TextBox txtFullName, txtEmail, txtPhone;
        private TextBox txtCurrentPassword, txtNewPassword, txtConfirmPassword;
        private Label lblUsername, lblRole;

        public SettingsForm()
        {
            currentUserId = SessionManager.CurrentUser.UserId;
            InitializeComponent();
            LoadUserProfile();
        }

        private void InitializeComponent()
        {
            this.Text = "Cài đặt - Settings";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.AutoScroll = true;

            // Header
            Panel panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            Label lblTitle = new Label
            {
                Text = "Cài đặt",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 30),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);
            this.Controls.Add(panelHeader);

            // Main content area
            Panel mainPanel = new Panel
            {
                Location = new Point(30, 120),
                Size = new Size(1320, 750),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            // Profile Section
            panelProfile = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(640, 550),
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            Label lblProfileTitle = new Label
            {
                Text = "Thông tin cá nhân",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelProfile.Controls.Add(lblProfileTitle);

            Label lblProfileDesc = new Label
            {
                Text = "Cập nhật thông tin tài khoản của bạn",
                Font = new Font("Segoe UI", 9),
                Location = new Point(30, 50),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelProfile.Controls.Add(lblProfileDesc);

            int yPos = 90;

            // Username (Read-only)
            AddLabel(panelProfile, "Tên đăng nhập", 30, yPos);
            lblUsername = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BackColor = Color.FromArgb(243, 244, 246),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelProfile.Controls.Add(lblUsername);
            yPos += 70;

            // Full Name
            AddLabel(panelProfile, "Họ và tên", 30, yPos);
            txtFullName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelProfile.Controls.Add(txtFullName);
            yPos += 70;

            // Email
            AddLabel(panelProfile, "Email", 30, yPos);
            txtEmail = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelProfile.Controls.Add(txtEmail);
            yPos += 70;

            // Phone
            AddLabel(panelProfile, "Số điện thoại", 30, yPos);
            txtPhone = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelProfile.Controls.Add(txtPhone);
            yPos += 70;

            // Role (Read-only)
            AddLabel(panelProfile, "Vai trò", 30, yPos);
            lblRole = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BackColor = Color.FromArgb(243, 244, 246),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelProfile.Controls.Add(lblRole);
            yPos += 70;

            // Save Profile Button
            Button btnSaveProfile = new Button
            {
                Text = "Lưu thông tin",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(470, yPos),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveProfile.FlatAppearance.BorderSize = 0;
            btnSaveProfile.Click += BtnSaveProfile_Click;
            panelProfile.Controls.Add(btnSaveProfile);

            mainPanel.Controls.Add(panelProfile);

            // Password Section
            panelPassword = new Panel
            {
                Location = new Point(680, 0),
                Size = new Size(640, 550),
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            Label lblPasswordTitle = new Label
            {
                Text = "Đổi mật khẩu",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelPassword.Controls.Add(lblPasswordTitle);

            Label lblPasswordDesc = new Label
            {
                Text = "Cập nhật mật khẩu của bạn để bảo mật tài khoản",
                Font = new Font("Segoe UI", 9),
                Location = new Point(30, 50),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelPassword.Controls.Add(lblPasswordDesc);

            yPos = 90;

            // Current Password
            AddLabel(panelPassword, "Mật khẩu hiện tại", 30, yPos);
            txtCurrentPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            panelPassword.Controls.Add(txtCurrentPassword);
            yPos += 70;

            // New Password
            AddLabel(panelPassword, "Mật khẩu mới", 30, yPos);
            txtNewPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            panelPassword.Controls.Add(txtNewPassword);
            yPos += 70;

            // Confirm Password
            AddLabel(panelPassword, "Nhập lại mật khẩu mới", 30, yPos);
            txtConfirmPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            panelPassword.Controls.Add(txtConfirmPassword);
            yPos += 90;

            // Change Password Button
            Button btnChangePassword = new Button
            {
                Text = "Đổi mật khẩu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(470, yPos),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.Click += BtnChangePassword_Click;
            panelPassword.Controls.Add(btnChangePassword);

            mainPanel.Controls.Add(panelPassword);

            this.Controls.Add(mainPanel);
        }

        private void AddLabel(Panel panel, string text, int x, int y)
        {
            Label label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            panel.Controls.Add(label);
        }

        private void LoadUserProfile()
        {
            try
            {
                string query = "SELECT Username, FullName, Email, Phone, Role FROM Users WHERE UserId = @UserId";
                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@UserId", currentUserId) });

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    lblUsername.Text = row["Username"].ToString();
                    txtFullName.Text = row["FullName"].ToString();
                    txtEmail.Text = row["Email"].ToString();
                    txtPhone.Text = row["Phone"].ToString();

                    UserRole role = (UserRole)Convert.ToInt32(row["Role"]);
                    lblRole.Text = role == UserRole.Admin ? "Quản trị viên" :
                                   role == UserRole.Teacher ? "Giảng viên" : "Sinh viên";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFullName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Vui lòng nhập email!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                // Check if email already exists (excluding current user)
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserId != @UserId";
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery,
                    new SqlParameter[] {
                        new SqlParameter("@Email", txtEmail.Text.Trim()),
                        new SqlParameter("@UserId", currentUserId)
                    }));

                if (count > 0)
                {
                    MessageBox.Show("Email đã tồn tại! Vui lòng sử dụng email khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                string updateQuery = @"UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone
                    WHERE UserId = @UserId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@UserId", currentUserId)
                };

                DatabaseHelper.ExecuteNonQuery(updateQuery, parameters);

                MessageBox.Show("Cập nhật thông tin thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCurrentPassword.Text))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu hiện tại!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCurrentPassword.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu mới!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNewPassword.Focus();
                    return;
                }

                if (txtNewPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }

                // Verify current password
                string checkQuery = "SELECT PasswordHash FROM Users WHERE UserId = @UserId";
                DataTable dt = DatabaseHelper.ExecuteQuery(checkQuery,
                    new SqlParameter[] { new SqlParameter("@UserId", currentUserId) });

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy thông tin người dùng!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string currentHash = dt.Rows[0]["PasswordHash"].ToString();
                if (!PasswordHelper.VerifyPassword(txtCurrentPassword.Text, currentHash))
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCurrentPassword.Focus();
                    txtCurrentPassword.SelectAll();
                    return;
                }

                // Update password
                string newHash = PasswordHelper.HashPassword(txtNewPassword.Text);
                string updateQuery = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId";

                DatabaseHelper.ExecuteNonQuery(updateQuery, new SqlParameter[]
                {
                    new SqlParameter("@PasswordHash", newHash),
                    new SqlParameter("@UserId", currentUserId)
                });

                MessageBox.Show("Đổi mật khẩu thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear password fields
                txtCurrentPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đổi mật khẩu: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
