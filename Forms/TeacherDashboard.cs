using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;
using ClosedXML.Excel;

namespace StudentManagement.Forms
{
    public partial class TeacherDashboard : Form
    {
        private Panel panelMenu;
        private Panel panelContent;
        private Label lblWelcome;

        // KHAI BÁO TOÀN CỤC – QUAN TRỌNG NHẤT!
        private ComboBox cboCourse;
        private DataGridView dgvStudents;

        public TeacherDashboard()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Giảng viên - Teacher Dashboard";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Menu Panel
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(39, 174, 96)
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
            AddMenuButton("📚 Môn học của tôi", yPos, (s, e) => LoadMyCourses()); yPos += 50;
            AddMenuButton("👨‍🎓 Danh sách Sinh viên", yPos, (s, e) => LoadStudentList()); yPos += 50;
            AddMenuButton("✏️ Nhập điểm", yPos, (s, e) => LoadGradeEntry()); yPos += 50;
            AddMenuButton("📋 Xem điểm đã nhập", yPos, (s, e) => LoadGradeView()); yPos += 50;
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
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 204, 113);
            btn.Click += clickHandler;
            panelMenu.Controls.Add(btn);
        }

        private void LoadDashboard()
        {
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "TỔNG QUAN GIẢNG VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(39, 174, 96)
            };
            panelContent.Controls.Add(lblTitle);

            try
            {
                int teacherId = SessionManager.CurrentTeacher.TeacherId;

                // Get statistics
                string queryMyCourses = "SELECT COUNT(*) FROM Courses WHERE TeacherId = @TeacherId AND IsActive = 1";
                int myCourses = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryMyCourses,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) }));

                string queryMyStudents = @"SELECT COUNT(DISTINCT e.StudentId)
                                          FROM Enrollments e
                                          INNER JOIN Courses c ON e.CourseId = c.CourseId
                                          WHERE c.TeacherId = @TeacherId";
                int myStudents = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryMyStudents,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) }));

                string queryPendingGrades = @"SELECT COUNT(*)
                                             FROM Enrollments e
                                             INNER JOIN Courses c ON e.CourseId = c.CourseId
                                             LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
                                             WHERE c.TeacherId = @TeacherId AND g.GradeId IS NULL";
                int pendingGrades = Convert.ToInt32(DatabaseHelper.ExecuteScalar(queryPendingGrades,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) }));

                // Create stat cards
                CreateStatCard("Môn học đang dạy", myCourses.ToString(), Color.FromArgb(52, 152, 219), 20, 80);
                CreateStatCard("Tổng số Sinh viên", myStudents.ToString(), Color.FromArgb(46, 204, 113), 260, 80);
                CreateStatCard("Chưa nhập điểm", pendingGrades.ToString(), Color.FromArgb(231, 76, 60), 500, 80);

                // Teacher info
                Label lblInfo = new Label
                {
                    Text = "THÔNG TIN GIẢNG VIÊN",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 230),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panelContent.Controls.Add(lblInfo);

                Panel infoPanel = new Panel
                {
                    Location = new Point(20, 270),
                    Size = new Size(900, 150),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                string info = $@"Mã giảng viên: {SessionManager.CurrentTeacher.TeacherCode}
Họ tên: {SessionManager.CurrentUser.FullName}
Khoa: {SessionManager.CurrentTeacher.Department}
Học vị: {SessionManager.CurrentTeacher.Degree}
Chuyên môn: {SessionManager.CurrentTeacher.Specialization}
Email: {SessionManager.CurrentUser.Email}
Số điện thoại: {SessionManager.CurrentUser.Phone}";

                Label lblDetails = new Label
                {
                    Text = info,
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, 5),
                    AutoSize = true
                };
                infoPanel.Controls.Add(lblDetails);
                panelContent.Controls.Add(infoPanel);

                // My courses list
                Label lblCourses = new Label
                {
                    Text = "MÔN HỌC ĐANG DẠY",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 440),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                panelContent.Controls.Add(lblCourses);

                DataGridView dgvCourses = new DataGridView
                {
                    Location = new Point(20, 480),
                    Size = new Size(900, 150),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None
                };

                string queryCourses = @"SELECT c.CourseCode, c.CourseName, c.Credits,
                                       c.Semester, c.AcademicYear,
                                       (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) as TotalStudents
                                       FROM Courses c
                                       WHERE c.TeacherId = @TeacherId AND c.IsActive = 1";
                dgvCourses.DataSource = DatabaseHelper.ExecuteQuery(queryCourses,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) });

                if (dgvCourses.Columns.Count >= 6)
                {
                    dgvCourses.Columns[0].HeaderText = "Mã môn";
                    dgvCourses.Columns[1].HeaderText = "Tên môn học";
                    dgvCourses.Columns[2].HeaderText = "Số tín chỉ";
                    dgvCourses.Columns[3].HeaderText = "Học kỳ";
                    dgvCourses.Columns[4].HeaderText = "Năm học";
                    dgvCourses.Columns[5].HeaderText = "Số SV";
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
            panelContent.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "MÔN HỌC CỦA TÔI",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(panelContent.Width - 60, panelContent.Height - 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            try
            {
                string query = @"SELECT c.CourseId, c.CourseCode, c.CourseName, c.Credits,
                                c.Semester, c.AcademicYear, c.MaxStudents,
                                (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.CourseId) as Enrolled
                                FROM Courses c
                                WHERE c.TeacherId = @TeacherId";
                dgv.DataSource = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@TeacherId", SessionManager.CurrentTeacher.TeacherId) });

                if (dgv.Columns.Count >= 8)
                {
                    dgv.Columns[0].HeaderText = "ID";
                    dgv.Columns[1].HeaderText = "Mã môn";
                    dgv.Columns[2].HeaderText = "Tên môn học";
                    dgv.Columns[3].HeaderText = "Tín chỉ";
                    dgv.Columns[4].HeaderText = "Học kỳ";
                    dgv.Columns[5].HeaderText = "Năm học";
                    dgv.Columns[6].HeaderText = "Sĩ số tối đa";
                    dgv.Columns[7].HeaderText = "Đã đăng ký";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            panelContent.Controls.Add(dgv);
        }

        // DANH SÁCH SINH VIÊN – ĐÃ SỬA HOÀN HẢO, KHÔNG LỖI
        private void LoadStudentList()
        {
            panelContent.Controls.Clear();

            var lblTitle = new Label
            {
                Text = "DANH SÁCH SINH VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96),
                Location = new Point(20, 20),
                AutoSize = true
            };
            panelContent.Controls.Add(lblTitle);

            var lblSelect = new Label
            {
                Text = "Chọn môn học:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 80),
                AutoSize = true
            };
            panelContent.Controls.Add(lblSelect);

            // DÙNG BIẾN TOÀN CỤC
            cboCourse = new ComboBox
            {
                Location = new Point(140, 78),
                Size = new Size(350, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            panelContent.Controls.Add(cboCourse);

            // NÚT XUẤT EXCEL – ĐẸP, CÓ ICON NHỎ (TÙY CHỌN)
            var btnExport = new Button
            {
                Text = "Xuất Excel",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(510, 76),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExportExcel_Click;
            panelContent.Controls.Add(btnExport);

            // DÙNG BIẾN TOÀN CỤC
            dgvStudents = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(panelContent.Width - 60, panelContent.Height - 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                }
            };
            panelContent.Controls.Add(dgvStudents);

            LoadCoursesToComboBox();

            cboCourse.SelectedIndexChanged += (s, e) =>
            {
                if (cboCourse.SelectedValue != null && int.TryParse(cboCourse.SelectedValue.ToString(), out int courseId))
                {
                    LoadStudentsByCourse(courseId);
                }
            };

            if (cboCourse.Items.Count > 0)
                cboCourse.SelectedIndex = 0;
        }

        private void LoadCoursesToComboBox()
        {
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT CourseId, CONCAT(CourseCode, ' - ', CourseName) AS DisplayName FROM Courses WHERE TeacherId = @TeacherId AND IsActive = 1",
                new SqlParameter[] { new SqlParameter("@TeacherId", SessionManager.CurrentTeacher.TeacherId) });

            cboCourse.DataSource = dt;
            cboCourse.DisplayMember = "DisplayName";
            cboCourse.ValueMember = "CourseId";
        }

        private void LoadStudentsByCourse(int courseId)
        {
            try
            {
                string query = @"
            SELECT 
                s.StudentCode AS [Mã SV],
                u.FullName AS [Họ tên],
                u.Email,
                u.Phone AS [Số ĐT],
                s.Class AS Lớp,
                s.Major AS Ngành,
                CONVERT(varchar, e.EnrollmentDate, 103) AS [Ngày đăng ký],
                e.Status AS [Trạng thái]  -- TRẢ VỀ DỮ LIỆU GỐC (1 hoặc 0)
            FROM Enrollments e
            INNER JOIN Students s ON e.StudentId = s.StudentId
            INNER JOIN Users u ON s.UserId = u.UserId
            WHERE e.CourseId = @CourseId
            ORDER BY s.StudentCode";

                var dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@CourseId", courseId) });

                // Dịch 1 → "Enrolled", 0 → "Dropped" ngay trong C#
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Trạng thái"] is int status)
                    {
                        row["Trạng thái"] = status == 1 ? "Enrolled" : "Dropped";
                    }
                }

                dgvStudents.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải sinh viên: " + ex.Message);
            }
        }

        // XUẤT EXCEL – HOẠT ĐỘNG HOÀN HẢO, KHÔNG CÒN LỖI
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            if (dgvStudents?.DataSource == null || ((DataTable)dgvStudents.DataSource).Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var dt = (DataTable)dgvStudents.DataSource;

                // ĐOẠN NÀY LÀ CHÌA KHÓA THÀNH CÔNG – LOẠI BỎ 100% KÝ TỰ CẤM
                string invalid = new string(Path.GetInvalidFileNameChars()) + @":\/*?""<>|";
                string safeCourseName = string.Join("_", cboCourse.Text.Split(invalid.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                                                    .TrimEnd('.').Trim();

                string fileName = $"Danh_sach_sinh_vien_{safeCourseName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                var saveDlg = new SaveFileDialog
                {
                    Filter = "Excel Workbook|*.xlsx",
                    FileName = fileName,
                    Title = "Xuất danh sách sinh viên"
                };

                if (saveDlg.ShowDialog() != DialogResult.OK) return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Danh sách sinh viên");

                    ws.Cell(1, 1).Value = "DANH SÁCH SINH VIÊN";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 16;
                    ws.Range(1, 1, 1, dt.Columns.Count).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(2, 1).Value = cboCourse.Text;
                    ws.Cell(2, 1).Style.Font.Italic = true;
                    ws.Cell(2, 1).Style.Font.FontSize = 13;
                    ws.Range(2, 1, 2, dt.Columns.Count).Merge();

                    var table = ws.Cell(4, 1).InsertTable(dt, true);
                    table.Theme = XLTableTheme.TableStyleMedium9;
                    table.ShowAutoFilter = true;
                    ws.Columns().AdjustToContents();

                    wb.SaveAs(saveDlg.FileName);
                }

                MessageBox.Show("Xuất Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (MessageBox.Show("Mở file ngay?", "Mở file", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGradeEntry()
        {
            panelContent.Controls.Clear();

            GradeEntryForm gradeForm = new GradeEntryForm();
            gradeForm.TopLevel = false;
            gradeForm.FormBorderStyle = FormBorderStyle.None;
            gradeForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(gradeForm);
            gradeForm.Show();
        }

        private void LoadGradeView()
        {
            panelContent.Controls.Clear();

            GradeViewForm gradeViewForm = new GradeViewForm();
            gradeViewForm.TopLevel = false;
            gradeViewForm.FormBorderStyle = FormBorderStyle.None;
            gradeViewForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(gradeViewForm);
            gradeViewForm.Show();
        }

        private void LoadProfile()
        {
            panelContent.Controls.Clear();

            TeacherProfileForm profileForm = new TeacherProfileForm();
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
