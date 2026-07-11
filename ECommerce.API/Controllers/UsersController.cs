using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.DTOs.User.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Authorize]
    public sealed class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var result = await _userService.GetProfileAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateProfileAsync(request, cancellationToken);
            return HandleResult(result);
        }
    }
}
