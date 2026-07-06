using ECommerce.Application.DTOs.Order.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Order
{
    public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusRequestValidator()
        {
            RuleFor(x => x.OrderStatus).IsInEnum().WithMessage("Geçerli bir sipariş durumu seçiniz.");
        }
    }
}
