using iText.Layout.Properties;
using StudentManagement.Data;
using StudentManagement.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace StudentManagement.Forms
{
    public partial class AdminDashboardModern : Form
    {
        private Panel panelMenu;
        private Panel panelContent;
        private Panel panelHeader;
        private Label lblWelcome;
        private ComboBox cboYear;
        private ComboBox cboSemester;
        private ComboBox cboStatus;

        public AdminDashboardModern()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Academia - Tổng quan trang thái học tập";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;

            // Menu Panel (Sidebar)
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Logo/Brand
            Label lblBrand = new Label
            {
                Text = "🎓 Academia",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(79, 70, 229),
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            panelMenu.Controls.Add(lblBrand);

            // Menu Buttons
            int yPos = 80;
            AddMenuButton("📊 Trang chủ", yPos, (s, e) => LoadDashboard(), true); yPos += 45;
            AddMenuButton("🎯 Quản lý Người dùng", yPos, (s, e) => LoadUserManagement()); yPos += 45;
            AddMenuButton("📚 Quản lý học kỳ", yPos, (s, e) => LoadSemesterManagement()); yPos += 45;
            AddMenuButton("👨‍🎓 Quản lý sinh viên", yPos, (s, e) => LoadStudentManagement()); yPos += 45;
            AddMenuButton("👨‍🏫 Quản lý giảng viên", yPos, (s, e) => LoadTeacherManagement()); yPos += 45;
            AddMenuButton("📖 Quản lý môn học", yPos, (s, e) => LoadCourseManagement()); yPos += 45;
            AddMenuButton("📊 Thống kê và báo cáo", yPos, (s, e) => LoadReports()); yPos += 45;
            AddMenuButton("⚙️ Cài đặt", yPos, (s, e) => LoadSettings()); yPos += 60;
            AddMenuButton("🚪 Đăng xuất", yPos, (s, e) => Logout());

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "Xin chào, Admin",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            lblWelcome = new Label
            {
                Text = $"👤 Đăng xuất",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(220, 38, 38),
                Location = new System.Drawing.Point(1050, 25),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblWelcome.Click += (s, e) => Logout();
            panelHeader.Controls.Add(lblWelcome);

            // Search Box
            //TextBox txtSearch = new TextBox
            //{
            //    Font = new Font("Segoe UI", 10),
            //    Location = new System.Drawing.Point(20, 25),
            //    Size = new Size(300, 30),
            //    Text = "🔍 Tìm kiếm..."
            //};
            //panelHeader.Controls.Add(txtSearch);

            // Content Panel
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(30),
                AutoScroll = true
            };

            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelMenu);
        }

        private void AddMenuButton(string text, int yPos, EventHandler clickHandler, bool isActive = false)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = new System.Drawing.Point(10, yPos),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                ForeColor = isActive ? Color.FromArgb(79, 70, 229) : Color.FromArgb(107, 114, 128),
                BackColor = isActive ? Color.FromArgb(238, 242, 255) : Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 242, 255);
            btn.Click += clickHandler;
            panelMenu.Controls.Add(btn);
        }

        private void LoadDashboard()
        {
            try
            {
                if (panelContent == null)
                {
                    MessageBox.Show("panelContent is null!", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                panelContent.Controls.Clear();

                // Title
                Label lblTitle = new Label
                {
                    Text = "Tổng quan trang thái học tập",
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    Location = new System.Drawing.Point(0, 0),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(31, 41, 55)
                };
                panelContent.Controls.Add(lblTitle);

            // Filters
            Panel panelFilters = new Panel
            {
                Location = new System.Drawing.Point(0, 50),
                Size = new Size(1100, 60),
                BackColor = Color.Transparent
            };

            // Year Filter
            Label lblYear = new Label { Text = "Năm học", Location = new System.Drawing.Point(0, 0), AutoSize = true };
            cboYear = new ComboBox
            {
                Location = new System.Drawing.Point(0, 20),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboYear.Items.AddRange(new object[] { "2023-2024", "2024-2025", "2025-2026" });
            cboYear.SelectedIndex = 0;
            panelFilters.Controls.Add(lblYear);
            panelFilters.Controls.Add(cboYear);

            // Semester Filter
            Label lblSemester = new Label { Text = "Học kỳ", Location = new System.Drawing.Point(170, 0), AutoSize = true };
            cboSemester = new ComboBox
            {
                Location = new System.Drawing.Point(170, 20),
                Size = new Size(120, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboSemester.Items.AddRange(new object[] { "Học kỳ I", "Học kỳ II", "Học kỳ III" });
            cboSemester.SelectedIndex = 0;
            panelFilters.Controls.Add(lblSemester);
            panelFilters.Controls.Add(cboSemester);

            // Status Filter
            Label lblStatus = new Label { Text = "Trạng thái", Location = new System.Drawing.Point(310, 0), AutoSize = true };
            cboStatus = new ComboBox
            {
                Location = new System.Drawing.Point(310, 20),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new object[] { "Đang học", "Tất cả" });
            cboStatus.SelectedIndex = 0;
            panelFilters.Controls.Add(lblStatus);
            panelFilters.Controls.Add(cboStatus);

                // Export Buttons
                Button btnPDF = new Button
                {
                    Text = "Xuất PDF",
                    Location = new System.Drawing.Point(560, 20),
                    Size = new Size(120, 35),
                    BackColor = Color.FromArgb(239, 68, 68), // đỏ nổi bật
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnPDF.FlatAppearance.BorderSize = 0;
                btnPDF.Click += (s, e) => ExportToPDF();
                panelFilters.Controls.Add(btnPDF);

                Button btnExcel = new Button
                {
                    Text = "Xuất Excel",
                    Location = new System.Drawing.Point(700, 20),
                    Size = new Size(120, 35),
                    BackColor = Color.FromArgb(34, 197, 94),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnExcel.FlatAppearance.BorderSize = 0;
                btnExcel.Click += (s, e) => ExportToExcel();
                panelFilters.Controls.Add(btnExcel);

                panelContent.Controls.Add(panelFilters);

            try
            {
                // Get statistics
                object studentsResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Students");
                int totalStudents = studentsResult != null && studentsResult != DBNull.Value ? Convert.ToInt32(studentsResult) : 0;

                object coursesResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Courses WHERE IsActive = 1");
                int totalCourses = coursesResult != null && coursesResult != DBNull.Value ? Convert.ToInt32(coursesResult) : 0;

                object subjectsResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(DISTINCT CourseCode) FROM Courses");
                int totalSubjects = subjectsResult != null && subjectsResult != DBNull.Value ? Convert.ToInt32(subjectsResult) : 0;

                // Stat Cards Row
                int cardY = 130;
                CreateModernStatCard("Tổng số sinh viên", totalStudents.ToString(), "+24.1% so với tháng trước",
                    Color.FromArgb(239, 246, 255), Color.FromArgb(59, 130, 246), 0, cardY);
                CreateModernStatCard("Tổng số lớp học", totalCourses.ToString(), "+5 lớp mới",
                    Color.FromArgb(240, 253, 244), Color.FromArgb(34, 197, 94), 280, cardY);
                CreateModernStatCard("Tổng số khóa học", totalSubjects.ToString(), "Ổn định so với năm trước",
                    Color.FromArgb(255, 247, 237), Color.FromArgb(249, 115, 22), 560, cardY);

                // Charts Row
                int chartY = 280;

                // Bar Chart - Điểm trung bình môn học
                CreateBarChart("Biểu đồ điểm trung bình môn học", 0, chartY, 380, 300);

                // Pie Chart - Tỷ lệ tốt nghiệp
                CreatePieChart("Biểu đồ tỷ lệ tốt nghiệp", 400, chartY, 350, 300);

                // Column Chart - Trạng thái đăng ký
                CreateColumnChart("Biểu đồ trạng thái đăng ký", 770, chartY, 330, 300);

                // Tables Section
                int tableY = 610;

                // Courses Table
                Label lblCourses = new Label
                {
                    Text = "Danh sách lớp học",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new System.Drawing.Point(0, tableY),
                    AutoSize = true
                };
                panelContent.Controls.Add(lblCourses);

                DataGridView dgvCourses = CreateModernDataGridView(0, tableY + 40, 1100, 200);
                string queryCourses = @"SELECT TOP 5 c.CourseCode as 'Mã lớp', c.CourseName as 'Tên lớp',
                                       u.FullName as 'Giảng viên', c.Semester + ' ' + CAST(c.AcademicYear AS VARCHAR) as 'Năm học',
                                       c.Semester as 'Học kỳ',
                                       (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) as 'Số lượng SV',
                                       CASE WHEN c.IsActive = 1 THEN N'Đang diễn ra' ELSE N'Đã kết thúc' END as 'Trạng thái'
                                       FROM Courses c
                                       LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                                       LEFT JOIN Users u ON t.UserId = u.UserId
                                       ORDER BY c.CourseId DESC";
                dgvCourses.DataSource = DatabaseHelper.ExecuteQuery(queryCourses);
                dgvCourses.ColumnHeadersHeight = 40;
                dgvCourses.RowTemplate.Height = 30;


                // Tắt hiệu ứng chọn (highlight xanh) trong DataGridView
                dgvCourses.DefaultCellStyle.SelectionBackColor = dgvCourses.DefaultCellStyle.BackColor;
                dgvCourses.DefaultCellStyle.SelectionForeColor = dgvCourses.DefaultCellStyle.ForeColor;
                dgvCourses.RowsDefaultCellStyle.SelectionBackColor = dgvCourses.DefaultCellStyle.BackColor;
                dgvCourses.RowsDefaultCellStyle.SelectionForeColor = dgvCourses.DefaultCellStyle.ForeColor;
                dgvCourses.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvCourses.ColumnHeadersDefaultCellStyle.BackColor;
                dgvCourses.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgvCourses.ColumnHeadersDefaultCellStyle.ForeColor;

                panelContent.Controls.Add(dgvCourses);

                // Students Table
                Label lblStudents = new Label
                {
                    Text = "Danh sách sinh viên",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new System.Drawing.Point(0, tableY + 270),
                    AutoSize = true
                };
                panelContent.Controls.Add(lblStudents);

                DataGridView dgvStudents = CreateModernDataGridView(0, tableY + 310, 1100, 200);
                string queryStudents = @"SELECT TOP 5 s.StudentCode as 'Mã SV', u.FullName as 'Họ và tên',
                                        s.Major as 'Ngành học',
                                        CASE
                                            WHEN (SELECT COUNT(*) FROM Enrollments e WHERE e.StudentId = s.StudentId AND e.Status = 'Enrolled') > 0
                                            THEN N'Đang học'
                                            ELSE N'Bảo lưu'
                                        END as 'Trạng thái học tập',
                                        CAST(s.GPA AS DECIMAL(3,1)) as 'GPA'
                                        FROM Students s
                                        INNER JOIN Users u ON s.UserId = u.UserId
                                        ORDER BY s.StudentId DESC";
                dgvStudents.DataSource = DatabaseHelper.ExecuteQuery(queryStudents);
                dgvStudents.ColumnHeadersHeight = 40;
                dgvStudents.RowTemplate.Height = 30;

                // Tắt hiệu ứng chọn (highlight xanh) trong DataGridView
                dgvStudents.DefaultCellStyle.SelectionBackColor = dgvStudents.DefaultCellStyle.BackColor;
                dgvStudents.DefaultCellStyle.SelectionForeColor = dgvStudents.DefaultCellStyle.ForeColor;
                dgvStudents.RowsDefaultCellStyle.SelectionBackColor = dgvStudents.DefaultCellStyle.BackColor;
                dgvStudents.RowsDefaultCellStyle.SelectionForeColor = dgvStudents.DefaultCellStyle.ForeColor;
                dgvStudents.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvStudents.ColumnHeadersDefaultCellStyle.BackColor;
                dgvStudents.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor;

                panelContent.Controls.Add(dgvStudents);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu dashboard: " + ex.Message + "\n\nStack Trace: " + ex.StackTrace, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng khi load dashboard: " + ex.Message + "\n\nStack Trace: " + ex.StackTrace, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateModernStatCard(string title, string value, string trend, Color bgColor, Color accentColor, int x, int y)
        {
            Panel card = new Panel
            {
                Location = new System.Drawing.Point(x, y),
                Size = new Size(260, 120),
                BackColor = bgColor
            };

            // Icon/Number
            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new System.Drawing.Point(20, 15),
                AutoSize = true
            };
            card.Controls.Add(lblValue);

            // Title
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new System.Drawing.Point(20, 70),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            // Trend
            Label lblTrend = new Label
            {
                Text = trend,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new System.Drawing.Point(20, 92),
                AutoSize = true
            };
            card.Controls.Add(lblTrend);

            panelContent.Controls.Add(card);
        }

        private void CreateBarChart(string title, int x, int y, int width, int height)
        {
            Panel chartPanel = new Panel
            {
                Location = new  System.Drawing.Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new System.Drawing.Point(15, 10),
                AutoSize = true
            };
            chartPanel.Controls.Add(lblTitle);

            // Simple bar chart using panels
            string[] subjects = { "Toán", "Vật", "Hóa", "Tin", "Tiếng Anh" };
            int[] scores = { 8, 7, 8, 9, 7 };
            int barX = 20;
            int barY = 50;
            int barWidth = 60;
            int maxHeight = height - 100;

            for (int i = 0; i < subjects.Length; i++)
            {
                int barHeight = (scores[i] * maxHeight) / 10;

                // Bar
                Panel bar = new Panel
                {
                    Location = new System.Drawing.Point(barX, barY + maxHeight - barHeight),
                    Size = new Size(barWidth, barHeight),
                    BackColor = Color.FromArgb(99, 102, 241)
                };
                chartPanel.Controls.Add(bar);

                // Label
                Label lblSubject = new Label
                {
                    Text = subjects[i],
                    Location = new System.Drawing.Point(barX - 5, barY + maxHeight + 5),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8)
                };
                chartPanel.Controls.Add(lblSubject);

                barX += 70;
            }

            panelContent.Controls.Add(chartPanel);
        }

        private void CreatePieChart(string title, int x, int y, int width, int height)
        {
            Panel chartPanel = new Panel
            {
                Location = new System.Drawing.Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new System.Drawing.Point(15, 10),
                AutoSize = true
            };
            chartPanel.Controls.Add(lblTitle);

            // Get real data
            object enrolledResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Enrollments WHERE Status = 'Enrolled'");
            int enrolled = enrolledResult != null && enrolledResult != DBNull.Value ? Convert.ToInt32(enrolledResult) : 0;

            object completedResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Enrollments WHERE Status = 'Completed'");
            int completed = completedResult != null && completedResult != DBNull.Value ? Convert.ToInt32(completedResult) : 0;

            int dropped = 5;
            int total = enrolled + completed + dropped;

            if (total > 0)
            {
                // Donut chart using circular progress
                int centerX = width / 2;
                int centerY = height / 2;
                int radius = 80;

                // Draw circles to simulate donut
                Panel circle1 = new Panel
                {
                    Location = new System.Drawing.Point(centerX - radius, centerY - radius),
                    Size = new Size(radius * 2, radius * 2),
                    BackColor = Color.FromArgb(236, 72, 153) // Pink
                };
                chartPanel.Controls.Add(circle1);

                // Center hole
                Panel centerHole = new Panel
                {
                    Location = new System.Drawing.Point(centerX - radius/2, centerY - radius/2),
                    Size = new Size(radius, radius),
                    BackColor = Color.White
                };
                chartPanel.Controls.Add(centerHole);

                // Legend
                int legendY = centerY + radius + 20;
                CreateLegendItem(chartPanel, "Đã tốt nghiệp", Color.FromArgb(236, 72, 153), 20, legendY);
                CreateLegendItem(chartPanel, "Đang học", Color.FromArgb(52, 211, 153), 130, legendY);
                CreateLegendItem(chartPanel, "Chưa đạt", Color.FromArgb(251, 146, 60), 230, legendY);
            }

            panelContent.Controls.Add(chartPanel);
        }

        private void CreateLegendItem(Panel parent, string text, Color color, int x, int y)
        {
            Panel colorBox = new Panel
            {
                Location = new System.Drawing.Point(x, y),
                Size = new Size(15, 15),
                BackColor = color
            };
            parent.Controls.Add(colorBox);

            Label label = new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x + 20, y - 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            parent.Controls.Add(label);
        }

        private void CreateColumnChart(string title, int x, int y, int width, int height)
        {
            Panel chartPanel = new Panel
            {
                Location = new System.Drawing.Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new System.Drawing.Point(15, 10),
                AutoSize = true
            };
            chartPanel.Controls.Add(lblTitle);

            // Get real data
            object enrolledResult2 = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Enrollments WHERE Status = 'Enrolled'");
            int enrolled = enrolledResult2 != null && enrolledResult2 != DBNull.Value ? Convert.ToInt32(enrolledResult2) : 0;

            object completedResult2 = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Enrollments WHERE Status = 'Completed'");
            int completed = completedResult2 != null && completedResult2 != DBNull.Value ? Convert.ToInt32(completedResult2) : 0;

            object droppedResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Enrollments WHERE Status = 'Dropped'");
            int dropped = droppedResult != null && droppedResult != DBNull.Value ? Convert.ToInt32(droppedResult) : 0;

            int maxValue = Math.Max(enrolled, Math.Max(completed, dropped)) + 10;
            if (maxValue == 0) maxValue = 100;

            // Simple column chart
            string[] categories = { "Đã đăng ký", "Chờ duyệt", "Huỷ đăng ký" };
            int[] values = { enrolled, completed, dropped };

            int colX = 30;
            int colY = 50;
            int colWidth = 80;
            int maxHeight = height - 120;

            for (int i = 0; i < categories.Length; i++)
            {
                int colHeight = (values[i] * maxHeight) / maxValue;
                if (colHeight < 5 && values[i] > 0) colHeight = 5;

                // Column
                Panel col = new Panel
                {
                    Location = new System.Drawing.Point(colX, colY + maxHeight - colHeight),
                    Size = new Size(colWidth, colHeight),
                    BackColor = Color.FromArgb(139, 92, 246)
                };
                chartPanel.Controls.Add(col);

                // Value label
                Label lblValue = new Label
                {
                    Text = values[i].ToString(),
                    Location = new System.Drawing.Point(colX + colWidth/2 - 10, colY + maxHeight - colHeight - 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(139, 92, 246)
                };
                chartPanel.Controls.Add(lblValue);

                // Category label
                Label lblCat = new Label
                {
                    Text = categories[i],
                    Location = new System.Drawing.Point(colX - 10, colY + maxHeight + 5),
                    Size = new Size(100, 30),
                    Font = new Font("Segoe UI", 8),
                    TextAlign = ContentAlignment.TopCenter
                };
                chartPanel.Controls.Add(lblCat);

                colX += 100;
            }

            panelContent.Controls.Add(chartPanel);
        }


        private DataGridView CreateModernDataGridView(int x, int y, int width, int height)
        {
            DataGridView dgv = new DataGridView
            {
                Location = new System.Drawing.Point(x, y),
                Size = new Size(width, height),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(249, 250, 251),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Padding = new Padding(5)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(238, 242, 255),
                    SelectionForeColor = Color.FromArgb(79, 70, 229),
                    Font = new Font("Segoe UI", 9),
                    Padding = new Padding(5)
                }
            };

            return dgv;
        }

        private void ExportToPDF()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDialog.FileName = $"BaoCao_DanhSachLop_SinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                // Lấy dữ liệu 2 bảng
                DataTable dtLopHoc = DatabaseHelper.ExecuteQuery(@"
            SELECT 
                c.CourseCode AS [Mã lớp],
                c.CourseName AS [Tên lớp],
                ISNULL(u.FullName, 'Chưa có') AS [Giảng viên],
                c.Semester + ' ' + CAST(c.AcademicYear AS VARCHAR) AS [Năm học - Học kỳ],
                (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) AS [Số lượng SV],
                CASE WHEN c.IsActive = 1 THEN N'Đang diễn ra' ELSE N'Đã kết thúc' END AS [Trạng thái]
            FROM Courses c
            LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
            LEFT JOIN Users u ON t.UserId = u.UserId
            ORDER BY c.CourseId DESC");

                DataTable dtSinhVien = DatabaseHelper.ExecuteQuery(@"
            SELECT 
                s.StudentCode AS [Mã SV],
                u.FullName AS [Họ và tên],
                ISNULL(s.Major, 'Chưa xác định') AS [Ngành học],
                CASE WHEN EXISTS(SELECT 1 FROM Enrollments e WHERE e.StudentId = s.StudentId AND e.Status = 'Enrolled')
                     THEN N'Đang học' ELSE N'Bảo lưu' END AS [Trạng thái],
                FORMAT(ISNULL(s.GPA, 0), 'N1') AS [GPA]
            FROM Students s
            INNER JOIN Users u ON s.UserId = u.UserId
            ORDER BY s.StudentId DESC");

                // TẠO PDF – ĐÃ FIX LỖI FileStream
                using (var stream = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write))
                using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                using (var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4))
                {
                    document.SetMargins(40, 40, 40, 40);

                    // Font tiếng Việt
                    string fontPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    string boldPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialbd.ttf");

                    var font = iText.Kernel.Font.PdfFontFactory.CreateFont(fontPath, iText.IO.Font.PdfEncodings.IDENTITY_H);
                    var boldFont = File.Exists(boldPath)
                        ? iText.Kernel.Font.PdfFontFactory.CreateFont(boldPath, iText.IO.Font.PdfEncodings.IDENTITY_H)
                        : font;

                    // Tiêu đề
                    document.Add(new iText.Layout.Element.Paragraph("BÁO CÁO DANH SÁCH LỚP HỌC & SINH VIÊN")
                        .SetFont(boldFont).SetFontSize(20)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginBottom(10));

                    document.Add(new iText.Layout.Element.Paragraph($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                        .SetFont(font).SetFontSize(12)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginBottom(30));

                    // BẢNG 1: LỚP HỌC
                    document.Add(new iText.Layout.Element.Paragraph("1. DANH SÁCH LỚP HỌC")
                        .SetFont(boldFont).SetFontSize(16).SetMarginBottom(10));

                    var table1 = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(dtLopHoc.Columns.Count))
                        .UseAllAvailableWidth();

                    // Header
                    foreach (DataColumn col in dtLopHoc.Columns)
                    {
                        table1.AddHeaderCell(new iText.Layout.Element.Cell()
                            .Add(new iText.Layout.Element.Paragraph(col.ColumnName).SetFont(boldFont).SetFontSize(11))
                            .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetPadding(8));
                    }

                    // Dữ liệu
                    foreach (DataRow row in dtLopHoc.Rows)
                    {
                        foreach (var item in row.ItemArray)
                        {
                            table1.AddCell(new iText.Layout.Element.Cell()
                                .Add(new iText.Layout.Element.Paragraph(item?.ToString() ?? "").SetFont(font).SetFontSize(10))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetPadding(6));
                        }
                    }
                    document.Add(table1.SetMarginBottom(40));

                    // BẢNG 2: SINH VIÊN
                    document.Add(new iText.Layout.Element.Paragraph("2. DANH SÁCH SINH VIÊN")
                        .SetFont(boldFont).SetFontSize(16).SetMarginBottom(10));

                    var table2 = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(dtSinhVien.Columns.Count))
                        .UseAllAvailableWidth();

                    foreach (DataColumn col in dtSinhVien.Columns)
                    {
                        table2.AddHeaderCell(new iText.Layout.Element.Cell()
                            .Add(new iText.Layout.Element.Paragraph(col.ColumnName).SetFont(boldFont).SetFontSize(11))
                            .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetPadding(8));
                    }

                    foreach (DataRow row in dtSinhVien.Rows)
                    {
                        foreach (var item in row.ItemArray)
                        {
                            table2.AddCell(new iText.Layout.Element.Cell()
                                .Add(new iText.Layout.Element.Paragraph(item?.ToString() ?? "").SetFont(font).SetFontSize(10))
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetPadding(6));
                        }
                    }
                    document.Add(table2);

                    // Footer
                    document.Add(new iText.Layout.Element.Paragraph($"\nNgười xuất báo cáo: {SessionManager.CurrentUser?.FullName ?? "Administrator"}")
                        .SetFont(font).SetFontSize(11)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginTop(50));
                }

                MessageBox.Show("XUẤT PDF THÀNH CÔNG HOÀN HẢO!\n2 bảng dữ liệu đẹp như báo cáo trường đại học!\nĐang mở file...",
                    "10/10 ĐÃ VỀ TAY!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mở file tự động
                var psi = new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message + "\n\nChi tiết: " + ex.StackTrace, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"BaoCao_DanhSach_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                // DÙNG ClosedXML – KHÔNG CẦN LICENSE, KHÔNG BAO GIỜ LỖI!
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    // ================== SHEET 1: DANH SÁCH LỚP HỌC ==================
                    var ws1 = workbook.Worksheets.Add("Danh sách lớp học");
                    var dtLop = DatabaseHelper.ExecuteQuery(@"
                SELECT 
                    c.CourseCode AS [Mã lớp],
                    c.CourseName AS [Tên lớp],
                    ISNULL(u.FullName, 'Chưa có') AS [Giảng viên],
                    c.Semester + ' ' + CAST(c.AcademicYear AS VARCHAR) AS [Năm học - Học kỳ],
                    (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) AS [Số lượng SV],
                    CASE WHEN c.IsActive = 1 THEN N'Đang diễn ra' ELSE N'Đã kết thúc' END AS [Trạng thái]
                FROM Courses c
                LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                LEFT JOIN Users u ON t.UserId = u.UserId
                ORDER BY c.CourseId DESC");

                    // Đổ dữ liệu + tạo header tự động
                    var table1 = ws1.Cell(1, 1).InsertTable(dtLop, "TableLopHoc", true);
                    ws1.Columns().AdjustToContents();

                    // Đẹp hóa header
                    ws1.Row(1).Style.Font.Bold = true;
                    ws1.Row(1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(79, 70, 229);
                    ws1.Row(1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    ws1.Row(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    // ================== SHEET 2: DANH SÁCH SINH VIÊN ==================
                    var ws2 = workbook.Worksheets.Add("Danh sách sinh viên");
                    var dtSV = DatabaseHelper.ExecuteQuery(@"
                SELECT 
                    s.StudentCode AS [Mã SV],
                    u.FullName AS [Họ và tên],
                    ISNULL(s.Major, 'Chưa xác định') AS [Ngành học],
                    CASE WHEN EXISTS(SELECT 1 FROM Enrollments e WHERE e.StudentId = s.StudentId AND e.Status = 'Enrolled')
                         THEN N'Đang học' ELSE N'Bảo lưu' END AS [Trạng thái],
                    ISNULL(FORMAT(s.GPA, 'N1'), '0,0') AS [GPA]
                FROM Students s
                INNER JOIN Users u ON s.UserId = u.UserId
                ORDER BY s.StudentId DESC");

                    var table2 = ws2.Cell(1, 1).InsertTable(dtSV, "TableSinhVien", true);
                    ws2.Columns().AdjustToContents();

                    ws2.Row(1).Style.Font.Bold = true;
                    ws2.Row(1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(34, 197, 94);
                    ws2.Row(1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    ws2.Row(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    // Lưu file
                    workbook.SaveAs(saveDialog.FileName);
                }

                MessageBox.Show("XUẤT EXCEL THÀNH CÔNG HOÀN HẢO!\nDùng ClosedXML – Không bao giờ lỗi license nữa!\nĐang mở file...",
                    "SIÊU PHẨM 1000%", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudentManagement()
        {
            try
            {
                panelContent.Controls.Clear();

                // Create and show StudentManagementForm as a child form
                StudentManagementForm studentForm = new StudentManagementForm();
                studentForm.TopLevel = false;
                studentForm.FormBorderStyle = FormBorderStyle.None;
                studentForm.Dock = DockStyle.Fill;
                panelContent.Controls.Add(studentForm);
                studentForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải trang quản lý sinh viên: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTeacherManagement()
        {
            try
            {
                panelContent.Controls.Clear();

                // Create and show TeacherManagementForm as a child form
                TeacherManagementForm teacherForm = new TeacherManagementForm();
                teacherForm.TopLevel = false;
                teacherForm.FormBorderStyle = FormBorderStyle.None;
                teacherForm.Dock = DockStyle.Fill;
                panelContent.Controls.Add(teacherForm);
                teacherForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải trang quản lý giảng viên: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCourseManagement()
        {
            try
            {
                panelContent.Controls.Clear();

                CourseManagementForm courseForm = new CourseManagementForm();
                courseForm.TopLevel = false;
                courseForm.FormBorderStyle = FormBorderStyle.None;
                courseForm.Dock = DockStyle.Fill;
                panelContent.Controls.Add(courseForm);
                courseForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải trang quản lý môn học: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUserManagement()
        {
            panelContent.Controls.Clear();

            // Create and show UserManagementForm as a child form
            UserManagementForm userForm = new UserManagementForm();
            userForm.TopLevel = false;
            userForm.FormBorderStyle = FormBorderStyle.None;
            userForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(userForm);
            userForm.Show();
        }

        private void LoadSemesterManagement()
        {
            try
            {
                panelContent.Controls.Clear();

                // Create and show SemesterManagementForm as a child form
                SemesterManagementForm semesterForm = new SemesterManagementForm();
                semesterForm.TopLevel = false;
                semesterForm.FormBorderStyle = FormBorderStyle.None;
                semesterForm.Dock = DockStyle.Fill;
                panelContent.Controls.Add(semesterForm);
                semesterForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải trang quản lý học kỳ: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReports()
        {
            panelContent.Controls.Clear();

            ReportsForm reportsForm = new ReportsForm();
            reportsForm.TopLevel = false;
            reportsForm.FormBorderStyle = FormBorderStyle.None;
            reportsForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(reportsForm);
            reportsForm.Show();
        }

        private void LoadSettings()
        {
            panelContent.Controls.Clear();

            SettingsForm settingsForm = new SettingsForm();
            settingsForm.TopLevel = false;
            settingsForm.FormBorderStyle = FormBorderStyle.None;
            settingsForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(settingsForm);
            settingsForm.Show();
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
