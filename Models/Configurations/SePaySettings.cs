namespace FapWeb.Models.Configurations
{
    public class SePaySettings
    {
        public const string SectionName = "SePay";

        public string UrlSePay { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public string MerchantId { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;
    }
}
