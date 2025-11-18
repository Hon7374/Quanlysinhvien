using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    public partial class GradeEntryForm : Form
    {
        private int teacherId;
        private ComboBox cboCourse;
        private DataGridView dgvStudents;
        private Button btnSaveGrades;

        public GradeEntryForm()
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
            CreateTablesIfNotExists();
            LoadTeacherCourses();
        }

        private void InitializeComponent()
        {
            this.Text = "Nhập điểm - Grade Entry";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);

            // Header
            Panel panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            Label lblTitle = new Label
            {
                Text = "Nhập điểm sinh viên",
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

            this.Controls.Add(panelHeader);

            // Content Panel
            Panel panelContent = new Panel
            {
                Location = new Point(30, 170),
                Size = new Size(1320, 650),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // DataGridView
            dgvStudents = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(1280, 550),
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
            panelContent.Controls.Add(dgvStudents);

            // Save Button
            btnSaveGrades = new Button
            {
                Text = "💾 Lưu điểm",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(1150, 580),
                Size = new Size(150, 50),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSaveGrades.FlatAppearance.BorderSize = 0;
            btnSaveGrades.Click += BtnSaveGrades_Click;
            panelContent.Controls.Add(btnSaveGrades);

            this.Controls.Add(panelContent);
        }

        private void CreateTablesIfNotExists()
        {
            try
            {
                // Create Enrollments table
                string createEnrollments = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
                BEGIN
                    CREATE TABLE Enrollments (
                        EnrollmentId INT PRIMARY KEY IDENTITY(1,1),
                        CourseId INT FOREIGN KEY REFERENCES Courses(CourseId),
                        StudentId INT FOREIGN KEY REFERENCES Students(StudentId),
                        EnrollmentDate DATETIME DEFAULT GETDATE(),
                        Status NVARCHAR(50) DEFAULT N'Đang học'
                    )
                END";
                DatabaseHelper.ExecuteNonQuery(createEnrollments);

                // Create Grades table
                string createGrades = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Grades')
                BEGIN
                    CREATE TABLE Grades (
                        GradeId INT PRIMARY KEY IDENTITY(1,1),
                        EnrollmentId INT FOREIGN KEY REFERENCES Enrollments(EnrollmentId),
                        MidtermScore DECIMAL(4,2),
                        FinalScore DECIMAL(4,2),
                        TotalScore DECIMAL(4,2),
                        LetterGrade NVARCHAR(5),
                        CreatedAt DATETIME DEFAULT GETDATE(),
                        UpdatedAt DATETIME DEFAULT GETDATE()
                    )
                END";
                DatabaseHelper.ExecuteNonQuery(createGrades);

                // Add UpdatedAt column if it doesn't exist
                string addUpdatedAtColumn = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Grades') AND name = 'UpdatedAt')
                BEGIN
                    ALTER TABLE Grades ADD UpdatedAt DATETIME DEFAULT GETDATE()
                END";
                DatabaseHelper.ExecuteNonQuery(addUpdatedAtColumn);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo bảng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTeacherCourses()
        {
            try
            {
                cboCourse.Items.Clear();
                cboCourse.Items.Add(new CourseItem { Text = "-- Chọn môn học --", Value = 0 });

                string query = @"SELECT CourseId, CourseCode, CourseName, Semester
                                FROM Courses
                                WHERE TeacherId = @TeacherId AND IsActive = 1
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
                LoadStudentsForCourse(selected.Value);
                btnSaveGrades.Enabled = true;
            }
            else
            {
                dgvStudents.DataSource = null;
                btnSaveGrades.Enabled = false;
            }
        }

        private void LoadStudentsForCourse(int courseId)
        {
            try
            {
                string query = @"
                    SELECT
                        e.EnrollmentId,
                        s.StudentCode as 'MSSV',
                        u.FullName as 'Họ tên',
                        s.Class as 'Lớp',
                        ISNULL(g.MidtermScore, 0) as 'Điểm giữa kỳ',
                        ISNULL(g.FinalScore, 0) as 'Điểm cuối kỳ',
                        ISNULL(g.TotalScore, 0) as 'Điểm tổng kết',
                        ISNULL(g.LetterGrade, '') as 'Xếp loại'
                    FROM Enrollments e
                    INNER JOIN Students s ON e.StudentId = s.StudentId
                    INNER JOIN Users u ON s.UserId = u.UserId
                    LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
                    WHERE e.CourseId = @CourseId
                    ORDER BY s.StudentCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
                dgvStudents.DataSource = dt;

                if (dgvStudents.Columns.Count > 0)
                {
                    dgvStudents.Columns["EnrollmentId"].Visible = false;
                    dgvStudents.Columns["MSSV"].ReadOnly = true;
                    dgvStudents.Columns["Họ tên"].ReadOnly = true;
                    dgvStudents.Columns["Lớp"].ReadOnly = true;
                    dgvStudents.Columns["Điểm tổng kết"].ReadOnly = true;
                    dgvStudents.Columns["Xếp loại"].ReadOnly = true;

                    dgvStudents.Columns["Điểm giữa kỳ"].Width = 120;
                    dgvStudents.Columns["Điểm cuối kỳ"].Width = 120;
                    dgvStudents.Columns["Điểm tổng kết"].Width = 120;
                    dgvStudents.Columns["Xếp loại"].Width = 100;
                }

                dgvStudents.CellValueChanged -= DgvStudents_CellValueChanged;
                dgvStudents.CellValueChanged += DgvStudents_CellValueChanged;

                // Add Edit button column if not exists
                if (!dgvStudents.Columns.Contains("EditButton"))
                {
                    DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
                    {
                        Name = "EditButton",
                        HeaderText = "Thao tác",
                        Text = "✏️ Sửa",
                        UseColumnTextForButtonValue = true,
                        Width = 100,
                        FlatStyle = FlatStyle.Flat
                    };
                    dgvStudents.Columns.Add(editCol);
                }

                dgvStudents.CellClick -= DgvStudents_CellClick;
                dgvStudents.CellClick += DgvStudents_CellClick;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dgvStudents.Columns[e.ColumnIndex].Name == "EditButton")
                {
                    // Get student info
                    var row = dgvStudents.Rows[e.RowIndex];
                    int enrollmentId = Convert.ToInt32(row.Cells["EnrollmentId"].Value);
                    string mssv = row.Cells["MSSV"].Value?.ToString();
                    string hoTen = row.Cells["Họ tên"].Value?.ToString();
                    decimal midterm = row.Cells["Điểm giữa kỳ"].Value != null ? Convert.ToDecimal(row.Cells["Điểm giữa kỳ"].Value) : 0;
                    decimal final = row.Cells["Điểm cuối kỳ"].Value != null ? Convert.ToDecimal(row.Cells["Điểm cuối kỳ"].Value) : 0;

                    // Show edit dialog
                    ShowEditGradeDialog(enrollmentId, mssv, hoTen, midterm, final, e.RowIndex);
                }
            }
        }

        private void ShowEditGradeDialog(int enrollmentId, string mssv, string hoTen, decimal midterm, decimal final, int rowIndex)
        {
            Form editForm = new Form
            {
                Text = "Sửa điểm sinh viên",
                Size = new Size(500, 350),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = $"Sinh viên: {hoTen} ({mssv})",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            editForm.Controls.Add(lblTitle);

            Label lblMidterm = new Label
            {
                Text = "Điểm giữa kỳ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 70),
                Size = new Size(150, 25)
            };
            editForm.Controls.Add(lblMidterm);

            TextBox txtMidterm = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(180, 68),
                Size = new Size(280, 30),
                Text = midterm.ToString("0.00")
            };
            editForm.Controls.Add(txtMidterm);

            Label lblFinal = new Label
            {
                Text = "Điểm cuối kỳ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 120),
                Size = new Size(150, 25)
            };
            editForm.Controls.Add(lblFinal);

            TextBox txtFinal = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(180, 118),
                Size = new Size(280, 30),
                Text = final.ToString("0.00")
            };
            editForm.Controls.Add(txtFinal);

            Label lblTotal = new Label
            {
                Text = "Điểm tổng kết:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 170),
                Size = new Size(150, 25)
            };
            editForm.Controls.Add(lblTotal);

            TextBox txtTotal = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(180, 168),
                Size = new Size(280, 30),
                ReadOnly = true,
                BackColor = Color.FromArgb(249, 250, 251),
                Text = ((midterm * 0.4m) + (final * 0.6m)).ToString("0.00")
            };
            editForm.Controls.Add(txtTotal);

            // Auto-calculate when textboxes change
            EventHandler calculateTotal = (s, ev) =>
            {
                try
                {
                    decimal m = string.IsNullOrWhiteSpace(txtMidterm.Text) ? 0 : decimal.Parse(txtMidterm.Text);
                    decimal f = string.IsNullOrWhiteSpace(txtFinal.Text) ? 0 : decimal.Parse(txtFinal.Text);
                    decimal total = (m * 0.4m) + (f * 0.6m);
                    txtTotal.Text = total.ToString("0.00");
                }
                catch { }
            };
            txtMidterm.TextChanged += calculateTotal;
            txtFinal.TextChanged += calculateTotal;

            Button btnSave = new Button
            {
                Text = "💾 Lưu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(250, 240),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, ev) =>
            {
                try
                {
                    decimal m = decimal.Parse(txtMidterm.Text);
                    decimal f = decimal.Parse(txtFinal.Text);
                    decimal total = (m * 0.4m) + (f * 0.6m);

                    string letterGrade = "";
                    if (total >= 8.5m) letterGrade = "A";
                    else if (total >= 7.0m) letterGrade = "B";
                    else if (total >= 5.5m) letterGrade = "C";
                    else if (total >= 4.0m) letterGrade = "D";
                    else letterGrade = "F";

                    // Check if grade exists
                    string checkQuery = "SELECT COUNT(*) FROM Grades WHERE EnrollmentId = @EnrollmentId";
                    int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery,
                        new SqlParameter[] { new SqlParameter("@EnrollmentId", enrollmentId) }));

                    if (count > 0)
                    {
                        // Update
                        string updateQuery = @"UPDATE Grades SET
                            MidtermScore = @Midterm,
                            FinalScore = @Final,
                            TotalScore = @Total,
                            LetterGrade = @Letter,
                            UpdatedAt = GETDATE()
                            WHERE EnrollmentId = @EnrollmentId";

                        DatabaseHelper.ExecuteNonQuery(updateQuery, new SqlParameter[]
                        {
                            new SqlParameter("@Midterm", m),
                            new SqlParameter("@Final", f),
                            new SqlParameter("@Total", total),
                            new SqlParameter("@Letter", letterGrade),
                            new SqlParameter("@EnrollmentId", enrollmentId)
                        });
                    }
                    else
                    {
                        // Insert
                        string insertQuery = @"INSERT INTO Grades
                            (EnrollmentId, MidtermScore, FinalScore, TotalScore, LetterGrade)
                            VALUES (@EnrollmentId, @Midterm, @Final, @Total, @Letter)";

                        DatabaseHelper.ExecuteNonQuery(insertQuery, new SqlParameter[]
                        {
                            new SqlParameter("@EnrollmentId", enrollmentId),
                            new SqlParameter("@Midterm", m),
                            new SqlParameter("@Final", f),
                            new SqlParameter("@Total", total),
                            new SqlParameter("@Letter", letterGrade)
                        });
                    }

                    // Update the DataGridView row
                    dgvStudents.Rows[rowIndex].Cells["Điểm giữa kỳ"].Value = m;
                    dgvStudents.Rows[rowIndex].Cells["Điểm cuối kỳ"].Value = f;
                    dgvStudents.Rows[rowIndex].Cells["Điểm tổng kết"].Value = total;
                    dgvStudents.Rows[rowIndex].Cells["Xếp loại"].Value = letterGrade;

                    MessageBox.Show("Đã lưu điểm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    editForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            editForm.Controls.Add(btnSave);

            Button btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10),
                Location = new Point(360, 240),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, ev) => editForm.Close();
            editForm.Controls.Add(btnCancel);

            editForm.ShowDialog(this);
        }

        private void DgvStudents_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    var row = dgvStudents.Rows[e.RowIndex];

                    decimal midterm = 0, final = 0;
                    if (row.Cells["Điểm giữa kỳ"].Value != null && row.Cells["Điểm giữa kỳ"].Value != DBNull.Value)
                        midterm = Convert.ToDecimal(row.Cells["Điểm giữa kỳ"].Value);

                    if (row.Cells["Điểm cuối kỳ"].Value != null && row.Cells["Điểm cuối kỳ"].Value != DBNull.Value)
                        final = Convert.ToDecimal(row.Cells["Điểm cuối kỳ"].Value);

                    // Calculate total: 40% midterm + 60% final
                    decimal total = (midterm * 0.4m) + (final * 0.6m);
                    row.Cells["Điểm tổng kết"].Value = Math.Round(total, 2);

                    // Letter grade
                    string letterGrade = "";
                    if (total >= 8.5m) letterGrade = "A";
                    else if (total >= 7.0m) letterGrade = "B";
                    else if (total >= 5.5m) letterGrade = "C";
                    else if (total >= 4.0m) letterGrade = "D";
                    else letterGrade = "F";

                    row.Cells["Xếp loại"].Value = letterGrade;
                }
                catch { }
            }
        }

        private void BtnSaveGrades_Click(object sender, EventArgs e)
        {
            try
            {
                int savedCount = 0;

                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (row.IsNewRow) continue;

                    int enrollmentId = Convert.ToInt32(row.Cells["EnrollmentId"].Value);
                    decimal midterm = row.Cells["Điểm giữa kỳ"].Value != null ? Convert.ToDecimal(row.Cells["Điểm giữa kỳ"].Value) : 0;
                    decimal final = row.Cells["Điểm cuối kỳ"].Value != null ? Convert.ToDecimal(row.Cells["Điểm cuối kỳ"].Value) : 0;
                    decimal total = row.Cells["Điểm tổng kết"].Value != null ? Convert.ToDecimal(row.Cells["Điểm tổng kết"].Value) : 0;
                    string letterGrade = row.Cells["Xếp loại"].Value?.ToString() ?? "";

                    // Check if grade exists
                    string checkQuery = "SELECT COUNT(*) FROM Grades WHERE EnrollmentId = @EnrollmentId";
                    int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery, new SqlParameter[] { new SqlParameter("@EnrollmentId", enrollmentId) }));

                    if (count > 0)
                    {
                        // Update
                        string updateQuery = @"UPDATE Grades SET
                            MidtermScore = @Midterm,
                            FinalScore = @Final,
                            TotalScore = @Total,
                            LetterGrade = @Letter,
                            UpdatedAt = GETDATE()
                            WHERE EnrollmentId = @EnrollmentId";

                        DatabaseHelper.ExecuteNonQuery(updateQuery, new SqlParameter[]
                        {
                            new SqlParameter("@Midterm", midterm),
                            new SqlParameter("@Final", final),
                            new SqlParameter("@Total", total),
                            new SqlParameter("@Letter", letterGrade),
                            new SqlParameter("@EnrollmentId", enrollmentId)
                        });
                    }
                    else
                    {
                        // Insert
                        string insertQuery = @"INSERT INTO Grades
                            (EnrollmentId, MidtermScore, FinalScore, TotalScore, LetterGrade)
                            VALUES (@EnrollmentId, @Midterm, @Final, @Total, @Letter)";

                        DatabaseHelper.ExecuteNonQuery(insertQuery, new SqlParameter[]
                        {
                            new SqlParameter("@EnrollmentId", enrollmentId),
                            new SqlParameter("@Midterm", midterm),
                            new SqlParameter("@Final", final),
                            new SqlParameter("@Total", total),
                            new SqlParameter("@Letter", letterGrade)
                        });
                    }
                    savedCount++;
                }

                MessageBox.Show($"Đã lưu điểm cho {savedCount} sinh viên!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu điểm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class CourseItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
