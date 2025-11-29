using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class ScheduleGridViewForm : Form
    {
        private Panel panelGrid;
        private ComboBox cboSemester;
        private string selectedSemester;
        private DateTime currentWeekStart;
        private Label lblCurrentWeek;
        private DataTable allSchedules; // Store all schedules for conflict detection

        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        private readonly string[] timeSlots = {
            "07:00\n07:50",  // Tiết 1 - Buổi sáng
            "07:55\n08:45",  // Tiết 2
            "08:50\n09:40",  // Tiết 3
            "09:50\n10:40",  // Tiết 4
            "10:45\n11:35",  // Tiết 5
            "12:30\n13:20",  // Tiết 6 - Buổi chiều
            "13:25\n14:15",  // Tiết 7
            "14:20\n15:10",  // Tiết 8
            "15:20\n16:10",  // Tiết 9
            "16:15\n17:05",  // Tiết 10
            "17:30\n18:20",  // Tiết 11 - Buổi tối
            "18:25\n19:15",  // Tiết 12
            "19:20\n20:10",  // Tiết 13
            "20:15\n21:05"   // Tiết 14
        };

        private class ScheduleBlock
        {
            public DataRow Row { get; set; }
            public int StartSlot { get; set; }
            public int EndSlot { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }

        public ScheduleGridViewForm(string semester = null)
        {
            this.selectedSemester = semester;
            this.currentWeekStart = GetWeekStart(DateTime.Now);
            InitializeComponent();
            LoadSemesters();
            LoadScheduleGrid();
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void UpdateWeekLabel()
        {
            if (lblCurrentWeek != null)
            {
                DateTime weekEnd = currentWeekStart.AddDays(6);
                lblCurrentWeek.Text = $"{currentWeekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}";
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Lưới Thời khóa biểu - Schedule Grid View";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Title
            Label lblTitle = new Label
            {
                Text = "LƯỚI THỜI KHÓA BIỂU",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            this.Controls.Add(lblTitle);

            // Semester Filter
            Label lblSemester = new Label
            {
                Text = "Học kỳ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 70),
                AutoSize = true
            };
            this.Controls.Add(lblSemester);

            cboSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(100, 68),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboSemester.SelectedIndexChanged += (s, e) => LoadScheduleGrid();
            this.Controls.Add(cboSemester);

            // Week navigation
            Button btnPrevWeek = new Button
            {
                Text = "◀ Tuần trước",
                Font = new Font("Segoe UI", 9),
                Location = new Point(320, 65),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPrevWeek.FlatAppearance.BorderSize = 0;
            btnPrevWeek.Click += (s, e) =>
            {
                currentWeekStart = currentWeekStart.AddDays(-7);
                UpdateWeekLabel();
                LoadScheduleGrid();
            };
            this.Controls.Add(btnPrevWeek);

            lblCurrentWeek = new Label
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(430, 70),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(99, 102, 241)
            };
            UpdateWeekLabel();
            this.Controls.Add(lblCurrentWeek);

            Button btnNextWeek = new Button
            {
                Text = "Tuần sau ▶",
                Font = new Font("Segoe UI", 9),
                Location = new Point(640, 65),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNextWeek.FlatAppearance.BorderSize = 0;
            btnNextWeek.Click += (s, e) =>
            {
                currentWeekStart = currentWeekStart.AddDays(7);
                UpdateWeekLabel();
                LoadScheduleGrid();
            };
            this.Controls.Add(btnNextWeek);

            Button btnToday = new Button
            {
                Text = "📅 Hôm nay",
                Font = new Font("Segoe UI", 9),
                Location = new Point(750, 65),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnToday.FlatAppearance.BorderSize = 0;
            btnToday.Click += (s, e) =>
            {
                currentWeekStart = GetWeekStart(DateTime.Now);
                UpdateWeekLabel();
                LoadScheduleGrid();
            };
            this.Controls.Add(btnToday);

            Button btnClose = new Button
            {
                Text = "Đóng",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(1050, 65),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(156, 163, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnClose);

            // Grid Panel
            panelGrid = new Panel
            {
                Location = new Point(30, 120),
                Size = new Size(1120, 560),
                BackColor = Color.White,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelGrid);
        }

        private void LoadSemesters()
        {
            try
            {
                cboSemester.Items.Clear();
                cboSemester.Items.Add("-- Tất cả học kỳ --");

                string query = "SELECT DISTINCT Semester FROM Courses WHERE IsActive = 1 ORDER BY Semester DESC";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    cboSemester.Items.Add(row["Semester"].ToString());
                }

                if (!string.IsNullOrEmpty(selectedSemester))
                {
                    int index = cboSemester.Items.IndexOf(selectedSemester);
                    if (index >= 0)
                        cboSemester.SelectedIndex = index;
                    else
                        cboSemester.SelectedIndex = 0;
                }
                else
                {
                    cboSemester.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách học kỳ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadScheduleGrid()
        {
            panelGrid.Controls.Clear();

            try
            {
                // Get all schedules
                string query = @"
                    SELECT
                        s.CourseId,
                        s.DayOfWeek,
                        s.TimeSlot,
                        c.CourseCode,
                        c.CourseName,
                        s.StartTime,
                        s.EndTime,
                        s.Room,
                        u.FullName as TeacherName
                    FROM Schedules s
                    INNER JOIN Courses c ON s.CourseId = c.CourseId
                    LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                    LEFT JOIN Users u ON t.UserId = u.UserId
                    WHERE 1=1";

                if (cboSemester.SelectedIndex > 0)
                {
                    query += " AND c.Semester = @Semester";
                }

                query += " ORDER BY s.DayOfWeek, s.TimeSlot";

                SqlParameter[] parameters = cboSemester.SelectedIndex > 0 ?
                    new SqlParameter[] { new SqlParameter("@Semester", cboSemester.SelectedItem.ToString()) } :
                    new SqlParameter[] { };

                DataTable schedules = DatabaseHelper.ExecuteQuery(query, parameters);

                CreateGrid(schedules);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lưới thời khóa biểu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateGrid(DataTable schedules)
        {
            // Store schedules for conflict detection
            this.allSchedules = schedules;

            int cellWidth = 180; // Increased for better readability
            int minCellHeight = 120;
            int headerHeight = 50;
            int timeColumnWidth = 120;

            // Define 3 sessions: Morning, Afternoon, Evening
            string[] sessions = { "Sáng", "Chiều", "Tối" };
            int[] sessionStartSlots = { 0, 5, 10 }; // Start slot for each session
            int[] sessionEndSlots = { 4, 9, 13 };   // End slot for each session

            // Header row (Time column) - Modern Google blue
            Label lblTimeHeader = new Label
            {
                Text = "Ca học",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(timeColumnWidth, headerHeight),
                BackColor = Color.FromArgb(26, 115, 232), // #1A73E8 - Google blue
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelGrid.Controls.Add(lblTimeHeader);

            // Day headers with dates - Modern Google blue
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                DateTime dayDate = currentWeekStart.AddDays(i);
                Label lblDay = new Label
                {
                    Text = $"{daysOfWeek[i]}\n{dayDate:dd/MM/yyyy}",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new Point(timeColumnWidth + (i * cellWidth), 0),
                    Size = new Size(cellWidth, headerHeight),
                    BackColor = Color.FromArgb(26, 115, 232), // #1A73E8 - Google blue
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle
                };
                panelGrid.Controls.Add(lblDay);
            }

            // Session rows (3 rows: Morning, Afternoon, Evening) with dynamic heights
            int currentY = headerHeight;

            for (int sessionRow = 0; sessionRow < sessions.Length; sessionRow++)
            {
                // Calculate dynamic row height based on maximum courses in any cell for this session
                int maxCoursesInSession = 0;
                for (int col = 0; col < daysOfWeek.Length; col++)
                {
                    int courseCount = 0;
                    HashSet<int> seenCourses = new HashSet<int>();
                    foreach (DataRow schedule in schedules.Rows)
                    {
                        if (schedule["DayOfWeek"] != DBNull.Value && schedule["TimeSlot"] != DBNull.Value && schedule["CourseId"] != DBNull.Value)
                        {
                            int dayOfWeek = Convert.ToInt32(schedule["DayOfWeek"]);
                            int timeSlot = Convert.ToInt32(schedule["TimeSlot"]);
                            int courseId = Convert.ToInt32(schedule["CourseId"]);
                            if (dayOfWeek == col &&
                                timeSlot >= sessionStartSlots[sessionRow] &&
                                timeSlot <= sessionEndSlots[sessionRow] &&
                                !seenCourses.Contains(courseId))
                            {
                                courseCount++;
                                seenCourses.Add(courseId);
                            }
                        }
                    }
                    maxCoursesInSession = Math.Max(maxCoursesInSession, courseCount);
                }
                int rowHeight = Math.Max(minCellHeight, maxCoursesInSession * 148 + 20); // 148px per course (140 + 8 gap) + padding

                // Session label with light gray background
                Label lblSession = new Label
                {
                    Text = sessions[sessionRow],
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(0, currentY),
                    Size = new Size(timeColumnWidth, rowHeight),
                    BackColor = Color.FromArgb(248, 249, 250), // #F8F9FA - Light gray
                    ForeColor = Color.FromArgb(31, 41, 55),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle
                };
                panelGrid.Controls.Add(lblSession);

                // Day cells for this session
                for (int col = 0; col < daysOfWeek.Length; col++)
                {
                    Panel cell = new Panel
                    {
                        Location = new Point(timeColumnWidth + (col * cellWidth), currentY),
                        Size = new Size(cellWidth, rowHeight),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        AutoScroll = true
                    };

                    // Add all courses in this session - group consecutive time slots
                    int courseYOffset = 0;
                    HashSet<int> processedCourses = new HashSet<int>(); // Track which courses we've already added

                    foreach (DataRow schedule in schedules.Rows)
                    {
                        int dayOfWeek = Convert.ToInt32(schedule["DayOfWeek"]);
                        int timeSlot = Convert.ToInt32(schedule["TimeSlot"]);
                        int courseId = Convert.ToInt32(schedule["CourseId"]);

                        // Check if this course is in the current session and not yet processed
                        if (dayOfWeek == col &&
                            timeSlot >= sessionStartSlots[sessionRow] &&
                            timeSlot <= sessionEndSlots[sessionRow] &&
                            !processedCourses.Contains(courseId))
                        {
                            // Find all consecutive time slots for this course on this day
                            List<DataRow> consecutiveSlots = new List<DataRow>();
                            int currentSlot = timeSlot;

                            while (true)
                            {
                                DataRow[] matches = schedules.Select($"CourseId = {courseId} AND DayOfWeek = {dayOfWeek} AND TimeSlot = {currentSlot}");
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
                                processedCourses.Add(courseId);
                            }
                        }
                    }

                    panelGrid.Controls.Add(cell);
                }

                currentY += rowHeight; // Move to next row
            }

            // Add legend
            CreateLegend();
        }

        private void AddCourseToCell(Panel cell, DataRow schedule, int yOffset)
        {
            string room = schedule["Room"] != DBNull.Value ? schedule["Room"].ToString() : "---";
            string startTime = null;
            string endTime = null;
            if (schedule.Table.Columns.Contains("StartTime") && schedule["StartTime"] != DBNull.Value)
            {
                TimeSpan ts = (TimeSpan)schedule["StartTime"];
                startTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }
            if (schedule.Table.Columns.Contains("EndTime") && schedule["EndTime"] != DBNull.Value)
            {
                TimeSpan ts = (TimeSpan)schedule["EndTime"];
                endTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }

            // Create a course block
            Panel coursePanel = new Panel
            {
                Location = new Point(2, yOffset),
                Size = new Size(cell.Width - 6, 90),
                BackColor = GetRandomPastelColor(schedule["CourseCode"].ToString()),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(3)
            };

            // Course code
            Label lblCellCourseCode = new Label
            {
                Text = schedule["CourseCode"].ToString(),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(3, 3),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 6, 16),
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            coursePanel.Controls.Add(lblCellCourseCode);

            // Course name
            Label lblCellCourseName = new Label
            {
                Text = schedule["CourseName"].ToString(),
                Font = new Font("Segoe UI", 7),
                Location = new Point(3, 20),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 6, 28),
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            coursePanel.Controls.Add(lblCellCourseName);

            // Time info (time slot + time range)
            int timeSlot = Convert.ToInt32(schedule["TimeSlot"]);
            string slotText = $"Tiết {timeSlot + 1}";
            string timeText = !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime) ? $"{startTime}-{endTime}" : "";
            Label lblCellTimeInfo = new Label
            {
                Text = slotText + (string.IsNullOrEmpty(timeText) ? "" : $" | {timeText}"),
                Font = new Font("Segoe UI", 7),
                Location = new Point(3, 50),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 6, 14),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            coursePanel.Controls.Add(lblCellTimeInfo);

            // Room
            Label lblCellRoom = new Label
            {
                Text = "📍 " + room,
                Font = new Font("Segoe UI", 7),
                Location = new Point(3, 66),
                AutoSize = false,
                Size = new Size(coursePanel.Width - 6, 14),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            coursePanel.Controls.Add(lblCellRoom);

            // Teacher (optional, only if space allows)
            if (schedule["TeacherName"] != DBNull.Value)
            {
                Label lblCellTeacher = new Label
                {
                    Text = "👤 " + schedule["TeacherName"].ToString(),
                    Font = new Font("Segoe UI", 6, FontStyle.Italic),
                    Location = new Point(3, 80),
                    AutoSize = false,
                    Size = new Size(coursePanel.Width - 6, 10),
                    ForeColor = Color.FromArgb(107, 114, 128)
                };
                coursePanel.Controls.Add(lblCellTeacher);
            }

            // Build tag object for click event
            ScheduleBlock sb = new ScheduleBlock
            {
                Row = schedule,
                StartSlot = timeSlot + 1,
                EndSlot = timeSlot + 1,
                StartTime = startTime,
                EndTime = endTime
            };
            coursePanel.Tag = sb;
            coursePanel.Click += Cell_Click;

            // Make labels clickable
            lblCellCourseCode.Click += (s, e) => Cell_Click(coursePanel, e);
            lblCellCourseName.Click += (s, e) => Cell_Click(coursePanel, e);
            lblCellTimeInfo.Click += (s, e) => Cell_Click(coursePanel, e);
            lblCellRoom.Click += (s, e) => Cell_Click(coursePanel, e);

            // Add tooltip
            ToolTip tooltip = new ToolTip();
            string tooltipText = $"{schedule["CourseCode"]} - {schedule["CourseName"]}\n" +
                               $"Tiết: {sb.StartSlot}\n" +
                               $"Giờ: {(sb.StartTime ?? "?")} - {(sb.EndTime ?? "?")}\n" +
                               $"Phòng: {room}\n" +
                               $"GV: {schedule["TeacherName"]}";
            tooltip.SetToolTip(coursePanel, tooltipText);

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
            if (!hasSessionViolation && allSchedules != null)
            {
                var conflicts = Helpers.ScheduleValidator.DetectConflicts(
                    dayOfWeek,
                    startSlotIndex,
                    endSlotIndex,
                    allSchedules,
                    Convert.ToInt32(firstSlot["CourseId"])
                );
                hasTimeConflict = conflicts.Count > 0;
            }

            bool hasConflict = hasSessionViolation || hasTimeConflict;

            // Determine time slot range and time range first
            int startSlot = startSlotIndex + 1;
            int endSlot = endSlotIndex + 1;
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

            // Create a modern course card with rounded corners and shadow
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

                ToolTip conflictTooltip = new ToolTip();
                conflictTooltip.SetToolTip(lblWarningIcon, conflictMessage);
                conflictTooltip.SetToolTip(coursePanel, conflictMessage);

                coursePanel.Controls.Add(lblWarningIcon);
            }

            // Add all labels to coursePanel
            coursePanel.Controls.Add(lblCourseHeader);
            coursePanel.Controls.Add(lblTeacher);
            coursePanel.Controls.Add(lblTimeSlot);
            coursePanel.Controls.Add(lblTimeRange);
            coursePanel.Controls.Add(lblLocation);

            // Build tag object for click event
            ScheduleBlock sb = new ScheduleBlock
            {
                Row = firstSlot,
                StartSlot = startSlot,
                EndSlot = endSlot,
                StartTime = startTime,
                EndTime = endTime
            };
            coursePanel.Tag = sb;
            coursePanel.Click += Cell_Click;

            // Make labels clickable
            lblCourseHeader.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTeacher.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTimeSlot.Click += (s, e) => Cell_Click(coursePanel, e);
            lblTimeRange.Click += (s, e) => Cell_Click(coursePanel, e);
            lblLocation.Click += (s, e) => Cell_Click(coursePanel, e);

            // Add tooltip
            ToolTip tooltip = new ToolTip();
            string tooltipText = $"{firstSlot["CourseCode"]} - {firstSlot["CourseName"]}\n" +
                               $"Tiết: {startSlot}-{endSlot}\n" +
                               $"Giờ: {(startTime ?? "?")} - {(endTime ?? "?")}\n" +
                               $"Phòng: {roomText}\n" +
                               $"GV: {firstSlot["TeacherName"]}";
            tooltip.SetToolTip(coursePanel, tooltipText);

            cell.Controls.Add(coursePanel);
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            Panel cell = sender as Panel;
            if (cell?.Tag is ScheduleBlock block)
            {
                var schedule = block.Row;
                string room = schedule["Room"] != DBNull.Value ? schedule["Room"].ToString() : "Chưa xác định";
                string teacher = schedule["TeacherName"] != DBNull.Value ? schedule["TeacherName"].ToString() : "Chưa xác định";

                string message = $"Thông tin chi tiết:\n\n" +
                               $"Môn học: {schedule["CourseName"]}\n" +
                               $"Mã môn: {schedule["CourseCode"]}\n" +
                               $"Tiết: {block.StartSlot} - {block.EndSlot}\n" +
                               $"Giờ: {(block.StartTime ?? "?")} - {(block.EndTime ?? "?")}\n" +
                               $"Phòng học: {room}\n" +
                               $"Giảng viên: {teacher}";

                MessageBox.Show(message, "Chi tiết lịch học",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (cell?.Tag is DataRow schedule)
            {
                string room = schedule["Room"] != DBNull.Value ? schedule["Room"].ToString() : "Chưa xác định";
                string teacher = schedule["TeacherName"] != DBNull.Value ? schedule["TeacherName"].ToString() : "Chưa xác định";
                string startTime = null;
                string endTime = null;
                if (schedule.Table.Columns.Contains("StartTime") && schedule["StartTime"] != DBNull.Value)
                {
                    TimeSpan ts = (TimeSpan)schedule["StartTime"];
                    startTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
                }
                if (schedule.Table.Columns.Contains("EndTime") && schedule["EndTime"] != DBNull.Value)
                {
                    TimeSpan ts = (TimeSpan)schedule["EndTime"];
                    endTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
                }

                string timeText = startTime != null && endTime != null ? $"\nGiờ: {startTime} - {endTime}" : string.Empty;

                string message = $"Thông tin chi tiết:\n\n" +
                               $"Môn học: {schedule["CourseName"]}\n" +
                               $"Mã môn: {schedule["CourseCode"]}\n" +
                               $"Phòng học: {room}{timeText}\n" +
                               $"Giảng viên: {teacher}";

                MessageBox.Show(message, "Chi tiết lịch học",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CreateLegend()
        {
            Label lblLegend = new Label
            {
                Text = "💡 Click vào ô để xem chi tiết môn học",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(5, 535),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelGrid.Controls.Add(lblLegend);
        }

        private Color GetRandomPastelColor(string seed)
        {
            // Generate consistent color based on course code
            int hash = seed.GetHashCode();
            Random random = new Random(hash);

            int r = random.Next(180, 255);
            int g = random.Next(180, 255);
            int b = random.Next(180, 255);

            return Color.FromArgb(r, g, b);
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
    }
}
