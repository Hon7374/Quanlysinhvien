-- =============================================
-- Student Management System - Complete Database Setup
-- Includes: Structure + Sample Data for Academic Year 2025-2026
-- 14 Time Slots System (Morning: 5, Afternoon: 5, Evening: 4)
-- Date: November 26, 2025
-- =============================================

USE master;
GO

-- =============================================
-- DROP AND CREATE DATABASE
-- =============================================
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentManagementDB')
BEGIN
    ALTER DATABASE StudentManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StudentManagementDB;
    PRINT N'✓ Đã xóa database cũ';
END
GO

CREATE DATABASE StudentManagementDB;
PRINT N'✓ Đã tạo database mới';
GO

USE StudentManagementDB;
GO

-- =============================================
-- TABLE 1: Users
-- =============================================
PRINT N'Đang tạo bảng Users...';
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NULL,
    PasswordHash NVARCHAR(512) NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    Phone NVARCHAR(20),
    Role INT NOT NULL CHECK (Role IN (1, 2, 3)), -- 1=Admin, 2=Teacher, 3=Student
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);

CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_Username ON Users(Username);
GO

-- =============================================
-- TABLE 2: Teachers
-- =============================================
PRINT N'Đang tạo bảng Teachers...';
CREATE TABLE Teachers (
    TeacherId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL UNIQUE,
    TeacherCode NVARCHAR(20) UNIQUE NOT NULL,
    Department NVARCHAR(100),
    Degree NVARCHAR(50),
    Specialization NVARCHAR(100),
    HireDate DATE DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT N'Đang làm việc',
    CONSTRAINT FK_Teachers_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IX_Teachers_Department ON Teachers(Department);
CREATE INDEX IX_Teachers_Status ON Teachers(Status);
GO

-- =============================================
-- TABLE 3: Students
-- =============================================
PRINT N'Đang tạo bảng Students...';
CREATE TABLE Students (
    StudentId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL UNIQUE,
    StudentCode NVARCHAR(20) UNIQUE NOT NULL,
    DateOfBirth DATE,
    Gender NVARCHAR(10),
    Address NVARCHAR(200),
    Class NVARCHAR(50),
    Major NVARCHAR(100),
    AcademicYear INT,
    GPA DECIMAL(3,2) DEFAULT 0.00 CHECK (GPA >= 0 AND GPA <= 4.0),
    Status NVARCHAR(50) DEFAULT N'Đang học',
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Students_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IX_Students_Class ON Students(Class);
CREATE INDEX IX_Students_Major ON Students(Major);
CREATE INDEX IX_Students_Status ON Students(Status);
CREATE INDEX IX_Students_StudentCode ON Students(StudentCode);
GO

-- =============================================
-- TABLE 4: Courses
-- =============================================
PRINT N'Đang tạo bảng Courses...';
CREATE TABLE Courses (
    CourseId INT PRIMARY KEY IDENTITY(1,1),
    CourseCode NVARCHAR(20) UNIQUE NOT NULL,
    CourseName NVARCHAR(100) NOT NULL,
    Credits INT NOT NULL CHECK (Credits > 0 AND Credits <= 10),
    Description NVARCHAR(500),
    TeacherId INT NULL,
    Semester NVARCHAR(20),
    AcademicYear INT,
    MaxStudents INT DEFAULT 50 CHECK (MaxStudents > 0),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Courses_Teachers FOREIGN KEY (TeacherId)
        REFERENCES Teachers(TeacherId) ON DELETE SET NULL
);

CREATE INDEX IX_Courses_TeacherId ON Courses(TeacherId);
CREATE INDEX IX_Courses_Semester ON Courses(Semester);
CREATE INDEX IX_Courses_IsActive ON Courses(IsActive);
CREATE INDEX IX_Courses_CourseCode ON Courses(CourseCode);
GO

-- =============================================
-- TABLE 5: Semesters
-- =============================================
PRINT N'Đang tạo bảng Semesters...';
CREATE TABLE Semesters (
    SemesterId INT PRIMARY KEY IDENTITY(1,1),
    SemesterCode NVARCHAR(20) UNIQUE NOT NULL,
    SemesterName NVARCHAR(100) NOT NULL,
    AcademicYear INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(50) DEFAULT N'Sắp tới',
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_Semester_Dates CHECK (EndDate > StartDate)
);

CREATE INDEX IX_Semesters_AcademicYear ON Semesters(AcademicYear);
CREATE INDEX IX_Semesters_Status ON Semesters(Status);
GO

-- =============================================
-- TABLE 6: Schedules (14 Time Slots)
-- =============================================
PRINT N'Đang tạo bảng Schedules (14 tiết học)...';
CREATE TABLE Schedules (
    ScheduleId INT PRIMARY KEY IDENTITY(1,1),
    CourseId INT NOT NULL,
    DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6), -- 0=Monday, 6=Sunday
    TimeSlot INT NOT NULL CHECK (TimeSlot BETWEEN 0 AND 13), -- 0-13 = 14 time slots
    Room NVARCHAR(50),
    StartTime TIME,
    EndTime TIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Schedules_Courses FOREIGN KEY (CourseId)
        REFERENCES Courses(CourseId) ON DELETE CASCADE,
    CONSTRAINT UQ_Schedule_CourseTime UNIQUE(CourseId, DayOfWeek, TimeSlot)
);

CREATE INDEX IX_Schedules_CourseId ON Schedules(CourseId);
CREATE INDEX IX_Schedules_DayOfWeek ON Schedules(DayOfWeek);
CREATE INDEX IX_Schedules_TimeSlot ON Schedules(TimeSlot);
-- Create a filtered unique index to avoid blocking multiple rows with NULL room
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Schedules') AND name = 'UQ_Schedule_Time')
    CREATE UNIQUE INDEX UQ_Schedule_Time ON Schedules(DayOfWeek, TimeSlot, Room) WHERE Room IS NOT NULL;
GO

-- =============================================
-- TABLE 7: Enrollments
-- =============================================
PRINT N'Đang tạo bảng Enrollments...';
CREATE TABLE Enrollments (
    EnrollmentId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrollmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT N'Enrolled',
    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId)
        REFERENCES Students(StudentId) ON DELETE CASCADE,
    CONSTRAINT FK_Enrollments_Courses FOREIGN KEY (CourseId)
        REFERENCES Courses(CourseId) ON DELETE CASCADE,
    CONSTRAINT UQ_Student_Course UNIQUE(StudentId, CourseId)
);

CREATE INDEX IX_Enrollments_StudentId ON Enrollments(StudentId);
CREATE INDEX IX_Enrollments_CourseId ON Enrollments(CourseId);
CREATE INDEX IX_Enrollments_Status ON Enrollments(Status);
GO

-- =============================================
-- TABLE 8: Grades
-- =============================================
PRINT N'Đang tạo bảng Grades...';
CREATE TABLE Grades (
    GradeId INT PRIMARY KEY IDENTITY(1,1),
    EnrollmentId INT NOT NULL UNIQUE,
    MidtermScore DECIMAL(4,2) CHECK (MidtermScore >= 0 AND MidtermScore <= 10),
    FinalScore DECIMAL(4,2) CHECK (FinalScore >= 0 AND FinalScore <= 10),
    TotalScore DECIMAL(4,2) CHECK (TotalScore >= 0 AND TotalScore <= 10),
    LetterGrade NVARCHAR(5),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Grades_Enrollments FOREIGN KEY (EnrollmentId)
        REFERENCES Enrollments(EnrollmentId) ON DELETE CASCADE
);

CREATE INDEX IX_Grades_EnrollmentId ON Grades(EnrollmentId);
CREATE INDEX IX_Grades_LetterGrade ON Grades(LetterGrade);
GO

-- =============================================
-- TRIGGERS
-- =============================================
PRINT N'Đang tạo triggers...';
GO

-- Trigger: Update student GPA when grades change
CREATE TRIGGER trg_UpdateStudentGPA
ON Grades
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s
    SET GPA = ISNULL((
        SELECT CASE WHEN SUM(c.Credits) > 0 THEN SUM(g.TotalScore * c.Credits) / SUM(c.Credits) / 2.5 ELSE 0 END
        FROM Grades g
        INNER JOIN Enrollments e ON g.EnrollmentId = e.EnrollmentId
        INNER JOIN Courses c ON e.CourseId = c.CourseId
        WHERE e.StudentId = s.StudentId
    ), 0)
    FROM Students s
    WHERE s.StudentId IN (
        SELECT DISTINCT e.StudentId
        FROM Enrollments e
        WHERE e.EnrollmentId IN (
            SELECT EnrollmentId FROM inserted
            UNION
            SELECT EnrollmentId FROM deleted
        )
    );
END
GO

-- Trigger: Auto-update UpdatedAt in Grades
CREATE TRIGGER trg_Grades_UpdatedAt
ON Grades
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Grades
    SET UpdatedAt = GETDATE()
    WHERE GradeId IN (SELECT GradeId FROM inserted);
END
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================
PRINT N'Đang tạo stored procedure sp_RegisterStudentToCourse...';
GO

IF OBJECT_ID('sp_RegisterStudentToCourse', 'P') IS NOT NULL
    DROP PROCEDURE sp_RegisterStudentToCourse;
GO

CREATE PROCEDURE sp_RegisterStudentToCourse
    @StudentId INT,
    @CourseId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Lock course row to prevent race in counting students
        DECLARE @MaxStudents INT;
        SELECT @MaxStudents = MaxStudents FROM Courses WITH (UPDLOCK, HOLDLOCK) WHERE CourseId = @CourseId;

        DECLARE @Enrolled INT;
        SELECT @Enrolled = COUNT(*) FROM Enrollments WITH (UPDLOCK, HOLDLOCK) WHERE CourseId = @CourseId AND Status = 'Enrolled';

        IF @MaxStudents > 0 AND @Enrolled >= @MaxStudents
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Course capacity exceeded', 16, 1);
            RETURN -1;
        END

        -- Check schedule conflict for student: pairwise day/time match
        IF EXISTS (
            SELECT 1
            FROM Schedules s_existing
            INNER JOIN Enrollments e ON e.CourseId = s_existing.CourseId AND e.StudentId = @StudentId AND e.Status = 'Enrolled'
            INNER JOIN Schedules s_new ON s_new.CourseId = @CourseId AND s_new.DayOfWeek = s_existing.DayOfWeek AND s_new.TimeSlot = s_existing.TimeSlot
        )
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Schedule conflict for student', 16, 1);
            RETURN -2;
        END

        -- Guard against duplicate enrollment (unique constraint will also prevent double-insert)
        IF EXISTS (SELECT 1 FROM Enrollments WHERE StudentId = @StudentId AND CourseId = @CourseId)
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Student is already enrolled in this course', 16, 1);
            RETURN -3;
        END

        INSERT INTO Enrollments (StudentId, CourseId, EnrollmentDate, Status)
        VALUES (@StudentId, @CourseId, GETDATE(), 'Enrolled');

        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN -99;
    END CATCH
END
GO

-- =============================================
-- INSERT SAMPLE DATA
-- =============================================
PRINT N'';
PRINT N'========================================';
PRINT N'BẮT ĐẦU CHÈN DỮ LIỆU MẪU';
PRINT N'========================================';
PRINT N'';

-- =============================================
-- 1. USERS (Admin, Teachers, Students)
-- =============================================
PRINT N'1. Đang thêm Users...';
SET IDENTITY_INSERT Users ON;

-- Admin users (2)
INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Phone, Role, IsActive, CreatedAt, LastLogin)
VALUES
(1, 'admin', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), N'Quản trị viên', 'admin@university.edu.vn', '0123456789', 1, 1, GETDATE(), GETDATE()),
(2, 'admin2', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), N'Nguyễn Văn Quản', 'admin2@university.edu.vn', '0987654321', 1, 1, GETDATE(), NULL);

-- Teacher users (8)
INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Phone, Role, IsActive, CreatedAt, LastLogin)
VALUES
(3, 'teacher01', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Vũ Văn Định', 'nvgiang@university.edu.vn', '0901234567', 2, 1, GETDATE(), GETDATE()),
(4, 'teacher02', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Trần Trung', 'ttminh@university.edu.vn', '0912345678', 2, 1, GETDATE(), NULL),
(5, 'teacher03', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Vũ Thị Yến', 'lhnam@university.edu.vn', '0923456789', 2, 1, GETDATE(), NULL),
(6, 'teacher04', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Phạm Quang Huy', 'ptha@university.edu.vn', '0934567890', 2, 1, GETDATE(), NULL),
(7, 'teacher05', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Bùi Khánh Linh', 'vdthang@university.edu.vn', '0945678901', 2, 1, GETDATE(), NULL),
(8, 'teacher06', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Lê Cường', 'htlan@university.edu.vn', '0956789012', 2, 1, GETDATE(), NULL),
(9, 'teacher07', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Đặng Minh Tuấn', 'dmtuan@university.edu.vn', '0967890123', 2, 1, GETDATE(), NULL),
(10, 'teacher08', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Bùi Thị Hương', 'bthuong@university.edu.vn', '0978901234', 2, 1, GETDATE(), NULL);

-- Student users (15)
INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Phone, Role, IsActive, CreatedAt, LastLogin)
VALUES
(11, 'student01', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Nguyễn Văn An', 'student01@stu.edu.vn', '0981234567', 3, 1, GETDATE(), GETDATE()),
(12, 'student02', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Trần Thị Bình', 'student02@stu.edu.vn', '0982345678', 3, 1, GETDATE(), GETDATE()),
(13, 'student03', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Lê Văn Cường', 'student03@stu.edu.vn', '0983456789', 3, 1, GETDATE(), NULL),
(14, 'student04', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Phạm Thị Dung', 'student04@stu.edu.vn', '0984567890', 3, 1, GETDATE(), NULL),
(15, 'student05', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Hoàng Văn Em', 'student05@stu.edu.vn', '0985678901', 3, 1, GETDATE(), NULL),
(16, 'student06', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Vũ Thị Phương', 'student06@stu.edu.vn', '0986789012', 3, 1, GETDATE(), NULL),
(17, 'student07', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Đặng Văn Giang', 'student07@stu.edu.vn', '0987890123', 3, 1, GETDATE(), NULL),
(18, 'student08', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Bùi Thị Hoa', 'student08@stu.edu.vn', '0988901234', 3, 1, GETDATE(), NULL),
(19, 'student09', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Ngô Văn Inh', 'student09@stu.edu.vn', '0989012345', 3, 1, GETDATE(), NULL),
(20, 'student10', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Mai Thị Kim', 'student10@stu.edu.vn', '0980123456', 3, 1, GETDATE(), NULL),
(21, 'student11', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Lý Văn Long', 'student11@stu.edu.vn', '0981112233', 3, 1, GETDATE(), NULL),
(22, 'student12', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Trịnh Thị Mai', 'student12@stu.edu.vn', '0982223344', 3, 1, GETDATE(), NULL),
(23, 'student13', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Phan Văn Nam', 'student13@stu.edu.vn', '0983334455', 3, 1, GETDATE(), NULL),
(24, 'student14', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Dương Thị Oanh', 'student14@stu.edu.vn', '0984445566', 3, 1, GETDATE(), NULL),
(25, 'student15', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'student123'), 2), N'Tạ Văn Phúc', 'student15@stu.edu.vn', '0985556677', 3, 1, GETDATE(), NULL);

SET IDENTITY_INSERT Users OFF;
PRINT N'   ✓ Đã thêm 25 users (2 admin, 8 teachers, 15 students)';
GO

-- =============================================
-- 2. TEACHERS
-- =============================================
PRINT N'2. Đang thêm Teachers...';
SET IDENTITY_INSERT Teachers ON;

INSERT INTO Teachers (TeacherId, UserId, TeacherCode, Department, Degree, Specialization, HireDate, Status)
VALUES
(1, 3, 'GV2020001', N'Khoa Công nghệ Thông tin', N'Tiến sĩ', N'Lập trình và Phát triển phần mềm', '2020-09-01', N'Đang làm việc'),
(2, 4, 'GV2019002', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Cơ sở dữ liệu và Big Data', '2019-08-15', N'Đang làm việc'),
(3, 5, 'GV2021003', N'Khoa Toán', N'Tiến sĩ', N'Giải tích và Đại số', '2021-01-10', N'Đang làm việc'),
(4, 6, 'GV2020004', N'Khoa Vật lý', N'Thạc sĩ', N'Vật lý đại cương', '2020-03-20', N'Đang làm việc'),
(5, 7, 'GV2018005', N'Khoa Ngoại ngữ', N'Thạc sĩ', N'Tiếng Anh giao tiếp', '2018-07-01', N'Đang làm việc'),
(6, 8, 'GV2022006', N'Khoa Kinh tế', N'Tiến sĩ', N'Quản trị kinh doanh', '2022-02-15', N'Đang làm việc'),
(7, 9, 'GV2019007', N'Khoa Mỹ thuật', N'Thạc sĩ', N'Thiết kế đồ họa', '2019-09-01', N'Đang làm việc'),
(8, 10, 'GV2021008', N'Khoa Hóa', N'Thạc sĩ', N'Hóa học đại cương', '2021-08-20', N'Đang làm việc');

SET IDENTITY_INSERT Teachers OFF;
PRINT N'   ✓ Đã thêm 8 giảng viên';
GO

-- =============================================
-- 3. STUDENTS
-- =============================================
PRINT N'3. Đang thêm Students...';
SET IDENTITY_INSERT Students ON;

INSERT INTO Students (StudentId, UserId, StudentCode, DateOfBirth, Gender, Address, Class, Major, AcademicYear, GPA, Status)
VALUES
(1, 11, 'SV2025001', '2003-05-15', N'Nam', N'123 Lê Lợi, Q1, TP.HCM', N'CNTT-K21A', N'Công nghệ thông tin', 2025, 3.45, N'Đang học'),
(2, 12, 'SV2025002', '2003-08-20', N'Nữ', N'456 Trần Hưng Đạo, Q5, TP.HCM', N'CNTT-K21A', N'Công nghệ thông tin', 2025, 3.67, N'Đang học'),
(3, 13, 'SV2025003', '2003-03-10', N'Nam', N'789 Nguyễn Huệ, Q1, TP.HCM', N'CNTT-K21B', N'Công nghệ thông tin', 2025, 3.21, N'Đang học'),
(4, 14, 'SV2025004', '2003-11-25', N'Nữ', N'321 Võ Văn Tần, Q3, TP.HCM', N'CNTT-K21B', N'Công nghệ thông tin', 2025, 3.89, N'Đang học'),
(5, 15, 'SV2025005', '2003-07-30', N'Nam', N'654 Điện Biên Phủ, Bình Thạnh, TP.HCM', N'DTVT-K21', N'Điện tử viễn thông', 2025, 3.12, N'Đang học'),
(6, 16, 'SV2025006', '2003-09-12', N'Nữ', N'987 Cách Mạng T8, Q10, TP.HCM', N'DTVT-K21', N'Điện tử viễn thông', 2025, 3.56, N'Đang học'),
(7, 17, 'SV2025007', '2003-01-18', N'Nam', N'147 Lý Thường Kiệt, Q11, TP.HCM', N'CK-K21', N'Cơ khí', 2025, 3.34, N'Đang học'),
(8, 18, 'SV2025008', '2003-04-22', N'Nữ', N'258 Phan Đăng Lưu, Phú Nhuận, TP.HCM', N'CK-K21', N'Cơ khí', 2025, 3.78, N'Đang học'),
(9, 19, 'SV2025009', '2003-06-08', N'Nam', N'369 Hoàng Văn Thụ, Tân Bình, TP.HCM', N'QTKD-K21', N'Quản trị kinh doanh', 2025, 3.23, N'Đang học'),
(10, 20, 'SV2025010', '2003-12-14', N'Nữ', N'741 Nguyễn TM Khai, Q3, TP.HCM', N'QTKD-K21', N'Quản trị kinh doanh', 2025, 3.91, N'Đang học'),
(11, 21, 'SV2025011', '2003-02-05', N'Nam', N'852 Lạc Long Quân, Q11, TP.HCM', N'CNTT-K21A', N'Công nghệ thông tin', 2025, 3.50, N'Đang học'),
(12, 22, 'SV2025012', '2003-10-17', N'Nữ', N'963 Hoàng Hoa Thám, Tân Bình, TP.HCM', N'CNTT-K21B', N'Công nghệ thông tin', 2025, 3.72, N'Đang học'),
(13, 23, 'SV2025013', '2003-04-28', N'Nam', N'159 Phan Xích Long, Phú Nhuận, TP.HCM', N'DTVT-K21', N'Điện tử viễn thông', 2025, 3.28, N'Đang học'),
(14, 24, 'SV2025014', '2003-07-19', N'Nữ', N'357 Trường Chinh, Q12, TP.HCM', N'QTKD-K21', N'Quản trị kinh doanh', 2025, 3.65, N'Đang học'),
(15, 25, 'SV2025015', '2003-09-30', N'Nam', N'486 Quang Trung, Gò Vấp, TP.HCM', N'CK-K21', N'Cơ khí', 2025, 3.41, N'Đang học');

SET IDENTITY_INSERT Students OFF;
PRINT N'   ✓ Đã thêm 15 sinh viên';
GO

-- =============================================
-- 4. COURSES (HK1, HK2, HK3 2025-2026)
-- =============================================
PRINT N'4. Đang thêm Courses...';
SET IDENTITY_INSERT Courses ON;

-- HK1 2025-2026 (7 courses) - Currently active semester
INSERT INTO Courses (CourseId, CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
VALUES
(1, '000146', N'Cấu trúc dữ liệu và giải thuật nâng cao', 3, N'Các cấu trúc dữ liệu và thuật toán nâng cao', 1, N'HK1 2025-2026', 2025, 42, 1),
(2, '000863', N'Hệ phân tán', 2, N'Các hệ thống phân tán và xử lý song song', 6, N'HK1 2025-2026', 2025, 30, 1),
(3, '001436', N'Lập trình Java', 3, N'Lập trình hướng đối tượng với Java', 2, N'HK1 2025-2026', 2025, 33, 1),
(4, '004755', N'Lập trình .net', 4, N'Phát triển ứng dụng với .NET Framework', 2, N'HK1 2025-2026', 2025, 48, 1),
(5, '003928', N'Lịch sử Đảng Cộng sản Việt Nam', 2, N'Lịch sử phát triển Đảng CSVN', 3, N'HK1 2025-2026', 2025, 30, 1),
(6, '001877', N'Nhập môn An toàn và bảo mật thông tin', 2, N'Các khái niệm cơ bản về an toàn thông tin', 4, N'HK1 2025-2026', 2025, 30, 1),
(7, '001995', N'Phân tích thiết kế hướng đối tượng', 3, N'UML và phân tích thiết kế hệ thống', 5, N'HK1 2025-2026', 2025, 45, 1);

-- HK2 2025-2026 (8 courses)
INSERT INTO Courses (CourseId, CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
VALUES
(8, '004750', N'Học máy cơ bản', 3, N'Các thuật toán machine learning cơ bản', 1, N'HK2 2025-2026', 2025, 45, 1),
(9, '001132', N'Kiểm thử và đảm bảo chất lượng PM', 2, N'Testing và QA trong phần mềm', 2, N'HK2 2025-2026', 2025, 27, 1),
(10, '001427', N'Lập trình hệ thống', 2, N'System programming với C/C++', 1, N'HK2 2025-2026', 2025, 27, 1),
(11, '004294', N'Lập trình trên thiết bị di động', 3, N'Phát triển ứng dụng mobile Android/iOS', 2, N'HK2 2025-2026', 2025, 30, 1),
(12, '004754', N'Lập trình web nâng cao', 4, N'React, Node.js, REST API', 1, N'HK2 2025-2026', 2025, 48, 1),
(13, '001957', N'Phần mềm mã nguồn mở', 2, N'Sử dụng và phát triển phần mềm mã nguồn mở', 2, N'HK2 2025-2026', 2025, 21, 1),
(14, '002234', N'Quản trị dự án CNTT', 2, N'Quản lý dự án phần mềm Agile/Scrum', 2, N'HK2 2025-2026', 2025, 30, 1),
(15, '004758', N'Trí tuệ nhân tạo', 3, N'AI, neural networks, deep learning', 1, N'HK2 2025-2026', 2025, 45, 1);

-- HK1 2026-2027 (8 courses)
INSERT INTO Courses (CourseId, CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
VALUES
(16, '004861', N'Hệ thống IoT và ứng dụng', 2, N'Internet of Things và ứng dụng thực tế', 1, N'HK1 2026-2027', 2026, 30, 1),
(17, '000958', N'Hệ thống thông tin không gian', 2, N'GIS và xử lý dữ liệu không gian', 2, N'HK1 2026-2027', 2026, 27, 1),
(18, '004295', N'Học máy nâng cao', 3, N'Advanced machine learning techniques', 1, N'HK1 2026-2027', 2026, 39, 1),
(19, '004753', N'Lập trình Blockchain', 3, N'Smart contracts và DApps', 1, N'HK1 2026-2027', 2026, 42, 1),
(20, '004757', N'Ngôn ngữ kịch bản', 3, N'Python, Ruby, Shell scripting', 1, N'HK1 2026-2027', 2026, 39, 1),
(21, '001901', N'Nhập môn xử lý ảnh', 2, N'Image processing cơ bản', 1, N'HK1 2026-2027', 2026, 30, 1),
(22, '004759', N'Phân tích và trực quan hóa dữ liệu', 3, N'Data analysis và visualization', 2, N'HK1 2026-2027', 2026, 45, 1),
(23, '002033', N'Phát triển phần mềm an toàn', 2, N'Secure coding practices', 1, N'HK1 2026-2027', 2026, 30, 1);

SET IDENTITY_INSERT Courses OFF;
PRINT N'   ✓ Đã thêm 23 môn học (HK1 2025-2026: 7, HK2 2025-2026: 8, HK1 2026-2027: 8)';
GO

-- =============================================
-- ADDITIONAL SAMPLE: Extra teachers & course sections
-- One logical course (Phân tích hệ thống) will have 5 sections taught by 5 different teachers
-- =============================================
PRINT N'Đang thêm users & teachers mẫu bổ sung (5 giảng viên cho 1 môn)...';
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Phone, Role, IsActive, CreatedAt, LastLogin)
VALUES
(26, 'teacher09', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Nguyễn Văn Hùng', 'vhung@university.edu.vn', '0987650011', 2, 1, GETDATE(), NULL),
(27, 'teacher10', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Hoàng Thị Lan', 'htlan2@university.edu.vn', '0987650012', 2, 1, GETDATE(), NULL),
(28, 'teacher11', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Phan Văn Long', 'pvlong@university.edu.vn', '0987650013', 2, 1, GETDATE(), NULL),
(29, 'teacher12', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Trần Nguyễn Minh', 'tnminh@university.edu.vn', '0987650014', 2, 1, GETDATE(), NULL),
(30, 'teacher13', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Lê Thị Phương', 'ltphuong@university.edu.vn', '0987650015', 2, 1, GETDATE(), NULL);
SET IDENTITY_INSERT Users OFF;

SET IDENTITY_INSERT Teachers ON;
INSERT INTO Teachers (TeacherId, UserId, TeacherCode, Department, Degree, Specialization, HireDate, Status)
VALUES
(9, 26, 'GV2021009', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Phân tích hệ thống', '2021-08-01', N'Đang làm việc'),
(10, 27, 'GV2021010', N'Khoa Công nghệ Thông tin', N'Tiến sĩ', N'Phân tích hệ thống', '2020-06-10', N'Đang làm việc'),
(11, 28, 'GV2021011', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Thiết kế hệ thống', '2019-09-12', N'Đang làm việc'),
(12, 29, 'GV2021012', N'Khoa Công nghệ Thông tin', N'Tiến sĩ', N'Phân tích & Thiết kế', '2018-04-22', N'Đang làm việc'),
(13, 30, 'GV2021013', N'Khoa Công nghệ Thông tin', N'Ỉn', N'Phân tích hệ thống', '2017-11-03', N'Đang làm việc');
SET IDENTITY_INSERT Teachers OFF;

-- Add course sections for the same logical course (Phân tích hệ thống) taught by different teachers
SET IDENTITY_INSERT Courses ON;
INSERT INTO Courses (CourseId, CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
VALUES
(24, 'PA001', N'Phân tích hệ thống', 3, N'Phân tích hệ thống - lớp A', 9, N'HK1 2025-2026', 2025, 40, 1),
(25, 'PA002', N'Phân tích hệ thống', 3, N'Phân tích hệ thống - lớp B', 10, N'HK1 2025-2026', 2025, 40, 1),
(26, 'PA003', N'Phân tích hệ thống', 3, N'Phân tích hệ thống - lớp C', 11, N'HK1 2025-2026', 2025, 40, 1),
(27, 'PA004', N'Phân tích hệ thống', 3, N'Phân tích hệ thống - lớp D', 12, N'HK1 2025-2026', 2025, 40, 1),
(28, 'PA005', N'Phân tích hệ thống', 3, N'Phân tích hệ thống - lớp E', 13, N'HK1 2025-2026', 2025, 40, 1);
SET IDENTITY_INSERT Courses OFF;
PRINT N'   ✓ Đã thêm 5 giảng viên mẫu & 5 section của môn Phân tích hệ thống (để test schedule)';
GO

-- =============================================
-- ADD SCHEDULES FOR SAMPLE COURSE SECTIONS (for testing conflict scenarios)
-- =============================================
PRINT N'Đang thêm một vài lịch cho course sections mới...';
INSERT INTO Schedules (CourseId, DayOfWeek, TimeSlot, Room, StartTime, EndTime)
VALUES
(24, 0, 0, N'R201', '07:00:00', '07:50:00'), -- Sect. A - Monday Slot 1
(25, 0, 0, N'R202', '07:00:00', '07:50:00'), -- Sect. B - Monday Slot 1 (conflicts for auto-scheduler check)
(26, 0, 2, N'R201', '08:50:00', '09:40:00'), -- Sect. C - Monday Slot 3
(27, 1, 0, N'R203', '07:00:00', '07:50:00'), -- Sect. D - Tue Slot 1
(28, 2, 1, N'R204', '07:55:00', '08:45:00'); -- Sect. E - Wed Slot 2

GO

-- =============================================
-- ADDITIONAL SAMPLE: More teachers + sections for other courses (multi-section for testing)
-- =============================================
PRINT N'Đang thêm thêm giảng viên và course sections bổ sung (multi-section courses)...';
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Phone, Role, IsActive, CreatedAt, LastLogin)
VALUES
(31, 'teacher14', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Ngô Thị Bích', 'ntbich@university.edu.vn', '0987650021', 2, 1, GETDATE(), NULL),
(32, 'teacher15', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Phùng Văn Tiến', 'pvttien@university.edu.vn', '0987650022', 2, 1, GETDATE(), NULL),
(33, 'teacher16', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Võ Minh Quân', 'vmquan@university.edu.vn', '0987650023', 2, 1, GETDATE(), NULL),
(34, 'teacher17', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Nguyễn Thị Hồng', 'nthong@university.edu.vn', '0987650024', 2, 1, GETDATE(), NULL),
(35, 'teacher18', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Đỗ Văn Long', 'dvlong@university.edu.vn', '0987650025', 2, 1, GETDATE(), NULL),
(36, 'teacher19', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'teacher123'), 2), N'Hoàng Thị Mai', 'htmai@university.edu.vn', '0987650026', 2, 1, GETDATE(), NULL);
SET IDENTITY_INSERT Users OFF;

SET IDENTITY_INSERT Teachers ON;
INSERT INTO Teachers (TeacherId, UserId, TeacherCode, Department, Degree, Specialization, HireDate, Status)
VALUES
(14, 31, 'GV2021014', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Lập trình Java', '2020-09-01', N'Đang làm việc'),
(15, 32, 'GV2021015', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Lập trình .NET', '2019-03-12', N'Đang làm việc'),
(16, 33, 'GV2021016', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Học máy', '2021-02-18', N'Đang làm việc'),
(17, 34, 'GV2021017', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Phát triển web', '2018-11-05', N'Đang làm việc'),
(18, 35, 'GV2021018', N'Khoa Công nghệ Thông tin', N'Tiến sĩ', N'Quản trị dự án CNTT', '2017-06-10', N'Đang làm việc'),
(19, 36, 'GV2021019', N'Khoa Công nghệ Thông tin', N'Thạc sĩ', N'Phân tích dữ liệu', '2019-10-01', N'Đang làm việc');
SET IDENTITY_INSERT Teachers OFF;

-- Add more Course sections for existing popular courses (2 sections each)
SET IDENTITY_INSERT Courses ON;
INSERT INTO Courses (CourseId, CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear, MaxStudents, IsActive)
VALUES
(29, 'JAVA_A', N'Lập trình Java', 3, N'Lập trình hướng đối tượng với Java - A', 14, N'HK1 2025-2026', 2025, 35, 1),
(30, 'JAVA_B', N'Lập trình Java', 3, N'Lập trình hướng đối tượng với Java - B', 9, N'HK1 2025-2026', 2025, 35, 1),
(31, 'DOTNET_A', N'Lập trình .NET', 4, N'Phát triển ứng dụng .NET - A', 15, N'HK1 2025-2026', 2025, 40, 1),
(32, 'DOTNET_B', N'Lập trình .NET', 4, N'Phát triển ứng dụng .NET - B', 2, N'HK1 2025-2026', 2025, 40, 1),
(33, 'ML_A', N'Học máy cơ bản', 3, N'Machine learning - A', 16, N'HK1 2025-2026', 2025, 45, 1),
(34, 'ML_B', N'Học máy cơ bản', 3, N'Machine learning - B', 1, N'HK1 2025-2026', 2025, 45, 1),
(35, 'WEB_A', N'Lập trình web nâng cao', 4, N'React, Node.js, REST - A', 17, N'HK1 2025-2026', 2025, 48, 1),
(36, 'WEB_B', N'Lập trình web nâng cao', 4, N'Lập trình web nâng cao - B', 6, N'HK1 2025-2026', 2025, 48, 1),
(37, 'PM_A', N'Quản trị dự án CNTT', 2, N'Quản trị dự án Agile/Scrum - A', 18, N'HK1 2025-2026', 2025, 30, 1),
(38, 'PM_B', N'Quản trị dự án CNTT', 2, N'Quản trị dự án Agile/Scrum - B', 7, N'HK1 2025-2026', 2025, 30, 1),
(39, 'DATA_A', N'Phân tích và trực quan hóa dữ liệu', 3, N'Data viz - A', 19, N'HK1 2025-2026', 2025, 40, 1),
(40, 'DATA_B', N'Phân tích và trực quan hóa dữ liệu', 3, N'Data viz - B', 2, N'HK1 2025-2026', 2025, 40, 1);
SET IDENTITY_INSERT Courses OFF;
PRINT N'   ✓ Đã thêm thêm 6 giảng viên và 12 course sections bổ sung (cho các bài test khác nhau)';
GO

-- Add schedules for new course sections (various timeslots)
PRINT N'Đang thêm lịch cho course sections bổ sung...';
INSERT INTO Schedules (CourseId, DayOfWeek, TimeSlot, Room, StartTime, EndTime)
VALUES
(29, 0, 3, N'R205', '09:50:00', '10:40:00'), -- JAVA_A Monday slot4
(30, 3, 3, N'R206', '09:50:00', '10:40:00'), -- JAVA_B Thu slot4
(31, 0, 4, N'R207', '10:45:00', '11:35:00'), -- DOTNET_A Mon slot5
(32, 2, 4, N'R208', '10:45:00', '11:35:00'), -- DOTNET_B Wed slot5
(33, 4, 6, N'R209', '12:30:00', '13:20:00'), -- ML_A Fri slot7
(34, 1, 7, N'R210', '13:25:00', '14:15:00'), -- ML_B Tue slot8
(35, 2, 9, N'R211', '16:15:00', '17:05:00'), -- WEB_A Wed slot10
(36, 0, 9, N'R212', '16:15:00', '17:05:00'), -- WEB_B Mon slot10
(37, 5, 2, N'R213', '08:50:00', '09:40:00'), -- PM_A Sat slot3
(38, 5, 3, N'R214', '09:50:00', '10:40:00'), -- PM_B Sat slot4
(39, 4, 11, N'R215', '18:25:00', '19:15:00'), -- DATA_A Fri slot12
(40, 4, 12, N'R216', '19:20:00', '20:10:00'); -- DATA_B Fri slot13
GO

-- =============================================
-- 5. SEMESTERS (Academic Year 2025-2026)
-- =============================================
PRINT N'5. Đang thêm Semesters cho năm học 2025-2026...';
SET IDENTITY_INSERT Semesters ON;

INSERT INTO Semesters (SemesterId, SemesterCode, SemesterName, AcademicYear, StartDate, EndDate, Status)
VALUES
(1, 'HK1-2025-2026', N'HK1 2025-2026', 2025, '2025-09-01', '2025-12-31', N'Hoạt động'),
(2, 'HK2-2025-2026', N'HK2 2025-2026', 2025, '2026-01-05', '2026-05-31', N'Sắp tới'),
(3, 'HK1-2026-2027', N'HK1 2026-2027', 2026, '2026-09-01', '2026-12-31', N'Sắp tới');

SET IDENTITY_INSERT Semesters OFF;
PRINT N'   ✓ Đã thêm 3 học kỳ (HK1 2025-2026, HK2 2025-2026, HK1 2026-2027)';
GO

-- =============================================
-- 6. SCHEDULES (14 Time Slots System)
-- =============================================
-- Không thêm schedules mẫu - sử dụng tính năng "Tự động phân lịch học" trong ứng dụng
PRINT N'6. Bỏ qua Schedules mẫu - sử dụng tính năng tự động phân lịch';
GO

-- =============================================
-- 7. ENROLLMENTS & GRADES
-- =============================================
-- Không thêm enrollments và grades mẫu - sinh viên sẽ tự đăng ký môn học
PRINT N'7-8. Bỏ qua Enrollments và Grades mẫu';
GO


-- =============================================
-- FINAL SUMMARY
-- =============================================
PRINT N'';
PRINT N'========================================';
PRINT N'HOÀN TẤT TẠO DATABASE';
PRINT N'========================================';
PRINT N'';
PRINT N'📊 THỐNG KÊ:';
PRINT N'';

-- Count records
DECLARE @UserCount INT, @TeacherCount INT, @StudentCount INT, @CourseCount INT, @SemesterCount INT, @ScheduleCount INT, @EnrollmentCount INT, @GradeCount INT;
SELECT @UserCount = COUNT(*) FROM Users;
SELECT @TeacherCount = COUNT(*) FROM Teachers;
SELECT @StudentCount = COUNT(*) FROM Students;
SELECT @CourseCount = COUNT(*) FROM Courses;
SELECT @SemesterCount = COUNT(*) FROM Semesters;
SELECT @ScheduleCount = COUNT(*) FROM Schedules;
SELECT @EnrollmentCount = COUNT(*) FROM Enrollments;
SELECT @GradeCount = COUNT(*) FROM Grades;

PRINT N'✓ Users: ' + CAST(@UserCount AS NVARCHAR(10));
PRINT N'  - Admin: 2';
PRINT N'  - Teachers: ' + CAST(@TeacherCount AS NVARCHAR(10));
PRINT N'  - Students: ' + CAST(@StudentCount AS NVARCHAR(10));
PRINT N'';
PRINT N'✓ Semesters: ' + CAST(@SemesterCount AS NVARCHAR(10));
PRINT N'✓ Courses: ' + CAST(@CourseCount AS NVARCHAR(10));

-- Course breakdown by semester
DECLARE @HK1_2025 INT, @HK2_2025 INT, @HK1_2026 INT;
SELECT @HK1_2025 = COUNT(*) FROM Courses WHERE Semester = 'HK1 2025-2026';
SELECT @HK2_2025 = COUNT(*) FROM Courses WHERE Semester = 'HK2 2025-2026';
SELECT @HK1_2026 = COUNT(*) FROM Courses WHERE Semester = 'HK1 2026-2027';

PRINT N'  - HK1 2025-2026: ' + CAST(@HK1_2025 AS NVARCHAR(10)) + N' môn (đang diễn ra)';
PRINT N'  - HK2 2025-2026: ' + CAST(@HK2_2025 AS NVARCHAR(10)) + N' môn';
PRINT N'  - HK1 2026-2027: ' + CAST(@HK1_2026 AS NVARCHAR(10)) + N' môn';
PRINT N'';
PRINT N'✓ Schedules: ' + CAST(@ScheduleCount AS NVARCHAR(10)) + N' (sử dụng tính năng tự động phân lịch)';
PRINT N'✓ Enrollments: ' + CAST(@EnrollmentCount AS NVARCHAR(10)) + N' (sinh viên tự đăng ký)';
PRINT N'✓ Grades: ' + CAST(@GradeCount AS NVARCHAR(10));
PRINT N'';
PRINT N'========================================';
PRINT N'📅 NĂM HỌC 2025-2026 & 2026-2027';
PRINT N'========================================';
PRINT N'Hiện tại: 26/11/2025 - Đang trong HK1 2025-2026';
PRINT N'';
PRINT N'• HK1 2025-2026 (Sep-Dec 2025): Đang diễn ra - 7 môn';
PRINT N'• HK2 2025-2026 (Jan-May 2026): Sắp tới - 8 môn';
PRINT N'• HK1 2026-2027 (Sep-Dec 2026): Sắp tới - 8 môn';
PRINT N'';
PRINT N'========================================';
PRINT N'🕐 KHUNG GIỜ HỌC (14 TIẾT/NGÀY)';
PRINT N'========================================';
PRINT N'BUỔI SÁNG (5 tiết):';
PRINT N'  Tiết 1: 07:00-07:50 | Tiết 2: 07:55-08:45';
PRINT N'  Tiết 3: 08:50-09:40 | Tiết 4: 09:50-10:40';
PRINT N'  Tiết 5: 10:45-11:35';
PRINT N'';
PRINT N'BUỔI CHIỀU (5 tiết):';
PRINT N'  Tiết 6: 12:30-13:20 | Tiết 7: 13:25-14:15';
PRINT N'  Tiết 8: 14:20-15:10 | Tiết 9: 15:20-16:10';
PRINT N'  Tiết 10: 16:15-17:05';
PRINT N'';
PRINT N'BUỔI TỐI (4 tiết):';
PRINT N'  Tiết 11: 17:30-18:20 | Tiết 12: 18:25-19:15';
PRINT N'  Tiết 13: 19:20-20:10 | Tiết 14: 20:15-21:05';
PRINT N'';
PRINT N'========================================';
PRINT N'🔐 TÀI KHOẢN MẪU';
PRINT N'========================================';
PRINT N'Admin:';
PRINT N'  Username: admin | Password: admin123';
PRINT N'';
PRINT N'Teacher:';
PRINT N'  Username: teacher01 | Password: teacher123';
PRINT N'';
PRINT N'Student:';
PRINT N'  Username: student01 | Password: student123';
PRINT N'  Username: student02 | Password: student123';
PRINT N'========================================';
PRINT N'';
PRINT N'✅ DATABASE SẴN SÀNG SỬ DỤNG!';
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    PRINT N'Đang tạo bảng Payments...';
    CREATE TABLE Payments (
        PaymentId INT PRIMARY KEY IDENTITY(1,1),
        StudentId INT NOT NULL,
        Semester NVARCHAR(20) NOT NULL,
        AcademicYear INT NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL CHECK (TotalAmount >= 0),
        Description NVARCHAR(500),
        Status NVARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Paid', 'Failed')),
        CreatedAt DATETIME DEFAULT GETDATE(),
        PaidAt DATETIME NULL,
        PaymentMethod NVARCHAR(50) DEFAULT 'VietQR',
        TransactionId NVARCHAR(100),
        CONSTRAINT FK_Payments_Students FOREIGN KEY (StudentId)
            REFERENCES Students(StudentId) ON DELETE CASCADE
    );

    CREATE INDEX IX_Payments_StudentId ON Payments(StudentId);
    CREATE INDEX IX_Payments_Status ON Payments(Status);
    CREATE INDEX IX_Payments_Semester ON Payments(Semester, AcademicYear);

    PRINT N'✓ Đã tạo bảng Payments';
END
ELSE
BEGIN
    PRINT N'⚠ Bảng Payments đã tồn tại';
END
GO

-- =============================================
-- 2. ADD PAYMENTSTATUS TO ENROLLMENTS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Enrollments') AND name = 'PaymentStatus')
BEGIN
    PRINT N'Đang thêm cột PaymentStatus vào bảng Enrollments...';
    ALTER TABLE Enrollments
    ADD PaymentStatus NVARCHAR(20) DEFAULT 'Unpaid' CHECK (PaymentStatus IN ('Unpaid', 'Paid'));

    PRINT N'✓ Đã thêm cột PaymentStatus';
END
ELSE
BEGIN
    PRINT N'⚠ Cột PaymentStatus đã tồn tại';
END
GO

-- =============================================
-- 3. ADD CANCELLEDDATE TO ENROLLMENTS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Enrollments') AND name = 'CancelledDate')
BEGIN
    PRINT N'Đang thêm cột CancelledDate vào bảng Enrollments...';
    ALTER TABLE Enrollments
    ADD CancelledDate DATETIME NULL;

    PRINT N'✓ Đã thêm cột CancelledDate';
END
ELSE
BEGIN
    PRINT N'⚠ Cột CancelledDate đã tồn tại';
END
GO

-- =============================================
-- 4. CREATE PAYMENT SETTINGS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentSettings')
BEGIN
    PRINT N'Đang tạo bảng PaymentSettings...';
    CREATE TABLE PaymentSettings (
        SettingId INT PRIMARY KEY IDENTITY(1,1),
        SettingKey NVARCHAR(50) UNIQUE NOT NULL,
        SettingValue NVARCHAR(500) NOT NULL,
        Description NVARCHAR(200),
        UpdatedAt DATETIME DEFAULT GETDATE()
    );

    -- Insert default VietQR settings
    INSERT INTO PaymentSettings (SettingKey, SettingValue, Description) VALUES
    ('VIETQR_BANK_ID', 'MB', N'Mã ngân hàng (ví dụ: MB, VCB, TCB)'),
    ('VIETQR_ACCOUNT_NO', '0123456789', N'Số tài khoản nhận tiền'),
    ('VIETQR_ACCOUNT_NAME', 'TRUONG DAI HOC XYZ', N'Tên chủ tài khoản'),
    ('VIETQR_TEMPLATE', 'print', N'Template VietQR (compact, compact2, print, qr_only)'),
    ('TUITION_PER_CREDIT', '750000', N'Học phí mỗi tín chỉ (VNĐ)'),
    ('CANCEL_DEADLINE_DAYS', '7', N'Số ngày được phép hủy môn sau khi đăng ký');

    PRINT N'✓ Đã tạo bảng PaymentSettings với dữ liệu mặc định';
END
ELSE
BEGIN
    PRINT N'⚠ Bảng PaymentSettings đã tồn tại';
END
GO

-- =============================================
-- 5. CREATE AUDIT LOG FOR CANCELLATIONS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EnrollmentAuditLog')
BEGIN
    PRINT N'Đang tạo bảng EnrollmentAuditLog...';
    CREATE TABLE EnrollmentAuditLog (
        LogId INT PRIMARY KEY IDENTITY(1,1),
        EnrollmentId INT NOT NULL,
        StudentId INT NOT NULL,
        CourseId INT NOT NULL,
        Action NVARCHAR(50) NOT NULL, -- 'Registered', 'Cancelled', 'PaymentCompleted'
        OldStatus NVARCHAR(20),
        NewStatus NVARCHAR(20),
        Reason NVARCHAR(500),
        CreatedBy INT, -- UserId who performed the action
        CreatedAt DATETIME DEFAULT GETDATE()
    );

    CREATE INDEX IX_AuditLog_EnrollmentId ON EnrollmentAuditLog(EnrollmentId);
    CREATE INDEX IX_AuditLog_StudentId ON EnrollmentAuditLog(StudentId);
    CREATE INDEX IX_AuditLog_Action ON EnrollmentAuditLog(Action);

    PRINT N'✓ Đã tạo bảng EnrollmentAuditLog';
END
ELSE
BEGIN
    PRINT N'⚠ Bảng EnrollmentAuditLog đã tồn tại';
END
GO

-- =============================================
-- 6. CREATE STORED PROCEDURE: sp_CancelEnrollment
-- =============================================
IF OBJECT_ID('sp_CancelEnrollment', 'P') IS NOT NULL
    DROP PROCEDURE sp_CancelEnrollment;
GO

CREATE PROCEDURE sp_CancelEnrollment
    @EnrollmentId INT,
    @StudentId INT,
    @Reason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CourseId INT, @CurrentStatus NVARCHAR(20), @PaymentStatus NVARCHAR(20);
        DECLARE @EnrollmentDate DATETIME, @CancelDeadlineDays INT;

        -- Get enrollment info
        SELECT
            @CourseId = CourseId,
            @CurrentStatus = Status,
            @PaymentStatus = PaymentStatus,
            @EnrollmentDate = EnrollmentDate
        FROM Enrollments
        WHERE EnrollmentId = @EnrollmentId AND StudentId = @StudentId;

        IF @CourseId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Không tìm thấy đăng ký môn học', 16, 1);
            RETURN -1;
        END

        -- Check if already cancelled
        IF @CurrentStatus = N'Cancelled'
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Môn học đã được hủy trước đó', 16, 1);
            RETURN -2;
        END

        -- Check if already paid
        IF @PaymentStatus = 'Paid'
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Không thể hủy môn đã thanh toán học phí', 16, 1);
            RETURN -3;
        END

        -- Check cancel deadline
        SELECT @CancelDeadlineDays = CAST(SettingValue AS INT)
        FROM PaymentSettings
        WHERE SettingKey = 'CANCEL_DEADLINE_DAYS';

        IF DATEDIFF(DAY, @EnrollmentDate, GETDATE()) > @CancelDeadlineDays
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Đã quá thời hạn cho phép hủy môn', 16, 1);
            RETURN -4;
        END

        -- Update enrollment status
        UPDATE Enrollments
        SET Status = N'Cancelled',
            CancelledDate = GETDATE()
        WHERE EnrollmentId = @EnrollmentId;

        -- Log the cancellation
        INSERT INTO EnrollmentAuditLog (EnrollmentId, StudentId, CourseId, Action, OldStatus, NewStatus, Reason, CreatedBy, CreatedAt)
        VALUES (@EnrollmentId, @StudentId, @CourseId, 'Cancelled', @CurrentStatus, N'Cancelled', @Reason, @StudentId, GETDATE());

        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN -99;
    END CATCH
END
GO

PRINT N'✓ Đã tạo stored procedure sp_CancelEnrollment';
GO

-- =============================================
-- 7. CREATE STORED PROCEDURE: sp_CreatePayment
-- =============================================
IF OBJECT_ID('sp_CreatePayment', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreatePayment;
GO

CREATE PROCEDURE sp_CreatePayment
    @StudentId INT,
    @Semester NVARCHAR(20),
    @AcademicYear INT,
    @PaymentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @TotalAmount DECIMAL(18,2);
        DECLARE @TuitionPerCredit DECIMAL(18,2);
        DECLARE @Description NVARCHAR(500);
        DECLARE @StudentCode NVARCHAR(20);

        -- Get tuition per credit
        SELECT @TuitionPerCredit = CAST(SettingValue AS DECIMAL(18,2))
        FROM PaymentSettings
        WHERE SettingKey = 'TUITION_PER_CREDIT';

        -- Get student code
        SELECT @StudentCode = StudentCode FROM Students WHERE StudentId = @StudentId;

        -- Calculate total amount from unpaid enrollments
        SELECT @TotalAmount = SUM(c.Credits * @TuitionPerCredit)
        FROM Enrollments e
        INNER JOIN Courses c ON e.CourseId = c.CourseId
        WHERE e.StudentId = @StudentId
        AND c.Semester = @Semester
        AND c.AcademicYear = @AcademicYear
        AND e.Status = N'Enrolled'
        AND e.PaymentStatus = 'Unpaid';

        IF @TotalAmount IS NULL OR @TotalAmount <= 0
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Không có môn học nào cần thanh toán', 16, 1);
            RETURN -1;
        END

        -- Check if payment already exists
        IF EXISTS (SELECT 1 FROM Payments
                   WHERE StudentId = @StudentId
                   AND Semester = @Semester
                   AND AcademicYear = @AcademicYear
                   AND Status = 'Pending')
        BEGIN
            -- Return existing payment
            SELECT @PaymentId = PaymentId
            FROM Payments
            WHERE StudentId = @StudentId
            AND Semester = @Semester
            AND AcademicYear = @AcademicYear
            AND Status = 'Pending';

            COMMIT TRANSACTION;
            RETURN 0;
        END

        -- Create description
        SET @Description = N'Học phí ' + @StudentCode + ' ' + @Semester;

        -- Insert payment
        INSERT INTO Payments (StudentId, Semester, AcademicYear, TotalAmount, Description, Status, CreatedAt)
        VALUES (@StudentId, @Semester, @AcademicYear, @TotalAmount, @Description, 'Pending', GETDATE());

        SET @PaymentId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN -99;
    END CATCH
END
GO

PRINT N'✓ Đã tạo stored procedure sp_CreatePayment';
GO

-- =============================================
-- 8. CREATE STORED PROCEDURE: sp_ConfirmPayment
-- =============================================
IF OBJECT_ID('sp_ConfirmPayment', 'P') IS NOT NULL
    DROP PROCEDURE sp_ConfirmPayment;
GO

CREATE PROCEDURE sp_ConfirmPayment
    @PaymentId INT,
    @TransactionId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @StudentId INT, @Semester NVARCHAR(20), @AcademicYear INT;

        -- Get payment info
        SELECT @StudentId = StudentId, @Semester = Semester, @AcademicYear = AcademicYear
        FROM Payments
        WHERE PaymentId = @PaymentId;

        IF @StudentId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Không tìm thấy thông tin thanh toán', 16, 1);
            RETURN -1;
        END

        -- Update payment status
        UPDATE Payments
        SET Status = 'Paid',
            PaidAt = GETDATE(),
            TransactionId = @TransactionId
        WHERE PaymentId = @PaymentId;

        -- Update all enrollments to Paid
        UPDATE Enrollments
        SET PaymentStatus = 'Paid'
        WHERE StudentId = @StudentId
        AND CourseId IN (
            SELECT CourseId FROM Courses
            WHERE Semester = @Semester
            AND AcademicYear = @AcademicYear
        )
        AND Status = N'Enrolled'
        AND PaymentStatus = 'Unpaid';

        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN -99;
    END CATCH
END
GO

PRINT N'✓ Đã tạo stored procedure sp_ConfirmPayment';
GO

PRINT N'';
PRINT N'========================================';
PRINT N'✅ HOÀN TẤT CÀI ĐẶT PAYMENT SYSTEM';
PRINT N'========================================';
GO
IF OBJECT_ID('sp_CreatePayment', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreatePayment;
GO

CREATE PROCEDURE sp_CreatePayment
    @StudentId INT,
    @Semester NVARCHAR(20),
    @PaymentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @TotalAmount DECIMAL(18,2);
        DECLARE @TuitionPerCredit DECIMAL(18,2);
        DECLARE @Description NVARCHAR(500);
        DECLARE @StudentCode NVARCHAR(20);
        DECLARE @AcademicYear INT;

        -- Extract academic year from semester (e.g., "HK1 2025-2026" -> 2025)
        SET @AcademicYear = CAST(SUBSTRING(@Semester, CHARINDEX(' ', @Semester) + 1, 4) AS INT);

        -- Get tuition per credit
        SELECT @TuitionPerCredit = CAST(SettingValue AS DECIMAL(18,2))
        FROM PaymentSettings
        WHERE SettingKey = 'TUITION_PER_CREDIT';

        -- Get student code
        SELECT @StudentCode = StudentCode FROM Students WHERE StudentId = @StudentId;

        -- Calculate total amount from unpaid enrollments
        SELECT @TotalAmount = SUM(c.Credits * @TuitionPerCredit)
        FROM Enrollments e
        INNER JOIN Courses c ON e.CourseId = c.CourseId
        WHERE e.StudentId = @StudentId
        AND c.Semester = @Semester
        AND c.AcademicYear = @AcademicYear
        AND e.Status = N'Enrolled'
        AND e.PaymentStatus = 'Unpaid';

        IF @TotalAmount IS NULL OR @TotalAmount <= 0
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Không có môn học nào cần thanh toán', 16, 1);
            RETURN -1;
        END

        -- Check if payment already exists
        IF EXISTS (SELECT 1 FROM Payments
                   WHERE StudentId = @StudentId
                   AND Semester = @Semester
                   AND AcademicYear = @AcademicYear
                   AND Status = 'Pending')
        BEGIN
            -- Return existing payment
            SELECT @PaymentId = PaymentId
            FROM Payments
            WHERE StudentId = @StudentId
            AND Semester = @Semester
            AND AcademicYear = @AcademicYear
            AND Status = 'Pending';

            COMMIT TRANSACTION;
            RETURN 0;
        END

        -- Create description
        SET @Description = N'Học phí ' + @StudentCode + ' ' + @Semester;

        -- Insert payment
        INSERT INTO Payments (StudentId, Semester, AcademicYear, TotalAmount, Description, Status, CreatedAt)
        VALUES (@StudentId, @Semester, @AcademicYear, @TotalAmount, @Description, 'Pending', GETDATE());

        SET @PaymentId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN -99;
    END CATCH
END
GO

PRINT N'✓ Đã cập nhật stored procedure sp_CreatePayment';
GO

-- Drop và tạo lại sp_ConfirmPayment
IF OBJECT_ID('sp_ConfirmPayment', 'P') IS NOT NULL
    DROP PROCEDURE sp_ConfirmPayment;
GO

CREATE PROCEDURE sp_ConfirmPayment
    @PaymentId INT,
    @TransactionId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @StudentId INT, @Semester NVARCHAR(20), @AcademicYear INT;

        -- Get payment info
        SELECT @StudentId = StudentId, @Semester = Semester
        FROM Payments
        WHERE PaymentId = @PaymentId;

        IF @StudentId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Không tìm thấy thông tin thanh toán', 16, 1);
            RETURN -1;
        END

        -- Update payment status
        UPDATE Payments
        SET Status = 'Paid',
            PaidAt = GETDATE(),
            TransactionId = @TransactionId
        WHERE PaymentId = @PaymentId;

        -- Determine academic year from semester and update all enrollments to Paid for this semester
        SET @AcademicYear = CAST(SUBSTRING(@Semester, CHARINDEX(' ', @Semester) + 1, 4) AS INT);
        -- Update all enrollments to Paid for this semester
        UPDATE Enrollments
        SET PaymentStatus = 'Paid'
        WHERE StudentId = @StudentId
        AND CourseId IN (
            SELECT CourseId FROM Courses
            WHERE Semester = @Semester
            AND AcademicYear = @AcademicYear
        )
        AND Status = N'Enrolled'
        AND PaymentStatus = 'Unpaid';

        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN -99;
    END CATCH
END
GO

PRINT N'✓ Đã cập nhật stored procedure sp_ConfirmPayment';
GO

PRINT N'';
PRINT N'========================================';
PRINT N'✅ HOÀN TẤT CẬP NHẬT PAYMENT SYSTEM';
PRINT N'========================================';
GO
