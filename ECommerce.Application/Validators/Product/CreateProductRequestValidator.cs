using ECommerce.Application.DTOs.Product.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Product
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.")
                .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

            RuleFor(x => x.SKU)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("SKU boş olamaz.")
                .MinimumLength(2).WithMessage("SKU en az 2 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("SKU en fazla 50 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stok 0'dan küçük olamaz.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Geçerli bir kategori seçiniz.");

            RuleFor(x => x.BrandId)
                .GreaterThan(0).WithMessage("Geçerli bir marka seçiniz.");
        }
    }
}
