using ECommerce.Application.DTOs.ProductImage.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Product
{
    public class CreateProductImageRequestValidator : AbstractValidator<CreateProductImageRequest>
    {
        public CreateProductImageRequestValidator()
        {
            RuleFor(x => x.ImageUrl)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Görsel URL boş olamaz.")
                .MaximumLength(500).WithMessage("Görsel URL en fazla 500 karakter olabilir.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir URL giriniz.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sıralama değeri 0'dan küçük olamaz.");
        }
    }
}
