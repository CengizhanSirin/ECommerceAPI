namespace ECommerce.Application.Common.Constants;

public static class AuthMessages
{
    // Errors
    public const string TokenExpired = "Oturum süreniz dolmuştur.";
    public const string InvalidToken = "Geçersiz token.";
    public const string InvalidRefreshToken = "Refresh token geçersiz veya süresi dolmuş.";
    public const string TokenRevoked = "Token iptal edilmiştir.";
    public const string RefreshTokenNotFound = "Refresh token bulunamadı.";
    public const string EmailAlreadyExists = "Bu e-posta adresi zaten kullanılmaktadır.";
    public const string UserNotFound = "Kullanıcı bulunamadı.";
    public const string RegisterFailed = "Kullanıcı kaydı oluşturulamadı.";
    public const string RoleAssignmentFailed = "Kullanıcı rolü atanamadı.";
    public const string InvalidEmailOrPassword = "E-posta adresi veya şifre hatalı.";



    // Success
    public const string LoginSuccess = "Giriş başarılı.";
    public const string RegisterSuccess = "Kullanıcı başarıyla kaydedildi";
    public const string LogoutSuccess = "Çıkış işlemi başarılı.";
    public const string PasswordResetSuccessful = "Şifre sıfırlama başarılı.";
    public const string EmailVerificationSuccessful = "E-posta doğrulama başarılı.";
    public const string RefreshTokenSuccess = "Token başarıyla yenilendi.";

}
