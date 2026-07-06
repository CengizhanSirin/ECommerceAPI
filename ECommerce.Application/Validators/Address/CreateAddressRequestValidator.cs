using ECommerce.Application.DTOs.Address.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Address
{
    public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
    {
        public CreateAddressRequestValidator()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Adres başlığı boş olamaz.")
                .MinimumLength(2).WithMessage("Adres başlığı en az 2 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Adres başlığı en fazla 50 karakter olabilir.");

            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Ad boş olamaz.")
                .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Soyad boş olamaz.")
                .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Telefon numarası boş olamaz.")
                .MinimumLength(10).WithMessage("Telefon numarası en az 10 karakter olmalıdır.")
                .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.")
                .Matches(@"^[0-9\+\-\s]+$").WithMessage("Geçerli bir telefon numarası giriniz.");

            RuleFor(x => x.City)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Şehir boş olamaz.")
                .MinimumLength(3).WithMessage("Şehir en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Şehir en fazla 50 karakter olabilir.");

            RuleFor(x => x.District)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("İlçe boş olamaz.")
                .MinimumLength(3).WithMessage("İlçe en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("İlçe en fazla 50 karakter olabilir.");

            RuleFor(x => x.Neighborhood)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Mahalle boş olamaz.")
                .MinimumLength(3).WithMessage("Mahalle en az 3 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Mahalle en fazla 100 karakter olabilir.");

            RuleFor(x => x.Street)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Sokak/Cadde boş olamaz.")
                .MinimumLength(3).WithMessage("Sokak/Cadde en az 3 karakter olmalıdır.")
                .MaximumLength(200).WithMessage("Sokak/Cadde en fazla 200 karakter olabilir.");

            RuleFor(x => x.PostalCode)
                .MaximumLength(10).WithMessage("Posta kodu en fazla 10 karakter olabilir.")
                .Matches(@"^\d+$").WithMessage("Posta kodu yalnızca rakamlardan oluşmalıdır.")
                .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));
        }
    }
}
