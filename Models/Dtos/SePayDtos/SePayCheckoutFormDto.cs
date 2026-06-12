namespace FapWeb.Models.Dtos.SePayDtos
{
    public class SePayCheckoutFormDto
    {
        public string ActionUrl { get; set; } = string.Empty;

        /// <summary>
        /// Các field của form theo đúng thứ tự đã ký. Không được đổi thứ tự
        /// vì SePay xác thực chữ ký dựa trên thứ tự field.
        /// </summary>
        public List<KeyValuePair<string, string>> Fields { get; set; } = new();
    }
}
