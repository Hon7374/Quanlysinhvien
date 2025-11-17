using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class SemesterCreateForm : Form
    {
        private TextBox txtSemesterName;
        private TextBox txtSemesterCode;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cmbStatus;

        public SemesterCreateForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Thêm Học kỳ Mới - Add New Semester";
            this.Size = new Size(600, 520);
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
                BackColor = Color.White
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "Thêm Học kỳ Mới",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblTitle);

            int yPos = 80;

            // Semester Name
            AddLabel(mainPanel, "Tên Học kỳ *", 30, yPos);
            txtSemesterName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(520, 30),
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
                Size = new Size(520, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtSemesterCode);

            yPos += 75;

            // Start Date
            AddLabel(mainPanel, "Ngày Bắt Đầu *", 30, yPos);
            dtpStartDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, yPos + 25),
                Size = new Size(250, 30),
                Format = DateTimePickerFormat.Short
            };
            mainPanel.Controls.Add(dtpStartDate);

            // End Date
            AddLabel(mainPanel, "Ngày Kết Thúc *", 300, yPos);
            dtpEndDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(300, yPos + 25),
                Size = new Size(250, 30),
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
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "Sắp tới", "Hoạt động", "Đã kết thúc" });
            cmbStatus.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbStatus);

            yPos += 85;

            // Button Panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(30, yPos),
                Size = new Size(520, 50),
                BackColor = Color.White
            };

            // Cancel Button
            Button btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(270, 0),
                Size = new Size(120, 45),
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
                Text = "Lưu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(400, 0),
                Size = new Size(120, 45),
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

                // Check if semester code already exists
                string checkQuery = "SELECT COUNT(*) FROM Semesters WHERE SemesterCode = @SemesterCode";
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery,
                    new SqlParameter[] { new SqlParameter("@SemesterCode", txtSemesterCode.Text.Trim()) }));

                if (count > 0)
                {
                    MessageBox.Show("Mã học kỳ đã tồn tại! Vui lòng nhập mã khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSemesterCode.Focus();
                    txtSemesterCode.SelectAll();
                    return;
                }

                // Insert semester
                string insertQuery = @"INSERT INTO Semesters
                    (SemesterName, SemesterCode, StartDate, EndDate, Status)
                    VALUES (@SemesterName, @SemesterCode, @StartDate, @EndDate, @Status)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@SemesterName", txtSemesterName.Text.Trim()),
                    new SqlParameter("@SemesterCode", txtSemesterCode.Text.Trim()),
                    new SqlParameter("@StartDate", dtpStartDate.Value.Date),
                    new SqlParameter("@EndDate", dtpEndDate.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem.ToString())
                };

                DatabaseHelper.ExecuteNonQuery(insertQuery, parameters);

                MessageBox.Show("Thêm học kỳ thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm học kỳ: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
