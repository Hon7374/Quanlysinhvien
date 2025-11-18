using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class CourseEditForm : Form
    {
        private int courseId;
        private TextBox txtCourseCode, txtCourseName, txtDescription, txtSemester;
        private NumericUpDown nudCredits, nudAcademicYear, nudMaxStudents;
        private ComboBox cboTeacher;
        private CheckBox chkIsActive;
        private Label lblCreatedDate;

        public CourseEditForm(int courseId)
        {
            this.courseId = courseId;
            InitializeComponent();
            LoadTeachers();
            LoadCourseData();
        }

        private void InitializeComponent()
        {
            this.Text = "Chỉnh sửa Môn học";
            this.Size = new Size(900, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40, 30, 40, 30), BackColor = Color.White, AutoScroll = true };

            Label lblTitle = new Label { Text = "Chỉnh sửa Môn học", Font = new Font("Segoe UI", 24, FontStyle.Bold), Location = new Point(0, 0), AutoSize = true, ForeColor = Color.FromArgb(31, 41, 55) };
            mainPanel.Controls.Add(lblTitle);

            int y = 70;
            AddLbl(mainPanel, "Mã môn học", 0, y);
            txtCourseCode = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 25), Size = new Size(380, 30), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true, BackColor = Color.FromArgb(243, 244, 246) };
            mainPanel.Controls.Add(txtCourseCode);

            AddLbl(mainPanel, "Tên môn học *", 420, y);
            txtCourseName = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(420, y + 25), Size = new Size(380, 30), BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(txtCourseName);

            y += 80;
            AddLbl(mainPanel, "Số tín chỉ *", 0, y);
            nudCredits = new NumericUpDown { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 25), Size = new Size(180, 30), Minimum = 1, Maximum = 10, Value = 3 };
            mainPanel.Controls.Add(nudCredits);

            AddLbl(mainPanel, "Sĩ số tối đa", 220, y);
            nudMaxStudents = new NumericUpDown { Font = new Font("Segoe UI", 10), Location = new Point(220, y + 25), Size = new Size(180, 30), Minimum = 10, Maximum = 200, Value = 50 };
            mainPanel.Controls.Add(nudMaxStudents);

            AddLbl(mainPanel, "Học kỳ", 420, y);
            txtSemester = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(420, y + 25), Size = new Size(180, 30), BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(txtSemester);

            AddLbl(mainPanel, "Năm học", 620, y);
            nudAcademicYear = new NumericUpDown { Font = new Font("Segoe UI", 10), Location = new Point(620, y + 25), Size = new Size(180, 30), Minimum = 2020, Maximum = 2030, Value = DateTime.Now.Year };
            mainPanel.Controls.Add(nudAcademicYear);

            y += 80;
            AddLbl(mainPanel, "Giảng viên", 0, y);
            cboTeacher = new ComboBox { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 25), Size = new Size(380, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            mainPanel.Controls.Add(cboTeacher);

            chkIsActive = new CheckBox { Text = "Đang mở đăng ký", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(420, y + 30), AutoSize = true };
            mainPanel.Controls.Add(chkIsActive);

            y += 80;
            AddLbl(mainPanel, "Mô tả", 0, y);
            txtDescription = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 25), Size = new Size(800, 100), BorderStyle = BorderStyle.FixedSingle, Multiline = true };
            mainPanel.Controls.Add(txtDescription);

            y += 130;
            AddLbl(mainPanel, "Ngày tạo", 0, y);
            lblCreatedDate = new Label { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 25), Size = new Size(300, 30), ForeColor = Color.FromArgb(107, 114, 128), BackColor = Color.FromArgb(243, 244, 246), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            mainPanel.Controls.Add(lblCreatedDate);

            y += 85;
            Panel btnPanel = new Panel { Location = new Point(0, y), Size = new Size(800, 50) };
            Button btnCancel = new Button { Text = "Hủy bỏ", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(550, 0), Size = new Size(120, 45), BackColor = Color.White, ForeColor = Color.FromArgb(107, 114, 128), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            btnPanel.Controls.Add(btnCancel);

            Button btnSave = new Button { Text = "Lưu thay đổi", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(680, 0), Size = new Size(120, 45), BackColor = Color.FromArgb(99, 102, 241), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            btnPanel.Controls.Add(btnSave);

            mainPanel.Controls.Add(btnPanel);
            this.Controls.Add(mainPanel);
        }

        private void AddLbl(Panel p, string t, int x, int y) => p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(x, y), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) });

        private void LoadTeachers()
        {
            try
            {
                cboTeacher.Items.Clear();
                cboTeacher.Items.Add(new CbItem { Text = "Chưa phân công", Value = null });
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT t.TeacherId, u.FullName, t.TeacherCode FROM Teachers t INNER JOIN Users u ON t.UserId = u.UserId WHERE t.Status = N'Đang làm việc' ORDER BY u.FullName");
                foreach (DataRow r in dt.Rows) cboTeacher.Items.Add(new CbItem { Text = $"{r["FullName"]} ({r["TeacherCode"]})", Value = r["TeacherId"] });
                cboTeacher.SelectedIndex = 0;
            }
            catch { }
        }

        private void LoadCourseData()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Courses WHERE CourseId = @CourseId", new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtCourseCode.Text = r["CourseCode"].ToString();
                    txtCourseName.Text = r["CourseName"].ToString();
                    nudCredits.Value = Convert.ToInt32(r["Credits"]);
                    txtDescription.Text = r["Description"].ToString();
                    txtSemester.Text = r["Semester"].ToString();
                    nudAcademicYear.Value = r["AcademicYear"] != DBNull.Value ? Convert.ToInt32(r["AcademicYear"]) : DateTime.Now.Year;
                    nudMaxStudents.Value = Convert.ToInt32(r["MaxStudents"]);
                    chkIsActive.Checked = Convert.ToBoolean(r["IsActive"]);
                    if (r["CreatedAt"] != DBNull.Value) lblCreatedDate.Text = Convert.ToDateTime(r["CreatedAt"]).ToString("yyyy-MM-dd HH:mm:ss");

                    if (r["TeacherId"] != DBNull.Value)
                    {
                        int tid = Convert.ToInt32(r["TeacherId"]);
                        foreach (CbItem item in cboTeacher.Items) if (item.Value != null && (int)item.Value == tid) { cboTeacher.SelectedItem = item; break; }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCourseName.Text)) { MessageBox.Show("Vui lòng nhập tên môn học!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                var ti = cboTeacher.SelectedItem as CbItem;
                object tid = ti?.Value ?? DBNull.Value;

                DatabaseHelper.ExecuteNonQuery("UPDATE Courses SET CourseName=@Name, Credits=@Credits, Description=@Desc, TeacherId=@TID, Semester=@Sem, AcademicYear=@Year, MaxStudents=@Max, IsActive=@Active WHERE CourseId=@ID",
                    new SqlParameter[] {
                        new SqlParameter("@Name", txtCourseName.Text.Trim()),
                        new SqlParameter("@Credits", (int)nudCredits.Value),
                        new SqlParameter("@Desc", txtDescription.Text.Trim()),
                        new SqlParameter("@TID", tid),
                        new SqlParameter("@Sem", txtSemester.Text.Trim()),
                        new SqlParameter("@Year", (int)nudAcademicYear.Value),
                        new SqlParameter("@Max", (int)nudMaxStudents.Value),
                        new SqlParameter("@Active", chkIsActive.Checked),
                        new SqlParameter("@ID", courseId)
                    });

                MessageBox.Show("Cập nhật thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private class CbItem { public string Text { get; set; } public object Value { get; set; } public override string ToString() => Text; }
    }
}
