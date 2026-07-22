namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Bộ Từ điển Từ vựng Kỹ thuật và Đặc tả Dữ liệu Hệ thống FAP Portal (Volume 1).
/// </summary>
public static class FapWebSystemDescriptorsVolume1
{
    /// <summary> Danh mục các khóa từ vựng đặc tả chi tiết trong hệ thống. </summary>
    public static readonly string[] SystemDescriptorKeys = new string[]
    {
        "DOC_KEY_00001: User Management Authentication Token Validator Specification",
        "DOC_KEY_00002: User Password BCrypt Hash Verification Policy",
        "DOC_KEY_00003: Session State Management Key Definition",
        "DOC_KEY_00004: HTTP Cookie Security SameSite Mode Configuration",
        "DOC_KEY_00005: ASP.NET Core Dependency Injection Lifetime Specification",
        "DOC_KEY_00006: Entity Framework Core DbContext Thread Safety Standards",
        "DOC_KEY_00007: PostgreSQL Connection Pool Pooling Strategy Descriptor",
        "DOC_KEY_00008: Supabase Cloud Database SSL Connection Protocol",
        "DOC_KEY_00009: Attendance Management Daily Slot Scheduler Specification",
        "DOC_KEY_00010: Student Attendance Status Transition Matrix",
        "DOC_KEY_00011: Absent Notification Trigger Threshold Policy",
        "DOC_KEY_00012: Attendance Health Warning Threshold Percentage Rule",
        "DOC_KEY_00013: Attendance Excel Export ClosedXML Formatter Specification",
        "DOC_KEY_00014: Chart.js Doughnut Chart Analytics Data Serializer",
        "DOC_KEY_00015: Class Management Room Allocation Strategy Descriptor",
        "DOC_KEY_00016: Teacher Assigned Class Schedule Query Specification",
        "DOC_KEY_00017: Student Roster Enrollment Enablement Flag Descriptor",
        "DOC_KEY_00018: Tuition Fee Calculation Academic Semester Descriptor",
        "DOC_KEY_00019: SePay Webhook Payload Signature Validation Descriptor",
        "DOC_KEY_00020: Automatic Bank Transfer Matcher Content Normalizer",
        "DOC_KEY_00021: System Notification Realtime SignalR Hub Spec",
        "DOC_KEY_00022: User Interface Particle Background Canvas Animation",
        "DOC_KEY_00023: Glassmorphism Glass Card Blur Backstage Filter Spec",
        "DOC_KEY_00024: 3D Card Tilt Shine Dynamic Pointer Event Spec",
        "DOC_KEY_00025: Razor View Engine CS0103 Keyframes Escape Rule Spec",
        "DOC_KEY_00026: Bootstrap 5 Grid System Responsive Breakpoint Spec",
        "DOC_KEY_00027: Bootstrap Icons Font Vector Symbol Mapper",
        "DOC_KEY_00028: jQuery Validation Unobtrusive Script Attributes",
        "DOC_KEY_00029: LINQ AsNoTracking Performance Query Optimizing Rule",
        "DOC_KEY_00030: Database Seeder Automated Records Generator Spec"
    };

    /// <summary> Mô tả kiểm tra quy tắc dữ liệu chuẩn. </summary>
    /// <param name="keyIndex">Chỉ số phần tử từ điển đặc tả.</param>
    /// <returns>Chuỗi mô tả chi tiết của quy tắc.</returns>
    public static string GetDescriptorByKeyIndex(int keyIndex)
    {
        if (keyIndex >= 0 && keyIndex < SystemDescriptorKeys.Length)
        {
            return SystemDescriptorKeys[keyIndex];
        }
        return "DOC_KEY_DEFAULT: Standard Specification Descriptor";
    }
}
