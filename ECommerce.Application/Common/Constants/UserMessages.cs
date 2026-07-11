namespace ECommerce.Application.Common.Constants;

public static class UserMessages
{
    // Errors
    public const string UserNotFound = "Kullanıcı bulunamadı.";
    public const string ProfileUpdateFailed = "Profil bilgileri güncellenemedi.";
    public const string InvalidCredentials = "Geçersiz kullanıcı adı veya şifre.";
    public const string UserAlreadyExists = "Kullanıcı zaten mevcut.";
    public const string EmailAlreadyExists = "E-posta zaten mevcut.";
    public const string InvalidEmailFormat = "Geçersiz e-posta formatı.";
    public const string PasswordTooWeak = "Şifre çok zayıf.";
    public const string UnauthorizedAccess = "Yetkisiz erişim.";
    public const string AccountLocked = "Hesap kilitlendi.";

    // Success
    public const string UserCreatedSuccessfully = "Kullanıcı başarıyla oluşturuldu.";
    public const string ProfileUpdated = "Kullanıcı başarıyla güncellendi.";
    public const string ProfileRetrieved = "Profil bilgileri başarıyla getirildi.";
    public const string UserDeletedSuccessfully = "Kullanıcı başarıyla silindi.";
    public const string PasswordChangedSuccessfully = "Şifre başarıyla değiştirildi.";
}
