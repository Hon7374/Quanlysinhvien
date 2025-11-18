-- =============================================
-- Create Schedule Table for Course Timetable
-- =============================================

USE StudentManagementDB;
GO

-- Check if Schedules table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Schedules')
BEGIN
    CREATE TABLE Schedules (
        ScheduleId INT PRIMARY KEY IDENTITY(1,1),
        CourseId INT NOT NULL,
        DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6), -- 0=Monday, 6=Sunday
        TimeSlot INT NOT NULL CHECK (TimeSlot BETWEEN 0 AND 5), -- 0-5 representing time periods
        Room NVARCHAR(50),
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Schedules_Courses FOREIGN KEY (CourseId)
            REFERENCES Courses(CourseId) ON DELETE CASCADE,
        CONSTRAINT UQ_Schedule UNIQUE(CourseId, DayOfWeek, TimeSlot)
    );

    CREATE INDEX IX_Schedules_CourseId ON Schedules(CourseId);
    CREATE INDEX IX_Schedules_DayOfWeek ON Schedules(DayOfWeek);

    PRINT 'Schedules table created successfully!';
END
ELSE
BEGIN
    PRINT 'Schedules table already exists.';
END
GO

-- Insert sample schedules for existing courses
-- This assumes you have courses with CourseId 1-10 in your database

DECLARE @CourseCount INT;
SELECT @CourseCount = COUNT(*) FROM Courses;

IF @CourseCount > 0 AND NOT EXISTS (SELECT * FROM Schedules)
BEGIN
    -- Get existing course IDs
    DECLARE @CourseId INT;
    DECLARE @Counter INT = 0;

    DECLARE course_cursor CURSOR FOR
    SELECT TOP 10 CourseId FROM Courses WHERE IsActive = 1;

    OPEN course_cursor;
    FETCH NEXT FROM course_cursor INTO @CourseId;

    WHILE @@FETCH_STATUS = 0 AND @Counter < 10
    BEGIN
        -- Assign different days and time slots for variety
        DECLARE @DayOfWeek INT = @Counter % 5; -- Monday to Friday
        DECLARE @TimeSlot INT = @Counter % 6; -- Different time slots

        INSERT INTO Schedules (CourseId, DayOfWeek, TimeSlot, Room, StartTime, EndTime)
        VALUES (
            @CourseId,
            @DayOfWeek,
            @TimeSlot,
            'P' + CAST(101 + @Counter AS NVARCHAR(10)),
            CASE @TimeSlot
                WHEN 0 THEN '07:00:00'
                WHEN 1 THEN '08:45:00'
                WHEN 2 THEN '10:30:00'
                WHEN 3 THEN '13:00:00'
                WHEN 4 THEN '14:45:00'
                WHEN 5 THEN '16:30:00'
            END,
            CASE @TimeSlot
                WHEN 0 THEN '08:30:00'
                WHEN 1 THEN '10:15:00'
                WHEN 2 THEN '12:00:00'
                WHEN 3 THEN '14:30:00'
                WHEN 4 THEN '16:15:00'
                WHEN 5 THEN '18:00:00'
            END
        );

        SET @Counter = @Counter + 1;
        FETCH NEXT FROM course_cursor INTO @CourseId;
    END

    CLOSE course_cursor;
    DEALLOCATE course_cursor;

    PRINT 'Sample schedules inserted successfully!';
END
GO

-- View to easily query schedules
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_CourseSchedules')
    DROP VIEW vw_CourseSchedules;
GO

CREATE VIEW vw_CourseSchedules AS
SELECT
    s.ScheduleId,
    c.CourseId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    s.DayOfWeek,
    CASE s.DayOfWeek
        WHEN 0 THEN N'Thứ 2'
        WHEN 1 THEN N'Thứ 3'
        WHEN 2 THEN N'Thứ 4'
        WHEN 3 THEN N'Thứ 5'
        WHEN 4 THEN N'Thứ 6'
        WHEN 5 THEN N'Thứ 7'
        WHEN 6 THEN N'Chủ nhật'
    END AS DayName,
    s.TimeSlot,
    CAST(s.StartTime AS VARCHAR(5)) + ' - ' + CAST(s.EndTime AS VARCHAR(5)) AS TimeRange,
    s.Room,
    u.FullName AS TeacherName
FROM Schedules s
INNER JOIN Courses c ON s.CourseId = c.CourseId
LEFT JOIN Teachers t ON c.TeacherId = t.TeacherId
LEFT JOIN Users u ON t.UserId = u.UserId;
GO

PRINT '======================================';
PRINT 'Schedule system setup completed!';
PRINT 'Table: Schedules';
PRINT 'View: vw_CourseSchedules';
PRINT '======================================';
GO
