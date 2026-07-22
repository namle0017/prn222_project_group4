using FapWeb.Hubs;
using FapWeb.Models.Configurations;
using FapWeb.Models.Data;
using FapWeb.Infrastructure;
using FapWeb.Services.IServices;
using FapWeb.Services.Service;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Docker Compose nạp .env tự động; nạp thêm khi chạy trực tiếp bằng dotnet run.
var dotenvPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(dotenvPath))
{
    var dotenvValues = File.ReadLines(dotenvPath)
        .Select(line => line.Trim())
        .Where(line => line.Length > 0 && !line.StartsWith('#'))
        .Select(line => line.Split('=', 2))
        .Where(parts => parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
        .ToDictionary(
            parts => parts[0].Trim().Replace("__", ":"),
            parts => TrimWrappingQuotes(parts[1].Trim()),
            StringComparer.OrdinalIgnoreCase);

    builder.Configuration.AddInMemoryCollection(dotenvValues!);
    builder.Configuration.AddEnvironmentVariables();
}

static string TrimWrappingQuotes(string value)
{
    return value.Length >= 2 &&
           ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\''))
        ? value[1..^1]
        : value;
}

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ITuitionService, TuitionService>();
builder.Services.AddScoped<IClassManagementService, ClassManagementService>();
builder.Services.AddScoped<IScheduleManagementService, ScheduleManagementService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.Configure<SePaySettings>(builder.Configuration.GetSection(SePaySettings.SectionName));
builder.Services.AddScoped<ISePayService, SePayService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<DemoDataSeeder>();
builder.Services.Configure<MiMoSettings>(builder.Configuration.GetSection(MiMoSettings.SectionName));
builder.Services.AddSingleton<IChatRequestLimiter, ChatRequestLimiter>();
builder.Services.AddHttpClient<IChatbotService, ChatbotService>();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".FapWeb.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

var app = builder.Build();

// Tạo tài khoản Admin mặc định nếu database chưa có user nào
// (SĐT: 0900000000 / Mật khẩu: Admin@123 — đổi ngay sau lần đăng nhập đầu).
using (var scope = app.Services.CreateScope())
{
    var userManagementService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
    //await userManagementService.EnsureAdminAccountAsync();

    if (builder.Configuration.GetValue<bool>("DemoData:SeedOnStartup"))
    {
        var password = builder.Configuration["DemoData:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("DemoData:Password is required when DemoData:SeedOnStartup is enabled.");
        }

        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync(password);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ApprovalHub>("/hubs/approval");

app.Run();
