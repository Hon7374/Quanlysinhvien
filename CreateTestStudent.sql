-- Create test student account
-- Username: student01, Password: student123

-- First, check if user exists
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'student01')
BEGIN
    -- Insert user
    INSERT INTO Users (Username, Password, FullName, Email, Phone, Role, IsActive)
    VALUES ('student01', 'student123', N'Nguyễn Văn A', 'student01@example.com', '0901234567', 3, 1)

    PRINT 'Created user: student01'
END
ELSE
BEGIN
    -- Update password if user exists
    UPDATE Users SET Password = 'student123', IsActive = 1 WHERE Username = 'student01'
    PRINT 'Updated user: student01'
END

-- Get the UserId
DECLARE @UserId INT
SELECT @UserId = UserId FROM Users WHERE Username = 'student01'

-- Check if student record exists
IF NOT EXISTS (SELECT * FROM Students WHERE UserId = @UserId)
BEGIN
    -- Insert student record
    INSERT INTO Students (UserId, StudentCode, Class, Major, AcademicYear, Status)
    VALUES (@UserId, 'SV2024001', N'CNTT-K18', N'Công nghệ thông tin', 2024, N'Đang học')

    PRINT 'Created student record for student01'
END
ELSE
BEGIN
    PRINT 'Student record already exists'
END

-- Display the account info
SELECT
    u.Username,
    u.Password,
    u.FullName,
    u.Role,
    s.StudentCode,
    s.Class,
    s.Major
FROM Users u
LEFT JOIN Students s ON u.UserId = s.UserId
WHERE u.Username = 'student01'
