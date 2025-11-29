using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class DatabaseUpdateForm : Form
    {
        private TextBox txtOutput;
        private Button btnUpdate;

        public DatabaseUpdateForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Cập nhật Database - Update Semester Format";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Title
            Label lblTitle = new Label
            {
                Text = "CẬP NHẬT DATABASE - CHUẨN HÓA HỌC KỲ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            this.Controls.Add(lblTitle);

            Label lblDesc = new Label
            {
                Text = "Script này sẽ:\n" +
                       "• Cập nhật định dạng học kỳ sang 'HK1 2025-2026', 'HK2 2025-2026', 'HK3 2025-2026'\n" +
                       "• Thêm 29 môn học mẫu cho các học kỳ\n" +
                       "• Tạo lịch học tự động cho HK1 2025-2026\n" +
                       "• Tạo enrollment mẫu cho sinh viên test",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 70),
                Size = new Size(820, 100),
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lblDesc);

            // Update Button
            btnUpdate = new Button
            {
                Text = "🔄 Chạy Script Cập Nhật",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 180),
                Size = new Size(250, 50),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += BtnUpdate_Click;
            this.Controls.Add(btnUpdate);

            // Output TextBox
            Label lblOutput = new Label
            {
                Text = "Kết quả:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 250),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lblOutput);

            txtOutput = new TextBox
            {
                Location = new Point(30, 280),
                Size = new Size(820, 350),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.FromArgb(229, 231, 235),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtOutput);

            // Close Button
            Button btnClose = new Button
            {
                Text = "Đóng",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(750, 180),
                Size = new Size(100, 50),
                BackColor = Color.FromArgb(156, 163, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnClose);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn cập nhật database?\n\n" +
                "Script sẽ:\n" +
                "• Cập nhật định dạng học kỳ hiện có\n" +
                "• Xóa và tạo lại môn học mẫu\n" +
                "• Xóa và tạo lại lịch học\n" +
                "• Cập nhật enrollment\n\n" +
                "LƯU Ý: Dữ liệu cũ sẽ bị XÓA!",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes) return;

            btnUpdate.Enabled = false;
            txtOutput.Clear();
            txtOutput.AppendText("Đang bắt đầu cập nhật database...\r\n");
            txtOutput.AppendText("==========================================\r\n\r\n");

            try
            {
                RunUpdate();
                txtOutput.AppendText("\r\n==========================================\r\n");
                txtOutput.AppendText("✓✓✓ HOÀN TẤT CẬP NHẬT DATABASE ✓✓✓\r\n");
                MessageBox.Show("Cập nhật database thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                txtOutput.AppendText($"\r\n❌ LỖI: {ex.Message}\r\n");
                MessageBox.Show($"Lỗi khi cập nhật database:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpdate.Enabled = true;
            }
        }

        private void RunUpdate()
        {
            // 1. Update existing semester format
            txtOutput.AppendText("1. Đang cập nhật định dạng học kỳ...\r\n");
            DatabaseHelper.ExecuteNonQuery("UPDATE Courses SET Semester = 'HK1 2025-2026' WHERE Semester IN ('HK1', 'Học kỳ I', '2024-1', 'Fall 2024', 'HK1 2024-2025', 'HK1-2024')");
            DatabaseHelper.ExecuteNonQuery("UPDATE Courses SET Semester = 'HK2 2025-2026' WHERE Semester IN ('HK2', 'Học kỳ II', '2024-2', 'Spring 2024', 'HK2 2024-2025', 'HK2-2024')");
            DatabaseHelper.ExecuteNonQuery("UPDATE Courses SET Semester = 'HK3 2025-2026' WHERE Semester IN ('HK3', 'Học kỳ III', '2024-3', 'Summer 2024', 'HK3 2024-2025', 'HK3-2024')");
            txtOutput.AppendText("   ✓ Đã cập nhật định dạng học kỳ\r\n\r\n");

            // 2. Get default teacher
            txtOutput.AppendText("2. Đang lấy thông tin giảng viên...\r\n");
            DataTable teachers = DatabaseHelper.ExecuteQuery("SELECT TOP 1 TeacherId FROM Teachers");
            if (teachers.Rows.Count == 0)
            {
                throw new Exception("Không tìm thấy giảng viên trong hệ thống. Vui lòng thêm giảng viên trước.");
            }
            int defaultTeacherId = Convert.ToInt32(teachers.Rows[0]["TeacherId"]);
            txtOutput.AppendText($"   ✓ Sử dụng TeacherId: {defaultTeacherId}\r\n\r\n");

            // 3. Delete old courses
            txtOutput.AppendText("3. Đang xóa môn học cũ...\r\n");
            string deleteCourses = @"DELETE FROM Courses WHERE CourseCode IN (
                'CS101', 'CS102', 'CS201', 'CS202', 'CS301', 'CS302',
                'MATH101', 'MATH201', 'PHYS101', 'ENG101', 'ENG102',
                'BUS101', 'BUS201', 'ART101', 'CHEM101', 'BIO101',
                'CS103', 'CS203', 'CS303', 'MATH102', 'MATH202',
                'PHYS102', 'ENG201', 'BUS102', 'ART201',
                'CS104', 'MATH103', 'ENG103', 'BUS103'
            )";
            DatabaseHelper.ExecuteNonQuery(deleteCourses);
            txtOutput.AppendText("   ✓ Đã xóa môn học cũ\r\n\r\n");

            // 4. Insert HK1 2025-2026 courses
            txtOutput.AppendText("4. Đang thêm môn học HK1 2025-2026...\r\n");
            AddCourse("CS101", "Lập trình căn bản", 3, "Học các khái niệm cơ bản về lập trình với C/C++", defaultTeacherId, "HK1 2025-2026", 60);
            AddCourse("CS102", "Cấu trúc dữ liệu", 4, "Các cấu trúc dữ liệu cơ bản và nâng cao", defaultTeacherId, "HK1 2025-2026", 50);
            AddCourse("CS201", "Cơ sở dữ liệu", 3, "Thiết kế và quản lý cơ sở dữ liệu quan hệ", defaultTeacherId, "HK1 2025-2026", 55);
            AddCourse("CS202", "Lập trình hướng đối tượng", 4, "OOP với Java và C#", defaultTeacherId, "HK1 2025-2026", 50);
            AddCourse("CS301", "Phát triển Web", 3, "HTML, CSS, JavaScript, React", defaultTeacherId, "HK1 2025-2026", 45);
            AddCourse("CS302", "Mạng máy tính", 3, "Các giao thức mạng và bảo mật", defaultTeacherId, "HK1 2025-2026", 40);
            AddCourse("MATH101", "Toán cao cấp A1", 3, "Giải tích hàm một biến", defaultTeacherId, "HK1 2025-2026", 80);
            AddCourse("MATH201", "Đại số tuyến tính", 3, "Ma trận, định thức, không gian vector", defaultTeacherId, "HK1 2025-2026", 70);
            AddCourse("PHYS101", "Vật lý đại cương", 3, "Cơ học, nhiệt học, điện từ học", defaultTeacherId, "HK1 2025-2026", 60);
            AddCourse("ENG101", "Tiếng Anh cơ bản", 2, "Ngữ pháp và từ vựng cơ bản", defaultTeacherId, "HK1 2025-2026", 50);
            AddCourse("ENG102", "Tiếng Anh giao tiếp", 2, "Kỹ năng nghe nói", defaultTeacherId, "HK1 2025-2026", 50);
            AddCourse("BUS101", "Kinh tế vi mô", 3, "Các nguyên lý kinh tế cơ bản", defaultTeacherId, "HK1 2025-2026", 60);
            AddCourse("BUS201", "Quản trị học", 3, "Nguyên lý quản trị doanh nghiệp", defaultTeacherId, "HK1 2025-2026", 55);
            AddCourse("ART101", "Mỹ thuật đại cương", 2, "Các khái niệm cơ bản về nghệ thuật", defaultTeacherId, "HK1 2025-2026", 40);
            AddCourse("CHEM101", "Hóa học đại cương", 3, "Hóa vô cơ và hóa hữu cơ cơ bản", defaultTeacherId, "HK1 2025-2026", 50);
            AddCourse("BIO101", "Sinh học đại cương", 3, "Tế bào học, di truyền học cơ bản", defaultTeacherId, "HK1 2025-2026", 50);
            txtOutput.AppendText("   ✓ Đã thêm 16 môn học HK1 2025-2026\r\n\r\n");

            // 5. Insert HK2 2025-2026 courses
            txtOutput.AppendText("5. Đang thêm môn học HK2 2025-2026...\r\n");
            AddCourse("CS103", "Lập trình nâng cao", 4, "Kỹ thuật lập trình nâng cao", defaultTeacherId, "HK2 2025-2026", 50);
            AddCourse("CS203", "Hệ điều hành", 3, "Các khái niệm hệ điều hành", defaultTeacherId, "HK2 2025-2026", 45);
            AddCourse("CS303", "Trí tuệ nhân tạo", 3, "Machine Learning cơ bản", defaultTeacherId, "HK2 2025-2026", 40);
            AddCourse("MATH102", "Toán cao cấp A2", 3, "Giải tích hàm nhiều biến", defaultTeacherId, "HK2 2025-2026", 70);
            AddCourse("MATH202", "Xác suất thống kê", 3, "Lý thuyết xác suất và thống kê", defaultTeacherId, "HK2 2025-2026", 60);
            AddCourse("PHYS102", "Vật lý nâng cao", 3, "Quang học, lượng tử", defaultTeacherId, "HK2 2025-2026", 50);
            AddCourse("ENG201", "Tiếng Anh chuyên ngành", 3, "Tiếng Anh IT và Kỹ thuật", defaultTeacherId, "HK2 2025-2026", 45);
            AddCourse("BUS102", "Marketing căn bản", 3, "Nguyên lý marketing", defaultTeacherId, "HK2 2025-2026", 50);
            AddCourse("ART201", "Thiết kế đồ họa", 3, "Photoshop, Illustrator cơ bản", defaultTeacherId, "HK2 2025-2026", 35);
            txtOutput.AppendText("   ✓ Đã thêm 9 môn học HK2 2025-2026\r\n\r\n");

            // 6. Insert HK3 2025-2026 courses
            txtOutput.AppendText("6. Đang thêm môn học HK3 2025-2026...\r\n");
            AddCourse("CS104", "Dự án thực tập", 3, "Dự án thực tế cuối khóa", defaultTeacherId, "HK3 2025-2026", 30);
            AddCourse("MATH103", "Toán rời rạc", 3, "Logic, đồ thị, tổ hợp", defaultTeacherId, "HK3 2025-2026", 40);
            AddCourse("ENG103", "TOEIC cơ bản", 2, "Luyện thi TOEIC", defaultTeacherId, "HK3 2025-2026", 35);
            AddCourse("BUS103", "Kỹ năng mềm", 2, "Giao tiếp, làm việc nhóm", defaultTeacherId, "HK3 2025-2026", 40);
            txtOutput.AppendText("   ✓ Đã thêm 4 môn học HK3 2025-2026\r\n\r\n");

            // 7. Delete old schedules
            txtOutput.AppendText("7. Đang xóa lịch học cũ...\r\n");
            DatabaseHelper.ExecuteNonQuery("DELETE FROM Schedules");
            txtOutput.AppendText("   ✓ Đã xóa lịch học cũ\r\n\r\n");

            // 8. Create schedules for HK1 2025-2026
            txtOutput.AppendText("8. Đang tạo lịch học cho HK1 2025-2026...\r\n");
            CreateSchedulesForSemester("HK1 2025-2026");
            txtOutput.AppendText("   ✓ Đã tạo lịch học\r\n\r\n");

            // 9. Create sample enrollments
            txtOutput.AppendText("9. Đang tạo enrollment mẫu...\r\n");
            CreateSampleEnrollments();
            txtOutput.AppendText("   ✓ Đã tạo enrollment mẫu\r\n\r\n");

            // 10. Show statistics
            txtOutput.AppendText("10. Thống kê:\r\n");
            DataTable stats = DatabaseHelper.ExecuteQuery(
                "SELECT Semester, COUNT(*) as SoMon, SUM(MaxStudents) as TongSiSo FROM Courses GROUP BY Semester ORDER BY Semester DESC");
            foreach (DataRow row in stats.Rows)
            {
                txtOutput.AppendText($"    • {row["Semester"]}: {row["SoMon"]} môn học, {row["TongSiSo"]} sĩ số\r\n");
            }

            DataTable scheduleCount = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) as Total FROM Schedules");
            txtOutput.AppendText($"    • Tổng lịch học: {scheduleCount.Rows[0]["Total"]}\r\n");

            DataTable enrollmentCount = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) as Total FROM Enrollments");
            txtOutput.AppendText($"    • Tổng enrollment: {enrollmentCount.Rows[0]["Total"]}\r\n");
        }

        private void AddCourse(string code, string name, int credits, string desc, int teacherId, string semester, int maxStudents)
        {
            string query = @"INSERT INTO Courses (CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
                           VALUES (@Code, @Name, @Credits, @Desc, @TeacherId, @Semester, 2025, @MaxStudents, 1)";
            DatabaseHelper.ExecuteNonQuery(query, new System.Data.SqlClient.SqlParameter[] {
                new System.Data.SqlClient.SqlParameter("@Code", code),
                new System.Data.SqlClient.SqlParameter("@Name", name),
                new System.Data.SqlClient.SqlParameter("@Credits", credits),
                new System.Data.SqlClient.SqlParameter("@Desc", desc),
                new System.Data.SqlClient.SqlParameter("@TeacherId", teacherId),
                new System.Data.SqlClient.SqlParameter("@Semester", semester),
                new System.Data.SqlClient.SqlParameter("@MaxStudents", maxStudents)
            });
        }

        private void CreateSchedulesForSemester(string semester)
        {
            // CS courses: Mon, Wed, Fri (Morning slots: Tiết 1, 2, 3)
            CreateSchedule("CS101", 0, 0, "P101"); // Mon Tiết 1 (7:00-7:50)
            CreateSchedule("CS102", 0, 1, "P102"); // Mon Tiết 2 (7:55-8:45)
            CreateSchedule("CS201", 2, 0, "P201"); // Wed Tiết 1 (7:00-7:50)
            CreateSchedule("CS202", 2, 1, "P202"); // Wed Tiết 2 (7:55-8:45)
            CreateSchedule("CS301", 4, 0, "P301"); // Fri Tiết 1 (7:00-7:50)
            CreateSchedule("CS302", 4, 1, "P302"); // Fri Tiết 2 (7:55-8:45)

            // MATH courses: Tue, Thu (Morning slots)
            CreateSchedule("MATH101", 1, 0, "A101"); // Tue Tiết 1 (7:00-7:50)
            CreateSchedule("MATH201", 3, 0, "A201"); // Thu Tiết 1 (7:00-7:50)

            // Other courses: Afternoon (Tiết 6-9: slots 5-8)
            CreateSchedule("PHYS101", 0, 6, "B101"); // Mon Tiết 7 (13:25-14:15)
            CreateSchedule("ENG101", 1, 6, "B201"); // Tue Tiết 7 (13:25-14:15)
            CreateSchedule("ENG102", 2, 6, "B202"); // Wed Tiết 7 (13:25-14:15)
            CreateSchedule("BUS101", 3, 6, "B301"); // Thu Tiết 7 (13:25-14:15)
            CreateSchedule("BUS201", 4, 6, "B302"); // Fri Tiết 7 (13:25-14:15)

            // Saturday courses (Morning slots)
            CreateSchedule("ART101", 5, 0, "C101"); // Sat Tiết 1 (7:00-7:50)
            CreateSchedule("CHEM101", 5, 1, "C201"); // Sat Tiết 2 (7:55-8:45)
            CreateSchedule("BIO101", 5, 2, "C301"); // Sat Tiết 3 (8:50-9:40)
        }

        private void CreateSchedule(string courseCode, int dayOfWeek, int timeSlot, string room)
        {
            // 14 time slots: Buổi sáng (0-4), Buổi chiều (5-9), Buổi tối (10-13)
            string[] startTimes = {
                "07:00:00", "07:55:00", "08:50:00", "09:50:00", "10:45:00",  // Tiết 1-5 (Morning)
                "12:30:00", "13:25:00", "14:20:00", "15:20:00", "16:15:00",  // Tiết 6-10 (Afternoon)
                "17:30:00", "18:25:00", "19:20:00", "20:15:00"               // Tiết 11-14 (Evening)
            };
            string[] endTimes = {
                "07:50:00", "08:45:00", "09:40:00", "10:40:00", "11:35:00",  // Tiết 1-5
                "13:20:00", "14:15:00", "15:10:00", "16:10:00", "17:05:00",  // Tiết 6-10
                "18:20:00", "19:15:00", "20:10:00", "21:05:00"               // Tiết 11-14
            };

            string query = @"INSERT INTO Schedules (CourseId, DayOfWeek, TimeSlot, Room, StartTime, EndTime)
                           SELECT CourseId, @Day, @Slot, @Room, @Start, @End FROM Courses WHERE CourseCode = @Code";

            DatabaseHelper.ExecuteNonQuery(query, new System.Data.SqlClient.SqlParameter[] {
                new System.Data.SqlClient.SqlParameter("@Code", courseCode),
                new System.Data.SqlClient.SqlParameter("@Day", dayOfWeek),
                new System.Data.SqlClient.SqlParameter("@Slot", timeSlot),
                new System.Data.SqlClient.SqlParameter("@Room", room),
                new System.Data.SqlClient.SqlParameter("@Start", startTimes[timeSlot]),
                new System.Data.SqlClient.SqlParameter("@End", endTimes[timeSlot])
            });
        }

        private void CreateSampleEnrollments()
        {
            DataTable students = DatabaseHelper.ExecuteQuery("SELECT TOP 2 StudentId FROM Students ORDER BY StudentId");
            if (students.Rows.Count == 0) return;

            int studentId1 = Convert.ToInt32(students.Rows[0]["StudentId"]);

            // Delete old enrollments (parameterized)
            DatabaseHelper.ExecuteNonQuery("DELETE FROM Enrollments WHERE StudentId = @StudentId", new System.Data.SqlClient.SqlParameter[] {
                new System.Data.SqlClient.SqlParameter("@StudentId", studentId1)
            });

            // Enroll student in 5 courses
            DataTable courses = DatabaseHelper.ExecuteQuery("SELECT TOP 5 CourseId FROM Courses WHERE Semester = 'HK1 2025-2026' ORDER BY CourseCode");
                foreach (DataRow course in courses.Rows)
                {
                    try
                    {
                        DatabaseHelper.ExecuteNonQueryStoredProcedure("sp_RegisterStudentToCourse", new System.Data.SqlClient.SqlParameter[] {
                            new System.Data.SqlClient.SqlParameter("@StudentId", studentId1),
                            new System.Data.SqlClient.SqlParameter("@CourseId", course["CourseId"])
                        });
                    }
                    catch (Exception ex)
                    {
                        // Ignore sample insertion failures but log in Debug
                        System.Diagnostics.Debug.WriteLine($"Sample enrollment failed for course {course["CourseId"]}: {ex.Message}");
                    }
                }
        }
    }
}
