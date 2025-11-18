using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Forms
{
    public partial class StudentEditForm : Form
    {
        private int studentId;
        private int userId;
        private TextBox txtFullName;
        private TextBox txtStudentCode;
        private TextBox txtClass;
        private TextBox txtMajor;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtAddress;
        private ComboBox cboStatus;
        private Label lblCreatedDate;
        private Button btnDownload;

        public StudentEditForm(int studentId)
        {
            this.studentId = studentId;
            InitializeComponent();
            LoadStudentData();
        }

        private void InitializeComponent()
        {
            this.Text = "Chỉnh sửa thông tin sinh viên - Edit Student Information";
            this.Size = new Size(1000, 750);
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

            // Header with Download Button
            Panel headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(900, 80),
                BackColor = Color.White
            };

            Label lblMainTitle = new Label
            {
                Text = "Quản lý Sinh viên",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            headerPanel.Controls.Add(lblMainTitle);

            Label lblSubTitle = new Label
            {
                Text = "Chỉnh sửa thông tin sinh viên",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(0, 35),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            headerPanel.Controls.Add(lblSubTitle);

            btnDownload = new Button
            {
                Text = "⬇ Tải xuống",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(750, 30),
                Size = new Size(150, 45),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(99, 102, 241),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDownload.FlatAppearance.BorderColor = Color.FromArgb(99, 102, 241);
            btnDownload.Click += BtnDownload_Click;
            headerPanel.Controls.Add(btnDownload);

            mainPanel.Controls.Add(headerPanel);

            int yPos = 100;

            // General Information Section
            Label lblGeneralInfo = new Label
            {
                Text = "Thông tin chung",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblGeneralInfo);

            Label lblGeneralDesc = new Label
            {
                Text = "Cập nhật chi tiết sinh viên và lưu thay đổi.",
                Font = new Font("Segoe UI", 9),
                Location = new Point(0, yPos + 25),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            mainPanel.Controls.Add(lblGeneralDesc);

            yPos += 70;

            // Personal Information Section Header
            Label lblPersonalInfo = new Label
            {
                Text = "Thông tin cá nhân",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblPersonalInfo);

            yPos += 40;

            // Full Name
            AddLabel(mainPanel, "Họ tên", 0, yPos);
            txtFullName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(430, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtFullName);

            // Student Code (MSSV) - Read Only
            AddLabel(mainPanel, "MSSV", 470, yPos);
            txtStudentCode = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(470, yPos + 25),
                Size = new Size(430, 30),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                BackColor = Color.FromArgb(243, 244, 246)
            };
            mainPanel.Controls.Add(txtStudentCode);

            yPos += 80;

            // Class
            AddLabel(mainPanel, "Lớp", 0, yPos);
            txtClass = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(430, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtClass);

            // Major
            AddLabel(mainPanel, "Ngành", 470, yPos);
            txtMajor = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(470, yPos + 25),
                Size = new Size(430, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtMajor);

            yPos += 80;

            // Contact Information Section Header
            Label lblContactInfo = new Label
            {
                Text = "Thông tin liên hệ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblContactInfo);

            yPos += 40;

            // Email
            AddLabel(mainPanel, "Email", 0, yPos);
            txtEmail = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(430, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtEmail);

            // Phone
            AddLabel(mainPanel, "SĐT", 470, yPos);
            txtPhone = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(470, yPos + 25),
                Size = new Size(430, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtPhone);

            yPos += 80;

            // Address
            AddLabel(mainPanel, "Địa chỉ", 0, yPos);
            txtAddress = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(900, 60),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true
            };
            mainPanel.Controls.Add(txtAddress);

            yPos += 110;

            // Status Section
            Label lblStatusSection = new Label
            {
                Text = "Trạng thái",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblStatusSection);

            yPos += 40;

            // Status
            AddLabel(mainPanel, "Trạng thái", 0, yPos);
            cboStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new object[] { "Đang hoạt động", "Đang học", "Bảo lưu", "Đã tốt nghiệp" });
            mainPanel.Controls.Add(cboStatus);

            yPos += 80;

            // Created Date - Read Only
            AddLabel(mainPanel, "Ngày tạo", 0, yPos);
            lblCreatedDate = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(300, 30),
                ForeColor = Color.FromArgb(107, 114, 128),
                BackColor = Color.FromArgb(243, 244, 246),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            mainPanel.Controls.Add(lblCreatedDate);

            yPos += 85;

            // Button Panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(900, 50),
                BackColor = Color.White
            };

            // Cancel Button
            Button btnCancel = new Button
            {
                Text = "Hủy bỏ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(650, 0),
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
                Text = "Lưu thay đổi",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(780, 0),
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

        private void LoadStudentData()
        {
            try
            {
                string query = @"
                    SELECT
                        s.UserId,
                        u.FullName,
                        s.StudentCode,
                        s.Class,
                        s.Major,
                        u.Email,
                        u.Phone,
                        s.Address,
                        s.Status,
                        s.CreatedAt
                    FROM Students s
                    INNER JOIN Users u ON s.UserId = u.UserId
                    WHERE s.StudentId = @StudentId";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@StudentId", studentId) });

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    userId = Convert.ToInt32(row["UserId"]);
                    txtFullName.Text = row["FullName"].ToString();
                    txtStudentCode.Text = row["StudentCode"].ToString();
                    txtClass.Text = row["Class"].ToString();
                    txtMajor.Text = row["Major"].ToString();
                    txtEmail.Text = row["Email"].ToString();
                    txtPhone.Text = row["Phone"].ToString();
                    txtAddress.Text = row["Address"].ToString();

                    string status = row["Status"].ToString();
                    cboStatus.SelectedItem = status;
                    if (cboStatus.SelectedIndex == -1 && cboStatus.Items.Count > 0)
                    {
                        cboStatus.SelectedIndex = 0;
                    }

                    if (row["CreatedAt"] != DBNull.Value)
                    {
                        lblCreatedDate.Text = Convert.ToDateTime(row["CreatedAt"]).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        lblCreatedDate.Text = "N/A";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin sinh viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Vui lòng nhập email!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                // Check if email already exists (excluding current user)
                string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserId != @UserId";
                int emailCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkEmailQuery,
                    new SqlParameter[] {
                        new SqlParameter("@Email", txtEmail.Text.Trim()),
                        new SqlParameter("@UserId", userId)
                    }));

                if (emailCount > 0)
                {
                    MessageBox.Show("Email đã tồn tại! Vui lòng nhập email khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    txtEmail.SelectAll();
                    return;
                }

                // Update User
                string updateUserQuery = @"UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone
                    WHERE UserId = @UserId";

                SqlParameter[] userParams = new SqlParameter[]
                {
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@UserId", userId)
                };

                DatabaseHelper.ExecuteNonQuery(updateUserQuery, userParams);

                // Update Student
                string updateStudentQuery = @"UPDATE Students SET
                    Class = @Class,
                    Major = @Major,
                    Address = @Address,
                    Status = @Status
                    WHERE StudentId = @StudentId";

                SqlParameter[] studentParams = new SqlParameter[]
                {
                    new SqlParameter("@Class", txtClass.Text.Trim()),
                    new SqlParameter("@Major", txtMajor.Text.Trim()),
                    new SqlParameter("@Address", txtAddress.Text.Trim()),
                    new SqlParameter("@Status", cboStatus.SelectedItem.ToString()),
                    new SqlParameter("@StudentId", studentId)
                };

                DatabaseHelper.ExecuteNonQuery(updateStudentQuery, studentParams);

                MessageBox.Show("Cập nhật thông tin sinh viên thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin sinh viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng tải xuống sẽ được triển khai sau!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
