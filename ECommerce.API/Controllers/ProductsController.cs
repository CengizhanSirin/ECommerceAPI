using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.Product.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    public sealed class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll( [FromQuery] PaginationRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.GetAllAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id,CancellationToken cancellationToken)   
        {
            var result = await _productService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request,CancellationToken cancellationToken)       
        {
            var result = await _productService.CreateAsync(request, cancellationToken);
            return HandleCreatedResult( result,nameof(GetById),data => new { id = data.Id });      
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id,[FromBody] UpdateProductRequest request,CancellationToken cancellationToken)
        {
            var result = await _productService.UpdateAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _productService.DeleteAsync(id, cancellationToken);
            return HandleNoContent(result);
        }
    }
}
