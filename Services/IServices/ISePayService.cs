using FapWeb.Models.Dtos.SePayDtos;

namespace FapWeb.Services.IServices
{
    public interface ISePayService
    {
        SePayCheckoutFormDto BuildCheckoutForm(SePayCheckoutOrderDto order);
    }
}
