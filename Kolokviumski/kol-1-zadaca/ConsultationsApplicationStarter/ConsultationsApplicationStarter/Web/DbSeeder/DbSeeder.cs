using Repository;

namespace Web.DbSeeder;

using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ConsultationsApplicationUser> userManager)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync()) return;

        var rng = new Random(42);
        
        var firstNames = new[]
        {
            "Liam", "Olivia", "Noah", "Emma", "Oliver", "Ava", "Elijah", "Sophia",
            "James", "Isabella", "William", "Mia", "Benjamin", "Charlotte", "Lucas",
            "Amelia", "Henry", "Harper", "Alexander", "Evelyn", "Mason", "Abigail",
            "Ethan", "Emily", "Daniel", "Ella", "Matthew", "Elizabeth", "Aiden", "Camila",
            "Jackson", "Luna", "Sebastian", "Sofia", "Jack", "Avery", "Owen", "Mila",
            "Samuel", "Aria", "Dylan", "Scarlett", "David", "Penelope", "Joseph", "Layla",
            "Carter", "Riley", "Wyatt", "Zoey", "John", "Nora", "Luke", "Lily",
            "Gabriel", "Eleanor", "Anthony", "Hannah", "Isaac", "Lillian", "Jayden", "Addison",
            "Lincoln", "Aubrey", "Joshua", "Ellie", "Christopher", "Stella", "Andrew", "Natalie",
            "Theodore", "Zoe", "Caleb", "Leah", "Ryan", "Hazel", "Nathan", "Violet",
            "Adrian", "Aurora", "Christian", "Savannah", "Maverick", "Audrey", "Colton", "Brooklyn",
            "Ezra", "Bella", "Isaiah", "Claire", "Hunter", "Skylar", "Thomas", "Lucy",
            "Aaron", "Paisley", "Eli", "Everly", "Landon", "Anna", "Jonathan", "Caroline"
        };

        var lastNames = new[]
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
            "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
            "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
            "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen",
            "Hill", "Flores", "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera",
            "Campbell", "Mitchell", "Carter", "Roberts"
        };

        var roles = new[] { Role.Professor, Role.Assistant, Role.Demonstrator };

        var users = new List<ConsultationsApplicationUser>();

        for (int i = 0; i < 120; i++)
        {
            var firstName = firstNames[i % firstNames.Length];
            var lastName = lastNames[i % lastNames.Length];
            var role = roles[i % roles.Length];
            var username = $"{firstName.ToLower()}.{lastName.ToLower()}{i}";

            var user = new ConsultationsApplicationUser
            {
                UserName = username,
                Email = $"{username}@university.edu",
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                Role = role
            };

            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded) users.Add(user);
        }

        await context.SaveChangesAsync();

        var roomNames = new[]
        {
            "A101", "A102", "A103", "A104", "A105",
            "B201", "B202", "B203", "B204", "B205",
            "C301", "C302", "C303", "C304", "C305",
            "Lab-1", "Lab-2", "Lab-3", "Lab-4", "Lab-5",
            "Seminar-1", "Seminar-2", "Seminar-3", "Seminar-4", "Seminar-5",
            "D401", "D402", "D403", "D404", "D405"
        };

        var rooms = roomNames.Select(name => new Room
        {
            Id = Guid.NewGuid(),
            Name = name,
            Capacity = rng.Next(20, 151)
        }).ToList();

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();


        var adminUser = users.First();
        var now = DateTime.UtcNow;

        var consultations = new List<Consultation>();

        for (int i = 0; i < 110; i++)
        {
            var startOffset = rng.Next(-60, 90);
            var startHour = rng.Next(8, 17);
            var duration = rng.Next(1, 4);

            var start = now.Date
                .AddDays(startOffset)
                .AddHours(startHour)
                .AddMinutes(rng.Next(0, 2) * 30);

            consultations.Add(new Consultation
            {
                Id = Guid.NewGuid(),
                StartTime = start,
                EndTime = start.AddHours(duration),
                RoomId = rooms[rng.Next(rooms.Count)].Id,
                RegisteredStudents = rng.Next(5, 51),
                CreatedAt = now.AddDays(-rng.Next(1, 30)),
                CreatedById = users[rng.Next(users.Count)].Id,
                LastModifiedAt = now,
                LastModifiedById = adminUser.Id
            });
        }

        await context.Consultations.AddRangeAsync(consultations);
        await context.SaveChangesAsync();
        
        var holdStatuses = new List<(Guid consultationId, string userId)>();
        var holds = new List<Holds>();

        foreach (var consultation in consultations)
        {
            var holderCount = rng.Next(1, 4);
            var shuffled = users.OrderBy(_ => rng.Next()).Take(holderCount);

            foreach (var holder in shuffled)
            {
                if (holdStatuses.Any(h => h.consultationId == consultation.Id && h.userId == holder.Id))
                    continue;

                holdStatuses.Add((consultation.Id, holder.Id));

                holds.Add(new Holds
                {
                    Id = Guid.NewGuid(),
                    ConsultationId = consultation.Id,
                    UserId = holder.Id,
                    CreatedAt = now.AddDays(-rng.Next(1, 20)),
                    CreatedById = adminUser.Id,
                    LastModifiedAt = now,
                    LastModifiedById = adminUser.Id
                });
            }
        }

        while (holds.Count < 100)
        {
            var consultation = consultations[rng.Next(consultations.Count)];
            var user = users[rng.Next(users.Count)];

            if (holdStatuses.Any(h => h.consultationId == consultation.Id && h.userId == user.Id))
                continue;

            holdStatuses.Add((consultation.Id, user.Id));

            holds.Add(new Holds
            {
                Id = Guid.NewGuid(),
                ConsultationId = consultation.Id,
                UserId = user.Id,
                CreatedAt = now.AddDays(-rng.Next(1, 20)),
                CreatedById = adminUser.Id,
                LastModifiedAt = now,
                LastModifiedById = adminUser.Id
            });
        }

        await context.Holds.AddRangeAsync(holds);
        await context.SaveChangesAsync();

        var statuses = Enum.GetValues<Status>();
        var cancellationReasons = new[]
        {
            null, null, null, null,                    
            "docs/medical_cert_001.pdf",
            "docs/medical_cert_002.pdf",
            "docs/excuse_letter_001.pdf",
            "docs/excuse_letter_002.pdf"
        };

        var attendanceKeys = new HashSet<(Guid consultationId, string userId)>();
        var attendances = new List<Attendance>();

        foreach (var consultation in consultations)
        {
            var attendeeCount = rng.Next(3, Math.Min(15, users.Count));
            var shuffled = users.OrderBy(_ => rng.Next()).Take(attendeeCount);

            foreach (var attendee in shuffled)
            {
                var key = (consultation.Id, attendee.Id);
                if (!attendanceKeys.Add(key)) continue;

                var status = statuses[rng.Next(statuses.Length)];
                var comment = status switch
                {
                    Status.Absent => "Student was absent without prior notice.",
                    Status.Late   => $"Arrived {rng.Next(5, 31)} minutes late.",
                    Status.Present => rng.Next(3) == 0 ? "Active participation noted." : null,
                    _             => null
                };

                attendances.Add(new Attendance
                {
                    Id = Guid.NewGuid(),
                    UserId = attendee.Id,
                    RoomId = consultation.RoomId,
                    ConsultationId = consultation.Id,
                    Status = status,
                    Comment = comment,
                    CancellationReasonDocumentPath = status == Status.Absent
                        ? cancellationReasons[rng.Next(cancellationReasons.Length)]
                        : null
                });
            }
        }

        while (attendances.Count < 100)
        {
            var consultation = consultations[rng.Next(consultations.Count)];
            var user = users[rng.Next(users.Count)];
            var key = (consultation.Id, user.Id);

            if (!attendanceKeys.Add(key)) continue;

            var status = statuses[rng.Next(statuses.Length)];

            attendances.Add(new Attendance
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoomId = consultation.RoomId,
                ConsultationId = consultation.Id,
                Status = status,
                Comment = null,
                CancellationReasonDocumentPath = null
            });
        }

        await context.Attendances.AddRangeAsync(attendances);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded: {users.Count} users, {rooms.Count} rooms, " +
                          $"{consultations.Count} consultations, {holds.Count} holds, " +
                          $"{attendances.Count} attendances.");
    }
}