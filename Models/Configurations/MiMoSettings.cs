namespace FapWeb.Models.Configurations
{
    public class MiMoSettings
    {
        public const string SectionName = "MiMo";

        public string ApiBaseUrl { get; set; } = "https://api.xiaomimimo.com/v1/chat/completions";
        public string Model { get; set; } = "mimo-v2.5";
        public string? ApiKey { get; set; }
        public int RequestTimeoutSeconds { get; set; } = 20;
        public int MaxQuestionLength { get; set; } = 500;
        public int MaxCompletionTokens { get; set; } = 500;
        public int MaxRequestsPerWindow { get; set; } = 10;
        public int RateLimitWindowMinutes { get; set; } = 5;
    }
}
