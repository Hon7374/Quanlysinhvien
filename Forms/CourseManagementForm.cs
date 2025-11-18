using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Forms
{
    public partial class CourseManagementForm : Form
    {
        private Panel panelHeader;
        private Panel panelFilters;
        private Panel panelContent;
        private DataGridView dgvCourses;
        private TextBox txtSearch;
        private ComboBox cboSemester;
        private ComboBox cboStatus;

        public CourseManagementForm()
        {
            InitializeComponent();
            CreateCoursesTableIfNotExists();
            LoadFilters();
            LoadCourses();
            this.Resize += CourseManagementForm_Resize;
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Môn học - Course Management";
            this.Size = new Size(1400, 900);
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
                Text = "Quản lý môn học",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            // Add Course Button
            Button btnAddCourse = new Button
            {
                Text = "+ Thêm môn học",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 65),
                Size = new Size(180, 45),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddCourse.FlatAppearance.BorderSize = 0;
            btnAddCourse.Click += BtnAddCourse_Click;
            panelHeader.Controls.Add(btnAddCourse);

            // Filters Panel
            panelFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(30, 10, 30, 10)
            };

            // Search Box
            Panel searchBg = new Panel
            {
                Location = new Point(30, 20),
                Size = new Size(400, 45),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            Label lblSearchIcon = new Label
            {
                Text = "🔍",
                Location = new Point(10, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(156, 163, 175)
            };
            searchBg.Controls.Add(lblSearchIcon);

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 12),
                Size = new Size(350, 30),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(249, 250, 251),
                PlaceholderText = "Tìm kiếm môn học theo mã hoặc tên"
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchBg.Controls.Add(txtSearch);
            panelFilters.Controls.Add(searchBg);

            // Semester Filter
            Label lblSemester = new Label
            {
                Text = "Tất cả học kỳ",
                Font = new Font("Segoe UI", 9),
                Location = new Point(450, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelFilters.Controls.Add(lblSemester);

            cboSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(450, 25),
                Size = new Size(180, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cboSemester.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboSemester);

            // Status Filter
            Label lblStatus = new Label
            {
                Text = "Tất cả trạng thái",
                Font = new Font("Segoe UI", 9),
                Location = new Point(650, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelFilters.Controls.Add(lblStatus);

            cboStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(650, 25),
                Size = new Size(180, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cboStatus.Items.AddRange(new object[] { "Tất cả", "Đang mở", "Đã đóng" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboStatus);

            // Content Panel
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };

            // DataGridView for Courses
            dgvCourses = new DataGridView
            {
                Location = new Point(0, 20),
                Size = new Size(1320, 650),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible = false,
                ColumnHeadersHeight = 50,
                RowTemplate = { Height = 60 },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(249, 250, 251),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Padding = new Padding(10),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(238, 242, 255),
                    SelectionForeColor = Color.FromArgb(79, 70, 229),
                    Font = new Font("Segoe UI", 9),
                    Padding = new Padding(10),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            };

            panelContent.Controls.Add(dgvCourses);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelFilters);
            this.Controls.Add(panelHeader);
        }

        private void CreateCoursesTableIfNotExists()
        {
            try
            {
                string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
                BEGIN
                    CREATE TABLE Courses (
                        CourseId INT PRIMARY KEY IDENTITY(1,1),
                        CourseCode NVARCHAR(50) NOT NULL UNIQUE,
                        CourseName NVARCHAR(200) NOT NULL,
                        Credits INT NOT NULL,
                        Description NVARCHAR(500),
                        TeacherId INT FOREIGN KEY REFERENCES Teachers(TeacherId),
                        Semester NVARCHAR(50),
                        AcademicYear INT,
                        MaxStudents INT DEFAULT 50,
                        IsActive BIT DEFAULT 1,
                        CreatedAt DATETIME DEFAULT GETDATE()
                    )
                END";

                DatabaseHelper.ExecuteNonQuery(createTableQuery);

                // Add CreatedAt column if it doesn't exist
                string addCreatedAtColumn = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE Courses ADD CreatedAt DATETIME DEFAULT GETDATE()
                END";

                DatabaseHelper.ExecuteNonQuery(addCreatedAtColumn);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo bảng Courses: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilters()
        {
            try
            {
                // Load Semesters
                cboSemester.Items.Clear();
                cboSemester.Items.Add("Tất cả");
                string semesterQuery = "SELECT DISTINCT Semester FROM Courses WHERE Semester IS NOT NULL ORDER BY Semester";
                DataTable dtSemester = DatabaseHelper.ExecuteQuery(semesterQuery);
                foreach (DataRow row in dtSemester.Rows)
                {
                    cboSemester.Items.Add(row["Semester"].ToString());
                }
                cboSemester.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải bộ lọc: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCourses(string searchText = "", string semesterFilter = "", string statusFilter = "")
        {
            try
            {
                string query = @"
                    SELECT
                        c.CourseId,
                        c.CourseCode as 'Mã môn học',
                        c.CourseName as 'Tên môn học',
                        c.Credits as 'Tín chỉ',
                        c.Semester as 'Học kỳ',
                        ISNULL(u.FullName, N'Chưa phân công') as 'Giảng viên',
                        c.MaxStudents as 'Sĩ số tối đa',
                        CASE WHEN c.IsActive = 1 THEN N'Đang mở' ELSE N'Đã đóng' END as 'StatusValue'
                    FROM Courses c
                    LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                    LEFT JOIN Users u ON t.UserId = u.UserId
                    WHERE 1=1";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " AND (c.CourseCode LIKE @Search OR c.CourseName LIKE @Search)";
                    parameters.Add(new SqlParameter("@Search", "%" + searchText + "%"));
                }

                if (!string.IsNullOrWhiteSpace(semesterFilter) && semesterFilter != "Tất cả")
                {
                    query += " AND c.Semester = @Semester";
                    parameters.Add(new SqlParameter("@Semester", semesterFilter));
                }

                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "Tất cả")
                {
                    if (statusFilter == "Đang mở")
                    {
                        query += " AND c.IsActive = 1";
                    }
                    else if (statusFilter == "Đã đóng")
                    {
                        query += " AND c.IsActive = 0";
                    }
                }

                query += " ORDER BY c.CourseCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
                dgvCourses.DataSource = dt;

                if (dgvCourses.Columns.Count > 0)
                {
                    // Hide ID columns
                    dgvCourses.Columns["CourseId"].Visible = false;
                    dgvCourses.Columns["StatusValue"].Visible = false;

                    // Add Status Badge Column
                    if (!dgvCourses.Columns.Contains("StatusBadge"))
                    {
                        DataGridViewButtonColumn statusCol = new DataGridViewButtonColumn
                        {
                            Name = "StatusBadge",
                            HeaderText = "TRẠNG THÁI",
                            UseColumnTextForButtonValue = false,
                            FlatStyle = FlatStyle.Flat,
                            Width = 100,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                        };
                        dgvCourses.Columns.Add(statusCol);
                    }

                    // Add Action Buttons
                    if (!dgvCourses.Columns.Contains("View"))
                    {
                        DataGridViewButtonColumn viewCol = new DataGridViewButtonColumn
                        {
                            Name = "View",
                            HeaderText = "HÀNH ĐỘNG",
                            Text = "👁",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                        };
                        dgvCourses.Columns.Add(viewCol);
                    }

                    if (!dgvCourses.Columns.Contains("Edit"))
                    {
                        DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
                        {
                            Name = "Edit",
                            Text = "✏️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                        };
                        dgvCourses.Columns.Add(editCol);
                    }

                    if (!dgvCourses.Columns.Contains("Delete"))
                    {
                        DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn
                        {
                            Name = "Delete",
                            Text = "🗑️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                        };
                        dgvCourses.Columns.Add(deleteCol);
                    }

                    // Auto-resize data columns to fill space (exclude button columns)
                    ResizeDataColumns();
                }

                // Remove old event handlers first to avoid multiple subscriptions
                dgvCourses.CellClick -= DgvCourses_CellClick;
                dgvCourses.CellPainting -= DgvCourses_CellPainting;

                // Add event handlers
                dgvCourses.CellClick += DgvCourses_CellClick;
                dgvCourses.CellPainting += DgvCourses_CellPainting;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách môn học: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvCourses_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvCourses.Columns[e.ColumnIndex].Name == "StatusBadge")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                // Get status value, default to "Đang mở" if null or empty
                string status = dgvCourses.Rows[e.RowIndex].Cells["StatusValue"].Value?.ToString();
                if (string.IsNullOrEmpty(status))
                {
                    status = "Đang mở";
                }

                Color bgColor;
                string text;

                switch (status.Trim())
                {
                    case "Đang mở":
                        bgColor = Color.FromArgb(16, 185, 129);
                        text = "Đang mở";
                        break;
                    case "Đã đóng":
                        bgColor = Color.FromArgb(107, 114, 128);
                        text = "Đã đóng";
                        break;
                    default:
                        bgColor = Color.FromArgb(16, 185, 129);
                        text = "Đang mở";
                        break;
                }

                Rectangle rect = new Rectangle(
                    e.CellBounds.X + 10,
                    e.CellBounds.Y + (e.CellBounds.Height - 28) / 2,
                    80,
                    28
                );

                using (Brush brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }

                TextRenderer.DrawText(e.Graphics, text, new Font("Segoe UI", 8, FontStyle.Bold),
                    rect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }

        private void DgvCourses_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int courseId = Convert.ToInt32(dgvCourses.Rows[e.RowIndex].Cells["CourseId"].Value);
                string columnName = dgvCourses.Columns[e.ColumnIndex].Name;

                if (columnName == "View")
                {
                    ViewCourse(courseId);
                }
                else if (columnName == "Edit")
                {
                    EditCourse(courseId);
                }
                else if (columnName == "Delete")
                {
                    DeleteCourse(courseId);
                }
            }
        }

        private void BtnAddCourse_Click(object sender, EventArgs e)
        {
            CourseCreateForm createForm = new CourseCreateForm();
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters();
                LoadCourses();
            }
        }

        private void ViewCourse(int courseId)
        {
            CourseEditForm viewForm = new CourseEditForm(courseId);
            viewForm.ShowDialog();
        }

        private void EditCourse(int courseId)
        {
            CourseEditForm editForm = new CourseEditForm(courseId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters();
                LoadCourses();
            }
        }

        private void DeleteCourse(int courseId)
        {
            try
            {
                string query = @"SELECT CourseCode, CourseName FROM Courses WHERE CourseId = @CourseId";
                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@CourseId", courseId) });

                if (dt.Rows.Count > 0)
                {
                    string courseCode = dt.Rows[0]["CourseCode"].ToString();
                    string courseName = dt.Rows[0]["CourseName"].ToString();

                    DialogResult result = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa môn học '{courseName}' ({courseCode})?\\n\\nHành động này không thể hoàn tác.",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        string deleteQuery = "DELETE FROM Courses WHERE CourseId = @CourseId";
                        DatabaseHelper.ExecuteNonQuery(deleteQuery,
                            new SqlParameter[] { new SqlParameter("@CourseId", courseId) });

                        MessageBox.Show("Xóa môn học thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadFilters();
                        LoadCourses();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa môn học: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string searchText = txtSearch.Text.Trim();
            string semesterFilter = cboSemester.SelectedItem?.ToString() ?? "Tất cả";
            string statusFilter = cboStatus.SelectedItem?.ToString() ?? "Tất cả";

            LoadCourses(searchText, semesterFilter, statusFilter);
        }

        private void ResizeDataColumns()
        {
            if (dgvCourses.Columns.Count > 0)
            {
                // Total width available for data columns (excluding fixed-width columns)
                int availableWidth = dgvCourses.Width - 100 - 60 - 60 - 60; // StatusBadge + View + Edit + Delete

                // Set column widths proportionally
                if (dgvCourses.Columns.Contains("Mã môn học") && dgvCourses.Columns["Mã môn học"] != null)
                    dgvCourses.Columns["Mã môn học"].Width = (int)(availableWidth * 0.15);
                if (dgvCourses.Columns.Contains("Tên môn học") && dgvCourses.Columns["Tên môn học"] != null)
                    dgvCourses.Columns["Tên môn học"].Width = (int)(availableWidth * 0.30);
                if (dgvCourses.Columns.Contains("Tín chỉ") && dgvCourses.Columns["Tín chỉ"] != null)
                    dgvCourses.Columns["Tín chỉ"].Width = (int)(availableWidth * 0.10);
                if (dgvCourses.Columns.Contains("Học kỳ") && dgvCourses.Columns["Học kỳ"] != null)
                    dgvCourses.Columns["Học kỳ"].Width = (int)(availableWidth * 0.15);
                if (dgvCourses.Columns.Contains("Giảng viên") && dgvCourses.Columns["Giảng viên"] != null)
                    dgvCourses.Columns["Giảng viên"].Width = (int)(availableWidth * 0.20);
                if (dgvCourses.Columns.Contains("Sĩ số tối đa") && dgvCourses.Columns["Sĩ số tối đa"] != null)
                    dgvCourses.Columns["Sĩ số tối đa"].Width = (int)(availableWidth * 0.10);
            }
        }

        private void CourseManagementForm_Resize(object sender, EventArgs e)
        {
            try
            {
                if (this.IsHandleCreated && dgvCourses != null && dgvCourses.IsHandleCreated && dgvCourses.DataSource != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
                {
                    // Recalculate column widths proportionally
                    ResizeDataColumns();
                }
            }
            catch (Exception)
            {
                // Ignore resize errors during form initialization
            }
        }
    }
}
