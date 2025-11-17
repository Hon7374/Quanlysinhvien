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
    public partial class StudentCreateForm : Form
    {
        private TextBox txtFullName;
        private TextBox txtStudentCode;
        private TextBox txtClass;
        private TextBox txtMajor;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtAddress;
        private ComboBox cboStatus;

        public StudentCreateForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Thêm Sinh viên Mới - Add New Student";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Main Panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40, 30, 40, 30),
                BackColor = Color.White,
                AutoScroll = true
            };

            // Title Section
            Label lblMainTitle = new Label
            {
                Text = "Quản lý Sinh viên",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblMainTitle);

            Label lblSubTitle = new Label
            {
                Text = "Thêm Sinh viên Mới",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(0, 35),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblSubTitle);

            int yPos = 100;

            // Student Information Section
            Label lblStudentInfo = new Label
            {
                Text = "Thông tin Sinh viên",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblStudentInfo);

            Label lblStudentDesc = new Label
            {
                Text = "Điền đầy đủ thông tin để thêm sinh viên mới vào hệ thống.",
                Font = new Font("Segoe UI", 9),
                Location = new Point(0, yPos + 25),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            mainPanel.Controls.Add(lblStudentDesc);

            yPos += 70;

            // Full Name
            AddLabel(mainPanel, "Họ tên", 0, yPos);
            txtFullName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtFullName);

            // Student Code (MSSV)
            AddLabel(mainPanel, "MSSV", 420, yPos);
            txtStudentCode = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(420, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtStudentCode);

            yPos += 80;

            // Class
            AddLabel(mainPanel, "Lớp", 0, yPos);
            txtClass = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "K20 CNTT"
            };
            mainPanel.Controls.Add(txtClass);

            // Major
            AddLabel(mainPanel, "Ngành", 420, yPos);
            txtMajor = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(420, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Công nghệ thông tin"
            };
            mainPanel.Controls.Add(txtMajor);

            yPos += 80;

            // Contact Information Section
            Label lblContactInfo = new Label
            {
                Text = "Thông tin liên hệ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblContactInfo);

            yPos += 50;

            // Email
            AddLabel(mainPanel, "Email", 0, yPos);
            txtEmail = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "nguyenvana@example.com"
            };
            mainPanel.Controls.Add(txtEmail);

            // Phone
            AddLabel(mainPanel, "SĐT", 420, yPos);
            txtPhone = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(420, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "0912345678"
            };
            mainPanel.Controls.Add(txtPhone);

            yPos += 80;

            // Address
            AddLabel(mainPanel, "Địa chỉ", 0, yPos);
            txtAddress = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(800, 60),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                PlaceholderText = "123 Đường ABC, Quận XYZ, TP.HCM"
            };
            mainPanel.Controls.Add(txtAddress);

            yPos += 110;

            // Status Section
            Label lblStatusSection = new Label
            {
                Text = "Trạng thái",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblStatusSection);

            yPos += 50;

            // Status
            AddLabel(mainPanel, "Trạng thái", 0, yPos);
            cboStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new object[] { "Đang học", "Bảo lưu", "Đã tốt nghiệp" });
            cboStatus.SelectedIndex = 0;
            mainPanel.Controls.Add(cboStatus);

            yPos += 85;

            // Button Panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(800, 50),
                BackColor = Color.White
            };

            // Cancel Button
            Button btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(550, 0),
                Size = new Size(120, 45),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(btnCancel);

            // Save Button
            Button btnSave = new Button
            {
                Text = "Lưu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(680, 0),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            mainPanel.Controls.Add(buttonPanel);

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

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Vui lòng nhập họ tên sinh viên!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFullName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtStudentCode.Text))
                {
                    MessageBox.Show("Vui lòng nhập mã sinh viên (MSSV)!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStudentCode.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Vui lòng nhập email!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                // Check if student code already exists
                string checkCodeQuery = "SELECT COUNT(*) FROM Students WHERE StudentCode = @StudentCode";
                int codeCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkCodeQuery,
                    new SqlParameter[] { new SqlParameter("@StudentCode", txtStudentCode.Text.Trim()) }));

                if (codeCount > 0)
                {
                    MessageBox.Show("Mã sinh viên đã tồn tại! Vui lòng nhập mã khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStudentCode.Focus();
                    txtStudentCode.SelectAll();
                    return;
                }

                // Check if email already exists
                string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                int emailCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkEmailQuery,
                    new SqlParameter[] { new SqlParameter("@Email", txtEmail.Text.Trim()) }));

                if (emailCount > 0)
                {
                    MessageBox.Show("Email đã tồn tại! Vui lòng nhập email khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    txtEmail.SelectAll();
                    return;
                }

                // Create User first
                string insertUserQuery = @"INSERT INTO Users
                    (Username, PasswordHash, FullName, Email, Phone, Role, IsActive)
                    VALUES (@Username, @PasswordHash, @FullName, @Email, @Phone, @Role, 1);
                    SELECT SCOPE_IDENTITY();";

                string defaultPassword = "Student@123";
                string passwordHash = PasswordHelper.HashPassword(defaultPassword);

                SqlParameter[] userParams = new SqlParameter[]
                {
                    new SqlParameter("@Username", txtStudentCode.Text.Trim()),
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@Role", (int)UserRole.Student)
                };

                int userId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertUserQuery, userParams));

                // Create Student
                string insertStudentQuery = @"INSERT INTO Students
                    (UserId, StudentCode, Class, Major, Address, Status)
                    VALUES (@UserId, @StudentCode, @Class, @Major, @Address, @Status)";

                SqlParameter[] studentParams = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@StudentCode", txtStudentCode.Text.Trim()),
                    new SqlParameter("@Class", txtClass.Text.Trim()),
                    new SqlParameter("@Major", txtMajor.Text.Trim()),
                    new SqlParameter("@Address", txtAddress.Text.Trim()),
                    new SqlParameter("@Status", cboStatus.SelectedItem.ToString())
                };

                DatabaseHelper.ExecuteNonQuery(insertStudentQuery, studentParams);

                MessageBox.Show($"Thêm sinh viên thành công!\\n\\nMật khẩu mặc định: {defaultPassword}", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sinh viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
