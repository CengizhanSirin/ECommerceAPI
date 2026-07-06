using ECommerce.Application.DTOs.Category.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Brand
{
    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Kategori adı boş olamaz.")
                .MinimumLength(2).WithMessage("Kategori adı en az 2 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Görsel URL en fazla 500 karakter olabilir.")
                .Must(url => string.IsNullOrWhiteSpace(url) ||
                             Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir URL giriniz.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sıralama değeri 0'dan küçük olamaz.");
        }
    }
}
