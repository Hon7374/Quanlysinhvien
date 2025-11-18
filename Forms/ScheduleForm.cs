using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    public partial class ScheduleForm : Form
    {
        private Panel panelWeek;
        private ComboBox cboSemester;
        private Label lblCurrentWeek;
        private DateTimePicker dtpWeekSelector;
        private DateTime currentWeekStart;
        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        private readonly string[] timeSlots = {
            "07:00 - 08:30",
            "08:45 - 10:15",
            "10:30 - 12:00",
            "13:00 - 14:30",
            "14:45 - 16:15",
            "16:30 - 18:00"
        };

        public ScheduleForm()
        {
            InitializeComponent();
            LoadSchedule();
        }

        private void InitializeComponent()
        {
            this.Text = "Lịch học";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Title
            Label lblTitle = new Label
            {
                Text = "LỊCH HỌC CỦA TÔI",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            this.Controls.Add(lblTitle);

            // Semester selector
            Label lblSemester = new Label
            {
                Text = "Học kỳ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 70),
                AutoSize = true
            };
            this.Controls.Add(lblSemester);

            cboSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(90, 68),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboSemester.Items.AddRange(new object[] { "HK1", "HK2", "HK3" });
            cboSemester.SelectedIndex = 0;
            cboSemester.SelectedIndexChanged += (s, e) => LoadSchedule();
            this.Controls.Add(cboSemester);

            // Week selector with DateTimePicker
            dtpWeekSelector = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(260, 68),
                Size = new Size(150, 30),
                Format = DateTimePickerFormat.Short
            };
            dtpWeekSelector.ValueChanged += (s, e) =>
            {
                currentWeekStart = GetWeekStart(dtpWeekSelector.Value);
                UpdateWeekLabel();
                LoadSchedule();
            };
            this.Controls.Add(dtpWeekSelector);

            // Previous Week Button
            Button btnPrevWeek = new Button
            {
                Text = "◀ Trở về",
                Font = new Font("Segoe UI", 9),
                Location = new Point(420, 68),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPrevWeek.FlatAppearance.BorderSize = 0;
            btnPrevWeek.Click += (s, e) =>
            {
                currentWeekStart = currentWeekStart.AddDays(-7);
                dtpWeekSelector.Value = currentWeekStart;
            };
            this.Controls.Add(btnPrevWeek);

            // Current Week Button
            Button btnCurrentWeek = new Button
            {
                Text = "📅 Hiện tại",
                Font = new Font("Segoe UI", 9),
                Location = new Point(510, 68),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCurrentWeek.FlatAppearance.BorderSize = 0;
            btnCurrentWeek.Click += (s, e) =>
            {
                currentWeekStart = GetWeekStart(DateTime.Now);
                dtpWeekSelector.Value = DateTime.Now;
            };
            this.Controls.Add(btnCurrentWeek);

            // Next Week Button
            Button btnNextWeek = new Button
            {
                Text = "Tiếp ▶",
                Font = new Font("Segoe UI", 9),
                Location = new Point(610, 68),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNextWeek.FlatAppearance.BorderSize = 0;
            btnNextWeek.Click += (s, e) =>
            {
                currentWeekStart = currentWeekStart.AddDays(7);
                dtpWeekSelector.Value = currentWeekStart;
            };
            this.Controls.Add(btnNextWeek);

            // Current week label
            lblCurrentWeek = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(700, 70),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            this.Controls.Add(lblCurrentWeek);

            // Initialize current week
            currentWeekStart = GetWeekStart(DateTime.Now);
            UpdateWeekLabel();

            // Week view panel
            panelWeek = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                AutoScroll = true
            };
            this.Controls.Add(panelWeek);
        }

        private string GetCurrentWeek()
        {
            DateTime now = DateTime.Now;
            int weekNumber = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                now,
                System.Globalization.CalendarWeekRule.FirstDay,
                DayOfWeek.Monday
            );
            return $"Tuần {weekNumber}";
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void UpdateWeekLabel()
        {
            DateTime weekEnd = currentWeekStart.AddDays(6);
            lblCurrentWeek.Text = $"{currentWeekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}";
        }

        private void LoadSchedule()
        {
            panelWeek.Controls.Clear();

            try
            {
                int studentId = SessionManager.CurrentStudent.StudentId;
                string semester = cboSemester.SelectedItem.ToString();

                // Ensure Schedules table exists
                EnsureSchedulesTableExists();

                // Get enrolled courses with schedule info
                string query = @"
                    SELECT
                        c.CourseCode,
                        c.CourseName,
                        c.Credits,
                        u.FullName as TeacherName,
                        e.EnrollmentId,
                        s.DayOfWeek,
                        s.TimeSlot,
                        s.Room,
                        s.StartTime,
                        s.EndTime
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.CourseId = c.CourseId
                    LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                    LEFT JOIN Users u ON t.UserId = u.UserId
                    LEFT JOIN Schedules s ON c.CourseId = s.CourseId
                    WHERE e.StudentId = @StudentId
                    AND c.Semester = @Semester
                    AND e.Status = N'Đang học'";

                DataTable courses = DatabaseHelper.ExecuteQuery(query, new SqlParameter[]
                {
                    new SqlParameter("@StudentId", studentId),
                    new SqlParameter("@Semester", semester)
                });

                // Create schedule grid
                CreateScheduleGrid(courses);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateScheduleGrid(DataTable courses)
        {
            int cellWidth = 130;
            int cellHeight = 80;
            int headerHeight = 40;
            int timeColumnWidth = 100;

            // Header row (days of week)
            Label lblTime = new Label
            {
                Text = "Giờ học",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(timeColumnWidth, headerHeight),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelWeek.Controls.Add(lblTime);

            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                DateTime dayDate = currentWeekStart.AddDays(i);
                Label lblDay = new Label
                {
                    Text = $"{daysOfWeek[i]}\n{dayDate:dd/MM/yyyy}",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new Point(timeColumnWidth + (i * cellWidth), 0),
                    Size = new Size(cellWidth, headerHeight),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle
                };
                panelWeek.Controls.Add(lblDay);
            }

            // Time slots and schedule cells
            for (int row = 0; row < timeSlots.Length; row++)
            {
                // Time slot label
                Label lblTimeSlot = new Label
                {
                    Text = timeSlots[row],
                    Font = new Font("Segoe UI", 8),
                    Location = new Point(0, headerHeight + (row * cellHeight)),
                    Size = new Size(timeColumnWidth, cellHeight),
                    BackColor = Color.FromArgb(236, 240, 241),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle
                };
                panelWeek.Controls.Add(lblTimeSlot);

                // Day cells
                for (int col = 0; col < daysOfWeek.Length; col++)
                {
                    Panel cell = new Panel
                    {
                        Location = new Point(timeColumnWidth + (col * cellWidth), headerHeight + (row * cellHeight)),
                        Size = new Size(cellWidth, cellHeight),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = $"{col}_{row}" // Store day and time slot
                    };

                    // Add course info if exists
                    foreach (DataRow course in courses.Rows)
                    {
                        if (course["DayOfWeek"] != DBNull.Value && course["TimeSlot"] != DBNull.Value)
                        {
                            int dayOfWeek = Convert.ToInt32(course["DayOfWeek"]);
                            int timeSlot = Convert.ToInt32(course["TimeSlot"]);

                            if (dayOfWeek == col && timeSlot == row)
                            {
                                AddCourseToCell(cell, course);
                                break;
                            }
                        }
                    }

                    cell.Click += Cell_Click;
                    panelWeek.Controls.Add(cell);
                }
            }

            // Add legend
            CreateLegend();
        }

        private void AddCourseToCell(Panel cell, DataRow course)
        {
            cell.BackColor = Color.FromArgb(52, 152, 219);
            cell.Cursor = Cursors.Hand;

            Label lblCourseCode = new Label
            {
                Text = course["CourseCode"].ToString(),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(5, 5),
                AutoSize = false,
                Size = new Size(cell.Width - 10, 15),
                ForeColor = Color.White
            };

            Label lblCourseName = new Label
            {
                Text = course["CourseName"].ToString(),
                Font = new Font("Segoe UI", 7),
                Location = new Point(5, 22),
                AutoSize = false,
                Size = new Size(cell.Width - 10, 30),
                ForeColor = Color.White
            };

            Label lblTeacher = new Label
            {
                Text = "GV: " + (course["TeacherName"] != DBNull.Value ? course["TeacherName"].ToString() : "TBA"),
                Font = new Font("Segoe UI", 7, FontStyle.Italic),
                Location = new Point(5, 55),
                AutoSize = false,
                Size = new Size(cell.Width - 10, 15),
                ForeColor = Color.White
            };

            cell.Controls.Add(lblCourseCode);
            cell.Controls.Add(lblCourseName);
            cell.Controls.Add(lblTeacher);

            // Store course data
            cell.Tag = course;
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            Panel cell = sender as Panel;
            if (cell?.Tag is DataRow course)
            {
                string room = course["Room"] != DBNull.Value ? course["Room"].ToString() : "Chưa có";
                string message = $"Môn học: {course["CourseName"]}\n" +
                               $"Mã môn: {course["CourseCode"]}\n" +
                               $"Số tín chỉ: {course["Credits"]}\n" +
                               $"Giảng viên: {(course["TeacherName"] != DBNull.Value ? course["TeacherName"].ToString() : "Chưa có")}\n" +
                               $"Phòng học: {room}";

                MessageBox.Show(message, "Chi tiết môn học",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void EnsureSchedulesTableExists()
        {
            try
            {
                // Check if Schedules table exists
                string checkTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Schedules')
                    BEGIN
                        CREATE TABLE Schedules (
                            ScheduleId INT PRIMARY KEY IDENTITY(1,1),
                            CourseId INT NOT NULL,
                            DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6),
                            TimeSlot INT NOT NULL CHECK (TimeSlot BETWEEN 0 AND 5),
                            Room NVARCHAR(50),
                            StartTime TIME NOT NULL,
                            EndTime TIME NOT NULL,
                            CreatedAt DATETIME DEFAULT GETDATE(),
                            CONSTRAINT FK_Schedules_Courses FOREIGN KEY (CourseId)
                                REFERENCES Courses(CourseId) ON DELETE CASCADE,
                            CONSTRAINT UQ_Schedule UNIQUE(CourseId, DayOfWeek, TimeSlot)
                        )
                    END";

                DatabaseHelper.ExecuteNonQuery(checkTable);
            }
            catch (Exception ex)
            {
                // Table creation failed, but we can continue
                System.Diagnostics.Debug.WriteLine($"Error creating Schedules table: {ex.Message}");
            }
        }

        private void CreateLegend()
        {
            Panel legendPanel = new Panel
            {
                Location = new Point(20, 670),
                Size = new Size(300, 30),
                BackColor = Color.Transparent
            };

            Label lblLegend = new Label
            {
                Text = "■ Có lịch học",
                Font = new Font("Segoe UI", 9),
                Location = new Point(0, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            legendPanel.Controls.Add(lblLegend);
            this.Controls.Add(legendPanel);
        }
    }
}
