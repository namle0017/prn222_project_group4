using FapWeb.Models.Data;
using FapWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Infrastructure;

public class DemoDataSeeder
{
    private readonly PostgresContext _context;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(PostgresContext context, ILogger<DemoDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(string password)
    {
        var adminRole = await EnsureRoleAsync(AppRoles.Admin, "Quản trị viên");
        var teacherRole = await EnsureRoleAsync(AppRoles.Teacher, "Giáo viên");
        var studentRole = await EnsureRoleAsync(AppRoles.Student, "Học sinh");
        var parentRole = await EnsureRoleAsync(AppRoles.Parent, "Phụ huynh");
        await _context.SaveChangesAsync();

        var admin = await EnsureUserAsync("Nguyen Van Admin", "0901000001", adminRole.Id, password);
        var teacher = await EnsureUserAsync("Tran Thi Mai", "0901000002", teacherRole.Id, password);
        var student = await EnsureUserAsync("Nguyen Minh Khang", "0901000005", studentRole.Id, password);
        var parent = await EnsureUserAsync("Nguyen Van Long", "0901000009", parentRole.Id, password);
        await _context.SaveChangesAsync();

        var relationship = await EnsureRelationshipAsync("PARENT", "Phụ huynh");
        var present = await EnsureAttendanceStatusAsync("PRESENT");
        var absent = await EnsureAttendanceStatusAsync("ABSENT");
        var partial = await EnsureTuitionStatusAsync("PARTIAL");
        var unpaid = await EnsureTuitionStatusAsync("UNPAID");
        await _context.SaveChangesAsync();

        await EnsureGuardianLinkAsync(student.Id, parent.Id, relationship.Id);

        var csharpClass = await EnsureClassAsync("Lập trình C# .NET cơ bản", teacher.Id, "P.101", 18);
        var webClass = await EnsureClassAsync("Lập trình Web ASP.NET Core", teacher.Id, "P.202", 20);
        await _context.SaveChangesAsync();

        await EnsureEnrollmentAsync(csharpClass.Id, student.Id);
        await EnsureEnrollmentAsync(webClass.Id, student.Id);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);
        var csharpSchedule = await EnsureScheduleAsync(csharpClass.Id, tomorrow, new TimeOnly(8, 0), new TimeOnly(10, 0), "Biến, kiểu dữ liệu và câu lệnh điều kiện trong C#");
        await EnsureScheduleAsync(csharpClass.Id, tomorrow.AddDays(2), new TimeOnly(8, 0), new TimeOnly(10, 0), "Lập trình hướng đối tượng với C#");
        await EnsureScheduleAsync(webClass.Id, tomorrow.AddDays(1), new TimeOnly(13, 30), new TimeOnly(15, 30), "ASP.NET Core MVC: Controller, View và Routing");
        await EnsureScheduleAsync(webClass.Id, tomorrow.AddDays(3), new TimeOnly(13, 30), new TimeOnly(15, 30), "Entity Framework Core và PostgreSQL");

        var attendanceSeed = new List<(Schedule Schedule, int StatusId)>
        {
            (await EnsureScheduleAsync(csharpClass.Id, today.AddDays(-16), new TimeOnly(8, 0), new TimeOnly(10, 0), "C# cơ bản: biến và kiểu dữ liệu"), present.Id),
            (await EnsureScheduleAsync(csharpClass.Id, today.AddDays(-12), new TimeOnly(8, 0), new TimeOnly(10, 0), "C# cơ bản: câu lệnh điều kiện"), present.Id),
            (await EnsureScheduleAsync(csharpClass.Id, today.AddDays(-8), new TimeOnly(8, 0), new TimeOnly(10, 0), "C# cơ bản: vòng lặp"), absent.Id),
            (await EnsureScheduleAsync(csharpClass.Id, today.AddDays(-4), new TimeOnly(8, 0), new TimeOnly(10, 0), "C# cơ bản: lập trình hướng đối tượng"), present.Id),
            (await EnsureScheduleAsync(webClass.Id, today.AddDays(-15), new TimeOnly(13, 30), new TimeOnly(15, 30), "ASP.NET Core MVC và Routing"), present.Id),
            (await EnsureScheduleAsync(webClass.Id, today.AddDays(-11), new TimeOnly(13, 30), new TimeOnly(15, 30), "Controller, ViewModel và Razor View"), absent.Id),
            (await EnsureScheduleAsync(webClass.Id, today.AddDays(-7), new TimeOnly(13, 30), new TimeOnly(15, 30), "Entity Framework Core"), present.Id),
            (await EnsureScheduleAsync(webClass.Id, today.AddDays(-3), new TimeOnly(13, 30), new TimeOnly(15, 30), "PostgreSQL và truy vấn LINQ"), present.Id)
        };
        await EnsureScheduleAsync(webClass.Id, today.AddDays(-2), new TimeOnly(13, 30), new TimeOnly(15, 30), "Xây dựng chức năng ASP.NET Core hoàn chỉnh");
        await _context.SaveChangesAsync();

        await EnsureAttendanceAsync(csharpSchedule.Id, student.Id, teacher.Id, present.Id);
        foreach (var item in attendanceSeed)
        {
            await EnsureAttendanceAsync(item.Schedule.Id, student.Id, teacher.Id, item.StatusId);
        }
        await EnsureTuitionAsync(student.Id, csharpClass.Id, partial.Id, 1_800_000m, 900_000m, tomorrow.AddDays(14));
        await EnsureTuitionAsync(student.Id, webClass.Id, unpaid.Id, 2_000_000m, 0m, tomorrow.AddDays(21));
        await EnsureNotificationAsync(teacher.Id, student.Id, "Chào mừng đến lớp C# .NET", "Bạn đã được ghi danh vào lớp Lập trình C# .NET cơ bản.");
        await EnsureNotificationAsync(teacher.Id, parent.Id, "Lịch học của Nguyễn Minh Khang", "Học sinh có lịch học lập trình C# .NET vào ngày mai lúc 08:00.");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Demo programming data has been seeded for admin, teacher, student, and parent accounts.");
    }

    private async Task<Role> EnsureRoleAsync(string roleName, string description)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(item => item.RoleName == roleName);
        if (role != null) return role;

        role = new Role { RoleName = roleName, Description = description };
        await _context.Roles.AddAsync(role);
        return role;
    }

    private async Task<User> EnsureUserAsync(string fullName, string phone, int roleId, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Phone == phone);
        if (user == null)
        {
            user = new User { Phone = phone, CreatedAt = DateTime.UtcNow };
            await _context.Users.AddAsync(user);
        }

        user.FullName = fullName;
        user.RoleId = roleId;
        user.IsActive = true;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        user.UpdatedAt = DateTime.UtcNow;
        return user;
    }

    private async Task<FamilyRelationship> EnsureRelationshipAsync(string name, string description)
    {
        var relationship = await _context.FamilyRelationships.FirstOrDefaultAsync(item => item.RelationshipName == name);
        if (relationship != null) return relationship;

        relationship = new FamilyRelationship { RelationshipName = name };
        await _context.FamilyRelationships.AddAsync(relationship);
        return relationship;
    }

    private async Task<AttendanceCheckStatus> EnsureAttendanceStatusAsync(string name)
    {
        var status = await _context.AttendanceCheckStatuses.FirstOrDefaultAsync(item => item.StatusName == name);
        if (status != null) return status;

        status = new AttendanceCheckStatus { StatusName = name };
        await _context.AttendanceCheckStatuses.AddAsync(status);
        return status;
    }

    private async Task<TuitionFeeStatus> EnsureTuitionStatusAsync(string name)
    {
        var status = await _context.TuitionFeeStatuses.FirstOrDefaultAsync(item => item.StatusName == name);
        if (status != null) return status;

        status = new TuitionFeeStatus { StatusName = name };
        await _context.TuitionFeeStatuses.AddAsync(status);
        return status;
    }

    private async Task EnsureGuardianLinkAsync(Guid studentId, Guid guardianId, int relationshipId)
    {
        var link = await _context.StudentGuardians.FirstOrDefaultAsync(item => item.StudentId == studentId && item.GuardianId == guardianId);
        if (link == null)
        {
            await _context.StudentGuardians.AddAsync(new StudentGuardian
            {
                StudentId = studentId,
                GuardianId = guardianId,
                RelationshipId = relationshipId,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private async Task<Class> EnsureClassAsync(string name, Guid teacherId, string room, int totalSessions)
    {
        var classEntity = await _context.Classes.FirstOrDefaultAsync(item => item.ClassName == name && item.TeacherId == teacherId);
        if (classEntity == null)
        {
            classEntity = new Class { ClassName = name, TeacherId = teacherId, CreatedAt = DateTime.UtcNow };
            await _context.Classes.AddAsync(classEntity);
        }

        classEntity.RoomName = room;
        classEntity.MaxStudents = 25;
        classEntity.StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-60));
        classEntity.EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(60));
        classEntity.TotalSessions = totalSessions;
        classEntity.UpdatedAt = DateTime.UtcNow;
        return classEntity;
    }

    private async Task EnsureEnrollmentAsync(Guid classId, Guid studentId)
    {
        var enrollment = await _context.ClassStudents.FirstOrDefaultAsync(item => item.ClassId == classId && item.StudentId == studentId);
        if (enrollment == null)
        {
            await _context.ClassStudents.AddAsync(new ClassStudent
            {
                ClassId = classId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow,
                IsEnable = true,
                Status = "ACTIVE"
            });
            return;
        }

        enrollment.IsEnable = true;
        enrollment.Status = "ACTIVE";
    }

    private async Task<Schedule> EnsureScheduleAsync(Guid classId, DateOnly date, TimeOnly start, TimeOnly end, string topic)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(item => item.ClassId == classId && item.ScheduleDate == date && item.StartTime == start);
        if (schedule == null)
        {
            schedule = new Schedule { ClassId = classId, ScheduleDate = date, StartTime = start, CreatedAt = DateTime.UtcNow };
            await _context.Schedules.AddAsync(schedule);
        }

        schedule.EndTime = end;
        schedule.Topic = topic;
        schedule.UpdatedAt = DateTime.UtcNow;
        return schedule;
    }

    private async Task EnsureAttendanceAsync(Guid scheduleId, Guid studentId, Guid teacherId, int statusId)
    {
        var attendance = await _context.AttendanceChecks.FirstOrDefaultAsync(item => item.ScheduleId == scheduleId && item.StudentId == studentId);
        if (attendance == null)
        {
            attendance = new AttendanceCheck { ScheduleId = scheduleId, StudentId = studentId };
            await _context.AttendanceChecks.AddAsync(attendance);
        }

        attendance.StatusId = statusId;
        attendance.CheckedBy = teacherId;
        attendance.CheckedAt = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;
    }

    private async Task EnsureTuitionAsync(Guid studentId, Guid? classId, int statusId, decimal total, decimal paid, DateOnly dueDate)
    {
        var fee = await _context.TuitionFees.FirstOrDefaultAsync(item => item.StudentId == studentId && item.ClassId == classId && item.DueDate == dueDate);
        if (fee == null)
        {
            fee = new TuitionFee { StudentId = studentId, ClassId = classId, DueDate = dueDate, CreatedAt = DateTime.UtcNow };
            await _context.TuitionFees.AddAsync(fee);
        }

        fee.StatusId = statusId;
        fee.TotalAmount = total;
        fee.PaidAmount = paid;
        fee.UpdatedAt = DateTime.UtcNow;
    }

    private async Task EnsureNotificationAsync(Guid senderId, Guid receiverId, string title, string content)
    {
        if (await _context.Notifications.AnyAsync(item => item.ReceiverId == receiverId && item.Title == title)) return;

        await _context.Notifications.AddAsync(new Notification
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Title = title,
            Content = content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
    }
}
