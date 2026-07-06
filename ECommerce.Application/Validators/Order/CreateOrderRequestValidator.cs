using ECommerce.Application.DTOs.Order.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Order
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.AddressId) .GreaterThan(0).WithMessage("Geçerli bir adres seçiniz.");             
        }
    }
}
