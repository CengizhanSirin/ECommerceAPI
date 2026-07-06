using ECommerce.Application.DTOs.Cart.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart
{
    public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
    {
        public AddToCartRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Geçerli bir ürün seçiniz.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
        }
    }
}
