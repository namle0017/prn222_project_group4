using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FapWeb.Models.Configurations;
using FapWeb.Models.Dtos.SePayDtos;
using FapWeb.Services.IServices;
using Microsoft.Extensions.Options;

namespace FapWeb.Services.Service
{
    public class SePayService : ISePayService
    {
        private readonly SePaySettings _settings;

        public SePayService(IOptions<SePaySettings> settings)
        {
            _settings = settings.Value;
        }

        public SePayCheckoutFormDto BuildCheckoutForm(SePayCheckoutOrderDto order)
        {
            // Thứ tự field phải khớp tuyệt đối với danh sách allowedFields
            // của SePay, nếu không signature sẽ không hợp lệ.
            var fields = new List<KeyValuePair<string, string>>();

            void AddField(string name, string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                fields.Add(new KeyValuePair<string, string>(name, value));
            }

            AddField("order_amount", order.Amount.ToString("0", CultureInfo.InvariantCulture));
            AddField("merchant", _settings.MerchantId);
            AddField("currency", "VND");
            AddField("operation", "PURCHASE");
            AddField("order_description", order.Description);
            AddField("order_invoice_number", order.InvoiceNumber);
            AddField("customer_id", order.CustomerId);
            AddField("payment_method", order.PaymentMethod);
            AddField("success_url", order.SuccessUrl);
            AddField("error_url", order.ErrorUrl);
            AddField("cancel_url", order.CancelUrl);

            var signature = SignFields(fields, _settings.SecretKey);
            fields.Add(new KeyValuePair<string, string>("signature", signature));

            return new SePayCheckoutFormDto
            {
                ActionUrl = _settings.UrlSePay,
                Fields = fields
            };
        }

        private static string SignFields(IEnumerable<KeyValuePair<string, string>> fields, string secretKey)
        {
            var signedString = string.Join(",", fields.Select(f => $"{f.Key}={f.Value}"));

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedString));
            return Convert.ToBase64String(hash);
        }
    }
}
