namespace TestExamIS.Tests.Utils;

public class TestResult
{
    public string TestName { get; set; }
    public string StudentId { get; set; }
    public string TestCategory { get; set; }
    public bool IsPassed { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string ErrorMessage { get; set; }
    public int Points { get; set; }
}
