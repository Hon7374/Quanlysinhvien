using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using StudentManagement.Data;

namespace StudentManagement.Forms
{
    public partial class ReportsForm : Form
    {
        private Panel panelHeader;
        private Panel panelStats;
        private Panel panelCharts;

        public ReportsForm()
        {
            InitializeComponent();
            LoadStatistics();
            LoadCharts();
        }

        private void InitializeComponent()
        {
            this.Text = "Thống kê và Báo cáo - Statistics & Reports";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.AutoScroll = true;

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            Label lblTitle = new Label
            {
                Text = "Thống kê và Báo cáo",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(30, 30),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            panelHeader.Controls.Add(lblTitle);

            // Stats Panel
            panelStats = new Panel
            {
                Location = new Point(30, 120),
                Size = new Size(1320, 200),
                BackColor = Color.Transparent
            };

            // Charts Panel
            panelCharts = new Panel
            {
                Location = new Point(30, 340),
                Size = new Size(1320, 500),
                BackColor = Color.Transparent
            };

            this.Controls.Add(panelCharts);
            this.Controls.Add(panelStats);
            this.Controls.Add(panelHeader);
        }

        private void LoadStatistics()
        {
            try
            {
                // Get statistics
                int totalStudents = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Students") ?? 0);
                int totalTeachers = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Teachers") ?? 0);
                int totalCourses = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Courses") ?? 0);
                int activeCourses = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Courses WHERE IsActive = 1") ?? 0);

                // Create stat cards
                CreateStatCard("Tổng sinh viên", totalStudents.ToString(), "👨‍🎓", Color.FromArgb(99, 102, 241), 0);
                CreateStatCard("Tổng giảng viên", totalTeachers.ToString(), "👨‍🏫", Color.FromArgb(16, 185, 129), 330);
                CreateStatCard("Tổng môn học", totalCourses.ToString(), "📚", Color.FromArgb(251, 191, 36), 660);
                CreateStatCard("Môn đang mở", activeCourses.ToString(), "📖", Color.FromArgb(239, 68, 68), 990);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thống kê: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateStatCard(string title, string value, string icon, Color color, int x)
        {
            Panel card = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(310, 180),
                BackColor = Color.White
            };

            // Icon circle
            Panel iconPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(60, 60),
                BackColor = Color.FromArgb(20, color.R, color.G, color.B)
            };

            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24),
                Location = new Point(12, 8),
                AutoSize = true
            };
            iconPanel.Controls.Add(lblIcon);
            card.Controls.Add(iconPanel);

            // Title
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 100),
                AutoSize = true,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            card.Controls.Add(lblTitle);

            // Value
            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                Location = new Point(20, 120),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            card.Controls.Add(lblValue);

            panelStats.Controls.Add(card);
        }

        private void LoadCharts()
        {
            try
            {
                // Student by Status Chart
                CreateBarChart("Sinh viên theo trạng thái",
                    "SELECT Status, COUNT(*) as Count FROM Students GROUP BY Status", 0, 0, 640, 450);

                // Teachers by Department Chart
                CreateBarChart("Giảng viên theo khoa",
                    "SELECT TOP 5 Department, COUNT(*) as Count FROM Teachers WHERE Department IS NOT NULL GROUP BY Department ORDER BY Count DESC", 680, 0, 640, 450);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải biểu đồ: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateBarChart(string title, string query, int x, int y, int width, int height)
        {
            Panel chartPanel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Title
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            chartPanel.Controls.Add(lblTitle);

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                if (dt.Rows.Count > 0)
                {
                    int maxValue = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        int val = Convert.ToInt32(row["Count"]);
                        if (val > maxValue) maxValue = val;
                    }

                    int barHeight = 40;
                    int spacing = 20;
                    int chartY = 70;
                    int chartWidth = width - 60;

                    Color[] colors = new Color[] {
                        Color.FromArgb(99, 102, 241),
                        Color.FromArgb(16, 185, 129),
                        Color.FromArgb(251, 191, 36),
                        Color.FromArgb(239, 68, 68),
                        Color.FromArgb(139, 92, 246)
                    };

                    int colorIndex = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        string category = row[0].ToString();
                        int count = Convert.ToInt32(row["Count"]);

                        // Category label
                        Label lblCategory = new Label
                        {
                            Text = category,
                            Font = new Font("Segoe UI", 9),
                            Location = new Point(20, chartY + 10),
                            Size = new Size(150, 20),
                            ForeColor = Color.FromArgb(107, 114, 128)
                        };
                        chartPanel.Controls.Add(lblCategory);

                        // Bar
                        int barWidth = maxValue > 0 ? (int)((double)count / maxValue * (chartWidth - 200)) : 0;
                        Panel bar = new Panel
                        {
                            Location = new Point(180, chartY),
                            Size = new Size(barWidth, barHeight),
                            BackColor = colors[colorIndex % colors.Length]
                        };
                        chartPanel.Controls.Add(bar);

                        // Count label
                        Label lblCount = new Label
                        {
                            Text = count.ToString(),
                            Font = new Font("Segoe UI", 10, FontStyle.Bold),
                            Location = new Point(190 + barWidth, chartY + 10),
                            AutoSize = true,
                            ForeColor = Color.FromArgb(31, 41, 55)
                        };
                        chartPanel.Controls.Add(lblCount);

                        chartY += barHeight + spacing;
                        colorIndex++;
                    }
                }
                else
                {
                    Label lblNoData = new Label
                    {
                        Text = "Không có dữ liệu",
                        Font = new Font("Segoe UI", 11),
                        Location = new Point(20, 70),
                        AutoSize = true,
                        ForeColor = Color.FromArgb(156, 163, 175)
                    };
                    chartPanel.Controls.Add(lblNoData);
                }
            }
            catch (Exception ex)
            {
                Label lblError = new Label
                {
                    Text = $"Lỗi: {ex.Message}",
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(20, 70),
                    Size = new Size(width - 40, 50),
                    ForeColor = Color.Red
                };
                chartPanel.Controls.Add(lblError);
            }

            panelCharts.Controls.Add(chartPanel);
        }
    }
}
