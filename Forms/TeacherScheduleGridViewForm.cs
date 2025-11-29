using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    /// <summary>
    /// Form hiển thị lưới thời khóa biểu cho GIẢNG VIÊN (READ-ONLY)
    /// Chỉ hiển thị lịch dạy của giảng viên, không có chức năng chỉnh sửa
    /// </summary>
    public partial class TeacherScheduleGridViewForm : Form
    {
        private int teacherId;
        private string selectedSemester;
        private Panel panelGrid;
        private Label lblTitle;

        private readonly string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        private readonly string[] sessionNames = { "Buổi sáng", "Buổi chiều", "Buổi tối" };

        public TeacherScheduleGridViewForm(int teacherId, string semester = null)
        {
            this.teacherId = teacherId;
            this.selectedSemester = semester;
            InitializeComponent();
            LoadScheduleGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Lưới Thời khóa biểu - Lịch dạy của tôi";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;

            // Header
            Panel panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(39, 174, 96)
            };

            lblTitle = new Label
            {
                Text = "📅 LƯỚI THỜI KHÓA BIỂU - LỊCH DẠY CỦA TÔI",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 20),
                AutoSize = true
            };
            panelHeader.Controls.Add(lblTitle);

            Label lblSemester = new Label
            {
                Text = string.IsNullOrEmpty(selectedSemester) ? "Tất cả học kỳ" : selectedSemester,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(30, 50),
                AutoSize = true
            };
            panelHeader.Controls.Add(lblSemester);

            this.Controls.Add(panelHeader);

            // Grid Panel
            panelGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                AutoScroll = true
            };
            this.Controls.Add(panelGrid);
        }

        private void LoadScheduleGrid()
        {
            try
            {
                panelGrid.Controls.Clear();

                // Lấy lịch dạy của giảng viên
                string query = @"
                    SELECT
                        s.ScheduleId,
                        c.CourseCode,
                        c.CourseName,
                        s.DayOfWeek,
                        s.TimeSlot,
                        s.Room
                    FROM Schedules s
                    INNER JOIN Courses c ON s.CourseId = c.CourseId
                    WHERE c.TeacherId = @TeacherId";

                SqlParameter[] parameters;
                if (!string.IsNullOrEmpty(selectedSemester))
                {
                    query += " AND c.Semester = @Semester";
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@TeacherId", teacherId),
                        new SqlParameter("@Semester", selectedSemester)
                    };
                }
                else
                {
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@TeacherId", teacherId)
                    };
                }

                DataTable schedules = DatabaseHelper.ExecuteQuery(query, parameters);

                if (schedules.Rows.Count == 0)
                {
                    Label lblEmpty = new Label
                    {
                        Text = "📭 Chưa có lịch dạy nào được phân bổ",
                        Font = new Font("Segoe UI", 14, FontStyle.Italic),
                        ForeColor = Color.FromArgb(156, 163, 175),
                        Location = new Point(100, 100),
                        AutoSize = true
                    };
                    panelGrid.Controls.Add(lblEmpty);
                    return;
                }

                CreateScheduleGrid(schedules);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lưới thời khóa biểu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateScheduleGrid(DataTable schedules)
        {
            int cellWidth = 180;
            int cellHeight = 120;
            int headerHeight = 60;
            int timeColumnWidth = 120;

            // Header row - Days of week
            Label lblTime = new Label
            {
                Text = "Ca học",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(timeColumnWidth, headerHeight),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelGrid.Controls.Add(lblTime);

            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                Label lblDay = new Label
                {
                    Text = daysOfWeek[i],
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(timeColumnWidth + (i * cellWidth), 0),
                    Size = new Size(cellWidth, headerHeight),
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle
                };
                panelGrid.Controls.Add(lblDay);
            }

            // 3 Sessions: Morning (0-4), Afternoon (5-9), Evening (10-13)
            int[] sessionStarts = { 0, 5, 10 };
            int[] sessionEnds = { 4, 9, 13 };
            int currentY = headerHeight;

            for (int session = 0; session < 3; session++)
            {
                // Session label
                Label lblSession = new Label
                {
                    Text = sessionNames[session],
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(0, currentY),
                    Size = new Size(timeColumnWidth, cellHeight),
                    BackColor = Color.FromArgb(248, 249, 250),
                    ForeColor = Color.FromArgb(39, 174, 96),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle
                };
                panelGrid.Controls.Add(lblSession);

                // Day cells for this session
                for (int day = 0; day < daysOfWeek.Length; day++)
                {
                    Panel cell = new Panel
                    {
                        Location = new Point(timeColumnWidth + (day * cellWidth), currentY),
                        Size = new Size(cellWidth, cellHeight),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Padding = new Padding(5)
                    };

                    // Add courses in this session and day
                    int yOffset = 5;
                    var coursesInCell = schedules.AsEnumerable()
                        .Where(r => Convert.ToInt32(r["DayOfWeek"]) == day &&
                                    Convert.ToInt32(r["TimeSlot"]) >= sessionStarts[session] &&
                                    Convert.ToInt32(r["TimeSlot"]) <= sessionEnds[session])
                        .GroupBy(r => r["CourseCode"].ToString())
                        .ToList();

                    foreach (var courseGroup in coursesInCell)
                    {
                        var firstSlot = courseGroup.OrderBy(r => Convert.ToInt32(r["TimeSlot"])).First();
                        var lastSlot = courseGroup.OrderBy(r => Convert.ToInt32(r["TimeSlot"])).Last();

                        int startSlot = Convert.ToInt32(firstSlot["TimeSlot"]) + 1;
                        int endSlot = Convert.ToInt32(lastSlot["TimeSlot"]) + 1;
                        string slotText = courseGroup.Count() > 1 ? $"Tiết {startSlot}-{endSlot}" : $"Tiết {startSlot}";

                        Panel coursePanel = new Panel
                        {
                            Location = new Point(5, yOffset),
                            Size = new Size(cell.Width - 10, 100),
                            BackColor = Color.FromArgb(220, 252, 231), // Light green
                            BorderStyle = BorderStyle.None,
                            Padding = new Padding(8)
                        };

                        // Course info
                        Label lblCourse = new Label
                        {
                            Text = $"{firstSlot["CourseCode"]}\n{firstSlot["CourseName"]}\n{slotText}",
                            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                            ForeColor = Color.FromArgb(21, 128, 61),
                            Location = new Point(5, 5),
                            Size = new Size(coursePanel.Width - 10, 60),
                            TextAlign = ContentAlignment.TopLeft
                        };
                        coursePanel.Controls.Add(lblCourse);

                        // Room info
                        string room = firstSlot["Room"] != DBNull.Value ? firstSlot["Room"].ToString() : "Chưa có";
                        Label lblRoom = new Label
                        {
                            Text = "📍 " + room,
                            Font = new Font("Segoe UI", 8f),
                            ForeColor = Color.FromArgb(107, 114, 128),
                            Location = new Point(5, 70),
                            AutoSize = true
                        };
                        coursePanel.Controls.Add(lblRoom);

                        cell.Controls.Add(coursePanel);
                        yOffset += 108;
                    }

                    panelGrid.Controls.Add(cell);
                }

                currentY += cellHeight;
            }
        }
    }
}
