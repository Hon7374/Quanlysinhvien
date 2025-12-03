using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    public partial class StudentDashboard : Form
    {
        private Panel panelMenu;
        private Panel panelContent;
        private Label lblWelcome;

        public StudentDashboard()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Sinh viên - Student Dashboard";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Menu Panel
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(52, 152, 219)
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
            AddMenuButton("📚 Môn học đã đăng ký", yPos, (s, e) => LoadMyCourses()); yPos += 50;
            AddMenuButton("📝 Xem điểm", yPos, (s, e) => LoadMyGrades()); yPos += 50;
            AddMenuButton("📖 Đăng ký môn học", yPos, (s, e) => LoadCourseRegistration()); yPos += 50;
            AddMenuButton("💳 Thanh toán học phí", yPos, (s, e) => LoadPayment()); yPos += 50;
            AddMenuButton("📅 Lịch học", yPos, (s, e) => LoadSchedule()); yPos += 50;
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
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btn.Click += clickHandler;
            panelMenu.Controls.Add(btn);
        }

        private void LoadDashboard()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "TỔNG QUAN SINH VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            panelContent.Controls.Add(lblTitle);

            try
            {
                int studentId = SessionManager.CurrentStudent.StudentId;

                // Get statistics
                string queryEnrolled = "SELECT COUNT(*) FROM Enrollments WHERE StudentId = @StudentId AND Status = 'Enrolled'";
                int enrolledCourses = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryEnrolled,
                    new SqlParameter[] { new SqlParameter("@StudentId", studentId) }));

                string queryCompleted = "SELECT COUNT(*) FROM Enrollments WHERE StudentId = @StudentId AND Status = 'Completed'";
                int completedCourses = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryCompleted,
                    new SqlParameter[] { new SqlParameter("@StudentId", studentId) }));

                decimal gpa = SessionManager.CurrentStudent.GPA;

                // Create stat cards
                CreateStatCard("Môn đang học", enrolledCourses.ToString(), Color.FromArgb(52, 152, 219), 20, 80);
                CreateStatCard("Môn đã hoàn thành", completedCourses.ToString(), Color.FromArgb(46, 204, 113), 260, 80);
                CreateStatCard("GPA", gpa.ToString("0.00"), Color.FromArgb(155, 89, 182), 500, 80);

                // Student info panel
                Label lblInfo = new Label
                {
                    Text = "THÔNG TIN SINH VIÊN",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 230),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panelContent.Controls.Add(lblInfo);

                Panel infoPanel = new Panel
                {
                    Location = new Point(20, 270),
                    Size = new Size(900, 180),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                string info = $@"Mã sinh viên: {SessionManager.CurrentStudent.StudentCode}
Họ tên: {SessionManager.CurrentUser.FullName}
Ngày sinh: {SessionManager.CurrentStudent.DateOfBirth:dd/MM/yyyy}
Giới tính: {SessionManager.CurrentStudent.Gender}
Lớp: {SessionManager.CurrentStudent.Class}
Ngành: {SessionManager.CurrentStudent.Major}
Năm học: {SessionManager.CurrentStudent.AcademicYear}
Email: {SessionManager.CurrentUser.Email}
Số điện thoại: {SessionManager.CurrentUser.Phone}
Địa chỉ: {SessionManager.CurrentStudent.Address}";

                Label lblDetails = new Label
                {
                    Text = info,
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, 5),
                    AutoSize = true
                };
                infoPanel.Controls.Add(lblDetails);
                panelContent.Controls.Add(infoPanel);

                // Current courses
                Label lblCourses = new Label
                {
                    Text = "MÔN HỌC ĐANG HỌC",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 470),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panelContent.Controls.Add(lblCourses);

                DataGridView dgvCourses = new DataGridView
                {
                    Location = new Point(20, 510),
                    Size = new Size(900, 130),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None
                };

                string queryCourses = @"SELECT c.CourseCode, c.CourseName, c.Credits,
                                       u.FullName as TeacherName, c.Semester, e.Status
                                       FROM Enrollments e
                                       INNER JOIN Courses c ON e.CourseId = c.CourseId
                                       LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                                       LEFT JOIN Users u ON t.UserId = u.UserId
                                       WHERE e.StudentId = @StudentId AND e.Status = 'Enrolled'";
                dgvCourses.DataSource = DatabaseHelper.ExecuteQuery(queryCourses,
                    new SqlParameter[] { new SqlParameter("@StudentId", studentId) });

                if (dgvCourses.Columns.Count >= 6)
                {
                    dgvCourses.Columns[0].HeaderText = "Mã môn";
                    dgvCourses.Columns[1].HeaderText = "Tên môn học";
                    dgvCourses.Columns[2].HeaderText = "Số tín chỉ";
                    dgvCourses.Columns[3].HeaderText = "Giảng viên";
                    dgvCourses.Columns[4].HeaderText = "Học kỳ";
                    dgvCourses.Columns[5].HeaderText = "Trạng thái";
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
            try
            {
                panelContent.Controls.Clear();
                MyCoursesForm myCoursesForm = new MyCoursesForm();
                myCoursesForm.TopLevel = false;
                myCoursesForm.FormBorderStyle = FormBorderStyle.None;
                myCoursesForm.Dock = DockStyle.Fill;
                panelContent.Controls.Add(myCoursesForm);
                myCoursesForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMyGrades()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "BẢNG ĐIỂM CỦA TÔI",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            panelContent.Controls.Add(lblTitle);

            // Statistics Panel
            Panel statsPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(900, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            try
            {
                int studentId = SessionManager.CurrentStudent.StudentId;

                // Calculate statistics
                string statsQuery = @"SELECT
                    COUNT(CASE WHEN g.TotalScore IS NOT NULL THEN 1 END) as GradedCourses,
                    COUNT(CASE WHEN g.TotalScore >= 5.0 THEN 1 END) as PassedCourses,
                    AVG(CASE WHEN g.TotalScore IS NOT NULL THEN g.TotalScore END) as AvgScore
                    FROM Enrollments e
                    LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
                    WHERE e.StudentId = @StudentId";

                DataTable statsTable = DatabaseHelper.ExecuteQuery(statsQuery,
                    new SqlParameter[] { new SqlParameter("@StudentId", studentId) });

                if (statsTable.Rows.Count > 0)
                {
                    DataRow statsRow = statsTable.Rows[0];
                    int gradedCourses = statsRow["GradedCourses"] != DBNull.Value ? Convert.ToInt32(statsRow["GradedCourses"]) : 0;
                    int passedCourses = statsRow["PassedCourses"] != DBNull.Value ? Convert.ToInt32(statsRow["PassedCourses"]) : 0;
                    decimal avgScore = statsRow["AvgScore"] != DBNull.Value ? Convert.ToDecimal(statsRow["AvgScore"]) : 0;

                    Label lblStats = new Label
                    {
                        Text = $"Tổng môn đã có điểm: {gradedCourses} | Môn đạt: {passedCourses} | Điểm TB: {avgScore:0.00} | GPA: {SessionManager.CurrentStudent.GPA:0.00}",
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        Location = new Point(20, 25),
                        AutoSize = true,
                        ForeColor = Color.FromArgb(52, 73, 94)
                    };
                    statsPanel.Controls.Add(lblStats);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thống kê: " + ex.Message);
            }

            panelContent.Controls.Add(statsPanel);

            // DataGridView for grades
            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 170),
                Size = new Size(panelContent.Width - 60, panelContent.Height - 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            string query = @"SELECT
                c.CourseCode as 'Mã môn',
                c.CourseName as 'Tên môn học',
                c.Credits as 'Tín chỉ',
                c.Semester as 'Học kỳ',
                ISNULL(g.MidtermScore, 0) as 'Điểm GK',
                ISNULL(g.FinalScore, 0) as 'Điểm CK',
                ISNULL(g.TotalScore, 0) as 'Điểm TB',
                ISNULL(g.LetterGrade, 'N/A') as 'Xếp loại',
                CASE
                    WHEN g.TotalScore >= 5.0 THEN N'Đạt'
                    WHEN g.TotalScore < 5.0 AND g.TotalScore > 0 THEN N'Không đạt'
                    ELSE N'Chưa có điểm'
                END as 'Kết quả'
                FROM Enrollments e
                INNER JOIN Courses c ON e.CourseId = c.CourseId
                LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
                WHERE e.StudentId = @StudentId
                ORDER BY c.Semester DESC, c.CourseCode";

            dgv.DataSource = DatabaseHelper.ExecuteQuery(query,
                new SqlParameter[] { new SqlParameter("@StudentId", SessionManager.CurrentStudent.StudentId) });

            // Color code the results
            dgv.CellFormatting += (s, e) =>
            {
                if (dgv.Columns[e.ColumnIndex].HeaderText == "Kết quả")
                {
                    string value = e.Value?.ToString();
                    if (value == "Đạt")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(46, 204, 113);
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                    }
                    else if (value == "Không đạt")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(231, 76, 60);
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.FromArgb(149, 165, 166);
                        e.CellStyle.ForeColor = Color.White;
                    }
                }
                else if (dgv.Columns[e.ColumnIndex].HeaderText == "Xếp loại")
                {
                    string value = e.Value?.ToString();
                    if (value == "A")
                        e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                    else if (value == "B")
                        e.CellStyle.ForeColor = Color.FromArgb(52, 152, 219);
                    else if (value == "C")
                        e.CellStyle.ForeColor = Color.FromArgb(241, 196, 15);
                    else if (value == "D" || value == "F")
                        e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                }
            };

            panelContent.Controls.Add(dgv);
        }

        private void LoadCourseRegistration()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "ĐĂNG KÝ MÔN HỌC",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            Label lblSemester = new Label
            {
                Text = "Chọn học kỳ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 70),
                AutoSize = true
            };
            panelContent.Controls.Add(lblSemester);

            ComboBox cboSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(140, 68),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            // Add common semesters
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year <= currentYear + 2; year++)
            {
                cboSemester.Items.Add($"HK1 {year}-{year + 1}");
                cboSemester.Items.Add($"HK2 {year}-{year + 1}");
                cboSemester.Items.Add($"HK3 {year}-{year + 1}");
            }
            panelContent.Controls.Add(cboSemester);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 120),
                Size = new Size(panelContent.Width - 60, panelContent.Height - 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            panelContent.Controls.Add(dgv);

            Button btnRegister = new Button
            {
                Text = "Đăng ký môn học đã chọn",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, panelContent.Height - 60),
                Size = new Size(200, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.Click += (s, e) => RegisterCourse(dgv);
            panelContent.Controls.Add(btnRegister);

            // Setup event handler BEFORE setting SelectedIndex
            cboSemester.SelectedIndexChanged += (s, e) =>
            {
                if (cboSemester.SelectedItem != null)
                {
                    LoadAvailableCourses(dgv, cboSemester.SelectedItem.ToString());
                }
            };

            // Set SelectedIndex AFTER adding dgv to form - this will trigger load
            cboSemester.SelectedIndex = 0;
        }

        private void LoadAvailableCourses(DataGridView dgv, string semester)
        {
            string query = @"SELECT c.CourseId, c.CourseCode, c.CourseName, c.Credits,
                            u.FullName as TeacherName, c.MaxStudents,
                            (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId AND Status = N'Enrolled') as Enrolled
                            FROM Courses c
                            LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                            LEFT JOIN Users u ON t.UserId = u.UserId
                            WHERE c.Semester = @Semester AND c.IsActive = 1
                            AND c.CourseName NOT IN (
                                SELECT c2.CourseName FROM Enrollments e
                                INNER JOIN Courses c2 ON e.CourseId = c2.CourseId
                                WHERE e.StudentId = @StudentId
                                AND e.Status = N'Enrolled'
                            )";

            dgv.DataSource = DatabaseHelper.ExecuteQuery(query,
                new SqlParameter[] {
                    new SqlParameter("@Semester", semester),
                    new SqlParameter("@StudentId", SessionManager.CurrentStudent.StudentId)
                });

            if (dgv.Columns.Count >= 7)
            {
                dgv.Columns[0].Visible = false; // Hide CourseId
                dgv.Columns[1].HeaderText = "Mã môn";
                dgv.Columns[2].HeaderText = "Tên môn học";
                dgv.Columns[3].HeaderText = "Tín chỉ";
                dgv.Columns[4].HeaderText = "Giảng viên";
                dgv.Columns[5].HeaderText = "Sĩ số max";
                dgv.Columns[6].HeaderText = "Đã đăng ký";
            }
        }

        private void RegisterCourse(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn môn học cần đăng ký!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int courseId = Convert.ToInt32(dgv.SelectedRows[0].Cells["CourseId"].Value);
                string courseName = dgv.SelectedRows[0].Cells[2].Value.ToString();
                int studentId = SessionManager.CurrentStudent.StudentId;

                DialogResult result = MessageBox.Show($"Bạn có chắc muốn đăng ký môn '{courseName}'?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Check if already enrolled (chỉ check các môn đang Enrolled, không check Cancelled)
                    string checkQuery = "SELECT COUNT(*) FROM Enrollments WHERE StudentId = @StudentId AND CourseId = @CourseId AND Status = N'Enrolled'";
                    object checkResult = DatabaseHelper.ExecuteScalar(checkQuery, new SqlParameter[]
                    {
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@CourseId", courseId)
                    });

                    if (Convert.ToInt32(checkResult) > 0)
                    {
                        MessageBox.Show("Bạn đã đăng ký môn học này rồi!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check course capacity
                    string capacityQuery = @"
                        SELECT c.MaxStudents, COUNT(e.EnrollmentId) as Enrolled
                        FROM Courses c
                        LEFT JOIN Enrollments e ON e.CourseId = c.CourseId AND e.Status = N'Enrolled'
                        WHERE c.CourseId = @CourseId
                        GROUP BY c.MaxStudents";

                    DataTable capacityData = DatabaseHelper.ExecuteQuery(capacityQuery, new SqlParameter[]
                    {
                        new SqlParameter("@CourseId", courseId)
                    });

                    if (capacityData.Rows.Count > 0)
                    {
                        int maxStudents = capacityData.Rows[0]["MaxStudents"] != DBNull.Value ? Convert.ToInt32(capacityData.Rows[0]["MaxStudents"]) : 0;
                        int enrolled = capacityData.Rows[0]["Enrolled"] != DBNull.Value ? Convert.ToInt32(capacityData.Rows[0]["Enrolled"]) : 0;

                        if (maxStudents > 0 && enrolled >= maxStudents)
                        {
                            MessageBox.Show("Môn học đã đủ sĩ số. Không thể đăng ký!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Check schedule conflicts
                    string conflictQuery = @"
                        SELECT COUNT(*)
                        FROM Enrollments e1
                        INNER JOIN Schedules s1 ON e1.CourseId = s1.CourseId
                        INNER JOIN Schedules s2 ON s1.DayOfWeek = s2.DayOfWeek AND s1.TimeSlot = s2.TimeSlot
                        WHERE e1.StudentId = @StudentId
                        AND e1.Status = N'Enrolled'
                        AND s2.CourseId = @CourseId";

                    object conflictResult = DatabaseHelper.ExecuteScalar(conflictQuery, new SqlParameter[]
                    {
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@CourseId", courseId)
                    });

                    if (Convert.ToInt32(conflictResult) > 0)
                    {
                        MessageBox.Show("Môn học này xung đột lịch với môn đã đăng ký!", "Lỗi xung đột lịch học",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Try to register via stored procedure to ensure atomic checks (capacity, conflict, duplicates)
                    int spResult = DatabaseHelper.ExecuteNonQueryStoredProcedure("sp_RegisterStudentToCourse", new SqlParameter[] {
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@CourseId", courseId)
                    });

                    if (spResult == 0)
                    {
                        MessageBox.Show("Đăng ký môn học thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (spResult == -1)
                    {
                        MessageBox.Show("Môn học đã đủ sĩ số. Không thể đăng ký!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else if (spResult == -2)
                    {
                        MessageBox.Show("Môn học này xung đột lịch với môn đã đăng ký!", "Lỗi xung đột lịch học",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else if (spResult == -3)
                    {
                        MessageBox.Show("Bạn đã đăng ký môn này trước đó.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Lỗi khi đăng ký môn học (mã lỗi: " + spResult + ")", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    LoadCourseRegistration(); // Reload
                }
            }
            catch (SqlException sex)
            {
                MessageBox.Show("Lỗi khi đăng ký môn: " + sex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi đăng ký",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSchedule()
        {
            panelContent.Controls.Clear();

            ScheduleForm scheduleForm = new ScheduleForm();
            scheduleForm.TopLevel = false;
            scheduleForm.FormBorderStyle = FormBorderStyle.None;
            scheduleForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(scheduleForm);
            scheduleForm.Show();
        }

        private void LoadPayment()
        {
            try
            {
                PaymentForm paymentForm = new PaymentForm();
                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    // Sau khi thanh toán thành công, hỏi có muốn xem lịch không
                    var result = MessageBox.Show("Bạn có muốn xem thời khóa biểu không?",
                        "Xem lịch học", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        LoadSchedule();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở form thanh toán: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProfile()
        {
            panelContent.Controls.Clear();

            StudentProfileForm profileForm = new StudentProfileForm();
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
