using FapWeb.Models.Dtos.SePayDtos;

namespace FapWeb.Services.IServices
{
    public interface ISePayService
    {
        SePayCheckoutFormDto BuildCheckoutForm(SePayCheckoutOrderDto order);

        /// <summary>
        /// Ky ma hoa don de gan vao URL callback, dung nhan biet callback
        /// that su xuat phat tu phien thanh toan do he thong tao ra.
        /// </summary>
        string SignInvoice(string invoiceNumber);

        bool VerifyInvoiceSignature(string invoiceNumber, string? signature);
    }
}
