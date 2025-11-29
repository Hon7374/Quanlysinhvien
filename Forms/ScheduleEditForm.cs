using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class ScheduleEditForm : Form
    {
        private int? scheduleId;
        private ComboBox cboCourse;
        private ComboBox cboDayOfWeek;
        private ComboBox cboTimeSlot;
        private TextBox txtRoom;
        private Label lblConflictWarning;
        private Button btnSave;
        private Button btnCancel;

        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        private readonly string[] timeSlots = {
            "Tiết 1 (07:00 - 07:50)",  // Buổi sáng
            "Tiết 2 (07:55 - 08:45)",
            "Tiết 3 (08:50 - 09:40)",
            "Tiết 4 (09:50 - 10:40)",
            "Tiết 5 (10:45 - 11:35)",
            "Tiết 6 (12:30 - 13:20)",  // Buổi chiều
            "Tiết 7 (13:25 - 14:15)",
            "Tiết 8 (14:20 - 15:10)",
            "Tiết 9 (15:20 - 16:10)",
            "Tiết 10 (16:15 - 17:05)",
            "Tiết 11 (17:30 - 18:20)", // Buổi tối
            "Tiết 12 (18:25 - 19:15)",
            "Tiết 13 (19:20 - 20:10)",
            "Tiết 14 (20:15 - 21:05)"
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

        // Constructor for adding new schedule
        public ScheduleEditForm()
        {
            this.scheduleId = null;
            InitializeComponent();
            LoadCourses();
            this.Text = "Thêm lịch học mới";
        }

        // Constructor for editing existing schedule
        public ScheduleEditForm(int scheduleId)
        {
            this.scheduleId = scheduleId;
            InitializeComponent();
            LoadCourses();
            LoadScheduleData();
            this.Text = "Sửa lịch học";
        }

        private void InitializeComponent()
        {
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title
            Label lblTitle = new Label
            {
                Text = scheduleId.HasValue ? "SỬA LỊCH HỌC" : "THÊM LỊCH HỌC MỚI",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            this.Controls.Add(lblTitle);

            int yPos = 80;
            int labelWidth = 120;
            int controlWidth = 400;

            // Course
            Label lblCourse = new Label
            {
                Text = "Môn học:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblCourse);

            cboCourse = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(160, yPos),
                Size = new Size(controlWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCourse.SelectedIndexChanged += CheckConflicts;
            this.Controls.Add(cboCourse);
            yPos += 50;

            // Day of Week
            Label lblDay = new Label
            {
                Text = "Thứ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblDay);

            cboDayOfWeek = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(160, yPos),
                Size = new Size(controlWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDayOfWeek.Items.AddRange(daysOfWeek);
            cboDayOfWeek.SelectedIndexChanged += CheckConflicts;
            this.Controls.Add(cboDayOfWeek);
            yPos += 50;

            // Time Slot
            Label lblTime = new Label
            {
                Text = "Tiết học:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblTime);

            cboTimeSlot = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(160, yPos),
                Size = new Size(controlWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTimeSlot.Items.AddRange(timeSlots);
            cboTimeSlot.SelectedIndexChanged += CheckConflicts;
            this.Controls.Add(cboTimeSlot);
            yPos += 50;

            // Room
            Label lblRoom = new Label
            {
                Text = "Phòng học:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblRoom);

            txtRoom = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(160, yPos),
                Size = new Size(controlWidth, 30),
                PlaceholderText = "VD: P101, A203..."
            };
            txtRoom.TextChanged += CheckConflicts;
            this.Controls.Add(txtRoom);
            yPos += 50;

            // Conflict Warning Label
            lblConflictWarning = new Label
            {
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(30, yPos),
                Size = new Size(530, 60),
                ForeColor = Color.FromArgb(239, 68, 68),
                Visible = false,
                Text = ""
            };
            this.Controls.Add(lblConflictWarning);
            yPos += 70;

            // Info Label
            Label lblInfo = new Label
            {
                Text = "💡 Hệ thống sẽ tự động kiểm tra xung đột lịch học",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(30, yPos),
                Size = new Size(530, 25),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            this.Controls.Add(lblInfo);
            yPos += 40;

            // Buttons
            btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(330, yPos),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(156, 163, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = "Lưu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(450, yPos),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadCourses()
        {
            try
            {
                string query = @"
                    SELECT
                        c.CourseId,
                        c.CourseCode + ' - ' + c.CourseName + ' (' + c.Semester + ')' as DisplayName
                    FROM Courses c
                    WHERE c.IsActive = 1
                    ORDER BY c.Semester DESC, c.CourseCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                cboCourse.Items.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    cboCourse.Items.Add(new ComboBoxItem
                    {
                        Text = row["DisplayName"].ToString(),
                        Value = row["CourseId"]
                    });
                }

                cboCourse.DisplayMember = "Text";
                cboCourse.ValueMember = "Value";

                if (cboCourse.Items.Count > 0)
                    cboCourse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadScheduleData()
        {
            try
            {
                string query = @"
                    SELECT CourseId, DayOfWeek, TimeSlot, Room
                    FROM Schedules
                    WHERE ScheduleId = @ScheduleId";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@ScheduleId", scheduleId.Value) });

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    // Set course
                    int courseId = Convert.ToInt32(row["CourseId"]);
                    for (int i = 0; i < cboCourse.Items.Count; i++)
                    {
                        var item = cboCourse.Items[i] as ComboBoxItem;
                        if (item != null && Convert.ToInt32(item.Value) == courseId)
                        {
                            cboCourse.SelectedIndex = i;
                            break;
                        }
                    }

                    // Set day and time
                    cboDayOfWeek.SelectedIndex = Convert.ToInt32(row["DayOfWeek"]);
                    cboTimeSlot.SelectedIndex = Convert.ToInt32(row["TimeSlot"]);
                    txtRoom.Text = row["Room"]?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin lịch học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckConflicts(object sender, EventArgs e)
        {
            if (cboCourse.SelectedIndex < 0 || cboDayOfWeek.SelectedIndex < 0 || cboTimeSlot.SelectedIndex < 0)
            {
                lblConflictWarning.Visible = false;
                btnSave.Enabled = true;
                return;
            }

            try
            {
                var selectedCourse = cboCourse.SelectedItem as ComboBoxItem;
                int courseId = Convert.ToInt32(selectedCourse.Value);
                int dayOfWeek = cboDayOfWeek.SelectedIndex;
                int timeSlot = cboTimeSlot.SelectedIndex;
                string room = txtRoom.Text.Trim();

                // Check 1: Duplicate schedule for same course
                string query1 = @"
                    SELECT COUNT(*)
                    FROM Schedules
                    WHERE CourseId = @CourseId
                    AND DayOfWeek = @DayOfWeek
                    AND TimeSlot = @TimeSlot";

                if (scheduleId.HasValue)
                    query1 += " AND ScheduleId != @ScheduleId";

                var param1 = new SqlParameter[]
                {
                    new SqlParameter("@CourseId", courseId),
                    new SqlParameter("@DayOfWeek", dayOfWeek),
                    new SqlParameter("@TimeSlot", timeSlot),
                    new SqlParameter("@ScheduleId", scheduleId ?? (object)DBNull.Value)
                };

                int duplicateCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query1, param1));

                if (duplicateCount > 0)
                {
                    lblConflictWarning.Text = "⚠️ Môn học này đã có lịch học vào thời gian này!";
                    lblConflictWarning.Visible = true;
                    btnSave.Enabled = false;
                    return;
                }

                // Check 2: Teacher conflict (same teacher teaching different course at same time)
                string query2 = @"
                    SELECT c2.CourseCode, c2.CourseName
                    FROM Schedules s
                    INNER JOIN Courses c1 ON s.CourseId = c1.CourseId
                    INNER JOIN Courses c2 ON c1.TeacherId = c2.TeacherId AND c1.CourseId != c2.CourseId
                    INNER JOIN Schedules s2 ON c2.CourseId = s2.CourseId
                    WHERE c1.CourseId = @CourseId
                    AND s2.DayOfWeek = @DayOfWeek
                    AND s2.TimeSlot = @TimeSlot";

                if (scheduleId.HasValue)
                    query2 += " AND s2.ScheduleId != @ScheduleId";

                var param2 = new SqlParameter[]
                {
                    new SqlParameter("@CourseId", courseId),
                    new SqlParameter("@DayOfWeek", dayOfWeek),
                    new SqlParameter("@TimeSlot", timeSlot),
                    new SqlParameter("@ScheduleId", scheduleId ?? (object)DBNull.Value)
                };

                DataTable teacherConflict = DatabaseHelper.ExecuteQuery(query2, param2);

                if (teacherConflict.Rows.Count > 0)
                {
                    string conflictCourse = teacherConflict.Rows[0]["CourseCode"].ToString() + " - " +
                                          teacherConflict.Rows[0]["CourseName"].ToString();
                    lblConflictWarning.Text = $"⚠️ Giảng viên đang dạy môn '{conflictCourse}' vào thời gian này!";
                    lblConflictWarning.Visible = true;
                    btnSave.Enabled = false;
                    return;
                }

                // Check 2.5: Student conflict - if students are already enrolled in this course and have other enrolled courses at this day/time
                string studentConflictQuery = @"
                    SELECT DISTINCT e1.StudentId
                    FROM Enrollments e1
                    INNER JOIN Enrollments e2 ON e1.StudentId = e2.StudentId AND e1.CourseId <> e2.CourseId
                    INNER JOIN Schedules s2 ON e2.CourseId = s2.CourseId
                    WHERE e1.CourseId = @CourseId
                      AND s2.DayOfWeek = @DayOfWeek
                      AND s2.TimeSlot = @TimeSlot
                      AND e2.Status = 'Enrolled'";

                var paramStudent = new SqlParameter[]
                {
                    new SqlParameter("@CourseId", courseId),
                    new SqlParameter("@DayOfWeek", dayOfWeek),
                    new SqlParameter("@TimeSlot", timeSlot)
                };

                DataTable studentConflict = DatabaseHelper.ExecuteQuery(studentConflictQuery, paramStudent);
                if (studentConflict.Rows.Count > 0)
                {
                    lblConflictWarning.Text = "⚠️ Việc lưu sẽ tạo xung đột thời khóa biểu cho một số sinh viên đã đăng ký học môn này. Vui lòng kiểm tra trước khi lưu!";
                    lblConflictWarning.Visible = true;
                    btnSave.Enabled = false;
                    return;
                }

                // Check 3: Room conflict (if room is specified)
                if (!string.IsNullOrWhiteSpace(room))
                {
                    string query3 = @"
                        SELECT c.CourseCode, c.CourseName
                        FROM Schedules s
                        INNER JOIN Courses c ON s.CourseId = c.CourseId
                        WHERE s.Room = @Room
                        AND s.DayOfWeek = @DayOfWeek
                        AND s.TimeSlot = @TimeSlot";

                    if (scheduleId.HasValue)
                        query3 += " AND s.ScheduleId != @ScheduleId";

                    var param3 = new SqlParameter[]
                    {
                        new SqlParameter("@Room", room),
                        new SqlParameter("@DayOfWeek", dayOfWeek),
                        new SqlParameter("@TimeSlot", timeSlot),
                        new SqlParameter("@ScheduleId", scheduleId ?? (object)DBNull.Value)
                    };

                    DataTable roomConflict = DatabaseHelper.ExecuteQuery(query3, param3);

                    if (roomConflict.Rows.Count > 0)
                    {
                        string conflictCourse = roomConflict.Rows[0]["CourseCode"].ToString() + " - " +
                                              roomConflict.Rows[0]["CourseName"].ToString();
                        lblConflictWarning.Text = $"⚠️ Phòng '{room}' đã được sử dụng cho môn '{conflictCourse}' vào thời gian này!";
                        lblConflictWarning.ForeColor = Color.FromArgb(245, 158, 11); // Orange warning
                        lblConflictWarning.Visible = true;
                        // Allow saving but warn user
                        btnSave.Enabled = true;
                        return;
                    }
                }

                // No conflicts
                lblConflictWarning.Visible = false;
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking conflicts: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var selectedCourse = cboCourse.SelectedItem as ComboBoxItem;
                int courseId = Convert.ToInt32(selectedCourse.Value);
                int dayOfWeek = cboDayOfWeek.SelectedIndex;
                int timeSlot = cboTimeSlot.SelectedIndex;
                string room = txtRoom.Text.Trim();
                TimeSpan startTime = startTimes[timeSlot];
                TimeSpan endTime = endTimes[timeSlot];

                // Validate session boundaries for this course's complete schedule
                string existingSchedulesQuery = @"
                    SELECT s.CourseId, s.DayOfWeek, s.TimeSlot, c.CourseCode, c.CourseName
                    FROM Schedules s
                    INNER JOIN Courses c ON s.CourseId = c.CourseId
                    WHERE s.CourseId = @CourseId";

                if (scheduleId.HasValue)
                    existingSchedulesQuery += " AND s.ScheduleId != @ScheduleId";

                DataTable existingCourseSchedules = DatabaseHelper.ExecuteQuery(existingSchedulesQuery, new SqlParameter[]
                {
                    new SqlParameter("@CourseId", courseId),
                    new SqlParameter("@ScheduleId", scheduleId ?? (object)DBNull.Value)
                });

                // Add the current slot being saved to check session boundary
                var daySlotPairs = new System.Collections.Generic.List<Tuple<int, int>>();
                daySlotPairs.Add(Tuple.Create(dayOfWeek, timeSlot));

                // Add existing slots for same day
                foreach (DataRow row in existingCourseSchedules.Rows)
                {
                    int existingDay = Convert.ToInt32(row["DayOfWeek"]);
                    int existingSlot = Convert.ToInt32(row["TimeSlot"]);
                    if (existingDay == dayOfWeek)
                    {
                        daySlotPairs.Add(Tuple.Create(existingDay, existingSlot));
                    }
                }

                // Check if all slots on this day are in the same session
                var slotsOnDay = daySlotPairs.Where(p => p.Item1 == dayOfWeek).Select(p => p.Item2).OrderBy(s => s).ToList();
                if (slotsOnDay.Count > 1)
                {
                    int minSlot = slotsOnDay.Min();
                    int maxSlot = slotsOnDay.Max();
                    if (!Helpers.ScheduleValidator.ValidateSessionRange(minSlot, maxSlot, out string sessionError))
                    {
                        MessageBox.Show(sessionError, "Lỗi vi phạm ca học", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string query;
                if (scheduleId.HasValue)
                {
                    // Update existing schedule
                    query = @"
                        UPDATE Schedules
                        SET CourseId = @CourseId,
                            DayOfWeek = @DayOfWeek,
                            TimeSlot = @TimeSlot,
                            Room = @Room,
                            StartTime = @StartTime,
                            EndTime = @EndTime
                        WHERE ScheduleId = @ScheduleId";

                    DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[]
                    {
                        new SqlParameter("@CourseId", courseId),
                        new SqlParameter("@DayOfWeek", dayOfWeek),
                        new SqlParameter("@TimeSlot", timeSlot),
                        new SqlParameter("@Room", string.IsNullOrEmpty(room) ? (object)DBNull.Value : room),
                        new SqlParameter("@StartTime", startTime),
                        new SqlParameter("@EndTime", endTime),
                        new SqlParameter("@ScheduleId", scheduleId.Value)
                    });

                    MessageBox.Show("Cập nhật lịch học thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Insert new schedule
                    query = @"
                        INSERT INTO Schedules (CourseId, DayOfWeek, TimeSlot, Room, StartTime, EndTime)
                        VALUES (@CourseId, @DayOfWeek, @TimeSlot, @Room, @StartTime, @EndTime)";

                    DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[]
                    {
                        new SqlParameter("@CourseId", courseId),
                        new SqlParameter("@DayOfWeek", dayOfWeek),
                        new SqlParameter("@TimeSlot", timeSlot),
                        new SqlParameter("@Room", string.IsNullOrEmpty(room) ? (object)DBNull.Value : room),
                        new SqlParameter("@StartTime", startTime),
                        new SqlParameter("@EndTime", endTime)
                    });

                    MessageBox.Show("Thêm lịch học thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu lịch học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (cboCourse.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn môn học!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCourse.Focus();
                return false;
            }

            if (cboDayOfWeek.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn thứ trong tuần!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboDayOfWeek.Focus();
                return false;
            }

            if (cboTimeSlot.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn tiết học!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboTimeSlot.Focus();
                return false;
            }

            return true;
        }
    }
}
