namespace FapWeb.Models.Dtos.LoginDtos
{
    /// <summary>
    /// Đối tượng DTO chứa kết quả xác thực đăng nhập thành công của người dùng.
    /// </summary>
    /// <remarks>
    /// Chứa thông tin định danh UserId, họ tên UserName và quyền hạn UserRole để thiết lập Session và Cookie.
    /// </remarks>
    public class LoginResponseDto
    {
        /// <summary>
        /// Mã định danh duy nhất (Guid) của người dùng trong cơ sở dữ liệu.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Họ và tên đầy đủ của người dùng.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Tên vai trò phân quyền của người dùng (ADMIN, TEACHER, STUDENT, PARENT).
        /// </summary>
        public string UserRole { get; set; } = string.Empty;
    }
}
