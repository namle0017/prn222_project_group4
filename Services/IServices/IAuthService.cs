using FapWeb.Models.Dtos.ChangePasswordDtos;
using FapWeb.Models.Dtos.LoginDtos;

namespace FapWeb.Services.IServices
{
    /// <summary>
    /// Giao diện dịch vụ nghiệp vụ Xác thực và Quản lý tài khoản người dùng.
    /// </summary>
    /// <remarks>
    /// Cung cấp các chức năng đăng nhập hệ thống dựa trên số điện thoại và mật khẩu,
    /// xác thực vai trò phân quyền và xử lý đổi mật khẩu tài khoản cá nhân.
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        /// Xử lý đăng nhập hệ thống dựa trên thông tin số điện thoại và mật khẩu.
        /// </summary>
        /// <param name="request">Đối tượng LoginRequestDto chứa thông tin số điện thoại và mật khẩu đăng nhập.</param>
        /// <returns>Trả về LoginResponseDto nếu thông tin hợp lệ, ngược lại trả về null.</returns>
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Xử lý thay đổi mật khẩu người dùng sau khi xác thực mật khẩu hiện tại.
        /// </summary>
        /// <param name="request">Đối tượng ChangePasswordRequestDto chứa thông tin mật khẩu cũ và mật khẩu mới.</param>
        /// <returns>Trả về true nếu cập nhật mật khẩu thành công, ngược lại trả về false.</returns>
        Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request);
    }
}
