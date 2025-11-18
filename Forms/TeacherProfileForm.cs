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
    public partial class TeacherProfileForm : Form
    {
        private int currentUserId;
        private int teacherId;
        private Panel panelProfile;
        private Panel panelPassword;
        private TextBox txtFullName, txtEmail, txtPhone;
        private TextBox txtDepartment, txtDegree, txtSpecialization;
        private TextBox txtCurrentPassword, txtNewPassword, txtConfirmPassword;
        private Label lblUsername, lblTeacherCode, lblHireDate;

        public TeacherProfileForm()
        {
            currentUserId = SessionManager.CurrentUser.UserId;
            teacherId = SessionManager.CurrentTeacher.TeacherId;
            InitializeComponent();
            LoadTeacherProfile();
        }

        private void InitializeComponent()
        {
            this.Text = "Thông tin cá nhân - Teacher Profile";
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
                Text = "Thông tin cá nhân",
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
                Size = new Size(640, 650),
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            Label lblProfileTitle = new Label
            {
                Text = "Thông tin giảng viên",
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

            // Teacher Code (Read-only)
            AddLabel(panelProfile, "Mã giảng viên", 30, yPos);
            lblTeacherCode = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BackColor = Color.FromArgb(243, 244, 246),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelProfile.Controls.Add(lblTeacherCode);
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

            // Department
            AddLabel(panelProfile, "Khoa", 30, yPos);
            txtDepartment = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelProfile.Controls.Add(txtDepartment);
            yPos += 70;

            // Degree
            AddLabel(panelProfile, "Học vị", 30, yPos);
            txtDegree = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(270, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelProfile.Controls.Add(txtDegree);

            // Specialization
            AddLabel(panelProfile, "Chuyên môn", 320, yPos);
            txtSpecialization = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(320, yPos + 25),
                Size = new Size(270, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelProfile.Controls.Add(txtSpecialization);
            yPos += 70;

            // Hire Date (Read-only)
            AddLabel(panelProfile, "Ngày vào làm", 30, yPos);
            lblHireDate = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(560, 35),
                BackColor = Color.FromArgb(243, 244, 246),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelProfile.Controls.Add(lblHireDate);
            yPos += 70;

            // Save Profile Button
            Button btnSaveProfile = new Button
            {
                Text = "💾 Lưu thông tin",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(450, yPos),
                Size = new Size(140, 45),
                BackColor = Color.FromArgb(39, 174, 96),
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
                Size = new Size(640, 650),
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
                Text = "Cập nhật mật khẩu để bảo mật tài khoản",
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

            // Password strength info
            Label lblPasswordInfo = new Label
            {
                Text = "💡 Mật khẩu nên có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và số",
                Font = new Font("Segoe UI", 8),
                Location = new Point(30, yPos),
                Size = new Size(560, 40),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelPassword.Controls.Add(lblPasswordInfo);
            yPos += 50;

            // Change Password Button
            Button btnChangePassword = new Button
            {
                Text = "🔒 Đổi mật khẩu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(440, yPos),
                Size = new Size(150, 45),
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

        private void LoadTeacherProfile()
        {
            try
            {
                string query = @"SELECT u.Username, u.FullName, u.Email, u.Phone,
                                t.TeacherCode, t.Department, t.Degree, t.Specialization, t.HireDate
                                FROM Users u
                                INNER JOIN Teachers t ON u.UserId = t.UserId
                                WHERE u.UserId = @UserId";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@UserId", currentUserId) });

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    lblUsername.Text = row["Username"].ToString();
                    lblTeacherCode.Text = row["TeacherCode"].ToString();
                    txtFullName.Text = row["FullName"].ToString();
                    txtEmail.Text = row["Email"].ToString();
                    txtPhone.Text = row["Phone"].ToString();
                    txtDepartment.Text = row["Department"].ToString();
                    txtDegree.Text = row["Degree"].ToString();
                    txtSpecialization.Text = row["Specialization"].ToString();

                    if (row["HireDate"] != DBNull.Value)
                    {
                        lblHireDate.Text = Convert.ToDateTime(row["HireDate"]).ToString("dd/MM/yyyy");
                    }
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

                // Update Users table
                string updateUserQuery = @"UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone
                    WHERE UserId = @UserId";

                DatabaseHelper.ExecuteNonQuery(updateUserQuery, new SqlParameter[]
                {
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@UserId", currentUserId)
                });

                // Update Teachers table
                string updateTeacherQuery = @"UPDATE Teachers SET
                    Department = @Department,
                    Degree = @Degree,
                    Specialization = @Specialization
                    WHERE TeacherId = @TeacherId";

                DatabaseHelper.ExecuteNonQuery(updateTeacherQuery, new SqlParameter[]
                {
                    new SqlParameter("@Department", txtDepartment.Text.Trim()),
                    new SqlParameter("@Degree", txtDegree.Text.Trim()),
                    new SqlParameter("@Specialization", txtSpecialization.Text.Trim()),
                    new SqlParameter("@TeacherId", teacherId)
                });

                // Update session
                SessionManager.CurrentUser.FullName = txtFullName.Text.Trim();
                SessionManager.CurrentUser.Email = txtEmail.Text.Trim();
                SessionManager.CurrentUser.Phone = txtPhone.Text.Trim();
                SessionManager.CurrentTeacher.Department = txtDepartment.Text.Trim();
                SessionManager.CurrentTeacher.Degree = txtDegree.Text.Trim();
                SessionManager.CurrentTeacher.Specialization = txtSpecialization.Text.Trim();

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

                if (txtNewPassword.Text.Length < 6)
                {
                    MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự!", "Thông báo",
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

                // Verify current password - Check which column exists
                string checkQuery = "SELECT Password FROM Users WHERE UserId = @UserId";
                DataTable dt = DatabaseHelper.ExecuteQuery(checkQuery,
                    new SqlParameter[] { new SqlParameter("@UserId", currentUserId) });

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy thông tin người dùng!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string currentPassword = dt.Rows[0]["Password"].ToString();

                // Simple password comparison (database uses plain text password)
                if (currentPassword != txtCurrentPassword.Text)
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCurrentPassword.Focus();
                    txtCurrentPassword.SelectAll();
                    return;
                }

                // Update password (plain text for now, matching the existing schema)
                string updateQuery = "UPDATE Users SET Password = @Password WHERE UserId = @UserId";

                DatabaseHelper.ExecuteNonQuery(updateQuery, new SqlParameter[]
                {
                    new SqlParameter("@Password", txtNewPassword.Text),
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
