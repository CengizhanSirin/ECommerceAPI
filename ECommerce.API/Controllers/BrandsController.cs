using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.Brand.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    public sealed class BrandsController : BaseApiController
    {
        private readonly IBrandService _brandService;
        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
        {
            var result = await _brandService.GetAllAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _brandService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBrandRequest request, CancellationToken cancellationToken)
        {
            var result = await _brandService.CreateAsync(request, cancellationToken);
            return HandleCreatedResult(result, nameof(GetById), data => new { id = data.Id });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _brandService.DeleteAsync(id, cancellationToken);
            return HandleNoContent(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandRequest request, CancellationToken cancellationToken)
        {
            var result = await _brandService.UpdateAsync(id, request, cancellationToken);
            return HandleResult(result);
        }
    }
}
