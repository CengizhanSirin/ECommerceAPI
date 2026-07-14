using AutoMapper;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.User.Requests;
using ECommerce.Application.DTOs.User.Responses;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ECommerce.UnitTests.Services.Users;

public sealed class UserServiceTests
{
    private const int CurrentUserId = 1923;

    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();

        _currentUserServiceMock.SetupGet(service => service.UserId).Returns(CurrentUserId);
        _userService = new UserService(_userManagerMock.Object, _currentUserServiceMock.Object, _mapperMock.Object);
    }

    // =========================================================
    // GET PROFILE
    // =========================================================

    [Fact]
    public async Task GetProfileAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        // Arrange
        _userManagerMock.Setup(manager => manager.FindByIdAsync(CurrentUserId.ToString())).ReturnsAsync((AppUser?)null);

        // Act
        var result = await _userService.GetProfileAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserMessages.UserNotFound, result.Message);
        Assert.Null(result.Data);

        _mapperMock.Verify(mapper => mapper.Map<UserDetailResponse>(It.IsAny<AppUser>()), Times.Never());

        _userManagerMock.Verify(manager => manager.FindByIdAsync(CurrentUserId.ToString()), Times.Once());
    }

    [Fact]
    public async Task GetProfileAsync_Should_ReturnProfile_When_UserExists()
    {
        // Arrange
        var user = CreateUser();

        var response = CreateUserDetailResponse(user);

        _userManagerMock.Setup(manager => manager.FindByIdAsync(CurrentUserId.ToString())).ReturnsAsync(user);

        _mapperMock.Setup(mapper => mapper.Map<UserDetailResponse>(user)).Returns(response);

        // Act
        var result = await _userService.GetProfileAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(UserMessages.ProfileRetrieved, result.Message);

        Assert.NotNull(result.Data);
        Assert.Same(response, result.Data);

        _mapperMock.Verify(mapper => mapper.Map<UserDetailResponse>(user), Times.Once());

        _userManagerMock.Verify(manager => manager.FindByIdAsync(CurrentUserId.ToString()), Times.Once());
    }

    // =========================================================
    // UPDATE PROFILE
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var request = CreateValidUpdateRequest();

        _userManagerMock.Setup(manager => manager.FindByIdAsync(CurrentUserId.ToString())).ReturnsAsync((AppUser?)null);

        // Act
        var result = await _userService.UpdateProfileAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserMessages.UserNotFound, result.Message);
        Assert.Null(result.Data);

        _mapperMock.Verify(mapper => mapper.Map<UpdateProfileRequest, AppUser>(It.IsAny<UpdateProfileRequest>(), It.IsAny<AppUser>()), Times.Never());

        _userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<AppUser>()), Times.Never());

        _mapperMock.Verify(mapper => mapper.Map<UserResponse>(It.IsAny<AppUser>()), Times.Never());
    }

    [Fact]
    public async Task UpdateProfileAsync_Should_ReturnFailure_When_UserManagerUpdateFails()
    {
        // Arrange
        var user = CreateUser();
        var request = CreateValidUpdateRequest();

        var updateResult = IdentityResult.Failed(
            new IdentityError
            {
                Description = "Telefon numarası geçersiz."
            },
            new IdentityError
            {
                Description = "Profil güncellenemedi."
            },
            new IdentityError
            {
                Description = "Telefon numarası geçersiz."
            });

        _userManagerMock.Setup(manager => manager.FindByIdAsync(CurrentUserId.ToString())).ReturnsAsync(user);

        _mapperMock.Setup(mapper => mapper.Map<UpdateProfileRequest, AppUser>(request, user)).Returns(user);

        _userManagerMock.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(updateResult);

        // Act
        var result = await _userService.UpdateProfileAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserMessages.ProfileUpdateFailed, result.Message);

        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);

        Assert.Contains("Telefon numarası geçersiz.", result.Errors);
        Assert.Contains("Profil güncellenemedi.", result.Errors);

        Assert.Equal(2, result.Errors.Distinct().Count());

        _mapperMock.Verify(mapper => mapper.Map<UpdateProfileRequest, AppUser>(request, user), Times.Once());

        _userManagerMock.Verify(manager => manager.UpdateAsync(user), Times.Once());

        _mapperMock.Verify(mapper => mapper.Map<UserResponse>(It.IsAny<AppUser>()), Times.Never());
    }

    [Fact]
    public async Task UpdateProfileAsync_Should_UpdateProfile_When_RequestIsValid()
    {
        // Arrange
        var user = CreateUser();
        var request = CreateValidUpdateRequest();

        var response = CreateUserResponse(user);

        _userManagerMock.Setup(manager => manager.FindByIdAsync(CurrentUserId.ToString())).ReturnsAsync(user);

        _mapperMock.Setup(mapper => mapper.Map<UpdateProfileRequest, AppUser>(request, user)).Returns(user);

        _userManagerMock.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        _mapperMock.Setup(mapper => mapper.Map<UserResponse>(user)).Returns(response);

        // Act
        var result = await _userService.UpdateProfileAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(UserMessages.ProfileUpdated, result.Message);

        Assert.NotNull(result.Data);
        Assert.Same(response, result.Data);

        _mapperMock.Verify(mapper => mapper.Map<UpdateProfileRequest, AppUser>(request, user), Times.Once());

        _userManagerMock.Verify(manager => manager.UpdateAsync(user), Times.Once());

        _mapperMock.Verify(mapper => mapper.Map<UserResponse>(user), Times.Once());
    }

    // =========================================================
    // USER MANAGER MOCK
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

    // =========================================================
    // TEST DATA HELPERS
    // =========================================================

    private static AppUser CreateUser()
    {
        return new AppUser
        {
            Id = CurrentUserId,
            FirstName = "Cengizhan",
            LastName = "Şirin",
            Email = "cengizhan@example.com",
            UserName = "cengizhan@example.com",
            PhoneNumber = "05555555555",
            EmailConfirmed = true
        };
    }

    private static UpdateProfileRequest CreateValidUpdateRequest()
    {
        return new UpdateProfileRequest(
            FirstName: "Cengizhan Updated",
            LastName: "Şirin Updated",
            PhoneNumber: "05551112233");
    }

    private static UserDetailResponse CreateUserDetailResponse(AppUser user)
    {
        return new UserDetailResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            LockoutEnd = user.LockoutEnd,
            EmailConfirmed = user.EmailConfirmed
        };
    }

    private static UserResponse CreateUserResponse(AppUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!
        };
    }
}