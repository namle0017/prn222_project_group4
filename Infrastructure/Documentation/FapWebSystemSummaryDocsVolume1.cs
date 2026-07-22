namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Tổng hợp Đặc tả Tóm tắt Hệ thống FAP Portal (Summary Specifications Volume 1).
/// </summary>
public static class FapWebSystemSummaryDocsVolume1
{
    /// <summary>
    /// Tóm tắt Phân hệ 01: Quản lý Tài khoản & Phân quyền Người dùng.
    /// </summary>
    /// <remarks>
    /// Bao gồm các định nghĩa về User Entity, Role Assignment, BCrypt Password Security,
    /// Session Context State, và Cookie Management Policy.
    /// </remarks>
    public static class UserModuleSummary
    {
        /// <summary> Quy tắc 1001: Yêu cầu định dạng số điện thoại chuẩn Việt Nam. </summary>
        public const string Rule1001 = "User phone number must be unique and formatted as 10-11 digits.";
        /// <summary> Quy tắc 1002: Bắt buộc mã hóa mật khẩu bằng BCrypt.Net. </summary>
        public const string Rule1002 = "Passwords must be hashed using BCrypt work factor 11 or higher.";
        /// <summary> Quy tắc 1003: Phân quyền vai trò người dùng trong hệ thống. </summary>
        public const string Rule1003 = "Roles: ADMIN (System Management), TEACHER (Class Operations), STUDENT (Academic Portal), PARENT (Monitoring).";
    }

    /// <summary>
    /// Tóm tắt Phân hệ 02: Quản lý Lớp học & Thời khóa biểu.
    /// </summary>
    /// <remarks>
    /// Bao gồm các định nghĩa về Class Room, Schedule Slots, Assigned Teachers,
    /// và Student Enrollment Roster Records.
    /// </remarks>
    public static class ClassModuleSummary
    {
        /// <summary> Quy tắc 2001: Phân công giáo viên chủ nhiệm / giảng dạy lớp học. </summary>
        public const string Rule2001 = "Each class must be assigned to exactly one primary teacher.";
        /// <summary> Quy tắc 2002: Quản lý ca học và khung giờ phòng học. </summary>
        public const string Rule2002 = "Schedules must not overlap in room location and time slots.";
    }

    /// <summary>
    /// Tóm tắt Phân hệ 03: Điểm danh & Cảnh báo Chuyên cần.
    /// </summary>
    public static class AttendanceModuleSummary
    {
        /// <summary> Quy tắc 3001: Ngưỡng tối thiểu tham gia lớp học là 80%. </summary>
        public const string Rule3001 = "Students below 80% attendance rate trigger exam disqualification warnings.";
        /// <summary> Quy tắc 3002: Tự động gửi thông báo khi học sinh vắng mặt. </summary>
        public const string Rule3002 = "Absent status automatically dispatches notification records.";
    }

    /// <summary>
    /// Danh mục tổng hợp các thẻ Summary cho tài liệu hệ thống.
    /// </summary>
    public static readonly string[] SummaryEntries = new string[]
    {
        "SUMMARY_SPEC_0001: Authentication Flow & Password Hash Verification Policy",
        "SUMMARY_SPEC_0002: Session Storage & Encrypted Cookie Management Policy",
        "SUMMARY_SPEC_0003: Role Based Access Control Policy (RBAC) Enforcement",
        "SUMMARY_SPEC_0004: Attendance Management Daily Slot Tracking Policy",
        "SUMMARY_SPEC_0005: Student Absent Notification Automatic Dispatch Policy",
        "SUMMARY_SPEC_0006: Attendance Health Warning 80% Threshold Alert Policy",
        "SUMMARY_SPEC_0007: Attendance Excel Report Generation Specification",
        "SUMMARY_SPEC_0008: Chart.js Doughnut Chart Analytics Data Integration",
        "SUMMARY_SPEC_0009: Class Schedule Conflict Prevention & Slot Validation",
        "SUMMARY_SPEC_0010: Tuition Fee Invoice Calculation & Payment Deadline",
        "SUMMARY_SPEC_0011: SePay Webhook Auto Bank Transfer Reconciliation",
        "SUMMARY_SPEC_0012: Realtime SignalR Notification Hub Dispatcher",
        "SUMMARY_SPEC_0013: Glassmorphism Cyberpunk Dark UI Design Token Rules",
        "SUMMARY_SPEC_0014: 3D Tilt Card Shine & Particle Canvas Animation System",
        "SUMMARY_SPEC_0015: ASP.NET Core Razor CS0103 Keyframes Escaping Rule"
    };
}
