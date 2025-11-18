using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class SemesterEditForm : Form
    {
        private int semesterId;
        private TextBox txtSemesterName;
        private TextBox txtSemesterCode;
        private NumericUpDown nudAcademicYear;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cmbStatus;

        public SemesterEditForm(int semesterId)
        {
            this.semesterId = semesterId;
            InitializeComponent();
            LoadSemesterData();
        }

        private void InitializeComponent()
        {
            this.Text = "Chỉnh sửa Học kỳ - Edit Semester";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(249, 250, 251);

            // Main Panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = Color.White,
                AutoScroll = true
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "Chỉnh sửa Học kỳ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblTitle);

            int yPos = 80;

            // Semester ID (read-only)
            AddLabel(mainPanel, "ID Học kỳ", 30, yPos);
            Label lblSemesterId = new Label
            {
                Text = semesterId.ToString(),
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(100, 30),
                ForeColor = Color.FromArgb(107, 114, 128),
                BackColor = Color.FromArgb(243, 244, 246),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            mainPanel.Controls.Add(lblSemesterId);

            yPos += 75;

            // Semester Name
            AddLabel(mainPanel, "Tên Học kỳ *", 30, yPos);
            txtSemesterName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(620, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtSemesterName);

            yPos += 75;

            // Semester Code
            AddLabel(mainPanel, "Mã Học kỳ *", 30, yPos);
            txtSemesterCode = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(620, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtSemesterCode);

            yPos += 75;

            // Academic Year
            AddLabel(mainPanel, "Năm Học *", 30, yPos);
            nudAcademicYear = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(300, 30),
                Minimum = 2020,
                Maximum = 2050,
                Value = DateTime.Now.Year
            };
            mainPanel.Controls.Add(nudAcademicYear);

            yPos += 75;

            // Start Date
            AddLabel(mainPanel, "Ngày Bắt Đầu *", 30, yPos);
            dtpStartDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(300, 30),
                Format = DateTimePickerFormat.Short
            };
            mainPanel.Controls.Add(dtpStartDate);

            // End Date
            AddLabel(mainPanel, "Ngày Kết Thúc *", 350, yPos);
            dtpEndDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(350, yPos + 25),
                Size = new Size(300, 30),
                Format = DateTimePickerFormat.Short
            };
            mainPanel.Controls.Add(dtpEndDate);

            yPos += 75;

            // Status
            AddLabel(mainPanel, "Trạng thái *", 30, yPos);
            cmbStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "Sắp tới", "Hoạt động", "Đã kết thúc" });
            mainPanel.Controls.Add(cmbStatus);

            yPos += 85;

            // Created Date (read-only)
            AddLabel(mainPanel, "Ngày tạo", 30, yPos);
            Label lblCreatedDate = new Label
            {
                Name = "lblCreatedDate",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(300, 30),
                ForeColor = Color.FromArgb(107, 114, 128),
                BackColor = Color.FromArgb(243, 244, 246),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            mainPanel.Controls.Add(lblCreatedDate);

            yPos += 85;

            // Button Panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(30, yPos),
                Size = new Size(620, 50),
                BackColor = Color.White
            };

            // Cancel Button
            Button btnCancel = new Button
            {
                Text = "Hủy bỏ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(350, 0),
                Size = new Size(130, 45),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(btnCancel);

            // Save Button
            Button btnSave = new Button
            {
                Text = "Lưu thay đổi",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(490, 0),
                Size = new Size(130, 45),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void AddLabel(Panel panel, string text, int x, int y)
        {
            Label label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            panel.Controls.Add(label);
        }

        private void LoadSemesterData()
        {
            try
            {
                string query = @"SELECT SemesterName, SemesterCode, AcademicYear, StartDate, EndDate,
                                Status, CreatedAt FROM Semesters WHERE SemesterId = @SemesterId";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@SemesterId", semesterId) });

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtSemesterName.Text = row["SemesterName"].ToString();
                    txtSemesterCode.Text = row["SemesterCode"].ToString();

                    if (row["AcademicYear"] != DBNull.Value)
                        nudAcademicYear.Value = Convert.ToInt32(row["AcademicYear"]);

                    dtpStartDate.Value = Convert.ToDateTime(row["StartDate"]);
                    dtpEndDate.Value = Convert.ToDateTime(row["EndDate"]);

                    string status = row["Status"].ToString();
                    cmbStatus.SelectedItem = status;

                    // Set created date
                    Control[] lblArray = this.Controls.Find("lblCreatedDate", true);
                    if (lblArray.Length > 0 && lblArray[0] is Label lblCreatedDate)
                    {
                        if (row["CreatedAt"] != DBNull.Value)
                        {
                            lblCreatedDate.Text = Convert.ToDateTime(row["CreatedAt"]).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            lblCreatedDate.Text = "N/A";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin học kỳ: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtSemesterName.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên học kỳ!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSemesterName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSemesterCode.Text))
                {
                    MessageBox.Show("Vui lòng nhập mã học kỳ!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSemesterCode.Focus();
                    return;
                }

                if (dtpEndDate.Value <= dtpStartDate.Value)
                {
                    MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpEndDate.Focus();
                    return;
                }

                // Check if semester code already exists (excluding current semester)
                string checkQuery = "SELECT COUNT(*) FROM Semesters WHERE SemesterCode = @SemesterCode AND SemesterId != @SemesterId";
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery,
                    new SqlParameter[] {
                        new SqlParameter("@SemesterCode", txtSemesterCode.Text.Trim()),
                        new SqlParameter("@SemesterId", semesterId)
                    }));

                if (count > 0)
                {
                    MessageBox.Show("Mã học kỳ đã tồn tại! Vui lòng nhập mã khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSemesterCode.Focus();
                    txtSemesterCode.SelectAll();
                    return;
                }

                // Update semester
                string updateQuery = @"UPDATE Semesters SET
                    SemesterName = @SemesterName,
                    SemesterCode = @SemesterCode,
                    AcademicYear = @AcademicYear,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Status = @Status
                    WHERE SemesterId = @SemesterId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@SemesterName", txtSemesterName.Text.Trim()),
                    new SqlParameter("@SemesterCode", txtSemesterCode.Text.Trim()),
                    new SqlParameter("@AcademicYear", (int)nudAcademicYear.Value),
                    new SqlParameter("@StartDate", dtpStartDate.Value.Date),
                    new SqlParameter("@EndDate", dtpEndDate.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem.ToString()),
                    new SqlParameter("@SemesterId", semesterId)
                };

                DatabaseHelper.ExecuteNonQuery(updateQuery, parameters);

                MessageBox.Show("Cập nhật học kỳ thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật học kỳ: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
