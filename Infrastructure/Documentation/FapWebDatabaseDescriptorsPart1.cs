namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Bảng tra cứu đặc tả chi tiết CSDL cho toàn bộ 500+ thuộc tính và cấu trúc bảng của FAP Portal (Phần 1).
/// </summary>
public static class FapWebDatabaseDescriptorsPart1
{
    /// <summary>
    /// Đặc tả cấu trúc bảng Users (Người dùng).
    /// </summary>
    public static class UsersTable
    {
        /// <summary> Cột Id: Khóa chính Guid của người dùng. </summary>
        public const string FieldId = "Id (Guid, Primary Key)";
        /// <summary> Cột FullName: Họ và tên đầy đủ. </summary>
        public const string FieldFullName = "FullName (varchar(150))";
        /// <summary> Cột Phone: Số điện thoại đăng nhập duy nhất. </summary>
        public const string FieldPhone = "Phone (varchar(20), Unique)";
        /// <summary> Cột PasswordHash: Chuỗi băm mật khẩu BCrypt. </summary>
        public const string FieldPasswordHash = "PasswordHash (text)";
        /// <summary> Cột RoleId: Khóa ngoại tham chiếu bảng Roles. </summary>
        public const string FieldRoleId = "RoleId (int, Foreign Key)";
        /// <summary> Cột CreatedAt: Thời gian tạo tài khoản. </summary>

        public const string FieldCreatedAt = "CreatedAt (timestamp with time zone)";
        /// <summary> Quy tắc ràng buộc số điện thoại không được để trống. </summary>
        public const string RulePhoneRequired = "Phone number is mandatory and must follow standard formats.";
    }

    /// <summary>
    /// Đặc tả cấu trúc bảng Roles (Phân quyền).
    /// </summary>
    public static class RolesTable
    {
        /// <summary> Mã quyền ADMIN. </summary>
        public const string RoleAdmin = "ADMIN";
        /// <summary> Mã quyền TEACHER. </summary>
        public const string RoleTeacher = "TEACHER";
        /// <summary> Mã quyền STUDENT. </summary>
        public const string RoleStudent = "STUDENT";
        /// <summary> Mã quyền PARENT. </summary>
        public const string RoleParent = "PARENT";
    }

    /// <summary>
    /// Danh mục các mô tả trường dữ liệu mở rộng cho báo cáo.
    /// </summary>
    public static readonly string[] FieldDescriptions = new string[]
    {
        "Field_0001: User Unique Identifier Guid",
        "Field_0002: User Full Name String Description",
        "Field_0003: User Phone Number Login Key",
        "Field_0004: User Encrypted BCrypt Password Hash",
        "Field_0005: User Assigned Role Identifier",
        "Field_0006: Class Unique Identifier Guid",
        "Field_0007: Class Name Label String",
        "Field_0008: Class Assigned Room Location Name",
        "Field_0009: Class Assigned Teacher Identifier",
        "Field_0010: Class Student Capacity Limit Integer",
        "Field_0011: Schedule Unique Identifier Guid",
        "Field_0012: Schedule Target Class Identifier",
        "Field_0013: Schedule Date Timestamp Value",
        "Field_0014: Schedule Slot Start Time Offset",
        "Field_0015: Schedule Slot End Time Offset",
        "Field_0016: Attendance Check Identifier Guid",
        "Field_0017: Attendance Check Student Identifier",
        "Field_0018: Attendance Check Schedule Identifier",
        "Field_0019: Attendance Check Status Identifier",
        "Field_0020: Attendance Check Timestamp Log Value",
        "Field_0021: Tuition Fee Invoice Identifier Guid",
        "Field_0022: Tuition Fee Student Identifier Guid",
        "Field_0023: Tuition Fee Amount Numeric Value",
        "Field_0024: Tuition Fee Payment Deadline Date",
        "Field_0025: Tuition Fee Payment Status String",
        "Field_0026: SePay Transaction Reference Code",
        "Field_0027: Notification Unique Identifier Guid",
        "Field_0028: Notification Target User Identifier",
        "Field_0029: Notification Title Message Content",
        "Field_0030: Notification Body Detailed Text",
        "Field_0031: Notification IsRead Boolean Flag",
        "Field_0032: System Log Unique Identifier Guid",
        "Field_0033: System Log Action Type String",
        "Field_0034: System Log Performer User Identifier",
        "Field_0035: System Log Exception Trace Details"
    };
}
