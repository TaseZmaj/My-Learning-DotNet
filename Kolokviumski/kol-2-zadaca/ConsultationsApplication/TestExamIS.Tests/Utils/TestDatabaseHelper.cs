using System.Linq.Expressions;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Repository.Implementation;
using Repository.Interface;
using Service.Implementation;
using Service.Interface;

namespace TestExamIS.Tests.Utils;

public static class TestDatabaseHelper
{
    public static void SeedDatabase(ApplicationDbContext context, IPasswordHasher<ConsultationsApplicationUser> passwordHasher)
    {
        var random = new Random(42);

        var roles = new[] { Role.Professor, Role.Assistant, Role.Demonstrator };

        var firstNames = new[] { "Liam", "Olivia", "Noah", "Emma", "Oliver", "Ava", "Elijah", "Sophia", "James", "Isabella" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

        // 1. ROOMS
        var rooms = new List<Room>
        {
            new Room { Id = Guid.NewGuid(), Name = "A101", Capacity = 30 },
            new Room { Id = Guid.NewGuid(), Name = "B202", Capacity = 50 },
            new Room { Id = Guid.NewGuid(), Name = "Lab-1", Capacity = 25 },
            new Room { Id = Guid.NewGuid(), Name = "C303", Capacity = 40 },
            new Room { Id = Guid.NewGuid(), Name = "D404", Capacity = 60 }
        };
        context.Rooms.AddRange(rooms);

        // 2. USERS (bulk insert with password hasher)
        var users = new List<ConsultationsApplicationUser>();
        for (int i = 0; i < 10; i++)
        {
            var firstName = firstNames[i];
            var lastName = lastNames[i];
            var username = $"{firstName.ToLower()}.{lastName.ToLower()}{i}";

            var user = new ConsultationsApplicationUser
            {
                Id = $"test-user-{i + 1}",
                UserName = username,
                NormalizedUserName = username.ToUpper(),
                Email = $"{username}@university.edu",
                NormalizedEmail = $"{username.ToUpper()}@UNIVERSITY.EDU",
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                Role = roles[i % roles.Length],
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");
            users.Add(user);
        }
        context.Users.AddRange(users);

        // 3. CONSULTATIONS
        var now = DateTime.UtcNow;
        var consultations = new List<Consultation>();
        for (int i = 0; i < 10; i++)
        {
            var startOffset = random.Next(-30, 30);
            var startHour = random.Next(8, 17);
            var start = now.Date.AddDays(startOffset).AddHours(startHour);

            consultations.Add(new Consultation
            {
                Id = Guid.NewGuid(),
                StartTime = start,
                EndTime = start.AddHours(random.Next(1, 3)),
                RoomId = rooms[i % rooms.Count].Id,
                RegisteredStudents = 0,
                CreatedAt = now.AddDays(-random.Next(1, 20)),
                CreatedById = users[0].Id,
                LastModifiedAt = now,
                LastModifiedById = users[0].Id
            });
        }
        context.Consultations.AddRange(consultations);

        // 4. ATTENDANCES
        var statuses = Enum.GetValues<Status>();
        var attendances = new List<Attendance>();
        for (int i = 0; i < 15; i++)
        {
            var consultation = consultations[i % consultations.Count];
            var user = users[(i + 1) % users.Count];

            attendances.Add(new Attendance
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoomId = consultation.RoomId,
                ConsultationId = consultation.Id,
                Status = statuses[i % statuses.Length],
                Comment = i % 3 == 0 ? $"Test comment {i}" : null,
                CancellationReasonDocumentPath = null
            });
        }
        context.Attendances.AddRange(attendances);

        // 5. HOLDS
        var holds = new List<Holds>();
        for (int i = 0; i < 10; i++)
        {
            holds.Add(new Holds
            {
                Id = Guid.NewGuid(),
                ConsultationId = consultations[i % consultations.Count].Id,
                UserId = users[i % users.Count].Id,
                CreatedAt = now.AddDays(-random.Next(1, 15)),
                CreatedById = users[0].Id,
                LastModifiedAt = now,
                LastModifiedById = users[0].Id
            });
        }
        context.Holds.AddRange(holds);

        context.SaveChanges();
    }

    public static async Task ResetDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ConsultationsApplicationUser>>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        SeedDatabase(context, hasher);
    }

    public static void SetupDI(ServiceCollection serviceCollection)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        serviceCollection.AddSingleton(connection);

        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(connection);
            options.UseLazyLoadingProxies();
        });

        serviceCollection.AddIdentity<ConsultationsApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IConsultationService, ConsultationService>();
        serviceCollection.AddScoped<IAttendanceService, AttendanceService>();
        serviceCollection.AddScoped<IFileUploadService, FileUploadService>();

        serviceCollection.AddLogging();
        serviceCollection.AddHttpContextAccessor();
    }

    public static async Task<int> GetCount<T>(IServiceProvider serviceProvider) where T : class
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Set<T>().CountAsync();
    }

    public static async Task<T> GetFirst<T>(IServiceProvider serviceProvider) where T : class
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Set<T>().FirstAsync();
    }

    public static async Task<List<T>> GetAllWhere<T>(
        IServiceProvider services,
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
        where T : class
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IQueryable<T> query = dbContext.Set<T>().Where(predicate);
        if (include != null) query = include(query);
        return await query.ToListAsync();
    }
    
    public static async Task<T?> GetFirstWhere<T>(
        IServiceProvider services,
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
        where T : class
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        IQueryable<T> query = dbContext.Set<T>().Where(predicate);

        if (include != null)
            query = include(query);

        return await query.FirstOrDefaultAsync();
    }

    public static T? GetById<T>(IServiceProvider serviceProvider, Func<T, bool> predicate) where T : class
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context.Set<T>().Where(predicate).FirstOrDefault();
    }

    public static async Task<T> CreateEntity<T>(IServiceProvider services, T entity) where T : class
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Set<T>().AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }
}
