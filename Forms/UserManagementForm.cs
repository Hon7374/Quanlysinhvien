using StudentManagement.Data;
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.InteropServices; // để dùng Marshal.ReleaseComObject
using System.Windows.Forms;
using ClosedXML.Excel;

namespace StudentManagement.Forms
{
    public partial class UserManagementForm : Form
    {
        private Panel panelHeader;
        private Panel panelContent;
        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private ComboBox cboRoleFilter;
        private ComboBox cboStatusFilter;

        public UserManagementForm()
        {
            InitializeComponent();
            LoadUsers();
            this.Resize += UserManagementForm_Resize;
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Tài khoản Người dùng";
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
                Text = "Quản lý Tài khoản Người dùng",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Xem và quản lý trạng thái tài khoản người dùng, bao gồm kích hoạt hoặc khóa tài khoản.",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 55),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            panelHeader.Controls.Add(lblSubtitle);

            // Add User Button
            Button btnAddUser = new Button
            {
                Text = "+ Thêm tài khoản",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 100),
                Size = new Size(250, 45),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddUser.FlatAppearance.BorderSize = 0;
            btnAddUser.Click += BtnAddUser_Click;
            panelHeader.Controls.Add(btnAddUser);

            // Export Buttons
            Button btnExport = new Button
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
            btnExport.FlatAppearance.BorderColor = Color.FromArgb(99, 102, 241);
            btnExport.Click += BtnImportExcel_Click;  // ← THÊM DÒNG NÀY
            panelHeader.Controls.Add(btnExport);

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

            // Filters Section
            Panel panelFilters = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1040, 80),
                BackColor = Color.White
            };
                        
            // Search Box
            Panel searchBg = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(350, 45),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 12),
                Size = new Size(300, 30),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(249, 250, 251),
                PlaceholderText = "Tìm kiếm tài khoản "
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            Label lblSearchIcon = new Label
            {
                Text = "🔍",
                Location = new Point(10, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(156, 163, 175)
            };

            searchBg.Controls.Add(lblSearchIcon);
            searchBg.Controls.Add(txtSearch);
            panelContent.Controls.Add(searchBg);

            // Role Filter
            cboRoleFilter = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(370, 15),
                Size = new Size(200, 40),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Standard
            };
            cboRoleFilter.Items.AddRange(new object[] { "Tất cả vai trò", "Admin", "Giảng viên", "Sinh viên" });
            cboRoleFilter.SelectedIndex = 0;
            cboRoleFilter.SelectedIndexChanged += FilterChanged;
            panelFilters.Controls.Add(cboRoleFilter);

            // Status Filter
            cboStatusFilter = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(590, 15),
                Size = new Size(200, 40),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Standard
            };
            cboStatusFilter.Items.AddRange(new object[] { "Tất cả trạng thái", "Hoạt động", "Bị khóa" });
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += FilterChanged;
            panelFilters.Controls.Add(cboStatusFilter);

            panelContent.Controls.Add(panelFilters);

            // DataGridView
            dgvUsers = new DataGridView
            {
                Location = new Point(0, 100),
                Size = new Size(1300, 550),
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
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


            panelContent.Controls.Add(dgvUsers);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
        }

        private void LoadUsers()
        {
            try
            {
                string query = @"SELECT
                                u.UserId,
                                u.Username as 'TÊN NGƯỜI DÙNG',
                                u.FullName,
                                u.Email as 'EMAIL',
                                CASE u.Role
                                    WHEN 1 THEN N'Quản trị viên'
                                    WHEN 2 THEN N'Giảng viên'
                                    WHEN 3 THEN N'Sinh viên'
                                END as 'VAI TRÒ',
                                FORMAT(u.LastLogin, 'yyyy-MM-dd') as 'ĐĂNG NHẬP GẦN NHẤT',
                                CASE WHEN u.IsActive = 1 THEN N'Hoạt động' ELSE N'Khóa' END as 'TRẠNG THÁI',
                                u.IsActive,
                                CASE WHEN u.IsActive = 1 THEN 'ON' ELSE 'OFF' END as 'Toggle'
                                FROM Users u
                                ORDER BY u.UserId DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dgvUsers.DataSource = dt;

                if (dgvUsers.Columns.Count > 0)
                {
                    dgvUsers.Columns["UserId"].Visible = false;
                    dgvUsers.Columns["FullName"].Visible = false;
                    dgvUsers.Columns["IsActive"].Visible = false;

                    // Set column widths
                    dgvUsers.Columns["TÊN NGƯỜI DÙNG"].Width = 180;
                    dgvUsers.Columns["EMAIL"].Width = 220;
                    dgvUsers.Columns["VAI TRÒ"].Width = 150;
                    dgvUsers.Columns["ĐĂNG NHẬP GẦN NHẤT"].Width = 180;
                    dgvUsers.Columns["TRẠNG THÁI"].Width = 120;
                    dgvUsers.Columns["Toggle"].Width = 123;

                    // Style Toggle column
                    dgvUsers.Columns["Toggle"].HeaderText = "KÍCH HOẠT";
                    dgvUsers.Columns["Toggle"].ReadOnly = true;
                    dgvUsers.Columns["Toggle"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvUsers.Columns["Toggle"].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    
                    // Add Avatar Column
                    //if (!dgvUsers.Columns.Contains("Avatar"))
                    //{
                    //    DataGridViewImageColumn avatarCol = new DataGridViewImageColumn
                    //    {
                    //        Name = "Avatar",
                    //        HeaderText = "",
                    //        Width = 60,
                    //        ImageLayout = DataGridViewImageCellLayout.Zoom
                    //    };
                    //    dgvUsers.Columns.Insert(0, avatarCol);
                    //}

                    // Add Action Buttons
                    if (!dgvUsers.Columns.Contains("Edit"))
                    {
                        DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
                        {
                            Name = "Edit",
                            HeaderText = "SỬA",
                            Text = "✏️",
                            UseColumnTextForButtonValue = true,
                            Width = 100
                        };
                        dgvUsers.Columns.Add(editCol);
                    }

                    if (!dgvUsers.Columns.Contains("Delete"))
                    {
                        DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn
                        {
                            Name = "Delete",
                            HeaderText = "XÓA",
                            Text = "🗑️",
                            UseColumnTextForButtonValue = true,
                            Width = 100
                        };
                        dgvUsers.Columns.Add(deleteCol);
                    }

                    // Customize column colors
                    foreach (DataGridViewRow row in dgvUsers.Rows)
                    {
                        if (row.Cells["TRẠNG THÁI"].Value.ToString() == "Hoạt động")
                        {
                            row.Cells["TRẠNG THÁI"].Style.BackColor = Color.FromArgb(209, 250, 229);
                            row.Cells["TRẠNG THÁI"].Style.ForeColor = Color.FromArgb(16, 185, 129);
                            row.Cells["Toggle"].Style.BackColor = Color.FromArgb(79, 70, 229);
                            row.Cells["Toggle"].Style.ForeColor = Color.White;
                        }
                        else
                        {
                            row.Cells["TRẠNG THÁI"].Style.BackColor = Color.FromArgb(254, 226, 226);
                            row.Cells["TRẠNG THÁI"].Style.ForeColor = Color.FromArgb(220, 38, 38);
                            row.Cells["Toggle"].Style.BackColor = Color.FromArgb(209, 213, 219);
                            row.Cells["Toggle"].Style.ForeColor = Color.FromArgb(107, 114, 128);
                        }

                        string role = row.Cells["VAI TRÒ"].Value.ToString();
                        if (role == "Quản trị viên")
                        {
                            row.Cells["VAI TRÒ"].Style.BackColor = Color.FromArgb(224, 231, 255);
                            row.Cells["VAI TRÒ"].Style.ForeColor = Color.FromArgb(79, 70, 229);
                        }
                        else if (role == "Sinh viên")
                        {
                            row.Cells["VAI TRÒ"].Style.BackColor = Color.FromArgb(219, 234, 254);
                            row.Cells["VAI TRÒ"].Style.ForeColor = Color.FromArgb(59, 130, 246);
                        }
                        else
                        {
                            row.Cells["VAI TRÒ"].Style.BackColor = Color.FromArgb(254, 243, 199);
                            row.Cells["VAI TRÒ"].Style.ForeColor = Color.FromArgb(245, 158, 11);
                        }
                    }

                    // Refresh to trigger cell painting
                    dgvUsers.Refresh();
                }

                // Remove old event handler before adding new one to prevent multiple firings
                dgvUsers.CellClick -= DgvUsers_CellClick;
                dgvUsers.CellClick += DgvUsers_CellClick;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách người dùng: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "DanhSachTaiKhoan_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Tài khoản");

                        // Header (đẹp lung linh luôn)
                        for (int i = 0; i < dgvUsers.Columns.Count; i++)
                        {
                            if (dgvUsers.Columns[i].Visible)
                            {
                                worksheet.Cell(1, i + 1).Value = dgvUsers.Columns[i].HeaderText;
                            }
                        }
                        worksheet.Row(1).Style.Font.Bold = true;
                        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromArgb(79, 70, 229);
                        worksheet.Row(1).Style.Font.FontColor = XLColor.White;

                        // Data
                        for (int i = 0; i < dgvUsers.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgvUsers.Columns.Count; j++)
                            {
                                if (dgvUsers.Columns[j].Visible)
                                {
                                    worksheet.Cell(i + 2, j + 1).Value = dgvUsers.Rows[i].Cells[j].FormattedValue?.ToString();
                                }
                            }
                        }

                        worksheet.Columns().AdjustToContents(); // AutoFit
                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show("Xuất Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook(openDialog.FileName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int rowCount = worksheet.LastRowUsed().RowNumber();

                        int count = 0;
                        for (int row = 2; row <= rowCount; row++) // dòng 1 là header
                        {
                            string username = worksheet.Cell(row, 1).GetString().Trim();
                            string fullName = worksheet.Cell(row, 2).GetString().Trim();
                            string email = worksheet.Cell(row, 3).GetString().Trim();
                            string password = worksheet.Cell(row, 4).GetString().Trim();

                            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) continue;

                            // Role: cột 5 (nếu có) – mặc định là Sinh viên (3)
                            int role = 3;
                            if (int.TryParse(worksheet.Cell(row, 5).GetString(), out int r))
                                role = r;

                            string query = @"IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
                                    INSERT INTO Users (Username, FullName, Email, PasswordHash, Role, IsActive)
                                    VALUES (@Username, @FullName, @Email, HASHBYTES('SHA2_256', @Password), @Role, 1)";

                            var parameters = new SqlParameter[]
                            {
                        new SqlParameter("@Username", username),
                        new SqlParameter("@FullName", fullName ?? ""),
                        new SqlParameter("@Email", email ?? ""),
                        new SqlParameter("@Password", password),
                        new SqlParameter("@Role", role)
                            };

                            DatabaseHelper.ExecuteNonQuery(query, parameters);
                            count++;
                        }

                        LoadUsers();
                        MessageBox.Show($"Nhập thành công {count} tài khoản từ Excel!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi nhập Excel: " + ex.Message);
                }
            }
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int userId = Convert.ToInt32(dgvUsers.Rows[e.RowIndex].Cells["UserId"].Value);
            string username = dgvUsers.Rows[e.RowIndex].Cells["TÊN NGƯỜI DÙNG"].Value.ToString();

            if (dgvUsers.Columns[e.ColumnIndex].Name == "Toggle")
            {
                ToggleUserStatus(userId, e.RowIndex);
            }
            else if (dgvUsers.Columns[e.ColumnIndex].Name == "Edit")
            {
                EditUser(userId);
            }
            else if (dgvUsers.Columns[e.ColumnIndex].Name == "Delete")
            {
                DeleteUser(userId, username);
            }
        }

        private void ToggleUserStatus(int userId, int rowIndex)
        {
            try
            {
                bool currentStatus = Convert.ToBoolean(dgvUsers.Rows[rowIndex].Cells["IsActive"].Value);
                bool newStatus = !currentStatus;

                string query = "UPDATE Users SET IsActive = @IsActive WHERE UserId = @UserId";
                SqlParameter[] parameters = {
                    new SqlParameter("@IsActive", newStatus),
                    new SqlParameter("@UserId", userId)
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);

                // Update the cell value directly without reloading entire grid
                dgvUsers.Rows[rowIndex].Cells["IsActive"].Value = newStatus;
                dgvUsers.Rows[rowIndex].Cells["TRẠNG THÁI"].Value = newStatus ? "Hoạt động" : "Khóa";
                dgvUsers.Rows[rowIndex].Cells["Toggle"].Value = newStatus ? "ON" : "OFF";

                // Update cell styles
                if (newStatus)
                {
                    dgvUsers.Rows[rowIndex].Cells["TRẠNG THÁI"].Style.BackColor = Color.FromArgb(209, 250, 229);
                    dgvUsers.Rows[rowIndex].Cells["TRẠNG THÁI"].Style.ForeColor = Color.FromArgb(16, 185, 129);
                    dgvUsers.Rows[rowIndex].Cells["Toggle"].Style.BackColor = Color.FromArgb(79, 70, 229);
                    dgvUsers.Rows[rowIndex].Cells["Toggle"].Style.ForeColor = Color.White;
                }
                else
                {
                    dgvUsers.Rows[rowIndex].Cells["TRẠNG THÁI"].Style.BackColor = Color.FromArgb(254, 226, 226);
                    dgvUsers.Rows[rowIndex].Cells["TRẠNG THÁI"].Style.ForeColor = Color.FromArgb(220, 38, 38);
                    dgvUsers.Rows[rowIndex].Cells["Toggle"].Style.BackColor = Color.FromArgb(209, 213, 219);
                    dgvUsers.Rows[rowIndex].Cells["Toggle"].Style.ForeColor = Color.FromArgb(107, 114, 128);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditUser(int userId)
        {
            UserEditForm editForm = new UserEditForm(userId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
            }
        }

        private void DeleteUser(int userId, string username)
        {
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn xóa tài khoản '{username}'?\nHành động này không thể hoàn tác!",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM Users WHERE UserId = @UserId";
                    DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[] {
                        new SqlParameter("@UserId", userId)
                    });

                    LoadUsers();
                    MessageBox.Show("Xóa tài khoản thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            UserCreateForm createForm = new UserCreateForm();
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterChanged(sender, e);
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.Trim();
                string roleFilter = cboRoleFilter.SelectedIndex == 0 ? "" : cboRoleFilter.SelectedItem.ToString();
                string statusFilter = cboStatusFilter.SelectedIndex == 0 ? "" : (cboStatusFilter.SelectedIndex == 1 ? "1" : "0");

                string query = @"SELECT
                                u.UserId,
                                u.Username as 'TÊN NGƯỜI DÙNG',
                                u.FullName,
                                u.Email as 'EMAIL',
                                CASE u.Role
                                    WHEN 1 THEN N'Quản trị viên'
                                    WHEN 2 THEN N'Giảng viên'
                                    WHEN 3 THEN N'Sinh viên'
                                END as 'VAI TRÒ',
                                FORMAT(u.LastLogin, 'yyyy-MM-dd') as 'ĐĂNG NHẬP GẦN NHẤT',
                                CASE WHEN u.IsActive = 1 THEN N'Hoạt động' ELSE N'Khóa' END as 'TRẠNG THÁI',
                                u.IsActive
                                FROM Users u
                                WHERE 1=1";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " AND (u.Username LIKE @Search OR u.FullName LIKE @Search OR u.Email LIKE @Search)";
                }

                if (roleFilter == "Admin")
                    query += " AND u.Role = 1";
                else if (roleFilter == "Giảng viên")
                    query += " AND u.Role = 2";
                else if (roleFilter == "Sinh viên")
                    query += " AND u.Role = 3";

                if (!string.IsNullOrEmpty(statusFilter))
                    query += " AND u.IsActive = " + statusFilter;

                query += " ORDER BY u.UserId DESC";

                SqlParameter[] parameters = string.IsNullOrEmpty(searchText) ? null : new SqlParameter[] {
                    new SqlParameter("@Search", "%" + searchText + "%")
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
                dgvUsers.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lọc: " + ex.Message);
            }
        }

        private void UserManagementForm_Resize(object sender, EventArgs e)
        {
            try
            {
                if (dgvUsers != null && dgvUsers.DataSource != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
                {
                    // Adjust DataGridView size based on form size
                    int newWidth = this.ClientSize.Width - 60; // 30px padding on each side
                    int newHeight = this.ClientSize.Height - 280; // Header + filters height

                    dgvUsers.Size = new Size(newWidth, newHeight);

                    // Recalculate column widths proportionally
                    if (dgvUsers.Columns.Count > 0)
                    {
                        // Total width available for data columns (excluding fixed-width columns)
                        int availableWidth = newWidth - 60 - 120 - 80 - 80; // Avatar + Toggle + Edit + Delete

                        // Set column widths proportionally
                        if (dgvUsers.Columns.Contains("TÊN NGƯỜI DÙNG") && dgvUsers.Columns["TÊN NGƯỜI DÙNG"] != null)
                            dgvUsers.Columns["TÊN NGƯỜI DÙNG"].Width = (int)(availableWidth * 0.18);
                        if (dgvUsers.Columns.Contains("EMAIL") && dgvUsers.Columns["EMAIL"] != null)
                            dgvUsers.Columns["EMAIL"].Width = (int)(availableWidth * 0.25);
                        if (dgvUsers.Columns.Contains("VAI TRÒ") && dgvUsers.Columns["VAI TRÒ"] != null)
                            dgvUsers.Columns["VAI TRÒ"].Width = (int)(availableWidth * 0.15);
                        if (dgvUsers.Columns.Contains("ĐĂNG NHẬP GẦN NHẤT") && dgvUsers.Columns["ĐĂNG NHẬP GẦN NHẤT"] != null)
                            dgvUsers.Columns["ĐĂNG NHẬP GẦN NHẤT"].Width = (int)(availableWidth * 0.22);
                        if (dgvUsers.Columns.Contains("TRẠNG THÁI") && dgvUsers.Columns["TRẠNG THÁI"] != null)
                            dgvUsers.Columns["TRẠNG THÁI"].Width = (int)(availableWidth * 0.20);
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
