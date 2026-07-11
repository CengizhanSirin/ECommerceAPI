using ECommerce.Application.DTOs.Payment.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Payment
{
    public class ProcessPaymentRequestValidator : AbstractValidator<ProcessPaymentRequest>
    {
        public ProcessPaymentRequestValidator()
        {
            RuleFor(x => x.CardHolderName).NotEmpty()
                .MaximumLength(100);


            RuleFor(x => x.CardNumber).NotEmpty()
                .Must(x =>
                {
                    var normalized = x.Replace(" ", string.Empty);
                    return normalized.All(char.IsDigit) && normalized.Length == 16;
                })
                .WithMessage("Kart numarası 16 haneli olmalıdır.");


            RuleFor(x => x.ExpirationMonth).NotEmpty()
                .Must(x => int.TryParse(x, out var month) && month is >= 1 and <= 12)
                .WithMessage("Geçerli bir son kullanma ayı giriniz.");


            RuleFor(x => x.ExpirationYear).NotEmpty()
                .Must(x => int.TryParse(x, out var year) && year >= DateTime.UtcNow.Year)
                .WithMessage("Geçerli bir son kullanma yılı giriniz.");


            RuleFor(x => x.Cvv).NotEmpty()
                .Matches(@"^\d{3,4}$")
                .WithMessage("CVV 3 veya 4 haneli olmalıdır.");
        }
    }
}
