using System.Reflection;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ReflectionTests;

[Collection("Test Suite")]
public class MapperReflectionTests : LoggedTestBase, IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Assembly _webAssembly;

    public MapperReflectionTests(GlobalTestFixture fixture) : base(fixture)
    {
        var serviceCollection = new ServiceCollection();
        TestDatabaseHelper.SetupDI(serviceCollection);

        _webAssembly = typeof(Program).Assembly;

        var consultationMapperType = _webAssembly.GetTypes().FirstOrDefault(t => t.Name == "ConsultationMapper");
        if (consultationMapperType != null)
            serviceCollection.AddScoped(consultationMapperType);

        var attendanceMapperType = _webAssembly.GetTypes().FirstOrDefault(t => t.Name == "AttendanceMapper");
        if (attendanceMapperType != null)
            serviceCollection.AddScoped(attendanceMapperType);

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        await TestDatabaseHelper.ResetDatabaseAsync(_serviceProvider);
    }

    public Task DisposeAsync() => Task.CompletedTask;


    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task ConsultationMapper_InsertAsync_ShouldReturnCorrectFields()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "ConsultationMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var room = await context.Rooms.FirstAsync();

            var requestType = _webAssembly.GetTypes().First(t => t.Name == "ConsultationRequest");
            var request = Activator.CreateInstance(requestType, room.Id, DateTime.UtcNow.AddDays(50), DateTime.UtcNow.AddDays(50).AddHours(2));

            var method = mapperType.GetMethod("InsertAsync");
            Assert.NotNull(method);

            var result = await InvokeAsync(method!, mapper, new[] { request });
            Assert.NotNull(result);

            AssertHasProperties(result!, "Id", "Start", "End", "RoomId");
        });
    }

    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task ConsultationMapper_GetAllAsync_ShouldReturnList()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "ConsultationMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);

            var method = mapperType.GetMethod("GetAllAsync");
            Assert.NotNull(method);

            var result = await InvokeAsync(method!, mapper, new object?[] { null, null });
            Assert.NotNull(result);

            var list = result as System.Collections.IList;
            Assert.NotNull(list);
            Assert.True(list!.Count > 0, "Should return at least one consultation");

            AssertHasProperties(list[0]!, "Id", "Date", "RoomId", "RegisteredStudents");
        });
    }

    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task ConsultationMapper_UpdateAsync_ShouldReturnUpdatedFields()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "ConsultationMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var consultation = await context.Consultations.FirstAsync();
            var room = await context.Rooms.FirstAsync();

            var requestType = _webAssembly.GetTypes().First(t => t.Name == "ConsultationRequest");
            var request = Activator.CreateInstance(requestType, room.Id, DateTime.UtcNow.AddDays(60), DateTime.UtcNow.AddDays(60).AddHours(3));

            var method = mapperType.GetMethod("UpdateAsync");
            Assert.NotNull(method);

            var result = await InvokeAsync(method!, mapper, new[] { consultation.Id, request });
            Assert.NotNull(result);

            AssertHasProperties(result!, "Id", "RoomId");

            var roomIdProp = result!.GetType().GetProperty("RoomId");
            Assert.Equal(room.Id, roomIdProp!.GetValue(result));
        });
    }

    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task ConsultationMapper_DeleteAsync_ShouldReturnDeletedEntity()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "ConsultationMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var consultation = await context.Consultations.FirstAsync();

            var method = mapperType.GetMethod("DeleteAsync");
            Assert.NotNull(method);

            var result = await InvokeAsync(method!, mapper, new object[] { consultation.Id });
            Assert.NotNull(result);

            AssertHasProperties(result!, "Id", "Start", "End", "RoomId");
        });
    }


    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task AttendanceMapper_RegisterAsync_ShouldReturnCorrectFields()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "AttendanceMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var consultation = await context.Consultations.FirstAsync();
            var user = await context.Users.OfType<ConsultationsApplicationUser>().FirstAsync();
            var room = await context.Rooms.OfType<Room>().FirstAsync();

            var requestType = _webAssembly.GetTypes().First(t => t.Name == "AttendanceRequest");
            var request = Activator.CreateInstance(requestType, consultation.Id, user.Id, room.Id, "Reflection test comment");

            var method = mapperType.GetMethod("RegisterAsync");
            Assert.NotNull(method);

            var result = await InvokeAsync(method!, mapper, new[] { request });
            Assert.NotNull(result);

            AssertHasProperties(result!, "UserId", "FirstName", "LastName", "Status", "Comment");
        });
    }

    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task AttendanceMapper_GetAllByConsultationIdAsync_ShouldReturnList()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "AttendanceMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var consultation = await context.Consultations
                .Include(c => c.Attendances)
                .FirstAsync(c => c.Attendances.Any());

            var method = mapperType.GetMethod("GetAllByConsultationIdAsync");
            Assert.NotNull(method);

            var result = await InvokeAsync(method!, mapper, new object[] { consultation.Id });
            Assert.NotNull(result);

            var list = result as System.Collections.IList;
            Assert.NotNull(list);
            Assert.True(list!.Count > 0);

            AssertHasProperties(list[0]!, "UserId", "FirstName", "LastName", "Status");
        });
    }

    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task AttendanceMapper_MarkAsAbsentAsync_ShouldSucceed()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "AttendanceMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var attendance = await context.Attendances.FirstAsync();

            var method = mapperType.GetMethod("MarkAsAbsentAsync");
            Assert.NotNull(method);

            var task = (Task)method!.Invoke(mapper, new object[] { attendance.Id })!;
            await task;
        });
    }

    [LoggedFact(Category = "Mappers", Points = 5)]
    public async Task AttendanceMapper_UploadReasonAsync_ShouldReturnCorrectFields()
    {
        await RunTestAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mapperType = _webAssembly.GetTypes().First(t => t.Name == "AttendanceMapper");
            var mapper = scope.ServiceProvider.GetRequiredService(mapperType);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var attendance = await context.Attendances.FirstAsync();

            var fileBytes = "test cancellation reason content"u8.ToArray();
            var stream = new MemoryStream(fileBytes);
            var formFile = new Microsoft.AspNetCore.Http.FormFile(stream, 0, fileBytes.Length, "file", "reason.pdf");

            var method = mapperType.GetMethod("UploadReasonByIdInFileSystemAsync");
            Assert.NotNull(method);

            var result = await InvokeAsync(method!, mapper, new object[] { attendance.Id, formFile });
            Assert.NotNull(result);

            AssertHasProperties(result!, "UserId", "Status");
        });
    }

    // --- Helpers ---

    private static async Task<object?> InvokeAsync(MethodInfo method, object instance, object?[] args)
    {
        var task = (Task)method.Invoke(instance, args)!;
        await task;

        var resultProp = task.GetType().GetProperty("Result");
        return resultProp?.GetValue(task);
    }

    private static void AssertHasProperties(object obj, params string[] propertyNames)
    {
        var type = obj.GetType();
        foreach (var name in propertyNames)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(prop);
        }
    }
}
