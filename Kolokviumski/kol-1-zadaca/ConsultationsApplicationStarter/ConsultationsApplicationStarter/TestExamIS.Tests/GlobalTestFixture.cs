using System.Reflection;
using System.Reflection.PortableExecutable;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests;

using System.IO.Compression;
using Xunit;
public class GlobalTestFixture : IAsyncLifetime
{
    private readonly string _projectPath;
    private readonly string _zipPath;
    private static bool _zipCreated = false;

    public string ZipPath => _zipPath;
    private static readonly ExamLogger _logger = new(
        "EXAM_ID",
        "YOUR_INDEX_HERE", // <---------- PUT YOUR INDEX HERE 
        logFilePath: Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName ?? "",
            "TestOutput",
            "test_results.json"),
        serverUploadUrl: "https://integriranisistemi.finki.ukim.mk/api/submission/upload");
    public ExamLogger Logger => _logger;
    
    private static readonly Dictionary<string, DateTime> _testStartTimes = new Dictionary<string, DateTime>();


    public GlobalTestFixture()
    {
        _projectPath = Directory.GetParent(AppContext.BaseDirectory)
                           ?.Parent?.Parent?.Parent?.Parent?.FullName
                       ?? throw new DirectoryNotFoundException("Could not find project root.");

        var outputDir = Path.Combine(_projectPath, "TestOutput");
        Directory.CreateDirectory(outputDir);

        _zipPath = Path.Combine(outputDir, "Solution.zip");
    }

    public Task InitializeAsync()
    {
        if (!_zipCreated)
        {
            if (File.Exists(_zipPath))
                File.Delete(_zipPath);

            SubmissionHelper.ZipProject(_projectPath, _zipPath);
            _zipCreated = true;
        }

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Logger.SaveToFileAsync();
        // await Logger.UploadToServerAsync(_zipPath);
        Logger.PrintSummary();
        Logger.FlushOutput();
    }
    
    public void BeginTest(string testName)
    {
        _testStartTimes[testName] = DateTime.Now;
    }

    public void EndTest(string testName, string category, int points, bool passed, string errorMessage = null)
    {
        Logger.LogTestResult(testName, category, passed, errorMessage, points);
    }
    
    public (string category, int points) GetTestMetadata(string testName, object testClassInstance)
    {
        var type = testClassInstance.GetType();
        var method = type.GetMethod(testName);
            
        if (method == null)
        {
            return ("Unknown", 1);
        }
            
        var factAttr = method.GetCustomAttribute<LoggedFactAttribute>();
            
        if (factAttr != null)
        {
            return (factAttr.Category, factAttr.Points);
        }
            
        return ("Default", 1);
    }
}
