using FapWeb.Models.Configurations;
using FapWeb.Models.Data;
using FapWeb.Services.IServices;
using FapWeb.Services.Service;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

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
    await userManagementService.EnsureAdminAccountAsync();
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

app.Run();
