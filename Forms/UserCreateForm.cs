using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;
using StudentManagement.Models;

namespace StudentManagement.Forms
{
    public partial class UserCreateForm : Form
    {
        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtCode; // Student/Teacher code
        private ComboBox cboRole;
        private CheckBox chkAutoPassword;
        private Button btnCreate;
        private Button btnCancel;
        private Panel panelRoleSpecific;
        private TextBox txtDepartment;
        private TextBox txtDegree;
        private TextBox txtSpecialization;
        private TextBox txtClass;
        private TextBox txtMajor;

        public UserCreateForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Tạo người dùng mới - Create New User";
            this.Size = new Size(600, 820);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Title
            Label lblTitle = new Label
            {
                Text = "TẠO TÀI KHOẢN MỚI",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(99, 102, 241)
            };
            this.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Điền thông tin để tạo tài khoản người dùng mới",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 50),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblSubtitle);

            int yPos = 90;

            // Full Name
            AddLabel("Họ và tên *", 20, yPos);
            txtFullName = AddTextBox(20, yPos + 25, 540);
            yPos += 70;

            // Email
            AddLabel("Email *", 20, yPos);
            txtEmail = AddTextBox(20, yPos + 25, 540);
            yPos += 70;

            // Phone
            AddLabel("Số điện thoại", 20, yPos);
            txtPhone = AddTextBox(20, yPos + 25, 540);
            yPos += 70;

            // Username
            AddLabel("Tên đăng nhập *", 20, yPos);
            txtUsername = AddTextBox(20, yPos + 25, 540);
            yPos += 70;

            // Password
            AddLabel("Mật khẩu *", 20, yPos);
            txtPassword = AddTextBox(20, yPos + 25, 540);
            txtPassword.UseSystemPasswordChar = true;
            yPos += 70;

            // Confirm Password
            AddLabel("Nhập lại mật khẩu *", 20, yPos);
            txtConfirmPassword = AddTextBox(20, yPos + 25, 540);
            txtConfirmPassword.UseSystemPasswordChar = true;

            chkAutoPassword = new CheckBox
            {
                Text = "Tự động tạo mật khẩu",
                Font = new Font("Segoe UI", 9),
                Location = new Point(22, yPos + 55),
                AutoSize = true
            };
            chkAutoPassword.CheckedChanged += ChkAutoPassword_CheckedChanged;
            this.Controls.Add(chkAutoPassword);
            yPos += 90;

            // Role
            AddLabel("Vai trò *", 20, yPos);
            cboRole = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, yPos + 25),
                Size = new Size(540, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRole.Items.Add(new ComboBoxItem("Admin", UserRole.Admin));
            cboRole.Items.Add(new ComboBoxItem("Giảng viên", UserRole.Teacher));
            cboRole.Items.Add(new ComboBoxItem("Sinh viên", UserRole.Student));
            cboRole.SelectedIndex = 0;
            cboRole.SelectedIndexChanged += CboRole_SelectedIndexChanged;
            this.Controls.Add(cboRole);
            yPos += 70;

            // Role-specific panel
            panelRoleSpecific = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(540, 150),
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelRoleSpecific);
            yPos += 160;

            // Buttons
            btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10),
                Location = new Point(350, yPos),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            btnCreate = new Button
            {
                Text = "Tạo tài khoản",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(460, yPos),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            LoadRoleSpecificFields();
        }

        private void ChkAutoPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoPassword.Checked)
            {
                string password = GenerateRandomPassword();
                txtPassword.Text = password;
                txtConfirmPassword.Text = password;
                txtPassword.ReadOnly = true;
                txtConfirmPassword.ReadOnly = true;
            }
            else
            {
                txtPassword.Text = "";
                txtConfirmPassword.Text = "";
                txtPassword.ReadOnly = false;
                txtConfirmPassword.ReadOnly = false;
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            var password = new char[12];
            for (int i = 0; i < 12; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            return new string(password);
        }

        private void CboRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRoleSpecificFields();
        }

        private void LoadRoleSpecificFields()
        {
            panelRoleSpecific.Controls.Clear();

            if (cboRole.SelectedItem == null) return;

            UserRole role = ((ComboBoxItem)cboRole.SelectedItem).Value;
            int y = 15;

            switch (role)
            {
                case UserRole.Teacher:
                    Label lblTeacher = new Label
                    {
                        Text = "THÔNG TIN GIẢNG VIÊN",
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        Location = new Point(10, y),
                        AutoSize = true
                    };
                    panelRoleSpecific.Controls.Add(lblTeacher);
                    y += 30;

                    panelRoleSpecific.Controls.Add(new Label
                    {
                        Text = "Mã giảng viên:",
                        Location = new Point(10, y),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9)
                    });
                    txtCode = new TextBox
                    {
                        Location = new Point(120, y - 2),
                        Size = new Size(150, 25),
                        Font = new Font("Segoe UI", 9)
                    };
                    panelRoleSpecific.Controls.Add(txtCode);

                    panelRoleSpecific.Controls.Add(new Label
                    {
                        Text = "Khoa:",
                        Location = new Point(290, y),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9)
                    });
                    txtDepartment = new TextBox
                    {
                        Location = new Point(350, y - 2),
                        Size = new Size(160, 25),
                        Font = new Font("Segoe UI", 9)
                    };
                    panelRoleSpecific.Controls.Add(txtDepartment);
                    y += 35;

                    panelRoleSpecific.Controls.Add(new Label
                    {
                        Text = "Học vị:",
                        Location = new Point(10, y),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9)
                    });
                    txtDegree = new TextBox
                    {
                        Location = new Point(120, y - 2),
                        Size = new Size(150, 25),
                        Font = new Font("Segoe UI", 9)
                    };
                    panelRoleSpecific.Controls.Add(txtDegree);

                    panelRoleSpecific.Controls.Add(new Label
                    {
                        Text = "Chuyên môn:",
                        Location = new Point(290, y),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9)
                    });
                    txtSpecialization = new TextBox
                    {
                        Location = new Point(380, y - 2),
                        Size = new Size(130, 25),
                        Font = new Font("Segoe UI", 9)
                    };
                    panelRoleSpecific.Controls.Add(txtSpecialization);
                    break;

                case UserRole.Student:
                    Label lblStudent = new Label
                    {
                        Text = "THÔNG TIN SINH VIÊN",
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        Location = new Point(10, y),
                        AutoSize = true
                    };
                    panelRoleSpecific.Controls.Add(lblStudent);
                    y += 30;

                    panelRoleSpecific.Controls.Add(new Label
                    {
                        Text = "Mã sinh viên:",
                        Location = new Point(10, y),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9)
                    });
                    txtCode = new TextBox
                    {
                        Location = new Point(110, y - 2),
                        Size = new Size(150, 25),
                        Font = new Font("Segoe UI", 9)
                    };
                    panelRoleSpecific.Controls.Add(txtCode);

                    panelRoleSpecific.Controls.Add(new Label
                    {
                        Text = "Lớp:",
                        Location = new Point(280, y),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9)
                    });
                    txtClass = new TextBox
                    {
                        Location = new Point(320, y - 2),
                        Size = new Size(190, 25),
                        Font = new Font("Segoe UI", 9)
                    };
                    panelRoleSpecific.Controls.Add(txtClass);
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
                        Location = new Point(110, y - 2),
                        Size = new Size(400, 25),
                        Font = new Font("Segoe UI", 9)
                    };
                    panelRoleSpecific.Controls.Add(txtMajor);
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
                        Size = new Size(500, 40),
                        ForeColor = Color.Gray
                    };
                    panelRoleSpecific.Controls.Add(lblNote);
                    break;
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
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

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập lại mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                txtConfirmPassword.SelectAll();
                return;
            }

            UserRole role = ((ComboBoxItem)cboRole.SelectedItem).Value;

            if (role == UserRole.Student || role == UserRole.Teacher)
            {
                if (string.IsNullOrWhiteSpace(txtCode?.Text))
                {
                    MessageBox.Show($"Vui lòng nhập mã {(role == UserRole.Student ? "sinh viên" : "giảng viên")}!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCode?.Focus();
                    return;
                }
            }

            try
            {
                // Check if username already exists
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                int exists = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery,
                    new SqlParameter[] { new SqlParameter("@Username", txtUsername.Text) }));

                if (exists > 0)
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUsername.Focus();
                    return;
                }

                // Hash password
                string hashedPassword = PasswordHelper.HashPassword(txtPassword.Text);

                // Insert User
                string insertUserQuery = @"INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, Role, IsActive, CreatedAt)
                                          OUTPUT INSERTED.UserId
                                          VALUES (@Username, @PasswordHash, @FullName, @Email, @Phone, @Role, 1, GETDATE())";

                SqlParameter[] userParams = new SqlParameter[]
                {
                    new SqlParameter("@Username", txtUsername.Text),
                    new SqlParameter("@PasswordHash", hashedPassword),
                    new SqlParameter("@FullName", txtFullName.Text),
                    new SqlParameter("@Email", txtEmail.Text),
                    new SqlParameter("@Phone", string.IsNullOrWhiteSpace(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text),
                    new SqlParameter("@Role", (int)role)
                };

                int userId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertUserQuery, userParams));

                // Insert role-specific data
                if (role == UserRole.Teacher)
                {
                    string insertTeacherQuery = @"INSERT INTO Teachers (UserId, TeacherCode, Department, Degree, Specialization)
                                                 VALUES (@UserId, @TeacherCode, @Department, @Degree, @Specialization)";

                    SqlParameter[] teacherParams = new SqlParameter[]
                    {
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@TeacherCode", txtCode.Text),
                        new SqlParameter("@Department", string.IsNullOrWhiteSpace(txtDepartment?.Text) ? (object)DBNull.Value : txtDepartment.Text),
                        new SqlParameter("@Degree", string.IsNullOrWhiteSpace(txtDegree?.Text) ? (object)DBNull.Value : txtDegree.Text),
                        new SqlParameter("@Specialization", string.IsNullOrWhiteSpace(txtSpecialization?.Text) ? (object)DBNull.Value : txtSpecialization.Text)
                    };

                    DatabaseHelper.ExecuteNonQuery(insertTeacherQuery, teacherParams);
                }
                else if (role == UserRole.Student)
                {
                    string insertStudentQuery = @"INSERT INTO Students (UserId, StudentCode, Class, Major, GPA, EnrollmentYear)
                                                 VALUES (@UserId, @StudentCode, @Class, @Major, 0, YEAR(GETDATE()))";

                    SqlParameter[] studentParams = new SqlParameter[]
                    {
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@StudentCode", txtCode.Text),
                        new SqlParameter("@Class", string.IsNullOrWhiteSpace(txtClass?.Text) ? (object)DBNull.Value : txtClass.Text),
                        new SqlParameter("@Major", string.IsNullOrWhiteSpace(txtMajor?.Text) ? (object)DBNull.Value : txtMajor.Text)
                    };

                    DatabaseHelper.ExecuteNonQuery(insertStudentQuery, studentParams);
                }

                MessageBox.Show("Tạo tài khoản thành công!" +
                    (chkAutoPassword.Checked ? $"\n\nMật khẩu: {txtPassword.Text}" : ""),
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo tài khoản: {ex.Message}", "Lỗi",
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
            public UserRole Value { get; set; }

            public ComboBoxItem(string text, UserRole value)
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
