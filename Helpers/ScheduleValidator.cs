using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace StudentManagement.Helpers
{
    /// <summary>
    /// Validates schedule conflicts and session boundaries
    /// </summary>
    public static class ScheduleValidator
    {
        // Session definitions: Morning, Afternoon, Evening
        public static readonly int[] SessionStartSlots = { 0, 5, 10 }; // Start slot for each session
        public static readonly int[] SessionEndSlots = { 4, 9, 13 };   // End slot for each session
        public static readonly string[] SessionNames = { "Sáng", "Chiều", "Tối" };

        /// <summary>
        /// Gets the session index (0=Morning, 1=Afternoon, 2=Evening) for a given time slot
        /// </summary>
        public static int GetSessionIndex(int timeSlot)
        {
            if (timeSlot >= SessionStartSlots[0] && timeSlot <= SessionEndSlots[0])
                return 0; // Morning
            if (timeSlot >= SessionStartSlots[1] && timeSlot <= SessionEndSlots[1])
                return 1; // Afternoon
            if (timeSlot >= SessionStartSlots[2] && timeSlot <= SessionEndSlots[2])
                return 2; // Evening

            return -1; // Invalid slot
        }

        /// <summary>
        /// Validates that all time slots are within the same session
        /// </summary>
        public static bool ValidateSessionRange(int startSlot, int endSlot, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Check if slots are valid
            if (startSlot < 0 || startSlot > 13 || endSlot < 0 || endSlot > 13)
            {
                // Note: internal slots are 0..13 while displayed slots are 1..14
                errorMessage = "Tiết học không hợp lệ (mã nội bộ phải từ 0-13, tương đương Tiết 1-14).";
                return false;
            }

            if (startSlot > endSlot)
            {
                errorMessage = "Tiết bắt đầu không được lớn hơn tiết kết thúc.";
                return false;
            }

            // Get sessions for start and end slots
            int startSession = GetSessionIndex(startSlot);
            int endSession = GetSessionIndex(endSlot);

            if (startSession == -1 || endSession == -1)
            {
                errorMessage = "Tiết học không thuộc ca học nào (Sáng/Chiều/Tối).";
                return false;
            }

            // Check if they're in the same session
            if (startSession != endSession)
            {
                errorMessage = $"Lịch học không được kéo dài qua 2 ca: Tiết {startSlot + 1} thuộc ca {SessionNames[startSession]}, " +
                             $"tiết {endSlot + 1} thuộc ca {SessionNames[endSession]}. " +
                             $"Vui lòng chọn tiết trong cùng một ca học.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if two time ranges have conflict
        /// </summary>
        public static bool HasTimeConflict(int startSlot1, int endSlot1, int startSlot2, int endSlot2)
        {
            // Two ranges [a,b] and [c,d] overlap if: max(a,c) <= min(b,d)
            return Math.Max(startSlot1, startSlot2) <= Math.Min(endSlot1, endSlot2);
        }

        /// <summary>
        /// Detects schedule conflicts for a course on a specific day
        /// Returns list of conflicting courses with details
        /// </summary>
        public static List<ScheduleConflict> DetectConflicts(
            int dayOfWeek,
            int startSlot,
            int endSlot,
            DataTable existingSchedules,
            int? excludeCourseId = null)
        {
            var conflicts = new List<ScheduleConflict>();

            foreach (DataRow schedule in existingSchedules.Rows)
            {
                if (schedule["DayOfWeek"] == DBNull.Value || schedule["TimeSlot"] == DBNull.Value)
                    continue;

                int scheduleDayOfWeek = Convert.ToInt32(schedule["DayOfWeek"]);
                if (scheduleDayOfWeek != dayOfWeek)
                    continue;

                int courseId = Convert.ToInt32(schedule["CourseId"]);
                if (excludeCourseId.HasValue && courseId == excludeCourseId.Value)
                    continue;

                // Build all time slots for this course on this day
                var courseSlots = existingSchedules.Select($"CourseId = {courseId} AND DayOfWeek = {dayOfWeek}")
                    .Select(r => Convert.ToInt32(r["TimeSlot"]))
                    .OrderBy(x => x)
                    .ToList();

                if (courseSlots.Count == 0)
                    continue;

                // Iterate over contiguous ranges for this course (only contiguous sequences are considered blocks)
                int idx = 0;
                while (idx < courseSlots.Count)
                {
                    int scheduleStartSlot = courseSlots[idx];
                    int scheduleEndSlot = scheduleStartSlot;
                    idx++;
                    while (idx < courseSlots.Count && courseSlots[idx] == scheduleEndSlot + 1)
                    {
                        scheduleEndSlot = courseSlots[idx];
                        idx++;
                    }

                    // Check for conflict between the candidate range and this contiguous block
                    if (HasTimeConflict(startSlot, endSlot, scheduleStartSlot, scheduleEndSlot))
                    {
                        // find an example row for course info
                        DataRow sampleRow = existingSchedules.Select($"CourseId = {courseId} AND DayOfWeek = {dayOfWeek} AND TimeSlot = {scheduleStartSlot}").FirstOrDefault();

                        conflicts.Add(new ScheduleConflict
                        {
                            CourseId = courseId,
                            CourseCode = sampleRow?["CourseCode"]?.ToString() ?? "N/A",
                            CourseName = sampleRow?["CourseName"]?.ToString() ?? "N/A",
                            DayOfWeek = dayOfWeek,
                            StartSlot = scheduleStartSlot,
                            EndSlot = scheduleEndSlot,
                            Session = GetSessionIndex(scheduleStartSlot)
                        });

                        break; // Only track first conflict per course
                    }
                }
            }

            return conflicts;
        }

        /// <summary>
        /// Validates a course schedule before saving
        /// </summary>
        public static bool ValidateCourseSchedule(
            int courseId,
            List<Tuple<int, int>> daySlotPairs, // List of (dayOfWeek, timeSlot)
            DataTable existingSchedules,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            if (daySlotPairs == null || daySlotPairs.Count == 0)
            {
                errorMessage = "Không có lịch học nào được chọn.";
                return false;
            }

            // Group by day to validate each day
            var byDay = daySlotPairs.GroupBy(p => p.Item1);

            foreach (var dayGroup in byDay)
            {
                int dayOfWeek = dayGroup.Key;
                var slots = dayGroup.Select(p => p.Item2).OrderBy(s => s).ToList();

                if (slots.Count == 0)
                    continue;

                int startSlot = slots.First();
                int endSlot = slots.Last();

                // Validate session range
                if (!ValidateSessionRange(startSlot, endSlot, out string sessionError))
                {
                    errorMessage = $"Ngày {GetDayName(dayOfWeek)}: {sessionError}";
                    return false;
                }

                // Check for conflicts with existing schedules
                var conflicts = DetectConflicts(dayOfWeek, startSlot, endSlot, existingSchedules, courseId);
                if (conflicts.Count > 0)
                {
                    var conflict = conflicts[0];
                    errorMessage = $"Ngày {GetDayName(dayOfWeek)}: Môn học trùng lịch với môn {conflict.CourseCode} - {conflict.CourseName} " +
                                 $"(Tiết {conflict.StartSlot + 1}-{conflict.EndSlot + 1}) trong ca {SessionNames[conflict.Session]}.";
                    return false;
                }
            }

            return true;
        }

        private static string GetDayName(int dayOfWeek)
        {
            string[] days = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
            return dayOfWeek >= 0 && dayOfWeek < days.Length ? days[dayOfWeek] : $"Ngày {dayOfWeek}";
        }
    }

    /// <summary>
    /// Represents a schedule conflict
    /// </summary>
    public class ScheduleConflict
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int DayOfWeek { get; set; }
        public int StartSlot { get; set; }
        public int EndSlot { get; set; }
        public int Session { get; set; }
    }
}
