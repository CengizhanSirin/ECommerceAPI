using ECommerce.Application.DTOs.User.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.User
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
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

            RuleFor(x => x.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(10).WithMessage("Telefon numarası en az 10 karakter olmalıdır.")
                .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.")
                .Matches(@"^[0-9\+\-\s]+$").WithMessage("Geçerli bir telefon numarası giriniz.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}
