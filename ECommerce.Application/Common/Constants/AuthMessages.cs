namespace ECommerce.Application.Common.Constants;

public static class AuthMessages
{
    // Errors
    public const string InvalidCredentials = "E-posta veya şifre hatalı.";
    public const string TokenExpired = "Oturum süreniz dolmuştur.";
    public const string InvalidToken = "Geçersiz token.";
    public const string TokenRevoked = "Token iptal edilmiştir.";
    public const string EmailAlreadyExists = "Bu e-posta adresi zaten kullanılmaktadır.";
    public const string UserNotFound = "Kullanıcı bulunamadı.";

    // Success
    public const string LoginSuccessful = "Giriş başarılı.";
    public const string RegistrationSuccessful = "Kayıt başarılı.";
    public const string LogoutSuccessful = "Çıkış başarılı.";
    public const string PasswordResetSuccessful = "Şifre sıfırlama başarılı.";
    public const string EmailVerificationSuccessful = "E-posta doğrulama başarılı.";
}
