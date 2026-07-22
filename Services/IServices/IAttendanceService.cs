using FapWeb.Models.Dtos.AttendanceDtos;

namespace FapWeb.Services.IServices
{
    /// <summary>
    /// Giao diện dịch vụ nghiệp vụ Quản lý Điểm danh và Theo dõi Chuyên cần.
    /// </summary>
    /// <remarks>
    /// Cung cấp các phương thức truy vấn danh sách lớp học, khởi tạo ca học điểm danh,
    /// lưu nhật ký điểm danh hàng ngày và truy xuất lịch sử chuyên cần của học sinh.
    /// </remarks>
    public interface IAttendanceService
    {
        /// <summary>
        /// Lấy danh sách các lớp học khả dụng để điểm danh theo quyền hạn người dùng.
        /// </summary>
        /// <param name="currentUserId">Mã định danh duy nhất của người dùng hiện tại (Guid).</param>
        /// <param name="roleName">Tên vai trò của người dùng (ADMIN, TEACHER, STUDENT, PARENT).</param>
        /// <returns>Trả về danh sách danh mục lớp học dạng AttendanceClassDto.</returns>
        Task<List<AttendanceClassDto>> GetAttendanceClassesAsync(Guid currentUserId, string? roleName);

        /// <summary>
        /// Lấy thông tin ViewModel khởi tạo cho trang thực hiện điểm danh ca học.
        /// </summary>
        /// <param name="classId">Mã lớp học cần điểm danh.</param>
        /// <param name="scheduleId">Mã ca học cụ thể (tùy chọn).</param>
        /// <param name="attendanceDate">Ngày điểm danh chỉ định (tùy chọn).</param>
        /// <param name="currentUserId">Mã người dùng đang thực hiện.</param>
        /// <param name="roleName">Tên vai trò của người dùng thực hiện.</param>
        /// <returns>Trả về AttendanceTakeViewModel chứa danh sách học sinh và ca học.</returns>
        Task<AttendanceTakeViewModel?> GetAttendanceTakeViewAsync(Guid classId, Guid? scheduleId, DateTime? attendanceDate, Guid currentUserId, string? roleName);

        /// <summary>
        /// Lưu kết quả điểm danh ca học của giáo viên xuống cơ sở dữ liệu.
        /// </summary>
        /// <param name="request">Đối tượng AttendanceSaveRequestDto chứa dữ liệu điểm danh.</param>
        /// <param name="teacherId">Mã định danh của giáo viên hoặc người thực hiện điểm danh.</param>
        /// <param name="roleName">Tên vai trò của người thực hiện.</param>
        /// <returns>Trả về true nếu lưu thành công, ngược lại trả về false.</returns>
        Task<bool> SaveAttendanceAsync(AttendanceSaveRequestDto request, Guid teacherId, string? roleName);

        /// <summary>
        /// Lấy nhật ký lịch sử điểm danh theo điều kiện người dùng hoặc học sinh cụ thể.
        /// </summary>
        /// <param name="currentUserId">Mã định danh người dùng yêu cầu tra cứu.</param>
        /// <param name="roleName">Tên vai trò người dùng yêu cầu.</param>
        /// <param name="studentId">Mã học sinh cụ thể cần lọc (tùy chọn).</param>
        /// <returns>Danh sách AttendanceHistoryDto biểu diễn nhật ký chuyên cần.</returns>
        Task<List<AttendanceHistoryDto>> GetAttendanceHistoryAsync(Guid currentUserId, string? roleName, Guid? studentId = null);
    }
}
