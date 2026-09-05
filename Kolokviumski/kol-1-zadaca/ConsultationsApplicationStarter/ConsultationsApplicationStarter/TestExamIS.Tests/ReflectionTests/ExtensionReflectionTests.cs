using System.Reflection;
using Domain.Enums;
using Domain.Models;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ReflectionTests;

[Collection("Test Suite")]
public class ExtensionReflectionTests : LoggedTestBase
{
    private readonly Assembly _webAssembly;

    public ExtensionReflectionTests(GlobalTestFixture fixture) : base(fixture)
    {
        _webAssembly = typeof(Program).Assembly;
    }

    [LoggedFact(Category = "Extensions", Points = 10)]
    public void ConsultationExtensions_ToResponse_ShouldMapAllFields()
    {
        RunTest(() =>
        {
            var extensionType = _webAssembly.GetTypes()
                .First(t => t.Name == "ConsultationExtensions" && t.IsAbstract && t.IsSealed);

            var toResponseMethod = extensionType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "ToResponse" &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(Consultation));

            var roomId = Guid.NewGuid();
            var startTime = new DateTime(2025, 6, 15, 10, 0, 0);
            var consultationId = Guid.NewGuid();
            var attendanceId = Guid.NewGuid();

            var user = new ConsultationsApplicationUser
            {
                Id = "user-1",
                FirstName = "John",
                LastName = "Doe",
                UserName = "john.doe",
                Email = "john.doe@university.edu",
                Role = Role.Professor
            };

            var consultation = new Consultation
            {
                Id = consultationId,
                StartTime = startTime,
                EndTime = startTime.AddHours(2),
                RoomId = roomId,
                RegisteredStudents = 42,
                CreatedAt = DateTime.UtcNow,
                CreatedById = "user-1",
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedById = "user-1",
                Attendances = new List<Attendance>
                {
                    new Attendance
                    {
                        Id = attendanceId,
                        UserId = "user-1",
                        User = user,
                        RoomId = roomId,
                        ConsultationId = consultationId,
                        Status = Status.Present,
                        Comment = "Test"
                    }
                }
            };

            var result = toResponseMethod.Invoke(null, new object[] { consultation })!;
            var resultType = result.GetType();

            var idProp = resultType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(idProp);
            Assert.Equal(consultationId, idProp!.GetValue(result));

            var dateProp = resultType.GetProperty("Date", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(dateProp);
            Assert.Equal(DateOnly.FromDateTime(startTime), dateProp!.GetValue(result));

            var roomIdProp = resultType.GetProperty("RoomId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(roomIdProp);
            Assert.Equal(roomId, roomIdProp!.GetValue(result));

            var studentsProp = resultType.GetProperty("RegisteredStudents", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(studentsProp);
            Assert.Equal(42, studentsProp!.GetValue(result));

            var attendancesProp = resultType.GetProperty("Attendances", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(attendancesProp);
            var attendances = attendancesProp!.GetValue(result) as System.Collections.IList;
            Assert.NotNull(attendances);
            Assert.Equal(1, attendances!.Count);

            var att = attendances[0]!;
            var attType = att.GetType();
            var attIdProp = attType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(attIdProp);
            Assert.Equal(attendanceId, attIdProp!.GetValue(att));

            var attFirstNameProp = attType.GetProperty("FirstName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(attFirstNameProp);
            Assert.Equal("John", attFirstNameProp!.GetValue(att));

            var attLastNameProp = attType.GetProperty("LastName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(attLastNameProp);
            Assert.Equal("Doe", attLastNameProp!.GetValue(att));
        });
    }

    [LoggedFact(Category = "Extensions", Points = 10)]
    public void AttendanceExtensions_ToResponse_ShouldMapAllFields()
    {
        RunTest(() =>
        {
            var extensionType = _webAssembly.GetTypes()
                .First(t => t.Name == "AttendanceExtensions" && t.IsAbstract && t.IsSealed);

            var toResponseMethod = extensionType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "ToResponse" &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(Attendance));

            var userId = "test-user-42";
            var user = new ConsultationsApplicationUser
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                UserName = "john.doe",
                Email = "john.doe@university.edu",
                Role = Role.Professor
            };

            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                RoomId = Guid.NewGuid(),
                ConsultationId = Guid.NewGuid(),
                Status = Status.Present,
                Comment = "Great participation"
            };

            var result = toResponseMethod.Invoke(null, new object[] { attendance })!;
            var resultType = result.GetType();

            var userIdProp = resultType.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(userIdProp);
            Assert.Equal(userId, userIdProp!.GetValue(result));

            var firstNameProp = resultType.GetProperty("FirstName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(firstNameProp);
            Assert.Equal("John", firstNameProp!.GetValue(result));

            var lastNameProp = resultType.GetProperty("LastName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(lastNameProp);
            Assert.Equal("Doe", lastNameProp!.GetValue(result));

            var statusProp = resultType.GetProperty("Status", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(statusProp);
            Assert.Equal("Present", statusProp!.GetValue(result)!.ToString());

            var commentProp = resultType.GetProperty("Comment", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(commentProp);
            Assert.Equal("Great participation", commentProp!.GetValue(result));
        });
    }
}
