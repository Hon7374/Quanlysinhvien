using System;

namespace StudentManagement.Models
{
    public class Grade
    {
        public int GradeId { get; set; }
        public int EnrollmentId { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public string LetterGrade { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public Enrollment Enrollment { get; set; }

        public void CalculateTotalScore()
        {
            if (MidtermScore.HasValue && FinalScore.HasValue)
            {
                TotalScore = (MidtermScore.Value * 0.4m) + (FinalScore.Value * 0.6m);
                LetterGrade = GetLetterGrade(TotalScore.Value);
            }
        }

        private string GetLetterGrade(decimal score)
        {
            if (score >= 8.5m) return "A";
            if (score >= 7.0m) return "B";
            if (score >= 5.5m) return "C";
            if (score >= 4.0m) return "D";
            return "F";
        }
    }
}
