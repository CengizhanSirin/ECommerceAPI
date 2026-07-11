using AutoMapper;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.User.Requests;
using ECommerce.Application.DTOs.User.Responses;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Services
{
    public sealed class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UserService(UserManager<AppUser> userManager, ICurrentUserService currentUserService, IMapper mapper)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<ResultT<UserDetailResponse>> GetProfileAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
                return ResultT<UserDetailResponse>.NotFound(UserMessages.UserNotFound);

            var response = _mapper.Map<UserDetailResponse>(user);

            return ResultT<UserDetailResponse>.Success(response, UserMessages.ProfileRetrieved);
        }

        public async Task<ResultT<UserResponse>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
                return ResultT<UserResponse>.NotFound(UserMessages.UserNotFound);

            _mapper.Map(request, user);

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(x => x.Description).Distinct().ToArray();

                return ResultT<UserResponse>.Failure(UserMessages.ProfileUpdateFailed, errors: errors);
            }

            var response = _mapper.Map<UserResponse>(user);

            return ResultT<UserResponse>.Success(response, UserMessages.ProfileUpdated);
        }
    }
}
