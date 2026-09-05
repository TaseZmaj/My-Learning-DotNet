using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TestExamIS.Tests.Utils;
public class ExamLogger
{
    private readonly List<TestResult> _testResults = new List<TestResult>();
    private readonly StringBuilder _output = new StringBuilder();
    private readonly string _logFilePath;
    private readonly string _serverUploadUrl;
    private readonly string _studentId;
    private readonly string _examId;

    public ExamLogger(string examId, string studentId, string logFilePath = null, string serverUploadUrl = null)
    {
        _examId = examId;
        _studentId = studentId;
        _logFilePath = logFilePath ?? $"test_results_{studentId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        _serverUploadUrl = serverUploadUrl;
    }

    public void LogTestResult(string testName, string category, bool isPassed,
        string errorMessage = null, int pointsWorth = 1)
    {
        var result = new TestResult
        {
            TestName = testName,
            StudentId = _studentId,
            TestCategory = category,
            IsPassed = isPassed,
            ExecutedAt = DateTime.Now,
            ErrorMessage = errorMessage,
            Points = pointsWorth,
        };

        _testResults.Add(result);

        // WriteTestResultToConsole(result);
        AppendTestResultToOutput(result);
    }

    private void WriteTestResultToConsole(TestResult result)
    {
        var originalColor = Console.ForegroundColor;

        Console.Write($"[{result.ExecutedAt:HH:mm:ss}] [{result.TestCategory}] {result.TestName} ({result.Points} pts): ");
        
        if (result.IsPassed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("PASSED\n");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("FAILED\n");
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                Console.WriteLine($" - {result.ErrorMessage}");
            }
        }

        Console.ForegroundColor = originalColor;
    }
    
    private void AppendTestResultToOutput(TestResult result)
    {
        _output.Append($"[{result.ExecutedAt:HH:mm:ss}] [{result.TestCategory}] {result.TestName} ({result.Points} pts): ");

        if (result.IsPassed)
        {
            _output.AppendLine("PASSED");
        }
        else
        {
            _output.AppendLine("FAILED");
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                _output.AppendLine($" - {result.ErrorMessage}");
            }
        }
    }

    public void PrintSummary()
    {
        var totalTests = 50;
        var passedTests = _testResults.Count(r => r.IsPassed);
        var totalPoints = 310;
        var earnedPoints = _testResults.Where(r => r.IsPassed).Sum(r => r.Points);
        var percentage = totalPoints > 0 ? (double)earnedPoints / totalPoints * 100 : 0;

        _output.AppendLine("\n========== TEST SUMMARY ==========");
        _output.AppendLine($"Student ID: {_studentId}");
        _output.AppendLine($"Total tests: {totalTests}");
        _output.AppendLine($"Passed tests: {passedTests} / {totalTests} ({passedTests * 100.0 / totalTests:0.##}%)");
        _output.AppendLine($"Points earned: {earnedPoints} / {totalPoints} ({percentage:0.##}%)");

        var categories = _testResults.Select(r => r.TestCategory).Distinct();
        _output.AppendLine("\nCategory Breakdown:");
        foreach (var category in categories)
        {
            var categoryTests = _testResults.Where(r => r.TestCategory == category).ToList();
            var categoryPassed = categoryTests.Count(r => r.IsPassed);
            var categoryPoints = categoryTests.Where(r => r.IsPassed).Sum(r => r.Points);
            var categoryTotalPoints = categoryTests.Sum(r => r.Points);

            _output.AppendLine($"  {category}: {categoryPassed}/{categoryTests.Count} tests, " +
                              $"{categoryPoints}/{categoryTotalPoints} points " +
                              $"({(categoryTotalPoints > 0 ? (double)categoryPoints / categoryTotalPoints * 100 : 0):0.##}%)");
        }

        _output.AppendLine("==================================");
    }

    public void FlushOutput()
    {
        Console.WriteLine(_output.ToString());
        _output.Clear();
    }

    public async Task SaveToFileAsync()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(_testResults, options);
        await File.WriteAllTextAsync(_logFilePath, json);

        Console.WriteLine($"\nTest results saved to {_logFilePath}");
    }

    public async Task UploadToServerAsync(string filePath)
    {
        if (string.IsNullOrEmpty(_serverUploadUrl))
        {
            Console.WriteLine("Server upload URL not specified. Skipping upload.");
            return;
        }
        
        try
        {
            var submission = new Submission()
            {
                TestResults = _testResults,
                ExamId = _examId,
                StudentId = _studentId,
            };
            
            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(submission);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using var formData = new MultipartFormDataContent();

            formData.Add(content, "Data");
            formData.Add(fileContent, "File", "Solution.zip");
            
            var response = await client.PostAsync(_serverUploadUrl, formData);
            response.EnsureSuccessStatusCode();
            
            Console.WriteLine($"Test results successfully uploaded to server: {_serverUploadUrl}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to upload results to server: {ex.Message}");
            Console.ResetColor();
        }
    }

    // public void PrintSummary()
    // {
    //     var totalTests = _testResults.Count;
    //     var passedTests = _testResults.Count(r => r.IsPassed);
    //     var totalPoints = _testResults.Sum(r => r.Points);
    //     var earnedPoints = _testResults.Where(r => r.IsPassed).Sum(r => r.Points);
    //     var percentage = totalPoints > 0 ? (double)earnedPoints / totalPoints * 100 : 0;
    //
    //     Console.WriteLine("\n========== TEST SUMMARY ==========");
    //     Console.WriteLine($"Student ID: {_studentId}");
    //     Console.WriteLine($"Total tests: {totalTests}");
    //     Console.WriteLine($"Passed tests: {passedTests} / {totalTests} ({passedTests * 100.0 / totalTests:0.##}%)");
    //     Console.WriteLine($"Points earned: {earnedPoints} / {totalPoints} ({percentage:0.##}%)");
    //
    //     var categories = _testResults.Select(r => r.TestCategory).Distinct();
    //     Console.WriteLine("\nCategory Breakdown:");
    //     foreach (var category in categories)
    //     {
    //         var categoryTests = _testResults.Where(r => r.TestCategory == category).ToList();
    //         var categoryPassed = categoryTests.Count(r => r.IsPassed);
    //         var categoryPoints = categoryTests.Where(r => r.IsPassed).Sum(r => r.Points);
    //         var categoryTotalPoints = categoryTests.Sum(r => r.Points);
    //
    //         Console.WriteLine($"  {category}: {categoryPassed}/{categoryTests.Count} tests, " +
    //                           $"{categoryPoints}/{categoryTotalPoints} points " +
    //                           $"({(categoryTotalPoints > 0 ? (double)categoryPoints / categoryTotalPoints * 100 : 0):0.##}%)");
    //     }
    //
    //     Console.WriteLine("==================================");
    // }
}