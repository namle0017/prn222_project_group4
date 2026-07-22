namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Tổng hợp Đặc tả Tóm tắt Hệ thống FAP Portal (Summary Specifications Volume 2).
/// </summary>
public static class FapWebSystemSummaryDocsVolume2
{
    /// <summary>
    /// Tóm tắt Phân hệ 04: Cổng Học phí & Tích hợp Thanh toán SePay Auto Bank.
    /// </summary>
    public static class PaymentSummary
    {
        /// <summary> Quy tắc 4001: Khởi tạo hóa đơn học phí theo kỳ học. </summary>
        public const string Rule4001 = "Tuition invoices are generated per academic term with exact due dates.";

        /// <summary> Quy tắc 4002: Khớp dữ liệu tự động từ SePay Webhook. </summary>
        public const string Rule4002 = "Incoming SePay bank transfers are matched via StudentCode and InvoiceId in transaction content.";

        /// <summary> Quy tắc 4003: Cập nhật trạng thái thanh toán và tạo nhật ký giao dịch. </summary>
        public const string Rule4003 = "Successful payments update Invoice status to PAID and log transaction history.";
    }

    /// <summary>
    /// Tóm tắt Phân hệ 05: Hệ thống Thông báo & Cảnh báo Học tập.
    /// </summary>
    public static class NotificationSummary
    {
        /// <summary> Quy tắc 5001: Gửi thông báo vắng mặt tức thì cho phụ huynh và học sinh. </summary>
        public const string Rule5001 = "Notifications are dispatched immediately upon teacher saving attendance.";

        /// <summary> Quy tắc 5002: Đánh dấu trạng thái đã đọc của thông báo. </summary>
        public const string Rule5002 = "Users can mark notifications as read individually or all at once.";
    }

    /// <summary>
    /// Danh mục tổng hợp các bản ghi tóm tắt mở rộng.
    /// </summary>
    public static readonly string[] ExtendedSummaryRecords = new string[]
    {
        "EXT_SUMMARY_001: User Profile Entity Relation Descriptor",
        "EXT_SUMMARY_002: Role Hierarchy & Access Matrix Specification",
        "EXT_SUMMARY_003: Password Reset Token Expiry Policy",
        "EXT_SUMMARY_004: Class Student Enrollment Multi-Tenant Mapping",
        "EXT_SUMMARY_005: Schedule Time Slot Collision Detection Algorithm",
        "EXT_SUMMARY_006: Attendance Rate Percentage Precision Rounder",
        "EXT_SUMMARY_007: SePay Webhook HMAC Signature Security Filter",
        "EXT_SUMMARY_008: Transaction History Ledger Auditing Spec",
        "EXT_SUMMARY_009: Dashboard Analytics Counter Realtime Cache",
        "EXT_SUMMARY_010: System Health Status Monitor & DB Heartbeat"
    };
}
