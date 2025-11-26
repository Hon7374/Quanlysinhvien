using ClosedXML.Excel;
using StudentManagement.Data;
using StudentManagement.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
            this.Resize += StudentManagementForm_Resize;
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
                Size = new Size(350, 35),
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
                FlatStyle = FlatStyle.Standard
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
                FlatStyle = FlatStyle.Standard
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
                FlatStyle = FlatStyle.Standard
            };
            cboStatus.Items.AddRange(new object[] { "Tất cả", "Đang học", "Bảo lưu", "Đã tốt nghiệp" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboStatus);

            // Upload/Download Buttons
            Button btnUpload = new Button
            {
                Text = "⬆ Tải lên",
                Font = new Font("Segoe UI", 10),
                Location = new Point(950, 60),
                Size = new Size(120, 45),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpload.FlatAppearance.BorderColor = Color.FromArgb(99, 102, 241);
            btnUpload.Click += BtnImportExcel_Click;  // ← THÊM DÒNG NÀY
            panelHeader.Controls.Add(btnUpload);

            Button btnDownload = new Button
            {
                Text = "⬇ Tải xuống",
                Font = new Font("Segoe UI", 10),
                Location = new Point(1100, 60),
                Size = new Size(130, 45),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDownload.FlatAppearance.BorderColor = Color.FromArgb(99, 102, 241);
            btnDownload.Click += BtnExportExcel_Click;  // ← THÊM DÒNG NÀY
            panelHeader.Controls.Add(btnDownload);

            // Content Panel
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };

            panelContent.HorizontalScroll.Enabled = false;
            panelContent.HorizontalScroll.Visible = false;
            panelContent.HorizontalScroll.Maximum = 0;
            panelContent.AutoScrollMinSize = new Size(0, 1000); // đảm bảo có chỗ cuộn dọc

            // DataGridView for Students
            dgvStudents = new DataGridView
            {
                Location = new Point(35, 20),
                Size = new Size(330, 650),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
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

            panelContent.Controls.Add(dgvStudents);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelFilters);
            this.Controls.Add(panelHeader);

            //// THÊM 3 DÒNG NÀY VÀO ĐÂY – SIÊU QUAN TRỌNG!
            //this.FormBorderStyle = FormBorderStyle.None;   // ← BỎ HOÀN TOÀN VIỀN WINDOWS
            //this.WindowState = FormWindowState.Maximized;  // ← Full màn hình (tùy chọn)
            //this.StartPosition = FormStartPosition.CenterScreen;
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

                // Add Status column if it doesn't exist (for existing tables)
                string addStatusColumn = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Students') AND name = 'Status')
                BEGIN
                    ALTER TABLE Students ADD Status NVARCHAR(50) DEFAULT N'Đang học'
                END";

                DatabaseHelper.ExecuteNonQuery(addStatusColumn);

                // Add CreatedAt column if it doesn't exist (for existing tables)
                string addCreatedAtColumn = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Students') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE Students ADD CreatedAt DATETIME DEFAULT GETDATE()
                END";

                DatabaseHelper.ExecuteNonQuery(addCreatedAtColumn);
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
                            Width = 120,
                            MinimumWidth = 120,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                            Frozen = false,
                            Visible = true
                        };
                        dgvStudents.Columns.Add(statusCol);
                    }

                    // Add Action Buttons
                    if (!dgvStudents.Columns.Contains("View"))
                    {
                        DataGridViewButtonColumn viewCol = new DataGridViewButtonColumn
                        {
                            Name = "View",
                            HeaderText = "",
                            Text = "👁",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60,
                            MinimumWidth = 60,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                            Frozen = false,
                            Visible = true
                        };
                        dgvStudents.Columns.Add(viewCol);
                    }

                    if (!dgvStudents.Columns.Contains("Edit"))
                    {
                        DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
                        {
                            Name = "Edit",
                            HeaderText = "",
                            Text = "✏️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60,
                            MinimumWidth = 60,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                            Frozen = false,
                            Visible = true
                        };
                        dgvStudents.Columns.Add(editCol);
                    }

                    if (!dgvStudents.Columns.Contains("Delete"))
                    {
                        DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn
                        {
                            Name = "Delete",
                            HeaderText = "",
                            Text = "🗑️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 60,
                            MinimumWidth = 60,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                            Frozen = false,
                            Visible = true
                        };
                        dgvStudents.Columns.Add(deleteCol);
                    }

                    // Set DisplayIndex to position button columns at the end (rightmost)
                    int totalColumns = dgvStudents.Columns.Count;
                    if (dgvStudents.Columns.Contains("StatusBadge"))
                        dgvStudents.Columns["StatusBadge"].DisplayIndex = totalColumns - 4;
                    if (dgvStudents.Columns.Contains("View"))
                        dgvStudents.Columns["View"].DisplayIndex = totalColumns - 3;
                    if (dgvStudents.Columns.Contains("Edit"))
                        dgvStudents.Columns["Edit"].DisplayIndex = totalColumns - 2;
                    if (dgvStudents.Columns.Contains("Delete"))
                        dgvStudents.Columns["Delete"].DisplayIndex = totalColumns - 1;

                    // Auto-resize data columns to fill space (exclude button columns)
                    ResizeDataColumns();
                }

                // Remove old event handlers first to avoid multiple subscriptions
                dgvStudents.CellClick -= DgvStudents_CellClick;
                dgvStudents.CellPainting -= DgvStudents_CellPainting;

                // Add event handlers
                dgvStudents.CellClick += DgvStudents_CellClick;
                dgvStudents.CellPainting += DgvStudents_CellPainting;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách sinh viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

            // === XUẤT & NHẬP EXCEL (ĐÃ SỬA LỖI) ===
            private void BtnExportExcel_Click(object sender, EventArgs e)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var ws = workbook.Worksheets.Add("Danh sách sinh viên");

                            // Header
                            for (int i = 0; i < dgvStudents.Columns.Count; i++)
                            {
                                if (dgvStudents.Columns[i].Visible)
                                    ws.Cell(1, dgvStudents.Columns[i].DisplayIndex + 1).Value = dgvStudents.Columns[i].HeaderText;
                            }
                            ws.Row(1).Style.Font.Bold = true;
                            ws.Row(1).Style.Fill.BackgroundColor = XLColor.FromArgb(79, 70, 229);
                            ws.Row(1).Style.Font.FontColor = XLColor.White;

                            // Data
                            for (int i = 0; i < dgvStudents.Rows.Count; i++)
                            {
                                for (int j = 0; j < dgvStudents.Columns.Count; j++)
                                {
                                    if (dgvStudents.Columns[j].Visible)
                                    {
                                        var value = dgvStudents.Rows[i].Cells[j].FormattedValue?.ToString() ?? "";
                                        ws.Cell(i + 2, dgvStudents.Columns[j].DisplayIndex + 1).Value = value;
                                    }
                                }
                            }

                            ws.Columns().AdjustToContents();
                            workbook.SaveAs(saveDialog.FileName);
                        }
                        MessageBox.Show("Xuất Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất Excel: " + ex.Message);
                    }
                }
            }

        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "Excel files (*.xlsx)|*.xlsx" };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook(openDialog.FileName))
                    {
                        var ws = workbook.Worksheets.First();
                        var rowCount = ws.LastRowUsed().RowNumber();
                        int success = 0;

                        for (int r = 2; r <= rowCount; r++)
                        {
                            var row = ws.Row(r);
                            string mssv = row.Cell(1).GetString().Trim();
                            string hoTen = row.Cell(2).GetString().Trim();
                            if (string.IsNullOrEmpty(mssv) || string.IsNullOrEmpty(hoTen)) continue;

                            string lop = row.Cell(3).GetString().Trim();
                            string chuyenNganh = row.Cell(4).GetString().Trim();
                            string email = row.Cell(5).GetString().Trim();
                            string sdt = row.Cell(6).GetString().Trim();
                            string status = row.Cell(7).GetString().Trim();

                            // Tạo User
                            string sqlUser = @"
                        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
                        INSERT INTO Users (Username, FullName, Email, Phone, PasswordHash, Role, IsActive)
                        VALUES (@Username, @FullName, @Email, @Phone, HASHBYTES('SHA2_256', @Password), 3, 1)";

                            DatabaseHelper.ExecuteNonQuery(sqlUser, new SqlParameter[]
                            {
                        new SqlParameter("@Username", mssv),
                        new SqlParameter("@FullName", hoTen),
                        new SqlParameter("@Email", email ?? ""),
                        new SqlParameter("@Phone", sdt ?? ""),
                        new SqlParameter("@Password", mssv)
                            });

                            // SỬA DÒNG NÀY: Phải truyền mảng SqlParameter[], không được truyền 1 cái
                            int userId = (int)DatabaseHelper.ExecuteScalar(
                                "SELECT UserId FROM Users WHERE Username = @Username",
                                new SqlParameter[] { new SqlParameter("@Username", mssv) }  // ← ĐÃ SỬA
                            );

                            // Thêm Student
                            string sqlStudent = @"
                        IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = @Code)
                        INSERT INTO Students (UserId, StudentCode, Class, Major, Status)
                        VALUES (@UserId, @Code, @Class, @Major, @Status)";

                            DatabaseHelper.ExecuteNonQuery(sqlStudent, new SqlParameter[]
                            {
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@Code", mssv),
                        new SqlParameter("@Class", lop ?? ""),
                        new SqlParameter("@Major", chuyenNganh ?? ""),
                        new SqlParameter("@Status", string.IsNullOrEmpty(status) ? "Đang học" : status)
                            });

                            success++;
                        }

                        LoadFilters();
                        LoadStudents();
                        MessageBox.Show($"Nhập thành công {success} sinh viên!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi nhập Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DgvStudents_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvStudents.Columns[e.ColumnIndex].Name == "StatusBadge")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                // Get status value, default to "Đang học" if null or empty
                string status = dgvStudents.Rows[e.RowIndex].Cells["StatusValue"].Value?.ToString();
                if (string.IsNullOrEmpty(status))
                {
                    status = "Đang học"; // Default status
                }

                Color bgColor;
                string text;

                switch (status.Trim())
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
                        bgColor = Color.FromArgb(16, 185, 129);
                        text = "Đang học";
                        break;
                }

                Rectangle rect = new Rectangle(
                    e.CellBounds.X + 10,
                    e.CellBounds.Y + (e.CellBounds.Height - 28) / 2,
                    100,
                    28
                );

                // Draw rounded rectangle background
                using (Brush brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }

                // Draw text
                TextRenderer.DrawText(e.Graphics, text, new Font("Segoe UI", 8, FontStyle.Bold),
                    rect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

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

        private void ResizeDataColumns()
        {
            if (dgvStudents.Columns.Count > 0)
            {
                // Total width available for data columns (excluding fixed-width columns)
                int availableWidth = dgvStudents.Width - 120 - 60 - 60 - 60 - 20; // StatusBadge + View + Edit + Delete + scrollbar

                // Set column widths proportionally - reduced percentages to fit all columns
                if (dgvStudents.Columns.Contains("MSSV") && dgvStudents.Columns["MSSV"] != null)
                    dgvStudents.Columns["MSSV"].Width = (int)(availableWidth * 0.10);
                if (dgvStudents.Columns.Contains("Họ tên") && dgvStudents.Columns["Họ tên"] != null)
                    dgvStudents.Columns["Họ tên"].Width = (int)(availableWidth * 0.15);
                if (dgvStudents.Columns.Contains("Lớp") && dgvStudents.Columns["Lớp"] != null)
                    dgvStudents.Columns["Lớp"].Width = (int)(availableWidth * 0.10);
                if (dgvStudents.Columns.Contains("Chuyên ngành") && dgvStudents.Columns["Chuyên ngành"] != null)
                    dgvStudents.Columns["Chuyên ngành"].Width = (int)(availableWidth * 0.15);
                if (dgvStudents.Columns.Contains("Email") && dgvStudents.Columns["Email"] != null)
                    dgvStudents.Columns["Email"].Width = (int)(availableWidth * 0.16);
                if (dgvStudents.Columns.Contains("SĐT") && dgvStudents.Columns["SĐT"] != null)
                    dgvStudents.Columns["SĐT"].Width = (int)(availableWidth * 0.10);
            }
        }

        private void StudentManagementForm_Resize(object sender, EventArgs e)
        {
            try
            {
                if (this.IsHandleCreated && dgvStudents != null && dgvStudents.IsHandleCreated && dgvStudents.DataSource != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
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
