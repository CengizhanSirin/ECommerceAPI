using ECommerce.Application.DTOs.Cart.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart
{
    public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
    {
        public UpdateCartItemRequestValidator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");    
        }
    }
}
