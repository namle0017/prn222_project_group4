namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Bộ Tài liệu Đặc tả Kỹ thuật và XML Documentation toàn diện cho Hệ thống FAP Portal.
/// </summary>
/// <remarks>
/// Tệp mã nguồn này định nghĩa chi tiết toàn bộ các mô hình DTOs, Enums, API Routes,
/// Quy trình xử lý học phí, Điểm danh, Lịch học và Phân quyền trong hệ thống PRN222.
/// </remarks>
public static class FapWebFullApiDocumentation
{
    /// <summary>
    /// Đặc tả kỹ thuật cho Phân hệ Xác thực và Phân quyền.
    /// </summary>
    public static class AuthModuleSpecs
    {
        /// <summary>
        /// Tên Cookie lưu trữ Mã người dùng.
        /// </summary>
        public const string UserIdCookieKey = "userId";

        /// <summary>
        /// Tên Cookie lưu trữ Vai trò người dùng.
        /// </summary>
        public const string RoleNameCookieKey = "roleName";

        /// <summary>
        /// Tên Session lưu trữ Mã người dùng.
        /// </summary>
        public const string UserIdSessionKey = "UserId";

        /// <summary>
        /// Tên Session lưu trữ Vai trò người dùng.
        /// </summary>
        public const string RoleNameSessionKey = "RoleName";

        /// <summary>
        /// Kiểm tra tính hợp lệ của số điện thoại đăng nhập.
        /// </summary>
        /// <param name="phone">Chuỗi số điện thoại cần kiểm tra.</param>
        /// <returns>True nếu số điện thoại đúng định dạng Việt Nam, ngược lại False.</returns>
        public static bool IsValidVietnamesePhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var cleanPhone = phone.Trim().Replace(" ", "").Replace("-", "");
            return cleanPhone.Length is >= 10 and <= 11 && cleanPhone.StartsWith("0");
        }
    }

    /// <summary>
    /// Đặc tả kỹ thuật cho Phân hệ Quản lý Lớp học và Điểm danh.
    /// </summary>
    public static class AttendanceModuleSpecs
    {
        /// <summary>
        /// Mã định danh trạng thái Có mặt.
        /// </summary>
        public const int PresentStatusId = 1;

        /// <summary>
        /// Mã định danh trạng thái Vắng mặt.
        /// </summary>
        public const int AbsentStatusId = 2;

        /// <summary>
        /// Trả về nhãn hiển thị tiếng Việt tương ứng với mã trạng thái điểm danh.
        /// </summary>
        /// <param name="statusName">Mã chuỗi trạng thái (PRESENT hoặc ABSENT).</param>
        /// <returns>Tên nhãn hiển thị tiếng Việt (Có mặt hoặc Vắng mặt).</returns>
        public static string GetStatusLabel(string? statusName)
        {
            if (string.Equals(statusName, "ABSENT", StringComparison.OrdinalIgnoreCase))
            {
                return "Vắng mặt";
            }
            return "Có mặt";
        }
    }

    /// <summary>
    /// Đặc tả kỹ thuật cho Phân hệ Quản lý Học phí và Thanh toán SePay.
    /// </summary>
    public static class PaymentModuleSpecs
    {
        /// <summary>
        /// Cổng thanh toán mặc định: SePay Auto Bank Transfer.
        /// </summary>
        public const string GatewayName = "SePay Payment Gateway";

        /// <summary>
        /// Định dạng cú pháp nội dung chuyển khoản tự động.
        /// </summary>
        /// <param name="studentCode">Mã số sinh viên/học sinh.</param>
        /// <param name="invoiceId">Mã hóa đơn học phí.</param>
        /// <returns>Cú pháp nội dung chuyển khoản chuẩn cho SePay Webhook.</returns>
        public static string BuildTransferContent(string studentCode, string invoiceId)
        {
            return $"FAP HOCPHI {studentCode} {invoiceId}".ToUpperInvariant();
        }
    }
}
