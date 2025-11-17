# Hướng dẫn Khắc phục Lỗi - Troubleshooting Guide

## Các lỗi đã sửa

### ✅ Lỗi "Index was out of range"

**Triệu chứng:**
```
Lỗi khi tải dữ liệu: Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')
```

**Nguyên nhân:**
Khi DataGridView không có dữ liệu hoặc số cột ít hơn dự kiến, việc truy cập vào `Columns[index]` sẽ gây lỗi.

**Giải pháp đã áp dụng:**
Thêm kiểm tra số lượng columns trước khi truy cập:

```csharp
// TRƯỚC (Lỗi)
dgv.DataSource = DatabaseHelper.ExecuteQuery(query);
dgv.Columns[0].HeaderText = "ID";  // ❌ Lỗi nếu không có dữ liệu

// SAU (Đã sửa)
dgv.DataSource = DatabaseHelper.ExecuteQuery(query);
if (dgv.Columns.Count >= 8)  // ✅ Kiểm tra trước
{
    dgv.Columns[0].HeaderText = "ID";
    // ...
}
```

**Các file đã được sửa:**
- `Forms/AdminDashboard.cs` - LoadStudentsData(), LoadTeacherManagement(), LoadCourseManagement()
- `Forms/TeacherDashboard.cs` - LoadDashboard(), LoadStudentsByCourse()
- `Forms/StudentDashboard.cs` - LoadDashboard(), LoadMyCourses(), LoadMyGrades(), LoadAvailableCourses()

## Hướng dẫn Kiểm tra và Test

### 1. Kiểm tra Database đã được tạo

```sql
-- Mở SQL Server Management Studio hoặc dùng command line
USE StudentManagementDB;

-- Kiểm tra có dữ liệu mẫu chưa
SELECT COUNT(*) FROM Users;      -- Phải = 3
SELECT COUNT(*) FROM Students;   -- Phải = 1
SELECT COUNT(*) FROM Teachers;   -- Phải = 1
SELECT COUNT(*) FROM Courses;    -- Phải = 4
```

### 2. Test Connection String

```csharp
// Trong LoginForm, khi click Đăng nhập sẽ test kết nối
if (!DatabaseHelper.TestConnection())
{
    MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!");
}
```

### 3. Test Đăng nhập

**Admin:**
- Username: `admin`
- Password: `admin123`
- Sau khi đăng nhập nên thấy AdminDashboard với 4 stat cards

**Teacher:**
- Username: `teacher01`
- Password: `teacher123`
- Sau khi đăng nhập nên thấy TeacherDashboard với thông tin giảng viên

**Student:**
- Username: `student01`
- Password: `student123`
- Sau khi đăng nhập nên thấy StudentDashboard với thông tin sinh viên

### 4. Test các chức năng

#### Test Admin Dashboard:
```
1. Đăng nhập admin
2. Click "Quản lý Sinh viên" → Nên hiện danh sách sinh viên
3. Click "Quản lý Giảng viên" → Nên hiện danh sách giảng viên
4. Click "Quản lý Môn học" → Nên hiện danh sách môn học (4 môn)
```

#### Test Teacher Dashboard:
```
1. Đăng nhập teacher01
2. Xem "Môn học đang dạy" → Nên có 4 môn học
3. Click "Danh sách Sinh viên" → Chọn môn → Xem sinh viên
```

#### Test Student Dashboard:
```
1. Đăng nhập student01
2. Xem "Môn học đã đăng ký" → Danh sách môn đã đăng ký
3. Click "Xem điểm" → Xem điểm của các môn
4. Click "Đăng ký môn học" → Chọn học kỳ → Đăng ký môn mới
```

## Lỗi thường gặp khác

### ❌ Lỗi "Cannot connect to database"

**Kiểm tra:**
1. SQL Server có đang chạy không?
   ```powershell
   Get-Service MSSQL* | Where-Object {$_.Status -eq 'Running'}
   ```

2. Connection string có đúng không?
   - Mở `App.config`
   - Kiểm tra `Data Source` phù hợp với SQL Server instance của bạn

3. Database đã được tạo chưa?
   ```bash
   sqlcmd -S . -Q "SELECT name FROM sys.databases WHERE name = 'StudentManagementDB'"
   ```

**Giải pháp:**
```xml
<!-- SQL Server local -->
<add name="StudentManagementDB"
     connectionString="Data Source=.;Initial Catalog=StudentManagementDB;Integrated Security=True;TrustServerCertificate=True"/>

<!-- SQL Express -->
<add name="StudentManagementDB"
     connectionString="Data Source=localhost\SQLEXPRESS;Initial Catalog=StudentManagementDB;Integrated Security=True;TrustServerCertificate=True"/>

<!-- SQL Authentication -->
<add name="StudentManagementDB"
     connectionString="Data Source=.;Initial Catalog=StudentManagementDB;User Id=sa;Password=your_password;TrustServerCertificate=True"/>
```

### ❌ Lỗi "Login failed"

**Nguyên nhân:**
- Sai username/password
- Database chưa có user mẫu
- Chưa chạy script CreateDatabase.sql

**Giải pháp:**
1. Chạy lại script `Database/CreateDatabase.sql`
2. Kiểm tra user có tồn tại:
   ```sql
   SELECT * FROM Users WHERE Username = 'admin'
   ```
3. Reset password nếu cần:
   ```sql
   UPDATE Users SET Password = 'admin123' WHERE Username = 'admin'
   ```

### ❌ Lỗi "Object reference not set to an instance"

**Nguyên nhân:**
- SessionManager.CurrentUser hoặc CurrentStudent/CurrentTeacher = null
- Chưa đăng nhập hoặc session bị mất

**Giải pháp:**
Đảm bảo đăng nhập thành công trước khi mở dashboard. Check trong Program.cs:

```csharp
if (loginResult == DialogResult.OK && SessionManager.IsLoggedIn)
{
    // Mở dashboard
}
```

### ❌ DataGridView không hiển thị dữ liệu

**Kiểm tra:**
1. Query có đúng không?
2. Database có dữ liệu không?
3. Có lỗi exception nào không?

**Debug:**
```csharp
try
{
    DataTable dt = DatabaseHelper.ExecuteQuery(query);
    MessageBox.Show($"Số dòng: {dt.Rows.Count}, Số cột: {dt.Columns.Count}");
    dgv.DataSource = dt;
}
catch (Exception ex)
{
    MessageBox.Show("Lỗi: " + ex.Message);
}
```

### ❌ Build Failed

**Lỗi common:**
1. **CS0246**: The type or namespace name could not be found
   - **Giải pháp**: `dotnet restore`

2. **CS0103**: The name does not exist in the current context
   - **Giải pháp**: Check using statements và namespace

3. **CS1061**: Does not contain a definition
   - **Giải pháp**: Check spelling và references

**Các lệnh hữu ích:**
```bash
# Clean build
dotnet clean

# Restore packages
dotnet restore

# Rebuild
dotnet build --no-incremental

# Run
dotnet run
```

## Tips Debug

### 1. Sử dụng Try-Catch

Luôn wrap database operations trong try-catch:

```csharp
try
{
    // Database operation
    dgv.DataSource = DatabaseHelper.ExecuteQuery(query);
}
catch (Exception ex)
{
    MessageBox.Show($"Lỗi: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

### 2. Log thông tin debug

```csharp
// Thêm vào DatabaseHelper.cs để log queries
System.Diagnostics.Debug.WriteLine($"Executing query: {query}");
```

### 3. Kiểm tra dữ liệu

```csharp
DataTable dt = DatabaseHelper.ExecuteQuery(query);
foreach (DataRow row in dt.Rows)
{
    Debug.WriteLine(string.Join(", ", row.ItemArray));
}
```

## Performance Tips

### 1. Sử dụng AutoSizeColumnsMode

```csharp
dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
```

### 2. Disable AddNew Row

```csharp
dgv.AllowUserToAddRows = false;
```

### 3. Set ReadOnly

```csharp
dgv.ReadOnly = true;
```

## Best Practices đã áp dụng

✅ **Error Handling**: Tất cả database operations đều có try-catch
✅ **Null Checking**: Kiểm tra Count trước khi truy cập Columns
✅ **SQL Injection Prevention**: Sử dụng SqlParameter thay vì string concatenation
✅ **Separation of Concerns**: DatabaseHelper riêng biệt cho data access
✅ **Session Management**: SessionManager quản lý user state
✅ **Clean Code**: Code dễ đọc, có comments phù hợp

## Liên hệ

Nếu gặp lỗi khác không có trong tài liệu này:
1. Check error message và stack trace
2. Search trong README.md và QUICKSTART.md
3. Tạo issue trên GitHub với thông tin chi tiết về lỗi
