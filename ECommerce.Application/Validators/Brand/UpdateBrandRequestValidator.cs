using ECommerce.Application.DTOs.Brand.Requests;
using ECommerce.Application.DTOs.Category.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Brand
{
    public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
    {
        public UpdateBrandRequestValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Marka adı boş olamaz.")
                .MinimumLength(2).WithMessage("Marka adı en az 2 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Marka adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500).WithMessage("Görsel URL en fazla 500 karakter olabilir.")
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))        
                .WithMessage("Geçerli bir URL giriniz.");
        }
    }
}
