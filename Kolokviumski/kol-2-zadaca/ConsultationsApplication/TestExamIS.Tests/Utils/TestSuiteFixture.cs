namespace TestExamIS.Tests;

public class TestSuiteFixture
{
    public TestSuiteFixture()
    {
        ZipAndUploadStudentExams();
    }

    private void ZipAndUploadStudentExams()
    {
        Console.WriteLine("Zipping and uploading student exams...");
    }
}

[CollectionDefinition("Test Suite", DisableParallelization = true)]
public class TestSuiteCollection : ICollectionFixture<GlobalTestFixture> { }

