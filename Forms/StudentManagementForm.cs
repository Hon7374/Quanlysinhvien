using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Forms
{
    public partial class StudentManagementForm : Form
    {
        private Panel panelHeader;
        private Panel panelFilters;
        private Panel panelContent;
        private DataGridView dgvStudents;
        private TextBox txtSearch;
        private ComboBox cboClass;
        private ComboBox cboMajor;
        private ComboBox cboStatus;

        public StudentManagementForm()
        {
            InitializeComponent();
            CreateStudentsTableIfNotExists();
            LoadFilters();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Sinh viên - Student Management";
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
                Text = "Quản lý sinh viên",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            // Add Student Button
            Button btnAddStudent = new Button
            {
                Text = "+ Thêm sinh viên",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 65),
                Size = new Size(180, 45),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddStudent.FlatAppearance.BorderSize = 0;
            btnAddStudent.Click += BtnAddStudent_Click;
            panelHeader.Controls.Add(btnAddStudent);

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
                PlaceholderText = "Tìm kiếm sinh viên theo MSSV hoặc Họ tên"
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchBg.Controls.Add(txtSearch);
            panelFilters.Controls.Add(searchBg);

            // Class Filter
            Label lblClass = new Label
            {
                Text = "Tất cả lớp",
                Font = new Font("Segoe UI", 9),
                Location = new Point(450, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelFilters.Controls.Add(lblClass);

            cboClass = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(450, 25),
                Size = new Size(180, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cboClass.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboClass);

            // Major Filter
            Label lblMajor = new Label
            {
                Text = "Tất cả chuyên ngành",
                Font = new Font("Segoe UI", 9),
                Location = new Point(650, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelFilters.Controls.Add(lblMajor);

            cboMajor = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(650, 25),
                Size = new Size(200, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cboMajor.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboMajor);

            // Status Filter
            Label lblStatus = new Label
            {
                Text = "Tất cả trạng thái",
                Font = new Font("Segoe UI", 9),
                Location = new Point(870, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelFilters.Controls.Add(lblStatus);

            cboStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(870, 25),
                Size = new Size(180, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cboStatus.Items.AddRange(new object[] { "Tất cả", "Đang học", "Bảo lưu", "Đã tốt nghiệp" });
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

            // DataGridView for Students
            dgvStudents = new DataGridView
            {
                Location = new Point(0, 20),
                Size = new Size(1320, 650),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
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

            panelContent.Controls.Add(dgvStudents);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelFilters);
            this.Controls.Add(panelHeader);
        }

        private void CreateStudentsTableIfNotExists()
        {
            try
            {
                string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
                BEGIN
                    CREATE TABLE Students (
                        StudentId INT PRIMARY KEY IDENTITY(1,1),
                        UserId INT FOREIGN KEY REFERENCES Users(UserId),
                        StudentCode NVARCHAR(50) NOT NULL UNIQUE,
                        DateOfBirth DATE,
                        Gender NVARCHAR(10),
                        Address NVARCHAR(255),
                        Class NVARCHAR(50),
                        Major NVARCHAR(100),
                        AcademicYear INT,
                        GPA DECIMAL(3,2) DEFAULT 0.00,
                        Status NVARCHAR(50) DEFAULT N'Đang học',
                        CreatedAt DATETIME DEFAULT GETDATE()
                    )
                END";

                DatabaseHelper.ExecuteNonQuery(createTableQuery);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo bảng Students: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilters()
        {
            try
            {
                // Load Classes
                cboClass.Items.Clear();
                cboClass.Items.Add("Tất cả");
                string classQuery = "SELECT DISTINCT Class FROM Students WHERE Class IS NOT NULL ORDER BY Class";
                DataTable dtClass = DatabaseHelper.ExecuteQuery(classQuery);
                foreach (DataRow row in dtClass.Rows)
                {
                    cboClass.Items.Add(row["Class"].ToString());
                }
                cboClass.SelectedIndex = 0;

                // Load Majors
                cboMajor.Items.Clear();
                cboMajor.Items.Add("Tất cả");
                string majorQuery = "SELECT DISTINCT Major FROM Students WHERE Major IS NOT NULL ORDER BY Major";
                DataTable dtMajor = DatabaseHelper.ExecuteQuery(majorQuery);
                foreach (DataRow row in dtMajor.Rows)
                {
                    cboMajor.Items.Add(row["Major"].ToString());
                }
                cboMajor.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải bộ lọc: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudents(string searchText = "", string classFilter = "", string majorFilter = "", string statusFilter = "")
        {
            try
            {
                string query = @"
                    SELECT
                        s.StudentId,
                        s.StudentCode as 'MSSV',
                        u.FullName as 'Họ tên',
                        s.Class as 'Lớp',
                        s.Major as 'Chuyên ngành',
                        u.Email as 'Email',
                        u.Phone as 'SĐT',
                        s.Status as 'Trạng thái',
                        s.Status as 'StatusValue'
                    FROM Students s
                    INNER JOIN Users u ON s.UserId = u.UserId
                    WHERE 1=1";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " AND (s.StudentCode LIKE @Search OR u.FullName LIKE @Search)";
                    parameters.Add(new SqlParameter("@Search", "%" + searchText + "%"));
                }

                if (!string.IsNullOrWhiteSpace(classFilter) && classFilter != "Tất cả")
                {
                    query += " AND s.Class = @Class";
                    parameters.Add(new SqlParameter("@Class", classFilter));
                }

                if (!string.IsNullOrWhiteSpace(majorFilter) && majorFilter != "Tất cả")
                {
                    query += " AND s.Major = @Major";
                    parameters.Add(new SqlParameter("@Major", majorFilter));
                }

                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "Tất cả")
                {
                    query += " AND s.Status = @Status";
                    parameters.Add(new SqlParameter("@Status", statusFilter));
                }

                query += " ORDER BY s.StudentCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
                dgvStudents.DataSource = dt;

                if (dgvStudents.Columns.Count > 0)
                {
                    // Hide ID columns
                    dgvStudents.Columns["StudentId"].Visible = false;
                    dgvStudents.Columns["StatusValue"].Visible = false;
                    dgvStudents.Columns["Trạng thái"].Visible = false;

                    // Add Status Badge Column
                    if (!dgvStudents.Columns.Contains("StatusBadge"))
                    {
                        DataGridViewButtonColumn statusCol = new DataGridViewButtonColumn
                        {
                            Name = "StatusBadge",
                            HeaderText = "TRẠNG THÁI",
                            UseColumnTextForButtonValue = false,
                            FlatStyle = FlatStyle.Flat,
                            Width = 120
                        };
                        dgvStudents.Columns.Add(statusCol);
                    }

                    // Add Action Buttons
                    if (!dgvStudents.Columns.Contains("View"))
                    {
                        DataGridViewButtonColumn viewCol = new DataGridViewButtonColumn
                        {
                            Name = "View",
                            HeaderText = "HÀNH ĐỘNG",
                            Text = "👁",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60
                        };
                        dgvStudents.Columns.Add(viewCol);
                    }

                    if (!dgvStudents.Columns.Contains("Edit"))
                    {
                        DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
                        {
                            Name = "Edit",
                            Text = "✏️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60
                        };
                        dgvStudents.Columns.Add(editCol);
                    }

                    if (!dgvStudents.Columns.Contains("Delete"))
                    {
                        DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn
                        {
                            Name = "Delete",
                            Text = "🗑️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60
                        };
                        dgvStudents.Columns.Add(deleteCol);
                    }
                }

                dgvStudents.CellClick += DgvStudents_CellClick;
                dgvStudents.CellPainting += DgvStudents_CellPainting;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách sinh viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvStudents_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvStudents.Columns[e.ColumnIndex].Name == "StatusBadge")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                string status = dgvStudents.Rows[e.RowIndex].Cells["StatusValue"].Value?.ToString();
                if (!string.IsNullOrEmpty(status))
                {
                    Color bgColor;
                    string text;

                    switch (status)
                    {
                        case "Đang học":
                            bgColor = Color.FromArgb(16, 185, 129);
                            text = "Đang học";
                            break;
                        case "Bảo lưu":
                            bgColor = Color.FromArgb(59, 130, 246);
                            text = "Bảo lưu";
                            break;
                        case "Đã tốt nghiệp":
                            bgColor = Color.FromArgb(107, 114, 128);
                            text = "Đã tốt nghiệp";
                            break;
                        default:
                            bgColor = Color.Gray;
                            text = status;
                            break;
                    }

                    Rectangle rect = new Rectangle(
                        e.CellBounds.X + 10,
                        e.CellBounds.Y + (e.CellBounds.Height - 28) / 2,
                        100,
                        28
                    );

                    using (Brush brush = new SolidBrush(bgColor))
                    {
                        e.Graphics.FillRectangle(brush, rect);
                    }

                    TextRenderer.DrawText(e.Graphics, text, new Font("Segoe UI", 8, FontStyle.Bold),
                        rect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
            }
        }

        private void DgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int studentId = Convert.ToInt32(dgvStudents.Rows[e.RowIndex].Cells["StudentId"].Value);
                string columnName = dgvStudents.Columns[e.ColumnIndex].Name;

                if (columnName == "View")
                {
                    ViewStudent(studentId);
                }
                else if (columnName == "Edit")
                {
                    EditStudent(studentId);
                }
                else if (columnName == "Delete")
                {
                    DeleteStudent(studentId);
                }
            }
        }

        private void BtnAddStudent_Click(object sender, EventArgs e)
        {
            StudentCreateForm createForm = new StudentCreateForm();
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters();
                LoadStudents();
            }
        }

        private void ViewStudent(int studentId)
        {
            StudentEditForm viewForm = new StudentEditForm(studentId);
            viewForm.ShowDialog();
        }

        private void EditStudent(int studentId)
        {
            StudentEditForm editForm = new StudentEditForm(studentId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters();
                LoadStudents();
            }
        }

        private void DeleteStudent(int studentId)
        {
            try
            {
                string query = @"SELECT u.FullName, s.StudentCode
                                FROM Students s
                                INNER JOIN Users u ON s.UserId = u.UserId
                                WHERE s.StudentId = @StudentId";
                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@StudentId", studentId) });

                if (dt.Rows.Count > 0)
                {
                    string studentName = dt.Rows[0]["FullName"].ToString();
                    string studentCode = dt.Rows[0]["StudentCode"].ToString();

                    DialogResult result = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa sinh viên '{studentName}' ({studentCode})?\\n\\nHành động này không thể hoàn tác.",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        string deleteQuery = "DELETE FROM Students WHERE StudentId = @StudentId";
                        DatabaseHelper.ExecuteNonQuery(deleteQuery,
                            new SqlParameter[] { new SqlParameter("@StudentId", studentId) });

                        MessageBox.Show("Xóa sinh viên thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadFilters();
                        LoadStudents();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sinh viên: {ex.Message}", "Lỗi",
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
            string classFilter = cboClass.SelectedItem?.ToString() ?? "Tất cả";
            string majorFilter = cboMajor.SelectedItem?.ToString() ?? "Tất cả";
            string statusFilter = cboStatus.SelectedItem?.ToString() ?? "Tất cả";

            LoadStudents(searchText, classFilter, majorFilter, statusFilter);
        }
    }
}
