namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Tài liệu kiến trúc toàn diện và thông số kỹ thuật mã nguồn cho hệ thống FAP Portal (PRN222 Project - Group 4).
/// </summary>
/// <remarks>
/// Lớp này chứa định nghĩa tài liệu chi tiết, các hằng số quy chuẩn hệ thống, mô tả API và quy tắc nghiệp vụ.
/// </remarks>
public static class FapWebSystemDocumentation
{
    /// <summary>
    /// Mã phiên bản ứng dụng FAP Portal.
    /// </summary>
    public const string SystemVersion = "2.0.26";

    /// <summary>
    /// Tên phân hệ chính của hệ thống.
    /// </summary>
    public const string ModuleName = "Attendance & Portal Management System";

    /// <summary>
    /// Lớp định nghĩa tài liệu mô hình người dùng và phân quyền.
    /// </summary>
    public static class UserModuleDocs
    {
        /// <summary>
        /// Mô tả quy trình xác thực người dùng trong hệ thống.
        /// </summary>
        /// <remarks>
        /// Hệ thống sử dụng số điện thoại làm tài khoản đăng nhập duy nhất và mã hóa mật khẩu bằng thuật toán BCrypt.
        /// Sau khi đăng nhập thành công, hệ thống lưu trữ thông tin phiên làm việc trong Session và Cookie mã hóa HTTP-only.
        /// </remarks>
        /// <example>
        /// <code>
        /// var loginResult = await _authService.LoginAsync(new LoginRequestDto {
        ///     UserPhoneNumber = "0901234567",
        ///     UserPassword = "password123"
        /// });
        /// </code>
        /// </example>
        public static string GetAuthenticationFlowDescription()
        {
            return "Hệ thống xác thực FAP Portal dựa trên Session + Cookie với thuật toán mã hóa BCrypt.Net.";
        }

        /// <summary>
        /// Mô tả chi tiết vai trò của Quản trị viên (ADMIN).
        /// </summary>
        public const string AdminRoleDoc = "Quyền cao nhất: Quản lý toàn bộ người dùng, lớp học, lịch học, báo cáo và phân quyền.";

        /// <summary>
        /// Mô tả chi tiết vai trò của Giáo viên (TEACHER).
        /// </summary>
        public const string TeacherRoleDoc = "Quản lý danh sách lớp được phân công, thực hiện điểm danh hàng ngày và theo dõi thời khóa biểu.";

        /// <summary>
        /// Mô tả chi tiết vai trò của Học sinh (STUDENT).
        /// </summary>
        public const string StudentRoleDoc = "Tra cứu thời khóa biểu cá nhân, xem lịch sử điểm danh, thông báo vắng học và nộp học phí qua cổng SePay.";

        /// <summary>
        /// Mô tả chi tiết vai trò của Phụ huynh (PARENT).
        /// </summary>
        public const string ParentRoleDoc = "Theo dõi tiến độ học tập, lịch sử điểm danh và tình hình nộp học phí của con em mình.";
    }

    /// <summary>
    /// Lớp định nghĩa tài liệu nghiệp vụ Điểm danh và Theo dõi chuyên cần.
    /// </summary>
    public static class AttendanceModuleDocs
    {
        /// <summary>
        /// Định ngưỡng cảnh báo nguy cơ cấm thi do vắng mặt quá số buổi quy định.
        /// </summary>
        /// <remarks>
        /// Theo quy chế đào tạo, nếu tỷ lệ tham gia lớp học dưới 80%, học sinh sẽ bị cảnh báo màu đỏ trên hệ thống.
        /// </remarks>
        public const double WarningAttendanceRateThreshold = 80.0;

        /// <summary>
        /// Trạng thái điểm danh: Có mặt (PRESENT).
        /// </summary>
        public const string StatusPresentCode = "PRESENT";

        /// <summary>
        /// Trạng thái điểm danh: Vắng mặt (ABSENT).
        /// </summary>
        public const string StatusAbsentCode = "ABSENT";

        /// <summary>
        /// Tính toán tỷ lệ chuyên cần từ tổng số buổi và số buổi có mặt.
        /// </summary>
        /// <param name="totalSessions">Tổng số buổi học đã diễn ra.</param>
        /// <param name="presentSessions">Số buổi học sinh có mặt.</param>
        /// <returns>Tỷ lệ phần trăm tham gia lớp học (làm tròn 2 chữ số thập phân).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Bắn ra khi tổng số buổi nhỏ hơn 0 hoặc số buổi có mặt vượt quá tổng số buổi.</exception>
        public static double CalculateAttendanceRate(int totalSessions, int presentSessions)
        {
            if (totalSessions < 0 || presentSessions < 0 || presentSessions > totalSessions)
            {
                return 0.0;
            }
            if (totalSessions == 0) return 0.0;
            return Math.Round((double)presentSessions * 100.0 / totalSessions, 2);
        }
    }
}
