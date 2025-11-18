using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    public partial class TeacherDashboard : Form
    {
        private Panel panelMenu;
        private Panel panelContent;
        private Label lblWelcome;

        public TeacherDashboard()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Giảng viên - Teacher Dashboard";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Menu Panel
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(39, 174, 96)
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
            AddMenuButton("📊 Tổng quan", yPos, (s, e) => LoadDashboard()); yPos += 50;
            AddMenuButton("📚 Môn học của tôi", yPos, (s, e) => LoadMyCourses()); yPos += 50;
            AddMenuButton("👨‍🎓 Danh sách Sinh viên", yPos, (s, e) => LoadStudentList()); yPos += 50;
            AddMenuButton("✏️ Nhập điểm", yPos, (s, e) => LoadGradeEntry()); yPos += 50;
            AddMenuButton("📋 Xem điểm đã nhập", yPos, (s, e) => LoadGradeView()); yPos += 50;
            AddMenuButton("👤 Thông tin cá nhân", yPos, (s, e) => LoadProfile()); yPos += 50;
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
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 204, 113);
            btn.Click += clickHandler;
            panelMenu.Controls.Add(btn);
        }

        private void LoadDashboard()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "TỔNG QUAN GIẢNG VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(39, 174, 96)
            };
            panelContent.Controls.Add(lblTitle);

            try
            {
                int teacherId = SessionManager.CurrentTeacher.TeacherId;

                // Get statistics
                string queryMyCourses = "SELECT COUNT(*) FROM Courses WHERE TeacherId = @TeacherId AND IsActive = 1";
                int myCourses = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryMyCourses,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) }));

                string queryMyStudents = @"SELECT COUNT(DISTINCT e.StudentId)
                                          FROM Enrollments e
                                          INNER JOIN Courses c ON e.CourseId = c.CourseId
                                          WHERE c.TeacherId = @TeacherId";
                int myStudents = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryMyStudents,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) }));

                string queryPendingGrades = @"SELECT COUNT(*)
                                             FROM Enrollments e
                                             INNER JOIN Courses c ON e.CourseId = c.CourseId
                                             LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
                                             WHERE c.TeacherId = @TeacherId AND g.GradeId IS NULL";
                int pendingGrades = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryPendingGrades,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) }));

                // Create stat cards
                CreateStatCard("Môn học đang dạy", myCourses.ToString(), Color.FromArgb(52, 152, 219), 20, 80);
                CreateStatCard("Tổng số Sinh viên", myStudents.ToString(), Color.FromArgb(46, 204, 113), 260, 80);
                CreateStatCard("Chưa nhập điểm", pendingGrades.ToString(), Color.FromArgb(231, 76, 60), 500, 80);

                // Teacher info
                Label lblInfo = new Label
                {
                    Text = "THÔNG TIN GIẢNG VIÊN",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 230),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panelContent.Controls.Add(lblInfo);

                Panel infoPanel = new Panel
                {
                    Location = new Point(20, 270),
                    Size = new Size(900, 150),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                string info = $@"Mã giảng viên: {SessionManager.CurrentTeacher.TeacherCode}
Họ tên: {SessionManager.CurrentUser.FullName}
Khoa: {SessionManager.CurrentTeacher.Department}
Học vị: {SessionManager.CurrentTeacher.Degree}
Chuyên môn: {SessionManager.CurrentTeacher.Specialization}
Email: {SessionManager.CurrentUser.Email}
Số điện thoại: {SessionManager.CurrentUser.Phone}";

                Label lblDetails = new Label
                {
                    Text = info,
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, 20),
                    AutoSize = true
                };
                infoPanel.Controls.Add(lblDetails);
                panelContent.Controls.Add(infoPanel);

                // My courses list
                Label lblCourses = new Label
                {
                    Text = "MÔN HỌC ĐANG DẠY",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 440),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panelContent.Controls.Add(lblCourses);

                DataGridView dgvCourses = new DataGridView
                {
                    Location = new Point(20, 480),
                    Size = new Size(900, 150),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None
                };

                string queryCourses = @"SELECT c.CourseCode, c.CourseName, c.Credits,
                                       c.Semester, c.AcademicYear,
                                       (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) as TotalStudents
                                       FROM Courses c
                                       WHERE c.TeacherId = @TeacherId AND c.IsActive = 1";
                dgvCourses.DataSource = DatabaseHelper.ExecuteQuery(queryCourses,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) });

                if (dgvCourses.Columns.Count >= 6)
                {
                    dgvCourses.Columns[0].HeaderText = "Mã môn";
                    dgvCourses.Columns[1].HeaderText = "Tên môn học";
                    dgvCourses.Columns[2].HeaderText = "Số tín chỉ";
                    dgvCourses.Columns[3].HeaderText = "Học kỳ";
                    dgvCourses.Columns[4].HeaderText = "Năm học";
                    dgvCourses.Columns[5].HeaderText = "Số SV";
                }

                panelContent.Controls.Add(dgvCourses);
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

        private void LoadMyCourses()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "MÔN HỌC CỦA TÔI",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(panelContent.Width - 60, panelContent.Height - 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            try
            {
                string query = @"SELECT c.CourseId, c.CourseCode, c.CourseName, c.Credits,
                                c.Semester, c.AcademicYear, c.MaxStudents,
                                (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) as Enrolled
                                FROM Courses c
                                WHERE c.TeacherId = @TeacherId";
                dgv.DataSource = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@TeacherId", SessionManager.CurrentTeacher.TeacherId) });

                if (dgv.Columns.Count >= 8)
                {
                    dgv.Columns[0].HeaderText = "ID";
                    dgv.Columns[1].HeaderText = "Mã môn";
                    dgv.Columns[2].HeaderText = "Tên môn học";
                    dgv.Columns[3].HeaderText = "Tín chỉ";
                    dgv.Columns[4].HeaderText = "Học kỳ";
                    dgv.Columns[5].HeaderText = "Năm học";
                    dgv.Columns[6].HeaderText = "Sĩ số tối đa";
                    dgv.Columns[7].HeaderText = "Đã đăng ký";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            panelContent.Controls.Add(dgv);
        }

        private void LoadStudentList()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "DANH SÁCH SINH VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            // Course selection
            Label lblCourse = new Label
            {
                Text = "Chọn môn học:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 70),
                AutoSize = true
            };
            panelContent.Controls.Add(lblCourse);

            ComboBox cboCourse = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(140, 68),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Load courses
            string queryCourses = @"SELECT CourseId, CONCAT(CourseCode, ' - ', CourseName) as DisplayName
                                   FROM Courses
                                   WHERE TeacherId = @TeacherId";
            DataTable dtCourses = DatabaseHelper.ExecuteQuery(queryCourses,
                new SqlParameter[] { new SqlParameter("@TeacherId", SessionManager.CurrentTeacher.TeacherId) });

            cboCourse.DisplayMember = "DisplayName";
            cboCourse.ValueMember = "CourseId";
            cboCourse.DataSource = dtCourses;

            panelContent.Controls.Add(cboCourse);

            // Student DataGridView
            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 120),
                Size = new Size(panelContent.Width - 60, panelContent.Height - 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Name = "dgvStudents"
            };
            panelContent.Controls.Add(dgv);

            cboCourse.SelectedIndexChanged += (s, e) =>
            {
                if (cboCourse.SelectedValue != null)
                {
                    LoadStudentsByCourse(dgv, Convert.ToInt32(cboCourse.SelectedValue));
                }
            };

            if (cboCourse.Items.Count > 0)
            {
                cboCourse.SelectedIndex = 0;
                // Manually trigger load for first item
                if (cboCourse.SelectedValue != null)
                {
                    LoadStudentsByCourse(dgv, Convert.ToInt32(cboCourse.SelectedValue));
                }
            }
        }

        private void LoadStudentsByCourse(DataGridView dgv, int courseId)
        {
            try
            {
                string query = @"SELECT s.StudentCode, u.FullName, u.Email, u.Phone,
                                s.Class, s.Major, e.EnrollmentDate, e.Status
                                FROM Enrollments e
                                INNER JOIN Students s ON e.StudentId = s.StudentId
                                INNER JOIN Users u ON s.UserId = u.UserId
                                WHERE e.CourseId = @CourseId
                                ORDER BY s.StudentCode";

                dgv.DataSource = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@CourseId", courseId) });

                if (dgv.Columns.Count >= 8)
                {
                    dgv.Columns[0].HeaderText = "Mã SV";
                    dgv.Columns[1].HeaderText = "Họ tên";
                    dgv.Columns[2].HeaderText = "Email";
                    dgv.Columns[3].HeaderText = "Số ĐT";
                    dgv.Columns[4].HeaderText = "Lớp";
                    dgv.Columns[5].HeaderText = "Ngành";
                    dgv.Columns[6].HeaderText = "Ngày đăng ký";
                    dgv.Columns[7].HeaderText = "Trạng thái";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách sinh viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGradeEntry()
        {
            panelContent.Controls.Clear();

            GradeEntryForm gradeForm = new GradeEntryForm();
            gradeForm.TopLevel = false;
            gradeForm.FormBorderStyle = FormBorderStyle.None;
            gradeForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(gradeForm);
            gradeForm.Show();
        }

        private void LoadGradeView()
        {
            panelContent.Controls.Clear();

            GradeViewForm gradeViewForm = new GradeViewForm();
            gradeViewForm.TopLevel = false;
            gradeViewForm.FormBorderStyle = FormBorderStyle.None;
            gradeViewForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(gradeViewForm);
            gradeViewForm.Show();
        }

        private void LoadProfile()
        {
            panelContent.Controls.Clear();

            TeacherProfileForm profileForm = new TeacherProfileForm();
            profileForm.TopLevel = false;
            profileForm.FormBorderStyle = FormBorderStyle.None;
            profileForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(profileForm);
            profileForm.Show();
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
