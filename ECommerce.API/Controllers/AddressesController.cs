using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.DTOs.Address.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Authorize]
    public sealed class AddressesController : BaseApiController
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _addressService.GetAllAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id,CancellationToken cancellationToken)
        {
            var result = await _addressService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create( [FromBody] CreateAddressRequest request,CancellationToken cancellationToken)
        {
            var result = await _addressService.CreateAsync(request, cancellationToken);
            return HandleCreatedResult( result,nameof(GetById),data => new { id = data.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressRequest request,CancellationToken cancellationToken)   
        {
            var result = await _addressService.UpdateAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)     
        {
            var result = await _addressService.DeleteAsync(id, cancellationToken);
            return HandleNoContent(result);
        }

        [HttpPatch("{id:int}/set-default")]
        public async Task<IActionResult> SetDefault( int id,CancellationToken cancellationToken)
        {
            var result = await _addressService.SetDefaultAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
