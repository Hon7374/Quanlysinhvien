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
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponent()
        {
            this.Text = "Đăng nhập - Hệ thống Quản lý Sinh viên";
            this.Size = new Size(450, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Title Label
            Label lblTitle = new Label
            {
                Text = "HỆ THỐNG QUẢN LÝ SINH VIÊN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                Size = new Size(400, 40),
                Location = new Point(25, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username Label
            Label lblUsername = new Label
            {
                Text = "Tên đăng nhập:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 80),
                Size = new Size(120, 25)
            };

            // Username TextBox
            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 110),
                Size = new Size(350, 30)
            };

            // Password Label
            Label lblPassword = new Label
            {
                Text = "Mật khẩu:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 150),
                Size = new Size(120, 25)
            };

            // Password TextBox
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 180),
                Size = new Size(350, 30),
                PasswordChar = '●'
            };

            // Remember Me CheckBox
            chkRememberMe = new CheckBox
            {
                Text = "Ghi nhớ đăng nhập",
                Font = new Font("Segoe UI", 9),
                Location = new Point(50, 220),
                AutoSize = true
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "Đăng nhập",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(50, 255),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Thoát",
                Font = new Font("Segoe UI", 10),
                Location = new Point(240, 255),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(189, 195, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;
            // Add controls
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(chkRememberMe);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnCancel);

            // Set Enter key to login
            this.AcceptButton = btnLogin;
        }

        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkRememberMe;
        private Button btnLogin;
        private Button btnCancel;

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin đăng nhập!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Test database connection
                if (!DatabaseHelper.TestConnection())
                {
                    MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!\nVui lòng kiểm tra cấu hình kết nối.",
                        "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Query user by username, then verify password hash in code.
                string query = "SELECT * FROM Users WHERE Username = @Username";
                SqlParameter[] parameters = { new SqlParameter("@Username", username) };

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    // Ensure account is active
                    if (row["IsActive"] == DBNull.Value || Convert.ToBoolean(row["IsActive"]) == false)
                    {
                        MessageBox.Show("Tài khoản đang bị khóa hoặc không hoạt động.", "Đăng nhập thất bại",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string storedHash = row.Table.Columns.Contains("PasswordHash") ? row["PasswordHash"].ToString() : (row.Table.Columns.Contains("Password") ? row["Password"].ToString() : null);

                    // Validate password using PasswordHelper
                    if (!PasswordHelper.VerifyPassword(password, storedHash))
                    {
                        MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!", "Đăng nhập thất bại",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Create user object
                    SessionManager.CurrentUser = new User
                    {
                        UserId = Convert.ToInt32(row["UserId"]),
                        Username = row["Username"].ToString(),
                        FullName = row["FullName"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Phone"].ToString(),
                        Role = (UserRole)Convert.ToInt32(row["Role"]),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    };

                    // Update last login
                    string updateQuery = "UPDATE Users SET LastLogin = @LastLogin WHERE UserId = @UserId";
                    SqlParameter[] updateParams = {
                        new SqlParameter("@LastLogin", DateTime.Now),
                        new SqlParameter("@UserId", SessionManager.CurrentUser.UserId)
                    };
                    DatabaseHelper.ExecuteNonQuery(updateQuery, updateParams);

                    // Load additional info based on role
                    if (SessionManager.CurrentUser.Role == UserRole.Student)
                    {
                        LoadStudentInfo(SessionManager.CurrentUser.UserId);
                    }
                    else if (SessionManager.CurrentUser.Role == UserRole.Teacher)
                    {
                        LoadTeacherInfo(SessionManager.CurrentUser.UserId);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!", "Đăng nhập thất bại",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi đăng nhập",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudentInfo(int userId)
        {
            try
            {
                string query = "SELECT * FROM Students WHERE UserId = @UserId";
                SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    SessionManager.CurrentStudent = new Student
                    {
                        StudentId = Convert.ToInt32(row["StudentId"]),
                        UserId = userId,
                        StudentCode = row["StudentCode"].ToString(),
                        DateOfBirth = row["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(row["DateOfBirth"]) : DateTime.MinValue,
                        Gender = row["Gender"] != DBNull.Value ? row["Gender"].ToString() : "",
                        Address = row["Address"] != DBNull.Value ? row["Address"].ToString() : "",
                        Class = row["Class"] != DBNull.Value ? row["Class"].ToString() : "",
                        Major = row["Major"] != DBNull.Value ? row["Major"].ToString() : "",
                        AcademicYear = row["AcademicYear"] != DBNull.Value ? Convert.ToInt32(row["AcademicYear"]) : DateTime.Now.Year,
                        GPA = row["GPA"] != DBNull.Value ? Convert.ToDecimal(row["GPA"]) : 0
                    };
                }
                else
                {
                    // Create a new student record if it doesn't exist
                    string studentCode = "SV" + userId.ToString("D6");
                    string insertQuery = @"INSERT INTO Students (UserId, StudentCode, Class, Major, AcademicYear, Status)
                                         VALUES (@UserId, @StudentCode, N'Chưa xác định', N'Chưa xác định', @Year, N'Đang học');
                                         SELECT CAST(SCOPE_IDENTITY() as int)";

                    SqlParameter[] insertParams = {
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@StudentCode", studentCode),
                        new SqlParameter("@Year", DateTime.Now.Year)
                    };

                    int newStudentId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(insertQuery, insertParams));

                    SessionManager.CurrentStudent = new Student
                    {
                        StudentId = newStudentId,
                        UserId = userId,
                        StudentCode = studentCode,
                        Class = "Chưa xác định",
                        Major = "Chưa xác định",
                        AcademicYear = DateTime.Now.Year,
                        GPA = 0
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin sinh viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTeacherInfo(int userId)
        {
            string query = "SELECT * FROM Teachers WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                SessionManager.CurrentTeacher = new Teacher
                {
                    TeacherId = Convert.ToInt32(row["TeacherId"]),
                    UserId = userId,
                    TeacherCode = row["TeacherCode"].ToString(),
                    Department = row["Department"].ToString(),
                    Degree = row["Degree"].ToString(),
                    Specialization = row["Specialization"].ToString(),
                    HireDate = row["HireDate"] != DBNull.Value ? Convert.ToDateTime(row["HireDate"]) : DateTime.MinValue
                };
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

    }
}
