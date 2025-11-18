using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    public partial class GradeViewForm : Form
    {
        private int teacherId;
        private ComboBox cboCourse;
        private DataGridView dgvGrades;
        private Label lblStats;
        private Button btnExport;

        public GradeViewForm()
        {
            // Get current teacher ID
            int userId = SessionManager.CurrentUser.UserId;
            string query = "SELECT TeacherId FROM Teachers WHERE UserId = @UserId";
            DataTable dt = DatabaseHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@UserId", userId) });
            if (dt.Rows.Count > 0)
            {
                teacherId = Convert.ToInt32(dt.Rows[0]["TeacherId"]);
            }

            InitializeComponent();
            LoadTeacherCourses();
        }

        private void InitializeComponent()
        {
            this.Text = "Xem điểm đã nhập - View Grades";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);

            // Header
            Panel panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            Label lblTitle = new Label
            {
                Text = "Xem điểm đã nhập",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            Label lblCourse = new Label
            {
                Text = "Chọn môn học:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 70),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            panelHeader.Controls.Add(lblCourse);

            cboCourse = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 95),
                Size = new Size(400, 35),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCourse.SelectedIndexChanged += CboCourse_SelectedIndexChanged;
            panelHeader.Controls.Add(cboCourse);

            // Stats label
            lblStats = new Label
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 140),
                Size = new Size(800, 25),
                ForeColor = Color.FromArgb(107, 114, 128),
                Text = "Chọn môn học để xem thống kê"
            };
            panelHeader.Controls.Add(lblStats);

            // Export button
            btnExport = new Button
            {
                Text = "📥 Xuất Excel",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(1170, 90),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            panelHeader.Controls.Add(btnExport);

            this.Controls.Add(panelHeader);

            // Content Panel
            Panel panelContent = new Panel
            {
                Location = new Point(30, 200),
                Size = new Size(1320, 650),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // DataGridView
            dgvGrades = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(1280, 600),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                ColumnHeadersHeight = 50,
                RowTemplate = { Height = 50 },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(249, 250, 251),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Padding = new Padding(10)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(238, 242, 255),
                    SelectionForeColor = Color.FromArgb(79, 70, 229),
                    Font = new Font("Segoe UI", 9),
                    Padding = new Padding(10)
                }
            };
            panelContent.Controls.Add(dgvGrades);

            this.Controls.Add(panelContent);
        }

        private void LoadTeacherCourses()
        {
            try
            {
                cboCourse.Items.Clear();
                cboCourse.Items.Add(new CourseItem { Text = "-- Chọn môn học --", Value = 0 });

                string query = @"SELECT CourseId, CourseCode, CourseName, Semester
                                FROM Courses
                                WHERE TeacherId = @TeacherId
                                ORDER BY Semester DESC, CourseCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) });

                foreach (DataRow row in dt.Rows)
                {
                    cboCourse.Items.Add(new CourseItem
                    {
                        Text = $"{row["CourseCode"]} - {row["CourseName"]} ({row["Semester"]})",
                        Value = Convert.ToInt32(row["CourseId"])
                    });
                }
                cboCourse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = cboCourse.SelectedItem as CourseItem;
            if (selected != null && selected.Value > 0)
            {
                LoadGradesForCourse(selected.Value);
                btnExport.Enabled = true;
            }
            else
            {
                dgvGrades.DataSource = null;
                lblStats.Text = "Chọn môn học để xem thống kê";
                btnExport.Enabled = false;
            }
        }

        private void LoadGradesForCourse(int courseId)
        {
            try
            {
                string query = @"
                    SELECT
                        s.StudentCode as 'MSSV',
                        u.FullName as 'Họ tên',
                        s.Class as 'Lớp',
                        ISNULL(g.MidtermScore, 0) as 'Điểm GK',
                        ISNULL(g.FinalScore, 0) as 'Điểm CK',
                        ISNULL(g.TotalScore, 0) as 'Điểm TB',
                        ISNULL(g.LetterGrade, 'N/A') as 'Xếp loại',
                        CASE
                            WHEN g.TotalScore >= 5.0 THEN N'Đạt'
                            WHEN g.TotalScore < 5.0 AND g.TotalScore > 0 THEN N'Không đạt'
                            ELSE N'Chưa có điểm'
                        END as 'Kết quả',
                        FORMAT(g.UpdatedAt, 'dd/MM/yyyy HH:mm') as 'Cập nhật lần cuối'
                    FROM Enrollments e
                    INNER JOIN Students s ON e.StudentId = s.StudentId
                    INNER JOIN Users u ON s.UserId = u.UserId
                    LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
                    WHERE e.CourseId = @CourseId
                    ORDER BY s.StudentCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
                dgvGrades.DataSource = dt;

                if (dgvGrades.Columns.Count > 0)
                {
                    // Column widths
                    dgvGrades.Columns["MSSV"].Width = 100;
                    dgvGrades.Columns["Họ tên"].Width = 200;
                    dgvGrades.Columns["Lớp"].Width = 100;
                    dgvGrades.Columns["Điểm GK"].Width = 90;
                    dgvGrades.Columns["Điểm CK"].Width = 90;
                    dgvGrades.Columns["Điểm TB"].Width = 90;
                    dgvGrades.Columns["Xếp loại"].Width = 90;
                    dgvGrades.Columns["Kết quả"].Width = 120;
                    dgvGrades.Columns["Cập nhật lần cuối"].Width = 150;

                    // Cell painting for grade columns
                    dgvGrades.CellFormatting += DgvGrades_CellFormatting;
                }

                // Calculate statistics
                CalculateStatistics(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvGrades_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Color code based on result
                if (dgvGrades.Columns[e.ColumnIndex].Name == "Kết quả")
                {
                    string result = e.Value?.ToString();
                    if (result == "Đạt")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (result == "Không đạt")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(239, 68, 68);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }

                // Color code letter grade
                if (dgvGrades.Columns[e.ColumnIndex].Name == "Xếp loại")
                {
                    string grade = e.Value?.ToString();
                    if (grade == "A")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                        e.CellStyle.ForeColor = Color.FromArgb(22, 163, 74);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (grade == "B")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(219, 234, 254);
                        e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (grade == "F")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
        }

        private void CalculateStatistics(DataTable dt)
        {
            int total = dt.Rows.Count;
            int graded = 0;
            int passed = 0;
            int failed = 0;
            int countA = 0, countB = 0, countC = 0, countD = 0, countF = 0;
            decimal sumScore = 0;

            foreach (DataRow row in dt.Rows)
            {
                string result = row["Kết quả"].ToString();
                string letterGrade = row["Xếp loại"].ToString();
                decimal totalScore = row["Điểm TB"] != DBNull.Value ? Convert.ToDecimal(row["Điểm TB"]) : 0;

                if (result != "Chưa có điểm")
                {
                    graded++;
                    sumScore += totalScore;

                    if (result == "Đạt") passed++;
                    else if (result == "Không đạt") failed++;

                    switch (letterGrade)
                    {
                        case "A": countA++; break;
                        case "B": countB++; break;
                        case "C": countC++; break;
                        case "D": countD++; break;
                        case "F": countF++; break;
                    }
                }
            }

            decimal avgScore = graded > 0 ? sumScore / graded : 0;
            decimal passRate = total > 0 ? (passed * 100.0m / total) : 0;

            lblStats.Text = $"📊 Tổng: {total} SV | Đã chấm: {graded} | Đạt: {passed} ({passRate:F1}%) | Không đạt: {failed} | " +
                           $"TB: {avgScore:F2} | A: {countA}, B: {countB}, C: {countC}, D: {countD}, F: {countF}";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng xuất Excel sẽ được triển khai sau!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private class CourseItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
