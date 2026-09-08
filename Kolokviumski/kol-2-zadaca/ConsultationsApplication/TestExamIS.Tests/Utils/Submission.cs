namespace TestExamIS.Tests.Utils;

public class Submission
{
    public string StudentId { get; set; }
    public string ExamId { get; set; }
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}