# Hướng dẫn Chạy Nhanh - Quick Start Guide

## Bước 1: Cài đặt SQL Server

### Windows:
1. Tải SQL Server Express (miễn phí): https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. Chạy file cài đặt và chọn "Basic" installation
3. Hoàn tất cài đặt

### Kiểm tra SQL Server đang chạy:
```bash
# Mở Services (services.msc) và tìm "SQL Server"
# Hoặc dùng PowerShell:
Get-Service MSSQL*
```

## Bước 2: Tạo Database

### Cách 1: Dùng SSMS (SQL Server Management Studio)
1. Mở SSMS và kết nối tới SQL Server
2. Mở file `Database/CreateDatabase.sql`
3. Nhấn F5 để chạy script

### Cách 2: Dùng Command Line
```bash
sqlcmd -S . -i "Database\CreateDatabase.sql"
```

### Cách 3: Dùng PowerShell
```powershell
Invoke-Sqlcmd -ServerInstance "." -InputFile "Database\CreateDatabase.sql"
```

## Bước 3: Cấu hình Connection String

Mở file `App.config` và kiểm tra connection string:

```xml
<add name="StudentManagementDB"
     connectionString="Data Source=.;Initial Catalog=StudentManagementDB;Integrated Security=True;TrustServerCertificate=True"
     providerName="System.Data.SqlClient"/>
```

**Nếu SQL Server của bạn khác:**
- SQL Express: `Data Source=localhost\SQLEXPRESS`
- Named Instance: `Data Source=.\INSTANCENAME`

## Bước 4: Build và Chạy

### Trong Visual Studio:
1. Mở `StudentManagement.sln`
2. Nhấn F5 để chạy

### Dùng Command Line:
```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Chạy ứng dụng
dotnet run
```

## Bước 5: Đăng nhập

Sử dụng một trong các tài khoản mẫu:

### Tài khoản Admin
```
Username: admin
Password: admin123
```

### Tài khoản Giảng viên
```
Username: teacher01
Password: teacher123
```

### Tài khoản Sinh viên
```
Username: student01
Password: student123
```

## Xử lý Lỗi Thường Gặp

### ❌ "Cannot connect to database"
**Nguyên nhân:** SQL Server không chạy hoặc connection string sai

**Giải pháp:**
1. Kiểm tra SQL Server đang chạy:
   ```powershell
   Get-Service MSSQL* | Where-Object {$_.Status -eq 'Running'}
   ```

2. Test kết nối:
   ```bash
   sqlcmd -S . -Q "SELECT @@VERSION"
   ```

3. Nếu dùng SQL Express, sửa connection string:
   ```
   Data Source=localhost\SQLEXPRESS
   ```

### ❌ "Login failed for user"
**Nguyên nhân:** Xác thực SQL Server không đúng

**Giải pháp:**
- Dùng Windows Authentication (Integrated Security=True)
- Hoặc dùng SQL Authentication với username/password

### ❌ Database không tồn tại
**Nguyên nhân:** Chưa chạy script tạo database

**Giải pháp:**
```bash
sqlcmd -S . -Q "SELECT name FROM sys.databases WHERE name = 'StudentManagementDB'"
```
Nếu không có kết quả, chạy lại script CreateDatabase.sql

### ❌ Build Failed
**Nguyên nhân:** Thiếu NuGet packages

**Giải pháp:**
```bash
dotnet restore
dotnet clean
dotnet build
```

## Kiểm tra Cài đặt

### 1. Kiểm tra .NET SDK
```bash
dotnet --version
# Cần: .NET 6.0 hoặc cao hơn
```

### 2. Kiểm tra SQL Server
```bash
sqlcmd -S . -Q "SELECT @@VERSION"
```

### 3. Kiểm tra Database
```bash
sqlcmd -S . -d StudentManagementDB -Q "SELECT COUNT(*) FROM Users"
# Kết quả: 3 (3 user mẫu)
```

### 4. Test kết nối từ C#
```bash
dotnet build
dotnet run
# Nếu form đăng nhập hiện lên = thành công!
```

## Thử Nghiệm Nhanh

### Test với Admin:
1. Đăng nhập: `admin / admin123`
2. Xem tổng quan hệ thống
3. Vào "Quản lý Sinh viên" để xem danh sách
4. Vào "Quản lý Môn học" để xem khóa học

### Test với Giảng viên:
1. Đăng nhập: `teacher01 / teacher123`
2. Xem môn học đang dạy
3. Xem danh sách sinh viên trong môn học
4. Test chức năng nhập điểm

### Test với Sinh viên:
1. Đăng nhập: `student01 / student123`
2. Xem thông tin cá nhân và GPA
3. Xem môn học đã đăng ký
4. Thử đăng ký môn học mới
5. Xem điểm số

## Cấu trúc Thư mục

```
QuanLySinhVien/
├── Models/              # Data models
├── Forms/               # UI Forms
├── Data/                # Database helper
├── Helpers/             # Utility classes
├── Database/            # SQL scripts
│   └── CreateDatabase.sql
├── Program.cs
├── App.config
└── StudentManagement.csproj
```

## Các File Quan Trọng

| File | Mô tả |
|------|-------|
| `Program.cs` | Entry point của ứng dụng |
| `App.config` | Cấu hình connection string |
| `Database/CreateDatabase.sql` | Script tạo database |
| `Data/DatabaseHelper.cs` | Helper kết nối database |
| `Forms/LoginForm.cs` | Form đăng nhập |
| `Forms/AdminDashboard.cs` | Dashboard admin |
| `Forms/TeacherDashboard.cs` | Dashboard giảng viên |
| `Forms/StudentDashboard.cs` | Dashboard sinh viên |

## Mẹo Sử dụng

1. **Backup Database trước khi test:**
   ```sql
   BACKUP DATABASE StudentManagementDB TO DISK = 'C:\Backup\StudentManagementDB.bak'
   ```

2. **Reset Database về trạng thái ban đầu:**
   - Chạy lại file `CreateDatabase.sql`

3. **Thêm dữ liệu mẫu:**
   - Script đã có sẵn 1 admin, 1 giảng viên, 1 sinh viên
   - Có thể thêm bằng Admin dashboard

4. **Debug trong Visual Studio:**
   - Đặt breakpoint tại `Program.cs` line 20
   - F5 để debug mode
   - F10/F11 để step through code

## Liên hệ

Nếu gặp vấn đề, hãy check:
1. README.md - Hướng dẫn chi tiết
2. GitHub Issues - Báo lỗi và hỏi đáp
3. Code comments - Giải thích trong source code

---

**Chúc bạn thành công! 🎉**
