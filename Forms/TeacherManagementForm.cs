using ClosedXML.Excel;
using StudentManagement.Data;
using StudentManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudentManagement.Forms
{
    public partial class TeacherManagementForm : Form
    {
        private Panel panelHeader;
        private Panel panelFilters;
        private Panel panelContent;
        private DataGridView dgvTeachers;
        private TextBox txtSearch;
        private ComboBox cboDepartment;
        private ComboBox cboDegree;
        private ComboBox cboStatus;

        public TeacherManagementForm()
        {
            InitializeComponent();
            CreateTeachersTableIfNotExists();
            LoadFilters();
            LoadTeachers();
            this.Resize += TeacherManagementForm_Resize;
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Giảng viên - Teacher Management";
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
                Text = "Quản lý giảng viên",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            // Add Teacher Button
            Button btnAddTeacher = new Button
            {
                Text = "+ Thêm giảng viên",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 65),
                Size = new Size(180, 45),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddTeacher.FlatAppearance.BorderSize = 0;
            btnAddTeacher.Click += BtnAddTeacher_Click;
            panelHeader.Controls.Add(btnAddTeacher);

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
                PlaceholderText = "Tìm kiếm giảng viên theo mã hoặc tên"
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchBg.Controls.Add(txtSearch);
            panelFilters.Controls.Add(searchBg);

            // Department Filter
            Label lblDept = new Label
            {
                Text = "Tất cả khoa",
                Font = new Font("Segoe UI", 9),
                Location = new Point(450, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelFilters.Controls.Add(lblDept);

            cboDepartment = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(450, 25),
                Size = new Size(180, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Standard
            };
            cboDepartment.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboDepartment);

            // Degree Filter
            Label lblDegree = new Label
            {
                Text = "Tất cả học vị",
                Font = new Font("Segoe UI", 9),
                Location = new Point(650, 5),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelFilters.Controls.Add(lblDegree);

            cboDegree = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(650, 25),
                Size = new Size(200, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Standard
            };
            cboDegree.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboDegree);

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
            cboStatus.Items.AddRange(new object[] { "Tất cả", "Đang làm việc", "Nghỉ phép", "Đã nghỉ việc" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += Filter_Changed;
            panelFilters.Controls.Add(cboStatus);

            // Upload/Download Buttons
            Button btnUpload = new Button
            {
                Text = "⬆ Tải lên",
                Font = new Font("Segoe UI", 10),
                Location = new Point(950, 50),
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
                Location = new Point(1100, 50),
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

            // DataGridView for Teachers
            dgvTeachers = new DataGridView
            {
                Location = new Point(0, 20),
                Size = new Size(250, 650),
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

            panelContent.Controls.Add(dgvTeachers);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelFilters);
            this.Controls.Add(panelHeader);
        }

        private void CreateTeachersTableIfNotExists()
        {
            try
            {
                string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
                BEGIN
                    CREATE TABLE Teachers (
                        TeacherId INT PRIMARY KEY IDENTITY(1,1),
                        UserId INT FOREIGN KEY REFERENCES Users(UserId),
                        TeacherCode NVARCHAR(50) NOT NULL UNIQUE,
                        Department NVARCHAR(100),
                        Degree NVARCHAR(50),
                        Specialization NVARCHAR(100),
                        HireDate DATE,
                        Status NVARCHAR(50) DEFAULT N'Đang làm việc',
                        CreatedAt DATETIME DEFAULT GETDATE()
                    )
                END";

                DatabaseHelper.ExecuteNonQuery(createTableQuery);

                // Add Status column if it doesn't exist
                string addStatusColumn = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Teachers') AND name = 'Status')
                BEGIN
                    ALTER TABLE Teachers ADD Status NVARCHAR(50) DEFAULT N'Đang làm việc'
                END";

                DatabaseHelper.ExecuteNonQuery(addStatusColumn);

                // Add CreatedAt column if it doesn't exist
                string addCreatedAtColumn = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Teachers') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE Teachers ADD CreatedAt DATETIME DEFAULT GETDATE()
                END";

                DatabaseHelper.ExecuteNonQuery(addCreatedAtColumn);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo bảng Teachers: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilters()
        {
            try
            {
                // Load Departments
                cboDepartment.Items.Clear();
                cboDepartment.Items.Add("Tất cả");
                string deptQuery = "SELECT DISTINCT Department FROM Teachers WHERE Department IS NOT NULL ORDER BY Department";
                DataTable dtDept = DatabaseHelper.ExecuteQuery(deptQuery);
                foreach (DataRow row in dtDept.Rows)
                {
                    cboDepartment.Items.Add(row["Department"].ToString());
                }
                cboDepartment.SelectedIndex = 0;

                // Load Degrees
                cboDegree.Items.Clear();
                cboDegree.Items.Add("Tất cả");
                string degreeQuery = "SELECT DISTINCT Degree FROM Teachers WHERE Degree IS NOT NULL ORDER BY Degree";
                DataTable dtDegree = DatabaseHelper.ExecuteQuery(degreeQuery);
                foreach (DataRow row in dtDegree.Rows)
                {
                    cboDegree.Items.Add(row["Degree"].ToString());
                }
                cboDegree.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải bộ lọc: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTeachers(string searchText = "", string deptFilter = "", string degreeFilter = "", string statusFilter = "")
        {
            try
            {
                string query = @"
                    SELECT
                        t.TeacherId,
                        t.TeacherCode as 'Mã GV',
                        u.FullName as 'Họ tên',
                        t.Department as 'Khoa',
                        t.Degree as 'Học vị',
                        t.Specialization as 'Chuyên môn',
                        u.Email as 'Email',
                        u.Phone as 'SĐT',
                        t.Status as 'StatusValue'
                    FROM Teachers t
                    INNER JOIN Users u ON t.UserId = u.UserId
                    WHERE 1=1";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " AND (t.TeacherCode LIKE @Search OR u.FullName LIKE @Search)";
                    parameters.Add(new SqlParameter("@Search", "%" + searchText + "%"));
                }

                if (!string.IsNullOrWhiteSpace(deptFilter) && deptFilter != "Tất cả")
                {
                    query += " AND t.Department = @Department";
                    parameters.Add(new SqlParameter("@Department", deptFilter));
                }

                if (!string.IsNullOrWhiteSpace(degreeFilter) && degreeFilter != "Tất cả")
                {
                    query += " AND t.Degree = @Degree";
                    parameters.Add(new SqlParameter("@Degree", degreeFilter));
                }

                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "Tất cả")
                {
                    query += " AND t.Status = @Status";
                    parameters.Add(new SqlParameter("@Status", statusFilter));
                }

                query += " ORDER BY t.TeacherCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
                dgvTeachers.DataSource = dt;

                if (dgvTeachers.Columns.Count > 0)
                {
                    // Hide ID columns
                    dgvTeachers.Columns["TeacherId"].Visible = false;
                    dgvTeachers.Columns["StatusValue"].Visible = false;

                    // Add Status Badge Column
                    if (!dgvTeachers.Columns.Contains("StatusBadge"))
                    {
                        DataGridViewButtonColumn statusCol = new DataGridViewButtonColumn
                        {
                            Name = "StatusBadge",
                            HeaderText = "TRẠNG THÁI",
                            UseColumnTextForButtonValue = false,
                            FlatStyle = FlatStyle.Flat,
                            Width = 130,
                            MinimumWidth = 130,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                            Frozen = false,
                            Visible = true
                        };
                        dgvTeachers.Columns.Add(statusCol);
                    }

                    // Add Action Buttons
                    if (!dgvTeachers.Columns.Contains("View"))
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
                        dgvTeachers.Columns.Add(viewCol);
                    }

                    if (!dgvTeachers.Columns.Contains("Edit"))
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
                        dgvTeachers.Columns.Add(editCol);
                    }

                    if (!dgvTeachers.Columns.Contains("Delete"))
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
                        dgvTeachers.Columns.Add(deleteCol);
                    }

                    // Set DisplayIndex to position button columns at the end (rightmost)
                    int totalColumns = dgvTeachers.Columns.Count;
                    if (dgvTeachers.Columns.Contains("StatusBadge"))
                        dgvTeachers.Columns["StatusBadge"].DisplayIndex = totalColumns - 4;
                    if (dgvTeachers.Columns.Contains("View"))
                        dgvTeachers.Columns["View"].DisplayIndex = totalColumns - 3;
                    if (dgvTeachers.Columns.Contains("Edit"))
                        dgvTeachers.Columns["Edit"].DisplayIndex = totalColumns - 2;
                    if (dgvTeachers.Columns.Contains("Delete"))
                        dgvTeachers.Columns["Delete"].DisplayIndex = totalColumns - 1;

                    // Auto-resize data columns to fill space (exclude button columns)
                    ResizeDataColumns();
                }

                // Remove old event handlers first to avoid multiple subscriptions
                dgvTeachers.CellClick -= DgvTeachers_CellClick;
                dgvTeachers.CellPainting -= DgvTeachers_CellPainting;

                // Add event handlers
                dgvTeachers.CellClick += DgvTeachers_CellClick;
                dgvTeachers.CellPainting += DgvTeachers_CellPainting;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách giảng viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== EXPORT EXCEL ====================
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDlg = new SaveFileDialog
                {
                    Filter = "Excel Workbook|*.xlsx",
                    FileName = $"Danh_sach_giang_vien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Giảng viên");

                        // Header
                        string[] headers = { "Mã GV", "Họ tên", "Khoa", "Học vị", "Chuyên môn", "Email", "SĐT", "Trạng thái" };
                        for (int i = 0; i < headers.Length; i++)
                            ws.Cell(1, i + 1).Value = headers[i];

                        var headerRange = ws.Range("A1:H1");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(99, 102, 241);
                        headerRange.Style.Font.FontColor = XLColor.White;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Data
                        string query = @"
                            SELECT t.TeacherCode, u.FullName, t.Department, t.Degree, 
                                   t.Specialization, u.Email, u.Phone, ISNULL(t.Status, N'Đang làm việc')
                            FROM Teachers t
                            INNER JOIN Users u ON t.UserId = u.UserId
                            ORDER BY t.TeacherCode";

                        DataTable dt = DatabaseHelper.ExecuteQuery(query);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ws.Cell(i + 2, 1).Value = dt.Rows[i][0].ToString();
                            ws.Cell(i + 2, 2).Value = dt.Rows[i][1].ToString();
                            ws.Cell(i + 2, 3).Value = dt.Rows[i][2].ToString();
                            ws.Cell(i + 2, 4).Value = dt.Rows[i][3].ToString();
                            ws.Cell(i + 2, 5).Value = dt.Rows[i][4].ToString();
                            ws.Cell(i + 2, 6).Value = dt.Rows[i][5].ToString();
                            ws.Cell(i + 2, 7).Value = dt.Rows[i][6].ToString();
                            ws.Cell(i + 2, 8).Value = dt.Rows[i][7].ToString();
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(saveDlg.FileName);
                    }

                    MessageBox.Show("Xuất danh sách giảng viên thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    System.Diagnostics.Process.Start(new ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== IMPORT EXCEL ====================
        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openDlg = new OpenFileDialog
                {
                    Filter = "Excel Workbook|*.xlsx",
                    Title = "Chọn file Excel chứa danh sách giảng viên"
                };

                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    using (var workbook = new XLWorkbook(openDlg.FileName))
                    {
                        var ws = workbook.Worksheets.First();
                        int rowCount = ws.LastRowUsed().RowNumber();
                        if (rowCount < 2)
                        {
                            MessageBox.Show("File Excel không có dữ liệu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        List<string> errors = new List<string>();
                        int success = 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                string maGV = ws.Cell(row, 1).GetString().Trim();
                                string hoTen = ws.Cell(row, 2).GetString().Trim();
                                string khoa = ws.Cell(row, 3).GetString().Trim();
                                string hocVi = ws.Cell(row, 4).GetString().Trim();
                                string chuyenMon = ws.Cell(row, 5).GetString().Trim();
                                string email = ws.Cell(row, 6).GetString().Trim();
                                string sdt = ws.Cell(row, 7).GetString().Trim();
                                string trangThai = ws.Cell(row, 8).GetString().Trim();

                                if (string.IsNullOrWhiteSpace(maGV) || string.IsNullOrWhiteSpace(hoTen))
                                {
                                    errors.Add($"Dòng {row}: Thiếu mã GV hoặc họ tên");
                                    continue;
                                }

                                // Kiểm tra trùng mã GV
                                int count = (int)DatabaseHelper.ExecuteScalar(
                                    "SELECT COUNT(*) FROM Teachers WHERE TeacherCode = @code",
                                    new SqlParameter[] { new SqlParameter("@code", maGV) });

                                if (count > 0)
                                {
                                    errors.Add($"Dòng {row}: Mã GV '{maGV}' đã tồn tại");
                                    continue;
                                }

                                // Tạm thời tạo User giả nếu chưa có (bạn có thể cải thiện sau)
                                // Ở đây mình insert User trước, rồi lấy UserId
                                string insertUser = @"
                                    INSERT INTO Users (FullName, Email, Phone, Role, PasswordHash, CreatedAt)
                                    VALUES (@FullName, @Email, @Phone, N'Giảng viên', 'temp_hash', GETDATE());
                                    SELECT SCOPE_IDENTITY();";

                                object userIdObj = DatabaseHelper.ExecuteScalar(insertUser,
                                    new SqlParameter[] {
                                        new SqlParameter("@FullName", hoTen),
                                        new SqlParameter("@Email", string.IsNullOrWhiteSpace(email) ? DBNull.Value : email),
                                        new SqlParameter("@Phone", string.IsNullOrWhiteSpace(sdt) ? DBNull.Value : sdt)
                                    });

                                int userId = Convert.ToInt32(userIdObj);

                                // Insert Teacher
                                string insertTeacher = @"
                                    INSERT INTO Teachers (UserId, TeacherCode, Department, Degree, Specialization, Status, HireDate)
                                    VALUES (@UserId, @Code, @Dept, @Degree, @Spec, @Status, GETDATE())";

                                DatabaseHelper.ExecuteNonQuery(insertTeacher,
                                    new SqlParameter[] {
                                        new SqlParameter("@UserId", userId),
                                        new SqlParameter("@Code", maGV),
                                        new SqlParameter("@Dept", string.IsNullOrWhiteSpace(khoa) ? DBNull.Value : khoa),
                                        new SqlParameter("@Degree", string.IsNullOrWhiteSpace(hocVi) ? DBNull.Value : hocVi),
                                        new SqlParameter("@Spec", string.IsNullOrWhiteSpace(chuyenMon) ? DBNull.Value : chuyenMon),
                                        new SqlParameter("@Status", string.IsNullOrWhiteSpace(trangThai) ? "Đang làm việc" : trangThai)
                                    });

                                success++;
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Dòng {row}: {ex.Message}");
                            }
                        }

                        LoadFilters();
                        LoadTeachers();

                        string msg = $"Nhập thành công {success} giảng viên.";
                        if (errors.Count > 0)
                            msg += $"\n\nLỗi ({errors.Count} dòng):\n" + string.Join("\n", errors.Take(10));

                        MessageBox.Show(msg, "Kết quả nhập Excel",
                            MessageBoxButtons.OK,
                            errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi nhập file Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvTeachers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvTeachers.Columns[e.ColumnIndex].Name == "StatusBadge")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                // Get status value, default to "Đang làm việc" if null or empty
                string status = dgvTeachers.Rows[e.RowIndex].Cells["StatusValue"].Value?.ToString();
                if (string.IsNullOrEmpty(status))
                {
                    status = "Đang làm việc"; // Default status
                }

                Color bgColor;
                string text;

                switch (status.Trim())
                {
                    case "Đang làm việc":
                        bgColor = Color.FromArgb(16, 185, 129);
                        text = "Đang làm việc";
                        break;
                    case "Nghỉ phép":
                        bgColor = Color.FromArgb(251, 191, 36);
                        text = "Nghỉ phép";
                        break;
                    case "Đã nghỉ việc":
                        bgColor = Color.FromArgb(107, 114, 128);
                        text = "Đã nghỉ việc";
                        break;
                    default:
                        bgColor = Color.FromArgb(16, 185, 129);
                        text = "Đang làm việc";
                        break;
                }

                Rectangle rect = new Rectangle(
                    e.CellBounds.X + 10,
                    e.CellBounds.Y + (e.CellBounds.Height - 28) / 2,
                    110,
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

        private void DgvTeachers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int teacherId = Convert.ToInt32(dgvTeachers.Rows[e.RowIndex].Cells["TeacherId"].Value);
                string columnName = dgvTeachers.Columns[e.ColumnIndex].Name;

                if (columnName == "View")
                {
                    ViewTeacher(teacherId);
                }
                else if (columnName == "Edit")
                {
                    EditTeacher(teacherId);
                }
                else if (columnName == "Delete")
                {
                    DeleteTeacher(teacherId);
                }
            }
        }

        private void BtnAddTeacher_Click(object sender, EventArgs e)
        {
            TeacherCreateForm createForm = new TeacherCreateForm();
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters();
                LoadTeachers();
            }
        }

        private void ViewTeacher(int teacherId)
        {
            TeacherEditForm viewForm = new TeacherEditForm(teacherId);
            viewForm.ShowDialog();
        }

        private void EditTeacher(int teacherId)
        {
            TeacherEditForm editForm = new TeacherEditForm(teacherId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters();
                LoadTeachers();
            }
        }

        private void DeleteTeacher(int teacherId)
        {
            try
            {
                string query = @"SELECT u.FullName, t.TeacherCode
                                FROM Teachers t
                                INNER JOIN Users u ON t.UserId = u.UserId
                                WHERE t.TeacherId = @TeacherId";
                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) });

                if (dt.Rows.Count > 0)
                {
                    string teacherName = dt.Rows[0]["FullName"].ToString();
                    string teacherCode = dt.Rows[0]["TeacherCode"].ToString();

                    DialogResult result = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa giảng viên '{teacherName}' ({teacherCode})?\\n\\nHành động này không thể hoàn tác.",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        string deleteQuery = "DELETE FROM Teachers WHERE TeacherId = @TeacherId";
                        DatabaseHelper.ExecuteNonQuery(deleteQuery,
                            new SqlParameter[] { new SqlParameter("@TeacherId", teacherId) });

                        MessageBox.Show("Xóa giảng viên thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadFilters();
                        LoadTeachers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa giảng viên: {ex.Message}", "Lỗi",
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
            string deptFilter = cboDepartment.SelectedItem?.ToString() ?? "Tất cả";
            string degreeFilter = cboDegree.SelectedItem?.ToString() ?? "Tất cả";
            string statusFilter = cboStatus.SelectedItem?.ToString() ?? "Tất cả";

            LoadTeachers(searchText, deptFilter, degreeFilter, statusFilter);
        }

        private void ResizeDataColumns()
        {
            if (dgvTeachers.Columns.Count > 0)
            {
                // Total width available for data columns (excluding fixed-width columns)
                int availableWidth = dgvTeachers.Width - 130 - 60 - 60 - 60 - 20; // StatusBadge + View + Edit + Delete + scrollbar

                // Set column widths proportionally - reduced percentages to fit all columns
                if (dgvTeachers.Columns.Contains("Mã GV") && dgvTeachers.Columns["Mã GV"] != null)
                    dgvTeachers.Columns["Mã GV"].Width = (int)(availableWidth * 0.08);
                if (dgvTeachers.Columns.Contains("Họ tên") && dgvTeachers.Columns["Họ tên"] != null)
                    dgvTeachers.Columns["Họ tên"].Width = (int)(availableWidth * 0.14);
                if (dgvTeachers.Columns.Contains("Khoa") && dgvTeachers.Columns["Khoa"] != null)
                    dgvTeachers.Columns["Khoa"].Width = (int)(availableWidth * 0.16);
                if (dgvTeachers.Columns.Contains("Học vị") && dgvTeachers.Columns["Học vị"] != null)
                    dgvTeachers.Columns["Học vị"].Width = (int)(availableWidth * 0.08);
                if (dgvTeachers.Columns.Contains("Chuyên môn") && dgvTeachers.Columns["Chuyên môn"] != null)
                    dgvTeachers.Columns["Chuyên môn"].Width = (int)(availableWidth * 0.14);
                if (dgvTeachers.Columns.Contains("Email") && dgvTeachers.Columns["Email"] != null)
                    dgvTeachers.Columns["Email"].Width = (int)(availableWidth * 0.16);
                if (dgvTeachers.Columns.Contains("SĐT") && dgvTeachers.Columns["SĐT"] != null)
                    dgvTeachers.Columns["SĐT"].Width = (int)(availableWidth * 0.10);
            }
        }

        private void TeacherManagementForm_Resize(object sender, EventArgs e)
        {
            try
            {
                if (this.IsHandleCreated && dgvTeachers != null && dgvTeachers.IsHandleCreated && dgvTeachers.DataSource != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
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
