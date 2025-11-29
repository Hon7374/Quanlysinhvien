using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudentManagement.Data;
using StudentManagement.Helpers;

namespace StudentManagement.Forms
{
    /// <summary>
    /// Form thanh toán học phí bằng VietQR
    /// </summary>
    public partial class PaymentForm : Form
    {
        private Panel panelLeft;
        private Panel panelRight;
        private DataGridView dgvCourses;
        private PictureBox picQRCode;
        private Label lblTotalAmount;
        private Label lblDescription;
        private Label lblStatus;
        private Button btnConfirmPayment;
        private Button btnRefreshQR;
        private Button btnClose;
        private LinkLabel lnkQRCodeUrl;

        private int currentPaymentId = 0;
        private decimal totalAmount = 0;
        private string semester;
        private int academicYear;

        // VietQR Settings
        private string bankId = "MB"; // default bank code
        private string accountNo = "0123456789";
        private string accountName = "TRUONG DAI HOC XYZ";
        private string template = "print";

        public PaymentForm()
        {
            LoadPaymentSettings();
            InitializeComponent();
            LoadCurrentSemester();

            // Load courses sau khi form hiển thị để tránh lỗi disposed
            this.Load += (s, e) => LoadUnpaidCourses();
        }

        private void LoadPaymentSettings()
        {
            try
            {
                string query = "SELECT SettingKey, SettingValue FROM PaymentSettings";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    string key = row["SettingKey"].ToString();
                    string value = row["SettingValue"].ToString();

                    switch (key)
                    {
                        case "VIETQR_BANK_ID":
                            bankId = value;
                            break;
                        case "VIETQR_ACCOUNT_NO":
                            accountNo = value;
                            break;
                        case "VIETQR_ACCOUNT_NAME":
                            accountName = value;
                            break;
                        case "VIETQR_TEMPLATE":
                            template = value;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải cài đặt thanh toán: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Thanh toán học phí - VietQR Payment";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header
            Panel panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(52, 152, 219)
            };

            Label lblTitle = new Label
            {
                Text = "💳 THANH TOÁN HỌC PHÍ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 20),
                AutoSize = true
            };
            panelHeader.Controls.Add(lblTitle);
            this.Controls.Add(panelHeader);

            // Left Panel - Course List
            panelLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 650,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblCoursesTitle = new Label
            {
                Text = "📚 Danh sách môn học cần thanh toán",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            panelLeft.Controls.Add(lblCoursesTitle);

            dgvCourses = new DataGridView
            {
                Location = new Point(20, 50),
                Size = new Size(610, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                ColumnHeadersHeight = 40
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

            panelLeft.Controls.Add(dgvCourses);

            // Total Amount
            Panel panelTotal = new Panel
            {
                Location = new Point(20, 460),
                Size = new Size(610, 80),
                BackColor = Color.FromArgb(46, 204, 113),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTotalAmount = new Label
            {
                Text = "TỔNG TIỀN: 0 VNĐ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                AutoSize = true
            };
            panelTotal.Controls.Add(lblTotalAmount);
            panelLeft.Controls.Add(panelTotal);

            // Description
            lblDescription = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(20, 550),
                Size = new Size(610, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            panelLeft.Controls.Add(lblDescription);

            this.Controls.Add(panelLeft);

            // Right Panel - QR Code
            panelRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(20)
            };

            Label lblQRTitle = new Label
            {
                Text = "📱 Quét mã QR để thanh toán",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            panelRight.Controls.Add(lblQRTitle);

            picQRCode = new PictureBox
            {
                Location = new Point(20, 50),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };
            picQRCode.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            // Subscribe to async load completed to update status and debug
            picQRCode.LoadCompleted += PicQRCode_LoadCompleted;
            panelRight.Controls.Add(picQRCode);

            // Instructions
            Label lblInstructions = new Label
            {
                Text = "📋 Hướng dẫn:\n" +
                       "1. Mở ứng dụng ngân hàng trên điện thoại\n" +
                       "2. Quét mã QR trên màn hình\n" +
                       "3. Kiểm tra thông tin và xác nhận thanh toán\n" +
                       "4. Sau khi chuyển khoản thành công, bấm 'Đã thanh toán'",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 460),
                Size = new Size(450, 120),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            panelRight.Controls.Add(lblInstructions);

            lnkQRCodeUrl = new LinkLabel
            {
                Text = "Mở QR trên trình duyệt",
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                Location = new Point(20, 590),
                AutoSize = true,
                LinkColor = Color.FromArgb(37, 99, 235)
            };
            lnkQRCodeUrl.Visible = false;
            lnkQRCodeUrl.Click += (s, e) =>
            {
                if (lnkQRCodeUrl.Tag is string link && !string.IsNullOrEmpty(link))
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(link) { UseShellExecute = true }); }
                    catch { }
                }
            };
            panelRight.Controls.Add(lnkQRCodeUrl);

            // Status
            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 620),
                AutoSize = true,
                ForeColor = Color.FromArgb(231, 76, 60)
            };
            panelRight.Controls.Add(lblStatus);

            // Buttons
            btnRefreshQR = new Button
            {
                Text = "🔄 Làm mới QR",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 630),
                Size = new Size(140, 45),
                BackColor = Color.FromArgb(156, 163, 175),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefreshQR.FlatAppearance.BorderSize = 0;
            btnRefreshQR.Click += BtnRefreshQR_Click;
            panelRight.Controls.Add(btnRefreshQR);
            // Show layout debug info when form is shown to ensure PictureBox isn't covered
            this.Shown += PaymentForm_Shown;

            btnConfirmPayment = new Button
            {
                Text = "✅ Đã thanh toán",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(170, 630),
                Size = new Size(160, 45),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConfirmPayment.FlatAppearance.BorderSize = 0;
            btnConfirmPayment.Click += BtnConfirmPayment_Click;
            panelRight.Controls.Add(btnConfirmPayment);

            btnClose = new Button
            {
                Text = "Đóng",
                Font = new Font("Segoe UI", 10),
                Location = new Point(340, 630),
                Size = new Size(100, 45),
                BackColor = Color.FromArgb(189, 195, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnClose.FlatAppearance.BorderSize = 0;
            this.Controls.Add(panelRight);
            panelRight.Controls.Add(btnClose);

            this.CancelButton = btnClose;
        }

        private void LoadCurrentSemester()
        {
            try
            {
                // Lấy học kỳ hiện tại (có thể tùy chỉnh logic)
                DateTime now = DateTime.Now;

                if (now.Month >= 9)
                {
                    academicYear = now.Year;
                    semester = $"HK1 {academicYear}-{academicYear + 1}";
                }
                else if (now.Month >= 1 && now.Month <= 5)
                {
                    academicYear = now.Year - 1;
                    semester = $"HK2 {academicYear}-{academicYear + 1}";
                }
                else
                {
                    academicYear = now.Year - 1;
                    semester = $"HK3 {academicYear}-{academicYear + 1}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xác định học kỳ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUnpaidCourses()
        {
            try
            {
                int studentId = SessionManager.CurrentStudent.StudentId;

                string query = @"
                    SELECT
                        c.CourseCode AS [Mã môn],
                        c.CourseName AS [Tên môn học],
                        c.Credits AS [Tín chỉ],
                        CAST((SELECT SettingValue FROM PaymentSettings WHERE SettingKey = 'TUITION_PER_CREDIT') AS DECIMAL(18,2)) AS [Học phí/TC],
                        c.Credits * CAST((SELECT SettingValue FROM PaymentSettings WHERE SettingKey = 'TUITION_PER_CREDIT') AS DECIMAL(18,2)) AS [Thành tiền]
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.CourseId = c.CourseId
                    WHERE e.StudentId = @StudentId
                    AND c.Semester = @Semester
                    AND e.Status = N'Enrolled'
                    AND e.PaymentStatus = @PaymentStatus";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, new SqlParameter[]
                {
                    new SqlParameter("@StudentId", studentId),
                    new SqlParameter("@Semester", semester),
                    new SqlParameter("@PaymentStatus", StudentManagement.Helpers.AppConstants.PAYMENT_STATUS_UNPAID)
                });

                dgvCourses.DataSource = dt;

                // Calculate total
                totalAmount = 0;
                foreach (DataRow row in dt.Rows)
                {
                    totalAmount += Convert.ToDecimal(row["Thành tiền"]);
                }

                lblTotalAmount.Text = $"TỔNG TIỀN: {totalAmount:N0} VNĐ";

                // Update payment info for QR card
                lblDescription.Text = $"Số tiền: {totalAmount:N0} VNĐ\n" +
                                     $"Mã SV: {SessionManager.CurrentStudent.StudentCode}\n" +
                                     $"Nội dung CK: Hoc phi {SessionManager.CurrentStudent.StudentCode} {semester}";

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có môn học nào cần thanh toán hoặc đã thanh toán đầy đủ!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    return;
                }

                // Create or get payment
                CreatePayment();

                // Load QR Code
                LoadQRCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách môn học: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreatePayment()
        {
            try
            {
                int studentId = SessionManager.CurrentStudent.StudentId;

                // Call sp_CreatePayment with only StudentId and Semester (procedure extracts AcademicYear from Semester)
                SqlParameter outputParam = new SqlParameter("@PaymentId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                int spResult = DatabaseHelper.ExecuteNonQueryStoredProcedure("sp_CreatePayment", new SqlParameter[]
                {
                    new SqlParameter("@StudentId", studentId),
                    new SqlParameter("@Semester", semester),
                    outputParam
                });
                if (spResult != 0)
                {
                    MessageBox.Show("Lỗi khi tạo phiếu thanh toán. Mã lỗi: " + spResult, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                currentPaymentId = outputParam.Value != DBNull.Value ? Convert.ToInt32(outputParam.Value) : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo phiếu thanh toán: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SafeUpdateUI(Action action)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(action);
                }
                catch (ObjectDisposedException)
                {
                    // Form disposed while invoking
                }
            }
            else
            {
                action();
            }
        }

        private async void LoadQRCode()
        {
            try
            {
                if (IsDisposed || Disposing) return;

                // Mask account number in debug logs
                string maskedAcc = accountNo.Length > 4 ? accountNo.Substring(0, 2) + new string('*', accountNo.Length - 4) + accountNo.Substring(accountNo.Length - 2) : "****";
                System.Diagnostics.Debug.WriteLine($"Loading QR: amount={totalAmount}, bank={bankId}, acc={maskedAcc}, template={template}");

                SafeUpdateUI(() => {
                    lblStatus.Text = "Đang tải mã QR...";
                    lblStatus.ForeColor = Color.FromArgb(245, 158, 11);
                });

                if (totalAmount <= 0)
                {
                    SafeUpdateUI(() => {
                        lblStatus.Text = "Không có số tiền để thanh toán";
                        lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                    });
                    return;
                }

                // Đảm bảo sử dụng TLS 1.2 (VietQR bắt buộc https mới)
                // Enforce TLS 1.2 only
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // amount: bỏ phần thập phân
                string amount = ((long)totalAmount).ToString("0");

                // Nội dung chuyển khoản: nên là tiếng Anh/không dấu cho chắc
                string addInfo = $"Hoc phi {SessionManager.CurrentStudent.StudentCode} {semester}";
                string addInfoEncoded = Uri.EscapeDataString(addInfo);

                // accountName cũng phải URL-encode, không chỉ replace space
                string accountNameEncoded = Uri.EscapeDataString(accountName);

                // Build VietQR URL
                string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={amount}&addInfo={addInfoEncoded}&accountName={accountNameEncoded}";
                // Set link label for debugging or manual opening
                try
                {
                    lnkQRCodeUrl.Tag = qrUrl;
                    lnkQRCodeUrl.Text = "Mở QR trên trình duyệt";
                    lnkQRCodeUrl.Visible = true;
                }
                catch { /* Ignore UI errors */ }

                btnRefreshQR.Enabled = false;
                picQRCode.Image = null; // clear previous
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("image/*"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT; Windows NT 10.0; Win64; x64)");
                    using (var response = await client.GetAsync(qrUrl))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            string err = $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}";
                            lblStatus.Text = $"❌ Không thể lấy QR: {err}";
                            lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                            var content = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine("QR load failed: " + err + "\n" + content);
                            MessageBox.Show("Không thể tải mã QR. Mã lỗi: " + err + "\nNếu cần, kiểm tra URL sau:\n" + qrUrl, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Check content type and length
                        var contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
                        var contentLength = response.Content.Headers.ContentLength ?? -1;
                        try
                        {
                            lblStatus.Text = $"⏳ Tải QR: {contentType}, {contentLength} bytes";
                            lblStatus.ForeColor = Color.FromArgb(245, 158, 11);
                        }
                        catch { }
                        if (!contentType.Contains("image") || (contentLength > 0 && contentLength < 100))
                        {
                            string details = $"ContentType={contentType}, Length={contentLength}";
                            string content = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine("QR content invalid: " + details + "\n" + content);
                            lblStatus.Text = "❌ Nội dung QR không hợp lệ"
                                + (details.Length > 0 ? ": " + details : string.Empty);
                            lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                            MessageBox.Show("Dữ liệu trả về không phải ảnh. Kiểm tra cấu hình VietQR hoặc kết nối:\n" + qrUrl,
                                "Lỗi tải QR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                        // Giải phóng ảnh cũ trước khi gán ảnh mới
                        if (picQRCode.Image != null)
                        {
                            var old = picQRCode.Image;
                            picQRCode.Image = null;
                            old.Dispose();
                        }

                        // Tạo ảnh từ stream, copy sang Bitmap mới để tránh lỗi dispose
                        bool imageSet = false;
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            {
                                using (Image tempImage = Image.FromStream(ms))
                                {
                                    // Copy into new Bitmap to ensure we don't rely on the stream
                                    picQRCode.Image = new Bitmap(tempImage);
                                    imageSet = true;
                                }
                            }
                        }
                        catch (Exception imgEx)
                        {
                            System.Diagnostics.Debug.WriteLine("Image.FromStream failed: " + imgEx.Message);
                        }

                        // If FromStream fails, save to a temporary file and load via Image.FromFile
                        if (!imageSet)
                        {
                            try
                            {
                                string tempFile = Path.Combine(Path.GetTempPath(), $"vietqr_{Guid.NewGuid()}.png");
                                File.WriteAllBytes(tempFile, imageBytes);
                                using (Image tempImage = Image.FromFile(tempFile))
                                {
                                    picQRCode.Image = new Bitmap(tempImage);
                                    imageSet = true;
                                }

                                // Attempt to delete the temp file (Image.FromFile may lock file until disposed)
                                try { File.Delete(tempFile); } catch { }
                            }
                            catch (Exception fileEx)
                            {
                                System.Diagnostics.Debug.WriteLine("Fallback file load failed: " + fileEx.Message);
                            }
                        }

                        if (!imageSet)
                        {
                            // Final fallback: instruct the PictureBox to load directly from the URL asynchronously
                            try
                            {
                                picQRCode.LoadAsync(qrUrl);
                                lblStatus.Text = "⏳ Tải QR (fallback)";
                                lblStatus.ForeColor = Color.FromArgb(245, 158, 11);
                                // Do not return; let the async loader update image when done
                            }
                            catch (Exception loadAsyncEx)
                            {
                                System.Diagnostics.Debug.WriteLine("LoadAsync fallback failed: " + loadAsyncEx.Message);
                                lblStatus.Text = "❌ Lỗi hiển thị ảnh QR";
                                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                                MessageBox.Show("Không thể hiển thị ảnh QR sau nhiều cố gắng. Hãy kiểm tra URL hoặc thử mở link trong trình duyệt:", "Lỗi ảnh QR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        // Bring to front and refresh UI
                        try
                        {
                            picQRCode.Visible = true;
                            picQRCode.BringToFront();
                            picQRCode.Refresh();
                            System.Diagnostics.Debug.WriteLine($"QR image set: {picQRCode.Image?.Width}x{picQRCode.Image?.Height}");
                        }
                        catch (Exception uiEx)
                        {
                            System.Diagnostics.Debug.WriteLine("UI refresh failed: " + uiEx.Message);
                        }
                    }
                }

                lblStatus.Text = "✅ Mã QR đã sẵn sàng";
                lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
                btnRefreshQR.Enabled = true;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Không thể tải mã QR";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                MessageBox.Show(
                    "Không thể tải mã QR.\n" +
                    "Vui lòng kiểm tra:\n" +
                    " - Kết nối Internet\n" +
                    " - Cấu hình VietQR (VIETQR_BANK_ID, VIETQR_ACCOUNT_NO, ...)\n\n" +
                    "Chi tiết lỗi: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                btnRefreshQR.Enabled = true;
            }
        }

        private void BtnRefreshQR_Click(object sender, EventArgs e)
        {
            LoadQRCode();
        }

        private void PaymentForm_Shown(object sender, EventArgs e)
        {
            try
            {
                // Force right panel and picture to front to avoid being covered
                panelLeft.SendToBack();
                panelRight.BringToFront();
                picQRCode.BringToFront();
                string info = $"Form: {this.ClientSize.Width}x{this.ClientSize.Height} | Left:{panelLeft.Bounds} | Right:{panelRight.Bounds} | Pic:{picQRCode.Bounds}";
                System.Diagnostics.Debug.WriteLine("LayoutInfo: " + info);
                lblStatus.Text = info;
                lblStatus.ForeColor = Color.FromArgb(52, 73, 94);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PaymentForm_Shown error: " + ex.Message);
            }
        }

        private void PicQRCode_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                lblStatus.Text = "❌ Lỗi tải QR (async)";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                System.Diagnostics.Debug.WriteLine("PictureBox.LoadCompleted Error: " + e.Error.Message);
            }
            else
            {
                lblStatus.Text = "✅ Mã QR đã sẵn sàng";
                lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        private void BtnConfirmPayment_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn đã hoàn tất việc chuyển khoản chưa?\n\n" +
                "Vui lòng chỉ bấm 'Có' sau khi đã chuyển khoản thành công.",
                "Xác nhận thanh toán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int spResult = DatabaseHelper.ExecuteNonQueryStoredProcedure("sp_ConfirmPayment", new SqlParameter[]
                    {
                        new SqlParameter("@PaymentId", currentPaymentId),
                        new SqlParameter("@TransactionId", DBNull.Value)
                    });

                    if (spResult == 0)
                    {
                        MessageBox.Show("Thanh toán thành công!\n\nBạn có thể xem thời khóa biểu của mình ngay bây giờ.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Xác nhận thanh toán thất bại (mã lỗi: " + spResult + ").", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xác nhận thanh toán: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
