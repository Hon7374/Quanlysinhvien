using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    /// <summary>
    /// Form hiển thị lịch dạy học của giảng viên (READ-ONLY)
    /// Giảng viên CHỈ xem được lịch dạy của chính họ, KHÔNG được phân lịch hay sửa lịch
    /// </summary>
    public partial class TeacherScheduleViewForm : Form
    {
        private Panel panelHeader;
        private Panel panelFilters;
        private Panel panelContent;
        private DataGridView dgvSchedules;
        private ComboBox cboFilterSemester;

        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };

        public TeacherScheduleViewForm()
        {
            InitializeComponent();
            LoadFilters();
            LoadSchedules();
        }

        private void InitializeComponent()
        {
            this.Text = "Lịch dạy của tôi - Teacher Schedule";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "📅 LỊCH DẠY HỌC CỦA TÔI",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(39, 174, 96)
            };
            panelHeader.Controls.Add(lblTitle);

            // Info Label
            Label lblInfo = new Label
            {
                Text = "Xem lịch dạy học của bạn theo học kỳ",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Location = new Point(30, 60),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelHeader.Controls.Add(lblInfo);

            // View Grid Button
            Button btnViewGrid = new Button
            {
                Text = "📅 Xem lưới TKB",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 75),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(59, 130, 246),
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
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(30, 10, 30, 10)
            };

            Label lblFilterSemester = new Label
            {
                Text = "Học kỳ:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblFilterSemester);

            cboFilterSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(100, 17),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilterSemester.SelectedIndexChanged += (s, e) => LoadSchedules();
            panelFilters.Controls.Add(cboFilterSemester);

            Button btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Font = new Font("Segoe UI", 9),
                Location = new Point(320, 15),
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
                Padding = new Padding(30)
            };

            // DataGridView
            dgvSchedules = new DataGridView
            {
                Dock = DockStyle.Fill,
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
                ColumnHeadersHeight = 50
            };

            dgvSchedules.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0)
            };

            dgvSchedules.DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(200, 247, 220),
                SelectionForeColor = Color.FromArgb(31, 41, 55),
                Padding = new Padding(10, 5, 10, 5)
            };

            dgvSchedules.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251)
            };

            dgvSchedules.CellFormatting += DgvSchedules_CellFormatting;

            panelContent.Controls.Add(dgvSchedules);
            this.Controls.Add(panelContent);
        }

        private void LoadFilters()
        {
            try
            {
                // Load Semesters cho môn học của giảng viên
                cboFilterSemester.Items.Clear();
                cboFilterSemester.Items.Add("-- Tất cả học kỳ --");

                string semesterQuery = @"
                    SELECT DISTINCT c.Semester
                    FROM Courses c
                    WHERE c.TeacherId = @TeacherId
                    AND c.IsActive = 1
                    ORDER BY c.Semester DESC";

                DataTable semesters = DatabaseHelper.ExecuteQuery(semesterQuery,
                    new SqlParameter[] { new SqlParameter("@TeacherId", SessionManager.CurrentTeacher.TeacherId) });

                foreach (DataRow row in semesters.Rows)
                {
                    cboFilterSemester.Items.Add(row["Semester"].ToString());
                }

                cboFilterSemester.SelectedIndex = 0;
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
                // CHỈ LẤY LỊCH DẠY CỦA GIẢNG VIÊN HIỆN TẠI
                string query = @"
                    SELECT
                        s.ScheduleId,
                        c.CourseCode,
                        c.CourseName,
                        c.Semester,
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
                        (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) AS StudentCount
                    FROM Schedules s
                    INNER JOIN Courses c ON s.CourseId = c.CourseId
                    WHERE c.TeacherId = @TeacherId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@TeacherId", SessionManager.CurrentTeacher.TeacherId)
                };

                // Filter by semester nếu được chọn
                if (cboFilterSemester != null && cboFilterSemester.SelectedIndex > 0 && cboFilterSemester.SelectedItem != null)
                {
                    query += " AND c.Semester = @Semester";
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@TeacherId", SessionManager.CurrentTeacher.TeacherId),
                        new SqlParameter("@Semester", cboFilterSemester.SelectedItem.ToString())
                    };
                }

                query += " ORDER BY c.Semester DESC, s.DayOfWeek, s.TimeSlot";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                // Add computed TimeRange column
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
                    // Hide internal columns
                    if (dgvSchedules.Columns.Contains("ScheduleId"))
                        dgvSchedules.Columns["ScheduleId"].Visible = false;
                    if (dgvSchedules.Columns.Contains("TimeSlot"))
                        dgvSchedules.Columns["TimeSlot"].Visible = false;
                    if (dgvSchedules.Columns.Contains("StartTime"))
                        dgvSchedules.Columns["StartTime"].Visible = false;
                    if (dgvSchedules.Columns.Contains("EndTime"))
                        dgvSchedules.Columns["EndTime"].Visible = false;

                    // Set header text
                    if (dgvSchedules.Columns.Contains("CourseCode"))
                        dgvSchedules.Columns["CourseCode"].HeaderText = "Mã môn";
                    if (dgvSchedules.Columns.Contains("CourseName"))
                        dgvSchedules.Columns["CourseName"].HeaderText = "Tên môn học";
                    if (dgvSchedules.Columns.Contains("Semester"))
                        dgvSchedules.Columns["Semester"].HeaderText = "Học kỳ";
                    if (dgvSchedules.Columns.Contains("DayName"))
                        dgvSchedules.Columns["DayName"].HeaderText = "Thứ";
                    if (dgvSchedules.Columns.Contains("TimeSlotName"))
                        dgvSchedules.Columns["TimeSlotName"].HeaderText = "Tiết học";
                    if (dgvSchedules.Columns.Contains("TimeRange"))
                        dgvSchedules.Columns["TimeRange"].HeaderText = "Giờ học";
                    if (dgvSchedules.Columns.Contains("Room"))
                        dgvSchedules.Columns["Room"].HeaderText = "Phòng";
                    if (dgvSchedules.Columns.Contains("StudentCount"))
                        dgvSchedules.Columns["StudentCount"].HeaderText = "Số SV";
                }

                // Show count
                Label lblCount = new Label
                {
                    Text = $"📊 Tổng số: {dt.Rows.Count} buổi dạy",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(39, 174, 96),
                    AutoSize = true,
                    Location = new Point(30, panelContent.Height - 60)
                };

                // Remove old count label if exists
                foreach (Control ctrl in panelContent.Controls)
                {
                    if (ctrl is Label lbl && lbl.Text.StartsWith("📊"))
                    {
                        panelContent.Controls.Remove(ctrl);
                        break;
                    }
                }

                panelContent.Controls.Add(lblCount);
                lblCount.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch dạy: " + ex.Message, "Lỗi",
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

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                string semester = cboFilterSemester.SelectedIndex > 0 ?
                    cboFilterSemester.SelectedItem.ToString() : null;

                // Tạo form xem lưới TKB chỉ cho giảng viên hiện tại
                TeacherScheduleGridViewForm form = new TeacherScheduleGridViewForm(
                    SessionManager.CurrentTeacher.TeacherId,
                    semester
                );
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở lưới thời khóa biểu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
