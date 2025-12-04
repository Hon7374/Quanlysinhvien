using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    /// <summary>
    /// Form hiển thị môn học đã đăng ký với chức năng HỦY MÔN
    /// </summary>
    public partial class MyCoursesForm : Form
    {
        private DataGridView dgvCourses;
        private Panel panelHeader;
        private Button btnCancelCourse;
        private Button btnRefresh;
        private Label lblStatus;
        private ComboBox cboSemester;

        public MyCoursesForm()
        {
            InitializeComponent();
            LoadSemesters();
            LoadMyCourses();
        }

        private void InitializeComponent()
        {
            this.Text = "Môn học đã đăng ký - My Courses";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            Label lblTitle = new Label
            {
                Text = "📚 MÔN HỌC ĐÃ ĐĂNG KÝ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            panelHeader.Controls.Add(lblTitle);

            // Semester filter
            Label lblSemester = new Label
            {
                Text = "Học kỳ:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 75),
                AutoSize = true
            };
            panelHeader.Controls.Add(lblSemester);

            cboSemester = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(100, 72),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboSemester.SelectedIndexChanged += (s, e) => LoadMyCourses();
            panelHeader.Controls.Add(cboSemester);

            // Refresh button
            btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(320, 70),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(156, 163, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadMyCourses();
            panelHeader.Controls.Add(btnRefresh);

            // Cancel Course button
            btnCancelCourse = new Button
            {
                Text = "❌ Hủy môn đã chọn",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(460, 70),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelCourse.FlatAppearance.BorderSize = 0;
            btnCancelCourse.Click += BtnCancelCourse_Click;
            panelHeader.Controls.Add(btnCancelCourse);

            this.Controls.Add(panelHeader);

            // DataGridView
            dgvCourses = new DataGridView
            {
                Location = new Point(30, 160),
                Size = new Size(this.ClientSize.Width - 60, this.ClientSize.Height - 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                ColumnHeadersHeight = 50
            };

            dgvCourses.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0)
            };

            dgvCourses.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251)
            };

            dgvCourses.CellFormatting += DgvCourses_CellFormatting;

            this.Controls.Add(dgvCourses);

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, this.ClientSize.Height - 60),
                Size = new Size(this.ClientSize.Width - 60, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            this.Controls.Add(lblStatus);
        }

        private void LoadSemesters()
        {
            try
            {
                cboSemester.Items.Clear();
                cboSemester.Items.Add("-- Tất cả học kỳ --");

                string query = @"
                    SELECT DISTINCT c.Semester
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.CourseId = c.CourseId
                    WHERE e.StudentId = @StudentId
                    ORDER BY c.Semester DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SqlParameter[] { new SqlParameter("@StudentId", SessionManager.CurrentStudent.StudentId) });

                foreach (DataRow row in dt.Rows)
                {
                    cboSemester.Items.Add(row["Semester"].ToString());
                }

                if (cboSemester.Items.Count > 0)
                    cboSemester.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách học kỳ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMyCourses()
        {
            try
            {
                int studentId = SessionManager.CurrentStudent.StudentId;

                string query = @"
                    SELECT
                        e.EnrollmentId,
                        c.CourseCode AS [Mã môn],
                        c.CourseName AS [Tên môn học],
                        c.Credits AS [Tín chỉ],
                        u.FullName AS [Giảng viên],
                        c.Semester AS [Học kỳ],
                        c.AcademicYear AS [Năm học],
                        FORMAT(e.EnrollmentDate, 'dd/MM/yyyy HH:mm') AS [Ngày đăng ký],
                        CASE
                            WHEN e.Status = N'Enrolled' THEN N'Đang học'
                            WHEN e.Status = N'Cancelled' THEN N'Đã hủy'
                            ELSE e.Status
                        END AS [Trạng thái],
                        CASE
                            WHEN e.PaymentStatus = 'Paid' THEN N'Đã thanh toán'
                            WHEN e.PaymentStatus = 'Unpaid' THEN N'Chưa thanh toán'
                            ELSE e.PaymentStatus
                        END AS [Thanh toán],
                        CASE
                            WHEN e.CancelledDate IS NOT NULL THEN FORMAT(e.CancelledDate, 'dd/MM/yyyy HH:mm')
                            ELSE ''
                        END AS [Ngày hủy]
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.CourseId = c.CourseId
                    LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
                    LEFT JOIN Users u ON t.UserId = u.UserId
                    WHERE e.StudentId = @StudentId
                    AND e.Status = N'Enrolled'";

                SqlParameter[] parameters;
                if (cboSemester != null && cboSemester.SelectedIndex > 0 && cboSemester.SelectedItem != null)
                {
                    query += " AND c.Semester = @Semester";
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@Semester", cboSemester.SelectedItem.ToString())
                    };
                }
                else
                {
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@StudentId", studentId)
                    };
                }

                query += " ORDER BY c.Semester DESC, c.CourseCode";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
                dgvCourses.DataSource = dt;

                // Hide EnrollmentId
                if (dgvCourses.Columns.Contains("EnrollmentId"))
                    dgvCourses.Columns["EnrollmentId"].Visible = false;

                // Update status
                int totalEnrolled = 0;
                int totalCancelled = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Trạng thái"].ToString() == "Đang học")
                        totalEnrolled++;
                    else if (row["Trạng thái"].ToString() == "Đã hủy")
                        totalCancelled++;
                }

                lblStatus.Text = $"📊 Tổng số: {dt.Rows.Count} môn học ({totalEnrolled} đang học, {totalCancelled} đã hủy)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvCourses_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvCourses.Columns[e.ColumnIndex].HeaderText == "Trạng thái")
            {
                if (e.Value != null)
                {
                    string status = e.Value.ToString();
                    if (status == "Đang học")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (status == "Đã hủy")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
            else if (dgvCourses.Columns[e.ColumnIndex].HeaderText == "Thanh toán")
            {
                if (e.Value != null)
                {
                    string status = e.Value.ToString();
                    if (status == "Đã thanh toán")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (status == "Chưa thanh toán")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                        e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
        }

        private void BtnCancelCourse_Click(object sender, EventArgs e)
        {
            if (dgvCourses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn môn học cần hủy!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dgvCourses.SelectedRows[0];
                int enrollmentId = Convert.ToInt32(selectedRow.Cells["EnrollmentId"].Value);
                string courseCode = selectedRow.Cells["Mã môn"].Value.ToString();
                string courseName = selectedRow.Cells["Tên môn học"].Value.ToString();
                string status = selectedRow.Cells["Trạng thái"].Value.ToString();
                string paymentStatus = selectedRow.Cells["Thanh toán"].Value.ToString();

                // Kiểm tra trạng thái
                if (status == "Đã hủy")
                {
                    MessageBox.Show("Môn học này đã được hủy trước đó!", "Không thể hủy",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //if (paymentStatus == "Đã thanh toán")
                //{
                //    MessageBox.Show("Không thể hủy môn đã thanh toán học phí!\n\n" +
                //        "Vui lòng liên hệ phòng đào tạo để được hỗ trợ.", "Không thể hủy",
                //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}

                // Xác nhận hủy
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn hủy đăng ký môn học?\n\n" +
                    $"Mã môn: {courseCode}\n" +
                    $"Tên môn: {courseName}\n\n" +
                    $"⚠️ Việc hủy sẽ xóa môn khỏi thời khóa biểu và cập nhật số tín chỉ.",
                    "Xác nhận hủy môn",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Gọi stored procedure để hủy môn
                    int spResult = DatabaseHelper.ExecuteNonQueryStoredProcedure("sp_CancelEnrollment", new SqlParameter[]
                    {
                        new SqlParameter("@EnrollmentId", enrollmentId),
                        new SqlParameter("@StudentId", SessionManager.CurrentStudent.StudentId),
                        new SqlParameter("@Reason", "Sinh viên tự hủy đăng ký")
                    });

                    if (spResult == 0)
                    {
                        MessageBox.Show("Hủy môn học thành công!\n\n" +
                            "Môn học đã được xóa khỏi thời khóa biểu của bạn.", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMyCourses();
                    }
                    //else if (spResult == -3)
                    //{
                    //    MessageBox.Show("Không thể hủy môn đã thanh toán học phí.", "Không thể hủy",
                    //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //}
                    else if (spResult == -4)
                    {
                        MessageBox.Show("Đã quá thời hạn cho phép hủy môn.", "Không thể hủy",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (spResult == -2)
                    {
                        MessageBox.Show("Môn học đã được hủy trước đó.", "Không thể hủy",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (spResult == -1)
                    {
                        MessageBox.Show("Không tìm thấy đăng ký môn học.", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Lỗi khi hủy môn học (mã lỗi: " + spResult + ")", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    LoadMyCourses();
                }
            }
            catch (SqlException sqlEx)
            {
                // Xử lý các lỗi từ stored procedure
                string message = "Lỗi khi hủy môn học:\n\n";

                //if (sqlEx.Message.Contains("Không thể hủy môn đã thanh toán"))
                //{
                //    message += "Không thể hủy môn đã thanh toán học phí.";
                //}
                if (sqlEx.Message.Contains("Đã quá thời hạn"))
                {
                    message += "Đã quá thời hạn cho phép hủy môn.\n" +
                              "Vui lòng liên hệ phòng đào tạo để được hỗ trợ.";
                }
                else if (sqlEx.Message.Contains("Môn học đã được hủy"))
                {
                    message += "Môn học đã được hủy trước đó.";
                }
                else
                {
                    message += sqlEx.Message;
                }

                MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hủy môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
