-- =============================================
-- Student Management System - Sample Data (Fixed)
-- =============================================

USE StudentManagementDB;
GO

-- Clear existing data (in correct order to respect foreign keys)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Grades')
    DELETE FROM Grades;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
    DELETE FROM Enrollments;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
    DELETE FROM Courses;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
    DELETE FROM Students;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
    DELETE FROM Teachers;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    DELETE FROM Users;
GO

-- =============================================
-- 1. USERS (20 records)
-- =============================================

SET IDENTITY_INSERT Users ON;

-- Admin (2)
INSERT INTO Users (UserId, Username, Password, FullName, Email, Phone, Role, IsActive, CreatedDate, LastLogin)
VALUES
(1, 'admin', 'admin123', N'Quản trị viên', 'admin@university.edu.vn', '0123456789', 1, 1, GETDATE(), GETDATE()),
(2, 'admin2', 'admin123', N'Nguyễn Văn Quản', 'admin2@university.edu.vn', '0987654321', 1, 1, GETDATE(), NULL);

-- Teachers (8)
INSERT INTO Users (UserId, Username, Password, FullName, Email, Phone, Role, IsActive, CreatedDate, LastLogin)
VALUES
(3, 'teacher01', 'teacher123', N'Nguyễn Văn Giảng', 'nvgiang@university.edu.vn', '0901234567', 2, 1, GETDATE(), GETDATE()),
(4, 'teacher02', 'teacher123', N'Trần Thị Minh', 'ttminh@university.edu.vn', '0912345678', 2, 1, GETDATE(), NULL),
(5, 'teacher03', 'teacher123', N'Lê Hoàng Nam', 'lhnam@university.edu.vn', '0923456789', 2, 1, GETDATE(), NULL),
(6, 'teacher04', 'teacher123', N'Phạm Thu Hà', 'ptha@university.edu.vn', '0934567890', 2, 1, GETDATE(), NULL),
(7, 'teacher05', 'teacher123', N'Vũ Đức Thắng', 'vdthang@university.edu.vn', '0945678901', 2, 1, GETDATE(), NULL),
(8, 'teacher06', 'teacher123', N'Hoàng Thị Lan', 'htlan@university.edu.vn', '0956789012', 2, 1, GETDATE(), NULL),
(9, 'teacher07', 'teacher123', N'Đặng Minh Tuấn', 'dmtuan@university.edu.vn', '0967890123', 2, 1, GETDATE(), NULL),
(10, 'teacher08', 'teacher123', N'Bùi Thị Hương', 'bthuong@university.edu.vn', '0978901234', 2, 1, GETDATE(), NULL);

-- Students (10)
INSERT INTO Users (UserId, Username, Password, FullName, Email, Phone, Role, IsActive, CreatedDate, LastLogin)
VALUES
(11, 'student01', 'student123', N'Nguyễn Văn An', 'student01@stu.edu.vn', '0981234567', 3, 1, GETDATE(), GETDATE()),
(12, 'student02', 'student123', N'Trần Thị Bình', 'student02@stu.edu.vn', '0982345678', 3, 1, GETDATE(), NULL),
(13, 'student03', 'student123', N'Lê Văn Cường', 'student03@stu.edu.vn', '0983456789', 3, 1, GETDATE(), NULL),
(14, 'student04', 'student123', N'Phạm Thị Dung', 'student04@stu.edu.vn', '0984567890', 3, 1, GETDATE(), NULL),
(15, 'student05', 'student123', N'Hoàng Văn Em', 'student05@stu.edu.vn', '0985678901', 3, 1, GETDATE(), NULL),
(16, 'student06', 'student123', N'Vũ Thị Phương', 'student06@stu.edu.vn', '0986789012', 3, 1, GETDATE(), NULL),
(17, 'student07', 'student123', N'Đặng Văn Giang', 'student07@stu.edu.vn', '0987890123', 3, 1, GETDATE(), NULL),
(18, 'student08', 'student123', N'Bùi Thị Hoa', 'student08@stu.edu.vn', '0988901234', 3, 1, GETDATE(), NULL),
(19, 'student09', 'student123', N'Ngô Văn Hùng', 'student09@stu.edu.vn', '0989012345', 3, 1, GETDATE(), NULL),
(20, 'student10', 'student123', N'Đinh Thị Kim', 'student10@stu.edu.vn', '0990123456', 3, 1, GETDATE(), NULL);

SET IDENTITY_INSERT Users OFF;
GO

-- =============================================
-- 2. TEACHERS (8 records)
-- =============================================

SET IDENTITY_INSERT Teachers ON;

INSERT INTO Teachers (TeacherId, UserId, TeacherCode, Department, Degree, Specialization, HireDate)
VALUES
(1, 3, 'GV001', N'Khoa Công nghệ thông tin', N'Tiến sĩ', N'Trí tuệ nhân tạo', '2015-09-01'),
(2, 4, 'GV002', N'Khoa Công nghệ thông tin', N'Thạc sĩ', N'Phát triển phần mềm', '2018-01-15'),
(3, 5, 'GV003', N'Khoa Công nghệ thông tin', N'Tiến sĩ', N'Cơ sở dữ liệu', '2016-03-10'),
(4, 6, 'GV004', N'Khoa Công nghệ thông tin', N'Thạc sĩ', N'An toàn thông tin', '2019-07-20'),
(5, 7, 'GV005', N'Khoa Điện - Điện tử', N'Tiến sĩ', N'Điện tử viễn thông', '2014-08-15'),
(6, 8, 'GV006', N'Khoa Điện - Điện tử', N'Thạc sĩ', N'Tự động hóa', '2017-09-01'),
(7, 9, 'GV007', N'Khoa Cơ khí', N'Tiến sĩ', N'Cơ kỹ thuật', '2013-05-20'),
(8, 10, 'GV008', N'Khoa Kinh tế', N'Thạc sĩ', N'Quản trị kinh doanh', '2020-02-10');

SET IDENTITY_INSERT Teachers OFF;
GO

-- =============================================
-- 3. STUDENTS (10 records)
-- =============================================

SET IDENTITY_INSERT Students ON;

INSERT INTO Students (StudentId, UserId, StudentCode, DateOfBirth, Gender, Address, Class, Major, AcademicYear, GPA)
VALUES
(1, 11, 'SV2024001', '2002-05-15', N'Nam', N'123 Lê Lợi, Q1, TP.HCM', N'CNTT-K18A', N'Công nghệ thông tin', 2024, 3.45),
(2, 12, 'SV2024002', '2002-08-20', N'Nữ', N'456 Trần Hưng Đạo, Q5, TP.HCM', N'CNTT-K18A', N'Công nghệ thông tin', 2024, 3.67),
(3, 13, 'SV2024003', '2002-03-10', N'Nam', N'789 Nguyễn Huệ, Q1, TP.HCM', N'CNTT-K18B', N'Công nghệ thông tin', 2024, 3.21),
(4, 14, 'SV2024004', '2002-11-25', N'Nữ', N'321 Võ Văn Tần, Q3, TP.HCM', N'CNTT-K18B', N'Công nghệ thông tin', 2024, 3.89),
(5, 15, 'SV2024005', '2002-07-30', N'Nam', N'654 Điện Biên Phủ, Bình Thạnh, TP.HCM', N'DTVT-K18', N'Điện tử viễn thông', 2024, 3.12),
(6, 16, 'SV2024006', '2002-09-12', N'Nữ', N'987 Cách Mạng T8, Q10, TP.HCM', N'DTVT-K18', N'Điện tử viễn thông', 2024, 3.56),
(7, 17, 'SV2024007', '2002-01-18', N'Nam', N'147 Lý Thường Kiệt, Q11, TP.HCM', N'CK-K18', N'Cơ khí', 2024, 3.34),
(8, 18, 'SV2024008', '2002-04-22', N'Nữ', N'258 Phan Đăng Lưu, Phú Nhuận, TP.HCM', N'CK-K18', N'Cơ khí', 2024, 3.78),
(9, 19, 'SV2024009', '2002-06-08', N'Nam', N'369 Hoàng Văn Thụ, Tân Bình, TP.HCM', N'QTKD-K18', N'Quản trị kinh doanh', 2024, 3.23),
(10, 20, 'SV2024010', '2002-12-14', N'Nữ', N'741 Nguyễn TM Khai, Q3, TP.HCM', N'QTKD-K18', N'Quản trị kinh doanh', 2024, 3.91);

SET IDENTITY_INSERT Students OFF;
GO

-- =============================================
-- 4. COURSES (10 records)
-- =============================================

SET IDENTITY_INSERT Courses ON;

INSERT INTO Courses (CourseId, CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
VALUES
(1, 'IT101', N'Lập trình C/C++ cơ bản', 3, N'Môn học cơ sở về lập trình C/C++', 1, N'HK1-2024', 2024, 50, 1),
(2, 'IT202', N'Lập trình hướng đối tượng', 4, N'Lập trình Java, C#, design patterns', 2, N'HK2-2024', 2024, 45, 1),
(3, 'IT303', N'Cơ sở dữ liệu', 3, N'SQL, thiết kế CSDL, quản trị SQL Server', 3, N'HK1-2024', 2024, 50, 1),
(4, 'IT404', N'An toàn bảo mật', 3, N'Mã hóa, bảo mật mạng, ethical hacking', 4, N'HK1-2024', 2024, 40, 1),
(5, 'IT505', N'Trí tuệ nhân tạo', 4, N'Machine Learning, Deep Learning, AI', 1, N'HK2-2024', 2024, 35, 1),
(6, 'EE201', N'Mạch điện tử', 3, N'Thiết kế và phân tích mạch điện tử', 5, N'HK1-2024', 2024, 40, 1),
(7, 'EE302', N'Xử lý tín hiệu số', 4, N'DSP, FFT, lọc số', 6, N'HK1-2024', 2024, 35, 1),
(8, 'ME201', N'Cơ kỹ thuật', 3, N'Động lực học, tĩnh học, vật liệu', 7, N'HK1-2024', 2024, 45, 1),
(9, 'BUS101', N'Quản trị học', 3, N'Nguyên lý quản trị, tổ chức doanh nghiệp', 8, N'HK1-2024', 2024, 60, 1),
(10, 'IT199', N'Đồ án môn học', 2, N'Đồ án tổng hợp kiến thức', 2, N'HK1-2023', 2023, 30, 0);

SET IDENTITY_INSERT Courses OFF;
GO

-- =============================================
-- 5. ENROLLMENTS (43 records)
-- =============================================

SET IDENTITY_INSERT Enrollments ON;

-- Student 1-4 (CNTT): IT courses
INSERT INTO Enrollments (EnrollmentId, StudentId, CourseId, EnrollmentDate, Status)
VALUES
(1, 1, 1, '2024-09-01', N'Enrolled'),
(2, 1, 3, '2024-09-01', N'Enrolled'),
(3, 1, 4, '2024-09-01', N'Enrolled'),
(4, 2, 1, '2024-09-01', N'Enrolled'),
(5, 2, 3, '2024-09-01', N'Enrolled'),
(6, 2, 4, '2024-09-01', N'Enrolled'),
(7, 3, 1, '2024-09-02', N'Enrolled'),
(8, 3, 3, '2024-09-02', N'Enrolled'),
(9, 3, 4, '2024-09-02', N'Enrolled'),
(10, 4, 1, '2024-09-02', N'Enrolled'),
(11, 4, 3, '2024-09-02', N'Enrolled'),
(12, 4, 4, '2024-09-02', N'Enrolled');

-- Student 5-6 (DTVT): EE courses
INSERT INTO Enrollments (EnrollmentId, StudentId, CourseId, EnrollmentDate, Status)
VALUES
(13, 5, 1, '2024-09-03', N'Enrolled'),
(14, 5, 6, '2024-09-03', N'Enrolled'),
(15, 5, 7, '2024-09-03', N'Enrolled'),
(16, 6, 1, '2024-09-03', N'Enrolled'),
(17, 6, 3, '2024-09-03', N'Enrolled'),
(18, 6, 6, '2024-09-03', N'Enrolled'),
(19, 6, 7, '2024-09-03', N'Enrolled');

-- Student 7-8 (CK): ME courses
INSERT INTO Enrollments (EnrollmentId, StudentId, CourseId, EnrollmentDate, Status)
VALUES
(20, 7, 1, '2024-09-04', N'Enrolled'),
(21, 7, 6, '2024-09-04', N'Enrolled'),
(22, 7, 8, '2024-09-04', N'Enrolled'),
(23, 8, 1, '2024-09-04', N'Enrolled'),
(24, 8, 3, '2024-09-04', N'Enrolled'),
(25, 8, 6, '2024-09-04', N'Enrolled'),
(26, 8, 8, '2024-09-04', N'Enrolled');

-- Student 9-10 (QTKD): Business courses
INSERT INTO Enrollments (EnrollmentId, StudentId, CourseId, EnrollmentDate, Status)
VALUES
(27, 9, 1, '2024-09-05', N'Enrolled'),
(28, 9, 3, '2024-09-05', N'Enrolled'),
(29, 9, 9, '2024-09-05', N'Enrolled'),
(30, 10, 1, '2024-09-05', N'Enrolled'),
(31, 10, 3, '2024-09-05', N'Enrolled'),
(32, 10, 4, '2024-09-05', N'Enrolled'),
(33, 10, 9, '2024-09-05', N'Enrolled');

SET IDENTITY_INSERT Enrollments OFF;
GO

-- =============================================
-- 6. GRADES (Sample for IT101 and IT303)
-- =============================================

SET IDENTITY_INSERT Grades ON;

-- IT101 grades (10 students)
INSERT INTO Grades (GradeId, EnrollmentId, MidtermScore, FinalScore, TotalScore, LetterGrade)
VALUES
(1, 1, 8.5, 9.0, 8.8, 'A'),
(2, 4, 7.0, 7.5, 7.3, 'B'),
(3, 7, 6.5, 6.0, 6.2, 'C'),
(4, 10, 9.0, 9.5, 9.3, 'A'),
(5, 13, 5.5, 6.0, 5.8, 'C'),
(6, 16, 8.0, 8.5, 8.3, 'A'),
(7, 20, 7.5, 7.0, 7.2, 'B'),
(8, 23, 9.5, 9.0, 9.2, 'A'),
(9, 27, 4.0, 4.5, 4.3, 'D'),
(10, 30, 8.5, 8.0, 8.2, 'A');

-- IT303 grades (7 students)
INSERT INTO Grades (GradeId, EnrollmentId, MidtermScore, FinalScore, TotalScore, LetterGrade)
VALUES
(11, 2, 7.5, 8.0, 7.8, 'B'),
(12, 5, 8.0, 8.5, 8.3, 'A'),
(13, 8, 6.0, 6.5, 6.3, 'C'),
(14, 11, 9.0, 9.0, 9.0, 'A'),
(15, 17, 7.0, 7.5, 7.3, 'B'),
(16, 24, 8.5, 8.0, 8.2, 'A'),
(17, 28, 5.0, 5.5, 5.3, 'C'),
(18, 31, 9.5, 9.0, 9.2, 'A');

SET IDENTITY_INSERT Grades OFF;
GO

-- =============================================
-- Summary
-- =============================================

PRINT '========================================';
PRINT 'Sample Data Summary:';
PRINT '========================================';
SELECT 'Users' AS [Table], COUNT(*) AS [Count] FROM Users
UNION ALL SELECT 'Teachers', COUNT(*) FROM Teachers
UNION ALL SELECT 'Students', COUNT(*) FROM Students
UNION ALL SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL SELECT 'Enrollments', COUNT(*) FROM Enrollments
UNION ALL SELECT 'Grades', COUNT(*) FROM Grades;

PRINT '';
PRINT 'Test Accounts:';
PRINT '  Admin: admin / admin123';
PRINT '  Teacher: teacher01 / teacher123';
PRINT '  Student: student01 / student123';
PRINT '========================================';
GO
