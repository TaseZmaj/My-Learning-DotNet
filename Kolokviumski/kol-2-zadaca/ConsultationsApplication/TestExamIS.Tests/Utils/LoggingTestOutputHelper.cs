using Xunit.Abstractions;

namespace TestExamIS.Tests.Utils;

public class LoggingTestOutputHelper : ITestOutputHelper
{
    private readonly ITestOutputHelper _originalOutputHelper;
    private readonly ExamLogger _logger;
    private readonly LoggedFactAttribute _attribute;
    private readonly string _testName;
    private readonly DateTime _startTime;

    public LoggingTestOutputHelper(ITestOutputHelper originalOutputHelper, ExamLogger logger,
        LoggedFactAttribute attribute, string testName)
    {
        _originalOutputHelper = originalOutputHelper;
        _logger = logger;
        _attribute = attribute;
        _testName = testName;
        _startTime = DateTime.Now;
    }

    public void WriteLine(string message)
    {
        _originalOutputHelper.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        _originalOutputHelper.WriteLine(format, args);
    }

    public void LogTestResult(bool isPassed, string errorMessage = null)
    {
        _logger.LogTestResult(
            _testName,
            _attribute?.Category ?? "Default",
            isPassed,
            errorMessage,
            _attribute?.Points ?? 1
        );
    }
}