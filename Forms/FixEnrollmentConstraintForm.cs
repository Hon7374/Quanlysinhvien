using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class FixEnrollmentConstraintForm : Form
    {
        private TextBox txtOutput;
        private Button btnFix;

        public FixEnrollmentConstraintForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Sửa lỗi Enrollment Constraint";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Title
            Label lblTitle = new Label
            {
                Text = "SỬA LỖI UNIQUE CONSTRAINT",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            this.Controls.Add(lblTitle);

            Label lblDesc = new Label
            {
                Text = "Script này sẽ:\n" +
                       "• Xóa constraint cũ UQ_Student_Course (không cho phép đăng ký lại môn đã hủy)\n" +
                       "• Tạo unique filtered index mới UQ_Student_Course_Enrolled\n" +
                       "• Cho phép sinh viên đăng ký lại môn đã hủy",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 70),
                Size = new Size(720, 80),
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lblDesc);

            // Fix Button
            btnFix = new Button
            {
                Text = "🔧 Sửa Constraint",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 170),
                Size = new Size(200, 50),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFix.FlatAppearance.BorderSize = 0;
            btnFix.Click += BtnFix_Click;
            this.Controls.Add(btnFix);

            // Output TextBox
            Label lblOutput = new Label
            {
                Text = "Kết quả:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 240),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(lblOutput);

            txtOutput = new TextBox
            {
                Location = new Point(30, 270),
                Size = new Size(720, 260),
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
                Location = new Point(650, 170),
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

        private void BtnFix_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn sửa constraint?\n\n" +
                "Script sẽ:\n" +
                "• Xóa constraint UQ_Student_Course\n" +
                "• Tạo unique filtered index mới\n" +
                "• Cho phép đăng ký lại môn đã hủy",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes) return;

            btnFix.Enabled = false;
            txtOutput.Clear();
            txtOutput.AppendText("Đang bắt đầu sửa constraint...\r\n");
            txtOutput.AppendText("==========================================\r\n\r\n");

            try
            {
                RunFix();
                txtOutput.AppendText("\r\n==========================================\r\n");
                txtOutput.AppendText("✓✓✓ HOÀN TẤT SỬA CONSTRAINT ✓✓✓\r\n");
                MessageBox.Show("Sửa constraint thành công!\n\nGiờ bạn có thể đăng ký lại môn đã hủy.", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                txtOutput.AppendText($"\r\n❌ LỖI: {ex.Message}\r\n");
                MessageBox.Show($"Lỗi khi sửa constraint:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnFix.Enabled = true;
            }
        }

        private void RunFix()
        {
            // 1. Check if old constraint exists
            txtOutput.AppendText("1. Kiểm tra constraint cũ...\r\n");
            string checkOldConstraint = @"
                SELECT COUNT(*) as Cnt
                FROM sys.indexes
                WHERE name = 'UQ_Student_Course'
                AND object_id = OBJECT_ID('Enrollments')";

            DataTable dt = DatabaseHelper.ExecuteQuery(checkOldConstraint);
            bool hasOldConstraint = Convert.ToInt32(dt.Rows[0]["Cnt"]) > 0;

            if (hasOldConstraint)
            {
                txtOutput.AppendText("   ✓ Tìm thấy constraint cũ UQ_Student_Course\r\n\r\n");

                // 2. Drop old constraint
                txtOutput.AppendText("2. Đang xóa constraint cũ...\r\n");
                try
                {
                    DatabaseHelper.ExecuteNonQuery("ALTER TABLE Enrollments DROP CONSTRAINT UQ_Student_Course");
                    txtOutput.AppendText("   ✓ Đã xóa constraint UQ_Student_Course\r\n\r\n");
                }
                catch (Exception ex)
                {
                    txtOutput.AppendText($"   ⚠ Không thể xóa constraint: {ex.Message}\r\n\r\n");
                }
            }
            else
            {
                txtOutput.AppendText("   ⚠ Constraint cũ không tồn tại\r\n\r\n");
            }

            // 3. Check if new index exists
            txtOutput.AppendText("3. Kiểm tra index mới...\r\n");
            string checkNewIndex = @"
                SELECT COUNT(*) as Cnt
                FROM sys.indexes
                WHERE name = 'UQ_Student_Course_Enrolled'
                AND object_id = OBJECT_ID('Enrollments')";

            DataTable dt2 = DatabaseHelper.ExecuteQuery(checkNewIndex);
            bool hasNewIndex = Convert.ToInt32(dt2.Rows[0]["Cnt"]) > 0;

            if (hasNewIndex)
            {
                txtOutput.AppendText("   ⚠ Index mới đã tồn tại, bỏ qua...\r\n\r\n");
            }
            else
            {
                txtOutput.AppendText("   ✓ Index mới chưa tồn tại, tiến hành tạo...\r\n\r\n");

                // 4. Create new filtered unique index
                txtOutput.AppendText("4. Đang tạo unique filtered index mới...\r\n");
                string createIndex = @"
                    CREATE UNIQUE INDEX UQ_Student_Course_Enrolled
                    ON Enrollments(StudentId, CourseId)
                    WHERE Status = N'Enrolled'";

                DatabaseHelper.ExecuteNonQuery(createIndex);
                txtOutput.AppendText("   ✓ Đã tạo index UQ_Student_Course_Enrolled\r\n");
                txtOutput.AppendText("   → Sinh viên chỉ có thể đăng ký một môn một lần (khi Status = Enrolled)\r\n");
                txtOutput.AppendText("   → Sinh viên CÓ THỂ đăng ký lại môn đã hủy (Status = Cancelled)\r\n\r\n");
            }

            // 5. Show statistics
            txtOutput.AppendText("5. Thống kê:\r\n");
            DataTable stats = DatabaseHelper.ExecuteQuery(
                "SELECT Status, COUNT(*) as SoLuong FROM Enrollments GROUP BY Status");

            foreach (DataRow row in stats.Rows)
            {
                txtOutput.AppendText($"    • Status '{row["Status"]}': {row["SoLuong"]} bản ghi\r\n");
            }
        }
    }
}
