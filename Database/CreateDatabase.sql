-- Create Database
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentManagementDB')
BEGIN
    CREATE DATABASE StudentManagementDB;
END
GO

USE StudentManagementDB;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) UNIQUE NOT NULL,
        Password NVARCHAR(255) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100),
        Phone NVARCHAR(20),
        Role INT NOT NULL, -- 1: Admin, 2: Teacher, 3: Student
        IsActive BIT DEFAULT 1,
        CreatedDate DATETIME DEFAULT GETDATE(),
        LastLogin DATETIME NULL
    );
END
GO

-- Create Students table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE Students (
        StudentId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT FOREIGN KEY REFERENCES Users(UserId) ON DELETE CASCADE,
        StudentCode NVARCHAR(20) UNIQUE NOT NULL,
        DateOfBirth DATE,
        Gender NVARCHAR(10),
        Address NVARCHAR(200),
        Class NVARCHAR(50),
        Major NVARCHAR(100),
        AcademicYear INT,
        GPA DECIMAL(3,2) DEFAULT 0.00
    );
END
GO

-- Create Teachers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
BEGIN
    CREATE TABLE Teachers (
        TeacherId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT FOREIGN KEY REFERENCES Users(UserId) ON DELETE CASCADE,
        TeacherCode NVARCHAR(20) UNIQUE NOT NULL,
        Department NVARCHAR(100),
        Degree NVARCHAR(50),
        Specialization NVARCHAR(100),
        HireDate DATE DEFAULT GETDATE()
    );
END
GO

-- Create Courses table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
BEGIN
    CREATE TABLE Courses (
        CourseId INT PRIMARY KEY IDENTITY(1,1),
        CourseCode NVARCHAR(20) UNIQUE NOT NULL,
        CourseName NVARCHAR(100) NOT NULL,
        Credits INT NOT NULL,
        Description NVARCHAR(500),
        TeacherId INT FOREIGN KEY REFERENCES Teachers(TeacherId) ON DELETE SET NULL,
        Semester NVARCHAR(20),
        AcademicYear INT,
        MaxStudents INT DEFAULT 50,
        IsActive BIT DEFAULT 1
    );
END
GO

-- Create Enrollments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
BEGIN
    CREATE TABLE Enrollments (
        EnrollmentId INT PRIMARY KEY IDENTITY(1,1),
        StudentId INT FOREIGN KEY REFERENCES Students(StudentId) ON DELETE CASCADE,
        CourseId INT FOREIGN KEY REFERENCES Courses(CourseId) ON DELETE CASCADE,
        EnrollmentDate DATETIME DEFAULT GETDATE(),
        Status NVARCHAR(20) DEFAULT 'Enrolled', -- Enrolled, Completed, Dropped
        UNIQUE(StudentId, CourseId)
    );
END
GO

-- Create Grades table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Grades')
BEGIN
    CREATE TABLE Grades (
        GradeId INT PRIMARY KEY IDENTITY(1,1),
        EnrollmentId INT FOREIGN KEY REFERENCES Enrollments(EnrollmentId) ON DELETE CASCADE,
        MidtermScore DECIMAL(4,2),
        FinalScore DECIMAL(4,2),
        AssignmentScore DECIMAL(4,2),
        TotalScore DECIMAL(4,2),
        LetterGrade NVARCHAR(5),
        GradedDate DATETIME,
        Comments NVARCHAR(500)
    );
END
GO

-- Insert default admin user (username: admin, password: admin123)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Password, FullName, Email, Phone, Role, IsActive)
    VALUES ('admin', 'admin123', N'Quản trị viên', 'admin@school.edu.vn', '0123456789', 1, 1);
END
GO

-- Insert sample teacher
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'teacher01')
BEGIN
    INSERT INTO Users (Username, Password, FullName, Email, Phone, Role, IsActive)
    VALUES ('teacher01', 'teacher123', N'Nguyễn Văn Giảng', 'nguyenvangiang@school.edu.vn', '0987654321', 2, 1);

    DECLARE @TeacherUserId INT = SCOPE_IDENTITY();

    INSERT INTO Teachers (UserId, TeacherCode, Department, Degree, Specialization)
    VALUES (@TeacherUserId, 'GV001', N'Khoa Công nghệ thông tin', N'Tiến sĩ', N'Khoa học máy tính');
END
GO

-- Insert sample student
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'student01')
BEGIN
    INSERT INTO Users (Username, Password, FullName, Email, Phone, Role, IsActive)
    VALUES ('student01', 'student123', N'Trần Thị Học', 'tranthihoc@student.edu.vn', '0912345678', 3, 1);

    DECLARE @StudentUserId INT = SCOPE_IDENTITY();

    INSERT INTO Students (UserId, StudentCode, DateOfBirth, Gender, Address, Class, Major, AcademicYear, GPA)
    VALUES (@StudentUserId, 'SV001', '2003-05-15', N'Nữ', N'Hà Nội', 'CNTT-K18', N'Công nghệ thông tin', 2024, 3.50);
END
GO

-- Insert sample courses
IF NOT EXISTS (SELECT * FROM Courses WHERE CourseCode = 'IT101')
BEGIN
    DECLARE @TeacherId INT = (SELECT TOP 1 TeacherId FROM Teachers);

    INSERT INTO Courses (CourseCode, CourseName, Credits, Description, TeacherId, Semester, AcademicYear)
    VALUES
    ('IT101', N'Lập trình căn bản', 3, N'Học lập trình C/C++ cơ bản', @TeacherId, 'HK1', 2024),
    ('IT102', N'Cấu trúc dữ liệu và giải thuật', 4, N'Các cấu trúc dữ liệu và thuật toán cơ bản', @TeacherId, 'HK1', 2024),
    ('IT201', N'Cơ sở dữ liệu', 3, N'Thiết kế và quản lý cơ sở dữ liệu', @TeacherId, 'HK2', 2024),
    ('IT202', N'Lập trình hướng đối tượng', 4, N'OOP với C# và Java', @TeacherId, 'HK2', 2024);
END
GO

PRINT 'Database created successfully!';
