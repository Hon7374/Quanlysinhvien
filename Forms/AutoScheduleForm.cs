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
    public partial class AutoScheduleForm : Form
    {
        private ComboBox cboSemester;
        private CheckedListBox clbCourses;
        private CheckedListBox clbDays;
        private CheckedListBox clbTimeSlots;
        private TextBox txtRoomPrefix;
        private CheckBox chkClearOldSchedules;
        private ProgressBar progressBar;
        private Label lblProgress;
        private Button btnGenerate;
        private Button btnCancel;

        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        private readonly string[] timeSlots = {
            "Tiết 1 (07:00-07:50)",  // Buổi sáng
            "Tiết 2 (07:55-08:45)",
            "Tiết 3 (08:50-09:40)",
            "Tiết 4 (09:50-10:40)",
            "Tiết 5 (10:45-11:35)",
            "Tiết 6 (12:30-13:20)",  // Buổi chiều
            "Tiết 7 (13:25-14:15)",
            "Tiết 8 (14:20-15:10)",
            "Tiết 9 (15:20-16:10)",
            "Tiết 10 (16:15-17:05)",
            "Tiết 11 (17:30-18:20)", // Buổi tối
            "Tiết 12 (18:25-19:15)",
            "Tiết 13 (19:20-20:10)",
            "Tiết 14 (20:15-21:05)"
        };

        private readonly TimeSpan[] startTimes = {
            new TimeSpan(7, 0, 0),   // Tiết 1
            new TimeSpan(7, 55, 0),  // Tiết 2
            new TimeSpan(8, 50, 0),  // Tiết 3
            new TimeSpan(9, 50, 0),  // Tiết 4
            new TimeSpan(10, 45, 0), // Tiết 5
            new TimeSpan(12, 30, 0), // Tiết 6
            new TimeSpan(13, 25, 0), // Tiết 7
            new TimeSpan(14, 20, 0), // Tiết 8
            new TimeSpan(15, 20, 0), // Tiết 9
            new TimeSpan(16, 15, 0), // Tiết 10
            new TimeSpan(17, 30, 0), // Tiết 11
            new TimeSpan(18, 25, 0), // Tiết 12
            new TimeSpan(19, 20, 0), // Tiết 13
            new TimeSpan(20, 15, 0)  // Tiết 14
        };

        private readonly TimeSpan[] endTimes = {
            new TimeSpan(7, 50, 0),  // Tiết 1
            new TimeSpan(8, 45, 0),  // Tiết 2
            new TimeSpan(9, 40, 0),  // Tiết 3
            new TimeSpan(10, 40, 0), // Tiết 4
            new TimeSpan(11, 35, 0), // Tiết 5
            new TimeSpan(13, 20, 0), // Tiết 6
            new TimeSpan(14, 15, 0), // Tiết 7
            new TimeSpan(15, 10, 0), // Tiết 8
            new TimeSpan(16, 10, 0), // Tiết 9
            new TimeSpan(17, 5, 0),  // Tiết 10
            new TimeSpan(18, 20, 0), // Tiết 11
            new TimeSpan(19, 15, 0), // Tiết 12
            new TimeSpan(20, 10, 0), // Tiết 13
            new TimeSpan(21, 5, 0)   // Tiết 14
        };

        public AutoScheduleForm()
        {
            InitializeComponent();
            LoadSemesters();
            InitializeDefaults();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(700, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Tự động phân lịch học";

            // Title
            Label lblTitle = new Label
            {
                Text = "TỰ ĐỘNG PHÂN LỊCH HỌC",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            this.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Hệ thống sẽ tự động phân bổ lịch học cho các môn chưa có lịch",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(30, 55),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            this.Controls.Add(lblSubtitle);

            int yPos = 100;

            // Semester Selection
            Label lblSemester = new Label
            {
                Text = "Chọn học kỳ:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblSemester);
            yPos += 30;

            cboSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos),
                Size = new Size(620, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboSemester.SelectedIndexChanged += CboSemester_SelectedIndexChanged;
            this.Controls.Add(cboSemester);
            yPos += 50;

            // Courses to Schedule
            Label lblCourses = new Label
            {
                Text = "Các môn học cần phân lịch:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblCourses);
            yPos += 30;

            clbCourses = new CheckedListBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(30, yPos),
                Size = new Size(620, 120),
                CheckOnClick = true
            };
            this.Controls.Add(clbCourses);
            yPos += 130;

            // Days Selection
            Label lblDays = new Label
            {
                Text = "Chọn các ngày trong tuần:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblDays);
            yPos += 30;

            clbDays = new CheckedListBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(30, yPos),
                Size = new Size(300, 100),
                CheckOnClick = true,
                MultiColumn = true,
                ColumnWidth = 100
            };
            clbDays.Items.AddRange(daysOfWeek);
            this.Controls.Add(clbDays);

            // Time Slots Selection
            Label lblTimeSlots = new Label
            {
                Text = "Chọn các tiết học:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(350, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblTimeSlots);

            clbTimeSlots = new CheckedListBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(350, yPos + 30),
                Size = new Size(300, 100),
                CheckOnClick = true
            };
            clbTimeSlots.Items.AddRange(timeSlots);
            this.Controls.Add(clbTimeSlots);
            yPos += 140;

            // Room Prefix
            Label lblRoom = new Label
            {
                Text = "Tiền tố phòng học (tùy chọn):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, yPos),
                AutoSize = true
            };
            this.Controls.Add(lblRoom);
            yPos += 30;

            txtRoomPrefix = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos),
                Size = new Size(200, 30),
                PlaceholderText = "VD: P, A, B..."
            };
            this.Controls.Add(txtRoomPrefix);

            Label lblRoomHint = new Label
            {
                Text = "Để trống nếu không cần phòng học cụ thể",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(240, yPos + 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            this.Controls.Add(lblRoomHint);
            yPos += 50;

            // Clear old schedules option
            chkClearOldSchedules = new CheckBox
            {
                Text = "⚠️ Xóa tất cả lịch học cũ của các môn đã chọn trước khi tạo mới",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(30, yPos),
                AutoSize = true,
                ForeColor = Color.FromArgb(220, 38, 38),
                Checked = true
            };
            this.Controls.Add(chkClearOldSchedules);
            yPos += 40;

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(30, yPos),
                Size = new Size(620, 25),
                Visible = false
            };
            this.Controls.Add(progressBar);
            yPos += 35;

            lblProgress = new Label
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(30, yPos),
                Size = new Size(620, 20),
                ForeColor = Color.FromArgb(107, 114, 128),
                Visible = false
            };
            this.Controls.Add(lblProgress);
            yPos += 40;

            // Buttons
            btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(430, yPos),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(156, 163, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            btnGenerate = new Button
            {
                Text = "⚡ Tạo lịch",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(550, yPos),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;
            this.Controls.Add(btnGenerate);

            this.CancelButton = btnCancel;
        }

        private void InitializeDefaults()
        {
            // Check Monday to Friday by default
            for (int i = 0; i < 5; i++)
            {
                clbDays.SetItemChecked(i, true);
            }

            // Check all time slots by default
            for (int i = 0; i < clbTimeSlots.Items.Count; i++)
            {
                clbTimeSlots.SetItemChecked(i, true);
            }
        }

        private void LoadSemesters()
        {
            try
            {
                string query = "SELECT DISTINCT Semester FROM Courses WHERE IsActive = 1 ORDER BY Semester DESC";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                cboSemester.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    cboSemester.Items.Add(row["Semester"].ToString());
                }

                if (cboSemester.Items.Count > 0)
                    cboSemester.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách học kỳ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCoursesWithoutSchedule();
        }

        private void LoadCoursesWithoutSchedule()
        {
            if (cboSemester.SelectedIndex < 0) return;

            try
            {
                string semester = cboSemester.SelectedItem.ToString();

                // Get courses without any schedule
                string query = @"
                    SELECT c.CourseId, c.CourseCode, c.CourseName, c.Credits
                    FROM Courses c
                    WHERE c.Semester = @Semester
                    AND c.IsActive = 1
                    AND NOT EXISTS (
                        SELECT 1 FROM Schedules s WHERE s.CourseId = c.CourseId
                    )
                    ORDER BY c.CourseCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@Semester", semester) });

                clbCourses.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    string display = $"{row["CourseCode"]} - {row["CourseName"]} ({row["Credits"]} tín chỉ)";
                    clbCourses.Items.Add(new CourseItem
                    {
                        CourseId = Convert.ToInt32(row["CourseId"]),
                        DisplayName = display,
                        Credits = Convert.ToInt32(row["Credits"])
                    }, true);
                }

                if (clbCourses.Items.Count == 0)
                {
                    MessageBox.Show("Tất cả các môn học trong học kỳ này đã có lịch!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn tạo lịch tự động?\n\n" +
                "Hệ thống sẽ tự động phân bổ lịch học cho các môn đã chọn.\n" +
                "Các môn sẽ được phân bổ nhiều buổi học dựa trên số tín chỉ.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                GenerateSchedules();
            }
        }

        private bool ValidateInput()
        {
            if (clbCourses.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một môn học!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (clbDays.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một ngày trong tuần!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (clbTimeSlots.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tiết học!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void GenerateSchedules()
        {
            try
            {
                btnGenerate.Enabled = false;
                btnCancel.Enabled = false;
                progressBar.Visible = true;
                lblProgress.Visible = true;

                List<int> selectedDays = new List<int>();
                for (int i = 0; i < clbDays.Items.Count; i++)
                {
                    if (clbDays.GetItemChecked(i))
                        selectedDays.Add(i);
                }

                List<int> selectedTimeSlots = new List<int>();
                for (int i = 0; i < clbTimeSlots.Items.Count; i++)
                {
                    if (clbTimeSlots.GetItemChecked(i))
                        selectedTimeSlots.Add(i);
                }

                List<CourseItem> coursesToSchedule = new List<CourseItem>();
                foreach (var item in clbCourses.CheckedItems)
                {
                    coursesToSchedule.Add((CourseItem)item);
                }

                progressBar.Maximum = coursesToSchedule.Count;
                progressBar.Value = 0;

                // Delete old schedules if option is checked
                if (chkClearOldSchedules.Checked && coursesToSchedule.Count > 0)
                {
                    lblProgress.Text = "Đang xóa lịch học cũ...";
                    lblProgress.Visible = true;
                    Application.DoEvents();

                    var ids = coursesToSchedule.Select((c, idx) => new { Id = c.CourseId, Param = "@id" + idx }).ToList();
                    string placeholders = string.Join(",", ids.Select(x => x.Param));
                    string deleteQuery = $"DELETE FROM Schedules WHERE CourseId IN ({placeholders})";
                    var sqlParams = ids.Select(x => new System.Data.SqlClient.SqlParameter(x.Param, x.Id)).ToArray();

                    try
                    {
                        DatabaseHelper.ExecuteNonQuery(deleteQuery, sqlParams);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa lịch cũ: {ex.Message}", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnGenerate.Enabled = true;
                        btnCancel.Enabled = true;
                        progressBar.Visible = false;
                        lblProgress.Visible = false;
                        return;
                    }
                }

                int successCount = 0;
                int failCount = 0;
                List<string> failedCourses = new List<string>();

                string roomPrefix = txtRoomPrefix.Text.Trim();
                int roomCounter = 101;

                foreach (var course in coursesToSchedule)
                {
                    lblProgress.Text = $"Đang xử lý: {course.DisplayName}";
                    Application.DoEvents();

                    // Each course needs ONE session block with consecutive time slots
                    // Credits determine how many consecutive periods
                    int consecutiveSlots = course.Credits; // 2 credits = 2 consecutive slots, 3 credits = 3 consecutive slots

                    // Try to create ONE schedule block (recurring weekly)
                    bool scheduled = false;
                    Random random = new Random(course.CourseId + DateTime.Now.Millisecond); // Add randomness for variety

                    // Shuffle days and time slots for variety
                    List<int> shuffledDays = selectedDays.OrderBy(x => random.Next()).ToList();
                    List<int> shuffledTimeSlots = selectedTimeSlots.OrderBy(x => random.Next()).ToList();

                    // Try to schedule ONE block of consecutive time slots
                    int attemptCount = 0;
                    int maxAttempts = selectedDays.Count * selectedTimeSlots.Count * 2;

                    while (!scheduled && attemptCount < maxAttempts)
                    {
                        // Cycle through combinations
                        int dayIndex = attemptCount % shuffledDays.Count;
                        int timeIndex = (attemptCount / shuffledDays.Count) % shuffledTimeSlots.Count;
                        int day = shuffledDays[dayIndex];
                        int startTimeSlot = shuffledTimeSlots[timeIndex];

                        // Check if we can fit consecutive slots starting from startTimeSlot
                        bool canFitBlock = true;

                        // Ensure we don't exceed 14 time slots (0-13)
                        if (startTimeSlot + consecutiveSlots > 14)
                        {
                            canFitBlock = false;
                        }
                        else
                        {
                            // Validate that all consecutive slots fit within same session
                            int endTimeSlot = startTimeSlot + consecutiveSlots - 1;
                            if (!Helpers.ScheduleValidator.ValidateSessionRange(startTimeSlot, endTimeSlot, out string _))
                            {
                                canFitBlock = false;
                            }
                            else
                            {
                                // Check all consecutive slots are available
                                for (int i = 0; i < consecutiveSlots; i++)
                                {
                                    int currentSlot = startTimeSlot + i;
                                    if (!CanSchedule(course.CourseId, day, currentSlot))
                                    {
                                        canFitBlock = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (canFitBlock)
                        {
                            // Try to find an available room for all slots
                            string room = null;
                            bool roomAvailableForAllSlots = true;

                            if (!string.IsNullOrEmpty(roomPrefix))
                            {
                                // Try up to 10 rooms to find one available for all slots
                                for (int roomAttempt = 0; roomAttempt < 10; roomAttempt++)
                                {
                                    room = $"{roomPrefix}{roomCounter + roomAttempt}";
                                    roomAvailableForAllSlots = true;

                                    // Check if room is available for ALL consecutive slots
                                    for (int i = 0; i < consecutiveSlots; i++)
                                    {
                                        if (!IsRoomAvailable(day, startTimeSlot + i, room))
                                        {
                                            roomAvailableForAllSlots = false;
                                            break;
                                        }
                                    }

                                    if (roomAvailableForAllSlots)
                                    {
                                        roomCounter++;
                                        break;
                                    }
                                    room = null;
                                }
                            }

                            if (room != null || string.IsNullOrEmpty(roomPrefix))
                            {
                                // Create schedules for all consecutive slots
                                for (int i = 0; i < consecutiveSlots; i++)
                                {
                                    CreateSchedule(course.CourseId, day, startTimeSlot + i, room);
                                }
                                scheduled = true;
                            }
                        }

                        attemptCount++;
                    }

                    if (scheduled)
                        successCount++;
                    else
                    {
                        failCount++;
                        failedCourses.Add(course.DisplayName);
                    }

                    progressBar.Value++;
                }

                progressBar.Visible = false;
                lblProgress.Visible = false;

                string message = $"Hoàn thành tạo lịch tự động!\n\n" +
                                $"Thành công: {successCount} môn học\n";

                if (failCount > 0)
                {
                    message += $"Thất bại: {failCount} môn học\n\n" +
                              $"Các môn không thể phân lịch:\n" +
                              string.Join("\n", failedCourses);
                }

                MessageBox.Show(message, "Kết quả",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo lịch tự động: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnGenerate.Enabled = true;
                btnCancel.Enabled = true;
                progressBar.Visible = false;
                lblProgress.Visible = false;
            }
        }

        private bool CanSchedule(int courseId, int dayOfWeek, int timeSlot)
        {
            try
            {
                // Check if course already has schedule at this time
                string query = @"
                    SELECT COUNT(*)
                    FROM Schedules
                    WHERE CourseId = @CourseId
                    AND DayOfWeek = @DayOfWeek
                    AND TimeSlot = @TimeSlot";

                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query,
                    new SqlParameter[] {
                        new SqlParameter("@CourseId", courseId),
                        new SqlParameter("@DayOfWeek", dayOfWeek),
                        new SqlParameter("@TimeSlot", timeSlot)
                    }));

                if (count > 0) return false;

                // Check teacher conflict - ensure teacher is not already teaching another course at this time
                string teacherQuery = @"
                    SELECT COUNT(*)
                    FROM Schedules s
                    INNER JOIN Courses c1 ON s.CourseId = c1.CourseId
                    INNER JOIN Courses c2 ON c1.TeacherId = c2.TeacherId AND c2.TeacherId IS NOT NULL
                    WHERE c2.CourseId = @CourseId
                    AND c1.CourseId <> @CourseId
                    AND s.DayOfWeek = @DayOfWeek
                    AND s.TimeSlot = @TimeSlot";

                int teacherCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(teacherQuery,
                    new SqlParameter[] {
                        new SqlParameter("@CourseId", courseId),
                        new SqlParameter("@DayOfWeek", dayOfWeek),
                        new SqlParameter("@TimeSlot", timeSlot)
                    }));

                if (teacherCount > 0) return false; // Teacher has conflict - already teaching another course at this time

                // Check student-level conflict: do any students already enrolled in this course have another course at this day/time?
                string studentConflictQuery = @"
                    SELECT COUNT(*)
                    FROM Enrollments e1
                    INNER JOIN Enrollments e2 ON e1.StudentId = e2.StudentId
                    INNER JOIN Schedules s2 ON e2.CourseId = s2.CourseId
                    WHERE e1.CourseId = @CourseId
                      AND e2.CourseId <> @CourseId
                      AND e2.Status = 'Enrolled'
                      AND s2.DayOfWeek = @DayOfWeek
                      AND s2.TimeSlot = @TimeSlot";

                int studentConflicts = Convert.ToInt32(DatabaseHelper.ExecuteScalar(studentConflictQuery,
                    new SqlParameter[] {
                        new SqlParameter("@CourseId", courseId),
                        new SqlParameter("@DayOfWeek", dayOfWeek),
                        new SqlParameter("@TimeSlot", timeSlot)
                    }));

                if (studentConflicts > 0) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsRoomAvailable(int dayOfWeek, int timeSlot, string room)
        {
            try
            {
                // If no room was specified, treat as always available (no room conflict check)
                if (string.IsNullOrWhiteSpace(room)) return true;
                string query = @"
                    SELECT COUNT(*)
                    FROM Schedules
                    WHERE DayOfWeek = @DayOfWeek
                    AND TimeSlot = @TimeSlot
                    AND Room = @Room";

                DataTable result = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] {
                        new SqlParameter("@DayOfWeek", dayOfWeek),
                        new SqlParameter("@TimeSlot", timeSlot),
                        new SqlParameter("@Room", room)
                    });

                return result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) == 0;
            }
            catch
            {
                return false;
            }
        }

        private void CreateSchedule(int courseId, int dayOfWeek, int timeSlot, string room)
        {
            string query = @"
                INSERT INTO Schedules (CourseId, DayOfWeek, TimeSlot, Room, StartTime, EndTime)
                VALUES (@CourseId, @DayOfWeek, @TimeSlot, @Room, @StartTime, @EndTime)";

            DatabaseHelper.ExecuteNonQuery(query,
                new SqlParameter[] {
                    new SqlParameter("@CourseId", courseId),
                    new SqlParameter("@DayOfWeek", dayOfWeek),
                    new SqlParameter("@TimeSlot", timeSlot),
                    new SqlParameter("@Room", room ?? (object)DBNull.Value),
                    new SqlParameter("@StartTime", startTimes[timeSlot]),
                    new SqlParameter("@EndTime", endTimes[timeSlot])
                });
        }

        private class CourseItem
        {
            public int CourseId { get; set; }
            public string DisplayName { get; set; }
            public int Credits { get; set; }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}
