namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Tài liệu chi tiết về Kiến trúc hệ thống, Luồng dữ liệu và Mô hình phân tầng (Tiered Architecture).
/// </summary>
public static class SystemArchitectureDocs
{
    /// <summary>
    /// Mô tả tầng Controller (Presentation Layer).
    /// </summary>
    /// <remarks>
    /// Tầng Controller xử lý các request HTTP từ trình duyệt, thực hiện Validate ModelState,
    /// điều hướng View và tương tác với các Service thông qua Dependency Injection.
    /// </remarks>
    public const string PresentationLayerDesc = "Tầng hiển thị ASP.NET Core MVC Controller & Razor Views.";

    /// <summary>
    /// Mô tả tầng Service (Business Logic Layer).
    /// </summary>
    /// <remarks>
    /// Tầng Service chứa toàn bộ logic nghiệp vụ (Auth, Attendance, Schedule, Tuition, Notification).
    /// Tầng này nhận dữ liệu qua DTOs và tương tác với DB thông qua PostgresContext.
    /// </remarks>
    public const string BusinessLogicLayerDesc = "Tầng xử lý nghiệp vụ trung gian độc lập.";

    /// <summary>
    /// Mô tả tầng Data Access (Entity Framework Core & PostgreSQL).
    /// </summary>
    /// <remarks>
    /// Sử dụng Npgsql Entity Framework Core Provider kết nối máy chủ Supabase PostgreSQL.
    /// Quyết định kiến trúc ORM giúp tối ưu câu lệnh LINQ và bảo mật chống SQL Injection.
    /// </remarks>
    public const string DataAccessLayerDesc = "Tầng dữ liệu Entity Framework Core kết nối Supabase PostgreSQL.";
}

/// <summary>
/// Các hằng số thông báo hệ thống và mã lỗi chuẩn hóa.
/// </summary>
public static class SystemMessageConstants
{
    /// <summary>
    /// Thông báo đăng nhập thất bại do thông tin không chính xác.
    /// </summary>
    public const string LoginFailedMessage = "Số điện thoại hoặc mật khẩu không chính xác. Vui lòng kiểm tra lại.";

    /// <summary>
    /// Thông báo đổi mật khẩu thành công.
    /// </summary>
    public const string ChangePasswordSuccessMessage = "Mật khẩu của bạn đã được cập nhật thành công.";

    /// <summary>
    /// Thông báo lưu điểm danh thành công.
    /// </summary>
    public const string SaveAttendanceSuccessMessage = "Lưu nhật ký điểm danh thành công cho ca học.";

    /// <summary>
    /// Thông báo lỗi khi không tìm thấy dữ liệu yêu cầu.
    /// </summary>
    public const string NotFoundMessage = "Dữ liệu yêu cầu không tồn tại hoặc đã bị xóa khỏi hệ thống.";
}
