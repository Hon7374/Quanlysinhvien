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
    public partial class UserEditForm : Form
    {
        private int userId;
        private UserRole currentRole;

        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtUsername;
        private ComboBox cboRole;
        private ComboBox cboStatus;
        private Panel panelPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private CheckBox chkChangePassword;
        private Button btnSave;
        private Button btnCancel;
        private Panel panelRoleSpecific;
        private TextBox txtCode;
        private TextBox txtDepartment;
        private TextBox txtDegree;
        private TextBox txtSpecialization;
        private TextBox txtClass;
        private TextBox txtMajor;
        private PictureBox picAvatar;

        public UserEditForm(int userId)
        {
            this.userId = userId;
            InitializeComponent();
            LoadUserData();
        }

        private void InitializeComponent()
        {
            this.Text = "Chỉnh sửa người dùng - Edit User";
            this.Size = new Size(700, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Title
            Label lblTitle = new Label
            {
                Text = "CHỈNH SỬA THÔNG TIN NGƯỜI DÙNG",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(99, 102, 241)
            };
            this.Controls.Add(lblTitle);

            int yPos = 70;

            // Avatar section
            Panel panelAvatar = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 100),
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle
            };

            picAvatar = new PictureBox
            {
                Location = new Point(15, 15),
                Size = new Size(70, 70),
                BackColor = Color.FromArgb(196, 181, 253),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            panelAvatar.Controls.Add(picAvatar);

            Button btnUploadAvatar = new Button
            {
                Text = "Tải ảnh lên",
                Font = new Font("Segoe UI", 9),
                Location = new Point(100, 30),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnUploadAvatar.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnUploadAvatar.Click += BtnUploadAvatar_Click;
            panelAvatar.Controls.Add(btnUploadAvatar);

            Label lblAvatarNote = new Label
            {
                Text = "Định dạng: JPG, PNG. Kích thước tối đa: 2MB",
                Font = new Font("Segoe UI", 8),
                Location = new Point(210, 35),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            panelAvatar.Controls.Add(lblAvatarNote);

            this.Controls.Add(panelAvatar);
            yPos += 120;

            // User Info Section
            Label lblUserInfo = new Label
            {
                Text = "THÔNG TIN CƠ BẢN",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lblUserInfo);
            yPos += 35;

            // Full Name
            AddLabel("Họ và tên *", 20, yPos);
            txtFullName = AddTextBox(20, yPos + 25, 300);

            // Email
            AddLabel("Email *", 350, yPos);
            txtEmail = AddTextBox(350, yPos + 25, 300);
            yPos += 70;

            // Phone
            AddLabel("Số điện thoại", 20, yPos);
            txtPhone = AddTextBox(20, yPos + 25, 300);

            // Username (read-only)
            AddLabel("Tên đăng nhập", 350, yPos);
            txtUsername = AddTextBox(350, yPos + 25, 300);
            txtUsername.ReadOnly = true;
            txtUsername.BackColor = Color.FromArgb(243, 244, 246);
            yPos += 70;

            // Account Details Section
            Label lblAccount = new Label
            {
                Text = "CHI TIẾT TÀI KHOẢN",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lblAccount);
            yPos += 35;

            // Role
            AddLabel("Vai trò *", 20, yPos);
            cboRole = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, yPos + 25),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRole.Items.Add(new ComboBoxItem("Admin", UserRole.Admin));
            cboRole.Items.Add(new ComboBoxItem("Giảng viên", UserRole.Teacher));
            cboRole.Items.Add(new ComboBoxItem("Sinh viên", UserRole.Student));
            cboRole.SelectedIndexChanged += CboRole_SelectedIndexChanged;
            this.Controls.Add(cboRole);

            // Status
            AddLabel("Trạng thái *", 350, yPos);
            cboStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(350, yPos + 25),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.Add(new ComboBoxItem("Hoạt động", true));
            cboStatus.Items.Add(new ComboBoxItem("Không hoạt động", false));
            this.Controls.Add(cboStatus);
            yPos += 70;

            // Role-specific panel
            panelRoleSpecific = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 120),
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelRoleSpecific);
            yPos += 135;

            // Password section
            Label lblPassword = new Label
            {
                Text = "MẬT KHẨU",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lblPassword);
            yPos += 35;

            chkChangePassword = new CheckBox
            {
                Text = "Đổi mật khẩu",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            chkChangePassword.CheckedChanged += ChkChangePassword_CheckedChanged;
            this.Controls.Add(chkChangePassword);
            yPos += 30;

            panelPassword = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 100),
                Visible = false
            };

            // Mật khẩu mới
            Label lblNewPassword = new Label
            {
                Text = "Mật khẩu mới:",
                Font = new Font("Segoe UI", 9),
                Location = new Point(0, 8),
                AutoSize = true
            };
            panelPassword.Controls.Add(lblNewPassword);

            txtNewPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(140, 5),
                Size = new Size(300, 30),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelPassword.Controls.Add(txtNewPassword);

            // Nhập lại mật khẩu
            Label lblConfirmPassword = new Label
            {
                Text = "Nhập lại mật khẩu:",
                Font = new Font("Segoe UI", 9),
                Location = new Point(0, 48),
                AutoSize = true
            };
            panelPassword.Controls.Add(lblConfirmPassword);

            txtConfirmPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(140, 45),
                Size = new Size(300, 30),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelPassword.Controls.Add(txtConfirmPassword);

            this.Controls.Add(panelPassword);
            yPos += 110;

            // Buttons
            btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10),
                Location = new Point(430, yPos),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = "Lưu thay đổi",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(540, yPos),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void LoadUserData()
        {
            try
            {
                string query = @"SELECT u.Username, u.FullName, u.Email, u.Phone, u.Role, u.IsActive
                                FROM Users u
                                WHERE u.UserId = @UserId";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@UserId", userId) });

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy người dùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                DataRow row = dt.Rows[0];

                txtUsername.Text = row["Username"].ToString();
                txtFullName.Text = row["FullName"].ToString();
                txtEmail.Text = row["Email"].ToString();
                txtPhone.Text = row["Phone"].ToString();

                currentRole = (UserRole)Convert.ToInt32(row["Role"]);
                bool isActive = Convert.ToBoolean(row["IsActive"]);

                // Set role
                foreach (ComboBoxItem item in cboRole.Items)
                {
                    if (item.Value is UserRole && (UserRole)item.Value == currentRole)
                    {
                        cboRole.SelectedItem = item;
                        break;
                    }
                }

                // Set status
                foreach (ComboBoxItem item in cboStatus.Items)
                {
                    if ((bool)item.Value == isActive)
                    {
                        cboStatus.SelectedItem = item;
                        break;
                    }
                }

                LoadRoleSpecificData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRoleSpecificData()
        {
            panelRoleSpecific.Controls.Clear();

            int y = 15;

            switch (currentRole)
            {
                case UserRole.Teacher:
                    try
                    {
                        string query = "SELECT TeacherCode, Department, Degree, Specialization FROM Teachers WHERE UserId = @UserId";
                        DataTable dt = DatabaseHelper.ExecuteQuery(query,
                            new SqlParameter[] { new SqlParameter("@UserId", userId) });

                        Label lblTeacher = new Label
                        {
                            Text = "THÔNG TIN GIẢNG VIÊN",
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            Location = new Point(10, y),
                            AutoSize = true
                        };
                        panelRoleSpecific.Controls.Add(lblTeacher);
                        y += 30;

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "Mã GV:",
                                Location = new Point(10, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            txtCode = new TextBox
                            {
                                Text = row["TeacherCode"].ToString(),
                                Location = new Point(90, y - 2),
                                Size = new Size(120, 25),
                                Font = new Font("Segoe UI", 9)
                            };
                            panelRoleSpecific.Controls.Add(txtCode);

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "Khoa:",
                                Location = new Point(230, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            txtDepartment = new TextBox
                            {
                                Text = row["Department"].ToString(),
                                Location = new Point(280, y - 2),
                                Size = new Size(160, 25),
                                Font = new Font("Segoe UI", 9)
                            };
                            panelRoleSpecific.Controls.Add(txtDepartment);

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "Học vị:",
                                Location = new Point(460, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            txtDegree = new TextBox
                            {
                                Text = row["Degree"].ToString(),
                                Location = new Point(520, y - 2),
                                Size = new Size(100, 25),
                                Font = new Font("Segoe UI", 9)
                            };
                            panelRoleSpecific.Controls.Add(txtDegree);
                            y += 35;

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "Chuyên môn:",
                                Location = new Point(10, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            txtSpecialization = new TextBox
                            {
                                Text = row["Specialization"].ToString(),
                                Location = new Point(100, y - 2),
                                Size = new Size(520, 25),
                                Font = new Font("Segoe UI", 9)
                            };
                            panelRoleSpecific.Controls.Add(txtSpecialization);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi tải thông tin giảng viên: {ex.Message}");
                    }
                    break;

                case UserRole.Student:
                    try
                    {
                        string query = "SELECT StudentCode, Class, Major, GPA FROM Students WHERE UserId = @UserId";
                        DataTable dt = DatabaseHelper.ExecuteQuery(query,
                            new SqlParameter[] { new SqlParameter("@UserId", userId) });

                        Label lblStudent = new Label
                        {
                            Text = "THÔNG TIN SINH VIÊN",
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            Location = new Point(10, y),
                            AutoSize = true
                        };
                        panelRoleSpecific.Controls.Add(lblStudent);
                        y += 30;

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "Mã SV:",
                                Location = new Point(10, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            txtCode = new TextBox
                            {
                                Text = row["StudentCode"].ToString(),
                                Location = new Point(80, y - 2),
                                Size = new Size(120, 25),
                                Font = new Font("Segoe UI", 9)
                            };
                            panelRoleSpecific.Controls.Add(txtCode);

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "Lớp:",
                                Location = new Point(220, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            txtClass = new TextBox
                            {
                                Text = row["Class"].ToString(),
                                Location = new Point(260, y - 2),
                                Size = new Size(150, 25),
                                Font = new Font("Segoe UI", 9)
                            };
                            panelRoleSpecific.Controls.Add(txtClass);

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "GPA:",
                                Location = new Point(430, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            Label lblGPA = new Label
                            {
                                Text = row["GPA"].ToString(),
                                Location = new Point(480, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                                ForeColor = Color.FromArgb(99, 102, 241)
                            };
                            panelRoleSpecific.Controls.Add(lblGPA);
                            y += 35;

                            panelRoleSpecific.Controls.Add(new Label
                            {
                                Text = "Ngành:",
                                Location = new Point(10, y),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9)
                            });
                            txtMajor = new TextBox
                            {
                                Text = row["Major"].ToString(),
                                Location = new Point(80, y - 2),
                                Size = new Size(540, 25),
                                Font = new Font("Segoe UI", 9)
                            };
                            panelRoleSpecific.Controls.Add(txtMajor);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi tải thông tin sinh viên: {ex.Message}");
                    }
                    break;

                case UserRole.Admin:
                    Label lblAdmin = new Label
                    {
                        Text = "THÔNG TIN QUẢN TRỊ VIÊN",
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        Location = new Point(10, y),
                        AutoSize = true
                    };
                    panelRoleSpecific.Controls.Add(lblAdmin);
                    y += 30;

                    Label lblNote = new Label
                    {
                        Text = "Tài khoản quản trị viên có toàn quyền truy cập hệ thống.",
                        Font = new Font("Segoe UI", 9, FontStyle.Italic),
                        Location = new Point(10, y),
                        Size = new Size(600, 40),
                        ForeColor = Color.Gray
                    };
                    panelRoleSpecific.Controls.Add(lblNote);
                    break;
            }
        }

        private void CboRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboRole.SelectedItem != null)
            {
                UserRole newRole = (UserRole)((ComboBoxItem)cboRole.SelectedItem).Value;
                if (newRole != currentRole)
                {
                    MessageBox.Show("Lưu ý: Thay đổi vai trò sẽ ảnh hưởng đến quyền truy cập của người dùng!",
                        "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ChkChangePassword_CheckedChanged(object sender, EventArgs e)
        {
            panelPassword.Visible = chkChangePassword.Checked;
            if (!chkChangePassword.Checked)
            {
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";
            }
        }

        private void BtnUploadAvatar_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Chọn ảnh đại diện"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    picAvatar.Image = Image.FromFile(openFileDialog.FileName);
                    MessageBox.Show("Ảnh đã được tải lên! Nhấn 'Lưu thay đổi' để lưu.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập email!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (chkChangePassword.Checked && string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (chkChangePassword.Checked && string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập lại mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            if (chkChangePassword.Checked && txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                txtConfirmPassword.SelectAll();
                return;
            }

            try
            {
                UserRole newRole = (UserRole)((ComboBoxItem)cboRole.SelectedItem).Value;
                bool isActive = (bool)((ComboBoxItem)cboStatus.SelectedItem).Value;

                // Update User
                string updateUserQuery = @"UPDATE Users
                                          SET FullName = @FullName,
                                              Email = @Email,
                                              Phone = @Phone,
                                              Role = @Role,
                                              IsActive = @IsActive
                                          WHERE UserId = @UserId";

                SqlParameter[] userParams = new SqlParameter[]
                {
                    new SqlParameter("@FullName", txtFullName.Text),
                    new SqlParameter("@Email", txtEmail.Text),
                    new SqlParameter("@Phone", string.IsNullOrWhiteSpace(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text),
                    new SqlParameter("@Role", (int)newRole),
                    new SqlParameter("@IsActive", isActive),
                    new SqlParameter("@UserId", userId)
                };

                DatabaseHelper.ExecuteNonQuery(updateUserQuery, userParams);

                // Update password if changed
                if (chkChangePassword.Checked && !string.IsNullOrWhiteSpace(txtNewPassword.Text))
                {
                    string hashedPassword = PasswordHelper.HashPassword(txtNewPassword.Text);
                    string updatePasswordQuery = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId";

                    SqlParameter[] passwordParams = new SqlParameter[]
                    {
                        new SqlParameter("@PasswordHash", hashedPassword),
                        new SqlParameter("@UserId", userId)
                    };

                    DatabaseHelper.ExecuteNonQuery(updatePasswordQuery, passwordParams);
                }

                // Update role-specific data
                if (newRole == UserRole.Teacher && txtCode != null)
                {
                    string updateTeacherQuery = @"UPDATE Teachers
                                                 SET TeacherCode = @TeacherCode,
                                                     Department = @Department,
                                                     Degree = @Degree,
                                                     Specialization = @Specialization
                                                 WHERE UserId = @UserId";

                    SqlParameter[] teacherParams = new SqlParameter[]
                    {
                        new SqlParameter("@TeacherCode", txtCode.Text),
                        new SqlParameter("@Department", string.IsNullOrWhiteSpace(txtDepartment?.Text) ? (object)DBNull.Value : txtDepartment.Text),
                        new SqlParameter("@Degree", string.IsNullOrWhiteSpace(txtDegree?.Text) ? (object)DBNull.Value : txtDegree.Text),
                        new SqlParameter("@Specialization", string.IsNullOrWhiteSpace(txtSpecialization?.Text) ? (object)DBNull.Value : txtSpecialization.Text),
                        new SqlParameter("@UserId", userId)
                    };

                    DatabaseHelper.ExecuteNonQuery(updateTeacherQuery, teacherParams);
                }
                else if (newRole == UserRole.Student && txtCode != null)
                {
                    string updateStudentQuery = @"UPDATE Students
                                                 SET StudentCode = @StudentCode,
                                                     Class = @Class,
                                                     Major = @Major
                                                 WHERE UserId = @UserId";

                    SqlParameter[] studentParams = new SqlParameter[]
                    {
                        new SqlParameter("@StudentCode", txtCode.Text),
                        new SqlParameter("@Class", string.IsNullOrWhiteSpace(txtClass?.Text) ? (object)DBNull.Value : txtClass.Text),
                        new SqlParameter("@Major", string.IsNullOrWhiteSpace(txtMajor?.Text) ? (object)DBNull.Value : txtMajor.Text),
                        new SqlParameter("@UserId", userId)
                    };

                    DatabaseHelper.ExecuteNonQuery(updateStudentQuery, studentParams);
                }

                MessageBox.Show("Cập nhật thông tin thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Label AddLabel(string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lbl);
            return lbl;
        }

        private TextBox AddTextBox(int x, int y, int width)
        {
            TextBox txt = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, y),
                Size = new Size(width, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txt);
            return txt;
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public ComboBoxItem(string text, object value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
