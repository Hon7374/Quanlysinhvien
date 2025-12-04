USE StudentManagementDB
GO

-- ĐỔI DÒNG NÀY THÀNH MÃ SỐ SINH VIÊN BẠN MUỐN XÓA SẠCH
DECLARE @StudentId INT = 1;   -- ← SỬA Ở ĐÂY (ví dụ: 1001, 999, v.v.)

PRINT N'';
PRINT N'══════════════════════════════════════════════════════════';
PRINT N'          XÓA SẠCH TOÀN BỘ MÔN HỌC ĐÃ ĐĂNG KÝ';
PRINT N'          StudentId = ' + CAST(@StudentId AS NVARCHAR(10));
PRINT N'          KỂ CẢ MÔN ĐÃ THANH TOÁN';
PRINT N'══════════════════════════════════════════════════════════';
PRINT N'';

-- CHỈ CẬP NHẬT 2 CỘT CHẮC CHẮN TỒN TẠI
UPDATE Enrollments
SET    Status       = N'Cancelled',
       CancelledDate = GETDATE()
WHERE  StudentId = @StudentId
  AND  Status = N'Enrolled';

-- Báo kết quả
DECLARE @Count INT = @@ROWCOUNT;

IF @Count > 0
BEGIN
    PRINT N'ĐÃ HỦY THÀNH CÔNG ' + CAST(@Count AS NVARCHAR(10)) + N' môn học!';
    PRINT N'Sinh viên này giờ trắng tinh – có thể đăng ký lại từ đầu';
END
ELSE
BEGIN
    PRINT N'Không có môn nào đang Enrolled để hủy (hoặc StudentId sai)';
END

PRINT N'';
PRINT N'HOÀN TẤT!';
GO