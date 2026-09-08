using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace TestExamIS.Tests.Utils;

public abstract class LoggedTestBase 
{
    protected readonly GlobalTestFixture Fixture;
        
    protected LoggedTestBase(GlobalTestFixture fixture)
    {
        Fixture = fixture;
    }
        
    /// <summary>
    /// Start a test method and begin tracking execution time
    /// </summary>
    protected void BeginTest([CallerMemberName] string methodName = "")
    {
        Fixture.BeginTest(methodName);
    }
        
    /// <summary>
    /// End a test method and log the result
    /// </summary>
    protected void EndTest(bool passed, string errorMessage = null, [CallerMemberName] string methodName = "")
    {
        var (category, points) = Fixture.GetTestMetadata(methodName, this);
        Fixture.EndTest($"{GetType().Name}.{methodName}", category, points, passed, errorMessage);
    }
        
    /// <summary>
    /// Run a test action with automatic logging
    /// </summary>
    protected void RunTest(Action testAction, [CallerMemberName] string methodName = "")
    {
        BeginTest(methodName);
            
        try
        {
            testAction();
            EndTest(true, null, methodName);
        }
        catch (Exception ex)
        {
            EndTest(false, ex.Message, methodName);
            throw;
        }
    }
    
    public async Task RunTestAsync(Func<Task> testAction, [CallerMemberName] string methodName = "")
    {
        BeginTest(methodName);
    
        try
        {
            await testAction();
            EndTest(true, null, methodName);
        }
        catch (Exception ex)
        {
            EndTest(false, ex.Message, methodName);
            throw;
        }
    }
}