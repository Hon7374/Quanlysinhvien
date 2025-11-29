-- =============================================
-- Student Management System - Database Creation
-- Optimized with indexes, constraints, and all necessary columns
-- =============================================

USE master;
GO

-- Drop database if exists (for clean setup)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentManagementDB')
BEGIN
    ALTER DATABASE StudentManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StudentManagementDB;
END
GO

-- Create database
CREATE DATABASE StudentManagementDB;
GO

USE StudentManagementDB;
GO

-- =============================================
-- TABLE 1: Users
-- =============================================
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    Phone NVARCHAR(20),
    Role INT NOT NULL CHECK (Role IN (1, 2, 3)), -- 1=Admin, 2=Teacher, 3=Student
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);

-- Create index on Role for faster filtering
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_Username ON Users(Username);
GO

-- =============================================
-- TABLE 2: Teachers
-- =============================================
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
-- TABLE 4: Semesters
-- =============================================
CREATE TABLE Semesters (
    SemesterId INT PRIMARY KEY IDENTITY(1,1),
    SemesterCode NVARCHAR(20) UNIQUE NOT NULL,
    SemesterName NVARCHAR(100) NOT NULL,
    AcademicYear INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(50) DEFAULT N'Sắp diễn ra',
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_Semester_Dates CHECK (EndDate > StartDate)
);

CREATE INDEX IX_Semesters_AcademicYear ON Semesters(AcademicYear);
CREATE INDEX IX_Semesters_Status ON Semesters(Status);
GO

-- =============================================
-- TABLE 5: Courses
-- =============================================
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
-- TABLE 6: Enrollments
-- =============================================
CREATE TABLE Enrollments (
    EnrollmentId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrollmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT N'Đang học',
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
-- TABLE 7: Grades
-- =============================================
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
-- VIEWS for easier querying
-- =============================================

-- View: All students with user info
CREATE VIEW vw_Students AS
SELECT
    s.StudentId,
    s.StudentCode,
    u.Username,
    u.FullName,
    u.Email,
    u.Phone,
    s.DateOfBirth,
    s.Gender,
    s.Address,
    s.Class,
    s.Major,
    s.AcademicYear,
    s.GPA,
    s.Status,
    s.CreatedAt,
    u.IsActive
FROM Students s
INNER JOIN Users u ON s.UserId = u.UserId;
GO

-- View: All teachers with user info
CREATE VIEW vw_Teachers AS
SELECT
    t.TeacherId,
    t.TeacherCode,
    u.Username,
    u.FullName,
    u.Email,
    u.Phone,
    t.Department,
    t.Degree,
    t.Specialization,
    t.HireDate,
    t.Status,
    u.IsActive
FROM Teachers t
INNER JOIN Users u ON t.UserId = u.UserId;
GO

-- View: Course enrollments with student and course details
CREATE VIEW vw_Enrollments AS
SELECT
    e.EnrollmentId,
    e.StudentId,
    s.StudentCode,
    u.FullName AS StudentName,
    e.CourseId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    t.FullName AS TeacherName,
    e.EnrollmentDate,
    e.Status AS EnrollmentStatus,
    g.MidtermScore,
    g.FinalScore,
    g.TotalScore,
    g.LetterGrade
FROM Enrollments e
INNER JOIN Students s ON e.StudentId = s.StudentId
INNER JOIN Users u ON s.UserId = u.UserId
INNER JOIN Courses c ON e.CourseId = c.CourseId
LEFT JOIN Teachers tc ON c.TeacherId = tc.TeacherId
LEFT JOIN Users t ON tc.UserId = t.UserId
LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId;
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

-- Procedure: Get student transcript
CREATE PROCEDURE sp_GetStudentTranscript
    @StudentId INT
AS
BEGIN
    SELECT
        c.CourseCode,
        c.CourseName,
        c.Credits,
        c.Semester,
        g.MidtermScore,
        g.FinalScore,
        g.TotalScore,
        g.LetterGrade,
        e.Status
    FROM Enrollments e
    INNER JOIN Courses c ON e.CourseId = c.CourseId
    LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
    WHERE e.StudentId = @StudentId
    ORDER BY c.Semester DESC, c.CourseCode;
END
GO

-- Procedure: Get course statistics
CREATE PROCEDURE sp_GetCourseStatistics
    @CourseId INT
AS
BEGIN
    SELECT
        COUNT(*) AS TotalStudents,
        COUNT(g.GradeId) AS GradedStudents,
        AVG(g.TotalScore) AS AverageScore,
        SUM(CASE WHEN g.TotalScore >= 5.0 THEN 1 ELSE 0 END) AS PassedStudents,
        SUM(CASE WHEN g.LetterGrade = 'A' THEN 1 ELSE 0 END) AS GradeA,
        SUM(CASE WHEN g.LetterGrade = 'B' THEN 1 ELSE 0 END) AS GradeB,
        SUM(CASE WHEN g.LetterGrade = 'C' THEN 1 ELSE 0 END) AS GradeC,
        SUM(CASE WHEN g.LetterGrade = 'D' THEN 1 ELSE 0 END) AS GradeD,
        SUM(CASE WHEN g.LetterGrade = 'F' THEN 1 ELSE 0 END) AS GradeF
    FROM Enrollments e
    LEFT JOIN Grades g ON e.EnrollmentId = g.EnrollmentId
    WHERE e.CourseId = @CourseId;
END
GO

-- =============================================
-- TRIGGERS
-- =============================================

-- Trigger: Update student GPA when grades change
CREATE TRIGGER trg_UpdateStudentGPA
ON Grades
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Update GPA for affected students
    UPDATE s
    SET GPA = ISNULL((
        SELECT AVG(g.TotalScore) / 2.5  -- Convert 10-scale to 4-scale
        FROM Grades g
        INNER JOIN Enrollments e ON g.EnrollmentId = e.EnrollmentId
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
-- SUMMARY
-- =============================================

PRINT '========================================';
PRINT 'Database Created Successfully!';
PRINT '========================================';
PRINT 'Tables: 7';
PRINT '  - Users';
PRINT '  - Teachers';
PRINT '  - Students';
PRINT '  - Semesters';
PRINT '  - Courses';
PRINT '  - Enrollments';
PRINT '  - Grades';
PRINT '';
PRINT 'Views: 3';
PRINT '  - vw_Students';
PRINT '  - vw_Teachers';
PRINT '  - vw_Enrollments';
PRINT '';
PRINT 'Stored Procedures: 2';
PRINT '  - sp_GetStudentTranscript';
PRINT '  - sp_GetCourseStatistics';
PRINT '';
PRINT 'Triggers: 2';
PRINT '  - trg_UpdateStudentGPA';
PRINT '  - trg_Grades_UpdatedAt';
PRINT '';
PRINT 'Indexes: Created on all key columns';
PRINT 'Constraints: Foreign keys, checks, unique';
PRINT '========================================';
PRINT '';
PRINT 'Next step: Run SampleData.sql to insert test data';
PRINT '========================================';
GO
