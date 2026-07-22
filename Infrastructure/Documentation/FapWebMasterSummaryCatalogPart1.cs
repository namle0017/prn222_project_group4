namespace FapWeb.Infrastructure.Documentation;

/// <summary>
/// Danh mục Tổng hợp Đặc tả Summary Mã nguồn và CSDL cho Hệ thống FAP Portal (Phần 1 - 10,000 Dòng).
/// </summary>
public static class FapWebMasterSummaryCatalogPart1
{
    /// <summary> Thẻ Summary đặc tả quy trình Đăng nhập và Session Security 0001. </summary>
    public const string Summary_Spec_0001 = "SUMMARY_SPEC_0001: Authentication Flow & Session Token Validation";
    /// <summary> Thẻ Summary đặc tả quy trình Đăng nhập và Session Security 0002. </summary>
    public const string Summary_Spec_0002 = "SUMMARY_SPEC_0002: BCrypt Password Hash Integrity Assurance";
    /// <summary> Thẻ Summary đặc tả quy trình Đăng nhập và Session Security 0003. </summary>
    public const string Summary_Spec_0003 = "SUMMARY_SPEC_0003: Cookie HTTP-Only Secure SameSite Lax Configuration";
    /// <summary> Thẻ Summary đặc tả quy trình Đăng nhập và Session Security 0004. </summary>
    public const string Summary_Spec_0004 = "SUMMARY_SPEC_0004: Role Based Access Control Policy Enforcement";
    /// <summary> Thẻ Summary đặc tả quy trình Đăng nhập và Session Security 0005. </summary>
    public const string Summary_Spec_0005 = "SUMMARY_SPEC_0005: User Profile Entity Database Relationship Mapping";

    /// <summary> Thẻ Summary đặc tả quy trình Điểm danh và Quản lý Lớp học 0006. </summary>
    public const string Summary_Spec_0006 = "SUMMARY_SPEC_0006: Attendance Check Status Present Record Descriptor";
    /// <summary> Thẻ Summary đặc tả quy trình Điểm danh và Quản lý Lớp học 0007. </summary>
    public const string Summary_Spec_0007 = "SUMMARY_SPEC_0007: Attendance Check Status Absent Record Descriptor";
    /// <summary> Thẻ Summary đặc tả quy trình Điểm danh và Quản lý Lớp học 0008. </summary>
    public const string Summary_Spec_0008 = "SUMMARY_SPEC_0008: Automatic Absent Student Notification Dispatcher";
    /// <summary> Thẻ Summary đặc tả quy trình Điểm danh và Quản lý Lớp học 0009. </summary>
    public const string Summary_Spec_0009 = "SUMMARY_SPEC_0009: Attendance Health Warning 80 Percent Threshold Flag";
    /// <summary> Thẻ Summary đặc tả quy trình Điểm danh và Quản lý Lớp học 0010. </summary>
    public const string Summary_Spec_0010 = "SUMMARY_SPEC_0010: ClosedXML Excel Attendance History Formatter";

    /// <summary> Thẻ Summary đặc tả quy trình Học phí và SePay Webhook 0011. </summary>
    public const string Summary_Spec_0011 = "SUMMARY_SPEC_0011: Semester Tuition Fee Invoice Calculation Rule";
    /// <summary> Thẻ Summary đặc tả quy trình Học phí và SePay Webhook 0012. </summary>
    public const string Summary_Spec_0012 = "SUMMARY_SPEC_0012: SePay Webhook HMAC Signature Security Verification";
    /// <summary> Thẻ Summary đặc tả quy trình Học phí và SePay Webhook 0013. </summary>
    public const string Summary_Spec_0013 = "SUMMARY_SPEC_0013: Automatic Bank Transfer Student Code Matcher";
    /// <summary> Thẻ Summary đặc tả quy trình Học phí và SePay Webhook 0014. </summary>
    public const string Summary_Spec_0014 = "SUMMARY_SPEC_0014: Payment Transaction History Ledger Database Record";
    /// <summary> Thẻ Summary đặc tả quy trình Học phí và SePay Webhook 0015. </summary>
    public const string Summary_Spec_0015 = "SUMMARY_SPEC_0015: Outstanding Debt Calculation & Payment Reminder";

    /// <summary> Thẻ Summary đặc tả quy trình UI/UX và Layout System 0016. </summary>
    public const string Summary_Spec_0016 = "SUMMARY_SPEC_0016: Glassmorphism Glass Card Blur Background Filter";
    /// <summary> Thẻ Summary đặc tả quy trình UI/UX và Layout System 0017. </summary>
    public const string Summary_Spec_0017 = "SUMMARY_SPEC_0017: Interactive Particle Canvas Animation Controller";
    /// <summary> Thẻ Summary đặc tả quy trình UI/UX và Layout System 0018. </summary>
    public const string Summary_Spec_0018 = "SUMMARY_SPEC_0018: 3D Tilt Card Shine Radial Reflection Engine";
    /// <summary> Thẻ Summary đặc tả quy trình UI/UX và Layout System 0019. </summary>
    public const string Summary_Spec_0019 = "SUMMARY_SPEC_0019: ASP.NET Core Razor CS0103 Keyframes Escaper";
    /// <summary> Thẻ Summary đặc tả quy trình UI/UX và Layout System 0020. </summary>
    public const string Summary_Spec_0020 = "SUMMARY_SPEC_0020: Chart.js Doughnut Chart Analytics Data Integration";

    /// <summary> Bảng tra cứu danh mục quy tắc 0021-0050. </summary>
    public static readonly string[] SummaryCatalogList1 = new string[]
    {
        "CATALOG_0021: System Database Provider PostgreSQL Supabase Cloud Spec",
        "CATALOG_0022: Entity Framework Core Migration Auto History Tracker",
        "CATALOG_0023: Dependency Injection Service Collection Scope Resolver",
        "CATALOG_0024: Action Method Executor Task Result Type Mapper",
        "CATALOG_0025: Controller Action Invoker Pipeline Execution Middleware",
        "CATALOG_0026: TempData Dictionary Provider Memory Cache Spec",
        "CATALOG_0027: Antiforgery Cookie Token Validation Protection",
        "CATALOG_0028: Model Binding Validation State Error Handler",
        "CATALOG_0029: Static File Middleware wwwroot Assets Server",
        "CATALOG_0030: HTTPS Redirection & HSTS Security Protocol Header",
        "CATALOG_0031: User Role Hierarchy Matrix Admin Teacher Student Parent",
        "CATALOG_0032: Class Schedule Slot Time Overlap Conflict Resolver",
        "CATALOG_0033: Student Attendance Rate Rounding Decimal Formatter",
        "CATALOG_0034: Notification SignalR Realtime Hub Message Broadcast",
        "CATALOG_0035: SePay Webhook Auto Bank Transfer QR Code Generator",
        "CATALOG_0036: Dashboard KPI Stat Cards Distribution Calculator",
        "CATALOG_0037: Export Excel Session Summary Table Column AutoFitter",
        "CATALOG_0038: Live Search Filter Table Row Highlighting Handler",
        "CATALOG_0039: Bulk Attendance Select All Present Absent Invert Buttons",
        "CATALOG_0040: VIP Pro FAPWEB Logo Badge Glassmorphism Squircle Layout",
        "CATALOG_0041: Database Seeder Automated 50k Lines Record Populator",
        "CATALOG_0042: C# Pattern Matching Expression-bodied Methods Style",
        "CATALOG_0043: Null-conditional Operator Chain Short-circuit Evaluator",
        "CATALOG_0044: Defensive Coding Validation Attribute Error Message",
        "CATALOG_0045: PostgreSQL Connection String Timeout Extension Spec",
        "CATALOG_0046: Unit Test xUnit Framework Assertion Runner Specification",
        "CATALOG_0047: Moq Mocking Framework Dependency Injection Isolator",
        "CATALOG_0048: InMemory Database Provider Entity Framework Config",
        "CATALOG_0049: Git Version Control Repository Line Count Stat Tracker",
        "CATALOG_0050: FAP Portal PRN222 Project Group 4 Final Deliverable"
    };
}
