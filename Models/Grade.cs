using System;

namespace StudentManagement.Models
{
    public class Grade
    {
        public int GradeId { get; set; }
        public int EnrollmentId { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? AssignmentScore { get; set; }
        public decimal? TotalScore { get; set; }
        public string LetterGrade { get; set; }
        public DateTime? GradedDate { get; set; }
        public string Comments { get; set; }

        // Navigation property
        public Enrollment Enrollment { get; set; }

        public void CalculateTotalScore()
        {
            if (MidtermScore.HasValue && FinalScore.HasValue && AssignmentScore.HasValue)
            {
                TotalScore = (MidtermScore.Value * 0.3m) + (FinalScore.Value * 0.5m) + (AssignmentScore.Value * 0.2m);
                LetterGrade = GetLetterGrade(TotalScore.Value);
                GradedDate = DateTime.Now;
            }
        }

        private string GetLetterGrade(decimal score)
        {
            if (score >= 9.0m) return "A+";
            if (score >= 8.5m) return "A";
            if (score >= 8.0m) return "B+";
            if (score >= 7.0m) return "B";
            if (score >= 6.5m) return "C+";
            if (score >= 5.5m) return "C";
            if (score >= 5.0m) return "D+";
            if (score >= 4.0m) return "D";
            return "F";
        }
    }
}
