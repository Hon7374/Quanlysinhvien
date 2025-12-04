using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
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
        private DataTable scheduleData; // Store schedule data for conflict detection
        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        private readonly string[] timeSlots = {
            "07:00 - 07:50",  // Tiết 1 - Buổi sáng
            "07:55 - 08:45",  // Tiết 2
            "08:50 - 09:40",  // Tiết 3
            "09:50 - 10:40",  // Tiết 4
            "10:45 - 11:35",  // Tiết 5
            "12:30 - 13:20",  // Tiết 6 - Buổi chiều
            "13:25 - 14:15",  // Tiết 7
            "14:20 - 15:10",  // Tiết 8
            "15:20 - 16:10",  // Tiết 9
            "16:15 - 17:05",  // Tiết 10
            "17:30 - 18:20",  // Tiết 11 - Buổi tối
            "18:25 - 19:15",  // Tiết 12
            "19:20 - 20:10",  // Tiết 13
            "20:15 - 21:05"   // Tiết 14
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
                Size = new Size(130, 30),
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
            cboSemester.SelectedIndex = 0;
            cboSemester.SelectedIndexChanged += (s, e) => LoadSchedule();
            this.Controls.Add(cboSemester);

            // Week selector with DateTimePicker
            dtpWeekSelector = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(230, 68),
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

                // Check payment status before loading schedule
                if (!CheckPaymentStatus(studentId, semester))
                {
                    return; // Payment status check failed, don't load schedule
                }

                // Ensure Schedules table exists
                EnsureSchedulesTableExists();

                // Get enrolled courses with schedule info
                string query = @"
                    SELECT
                        c.CourseId,
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
                    AND e.Status = N'Enrolled'
                    AND e.PaymentStatus = @PaymentStatus";

                DataTable courses = DatabaseHelper.ExecuteQuery(query, new SqlParameter[]
                {
                    new SqlParameter("@StudentId", studentId),
                    new SqlParameter("@Semester", semester),
                    new SqlParameter("@PaymentStatus", StudentManagement.Helpers.AppConstants.PAYMENT_STATUS_PAID)
                });

                // Debug: Check if we have any data
                if (courses.Rows.Count == 0)
                {
                    // Check if student has ANY enrollments
                    string checkEnrollments = @"
                        SELECT COUNT(*)
                        FROM Enrollments e
                        INNER JOIN Courses c ON e.CourseId = c.CourseId
                        WHERE e.StudentId = @StudentId
                        AND c.Semester = @Semester
                        AND e.Status = N'Enrolled'
                        AND e.PaymentStatus = @PaymentStatus";

                    object enrollmentCount = DatabaseHelper.ExecuteScalar(checkEnrollments, new SqlParameter[]
                    {
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@Semester", semester),
                        new SqlParameter("@PaymentStatus", StudentManagement.Helpers.AppConstants.PAYMENT_STATUS_PAID)
                    });

                    if (Convert.ToInt32(enrollmentCount) == 0)
                    {
                        MessageBox.Show($"Bạn chưa đăng ký môn học nào trong học kỳ {semester}.\n\n" +
                                      "Vui lòng đăng ký môn học trước khi xem lịch.",
                                      "Thông báo",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Bạn đã đăng ký {enrollmentCount} môn học trong học kỳ {semester}, " +
                                      "nhưng các môn này chưa có lịch học.\n\n" +
                                      "Vui lòng liên hệ giáo vụ để được hỗ trợ.",
                                      "Thông báo",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);
                    }
                }

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
            // Store schedule data for conflict detection
            this.scheduleData = courses;

            int cellWidth = 180; // Increased for better readability
            int minCellHeight = 120; // Minimum height, will auto-expand
            int headerHeight = 50;
            int timeColumnWidth = 100;

            // Define 3 sessions: Morning, Afternoon, Evening
            string[] sessions = { "Sáng", "Chiều", "Tối" };
            int[] sessionStartSlots = { 0, 5, 10 }; // Start slot for each session
            int[] sessionEndSlots = { 4, 9, 13 };   // End slot for each session

            // Header row (days of week) - Modern Google Calendar style
            Label lblTime = new Label
            {
                Text = "Ca học",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(timeColumnWidth, headerHeight),
                BackColor = Color.FromArgb(26, 115, 232), // #1A73E8 - Google blue
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.None
            };
            panelWeek.Controls.Add(lblTime);

            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                DateTime dayDate = currentWeekStart.AddDays(i);
                Label lblDay = new Label
                {
                    Text = $"{daysOfWeek[i]}\n{dayDate:dd/MM/yyyy}",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(timeColumnWidth + (i * cellWidth), 0),
                    Size = new Size(cellWidth, headerHeight),
                    BackColor = Color.FromArgb(26, 115, 232), // #1A73E8
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.None,
                    Padding = new Padding(4)
                };
                panelWeek.Controls.Add(lblDay);
            }

            // Session rows (3 rows: Morning, Afternoon, Evening)
            int currentY = headerHeight;

            for (int sessionRow = 0; sessionRow < sessions.Length; sessionRow++)
            {
                // Count max courses in this session to calculate row height
                int maxCoursesInSession = 0;
                for (int col = 0; col < daysOfWeek.Length; col++)
                {
                    int courseCount = 0;
                    HashSet<string> seenCourses = new HashSet<string>();

                    foreach (DataRow course in courses.Rows)
                    {
                        if (course["DayOfWeek"] != DBNull.Value && course["TimeSlot"] != DBNull.Value && course["CourseCode"] != DBNull.Value)
                        {
                            int dayOfWeek = Convert.ToInt32(course["DayOfWeek"]);
                            int timeSlot = Convert.ToInt32(course["TimeSlot"]);
                            string courseCode = course["CourseCode"].ToString();

                            if (dayOfWeek == col &&
                                timeSlot >= sessionStartSlots[sessionRow] &&
                                timeSlot <= sessionEndSlots[sessionRow] &&
                                !seenCourses.Contains(courseCode))
                            {
                                courseCount++;
                                seenCourses.Add(courseCode);
                            }
                        }
                    }
                    maxCoursesInSession = Math.Max(maxCoursesInSession, courseCount);
                }

                // Calculate dynamic row height based on course count
                int rowHeight = Math.Max(minCellHeight, maxCoursesInSession * 148 + 20); // 148px per course (140 + 8 gap) + padding

                // Session label
                Label lblSession = new Label
                {
                    Text = sessions[sessionRow],
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(0, currentY),
                    Size = new Size(timeColumnWidth, rowHeight),
                    BackColor = Color.FromArgb(248, 249, 250),
                    ForeColor = Color.FromArgb(68, 68, 68), // #444
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.None
                };
                panelWeek.Controls.Add(lblSession);

                // Add subtle border
                Panel sessionBorder = new Panel
                {
                    Location = new Point(0, currentY + rowHeight - 1),
                    Size = new Size(timeColumnWidth, 1),
                    BackColor = Color.FromArgb(208, 215, 222) // #D0D7DE
                };
                panelWeek.Controls.Add(sessionBorder);

                // Day cells for this session
                for (int col = 0; col < daysOfWeek.Length; col++)
                {
                    Panel cell = new Panel
                    {
                        Location = new Point(timeColumnWidth + (col * cellWidth), currentY),
                        Size = new Size(cellWidth, rowHeight),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.None,
                        AutoScroll = false, // We'll handle overflow with dynamic height
                        Padding = new Padding(8)
                    };

                    // Add cell border
                    Panel cellBorder = new Panel
                    {
                        Location = new Point(0, rowHeight - 1),
                        Size = new Size(cellWidth, 1),
                        BackColor = Color.FromArgb(208, 215, 222) // #D0D7DE
                    };
                    cell.Controls.Add(cellBorder);

                    // Add all courses in this session - group consecutive time slots
                    int courseYOffset = 0;
                    HashSet<string> processedCourses = new HashSet<string>(); // Track which courses we've already added (use CourseCode as key)

                    foreach (DataRow course in courses.Rows)
                    {
                        if (course["DayOfWeek"] != DBNull.Value && course["TimeSlot"] != DBNull.Value && course["CourseCode"] != DBNull.Value)
                        {
                            int dayOfWeek = Convert.ToInt32(course["DayOfWeek"]);
                            int timeSlot = Convert.ToInt32(course["TimeSlot"]);
                            string courseCode = course["CourseCode"].ToString();

                            // Check if this course is in the current session and not yet processed
                            if (dayOfWeek == col &&
                                timeSlot >= sessionStartSlots[sessionRow] &&
                                timeSlot <= sessionEndSlots[sessionRow] &&
                                !processedCourses.Contains(courseCode))
                            {
                                // Find all consecutive time slots for this course on this day
                                List<DataRow> consecutiveSlots = new List<DataRow>();
                                int currentSlot = timeSlot;

                                while (true)
                                {
                                    DataRow[] matches = courses.Select($"CourseCode = '{courseCode}' AND DayOfWeek = {dayOfWeek} AND TimeSlot = {currentSlot}");
                                    if (matches.Length > 0)
                                    {
                                        consecutiveSlots.Add(matches[0]);
                                        currentSlot++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                // Add merged course block
                                if (consecutiveSlots.Count > 0)
                                {
                                    AddMergedCourseToCell(cell, consecutiveSlots, courseYOffset);
                                    courseYOffset += 148; // 140px card height + 8px gap
                                    processedCourses.Add(courseCode);
                                }
                            }
                        }
                    }

                    cell.Click += Cell_Click;
                    panelWeek.Controls.Add(cell);
                }

                currentY += rowHeight; // Move to next session row
            }

            // Add legend
            CreateLegend();
        }

        private void AddCourseToCell(Panel cell, DataRow course, int yOffset)
        {
            // Determine time slot / time range / room first
            string slotText = course.Table.Columns.Contains("TimeSlot") && course["TimeSlot"] != DBNull.Value ? $"Tiết: {Convert.ToInt32(course["TimeSlot"]) + 1}" : string.Empty;
            string startTime = null;
            string endTime = null;
            if (course.Table.Columns.Contains("StartTime") && course["StartTime"] != DBNull.Value)
            {
                TimeSpan ts = (TimeSpan)course["StartTime"];
                startTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }
            if (course.Table.Columns.Contains("EndTime") && course["EndTime"] != DBNull.Value)
            {
                TimeSpan ts = (TimeSpan)course["EndTime"];
                endTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }
            string timeRange = !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime) ? $"{startTime} - {endTime}" : string.Empty;
            string roomText = course.Table.Columns.Contains("Room") && course["Room"] != DBNull.Value ? course["Room"].ToString() : "Chưa có";

            // Create a modern course card with rounded corners and shadow
            Panel coursePanel = new Panel
            {
                Location = new Point(8, yOffset), // 8px left margin
                Size = new Size(cell.Width - 20, 140), // Increased height for word wrap, 10px margin on each side
                BackColor = Color.FromArgb(231, 241, 255), // #E7F1FF - Light blue
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12), // Increased padding to 12px
                AutoSize = false
            };

            // Simulate rounded corners with Paint event
            coursePanel.Paint += (s, e) =>
            {
                using (var path = GetRoundedRectanglePath(new Rectangle(0, 0, coursePanel.Width - 1, coursePanel.Height - 1), 8))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // Fill background
                    using (var brush = new SolidBrush(Color.FromArgb(231, 241, 255))) // #E7F1FF
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    // Draw border
                    using (var pen = new Pen(Color.FromArgb(167, 199, 231), 1)) // #A7C7E7
                    {
                        e.Graphics.DrawPath(pen, path);
                    }

                    // Draw subtle shadow effect
                    using (var shadowPen = new Pen(Color.FromArgb(20, 0, 0, 0), 2))
                    {
                        e.Graphics.DrawPath(shadowPen, path);
                    }
                }
            };

            // Course code and name - bold, prominent (13-14px as per spec) with WORD WRAP
            Label lblCourseHeader = new Label
            {
                Text = $"{course["CourseCode"]} – {course["CourseName"]}",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), // 13-14px
                Location = new Point(12, 10),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 45), // Increased height for wrap
                ForeColor = Color.FromArgb(0, 51, 102), // #003366 - Dark blue
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };

            // Teacher info
            Label lblTeacher = new Label
            {
                Text = "GV: " + (course["TeacherName"] != DBNull.Value ? course["TeacherName"].ToString() : "TBA"),
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 58),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(68, 68, 68), // #444
                BackColor = Color.Transparent
            };

            // Time slot info
            Label lblTimeSlot = new Label
            {
                Text = slotText,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 79),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(68, 68, 68), // #444
                BackColor = Color.Transparent
            };

            // Time range info
            Label lblTimeRange = new Label
            {
                Text = "Giờ: " + timeRange,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 100),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(68, 68, 68), // #444
                BackColor = Color.Transparent
            };

            // Location info
            Label lblLocation = new Label
            {
                Text = "Địa điểm: " + roomText,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 121),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(0, 102, 204), // #0066CC - Blue for location
                BackColor = Color.Transparent
            };

            // Add all labels to coursePanel
            coursePanel.Controls.Add(lblCourseHeader);
            coursePanel.Controls.Add(lblTeacher);
            coursePanel.Controls.Add(lblTimeSlot);
            coursePanel.Controls.Add(lblTimeRange);
            coursePanel.Controls.Add(lblLocation);

            // Ensure clicking labels will trigger the course panel click
            lblCourseHeader.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTeacher.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTimeSlot.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTimeRange.Click += (s, e) => Cell_Click(coursePanel, e);
            lblLocation.Click += (s, e) => Cell_Click(coursePanel, e);

            // Store course data and attach click handler to individual course panel
            coursePanel.Tag = course;
            coursePanel.Click += Cell_Click;

            // Add the course panel into the cell so multiple course panels can be stacked
            cell.Controls.Add(coursePanel);
        }

        private void AddMergedCourseToCell(Panel cell, List<DataRow> consecutiveSlots, int yOffset)
        {
            if (consecutiveSlots.Count == 0) return;

            DataRow firstSlot = consecutiveSlots[0];
            DataRow lastSlot = consecutiveSlots[consecutiveSlots.Count - 1];

            // Check for session boundary violations
            int startSlotIndex = Convert.ToInt32(firstSlot["TimeSlot"]);
            int endSlotIndex = Convert.ToInt32(lastSlot["TimeSlot"]);
            int dayOfWeek = Convert.ToInt32(firstSlot["DayOfWeek"]);
            bool hasSessionViolation = !Helpers.ScheduleValidator.ValidateSessionRange(startSlotIndex, endSlotIndex, out string _);

            // Check for time conflicts with other courses at the same time
            bool hasTimeConflict = false;
            if (!hasSessionViolation && scheduleData != null)
            {
                var conflicts = Helpers.ScheduleValidator.DetectConflicts(
                    dayOfWeek,
                    startSlotIndex,
                    endSlotIndex,
                    scheduleData,
                    Convert.ToInt32(firstSlot["CourseId"])
                );
                hasTimeConflict = conflicts.Count > 0;
            }

            bool hasConflict = hasSessionViolation || hasTimeConflict;

            // Create a modern course card with rounded corners and shadow
            // Auto-height based on content with proper margins
            Panel coursePanel = new Panel
            {
                Location = new Point(8, yOffset), // 8px left margin
                Size = new Size(cell.Width - 20, 140), // Increased height for word wrap, 10px margin on each side
                BackColor = hasConflict ? Color.FromArgb(254, 226, 226) : Color.FromArgb(231, 241, 255), // Red tint if conflict, else #E7F1FF
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12), // Increased padding to 12px
                AutoSize = false
            };

            // Simulate rounded corners with Paint event
            coursePanel.Paint += (s, e) =>
            {
                using (var path = GetRoundedRectanglePath(new Rectangle(0, 0, coursePanel.Width - 1, coursePanel.Height - 1), 8))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // Fill background
                    Color bgColor = hasConflict ? Color.FromArgb(254, 226, 226) : Color.FromArgb(231, 241, 255); // Red tint if conflict
                    using (var brush = new SolidBrush(bgColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    // Draw border - red if conflict, normal blue otherwise
                    Color borderColor = hasConflict ? Color.FromArgb(220, 38, 38) : Color.FromArgb(167, 199, 231); // Red or #A7C7E7
                    int borderWidth = hasConflict ? 2 : 1; // Thicker border for conflicts
                    using (var pen = new Pen(borderColor, borderWidth))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }

                    // Draw subtle shadow effect
                    using (var shadowPen = new Pen(Color.FromArgb(20, 0, 0, 0), 2))
                    {
                        e.Graphics.DrawPath(shadowPen, path);
                    }
                }
            };

            // Determine time slot range and time range first
            int startSlot = Convert.ToInt32(firstSlot["TimeSlot"]) + 1;
            int endSlot = Convert.ToInt32(lastSlot["TimeSlot"]) + 1;
            string slotText = consecutiveSlots.Count > 1 ? $"Tiết: {startSlot}-{endSlot}" : $"Tiết: {startSlot}";

            string startTime = null;
            string endTime = null;
            if (firstSlot.Table.Columns.Contains("StartTime") && firstSlot["StartTime"] != DBNull.Value)
            {
                TimeSpan ts = (TimeSpan)firstSlot["StartTime"];
                startTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }
            if (lastSlot.Table.Columns.Contains("EndTime") && lastSlot["EndTime"] != DBNull.Value)
            {
                TimeSpan ts = (TimeSpan)lastSlot["EndTime"];
                endTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }
            string timeRange = !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime) ? $"{startTime} - {endTime}" : string.Empty;
            string roomText = firstSlot.Table.Columns.Contains("Room") && firstSlot["Room"] != DBNull.Value ? firstSlot["Room"].ToString() : "Chưa có";

            // Course code and name - bold, prominent (13-14px as per spec) with WORD WRAP
            Label lblCourseHeader = new Label
            {
                Text = $"{firstSlot["CourseCode"]} – {firstSlot["CourseName"]}",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), // 13-14px
                Location = new Point(12, 10),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 45), // Increased height for wrap
                ForeColor = Color.FromArgb(0, 51, 102), // #003366 - Dark blue
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };

            // Teacher info
            Label lblTeacher = new Label
            {
                Text = "GV: " + (firstSlot["TeacherName"] != DBNull.Value ? firstSlot["TeacherName"].ToString() : "TBA"),
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 58),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(68, 68, 68), // #444
                BackColor = Color.Transparent
            };

            // Time slot info
            Label lblTimeSlot = new Label
            {
                Text = slotText,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 79),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(68, 68, 68), // #444
                BackColor = Color.Transparent
            };

            // Time range info
            Label lblTimeRange = new Label
            {
                Text = "Giờ: " + timeRange,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 100),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(68, 68, 68), // #444
                BackColor = Color.Transparent
            };

            // Location info with icon
            Label lblLocation = new Label
            {
                Text = "Địa điểm: " + roomText,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(12, 121),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 24, 18),
                ForeColor = Color.FromArgb(0, 102, 204), // #0066CC - Blue for location
                BackColor = Color.Transparent
            };

            // Add conflict warning icon if needed
            if (hasConflict)
            {
                Label lblWarningIcon = new Label
                {
                    Text = "⚠",
                    Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                    Location = new Point(coursePanel.Width - 35, 8),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(220, 38, 38), // Red
                    BackColor = Color.Transparent
                };

                string conflictMessage = hasSessionViolation
                    ? "Lịch học này vượt qua ranh giới ca học (Sáng/Chiều/Tối)"
                    : "Lịch học này trùng với môn khác";

                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(lblWarningIcon, conflictMessage);
                tooltip.SetToolTip(coursePanel, conflictMessage);

                coursePanel.Controls.Add(lblWarningIcon);
            }

            // Add all labels to coursePanel
            coursePanel.Controls.Add(lblCourseHeader);
            coursePanel.Controls.Add(lblTeacher);
            coursePanel.Controls.Add(lblTimeSlot);
            coursePanel.Controls.Add(lblTimeRange);
            coursePanel.Controls.Add(lblLocation);

            // Ensure clicking labels will trigger the course panel click
            lblCourseHeader.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTeacher.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTimeSlot.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTimeRange.Click += (s, e) => Cell_Click(coursePanel, e);
            lblLocation.Click += (s, e) => Cell_Click(coursePanel, e);

            // Store course data and attach click handler to individual course panel
            coursePanel.Tag = firstSlot;
            coursePanel.Click += Cell_Click;

            // Add the course panel into the cell so multiple course panels can be stacked
            cell.Controls.Add(coursePanel);
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

        /// <summary>
        /// Creates a rounded rectangle path for modern UI design
        /// </summary>
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;

            // Top-left corner
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Top-right corner
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Bottom-right corner
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Bottom-left corner
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Check if student has paid for all enrolled courses in the semester
        /// </summary>
        private bool CheckPaymentStatus(int studentId, string semester)
        {
            try
            {
                // Parse semester to get academic year
                string[] semesterParts = semester.Split(' ');
                if (semesterParts.Length < 2)
                {
                    MessageBox.Show("Định dạng học kỳ không hợp lệ!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                string semesterCode = semesterParts[0]; // HK1, HK2, HK3
                string yearRange = semesterParts[1]; // 2024-2025
                int academicYear = int.Parse(yearRange.Split('-')[0]);
                // Construct semester full string (HKx YYYY-YYYY) for comparison
                string semesterFull = semesterParts.Length >= 2 ? semester : $"{semesterCode} {academicYear}-{academicYear + 1}";

                // Check if student has any unpaid courses in this semester
                string checkQuery = @"
                    SELECT COUNT(*) as UnpaidCount
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.CourseId = c.CourseId
                    WHERE e.StudentId = @StudentId
                    AND c.Semester = @Semester
                    AND c.AcademicYear = @AcademicYear
                    AND e.Status = N'Enrolled'
                    AND e.PaymentStatus = @PaymentStatus";

                object result = DatabaseHelper.ExecuteScalar(checkQuery, new SqlParameter[]
                {
                    new SqlParameter("@StudentId", studentId),
                    new SqlParameter("@Semester", semesterFull),
                    new SqlParameter("@AcademicYear", academicYear),
                    new SqlParameter("@PaymentStatus", StudentManagement.Helpers.AppConstants.PAYMENT_STATUS_UNPAID)
                });

                int unpaidCount = result != null ? Convert.ToInt32(result) : 0;

                if (unpaidCount > 0)
                {
                    // Show payment required message
                    ShowPaymentRequiredMessage(unpaidCount, semester);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kiểm tra trạng thái thanh toán: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Display payment required message with option to pay now
        /// </summary>
        private void ShowPaymentRequiredMessage(int unpaidCount, string semester)
        {
            panelWeek.Controls.Clear();

            // Create a centered panel for the message
            Panel messagePanel = new Panel
            {
                Size = new Size(600, 400),
                BackColor = Color.FromArgb(252, 248, 227),
                BorderStyle = BorderStyle.None
            };
            messagePanel.Location = new Point(
                (panelWeek.Width - messagePanel.Width) / 2,
                (panelWeek.Height - messagePanel.Height) / 2
            );

            // Warning icon
            Label lblIcon = new Label
            {
                Text = "⚠️",
                Font = new Font("Segoe UI", 48),
                Location = new Point((messagePanel.Width - 80) / 2, 30),
                Size = new Size(80, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            messagePanel.Controls.Add(lblIcon);

            // Title
            Label lblTitle = new Label
            {
                Text = "YÊU CẦU THANH TOÁN HỌC PHÍ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(50, 120),
                Size = new Size(500, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(180, 83, 9)
            };
            messagePanel.Controls.Add(lblTitle);

            // Message
            Label lblMessage = new Label
            {
                Text = $"Bạn có {unpaidCount} môn học chưa thanh toán trong học kỳ {semester}.\n\n" +
                       "Vui lòng hoàn tất thanh toán học phí để xem thời khóa biểu.\n\n" +
                       "Nhấn nút bên dưới để thanh toán ngay.",
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 165),
                Size = new Size(500, 120),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(120, 53, 15)
            };
            messagePanel.Controls.Add(lblMessage);

            // Pay Now button
            Button btnPayNow = new Button
            {
                Text = "💳 Thanh toán ngay",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(175, 300),
                Size = new Size(250, 50),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPayNow.FlatAppearance.BorderSize = 0;
            btnPayNow.Click += (s, e) =>
            {
                // Open Payment Form
                PaymentForm paymentForm = new PaymentForm();
                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh schedule after successful payment
                    LoadSchedule();
                }
            };
            messagePanel.Controls.Add(btnPayNow);

            panelWeek.Controls.Add(messagePanel);
        }
    }
}
