using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    public partial class StudentProfileForm : Form
    {
        private int currentUserId;
        private int currentStudentId;

        // Profile controls
        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtStudentCode;
        private TextBox txtClass;
        private TextBox txtMajor;
        private TextBox txtAddress;
        private DateTimePicker dtpDateOfBirth;
        private ComboBox cboGender;
        private NumericUpDown nudAcademicYear;
        private Label lblGPA;

        // Password controls
        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;

        public StudentProfileForm()
        {
            InitializeComponent();
            LoadStudentProfile();
        }

        private void InitializeComponent()
        {
            this.Text = "Thông tin cá nhân";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Title
            Label lblTitle = new Label
            {
                Text = "THÔNG TIN CÁ NHÂN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            this.Controls.Add(lblTitle);

            // Left Panel - Profile Info
            Panel leftPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(480, this.ClientSize.Height - 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblProfileTitle = new Label
            {
                Text = "Thông tin sinh viên",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            leftPanel.Controls.Add(lblProfileTitle);

            int yPos = 60;
            int textBoxLeft = 140;
            int textBoxWidth = 300;

            // Student Code (Read-only)
            AddLabel(leftPanel, "Mã sinh viên:", 20, yPos);
            txtStudentCode = new TextBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BackColor = Color.FromArgb(236, 240, 241)
            };
            leftPanel.Controls.Add(txtStudentCode);
            yPos += 40;

            // Full Name
            AddLabel(leftPanel, "Họ tên:", 20, yPos);
            txtFullName = new TextBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10)
            };
            leftPanel.Controls.Add(txtFullName);
            yPos += 40;

            // Date of Birth
            AddLabel(leftPanel, "Ngày sinh:", 20, yPos);
            dtpDateOfBirth = new DateTimePicker
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short
            };
            leftPanel.Controls.Add(dtpDateOfBirth);
            yPos += 40;

            // Gender
            AddLabel(leftPanel, "Giới tính:", 20, yPos);
            cboGender = new ComboBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            leftPanel.Controls.Add(cboGender);
            yPos += 40;

            // Email
            AddLabel(leftPanel, "Email:", 20, yPos);
            txtEmail = new TextBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10)
            };
            leftPanel.Controls.Add(txtEmail);
            yPos += 40;

            // Phone
            AddLabel(leftPanel, "Số điện thoại:", 20, yPos);
            txtPhone = new TextBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10)
            };
            leftPanel.Controls.Add(txtPhone);
            yPos += 40;

            // Class
            AddLabel(leftPanel, "Lớp:", 20, yPos);
            txtClass = new TextBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10)
            };
            leftPanel.Controls.Add(txtClass);
            yPos += 40;

            // Major
            AddLabel(leftPanel, "Ngành:", 20, yPos);
            txtMajor = new TextBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10)
            };
            leftPanel.Controls.Add(txtMajor);
            yPos += 40;

            // Academic Year
            AddLabel(leftPanel, "Năm học:", 20, yPos);
            nudAcademicYear = new NumericUpDown
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 25),
                Font = new Font("Segoe UI", 10),
                Minimum = 2020,
                Maximum = 2030,
                Value = DateTime.Now.Year
            };
            leftPanel.Controls.Add(nudAcademicYear);
            yPos += 40;

            // Address
            AddLabel(leftPanel, "Địa chỉ:", 20, yPos);
            txtAddress = new TextBox
            {
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(textBoxWidth, 50),
                Font = new Font("Segoe UI", 10),
                Multiline = true
            };
            leftPanel.Controls.Add(txtAddress);
            yPos += 60;

            // GPA (Read-only)
            AddLabel(leftPanel, "GPA:", 20, yPos);
            lblGPA = new Label
            {
                Location = new Point(textBoxLeft, yPos + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            leftPanel.Controls.Add(lblGPA);
            yPos += 40;

            // Save Profile Button
            Button btnSaveProfile = new Button
            {
                Text = "Cập nhật thông tin",
                Location = new Point(textBoxLeft, yPos),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveProfile.FlatAppearance.BorderSize = 0;
            btnSaveProfile.Click += BtnSaveProfile_Click;
            leftPanel.Controls.Add(btnSaveProfile);

            this.Controls.Add(leftPanel);

            // Right Panel - Change Password
            Panel rightPanel = new Panel
            {
                Location = new Point(520, 70),
                Size = new Size(this.ClientSize.Width - 540, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPasswordTitle = new Label
            {
                Text = "Đổi mật khẩu",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            rightPanel.Controls.Add(lblPasswordTitle);

            yPos = 60;
            int pwdTextBoxLeft = 160;
            int pwdTextBoxWidth = 250;

            // Current Password
            AddLabel(rightPanel, "Mật khẩu hiện tại:", 20, yPos);
            txtCurrentPassword = new TextBox
            {
                Location = new Point(pwdTextBoxLeft, yPos),
                Size = new Size(pwdTextBoxWidth, 25),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●'
            };
            rightPanel.Controls.Add(txtCurrentPassword);
            yPos += 40;

            // New Password
            AddLabel(rightPanel, "Mật khẩu mới:", 20, yPos);
            txtNewPassword = new TextBox
            {
                Location = new Point(pwdTextBoxLeft, yPos),
                Size = new Size(pwdTextBoxWidth, 25),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●'
            };
            rightPanel.Controls.Add(txtNewPassword);
            yPos += 40;

            // Confirm Password
            AddLabel(rightPanel, "Xác nhận mật khẩu:", 20, yPos);
            txtConfirmPassword = new TextBox
            {
                Location = new Point(pwdTextBoxLeft, yPos),
                Size = new Size(pwdTextBoxWidth, 25),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●'
            };
            rightPanel.Controls.Add(txtConfirmPassword);
            yPos += 50;

            // Change Password Button
            Button btnChangePassword = new Button
            {
                Text = "Đổi mật khẩu",
                Location = new Point(pwdTextBoxLeft, yPos),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.Click += BtnChangePassword_Click;
            rightPanel.Controls.Add(btnChangePassword);

            this.Controls.Add(rightPanel);

            // Statistics Panel
            Panel statsPanel = new Panel
            {
                Location = new Point(520, 390),
                Size = new Size(this.ClientSize.Width - 540, this.ClientSize.Height - 410),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(236, 240, 241),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblStatsTitle = new Label
            {
                Text = "Thống kê học tập",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            statsPanel.Controls.Add(lblStatsTitle);

            this.Controls.Add(statsPanel);
        }

        private void AddLabel(Panel panel, string text, int x, int y)
        {
            Label label = new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            panel.Controls.Add(label);
        }

        private void LoadStudentProfile()
        {
            try
            {
                currentUserId = SessionManager.CurrentUser.UserId;
                currentStudentId = SessionManager.CurrentStudent.StudentId;

                // Load User info
                string userQuery = "SELECT * FROM Users WHERE UserId = @UserId";
                DataTable userDt = DatabaseHelper.ExecuteQuery(userQuery,
                    new SqlParameter[] { new SqlParameter("@UserId", currentUserId) });

                if (userDt.Rows.Count > 0)
                {
                    DataRow userRow = userDt.Rows[0];
                    txtFullName.Text = userRow["FullName"].ToString();
                    txtEmail.Text = userRow["Email"] != DBNull.Value ? userRow["Email"].ToString() : "";
                    txtPhone.Text = userRow["Phone"] != DBNull.Value ? userRow["Phone"].ToString() : "";
                }

                // Load Student info
                string studentQuery = "SELECT * FROM Students WHERE StudentId = @StudentId";
                DataTable studentDt = DatabaseHelper.ExecuteQuery(studentQuery,
                    new SqlParameter[] { new SqlParameter("@StudentId", currentStudentId) });

                if (studentDt.Rows.Count > 0)
                {
                    DataRow studentRow = studentDt.Rows[0];
                    txtStudentCode.Text = studentRow["StudentCode"].ToString();
                    txtClass.Text = studentRow["Class"] != DBNull.Value ? studentRow["Class"].ToString() : "";
                    txtMajor.Text = studentRow["Major"] != DBNull.Value ? studentRow["Major"].ToString() : "";
                    txtAddress.Text = studentRow["Address"] != DBNull.Value ? studentRow["Address"].ToString() : "";

                    if (studentRow["DateOfBirth"] != DBNull.Value)
                        dtpDateOfBirth.Value = Convert.ToDateTime(studentRow["DateOfBirth"]);

                    string gender = studentRow["Gender"] != DBNull.Value ? studentRow["Gender"].ToString() : "";
                    if (cboGender.Items.Contains(gender))
                        cboGender.SelectedItem = gender;
                    else if (cboGender.Items.Count > 0)
                        cboGender.SelectedIndex = 0;

                    if (studentRow["AcademicYear"] != DBNull.Value)
                        nudAcademicYear.Value = Convert.ToInt32(studentRow["AcademicYear"]);

                    decimal gpa = studentRow["GPA"] != DBNull.Value ? Convert.ToDecimal(studentRow["GPA"]) : 0;
                    lblGPA.Text = gpa.ToString("0.00");
                }

                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatistics()
        {
            try
            {
                // Get statistics
                string statsQuery = @"
                    SELECT
                        COUNT(DISTINCT e.CourseId) as TotalCourses,
                        COUNT(CASE WHEN g.TotalScore >= 5.0 THEN 1 END) as PassedCourses,
                        COUNT(CASE WHEN g.TotalScore < 5.0 THEN 1 END) as FailedCourses,
                        SUM(CASE WHEN g.TotalScore >= 5.0 THEN c.Credits ELSE 0 END) as TotalCredits
                    FROM Enrollments e
                    LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
                    LEFT JOIN Courses c ON e.CourseId = c.CourseId
                    WHERE e.StudentId = @StudentId";

                DataTable statsDt = DatabaseHelper.ExecuteQuery(statsQuery,
                    new SqlParameter[] { new SqlParameter("@StudentId", currentStudentId) });

                if (statsDt.Rows.Count > 0)
                {
                    DataRow statsRow = statsDt.Rows[0];
                    int totalCourses = statsRow["TotalCourses"] != DBNull.Value ? Convert.ToInt32(statsRow["TotalCourses"]) : 0;
                    int passedCourses = statsRow["PassedCourses"] != DBNull.Value ? Convert.ToInt32(statsRow["PassedCourses"]) : 0;
                    int failedCourses = statsRow["FailedCourses"] != DBNull.Value ? Convert.ToInt32(statsRow["FailedCourses"]) : 0;
                    int totalCredits = statsRow["TotalCredits"] != DBNull.Value ? Convert.ToInt32(statsRow["TotalCredits"]) : 0;

                    Panel statsPanel = this.Controls[3] as Panel; // The statistics panel
                    if (statsPanel != null)
                    {
                        int statY = 60;
                        AddStatItem(statsPanel, "Tổng số môn học:", totalCourses.ToString(), 20, statY);
                        statY += 35;
                        AddStatItem(statsPanel, "Môn đã đạt:", passedCourses.ToString(), 20, statY);
                        statY += 35;
                        AddStatItem(statsPanel, "Môn chưa đạt:", failedCourses.ToString(), 20, statY);
                        statY += 35;
                        AddStatItem(statsPanel, "Tổng tín chỉ tích lũy:", totalCredits.ToString(), 20, statY);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thống kê: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddStatItem(Panel panel, string label, string value, int x, int y)
        {
            Label lblLabel = new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11)
            };

            Label lblValue = new Label
            {
                Text = value,
                Location = new Point(x + 200, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            panel.Controls.Add(lblLabel);
            panel.Controls.Add(lblValue);
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFullName.Focus();
                    return;
                }

                // Update Users table
                string updateUser = @"UPDATE Users SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone
                    WHERE UserId = @UserId";

                DatabaseHelper.ExecuteNonQuery(updateUser, new SqlParameter[]
                {
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@UserId", currentUserId)
                });

                // Update Students table
                string updateStudent = @"UPDATE Students SET
                    DateOfBirth = @DateOfBirth,
                    Gender = @Gender,
                    Address = @Address,
                    Class = @Class,
                    Major = @Major,
                    AcademicYear = @AcademicYear
                    WHERE StudentId = @StudentId";

                DatabaseHelper.ExecuteNonQuery(updateStudent, new SqlParameter[]
                {
                    new SqlParameter("@DateOfBirth", dtpDateOfBirth.Value),
                    new SqlParameter("@Gender", cboGender.SelectedItem?.ToString() ?? ""),
                    new SqlParameter("@Address", txtAddress.Text.Trim()),
                    new SqlParameter("@Class", txtClass.Text.Trim()),
                    new SqlParameter("@Major", txtMajor.Text.Trim()),
                    new SqlParameter("@AcademicYear", (int)nudAcademicYear.Value),
                    new SqlParameter("@StudentId", currentStudentId)
                });

                // Update session
                SessionManager.CurrentUser.FullName = txtFullName.Text.Trim();
                SessionManager.CurrentUser.Email = txtEmail.Text.Trim();
                SessionManager.CurrentUser.Phone = txtPhone.Text.Trim();

                SessionManager.CurrentStudent.Class = txtClass.Text.Trim();
                SessionManager.CurrentStudent.Major = txtMajor.Text.Trim();
                SessionManager.CurrentStudent.Address = txtAddress.Text.Trim();
                SessionManager.CurrentStudent.Gender = cboGender.SelectedItem?.ToString() ?? "";
                SessionManager.CurrentStudent.AcademicYear = (int)nudAcademicYear.Value;
                SessionManager.CurrentStudent.DateOfBirth = dtpDateOfBirth.Value;

                MessageBox.Show("Cập nhật thông tin thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate
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

                // Verify current password
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

                if (currentPassword != txtCurrentPassword.Text)
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCurrentPassword.Focus();
                    txtCurrentPassword.SelectAll();
                    return;
                }

                // Update password
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
