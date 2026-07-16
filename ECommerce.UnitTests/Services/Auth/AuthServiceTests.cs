using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Auth.Requests;
using ECommerce.Application.DTOs.Auth.Responses;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ECommerce.UnitTests.Services.Auth
{
    public sealed class AuthServiceTests
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userManagerMock = CreateUserManagerMock();
            _jwtServiceMock = new Mock<IJwtService>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _authService = new AuthService(_userManagerMock.Object, _jwtServiceMock.Object, _refreshTokenRepositoryMock.Object, _unitOfWorkMock.Object, _mapperMock.Object);
        }


        // =========================================================
        // LOGOUT
        // =========================================================


        [Fact]
        public async Task LogoutAsync_Should_ReturnNotFound_When_RefreshTokenDoesNotExist()
        {
            // Arrange
            const string refreshToken = "missing-refresh-token";

            _refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshToken, It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

            // Act
            var result = await _authService.LogoutAsync(refreshToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.RefreshTokenNotFound, result.Message);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task LogoutAsync_Should_ReturnSuccessWithoutSaving_When_TokenIsAlreadyRevoked()
        {
            // Arrange
            var revokedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

            var refreshToken = CreateRefreshToken(token: "already-revoked-token", revokedAt: revokedAt);

            _refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshToken.Token, It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);

            // Act
            var result = await _authService.LogoutAsync(refreshToken.Token);


            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuthMessages.LogoutSuccess, result.Message);

            Assert.Equal(revokedAt, refreshToken.RevokedAt);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task LogoutAsync_Should_RevokeTokenAndSave_When_TokenIsActive()
        {
            // Arrange
            var refreshToken = CreateRefreshToken(token: "active-refresh-token");

            _refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshToken.Token, It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);

            var beforeExecution = DateTimeOffset.UtcNow;

            // Act
            var result = await _authService.LogoutAsync(refreshToken.Token);


            var afterExecution = DateTimeOffset.UtcNow;

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuthMessages.LogoutSuccess, result.Message);

            Assert.NotNull(refreshToken.RevokedAt);

            Assert.InRange(refreshToken.RevokedAt.Value, beforeExecution, afterExecution);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // REFRESH TOKEN
        // =========================================================

        [Fact]
        public async Task RefreshTokenAsync_Should_ReturnUnauthorized_When_TokenDoesNotExist()
        {
            // Arrange
            var request = new RefreshTokenRequest(RefreshToken: "missing-token");


            _refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

            // Act
            var result = await _authService.RefreshTokenAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.InvalidRefreshToken, result.Message);

            Assert.Null(result.Data);

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RefreshTokenAsync_Should_ReturnUnauthorized_When_TokenIsRevoked()
        {
            // Arrange
            var refreshToken = CreateRefreshToken(
                token: "revoked-token",
                expiresAt: DateTimeOffset.UtcNow.AddDays(1),
                revokedAt: DateTimeOffset.UtcNow.AddMinutes(-5));

            var request = new RefreshTokenRequest(refreshToken.Token);


            _refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);


            // Act
            var result = await _authService.RefreshTokenAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.InvalidRefreshToken, result.Message);

            Assert.Null(result.Data);

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RefreshTokenAsync_Should_ReturnUnauthorized_When_TokenIsExpired()
        {
            // Arrange
            var refreshToken = CreateRefreshToken(token: "expired-token", expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

            var request = new RefreshTokenRequest(refreshToken.Token);

            _refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);


            // Act
            var result = await _authService.RefreshTokenAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.InvalidRefreshToken, result.Message);

            Assert.Null(result.Data);

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RefreshTokenAsync_Should_RevokeOldTokenAndCreateNewToken_When_TokenIsValid()
        {
            // Arrange
            var user = CreateUser();

            var existingRefreshToken = CreateRefreshToken(token: "old-refresh-token", user: user, expiresAt: DateTimeOffset.UtcNow.AddDays(1));

            var request = new RefreshTokenRequest(existingRefreshToken.Token);

            var newToken = CreateTokenResponse(refreshToken: "new-refresh-token");


            RefreshToken? capturedRefreshToken = null;

            _refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(existingRefreshToken);

            _jwtServiceMock.Setup(service => service.CreateTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(newToken);

            _refreshTokenRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                .Callback<RefreshToken, CancellationToken>((token, _) => capturedRefreshToken = token)
                .Returns(Task.CompletedTask);

            var beforeExecution = DateTimeOffset.UtcNow;

            // Act
            var result = await _authService.RefreshTokenAsync(request);

            var afterExecution = DateTimeOffset.UtcNow;

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuthMessages.RefreshTokenSuccess, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(newToken, result.Data);

            Assert.NotNull(existingRefreshToken.RevokedAt);

            Assert.InRange(existingRefreshToken.RevokedAt.Value, beforeExecution, afterExecution);

            Assert.NotNull(capturedRefreshToken);

            Assert.Equal(newToken.RefreshToken, capturedRefreshToken.Token);

            Assert.Equal(newToken.RefreshTokenExpiration, capturedRefreshToken.ExpiresAt);

            Assert.Equal(user.Id, capturedRefreshToken.AppUserId);
            Assert.Null(capturedRefreshToken.RevokedAt);

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once());

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // LOGIN
        // =========================================================

        [Fact]
        public async Task LoginAsync_Should_ReturnUnauthorized_When_UserDoesNotExist()
        {
            // Arrange
            var request = CreateLoginRequest();

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.InvalidEmailOrPassword, result.Message);

            Assert.Null(result.Data);

            _userManagerMock.Verify(manager => manager.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never());

            _refreshTokenRepositoryMock.Verify(repository => repository.GetActiveByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task LoginAsync_Should_ReturnUnauthorized_When_PasswordIsInvalid()
        {
            // Arrange
            var user = CreateUser();
            var request = CreateLoginRequest();

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync(user);

            _userManagerMock.Setup(manager => manager.CheckPasswordAsync(user, request.Password)).ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.InvalidEmailOrPassword, result.Message);

            Assert.Null(result.Data);

            _refreshTokenRepositoryMock.Verify(repository => repository.GetActiveByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task LoginAsync_Should_CreateTokens_When_UserCredentialsAreValid()
        {
            // Arrange
            var user = CreateUser();
            var request = CreateLoginRequest();

            var token = CreateTokenResponse();
            var authResponse = CreateAuthResponse(user);

            RefreshToken? capturedRefreshToken = null;

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync(user);

            _userManagerMock.Setup(manager => manager.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);

            _refreshTokenRepositoryMock.Setup(repository => repository.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

            _jwtServiceMock.Setup(service => service.CreateTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(token);

            _refreshTokenRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                .Callback<RefreshToken, CancellationToken>((refreshToken, _) => capturedRefreshToken = refreshToken)
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(mapper => mapper.Map<AuthResponse>(user)).Returns(authResponse);


            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuthMessages.LoginSuccess, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(authResponse, result.Data);
            Assert.Same(token, result.Data.Token);

            Assert.NotNull(capturedRefreshToken);

            Assert.Equal(token.RefreshToken, capturedRefreshToken.Token);

            Assert.Equal(token.RefreshTokenExpiration, capturedRefreshToken.ExpiresAt);

            Assert.Equal(user.Id, capturedRefreshToken.AppUserId);

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<AuthResponse>(user), Times.Once());
        }

        [Fact]
        public async Task LoginAsync_Should_RevokeActiveTokenAndCreateNewToken_When_ActiveTokenExists()
        {
            // Arrange
            var user = CreateUser();
            var request = CreateLoginRequest();

            var activeRefreshToken = CreateRefreshToken(token: "previous-refresh-token", user: user);

            var newToken = CreateTokenResponse(refreshToken: "new-login-refresh-token");


            var authResponse = CreateAuthResponse(user);

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync(user);

            _userManagerMock.Setup(manager => manager.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);

            _refreshTokenRepositoryMock.Setup(repository => repository.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activeRefreshToken);

            _jwtServiceMock.Setup(service => service.CreateTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(newToken);

            _refreshTokenRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _mapperMock.Setup(mapper => mapper.Map<AuthResponse>(user)).Returns(authResponse);

            var beforeExecution = DateTimeOffset.UtcNow;

            // Act
            var result = await _authService.LoginAsync(request);

            var afterExecution = DateTimeOffset.UtcNow;

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuthMessages.LoginSuccess, result.Message);

            Assert.NotNull(activeRefreshToken.RevokedAt);

            Assert.InRange(activeRefreshToken.RevokedAt.Value, beforeExecution, afterExecution);

            Assert.NotNull(result.Data);
            Assert.Same(newToken, result.Data.Token);

            _refreshTokenRepositoryMock.Verify(
                repository => repository.AddAsync(It.Is<RefreshToken>(token => token.Token == newToken.RefreshToken && token.ExpiresAt == newToken.RefreshTokenExpiration && token.AppUserId == user.Id),
                It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        // =========================================================
        // REGISTER
        // =========================================================

        [Fact]
        public async Task RegisterAsync_Should_ReturnConflict_When_EmailAlreadyExists()
        {
            // Arrange
            var request = CreateRegisterRequest();
            var existingUser = CreateUser();

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.EmailAlreadyExists, result.Message);

            Assert.Null(result.Data);

            _mapperMock.Verify(mapper => mapper.Map<AppUser>(It.IsAny<RegisterRequest>()), Times.Never());

            _userManagerMock.Verify(manager => manager.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never());

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RegisterAsync_Should_ReturnFailure_When_UserCreationFails()
        {
            // Arrange
            var request = CreateRegisterRequest();
            var user = CreateUser();

            var createResult = IdentityResult.Failed(
                new IdentityError
                {
                    Code = "PasswordTooShort",
                    Description = "Şifre çok kısa."
                },
                new IdentityError
                {
                    Code = "PasswordTooShortDuplicate",
                    Description = "Şifre çok kısa."
                },
                new IdentityError
                {
                    Code = "PasswordRequiresDigit",
                    Description = "Şifre rakam içermelidir."
                });

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);

            _mapperMock.Setup(mapper => mapper.Map<AppUser>(request)).Returns(user);

            _userManagerMock.Setup(manager => manager.CreateAsync(user, request.Password)).ReturnsAsync(createResult);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.RegisterFailed, result.Message);
            Assert.Null(result.Data);

            Assert.NotNull(result.Errors);

            Assert.Contains("Şifre çok kısa.", result.Errors);

            Assert.Contains("Şifre rakam içermelidir.", result.Errors);

            Assert.Equal(2, result.Errors.Distinct().Count());


            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _userManagerMock.Verify(manager => manager.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never());

            _userManagerMock.Verify(manager => manager.DeleteAsync(It.IsAny<AppUser>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RegisterAsync_Should_DeleteUserAndReturnFailure_When_RoleAssignmentFails()
        {
            // Arrange
            var request = CreateRegisterRequest();
            var user = CreateUser();

            var roleResult = IdentityResult.Failed(
                new IdentityError
                {
                    Code = "RoleNotFound",
                    Description = "Rol bulunamadı."
                },
                new IdentityError
                {
                    Code = "RoleNotFoundDuplicate",
                    Description = "Rol bulunamadı."
                },
                new IdentityError
                {
                    Code = "RoleAssignmentError",
                    Description = "Rol atanırken hata oluştu."
                });

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);


            _mapperMock.Setup(mapper => mapper.Map<AppUser>(request)).Returns(user);

            _userManagerMock.Setup(manager => manager.CreateAsync(user, request.Password)).ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(manager => manager.AddToRoleAsync(user, Roles.Customer)).ReturnsAsync(roleResult);

            _userManagerMock.Setup(manager => manager.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AuthMessages.RoleAssignmentFailed, result.Message);

            Assert.Null(result.Data);

            Assert.NotNull(result.Errors);

            Assert.Contains("Rol bulunamadı.", result.Errors);

            Assert.Contains("Rol atanırken hata oluştu.", result.Errors);

            Assert.Equal(2, result.Errors.Distinct().Count());


            _userManagerMock.Verify(manager => manager.DeleteAsync(user), Times.Once());

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never());

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task RegisterAsync_Should_CreateUserRoleAndTokens_When_RequestIsValid()
        {
            // Arrange
            var request = CreateRegisterRequest();
            var user = CreateUser();

            var token = CreateTokenResponse();
            var authResponse = CreateAuthResponse(user);

            RefreshToken? capturedRefreshToken = null;

            _userManagerMock.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync((AppUser?)null);

            _mapperMock.Setup(mapper => mapper.Map<AppUser>(request)).Returns(user);

            _userManagerMock.Setup(manager => manager.CreateAsync(user, request.Password)).ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(manager => manager.AddToRoleAsync(user, Roles.Customer)).ReturnsAsync(IdentityResult.Success);

            _jwtServiceMock.Setup(service => service.CreateTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(token);

            _refreshTokenRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                .Callback<RefreshToken, CancellationToken>((refreshToken, _) => capturedRefreshToken = refreshToken).Returns(Task.CompletedTask);

            _mapperMock.Setup(mapper => mapper.Map<AuthResponse>(user)).Returns(authResponse);


            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuthMessages.RegisterSuccess, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(authResponse, result.Data);
            Assert.Same(token, result.Data.Token);

            Assert.NotNull(capturedRefreshToken);

            Assert.Equal(token.RefreshToken, capturedRefreshToken.Token);
            Assert.Equal(token.RefreshTokenExpiration, capturedRefreshToken.ExpiresAt);
            Assert.Equal(user.Id, capturedRefreshToken.AppUserId);

            _userManagerMock.Verify(manager => manager.CreateAsync(user, request.Password), Times.Once());

            _userManagerMock.Verify(manager => manager.AddToRoleAsync(user, Roles.Customer), Times.Once());

            _userManagerMock.Verify(manager => manager.DeleteAsync(It.IsAny<AppUser>()), Times.Never());

            _jwtServiceMock.Verify(service => service.CreateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once());

            _refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _mapperMock.Verify(mapper => mapper.Map<AuthResponse>(user), Times.Once());
        }


        // =========================================================
        // TEST DATA HELPERS
        // =========================================================


        private static Mock<UserManager<AppUser>> CreateUserManagerMock()
        {
            var userStoreMock = new Mock<IUserStore<AppUser>>();

            return new Mock<UserManager<AppUser>>(
                userStoreMock.Object,
                Options.Create(new IdentityOptions()),
                Mock.Of<IPasswordHasher<AppUser>>(),
                Array.Empty<IUserValidator<AppUser>>(),
                Array.Empty<IPasswordValidator<AppUser>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<AppUser>>>());
        }

        private static AppUser CreateUser(int id = 42, string email = "cengizhan@example.com")
        {
            return new AppUser
            {
                Id = id,
                FirstName = "Cengizhan",
                LastName = "Şirin",
                Email = email,
                UserName = email
            };
        }

        private static RegisterRequest CreateRegisterRequest()
        {
            return new RegisterRequest(
                FirstName: "Cengizhan",
                LastName: "Şirin",
                Email: "cengizhan@example.com",
                Password: "Test123!",
                ConfirmPassword: "Test123!");
        }

        private static LoginRequest CreateLoginRequest()
        {
            return new LoginRequest(Email: "cengizhan@example.com", Password: "Test123!");
        }

        private static TokenResponse CreateTokenResponse(string accessToken = "access-token", string refreshToken = "refresh-token")
        {
            var now = DateTimeOffset.UtcNow;

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = now.AddMinutes(30),
                RefreshTokenExpiration = now.AddDays(7)
            };
        }

        private static AuthResponse CreateAuthResponse(AppUser user)
        {
            return new AuthResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!
            };
        }

        private static RefreshToken CreateRefreshToken(string token = "refresh-token", AppUser? user = null, DateTimeOffset? expiresAt = null, DateTimeOffset? revokedAt = null)
        {
            user ??= CreateUser();

            return new RefreshToken
            {
                Token = token,
                ExpiresAt = expiresAt ?? DateTimeOffset.UtcNow.AddDays(7),
                RevokedAt = revokedAt,
                AppUserId = user.Id,
                AppUser = user
            };
        }
    }
}

