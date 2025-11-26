using ClosedXML.Excel;
using StudentManagement.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StudentManagement.Forms
{
    public partial class SemesterManagementForm : Form
    {
        private Panel panelHeader;
        private Panel panelContent;
        private DataGridView dgvSemesters;
        private TextBox txtSearch;

        public SemesterManagementForm()
        {
            InitializeComponent();
            LoadSemesters();
            this.Resize += SemesterManagementForm_Resize;
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Học kỳ - Semester Management";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "Danh sách Học kỳ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            // Add Semester Button
            Button btnAddSemester = new Button
            {
                Text = "+ Thêm học kỳ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 100),
                Size = new Size(180, 45),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddSemester.FlatAppearance.BorderSize = 0;
            btnAddSemester.Click += BtnAddSemester_Click;
            panelHeader.Controls.Add(btnAddSemester);

            // Upload/Download Buttons
            Button btnUpload = new Button
            {
                Text = "⬆ Tải lên",
                Font = new Font("Segoe UI", 10),
                Location = new Point(950, 100),
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
                Location = new Point(1100, 100),
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

            // Search Box
            Panel searchBg = new Panel
            {
                Location = new Point(10, 2),
                Size = new Size(350, 45),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 10),
                Size = new Size(300, 30),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(249, 250, 251),
                PlaceholderText = "Tìm kiếm học kỳ"
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            Label lblSearchIcon = new Label
            {
                Text = "🔍",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(156, 163, 175)
            };

            searchBg.Controls.Add(lblSearchIcon);
            searchBg.Controls.Add(txtSearch);
            panelContent.Controls.Add(searchBg);

            // DataGridView for Semesters
            dgvSemesters = new DataGridView
            {
                Location = new Point(0, 70),
                Size = new Size(1250, 600),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                ColumnHeadersHeight = 50,
                RowTemplate = { Height = 60 },
                ScrollBars = ScrollBars.Vertical,
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

            panelContent.Controls.Add(dgvSemesters);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
        }

        private void LoadSemesters()
        {
            try
            {
                // Create Semesters table if not exists
                string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Semesters')
                BEGIN
                    CREATE TABLE Semesters (
                        SemesterId INT PRIMARY KEY IDENTITY(1,1),
                        SemesterName NVARCHAR(100) NOT NULL,
                        SemesterCode NVARCHAR(50) NOT NULL UNIQUE,
                        StartDate DATE NOT NULL,
                        EndDate DATE NOT NULL,
                        Status NVARCHAR(50) DEFAULT N'Sắp tới',
                        CreatedAt DATETIME DEFAULT GETDATE()
                    )
                END";

                DatabaseHelper.ExecuteNonQuery(createTableQuery);

                // Check if table is empty and insert sample data
                string checkQuery = "SELECT COUNT(*) FROM Semesters";
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery));

                if (count == 0)
                {
                    InsertSampleSemesters();
                }

                // Load semesters
                string query = @"SELECT
                                SemesterId,
                                SemesterName as 'Tên học kỳ',
                                SemesterCode as 'Mã học kỳ',
                                FORMAT(StartDate, 'yyyy-MM-dd') as 'Ngày bắt đầu',
                                FORMAT(EndDate, 'yyyy-MM-dd') as 'Ngày kết thúc',
                                Status as 'Trạng thái'
                                FROM Semesters
                                ORDER BY StartDate DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dgvSemesters.DataSource = dt;

                if (dgvSemesters.Columns.Count > 0)
                {
                    // Hide ID column
                    dgvSemesters.Columns["SemesterId"].Visible = false;

                    // Add Status Badge Column (custom rendering)
                    if (!dgvSemesters.Columns.Contains("StatusBadge"))
                    {
                        DataGridViewButtonColumn statusCol = new DataGridViewButtonColumn
                        {
                            Name = "StatusBadge",
                            HeaderText = "TRẠNG THÁI",
                            UseColumnTextForButtonValue = false,
                            FlatStyle = FlatStyle.Flat,
                            Width = 150
                        };
                        dgvSemesters.Columns.Add(statusCol);
                    }

                    // Add Action Buttons
                    if (!dgvSemesters.Columns.Contains("Edit"))
                    {
                        DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
                        {
                            Name = "Edit",
                            HeaderText = "HÀNH ĐỘNG",
                            Text = "✏️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 20
                        };
                        dgvSemesters.Columns.Add(editCol);
                    }

                    if (!dgvSemesters.Columns.Contains("Delete"))
                    {
                        DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn
                        {
                            Name = "Delete",
                            Text = "🗑️",
                            UseColumnTextForButtonValue = true,
                            FlatStyle = FlatStyle.Flat,
                            Width = 20
                        };
                        dgvSemesters.Columns.Add(deleteCol);
                    }

                    dgvSemesters.Columns["Trạng thái"].Visible = false; // Hide original status
                }

                dgvSemesters.CellClick += DgvSemesters_CellClick;
                dgvSemesters.CellPainting += DgvSemesters_CellPainting;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách học kỳ: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InsertSampleSemesters()
        {
            string insertQuery = @"
            INSERT INTO Semesters (SemesterName, SemesterCode, AcademicYear, StartDate, EndDate, Status) VALUES
            (N'Học kỳ 1 (2023-2024)', 'HK1_2324', 2023, '2023-09-04', '2024-01-12', N'Hoạt động'),
            (N'Học kỳ 2 (2023-2024)', 'HK2_2324', 2024, '2024-01-22', '2024-05-24', N'Hoạt động'),
            (N'Học kỳ hè (2024)', 'HKH_24', 2024, '2024-06-03', '2024-07-26', N'Đã kết thúc'),
            (N'Học kỳ 1 (2024-2025)', 'HK1_2425', 2024, '2024-09-02', '2025-01-10', N'Sắp tới'),
            (N'Học kỳ 2 (2024-2025)', 'HK2_2425', 2025, '2025-01-20', '2025-05-23', N'Sắp tới')";

            DatabaseHelper.ExecuteNonQuery(insertQuery);
        }

        // ==================== EXPORT EXCEL ====================
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDlg = new SaveFileDialog
                {
                    Filter = "Excel Workbook|*.xlsx",
                    FileName = $"Danh_sach_hoc_ky_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Học kỳ");

                        ws.Cell(1, 1).Value = "Tên học kỳ";
                        ws.Cell(1, 2).Value = "Mã học kỳ";
                        ws.Cell(1, 3).Value = "Ngày bắt đầu";
                        ws.Cell(1, 4).Value = "Ngày kết thúc";
                        ws.Cell(1, 5).Value = "Trạng thái";

                        var header = ws.Range("A1:E1");
                        header.Style.Font.Bold = true;
                        header.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 70, 229);
                        header.Style.Font.FontColor = XLColor.White;
                        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        string query = "SELECT SemesterName, SemesterCode, CONVERT(varchar, StartDate, 23), CONVERT(varchar, EndDate, 23), Status FROM Semesters ORDER BY StartDate DESC";
                        DataTable dt = DatabaseHelper.ExecuteQuery(query);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ws.Cell(i + 2, 1).Value = dt.Rows[i][0].ToString();
                            ws.Cell(i + 2, 2).Value = dt.Rows[i][1].ToString();
                            ws.Cell(i + 2, 3).Value = dt.Rows[i][2].ToString();
                            ws.Cell(i + 2, 4).Value = dt.Rows[i][3].ToString();
                            ws.Cell(i + 2, 5).Value = dt.Rows[i][4].ToString();
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(saveDlg.FileName);
                    }

                    MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
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
                    Title = "Chọn file Excel để nhập"
                };

                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    using (var workbook = new XLWorkbook(openDlg.FileName))
                    {
                        var ws = workbook.Worksheets.First();
                        int rowCount = ws.LastRowUsed().RowNumber();
                        if (rowCount < 2)
                        {
                            MessageBox.Show("File không có dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        List<string> errors = new List<string>();
                        int success = 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            string ten = ws.Cell(row, 1).GetString().Trim();
                            string ma = ws.Cell(row, 2).GetString().Trim();
                            string startStr = ws.Cell(row, 3).GetString().Trim();
                            string endStr = ws.Cell(row, 4).GetString().Trim();
                            string status = ws.Cell(row, 5).GetString().Trim();

                            if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(ma))
                            {
                                errors.Add($"Dòng {row}: Thiếu tên hoặc mã học kỳ");
                                continue;
                            }

                            if (!DateTime.TryParse(startStr, out DateTime startDate))
                            {
                                errors.Add($"Dòng {row}: Ngày bắt đầu không hợp lệ");
                                continue;
                            }

                            if (!DateTime.TryParse(endStr, out DateTime endDate))
                            {
                                errors.Add($"Dòng {row}: Ngày kết thúc không hợp lệ");
                                continue;
                            }

                            if (endDate <= startDate)
                            {
                                errors.Add($"Dòng {row}: Ngày kết thúc phải sau ngày bắt đầu");
                                continue;
                            }

                            // Kiểm tra trùng mã (dùng mảng parameter đúng cách)
                            int count = (int)DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Semesters WHERE SemesterCode = @code",
                                new SqlParameter[] { new SqlParameter("@code", ma) });

                            if (count > 0)
                            {
                                errors.Add($"Dòng {row}: Mã học kỳ '{ma}' đã tồn tại");
                                continue;
                            }

                            // Insert (dùng mảng parameter)
                            DatabaseHelper.ExecuteNonQuery(@"
                                INSERT INTO Semesters (SemesterName, SemesterCode, StartDate, EndDate, Status)
                                VALUES (@name, @code, @start, @end, @status)",
                                new SqlParameter[] {
                                    new SqlParameter("@name", ten),
                                    new SqlParameter("@code", ma),
                                    new SqlParameter("@start", startDate),
                                    new SqlParameter("@end", endDate),
                                    new SqlParameter("@status", string.IsNullOrWhiteSpace(status) ? "Sắp tới" : status)
                                });

                            success++;
                        }

                        LoadSemesters();

                        string msg = $"Nhập thành công {success} học kỳ.";
                        if (errors.Count > 0)
                            msg += $"\n\nLỗi ({errors.Count} dòng):\n" + string.Join("\n", errors.Take(10));

                        MessageBox.Show(msg, "Kết quả nhập", MessageBoxButtons.OK,
                            errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nhập Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvSemesters_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvSemesters.Columns[e.ColumnIndex].Name == "StatusBadge")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                string status = dgvSemesters.Rows[e.RowIndex].Cells["Trạng thái"].Value?.ToString();
                if (!string.IsNullOrEmpty(status))
                {
                    Color bgColor;
                    string text;

                    switch (status)
                    {
                        case "Hoạt động":
                            bgColor = Color.FromArgb(16, 185, 129);
                            text = "Hoạt động";
                            break;
                        case "Sắp tới":
                            bgColor = Color.FromArgb(249, 115, 22);
                            text = "Sắp tới";
                            break;
                        case "Đã kết thúc":
                            bgColor = Color.FromArgb(107, 114, 128);
                            text = "Đã kết thúc";
                            break;
                        default:
                            bgColor = Color.Gray;
                            text = status;
                            break;
                    }

                    Rectangle rect = new Rectangle(
                        e.CellBounds.X + 10,
                        e.CellBounds.Y + (e.CellBounds.Height - 30) / 2,
                        100,
                        30
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

        private void DgvSemesters_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int semesterId = Convert.ToInt32(dgvSemesters.Rows[e.RowIndex].Cells["SemesterId"].Value);
                string columnName = dgvSemesters.Columns[e.ColumnIndex].Name;

                if (columnName == "Edit")
                {
                    EditSemester(semesterId);
                }
                else if (columnName == "Delete")
                {
                    DeleteSemester(semesterId);
                }
            }
        }

        private void BtnAddSemester_Click(object sender, EventArgs e)
        {
            SemesterCreateForm createForm = new SemesterCreateForm();
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                LoadSemesters();
            }
        }

        private void EditSemester(int semesterId)
        {
            SemesterEditForm editForm = new SemesterEditForm(semesterId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadSemesters();
            }
        }

        private void DeleteSemester(int semesterId)
        {
            try
            {
                // Get semester info
                string query = "SELECT SemesterName FROM Semesters WHERE SemesterId = @SemesterId";
                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@SemesterId", semesterId) });

                if (dt.Rows.Count > 0)
                {
                    string semesterName = dt.Rows[0]["SemesterName"].ToString();

                    DialogResult result = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa học kỳ '{semesterName}'?\n\nHành động này không thể hoàn tác.",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        string deleteQuery = "DELETE FROM Semesters WHERE SemesterId = @SemesterId";
                        DatabaseHelper.ExecuteNonQuery(deleteQuery,
                            new SqlParameter[] { new SqlParameter("@SemesterId", semesterId) });

                        MessageBox.Show("Xóa học kỳ thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadSemesters();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa học kỳ: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.Trim();

                string query = @"SELECT
                                SemesterId,
                                SemesterName as 'Tên học kỳ',
                                SemesterCode as 'Mã học kỳ',
                                FORMAT(StartDate, 'yyyy-MM-dd') as 'Ngày bắt đầu',
                                FORMAT(EndDate, 'yyyy-MM-dd') as 'Ngày kết thúc',
                                Status as 'Trạng thái'
                                FROM Semesters
                                WHERE SemesterName LIKE @Search OR SemesterCode LIKE @Search
                                ORDER BY StartDate DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@Search", "%" + searchText + "%") });

                dgvSemesters.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SemesterManagementForm_Resize(object sender, EventArgs e)
        {
            try
            {
                if (this.IsHandleCreated && dgvSemesters != null && dgvSemesters.IsHandleCreated && dgvSemesters.DataSource != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
                {
                    // Adjust DataGridView size based on form size
                    int newWidth = this.ClientSize.Width - 60; // 30px padding on each side
                    int newHeight = this.ClientSize.Height - 320; // Header + search + padding height

                    dgvSemesters.Size = new Size(newWidth, newHeight);

                    // Recalculate column widths proportionally
                    if (dgvSemesters.Columns.Count > 0)
                    {
                        // Total width available for data columns (excluding fixed-width columns)
                        int availableWidth = newWidth - 150 - 20 - 20; // StatusBadge + Edit + Delete

                        // Set column widths proportionally
                        if (dgvSemesters.Columns.Contains("Tên học kỳ") && dgvSemesters.Columns["Tên học kỳ"] != null)
                            dgvSemesters.Columns["Tên học kỳ"].Width = (int)(availableWidth * 0.25);
                        if (dgvSemesters.Columns.Contains("Mã học kỳ") && dgvSemesters.Columns["Mã học kỳ"] != null)
                            dgvSemesters.Columns["Mã học kỳ"].Width = (int)(availableWidth * 0.15);
                        if (dgvSemesters.Columns.Contains("Ngày bắt đầu") && dgvSemesters.Columns["Ngày bắt đầu"] != null)
                            dgvSemesters.Columns["Ngày bắt đầu"].Width = (int)(availableWidth * 0.20);
                        if (dgvSemesters.Columns.Contains("Ngày kết thúc") && dgvSemesters.Columns["Ngày kết thúc"] != null)
                            dgvSemesters.Columns["Ngày kết thúc"].Width = (int)(availableWidth * 0.20);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore resize errors during form initialization
            }
        }
    }
}
