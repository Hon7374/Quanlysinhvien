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
    public partial class ScheduleManagementForm : Form
    {
        private Panel panelHeader;
        private Panel panelFilters;
        private Panel panelContent;
        private DataGridView dgvSchedules;
        private ComboBox cboFilterCourse;
        private ComboBox cboFilterSemester;

        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        private readonly string[] timeSlotNames = {
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

        public ScheduleManagementForm()
        {
            InitializeComponent();
            EnsureSchedulesTableExists();
            LoadFilters();
            LoadSchedules();
            this.Resize += ScheduleManagementForm_Resize;
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Thời khóa biểu - Schedule Management";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "Quản lý Thời khóa biểu",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            // Add Schedule Button
            Button btnAddSchedule = new Button
            {
                Text = "+ Thêm lịch học",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 75),
                Size = new Size(160, 45),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddSchedule.FlatAppearance.BorderSize = 0;
            btnAddSchedule.Click += BtnAddSchedule_Click;
            panelHeader.Controls.Add(btnAddSchedule);

            // Auto Generate Button
            Button btnAutoGenerate = new Button
            {
                Text = "⚡ Tự động phân lịch",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(200, 75),
                Size = new Size(180, 45),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAutoGenerate.FlatAppearance.BorderSize = 0;
            btnAutoGenerate.Click += BtnAutoGenerate_Click;
            panelHeader.Controls.Add(btnAutoGenerate);

            // View Grid Button
            Button btnViewGrid = new Button
            {
                Text = "📅 Xem lưới TKB",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(390, 75),
                Size = new Size(160, 45),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnViewGrid.FlatAppearance.BorderSize = 0;
            btnViewGrid.Click += BtnViewGrid_Click;
            panelHeader.Controls.Add(btnViewGrid);

            this.Controls.Add(panelHeader);

            // Filters Panel
            panelFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(30, 10, 30, 10)
            };

            Label lblFilterSemester = new Label
            {
                Text = "Học kỳ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 25),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblFilterSemester);

            cboFilterSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(100, 22),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilterSemester.SelectedIndexChanged += (s, e) => LoadSchedules();
            panelFilters.Controls.Add(cboFilterSemester);

            Label lblFilterCourse = new Label
            {
                Text = "Môn học:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(270, 25),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblFilterCourse);

            cboFilterCourse = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(350, 22),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilterCourse.SelectedIndexChanged += (s, e) => LoadSchedules();
            panelFilters.Controls.Add(cboFilterCourse);

            Button btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Font = new Font("Segoe UI", 9),
                Location = new Point(670, 20),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(156, 163, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => { LoadFilters(); LoadSchedules(); };
            panelFilters.Controls.Add(btnRefresh);

            this.Controls.Add(panelFilters);

            // Content Panel
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll=true
            };

            // DataGridView
            dgvSchedules = new DataGridView
            {
                Location = new Point(35, 230),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                ColumnHeadersHeight = 50,
                Width=160,
            };

            dgvSchedules.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0)
            };

            dgvSchedules.DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(224, 231, 255),
                SelectionForeColor = Color.FromArgb(31, 41, 55),
                Padding = new Padding(10, 5, 10, 5)
            };

            dgvSchedules.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251)
            };

            dgvSchedules.CellDoubleClick += DgvSchedules_CellDoubleClick;
            dgvSchedules.CellFormatting += DgvSchedules_CellFormatting;
            // Also ensure we set columns width after data binding is complete
            dgvSchedules.DataBindingComplete += DgvSchedules_DataBindingComplete;

            panelContent.Controls.Add(dgvSchedules);

            // Action Buttons Panel
            Panel panelActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(0, 10, 0, 10)
            };

            Button btnEdit = new Button
            {
                Text = "✏️ Sửa",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 15),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEdit_Click;
            panelActions.Controls.Add(btnEdit);

            Button btnDelete = new Button
            {
                Text = "🗑️ Xóa",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(160, 15),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            panelActions.Controls.Add(btnDelete);

            panelContent.Controls.Add(panelActions);
            this.Controls.Add(panelContent);
        }

        private void ScheduleManagementForm_Resize(object sender, EventArgs e)
        {
            // Safely adjust column widths only when the grid and columns are initialized and not disposed
            try
            {
                if (this.IsDisposed || this.Disposing) return;
                if (dgvSchedules == null || dgvSchedules.IsDisposed) return;
                if (!dgvSchedules.IsHandleCreated) return;
                if (dgvSchedules.Columns == null || dgvSchedules.Columns.Count == 0) return;

                // Use a helper method to check and assign each column only when it exists
                ResizeScheduleColumns();
            }
            catch
            {
                // Ignore resize errors — defensive: avoid throwing during form resize
            }
        }

        private void DgvSchedules_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                // After binding completes, also adjust column sizes
                ResizeScheduleColumns();
            }
            catch
            {
                // Ignore binding errors
            }
        }

        /// <summary>
        /// Helper: Safely set column widths if columns exist. Defensive to avoid NullReferenceException.
        /// </summary>
        private void ResizeScheduleColumns()
        {
            if (dgvSchedules == null || dgvSchedules.IsDisposed) return;
            if (dgvSchedules.Columns == null || dgvSchedules.Columns.Count == 0) return;

            int totalWidth = dgvSchedules.ClientSize.Width;
            // Use extension helpers for defensive assignment
            dgvSchedules.TrySetColumnWidth("CourseCode", (int)(totalWidth * 0.1));
            dgvSchedules.TrySetColumnWidth("CourseName", (int)(totalWidth * 0.20));
            dgvSchedules.TrySetColumnWidth("DayName", (int)(totalWidth * 0.12));
            dgvSchedules.TrySetColumnWidth("TimeSlotName", (int)(totalWidth * 0.18));
            dgvSchedules.TrySetColumnWidth("TimeRange", (int)(totalWidth * 0.12));
            dgvSchedules.TrySetColumnWidth("Room", (int)(totalWidth * 0.10));
            dgvSchedules.TrySetColumnWidth("TeacherName", (int)(totalWidth * 0.18));
            dgvSchedules.TrySetColumnWidth("Semester", (int)(totalWidth * 0.10));
        }

        private void LoadFilters()
        {
            try
            {
                // Load Semesters
                cboFilterSemester.Items.Clear();
                cboFilterSemester.Items.Add("-- Tất cả học kỳ --");
                string semesterQuery = "SELECT DISTINCT Semester FROM Courses WHERE IsActive = 1 ORDER BY Semester DESC";
                DataTable semesters = DatabaseHelper.ExecuteQuery(semesterQuery);
                foreach (DataRow row in semesters.Rows)
                {
                    cboFilterSemester.Items.Add(row["Semester"].ToString());
                }
                cboFilterSemester.SelectedIndex = 0;

                // Load Courses
                cboFilterCourse.Items.Clear();
                cboFilterCourse.Items.Add("-- Tất cả môn học --");
                string courseQuery = @"
                    SELECT CourseId, CourseCode + ' - ' + CourseName as DisplayName
                    FROM Courses
                    WHERE IsActive = 1
                    ORDER BY CourseCode";
                DataTable courses = DatabaseHelper.ExecuteQuery(courseQuery);
                foreach (DataRow row in courses.Rows)
                {
                    cboFilterCourse.Items.Add(new ComboBoxItem
                    {
                        Text = row["DisplayName"].ToString(),
                        Value = row["CourseId"]
                    });
                }
                cboFilterCourse.DisplayMember = "Text";
                cboFilterCourse.ValueMember = "Value";
                cboFilterCourse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải bộ lọc: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSchedules()
        {
            try
            {
                string query = @"
                    SELECT
                        s.ScheduleId,
                        s.CourseId,
                        c.CourseCode,
                        c.CourseName,
                        c.Semester,
                        s.DayOfWeek,
                        CASE s.DayOfWeek
                            WHEN 0 THEN N'Thứ 2'
                            WHEN 1 THEN N'Thứ 3'
                            WHEN 2 THEN N'Thứ 4'
                            WHEN 3 THEN N'Thứ 5'
                            WHEN 4 THEN N'Thứ 6'
                            WHEN 5 THEN N'Thứ 7'
                            WHEN 6 THEN N'Chủ nhật'
                        END AS DayName,
                        s.TimeSlot,
                        CASE s.TimeSlot
                            WHEN 0 THEN N'Tiết 1 (07:00-07:50)'
                            WHEN 1 THEN N'Tiết 2 (07:55-08:45)'
                            WHEN 2 THEN N'Tiết 3 (08:50-09:40)'
                            WHEN 3 THEN N'Tiết 4 (09:50-10:40)'
                            WHEN 4 THEN N'Tiết 5 (10:45-11:35)'
                            WHEN 5 THEN N'Tiết 6 (12:30-13:20)'
                            WHEN 6 THEN N'Tiết 7 (13:25-14:15)'
                            WHEN 7 THEN N'Tiết 8 (14:20-15:10)'
                            WHEN 8 THEN N'Tiết 9 (15:20-16:10)'
                            WHEN 9 THEN N'Tiết 10 (16:15-17:05)'
                            WHEN 10 THEN N'Tiết 11 (17:30-18:20)'
                            WHEN 11 THEN N'Tiết 12 (18:25-19:15)'
                            WHEN 12 THEN N'Tiết 13 (19:20-20:10)'
                            WHEN 13 THEN N'Tiết 14 (20:15-21:05)'
                        END AS TimeSlotName,
                        s.StartTime,
                        s.EndTime,
                        s.Room,
                        u.FullName as TeacherName
                    FROM Schedules s
                    INNER JOIN Courses c ON s.CourseId = c.CourseId
                    LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                    LEFT JOIN Users u ON t.UserId = u.UserId
                    WHERE 1=1";

                List<SqlParameter> parameters = new List<SqlParameter>();

                // Filter by semester
                if (cboFilterSemester != null && cboFilterSemester.SelectedIndex > 0 && cboFilterSemester.SelectedItem != null)
                {
                    query += " AND c.Semester = @Semester";
                    parameters.Add(new SqlParameter("@Semester", cboFilterSemester.SelectedItem.ToString()));
                }

                // Filter by course
                if (cboFilterCourse != null && cboFilterCourse.SelectedIndex > 0)
                {
                    var selectedItem = cboFilterCourse.SelectedItem as ComboBoxItem;
                    if (selectedItem != null)
                    {
                        query += " AND c.CourseId = @CourseId";
                        parameters.Add(new SqlParameter("@CourseId", selectedItem.Value));
                    }
                }

                query += " ORDER BY c.Semester DESC, s.DayOfWeek, s.TimeSlot";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());

                // Add a computed TimeRange column so we can display StartTime-EndTime in the grid
                if (!dt.Columns.Contains("TimeRange"))
                {
                    dt.Columns.Add("TimeRange", typeof(string));
                }
                foreach (DataRow r in dt.Rows)
                {
                    string start = string.Empty;
                    string end = string.Empty;

                    if (r.Table.Columns.Contains("StartTime") && r["StartTime"] != DBNull.Value)
                    {
                        TimeSpan ts = (TimeSpan)r["StartTime"];
                        start = $"{ts.Hours:D2}:{ts.Minutes:D2}";
                    }
                    if (r.Table.Columns.Contains("EndTime") && r["EndTime"] != DBNull.Value)
                    {
                        TimeSpan ts = (TimeSpan)r["EndTime"];
                        end = $"{ts.Hours:D2}:{ts.Minutes:D2}";
                    }

                    r["TimeRange"] = string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end) ? string.Empty : start + " - " + end;
                }

                dgvSchedules.DataSource = dt;

                if (dgvSchedules.Columns.Count > 0)
                {
                    // Hide internal id / columns if they exist
                    dgvSchedules.TrySetColumnVisible("ScheduleId", false);
                    dgvSchedules.TrySetColumnVisible("CourseId", false);
                    dgvSchedules.TrySetColumnVisible("DayOfWeek", false);
                    dgvSchedules.TrySetColumnVisible("TimeSlot", false);
                    dgvSchedules.TrySetColumnVisible("StartTime", false);
                    dgvSchedules.TrySetColumnVisible("EndTime", false);

                    // Header text - only if columns exist
                    dgvSchedules.TrySetHeaderText("CourseCode", "Mã môn");
                    dgvSchedules.TrySetHeaderText("CourseName", "Tên môn học");
                    dgvSchedules.TrySetHeaderText("Semester", "Học kỳ");
                    dgvSchedules.TrySetHeaderText("DayName", "Thứ");
                    dgvSchedules.TrySetHeaderText("TimeSlotName", "Tiết học");
                        dgvSchedules.DataSource = dt;
                        if (dgvSchedules.Columns.Contains("TimeRange")) dgvSchedules.Columns["TimeRange"].HeaderText = "Giờ";
                    dgvSchedules.TrySetHeaderText("TeacherName", "Giảng viên");

                    // Resize columns
                    try
                    {
                        ScheduleManagementForm_Resize(this, EventArgs.Empty);
                    }
                    catch
                    {
                        // Ignore resize errors on initial load
                    }
                }

                // Update status
                if (panelContent != null)
                {
                    Label lblCount = panelContent.Controls.OfType<Label>()
                        .FirstOrDefault(l => l.Name == "lblCount");
                    if (lblCount == null)
                    {
                        lblCount = new Label
                        {
                            Name = "lblCount",
                            Font = new Font("Segoe UI", 9),
                            ForeColor = Color.FromArgb(107, 114, 128),
                            AutoSize = true,
                            Location = new Point(30, 5)
                        };
                        panelContent.Controls.Add(lblCount);
                        lblCount.BringToFront();
                    }
                    lblCount.Text = $"Tổng số: {dt.Rows.Count} lịch học";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách lịch học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSchedules_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvSchedules.Columns[e.ColumnIndex].Name == "Room")
            {
                if (e.Value == null || string.IsNullOrEmpty(e.Value.ToString()))
                {
                    e.Value = "Chưa có";
                    e.CellStyle.ForeColor = Color.FromArgb(156, 163, 175);
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                }
            }
        }

        private void BtnAddSchedule_Click(object sender, EventArgs e)
        {
            ScheduleEditForm form = new ScheduleEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadSchedules();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvSchedules.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn lịch học cần sửa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int scheduleId = Convert.ToInt32(dgvSchedules.SelectedRows[0].Cells["ScheduleId"].Value);
            ScheduleEditForm form = new ScheduleEditForm(scheduleId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadSchedules();
            }
        }

        private void DgvSchedules_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnEdit_Click(sender, e);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSchedules.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn lịch học cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa lịch học này?\nLưu ý: Sinh viên sẽ không thấy lịch học này nữa!",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int scheduleId = Convert.ToInt32(dgvSchedules.SelectedRows[0].Cells["ScheduleId"].Value);
                    string query = "DELETE FROM Schedules WHERE ScheduleId = @ScheduleId";
                    DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@ScheduleId", scheduleId) });
                    MessageBox.Show("Xóa lịch học thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSchedules();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa lịch học: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnAutoGenerate_Click(object sender, EventArgs e)
        {
            AutoScheduleForm form = new AutoScheduleForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadSchedules();
            }
        }

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            string semester = cboFilterSemester.SelectedIndex > 0 ?
                cboFilterSemester.SelectedItem.ToString() : null;
            ScheduleGridViewForm form = new ScheduleGridViewForm(semester);
            form.ShowDialog();
        }

        private void EnsureSchedulesTableExists()
        {
            try
            {
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
                        );

                        CREATE INDEX IX_Schedules_CourseId ON Schedules(CourseId);
                        CREATE INDEX IX_Schedules_DayOfWeek ON Schedules(DayOfWeek);
                    END";

                DatabaseHelper.ExecuteNonQuery(checkTable);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating Schedules table: {ex.Message}");
            }
        }
    }

    // Helper class for ComboBox items
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
