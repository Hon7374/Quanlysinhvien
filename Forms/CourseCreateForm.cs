using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class CourseCreateForm : Form
    {
        private TextBox txtCourseCode;
        private TextBox txtCourseName;
        private NumericUpDown nudCredits;
        private TextBox txtDescription;
        private ComboBox cboTeacher;
        private TextBox txtSemester;
        private NumericUpDown nudAcademicYear;
        private NumericUpDown nudMaxStudents;
        private CheckBox chkIsActive;

        public CourseCreateForm()
        {
            InitializeComponent();
            LoadTeachers();
        }

        private void InitializeComponent()
        {
            this.Text = "Thêm Môn học Mới - Add New Course";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40, 30, 40, 30),
                BackColor = Color.White,
                AutoScroll = true
            };

            Label lblMainTitle = new Label
            {
                Text = "Quản lý Môn học",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblMainTitle);

            Label lblSubTitle = new Label
            {
                Text = "Thêm Môn học Mới",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(0, 35),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(lblSubTitle);

            int yPos = 100;

            // Course Code
            AddLabel(mainPanel, "Mã môn học *", 0, yPos);
            txtCourseCode = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtCourseCode);

            // Course Name
            AddLabel(mainPanel, "Tên môn học *", 420, yPos);
            txtCourseName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(420, yPos + 25),
                Size = new Size(380, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtCourseName);

            yPos += 80;

            // Credits
            AddLabel(mainPanel, "Số tín chỉ *", 0, yPos);
            nudCredits = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(180, 30),
                Minimum = 1,
                Maximum = 10,
                Value = 3
            };
            mainPanel.Controls.Add(nudCredits);

            // Max Students
            AddLabel(mainPanel, "Sĩ số tối đa *", 220, yPos);
            nudMaxStudents = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(220, yPos + 25),
                Size = new Size(180, 30),
                Minimum = 10,
                Maximum = 200,
                Value = 50
            };
            mainPanel.Controls.Add(nudMaxStudents);

            // Semester
            AddLabel(mainPanel, "Học kỳ", 420, yPos);
            txtSemester = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(420, yPos + 25),
                Size = new Size(180, 30),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "HK1 2024-2025"
            };
            mainPanel.Controls.Add(txtSemester);

            // Academic Year
            AddLabel(mainPanel, "Năm học", 620, yPos);
            nudAcademicYear = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(620, yPos + 25),
                Size = new Size(180, 30),
                Minimum = 2020,
                Maximum = 2030,
                Value = DateTime.Now.Year
            };
            mainPanel.Controls.Add(nudAcademicYear);

            yPos += 80;

            // Teacher
            AddLabel(mainPanel, "Giảng viên phụ trách", 0, yPos);
            cboTeacher = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(380, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(cboTeacher);

            // Is Active
            chkIsActive = new CheckBox
            {
                Text = "Đang mở đăng ký",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(420, yPos + 30),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            mainPanel.Controls.Add(chkIsActive);

            yPos += 80;

            // Description
            AddLabel(mainPanel, "Mô tả", 0, yPos);
            txtDescription = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos + 25),
                Size = new Size(800, 100),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                PlaceholderText = "Mô tả chi tiết về môn học..."
            };
            mainPanel.Controls.Add(txtDescription);

            yPos += 150;

            // Buttons
            Panel buttonPanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(800, 50),
                BackColor = Color.White
            };

            Button btnCancel = new Button
            {
                Text = "Hủy",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(550, 0),
                Size = new Size(120, 45),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(btnCancel);

            Button btnSave = new Button
            {
                Text = "Lưu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(680, 0),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(99, 102, 241),
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

        private void LoadTeachers()
        {
            try
            {
                cboTeacher.Items.Clear();
                cboTeacher.Items.Add(new ComboBoxItem { Text = "Chưa phân công", Value = null });

                string query = @"SELECT t.TeacherId, u.FullName, t.TeacherCode
                                FROM Teachers t
                                INNER JOIN Users u ON t.UserId = u.UserId
                                WHERE t.Status = N'Đang làm việc'
                                ORDER BY u.FullName";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    cboTeacher.Items.Add(new ComboBoxItem
                    {
                        Text = $"{row["FullName"]} ({row["TeacherCode"]})",
                        Value = row["TeacherId"]
                    });
                }
                cboTeacher.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách giảng viên: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCourseCode.Text))
                {
                    MessageBox.Show("Vui lòng nhập mã môn học!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCourseCode.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCourseName.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên môn học!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCourseName.Focus();
                    return;
                }

                string checkQuery = "SELECT COUNT(*) FROM Courses WHERE CourseCode = @CourseCode";
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery,
                    new SqlParameter[] { new SqlParameter("@CourseCode", txtCourseCode.Text.Trim()) }));

                if (count > 0)
                {
                    MessageBox.Show("Mã môn học đã tồn tại! Vui lòng nhập mã khác.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCourseCode.Focus();
                    txtCourseCode.SelectAll();
                    return;
                }

                string insertQuery = @"INSERT INTO Courses
                    (CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
                    VALUES (@CourseCode, @CourseName, @Credits, @Description, @TeacherId, @Semester, @AcademicYear, @MaxStudents, @IsActive)";

                var teacherItem = cboTeacher.SelectedItem as ComboBoxItem;
                object teacherId = teacherItem?.Value ?? DBNull.Value;

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@CourseCode", txtCourseCode.Text.Trim()),
                    new SqlParameter("@CourseName", txtCourseName.Text.Trim()),
                    new SqlParameter("@Credits", (int)nudCredits.Value),
                    new SqlParameter("@Description", txtDescription.Text.Trim()),
                    new SqlParameter("@TeacherId", teacherId),
                    new SqlParameter("@Semester", txtSemester.Text.Trim()),
                    new SqlParameter("@AcademicYear", (int)nudAcademicYear.Value),
                    new SqlParameter("@MaxStudents", (int)nudMaxStudents.Value),
                    new SqlParameter("@IsActive", chkIsActive.Checked)
                };

                DatabaseHelper.ExecuteNonQuery(insertQuery, parameters);

                MessageBox.Show("Thêm môn học thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm môn học: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}
