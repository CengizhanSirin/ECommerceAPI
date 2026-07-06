using ECommerce.Application.DTOs.Auth.Requests;
using FluentValidation;

namespace ECommerce.Application.Validators.Auth
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token boş olamaz."); 
        }
    }
}
