-- =============================================
-- FIX ENROLLMENT CONSTRAINT
-- Cho phép đăng ký lại môn đã hủy
-- =============================================

USE StudentManagementDB;
GO

PRINT N'Bắt đầu sửa constraint Enrollments...';
GO

-- =============================================
-- 1. XÓA UNIQUE CONSTRAINT CŨ
-- =============================================
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Student_Course' AND object_id = OBJECT_ID('Enrollments'))
BEGIN
    PRINT N'Đang xóa constraint cũ UQ_Student_Course...';
    ALTER TABLE Enrollments DROP CONSTRAINT UQ_Student_Course;
    PRINT N'✓ Đã xóa constraint cũ';
END
ELSE
BEGIN
    PRINT N'⚠ Constraint UQ_Student_Course không tồn tại';
END
GO

-- =============================================
-- 2. TẠO UNIQUE INDEX MỚI - CHỈ ÁP DỤNG CHO Status = 'Enrolled'
-- =============================================
-- Unique filtered index chỉ enforce uniqueness khi Status = 'Enrolled'
-- Cho phép nhiều bản ghi Cancelled cho cùng StudentId, CourseId

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Student_Course_Enrolled' AND object_id = OBJECT_ID('Enrollments'))
BEGIN
    PRINT N'Đang tạo unique filtered index mới...';

    CREATE UNIQUE INDEX UQ_Student_Course_Enrolled
    ON Enrollments(StudentId, CourseId)
    WHERE Status = N'Enrolled';

    PRINT N'✓ Đã tạo unique filtered index UQ_Student_Course_Enrolled';
    PRINT N'  → Sinh viên chỉ có thể đăng ký một môn một lần (khi Status = Enrolled)';
    PRINT N'  → Sinh viên có thể đăng ký lại môn đã hủy (Status = Cancelled)';
END
ELSE
BEGIN
    PRINT N'⚠ Index UQ_Student_Course_Enrolled đã tồn tại';
END
GO

PRINT N'';
PRINT N'========================================';
PRINT N'✅ HOÀN TẤT SỬA CONSTRAINT';
PRINT N'========================================';
PRINT N'Giờ sinh viên có thể:';
PRINT N'  - Đăng ký môn học';
PRINT N'  - Hủy môn học (Status → Cancelled)';
PRINT N'  - Đăng ký lại môn đã hủy (tạo enrollment mới)';
PRINT N'';
GO
