using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    public partial class AdminDashboard : Form
    {
        private Panel panelMenu;
        private Panel panelContent;
        private Label lblWelcome;

        public AdminDashboard()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản trị hệ thống - Admin Dashboard";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Menu Panel
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            // Welcome Label
            lblWelcome = new Label
            {
                Text = $"Xin chào, {SessionManager.CurrentUser.FullName}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 20),
                AutoSize = false,
                Size = new Size(230, 60),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelMenu.Controls.Add(lblWelcome);

            // Menu Buttons
            int yPos = 100;
            AddMenuButton("📊 Tổng quan", yPos, (s, e) => LoadDashboard()); yPos += 60;
            AddMenuButton("👨‍🎓 Quản lý Sinh viên", yPos, (s, e) => LoadStudentManagement()); yPos += 60;
            AddMenuButton("👨‍🏫 Quản lý Giảng viên", yPos, (s, e) => LoadTeacherManagement()); yPos += 60;
            AddMenuButton("📚 Quản lý Môn học", yPos, (s, e) => LoadCourseManagement()); yPos += 60;
            AddMenuButton("📅 Quản lý Thời khóa biểu", yPos, (s, e) => LoadScheduleManagement()); yPos += 60;
            AddMenuButton("👤 Quản lý Tài khoản", yPos, (s, e) => LoadUserManagement()); yPos += 60;
            AddMenuButton("📈 Báo cáo Thống kê", yPos, (s, e) => LoadReports()); yPos += 60;
            AddMenuButton("🔄 Cập nhật Database", yPos, (s, e) => LoadDatabaseUpdate()); yPos += 60;
            AddMenuButton("🚪 Đăng xuất", yPos, (s, e) => Logout());

            // Content Panel
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(20)
            };

            this.Controls.Add(panelContent);
            this.Controls.Add(panelMenu);
        }

        private void AddMenuButton(string text, int yPos, EventHandler clickHandler)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos),
                Size = new Size(250, 45),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btn.Click += clickHandler;
            panelMenu.Controls.Add(btn);
        }

        private void LoadDashboard()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "TỔNG QUAN HỆ THỐNG",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            panelContent.Controls.Add(lblTitle);

            try
            {
                // Get statistics
                int totalStudents = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Students"));
                int totalTeachers = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Teachers"));
                int totalCourses = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Courses"));
                int totalEnrollments = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Enrollments"));

                // Create stat cards
                CreateStatCard("Tổng số Sinh viên", totalStudents.ToString(), Color.FromArgb(52, 152, 219), 20, 80);
                CreateStatCard("Tổng số Giảng viên", totalTeachers.ToString(), Color.FromArgb(46, 204, 113), 260, 80);
                CreateStatCard("Tổng số Môn học", totalCourses.ToString(), Color.FromArgb(155, 89, 182), 500, 80);
                CreateStatCard("Tổng số Đăng ký", totalEnrollments.ToString(), Color.FromArgb(241, 196, 15), 740, 80);

                // Recent activities
                Label lblRecent = new Label
                {
                    Text = "HOẠT ĐỘNG GÂN ĐÂY",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 250),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panelContent.Controls.Add(lblRecent);

                // Recent logins
                DataGridView dgvRecent = new DataGridView
                {
                    Location = new Point(20, 290),
                    Size = new Size(900, 300),
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None
                };

                string query = @"SELECT TOP 10 u.FullName, u.Username, u.Role, u.LastLogin
                                FROM Users u
                                WHERE u.LastLogin IS NOT NULL
                                ORDER BY u.LastLogin DESC";
                dgvRecent.DataSource = DatabaseHelper.ExecuteQuery(query);

                if (dgvRecent.Columns.Count >= 4)
                {
                    dgvRecent.Columns[0].HeaderText = "Họ tên";
                    dgvRecent.Columns[1].HeaderText = "Tên đăng nhập";
                    dgvRecent.Columns[2].HeaderText = "Vai trò";
                    dgvRecent.Columns[3].HeaderText = "Lần đăng nhập cuối";
                }

                panelContent.Controls.Add(dgvRecent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateStatCard(string title, string value, Color color, int x, int y)
        {
            Panel card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(220, 120),
                BackColor = color
            };

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(20, 75),
                AutoSize = true
            };

            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            panelContent.Controls.Add(card);
        }

        private void LoadStudentManagement()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ SINH VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            // Add Student Button
            Button btnAdd = new Button
            {
                Text = "+ Thêm Sinh viên",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 70),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.Click += (s, e) => MessageBox.Show("Chức năng thêm sinh viên sẽ được phát triển!");
            panelContent.Controls.Add(btnAdd);

            // Search TextBox
            TextBox txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(200, 75),
                Size = new Size(300, 30)
            };
            txtSearch.TextChanged += (s, e) => SearchStudents(txtSearch.Text);
            panelContent.Controls.Add(txtSearch);

            // DataGridView
            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(900, 450),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                Tag = txtSearch // Store reference for search
            };

            LoadStudentsData(dgv);
            panelContent.Controls.Add(dgv);
        }

        private void LoadStudentsData(DataGridView dgv)
        {
            try
            {
                string query = @"SELECT s.StudentId, s.StudentCode, u.FullName, u.Email, u.Phone,
                                s.Class, s.Major, s.GPA
                                FROM Students s
                                INNER JOIN Users u ON s.UserId = u.UserId";
                dgv.DataSource = DatabaseHelper.ExecuteQuery(query);

                if (dgv.Columns.Count >= 8)
                {
                    dgv.Columns[0].HeaderText = "ID";
                    dgv.Columns[1].HeaderText = "Mã SV";
                    dgv.Columns[2].HeaderText = "Họ tên";
                    dgv.Columns[3].HeaderText = "Email";
                    dgv.Columns[4].HeaderText = "Số điện thoại";
                    dgv.Columns[5].HeaderText = "Lớp";
                    dgv.Columns[6].HeaderText = "Ngành";
                    dgv.Columns[7].HeaderText = "GPA";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách sinh viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchStudents(string searchText)
        {
            // Implementation for search functionality
        }

        private void LoadTeacherManagement()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ GIẢNG VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(900, 450),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            try
            {
                string query = @"SELECT t.TeacherId, t.TeacherCode, u.FullName, u.Email,
                                t.Department, t.Degree, t.Specialization
                                FROM Teachers t
                                INNER JOIN Users u ON t.UserId = u.UserId";
                dgv.DataSource = DatabaseHelper.ExecuteQuery(query);

                if (dgv.Columns.Count >= 7)
                {
                    dgv.Columns[0].HeaderText = "ID";
                    dgv.Columns[1].HeaderText = "Mã GV";
                    dgv.Columns[2].HeaderText = "Họ tên";
                    dgv.Columns[3].HeaderText = "Email";
                    dgv.Columns[4].HeaderText = "Khoa";
                    dgv.Columns[5].HeaderText = "Học vị";
                    dgv.Columns[6].HeaderText = "Chuyên môn";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách giảng viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            panelContent.Controls.Add(dgv);
        }

        private void LoadCourseManagement()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ MÔN HỌC",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(900, 450),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            try
            {
                string query = @"SELECT c.CourseId, c.CourseCode, c.CourseName, c.Credits,
                                u.FullName as TeacherName, c.Semester, c.AcademicYear
                                FROM Courses c
                                LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                                LEFT JOIN Users u ON t.UserId = u.UserId";
                dgv.DataSource = DatabaseHelper.ExecuteQuery(query);

                if (dgv.Columns.Count >= 7)
                {
                    dgv.Columns[0].HeaderText = "ID";
                    dgv.Columns[1].HeaderText = "Mã môn";
                    dgv.Columns[2].HeaderText = "Tên môn học";
                    dgv.Columns[3].HeaderText = "Tín chỉ";
                    dgv.Columns[4].HeaderText = "Giảng viên";
                    dgv.Columns[5].HeaderText = "Học kỳ";
                    dgv.Columns[6].HeaderText = "Năm học";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            panelContent.Controls.Add(dgv);
        }

        private void LoadUserManagement()
        {
            MessageBox.Show("Chức năng quản lý tài khoản đang được phát triển!");
        }

        private void LoadScheduleManagement()
        {
            panelContent.Controls.Clear();
            ScheduleManagementForm form = new ScheduleManagementForm();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panelContent.Controls.Add(form);
            form.Show();
        }

        private void LoadReports()
        {
            MessageBox.Show("Chức năng báo cáo thống kê đang được phát triển!");
        }

        private void LoadDatabaseUpdate()
        {
            DatabaseUpdateForm form = new DatabaseUpdateForm();
            form.ShowDialog();
            // Reload dashboard after update
            LoadDashboard();
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SessionManager.Logout();
                this.Close();
            }
        }
    }
}
