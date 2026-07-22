using System.Net;
using FapWeb.Infrastructure;
using FapWeb.Models.Configurations;
using FapWeb.Models.Data;
using FapWeb.Models.Entities;
using FapWeb.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FapWeb.Tests;

public class ChatbotAttendanceSummaryTests
{
    [Fact]
    public async Task ParentSummary_CountsOnlyLinkedChild_AndDoesNotCallMiMo()
    {
        await using var context = CreateContext();
        var data = await SeedAttendanceDataAsync(context);
        var handler = new CountingHandler();
        var service = CreateService(context, handler);

        var response = await service.AskAsync(
            "Con tôi đã có mặt và vắng bao nhiêu buổi?",
            data.ParentId,
            AppRoles.Parent);

        Assert.True(response.IsAvailable);
        Assert.Contains("Nguyen Minh Khang", response.Answer);
        Assert.Contains("3 buổi", response.Answer);
        Assert.Contains("2 buổi có mặt", response.Answer);
        Assert.Contains("1 buổi vắng", response.Answer);
        Assert.Contains("66,7%", response.Answer);
        Assert.Contains("1 buổi đã qua chưa có kết quả", response.Answer);
        Assert.DoesNotContain("Hoc Sinh Khong Lien Ket", response.Answer);
        Assert.Equal("/Attendance/History", response.SuggestedActionUrl);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task ParentSummary_CanFilterByProgrammingClass()
    {
        await using var context = CreateContext();
        var data = await SeedAttendanceDataAsync(context);
        var service = CreateService(context, new CountingHandler());

        var response = await service.AskAsync(
            "Môn ASP.NET Core con tôi có mặt và vắng bao nhiêu buổi?",
            data.ParentId,
            AppRoles.Parent);

        Assert.Contains("Lập trình Web ASP.NET Core", response.Answer);
        Assert.Contains("2 buổi", response.Answer);
        Assert.Contains("1 buổi có mặt", response.Answer);
        Assert.Contains("1 buổi vắng", response.Answer);
        Assert.DoesNotContain("C# .NET", response.Answer);
    }

    [Fact]
    public async Task StudentSummary_UsesOnlyCurrentStudent()
    {
        await using var context = CreateContext();
        var data = await SeedAttendanceDataAsync(context);
        var service = CreateService(context, new CountingHandler());

        var response = await service.AskAsync(
            "Tỷ lệ chuyên cần của tôi là bao nhiêu?",
            data.StudentId,
            AppRoles.Student);

        Assert.Contains("Nguyen Minh Khang", response.Answer);
        Assert.Contains("66,7%", response.Answer);
        Assert.DoesNotContain("Hoc Sinh Khong Lien Ket", response.Answer);
    }

    private static PostgresContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PostgresContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PostgresContext(options);
    }

    private static ChatbotService CreateService(PostgresContext context, CountingHandler handler)
    {
        var settings = Options.Create(new MiMoSettings
        {
            ApiBaseUrl = "https://example.invalid/v1/chat/completions",
            ApiKey = string.Empty,
            Model = "mimo-v2.5",
            MaxQuestionLength = 500,
            MaxCompletionTokens = 500,
            RequestTimeoutSeconds = 20
        });
        return new ChatbotService(context, new HttpClient(handler), settings, NullLogger<ChatbotService>.Instance);
    }

    private static async Task<(Guid ParentId, Guid StudentId)> SeedAttendanceDataAsync(PostgresContext context)
    {
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var hiddenStudentId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var csharpClassId = Guid.NewGuid();
        var webClassId = Guid.NewGuid();
        var hiddenClassId = Guid.NewGuid();
        var present = new AttendanceCheckStatus { Id = 1, StatusName = "PRESENT" };
        var absent = new AttendanceCheckStatus { Id = 2, StatusName = "ABSENT" };
        var parent = NewUser(parentId, "Nguyen Van Long", 4);
        var student = NewUser(studentId, "Nguyen Minh Khang", 3);
        var hiddenStudent = NewUser(hiddenStudentId, "Hoc Sinh Khong Lien Ket", 3);
        var teacher = NewUser(teacherId, "Tran Thi Mai", 2);
        var csharpClass = new Class { Id = csharpClassId, ClassName = "Lập trình C# .NET cơ bản", TeacherId = teacherId, TotalSessions = 10 };
        var webClass = new Class { Id = webClassId, ClassName = "Lập trình Web ASP.NET Core", TeacherId = teacherId, TotalSessions = 10 };
        var hiddenClass = new Class { Id = hiddenClassId, ClassName = "Lớp dữ liệu riêng", TeacherId = teacherId, TotalSessions = 10 };

        context.AddRange(parent, student, hiddenStudent, teacher, present, absent, csharpClass, webClass, hiddenClass);
        context.StudentGuardians.Add(new StudentGuardian { Id = Guid.NewGuid(), GuardianId = parentId, StudentId = studentId });
        context.ClassStudents.AddRange(
            new ClassStudent { Id = Guid.NewGuid(), ClassId = csharpClassId, StudentId = studentId, IsEnable = true },
            new ClassStudent { Id = Guid.NewGuid(), ClassId = webClassId, StudentId = studentId, IsEnable = true },
            new ClassStudent { Id = Guid.NewGuid(), ClassId = hiddenClassId, StudentId = hiddenStudentId, IsEnable = true });

        var today = DateOnly.FromDateTime(DateTime.Today);
        var csharpPresent = NewSchedule(csharpClassId, today.AddDays(-4), 8);
        var webPresent = NewSchedule(webClassId, today.AddDays(-3), 13);
        var webAbsent = NewSchedule(webClassId, today.AddDays(-2), 13);
        var webUnmarked = NewSchedule(webClassId, today.AddDays(-1), 13);
        var hiddenAbsent = NewSchedule(hiddenClassId, today.AddDays(-2), 8);
        context.Schedules.AddRange(csharpPresent, webPresent, webAbsent, webUnmarked, hiddenAbsent);
        context.AttendanceChecks.AddRange(
            NewAttendance(csharpPresent.Id, studentId, teacherId, present.Id),
            NewAttendance(webPresent.Id, studentId, teacherId, present.Id),
            NewAttendance(webAbsent.Id, studentId, teacherId, absent.Id),
            NewAttendance(hiddenAbsent.Id, hiddenStudentId, teacherId, absent.Id));
        await context.SaveChangesAsync();
        return (parentId, studentId);
    }

    private static User NewUser(Guid id, string fullName, int roleId) => new()
    {
        Id = id,
        FullName = fullName,
        RoleId = roleId,
        PasswordHash = "test",
        IsActive = true
    };

    private static Schedule NewSchedule(Guid classId, DateOnly date, int startHour) => new()
    {
        Id = Guid.NewGuid(),
        ClassId = classId,
        ScheduleDate = date,
        StartTime = new TimeOnly(startHour, 0),
        EndTime = new TimeOnly(startHour + 2, 0)
    };

    private static AttendanceCheck NewAttendance(Guid scheduleId, Guid studentId, Guid teacherId, int statusId) => new()
    {
        Id = Guid.NewGuid(),
        ScheduleId = scheduleId,
        StudentId = studentId,
        CheckedBy = teacherId,
        StatusId = statusId,
        CheckedAt = DateTime.UtcNow
    };

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }
    }
}
